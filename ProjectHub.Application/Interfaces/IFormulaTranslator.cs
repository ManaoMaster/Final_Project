using System.Collections.Generic;

namespace ProjectHub.Application.Interfaces
{
    public interface IFormulaTranslator
    {
        /// <summary>
        /// แปลง JSON AST (FormulaDefinition) ให้เป็น SQL Snippet
        /// </summary>
        /// <param name="jsonAst">JSON สูตรจาก Columns.FormulaDefinition</param>
        /// <param name="dataColumnName">ชื่อคอลัมน์ jsonb ในตาราง Rows (เช่น "Data")</param>
        /// <returns>SQL string ที่ PostgreSQL เข้าใจ</returns>
        string Translate(string jsonAst, string dataColumnName = "Data");

        /// <summary>
        /// ดึงรายชื่อคอลัมน์ทั้งหมดที่สูตรนี้อ้างอิงถึง
        /// </summary>
        /// <param name="jsonAst">JSON สูตรจาก Columns.FormulaDefinition</param>
        /// <returns>List ของชื่อคอลัมน์ (เช่น ["salary", "bonus"])</returns>
        List<string> GetReferencedColumnNames(string jsonAst);
    }
}