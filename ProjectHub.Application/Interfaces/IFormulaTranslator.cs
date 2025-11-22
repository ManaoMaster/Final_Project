using System.Collections.Generic;

namespace ProjectHub.Application.Interfaces
{
    public interface IFormulaTranslator
    {
        
        
        
        
        
        
        string Translate(string jsonAst, string dataColumnName = "Data");

        
        
        
        
        
        List<string> GetReferencedColumnNames(string jsonAst);
    }
}