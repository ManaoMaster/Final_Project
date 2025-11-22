using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using System.Data;
using Dapper; 
using System.Linq;
using System;
using System.Text;
using ProjectHub.Application.Common; 
using ColumnEntity = ProjectHub.Domain.Entities.Columns; 
using System.Diagnostics;
using Microsoft.Extensions.Logging; 

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

        private static bool IsLookup(ColumnEntity c) =>
    string.Equals(c.Data_type, "LOOKUP", StringComparison.OrdinalIgnoreCase);

        private static bool IsFormula(ColumnEntity c) =>
            string.Equals(c.Data_type, "FORMULA", StringComparison.OrdinalIgnoreCase);


        public async Task<IEnumerable<IDictionary<string, object>>> Handle(GetRowsByTableIdQuery request, CancellationToken cancellationToken)
        {
            
            await _securityService.ValidateTableAccessAsync(request.TableId);

            
            
            var dapperParams = new DynamicParameters();
            dapperParams.Add("TableId", request.TableId); 

            
            var columns = (await _columnRepository.GetColumnsByTableIdAsync(request.TableId)).ToList();
            if (!columns.Any())
            {
                return new List<IDictionary<string, object>>();
            }


            var lookupColumns = columns
     .Where(c => IsLookup(c) && c.LookupRelationshipId != null)
     .ToList();

            
            var requiredRelIds = lookupColumns.Select(c => c.LookupRelationshipId.Value).Distinct().ToList();
            
            var allRelationships = (await _relationshipRepository.GetByIdsAsync(requiredRelIds))
                                     .ToDictionary(r => r.RelationshipId); 

            
            
            
            
            
            
            
            
            
            var requiredColIds = new HashSet<int>();

            
            foreach (var col in lookupColumns)
            {
                if (col.LookupTargetColumnId.HasValue)
                {
                    requiredColIds.Add(col.LookupTargetColumnId.Value);
                }
            }

            
            foreach (var rel in allRelationships.Values)
            {
                requiredColIds.Add(rel.PrimaryColumnId);

                if (rel.ForeignColumnId.HasValue)          
                {
                    requiredColIds.Add(rel.ForeignColumnId.Value);
                }
            }

            
            var allColumnsMetadata = (await _columnRepository.GetByIdsAsync(requiredColIds.ToList()))
                                       .ToDictionary(c => c.Column_id); 


            
            var selectClauses = new List<string>();
            var joinClauses = new List<string>();
            var columnSqlAliasMap = new Dictionary<string, string>(); 
            string mainTableAlias = SanitizeIdentifier("t_main");

            selectClauses.Add($"{mainTableAlias}.\"Row_id\"");
            selectClauses.Add($"{mainTableAlias}.\"Data\"");

            
            var nativeColumns = columns
     .Where(c => !IsLookup(c) && !IsFormula(c))
     .ToList();

            foreach (var col in nativeColumns)
            {
                string safeColName = SanitizeLiteral(col.Name); 
                string colCast = ColumnTypeHelper.GetSqlCast(col.Data_type); 
                string sqlSnippet = $"({mainTableAlias}.\"Data\"->>'{safeColName}')";

                selectClauses.Add($"{sqlSnippet} AS {SanitizeIdentifier(col.Name)}"); 
                columnSqlAliasMap[col.Name] = $"{sqlSnippet}{colCast}";
            }

            
            int lookupIndex = 1;
            foreach (var col in lookupColumns)
            {
                
                
                
                
                
                
                
                
                
                if (!allRelationships.TryGetValue(col.LookupRelationshipId!.Value, out var relationship) ||
    !allColumnsMetadata.TryGetValue(col.LookupTargetColumnId!.Value, out var targetColumn))
                {
                    selectClauses.Add($@"'LOOKUP_ERROR: Config Error' AS {SanitizeIdentifier(col.Name)}");
                    continue;
                }

                
                var foreignColumnId = relationship.ForeignColumnId
                    ?? throw new InvalidOperationException(
                        $"Relationship {relationship.RelationshipId} has no ForeignColumnId.");

                if (!allColumnsMetadata.TryGetValue(foreignColumnId, out var foreignKeyCol) ||
                    !allColumnsMetadata.TryGetValue(relationship.PrimaryColumnId, out var primaryKeyCol))
                {
                    selectClauses.Add($@"'LOOKUP_ERROR: Config Error' AS {SanitizeIdentifier(col.Name)}");
                    continue;
                }


                string joinTableAlias = SanitizeIdentifier($"t_lookup_{lookupIndex}");
                string paramName = $"p_{lookupIndex}"; 
                lookupIndex++;

                
                string safeFkName = SanitizeLiteral(foreignKeyCol.Name);
                string safePkName = SanitizeLiteral(primaryKeyCol.Name);

                
                string joinSql = $@"
LEFT JOIN ""Rows"" AS {joinTableAlias} 
    ON ({mainTableAlias}.""Data""->>'{safeFkName}')::int = ({joinTableAlias}.""Data""->>'{safePkName}')::int
    AND {joinTableAlias}.""Table_id"" = @{paramName}
";
                joinClauses.Add(joinSql);
                dapperParams.Add(paramName, relationship.PrimaryTableId); 

                
                string safeTargetName = SanitizeLiteral(targetColumn.Name);
                string targetCast = ColumnTypeHelper.GetSqlCast(targetColumn.Data_type);
                string selectSnippet = $"({joinTableAlias}.\"Data\"->>'{safeTargetName}')";

                selectClauses.Add($"{selectSnippet} AS {SanitizeIdentifier(col.Name)}");
                columnSqlAliasMap[col.Name] = $"{selectSnippet}{targetCast}";
            }

            
            var formulaColumns = columns
    .Where(c => IsFormula(c) && !string.IsNullOrWhiteSpace(c.FormulaDefinition))
    .ToList();
            foreach (var col in formulaColumns)
            {
                try
                {
                    
                    string sqlSnippet = _formulaTranslator.Translate(col.FormulaDefinition!, "Data");
                    _logger.LogInformation("\n--- [Formula: {FormulaName}] ---", col.Name);
                    _logger.LogInformation("[LOG 1] Snippet จาก Translator:\n{Snippet}", sqlSnippet);
                    foreach (var entry in columnSqlAliasMap)
                    {
                        
                        var placeholderCol = columns.FirstOrDefault(c => c.Name == entry.Key);
                        string placeholderCast = ColumnTypeHelper.GetSqlCast(placeholderCol?.Data_type);

                        
                        
                        
                        string placeholder = $"\"Data\"->>'{SanitizeLiteral(entry.Key)}'"; 

                        
                        
                        string replacementValue = entry.Value;
                        
                        _logger.LogInformation("[LOG 2] กำลังค้นหา: {Placeholder}", placeholder);
                        
                        _logger.LogInformation("[LOG 3] จะแทนที่ด้วย: {Replacement}", replacementValue);
                        _logger.LogInformation("[LOG 3.5] เจอหรือไม่: {Found}", sqlSnippet.Contains(placeholder));
                        

                        sqlSnippet = sqlSnippet.Replace(placeholder, replacementValue);
                    }
                    _logger.LogInformation("[LOG 4] Snippet หลัง Replace:\n{SnippetAfter}", sqlSnippet);
                    
                    

                    
                    
                    
                    string finalCast = ColumnTypeHelper.GetSqlCast(col.Data_type); 

                    selectClauses.Add($"({sqlSnippet}){finalCast} AS {SanitizeIdentifier(col.Name)}");
                }
                catch (Exception ex)
                {
                    selectClauses.Add($@"'FORMULA_ERROR: {SanitizeLiteral(ex.Message)}' AS {SanitizeIdentifier(col.Name)}");
                }
            }

            
            string finalSql = $@"
                SELECT {string.Join(", \n\t", selectClauses)}
                FROM ""Rows"" AS {mainTableAlias}
                {string.Join("\n", joinClauses)}
                WHERE {mainTableAlias}.""Table_id"" = @TableId
            ";

            
            var results = await _dbConnection.QueryAsync<dynamic>(finalSql, dapperParams);

            return results.Cast<IDictionary<string, object>>();
        }

        

        
        
        
        
        private string SanitizeIdentifier(string identifier)
        {
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }

        
        
        
        
        private string SanitizeLiteral(string literal)
        {
            return literal.Replace("'", "''");
        }
    }
}