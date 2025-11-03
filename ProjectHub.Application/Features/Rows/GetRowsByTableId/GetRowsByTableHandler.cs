using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories; // <-- *** 1. เพิ่ม Using นี้ (สำหรับ IRelationshipRepository) ***
using System.Data;
using Dapper;
using System.Linq;
using System;
using System.Text; // <-- *** 2. เพิ่ม Using นี้ (สำหรับ StringBuilder) ***

namespace ProjectHub.Application.Features.Rows.GetRowsByTableId
{
    public class GetRowsByTableIdHandler : IRequestHandler<GetRowsByTableIdQuery, IEnumerable<IDictionary<string, object>>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IFormulaTranslator _formulaTranslator;
        private readonly IDbConnection _dbConnection;
        // --- *** 3. Inject Repositories ที่จำเป็นเพิ่ม *** ---
        private readonly IRelationshipRepository _relationshipRepository;

        public GetRowsByTableIdHandler(
            IColumnRepository columnRepository,
            IFormulaTranslator formulaTranslator,
            IDbConnection dbConnection,
            // --- *** 4. เพิ่ม Parameter นี้เข้ามา *** ---
            IRelationshipRepository relationshipRepository)
        {
            _columnRepository = columnRepository;
            _formulaTranslator = formulaTranslator;
            _dbConnection = dbConnection;
            _relationshipRepository = relationshipRepository; // <-- 5. กำหนดค่า
        }

        public async Task<IEnumerable<IDictionary<string, object>>> Handle(GetRowsByTableIdQuery request, CancellationToken cancellationToken)
        {
            // === 1. ดึง Schema ทั้งหมด ===
            var columns = (await _columnRepository.GetColumnsByTableIdAsync(request.TableId)).ToList();

            if (!columns.Any())
            {
                return new List<IDictionary<string, object>>();
            }

            // === 2. เตรียมตัวแปรสำหรับสร้าง SQL ===
            var selectClauses = new List<string>();
            var joinClauses = new List<string>();

            // (สำคัญ) นี่คือ "พจนานุกรม" ที่เก็บว่าคอลัมน์ไหน (เช่น "Num1_lookup") 
            // จะถูกแทนที่ด้วย SQL อะไร (เช่น "t_lookup_1.\"Data\"->>'Num1'")
            var columnSqlAliasMap = new Dictionary<string, string>();

            // ตั้งชื่อเล่นให้ตารางหลัก (Rows)
            string mainTableAlias = "t_main";

            selectClauses.Add($"{mainTableAlias}.\"Row_id\"");
            selectClauses.Add($"{mainTableAlias}.\"Data\"");

            // === 3. (Loop ที่ 1) ประมวลผลคอลัมน์ธรรมดา (เพื่อสร้าง Alias Map) ===
            var nativeColumns = columns.Where(c =>
                c.Data_type != "Formula" && c.Data_type != "Lookup" &&
                !string.IsNullOrWhiteSpace(c.Name));

            foreach (var col in nativeColumns)
            {
                // เก็บว่า "Num1" = '("t_main"."Data"->>'Num1')::numeric'
                columnSqlAliasMap[col.Name] = $"(\"{mainTableAlias}\".\"Data\"->>'{col.Name}')::numeric";
            }

            // === 4. (Loop ที่ 2) สร้าง JOINs สำหรับคอลัมน์ Lookup (นี่คือ Logic ใหม่) ===
            var lookupColumns = columns.Where(c => c.Data_type == "Lookup").ToList();
            int lookupIndex = 1; // ใช้นับเพื่อสร้างชื่อเล่น t_lookup_1, t_lookup_2

            foreach (var col in lookupColumns)
            {
                if (col.LookupRelationshipId == null || col.LookupTargetColumnId == null)
                {
                    selectClauses.Add($"'LOOKUP_ERROR: Column {col.Name} is not configured.' AS \"{col.Name}\"");
                    continue;
                }

                // 1. ดึง "กฎ" การเชื่อมโยง (จากตาราง Relationships)
                var relationship = await _relationshipRepository.GetByIdAsync(col.LookupRelationshipId.Value);
                // 2. ดึง "คอลัมน์เป้าหมาย" ที่เราจะเอาค่า (เช่น "Num1" จาก Table 1)
                var targetColumn = await _columnRepository.GetColumnByIdAsync(col.LookupTargetColumnId.Value);
                // 3. ดึง "คอลัมน์ Foreign Key" (ในตารางนี้)
                var foreignKeyCol = await _columnRepository.GetColumnByIdAsync(relationship.ForeignColumnId);
                // 4. ดึง "คอลัมน์ Primary Key" (ในตารางเป้าหมาย)
                var primaryKeyCol = await _columnRepository.GetColumnByIdAsync(relationship.PrimaryColumnId);

                if (relationship == null || targetColumn == null || foreignKeyCol == null || primaryKeyCol == null)
                {
                    selectClauses.Add($"'LOOKUP_ERROR: Invalid configuration.' AS \"{col.Name}\"");
                    continue;
                }

                // 5. สร้างชื่อเล่น (Alias) ให้ตารางที่จะ Join
                string joinTableAlias = $"t_lookup_{lookupIndex++}";

                // 6. สร้าง SQL LEFT JOIN
                // (Join jsonb กับ jsonb โดยอิงจาก Metadata)
                string joinSql = $@"
LEFT JOIN ""Rows"" AS {joinTableAlias} 
       ON (""{mainTableAlias}"".""Data""->>'{foreignKeyCol.Name}')::int = (""{joinTableAlias}"".""Data""->>'{primaryKeyCol.Name}')::int
       AND ""{joinTableAlias}"".""Table_id"" = {relationship.PrimaryTableId}
";
                joinClauses.Add(joinSql);

                // 7. สร้าง SQL Snippet ที่จะใช้ดึงค่า
                string selectSnippet = $"(\"{joinTableAlias}\".\"Data\"->>'{targetColumn.Name}')";

                // 8. เพิ่มเข้าไปใน SELECT List
                selectClauses.Add($"{selectSnippet} AS \"{col.Name}\"");

                // 9. (สำคัญ) เก็บเข้าพจนานุกรม เพื่อให้ Formula (Loop 3) รู้จัก
                columnSqlAliasMap[col.Name] = $"{selectSnippet}::numeric"; // (สมมติว่า Lookup มาเป็นตัวเลข)
            }

            // === 5. (Loop ที่ 3) ประมวลผลคอลัมน์ Formula (อัปเกรด Logic) ===
            var formulaColumns = columns.Where(c => c.Data_type == "Formula"
                                        && !string.IsNullOrWhiteSpace(c.FormulaDefinition));

            foreach (var col in formulaColumns)
            {
                try
                {
                    // 1. แปลง JSON AST เป็น SQL (เช่น ("Data"->>'Num1')::numeric + ("Data"->>'T1_Num1')::numeric)
                    string sqlSnippet = _formulaTranslator.Translate(col.FormulaDefinition!, "Data");

                    // 2. *** (Logic ใหม่ที่แก้ไขแล้ว) แทนที่ ***
                    // วนลูปในพจนานุกรม แล้วแทนที่ SQL ที่ FormulaTranslator สร้าง
                    foreach (var entry in columnSqlAliasMap)
                    {
                        // *** (แก้ไข) Placeholder ต้องตรงกับที่ Translator สร้าง ***
                        // (เช่น ("Data"->>'Num1')::numeric)
                        string placeholder = $"(\"Data\"->>'{entry.Key}')::numeric";

                        // (เช่น ("t_main"."Data"->>'Num1')::numeric หรือ ("t_lookup_1"."Data"->>'Num1')::numeric)
                        string replacementValue = entry.Value;

                        sqlSnippet = sqlSnippet.Replace(placeholder, replacementValue);
                    }

                    selectClauses.Add($"({sqlSnippet}) AS \"{col.Name}\"");
                }
                catch (Exception ex)
                {
                    selectClauses.Add($"'FORMULA_ERROR: {ex.Message.Replace("'", "''")}' AS \"{col.Name}\"");
                }
            }
            // === 6. สร้าง SQL ฉบับเต็ม ===
            string finalSql = $@"
                SELECT {string.Join(", ", selectClauses)}
                FROM ""Rows"" AS {mainTableAlias}
                {string.Join("\n", joinClauses)}
                WHERE ""{mainTableAlias}"".""Table_id"" = @TableId
            ";

            // === 7. รัน Raw SQL ด้วย Dapper ===
            var results = await _dbConnection.QueryAsync<dynamic>(finalSql, new { request.TableId });

            return results.Cast<IDictionary<string, object>>();
        }
    }
}