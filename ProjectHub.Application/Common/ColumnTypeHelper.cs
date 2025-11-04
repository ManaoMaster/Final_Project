using System.Collections.Generic;
using System.Text.Json;

namespace ProjectHub.Application.Common
{
    public static class ColumnTypeHelper
    {
        // === 1. ศูนย์กลางสำหรับ SQL Cast ===
        public static string GetSqlCast(string dataType)
        {
            return dataType?.ToUpperInvariant() switch
            {
                "INTEGER" => "::bigint",
                "INT" => "::bigint",
                "REAL" => "::numeric",
                "NUMBER" => "::numeric", // (เพิ่ม Number ถ้าคุณใช้)
                "BOOLEAN" => "::boolean",
                "TEXT" => "::text",
                "IMAGE" => "::text",
                _ => "::text" // Default ที่ปลอดภัยที่สุด
            };
        }

        // === 2. ศูนย์กลางสำหรับ Validate JSON ===
        public static bool IsValidJsonValue(string dataType, JsonElement valueElement)
        {
            var valueKind = valueElement.ValueKind;
            return dataType?.ToUpperInvariant() switch
            {
                "TEXT" => valueKind == JsonValueKind.String,
                "IMAGE" => valueKind == JsonValueKind.String,
                "INTEGER" => valueKind == JsonValueKind.Number && valueElement.TryGetInt64(out _),
                "INT" => valueKind == JsonValueKind.Number && valueElement.TryGetInt64(out _),
                "REAL" => valueKind == JsonValueKind.Number,
                "NUMBER" => valueKind == JsonValueKind.Number,
                "BOOLEAN" => valueKind == JsonValueKind.True || valueKind == JsonValueKind.False,
                _ => false
            };
        }

        // === 3. ศูนย์กลางสำหรับรายชื่อ Type ทั้งหมด ===
        public static IReadOnlyList<string> GetKnownDataTypes()
        {
            return new[] 
            { 
                "TEXT", "IMAGE", "INTEGER", "INT", "REAL", "NUMBER", "BOOLEAN" 
            };
        }
    }
}