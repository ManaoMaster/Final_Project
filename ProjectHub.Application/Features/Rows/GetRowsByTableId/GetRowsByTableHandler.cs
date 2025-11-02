using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces;
using System.Data;
using Dapper;
using System.Linq;
using System;

namespace ProjectHub.Application.Features.Rows.GetRowsByTableId
{
    // *** 1. แก้ไข Signature ให้ตรงกับ Query ***
    public class GetRowsByTableIdHandler : IRequestHandler<GetRowsByTableIdQuery, IEnumerable<IDictionary<string, object>>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IFormulaTranslator _formulaTranslator;
        private readonly IDbConnection _dbConnection; 

        public GetRowsByTableIdHandler(
            IColumnRepository columnRepository, 
            IFormulaTranslator formulaTranslator, 
            IDbConnection dbConnection)
        {
            _columnRepository = columnRepository;
            _formulaTranslator = formulaTranslator;
            _dbConnection = dbConnection;
        }

        // *** 2. แก้ไข Return Type ของ Task ให้ตรงกัน ***
        public async Task<IEnumerable<IDictionary<string, object>>> Handle(GetRowsByTableIdQuery request, CancellationToken cancellationToken)
        {
            var columns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);
            
            if (!columns.Any())
            {
                // *** 3. แก้ไข Return List ว่าง ให้ตรง Type ***
                return new List<IDictionary<string, object>>();
            }

            // ... (โค้ดสร้าง SQL ทั้งหมดของคุณถูกต้องแล้ว) ...
            var selectClauses = new List<string> { "\"Row_id\"", "\"Data\"" };
            var formulaColumns = columns.Where(c => c.Data_type == "Formula" && !string.IsNullOrWhiteSpace(c.FormulaDefinition));

            foreach (var col in formulaColumns)
            {
                // ... (try...catch... เหมือนเดิม) ...
                try
                {
                    string sqlSnippet = _formulaTranslator.Translate(col.FormulaDefinition, "Data");
                    selectClauses.Add($"({sqlSnippet}) AS \"{col.Name}\"");
                }
                catch (Exception ex)
                {
                    selectClauses.Add($"'FORMULA_ERROR: {ex.Message.Replace("'", "''")}' AS \"{col.Name}\"");
                }
            }
            
            string finalSql = $@"
                SELECT {string.Join(", ", selectClauses)}
                FROM ""Rows""
                WHERE ""Table_id"" = @TableId
            ";

            // 8. รัน Raw SQL ด้วย Dapper
            // *** 4. (จุดสำคัญ) Dapper ใช้ <dynamic> เหมือนเดิม! ***
            // (นี่คือจุดที่ทำให้ไม่ Error)
            var results = await _dbConnection.QueryAsync<dynamic>(finalSql, new { request.TableId }); 

            return results.Cast<IDictionary<string, object>>();
        }
    }
}