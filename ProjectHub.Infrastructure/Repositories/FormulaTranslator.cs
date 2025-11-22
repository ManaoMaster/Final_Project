using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Services
{
    public class FormulaTranslator : IFormulaTranslator
    {
        private string _dataColumn = "Data";

        public string Translate(string jsonAst, string dataColumnName = "Data")
        {
            _dataColumn = dataColumnName;
            if (string.IsNullOrWhiteSpace(jsonAst))
            {
                throw new ArgumentNullException(nameof(jsonAst));
            }

            using (JsonDocument doc = JsonDocument.Parse(jsonAst))
            {
                return ParseNode(doc.RootElement);
            }
        }

        public List<string> GetReferencedColumnNames(string jsonAst)
        {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(jsonAst))
            {
                return names;
            }

            using (JsonDocument doc = JsonDocument.Parse(jsonAst))
            {
                ExtractColumnNames(doc.RootElement, names);
                return names.Distinct().ToList();
            }
        }

        

        private string ParseNode(JsonElement node)
        {
            string type = node.GetProperty("type").GetString() ?? "";

            switch (type)
            {
                case "operator":
                    string op = node.GetProperty("value").GetString() ?? "+";
                    string left = ParseNode(node.GetProperty("left"));
                    string right = ParseNode(node.GetProperty("right"));
                    return $"({left} {op} {right})";

                case "column":
                    string colName = node.GetProperty("name").GetString() ?? "";

                    
                    
                    return $"\"{_dataColumn}\"->>'{colName}'";

                case "literal":
                    
                    
                    return node.GetProperty("value").GetRawText();
                default:
                    throw new NotSupportedException($"Unsupported AST node type: {type}");
            }
        }

        private void ExtractColumnNames(JsonElement node, List<string> names)
        {
            string type = node.GetProperty("type").GetString() ?? "";

            switch (type)
            {
                case "operator":
                    ExtractColumnNames(node.GetProperty("left"), names);
                    ExtractColumnNames(node.GetProperty("right"), names);
                    break;

                case "column":
                    names.Add(node.GetProperty("name").GetString() ?? "");
                    break;

                case "literal":
                    
                    break;
            }
        }
    }
}