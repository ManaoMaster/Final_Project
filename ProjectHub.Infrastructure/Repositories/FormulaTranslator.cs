using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Services
{
    public class FormulaTranslator : IFormulaTranslator
    {
<<<<<<< HEAD
        // (เราจะใช้ DataColumnName = "Data" ตามที่คุณบอกว่าใช้ jsonb)
=======
>>>>>>> feature/create-calculation
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

        // --- Recursive Parser (ส่วนที่ทำงานจริง) ---

        private string ParseNode(JsonElement node)
        {
            string type = node.GetProperty("type").GetString() ?? "";

            switch (type)
            {
                case "operator":
                    string op = node.GetProperty("value").GetString() ?? "+";
<<<<<<< HEAD
                    // (เรียกตัวเองซ้ำเพื่อเจาะลงไปใน "left" และ "right")
                    string left = ParseNode(node.GetProperty("left"));
                    string right = ParseNode(node.GetProperty("right"));
                    // (สร้าง SQL โดยใส่วงเล็บให้ถูกต้อง)
=======
                    string left = ParseNode(node.GetProperty("left"));
                    string right = ParseNode(node.GetProperty("right"));
>>>>>>> feature/create-calculation
                    return $"({left} {op} {right})";

                case "column":
                    string colName = node.GetProperty("name").GetString() ?? "";
<<<<<<< HEAD
                    // *** นี่คือส่วนที่แปลง "name" เป็น SQL ครับ ***
                    // (Data->>'salary')::numeric
                    return $"({_dataColumn}->>'{colName}')::numeric";

                case "literal":
                    // (ส่งค่าตัวเลข/ข้อความ กลับไปตรงๆ)
=======
                    
                    // --- *** นี่คือจุดที่แก้ไขครับ *** ---
                    // เพิ่ม \" (ฟันหนู) รอบ _dataColumn
                    // เพื่อให้ SQL ที่ได้เป็น ("Data"->>'salary')::numeric
                    return $"(\"{_dataColumn}\"->>'{colName}')::numeric";

                case "literal":
>>>>>>> feature/create-calculation
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
                    // Literal nodes ไม่มีชื่อคอลัมน์
                    break;
            }
        }
    }
}