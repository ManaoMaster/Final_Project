using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ProjectHub.Application.Common;
using ColumnEntity = ProjectHub.Domain.Entities.Columns; // Using Alias

namespace ProjectHub.Application.Validation
{
    // Class นี้ทำหน้าที่ Validate JSON data เทียบกับ Schema
    public static class JsonDataValidator
    {
        // ย้าย Logic มาไว้ที่นี่ และทำให้เป็น static เพื่อเรียกใช้ง่าย
        public static void Validate(string jsonDataString, List<ColumnEntity> schema)
        {
            if (string.IsNullOrWhiteSpace(jsonDataString))
            {
                throw new ArgumentException("Data JSON string cannot be empty.");
            }

            JsonDocument jsonData;
            try
            {
                jsonData = JsonDocument.Parse(jsonDataString);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}", ex);
            }

            if (jsonData.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Data must be a JSON object.");
            }

            // Check if all required columns (Is_nullable = false) exist in JSON
            foreach (var column in schema)
            {
                // **STEP 1: ข้าม Formula/Lookup**
                var colType = column.Data_type?.ToUpperInvariant();
                if (colType == "FORMULA" || colType == "LOOKUP")
                {
                    // ข้ามการ Validation สำหรับคอลัมน์ที่คำนวณทีหลัง
                    continue;
                }

                // vvv --- [นี่คือโค้ด Task 2.3 ที่เพิ่มเข้ามา] --- vvv

                // ถ้าคอลัมน์นี้เป็น AutoIncrement...
                // ให้ "ข้าม" การเช็ค "Required" (Is_nullable) ไปเลย
                // เพราะ Backend (CreateRowHandler) จะเป็นคนยัดค่าให้เอง
                if (column.PrimaryKeyType == "AUTO_INCREMENT")
                {
                    continue;
                }
                // ^^^ --- [สิ้นสุดโค้ดที่เพิ่ม] --- ^^^


                // Check if property exists in JSON, consider case-insensitivity
                var propertyExists = jsonData
                    .RootElement.EnumerateObject()
                    .Any(p => p.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase));

                if (!column.Is_nullable && !propertyExists)
                {
                    throw new ArgumentException(
                        $"Required column '{column.Name}' is missing in data."
                    );
                }
            }

            // Check each property in JSON against the schema
            foreach (var property in jsonData.RootElement.EnumerateObject())
            {
                var columnName = property.Name;
                var valueElement = property.Value;

                // Find the schema definition for this column, ignoring case
                var columnSchema = schema.FirstOrDefault(c =>
                    c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                );

                if (columnSchema == null)
                {
                    // Allow extra fields or throw error? For now, be strict.
                    throw new ArgumentException(
                        $"Column '{columnName}' does not exist in the table schema."
                    );
                }

                // **STEP 2: บล็อก Formula/Lookup**
                var schemaColType = columnSchema.Data_type?.ToUpperInvariant();
                if (schemaColType == "FORMULA" || schemaColType == "LOOKUP")
                {
                    throw new ArgumentException(
                        $"Cannot provide a value for calculated column '{columnSchema.Name}'."
                    );
                }


                // Check for null if column is not nullable
                if (!columnSchema.Is_nullable && valueElement.ValueKind == JsonValueKind.Null)
                {
                    // (ข้อยกเว้น: ถ้า PK เป็น AutoIncrement User อาจจะส่ง null มาก็ได้)
                    if (columnSchema.PrimaryKeyType != "AUTO_INCREMENT")
                    {
                        throw new ArgumentException($"Column '{columnSchema.Name}' cannot be null.");
                    }
                }

                // Check Data Type (only if value is not null)
                if (valueElement.ValueKind != JsonValueKind.Null)
                {
                    // **STEP 3: เพิ่ม "IMAGE"**
                    bool typeMatch = ColumnTypeHelper.IsValidJsonValue(
                    schemaColType ?? string.Empty,
                    valueElement
                    );

                    if (!typeMatch)
                    {
                        // Throw specific error if the data type string itself is unsupported
                        if (!ColumnTypeHelper.GetKnownDataTypes().Contains(schemaColType))
                        {
                            throw new ArgumentException(
                                $"Unsupported data type '{columnSchema.Data_type}' defined in schema for column '{columnSchema.Name}'."
                            );
                        }
                        // Otherwise, it's a value mismatch
                        throw new ArgumentException(
                            $"Data type mismatch for column '{columnSchema.Name}'. Expected '{columnSchema.Data_type}' but received value '{valueElement.ToString()}' which is of type '{valueElement.ValueKind}'."
                        );
                    }
                }
            }
        }
    }
}