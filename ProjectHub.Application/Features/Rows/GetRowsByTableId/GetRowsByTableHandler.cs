using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using System.Data;
using Dapper; // <-- 1. [ADD] ต้องใช้ Dapper ตัวเต็ม
using System.Linq;
using System;
using System.Text;
using ProjectHub.Application.Common; // (สำหรับ ColumnTypeHelper)
using ColumnEntity = ProjectHub.Domain.Entities.Columns; // (Alias)
using System.Diagnostics;
using Microsoft.Extensions.Logging; // <--- เพิ่มบรรทัดนี้

namespace ProjectHub.Application.Features.Rows.GetRowsByTableId
{
    public class GetRowsByTableIdHandler : IRequestHandler<GetRowsByTableIdQuery, IEnumerable<IDictionary<string, object>>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IFormulaTranslator _formulaTranslator;
        private readonly IDbConnection _dbConnection;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IProjectSecurityService _securityService;
        private readonly ILogger<GetRowsByTableIdHandler> _logger;

        public GetRowsByTableIdHandler(
            IColumnRepository columnRepository,
            IFormulaTranslator formulaTranslator,
            IDbConnection dbConnection,
            IRelationshipRepository relationshipRepository,
            IProjectSecurityService securityService,
            ILogger<GetRowsByTableIdHandler> logger)
        {
            _columnRepository = columnRepository;
            _formulaTranslator = formulaTranslator;
            _dbConnection = dbConnection;
            _relationshipRepository = relationshipRepository;
            _securityService = securityService;
            _logger = logger;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> Handle(GetRowsByTableIdQuery request, CancellationToken cancellationToken)
        {
            // === 1. [Security] ตรวจสอบสิทธิ์ (Task 1) ===
            await _securityService.ValidateTableAccessAsync(request.TableId);

            // === 2. [Prep] เตรียม Dapper Parameters (SQL Injection Fix) ===
            // เราจะใช้ DynamicParameters เพื่อเพิ่ม Parameter เข้าไปทีหลัง
            var dapperParams = new DynamicParameters();
            dapperParams.Add("TableId", request.TableId); // @TableId

            // === 3. [Performance] ดึง Metadata ทั้งหมดนอก Loop (N+1 Fix) ===
            var columns = (await _columnRepository.GetColumnsByTableIdAsync(request.TableId)).ToList();
            if (!columns.Any())
            {
                return new List<IDictionary<string, object>>();
            }

            var lookupColumns = columns.Where(c => c.Data_type == "Lookup" && c.LookupRelationshipId != null).ToList();

            // 3.1 ดึง Relationship ที่จำเป็นทั้งหมด (1 Query)
            var requiredRelIds = lookupColumns.Select(c => c.LookupRelationshipId.Value).Distinct().ToList();
            // (คุณต้องไปสร้าง Method นี้ใน Repository)
            var allRelationships = (await _relationshipRepository.GetByIdsAsync(requiredRelIds))
                                     .ToDictionary(r => r.RelationshipId); // แปลงเป็น Dictionary เพื่อให้ค้นหาเร็ว

            // 3.2 ดึง Column (Metadata) ที่จำเป็นทั้งหมด (1 Query)
            var requiredColIds = new HashSet<int>();
            foreach (var col in lookupColumns)
                requiredColIds.Add(col.LookupTargetColumnId.Value);
            foreach (var rel in allRelationships.Values)
            {
                requiredColIds.Add(rel.PrimaryColumnId);
                requiredColIds.Add(rel.ForeignColumnId);
            }
            // (คุณต้องไปสร้าง Method นี้ใน Repository)
            var allColumnsMetadata = (await _columnRepository.GetByIdsAsync(requiredColIds.ToList()))
                                       .ToDictionary(c => c.Column_id); // แปลงเป็น Dictionary


            // === 4. [Build SQL] สร้าง Query แบบไดนามิก ===
            var selectClauses = new List<string>();
            var joinClauses = new List<string>();
            var columnSqlAliasMap = new Dictionary<string, string>(); // Map["Num1"] = "(t_main."Data"->>'Num1')::numeric"
            string mainTableAlias = SanitizeIdentifier("t_main");

            selectClauses.Add($"{mainTableAlias}.\"Row_id\"");
            selectClauses.Add($"{mainTableAlias}.\"Data\"");

            // --- Loop 1: คอลัมน์ธรรมดา (Native) ---
            var nativeColumns = columns.Where(c => c.Data_type != "Formula" && c.Data_type != "Lookup");
            foreach (var col in nativeColumns)
            {
                string safeColName = SanitizeLiteral(col.Name); // (SQL Injection Fix)
                string colCast = ColumnTypeHelper.GetSqlCast(col.Data_type); // (Bug Fix)
                string sqlSnippet = $"({mainTableAlias}.\"Data\"->>'{safeColName}')";

                selectClauses.Add($"{sqlSnippet} AS {SanitizeIdentifier(col.Name)}"); // (Bug Fix)
                columnSqlAliasMap[col.Name] = $"{sqlSnippet}{colCast}";
            }

            // --- Loop 2: คอลัมน์ Lookup (อ่านจาก Dictionary, ไม่มี await) ---
            int lookupIndex = 1;
            foreach (var col in lookupColumns)
            {
                // ดึงข้อมูล Metadata จาก Dictionary (เร็วมาก)
                if (!allRelationships.TryGetValue(col.LookupRelationshipId.Value, out var relationship) ||
                    !allColumnsMetadata.TryGetValue(col.LookupTargetColumnId.Value, out var targetColumn) ||
                    !allColumnsMetadata.TryGetValue(relationship.ForeignColumnId, out var foreignKeyCol) ||
                    !allColumnsMetadata.TryGetValue(relationship.PrimaryColumnId, out var primaryKeyCol))
                {
                    selectClauses.Add($@"'LOOKUP_ERROR: Config Error' AS {SanitizeIdentifier(col.Name)}");
                    continue;
                }

                string joinTableAlias = SanitizeIdentifier($"t_lookup_{lookupIndex}");
                string paramName = $"p_{lookupIndex}"; // e.g. @p_1, @p_2
                lookupIndex++;

                // (SQL Injection Fix 1: ฆ่าเชื้อชื่อคอลัมน์)
                string safeFkName = SanitizeLiteral(foreignKeyCol.Name);
                string safePkName = SanitizeLiteral(primaryKeyCol.Name);

                // (SQL Injection Fix 2: ใช้ Parameter @paramName แทนการต่อ String)
                string joinSql = $@"
LEFT JOIN ""Rows"" AS {joinTableAlias} 
    ON ({mainTableAlias}.""Data""->>'{safeFkName}')::int = ({joinTableAlias}.""Data""->>'{safePkName}')::int
    AND {joinTableAlias}.""Table_id"" = @{paramName}
";
                joinClauses.Add(joinSql);
                dapperParams.Add(paramName, relationship.PrimaryTableId); // เพิ่ม Parameter ให้ Dapper

                // สร้าง SELECT
                string safeTargetName = SanitizeLiteral(targetColumn.Name);
                string targetCast = ColumnTypeHelper.GetSqlCast(targetColumn.Data_type);
                string selectSnippet = $"({joinTableAlias}.\"Data\"->>'{safeTargetName}')";

                selectClauses.Add($"{selectSnippet} AS {SanitizeIdentifier(col.Name)}");
                columnSqlAliasMap[col.Name] = $"{selectSnippet}{targetCast}";
            }

            // --- Loop 3: คอลัมน์ Formula ---
            var formulaColumns = columns.Where(c =>
                c.Data_type.Equals("Formula", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(c.FormulaDefinition));
            foreach (var col in formulaColumns)
            {
                try
                {
                    // 1. แปลงสูตร (จะได้: ( (("Data"->>'price')) + 10 ) )
                    string sqlSnippet = _formulaTranslator.Translate(col.FormulaDefinition!, "Data");
                    _logger.LogInformation("\n--- [Formula: {FormulaName}] ---", col.Name);
                    _logger.LogInformation("[LOG 1] Snippet จาก Translator:\n{Snippet}", sqlSnippet);
                    foreach (var entry in columnSqlAliasMap)
                    {
                        // [โค้ดเดิม ไม่ต้องแก้]
                        var placeholderCol = columns.FirstOrDefault(c => c.Name == entry.Key);
                        string placeholderCast = ColumnTypeHelper.GetSqlCast(placeholderCol?.Data_type);

                        // --- *** [FIX 3] *** ---
                        // "สตริงสำหรับค้นหา" (placeholder) ต้อง *ไม่มี* Cast
                        // เพื่อให้ตรงกับที่ Translator ตัวใหม่สร้าง
                        string placeholder = $"\"Data\"->>'{SanitizeLiteral(entry.Key)}'"; // <--- เอา placeholderCast ออก

                        // "สตริงสำหรับแทนที่" (replacementValue) ยังถูกต้องเหมือนเดิม
                        // (เพราะมันมี t_main และ ::bigint อยู่แล้วจาก Loop 1)
                        string replacementValue = entry.Value;
                        // --- *** [LOG 2] *** ---
                        _logger.LogInformation("[LOG 2] กำลังค้นหา: {Placeholder}", placeholder);
                        // --- *** [LOG 3] *** ---
                        _logger.LogInformation("[LOG 3] จะแทนที่ด้วย: {Replacement}", replacementValue);
                        _logger.LogInformation("[LOG 3.5] เจอหรือไม่: {Found}", sqlSnippet.Contains(placeholder));
                        // --------------------------

                        sqlSnippet = sqlSnippet.Replace(placeholder, replacementValue);
                    }
                    _logger.LogInformation("[LOG 4] Snippet หลัง Replace:\n{SnippetAfter}", sqlSnippet);
                    // ตอนนี้ sqlSnippet จะเป็น: ( ((t_main."Data"->>'price')::bigint) + 10 )
                    // ซึ่ง 10 ยังไม่มี Type แต่ PostgreSQL จะจัดการให้ (bigint + literal)

                    // --- *** [FIX 4] *** ---
                    // Cast ผลลัพธ์สุดท้ายทั้งก้อน ด้วย Type ของคอลัมน์ Formula
                    // (จากรูป Data_type ของ "Price_Plus_..." คือ "FORMULA")
                    string finalCast = ColumnTypeHelper.GetSqlCast(col.Data_type); // (จะได้ ::text จาก default)

                    selectClauses.Add($"({sqlSnippet}){finalCast} AS {SanitizeIdentifier(col.Name)}");
                }
                catch (Exception ex)
                {
                    selectClauses.Add($@"'FORMULA_ERROR: {SanitizeLiteral(ex.Message)}' AS {SanitizeIdentifier(col.Name)}");
                }
            }

            // === 5. [Build SQL] สร้าง SQL ฉบับเต็ม ===
            string finalSql = $@"
                SELECT {string.Join(", \n\t", selectClauses)}
                FROM ""Rows"" AS {mainTableAlias}
                {string.Join("\n", joinClauses)}
                WHERE {mainTableAlias}.""Table_id"" = @TableId
            ";

            // === 6. [Execute] รัน Dapper ด้วย Parameters ที่เราสร้างไว้ ===
            var results = await _dbConnection.QueryAsync<dynamic>(finalSql, dapperParams);

            return results.Cast<IDictionary<string, object>>();
        }

        // --- [SQL INJECTION HELPERS] ---

        /// <summary>
        /// Sanitize (ฆ่าเชื้อ) ชื่อ Identifier (เช่น ชื่อตาราง, ชื่อคอลัมน์)
        /// โดยการครอบด้วย "..." และ Escape " ภายใน
        /// </summary>
        private string SanitizeIdentifier(string identifier)
        {
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }

        /// <summary>
        /// Sanitize (ฆ่าเชื้อ) ค่า String Literal (สำหรับใช้ใน ->> '...')
        /// โดยการ Escape ' (single-quote) ภายใน
        /// </summary>
        private string SanitizeLiteral(string literal)
        {
            return literal.Replace("'", "''");
        }
    }
}