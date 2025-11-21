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

            // ======================= STEP 1: เช็ค required columns =======================
            foreach (var column in schema)
            {
                var colType = column.Data_type?.ToUpperInvariant();
                if (colType == "FORMULA")
                {

                    continue;
                }

                if (column.PrimaryKeyType == "AUTO_INCREMENT")
                {
                    continue;
                }

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

            // ======================= STEP 2: เช็คค่าที่ส่งมาทั้งหมด =======================
            foreach (var property in jsonData.RootElement.EnumerateObject())
            {
                var columnName = property.Name;
                var valueElement = property.Value;

                // หา schema ของคอลัมน์นี้ (ไม่สน case)
                var columnSchema = schema.FirstOrDefault(c =>
                    c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                );

                if (columnSchema == null)
                {
                    // ตอนนี้ strict: ถ้ามี field ที่ schema ไม่รู้จักให้ error
                    throw new ArgumentException(
                        $"Column '{columnName}' does not exist in the table schema."
                    );
                }

                var schemaColType = columnSchema.Data_type?.ToUpperInvariant();

                // เดิม: FORMULA || LOOKUP → ห้ามส่งค่า
                // ตอนนี้: FORMULA อย่างเดียวที่ห้ามส่งค่า (LOOKUP ส่งได้ปกติ)
                if (schemaColType == "FORMULA")
                {
                    throw new ArgumentException(
                        $"Cannot provide a value for calculated column '{columnSchema.Name}'."
                    );
                }

                // ถ้า column ไม่ nullable แต่ค่าที่ส่งมาเป็น null → error
                if (!columnSchema.Is_nullable && valueElement.ValueKind == JsonValueKind.Null)
                {
                    // ยกเว้น PK แบบ AUTO_INCREMENT
                    if (columnSchema.PrimaryKeyType != "AUTO_INCREMENT")
                    {
                        throw new ArgumentException($"Column '{columnSchema.Name}' cannot be null.");
                    }
                }

                // ถ้าค่าไม่ใช่ null → เช็ค type
                if (valueElement.ValueKind != JsonValueKind.Null)
                {
                    bool typeMatch = ColumnTypeHelper.IsValidJsonValue(
                        schemaColType ?? string.Empty,
                        valueElement
                    );

                    if (!typeMatch)
                    {
                        // data type ใน schema ไม่รู้จักเลย
                        if (!ColumnTypeHelper.GetKnownDataTypes().Contains(schemaColType))
                        {
                            throw new ArgumentException(
                                $"Unsupported data type '{columnSchema.Data_type}' defined in schema for column '{columnSchema.Name}'."
                            );
                        }

                        // ค่า type ไม่ตรงกับ schema
                        throw new ArgumentException(
                            $"Data type mismatch for column '{columnSchema.Name}'. Expected '{columnSchema.Data_type}' but received value '{valueElement}' which is of type '{valueElement.ValueKind}'."
                        );
                    }
                }
            }
        }
    }
}
