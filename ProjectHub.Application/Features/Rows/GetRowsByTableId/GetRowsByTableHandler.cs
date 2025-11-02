using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces; // (สำหรับ IFormulaTranslator, IColumnRepository)
using System.Data; // (สำหรับ IDbConnection)
using Dapper; // (สำหรับ .QueryAsync)
using System.Linq; // (สำหรับ .Where, .Any)
using System; // (สำหรับ Exception)

namespace ProjectHub.Application.Features.Rows.GetRowsByTableId
{
    public class GetRowsByTableIdHandler : IRequestHandler<GetRowsByTableIdQuery, IEnumerable<IDictionary<string, object>>>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly IFormulaTranslator _formulaTranslator;
        private readonly IDbConnection _dbConnection; // <-- ใช้ Dapper

        public GetRowsByTableIdHandler(
            IColumnRepository columnRepository,
            IFormulaTranslator formulaTranslator,
            IDbConnection dbConnection)
        {
            _columnRepository = columnRepository;
            _formulaTranslator = formulaTranslator;
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> Handle(GetRowsByTableIdQuery request, CancellationToken cancellationToken)
        {
            var columns = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);

            if (!columns.Any())
            {
                // *** 3. แก้ไข: Return List ของ type ใหม่ ***
                return new List<IDictionary<string, object>>();
            }

            // 2. เตรียม SQL SELECT List
            var selectClauses = new List<string> { "\"Row_id\"", "\"Data\"" };
            var formulaColumns = columns.Where(c => c.Data_type == "Formula" && !string.IsNullOrWhiteSpace(c.FormulaDefinition));

            foreach (var col in formulaColumns)
            {
                try
                {
                    string sqlSnippet = _formulaTranslator.Translate(col.FormulaDefinition!, "Data");
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
            // *** 4. แก้ไข: เปลี่ยน <dynamic> เป็น <IDictionary<string, object>> ***
            var results = await _dbConnection.QueryAsync<IDictionary<string, object>>(finalSql, new { request.TableId });

            return results;
        }
    }
}