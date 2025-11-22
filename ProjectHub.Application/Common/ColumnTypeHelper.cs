









































































using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace ProjectHub.Application.Common
{
    public static class ColumnTypeHelper
    {
        
        public static string GetSqlCast(string dataType)
        {
            return dataType?.ToUpperInvariant() switch
            {
                "INTEGER" => "::bigint",
                "INT" => "::bigint",
                "LOOKUP" => "::bigint",   

                "REAL" => "::numeric",
                "NUMBER" => "::numeric",
                "BOOLEAN" => "::boolean",
                "FORMULA" => "::numeric",
                "DATE" => "::date",
                "TEXT" => "::text",
                "IMAGE" => "::text",

                _ => "::text" 
            };
        }

        
        public static bool IsValidJsonValue(string dataType, JsonElement valueElement)
        {
            var valueKind = valueElement.ValueKind;

            return dataType?.ToUpperInvariant() switch
            {
                "TEXT" =>
                    valueKind == JsonValueKind.String ||
                    valueKind == JsonValueKind.Number,

                "IMAGE" =>
                    valueKind == JsonValueKind.String,

                "INTEGER" =>
                    valueKind == JsonValueKind.Number &&
                    valueElement.TryGetInt64(out _),

                "INT" =>
                    valueKind == JsonValueKind.Number &&
                    valueElement.TryGetInt64(out _),

                
                "LOOKUP" =>
                    valueKind == JsonValueKind.Number &&
                    valueElement.TryGetInt64(out _),

                "REAL" =>
                    valueKind == JsonValueKind.Number,

                "NUMBER" =>
                    valueKind == JsonValueKind.Number,

                "FORMULA" =>
                    
                    
                    valueKind == JsonValueKind.Number,

                "BOOLEAN" =>
                    valueKind == JsonValueKind.True ||
                    valueKind == JsonValueKind.False,

                "DATE" =>
                    valueKind == JsonValueKind.String &&
                    IsValidDate(valueElement.GetString()),

                _ => false
            };
        }

        private static bool IsValidDate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            
            return DateTime.TryParseExact(
                text,
                new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }

        
        public static IReadOnlyList<string> GetKnownDataTypes()
        {
            return new[]
            {
                "TEXT",
                "IMAGE",
                "INTEGER",
                "INT",
                "LOOKUP",  
                "REAL",
                "NUMBER",
                "BOOLEAN",
                "FORMULA",
                "DATE"
            };
        }
    }
}
