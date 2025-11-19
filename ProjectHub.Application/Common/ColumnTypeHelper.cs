//using System.Collections.Generic;
//using System.Globalization;
//using System.Text.Json;

//namespace ProjectHub.Application.Common
//{
//    public static class ColumnTypeHelper
//    {
//        // === 1. ศูนย์กลางสำหรับ SQL Cast ===
//        public static string GetSqlCast(string dataType)
//        {
//            return dataType?.ToUpperInvariant() switch
//            {
//                "INTEGER" => "::bigint",
//                "INT" => "::bigint",
//                "REAL" => "::numeric",
//                "NUMBER" => "::numeric", // (เพิ่ม Number ถ้าคุณใช้)
//                "BOOLEAN" => "::boolean",
//                "FORMULA" => "::numeric",
//                "DATE" => "::date",
//                "TEXT" => "::text",
//                "IMAGE" => "::text",
//                _ => "::text" // Default ที่ปลอดภัยที่สุด
//            };
//        }

//        // === 2. ศูนย์กลางสำหรับ Validate JSON ===
//        public static bool IsValidJsonValue(string dataType, JsonElement valueElement)
//        {
//            var valueKind = valueElement.ValueKind;
//            return dataType?.ToUpperInvariant() switch
//            {
//                "TEXT" => valueKind == JsonValueKind.String || valueKind == JsonValueKind.Number,
//                "IMAGE" => valueKind == JsonValueKind.String,
//                "INTEGER" => valueKind == JsonValueKind.Number && valueElement.TryGetInt64(out _),
//                "INT" => valueKind == JsonValueKind.Number && valueElement.TryGetInt64(out _),
//                "REAL" => valueKind == JsonValueKind.Number,
//                "NUMBER" => valueKind == JsonValueKind.Number,
//                "FORMULA" => valueKind == JsonValueKind.Number,
//                "BOOLEAN" => valueKind == JsonValueKind.True || valueKind == JsonValueKind.False,
//                "DATE" => valueKind == JsonValueKind.String
//                           && IsValidDate(valueElement.GetString()),
//                _ => false

//            };
//        }

//        private static bool IsValidDate(string? text)
//        {
//            if (string.IsNullOrWhiteSpace(text)) return false;

//            // แนะนำใช้รูปแบบ ISO จาก frontend เช่น "2025-11-15"
//            return DateTime.TryParseExact(
//                text,
//                new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ" },
//                CultureInfo.InvariantCulture,
//                DateTimeStyles.None,
//                out _
//            );
//        }


//        // === 3. ศูนย์กลางสำหรับรายชื่อ Type ทั้งหมด ===
//        public static IReadOnlyList<string> GetKnownDataTypes()
//        {
//            return new[]
//            {
//                "TEXT", "IMAGE", "INTEGER", "INT", "REAL", "NUMBER", "BOOLEAN", "FORMULA","DATE"
//            };
//        }
//    }
//}


using System.Collections.Generic;
using System.Globalization;
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
                "LOOKUP" => "::bigint",   // ✅ FK ของ lookup ใช้เป็นเลข id

                "REAL" => "::numeric",
                "NUMBER" => "::numeric",
                "BOOLEAN" => "::boolean",
                "FORMULA" => "::numeric",
                "DATE" => "::date",
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

                // ✅ LOOKUP = เก็บ id (เลข) → validate เหมือน INTEGER
                "LOOKUP" =>
                    valueKind == JsonValueKind.Number &&
                    valueElement.TryGetInt64(out _),

                "REAL" =>
                    valueKind == JsonValueKind.Number,

                "NUMBER" =>
                    valueKind == JsonValueKind.Number,

                "FORMULA" =>
                    // ปกติ JsonDataValidator จะไม่ให้ส่งค่า FORMULA อยู่แล้ว
                    // ถ้าเผื่อมีค่าเข้ามา ให้ถือว่าเป็นตัวเลขเท่านั้น
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

            // แนะนำใช้รูปแบบ ISO จาก frontend เช่น "2025-11-15"
            return DateTime.TryParseExact(
                text,
                new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }

        // === 3. รายชื่อ Type ทั้งหมด ===
        public static IReadOnlyList<string> GetKnownDataTypes()
        {
            return new[]
            {
                "TEXT",
                "IMAGE",
                "INTEGER",
                "INT",
                "LOOKUP",  // ✅ เพิ่มให้รู้จัก
                "REAL",
                "NUMBER",
                "BOOLEAN",
                "FORMULA",
                "DATE"
            };
        }
    }
}
