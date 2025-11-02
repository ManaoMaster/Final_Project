using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

                // Check for null if column is not nullable
                if (!columnSchema.Is_nullable && valueElement.ValueKind == JsonValueKind.Null)
                {
                    throw new ArgumentException($"Column '{columnSchema.Name}' cannot be null.");
                }

                // Check Data Type (only if value is not null)
                if (valueElement.ValueKind != JsonValueKind.Null)
                {
                    // Use ToUpperInvariant for reliable comparison
                    bool typeMatch = columnSchema.Data_type?.ToUpperInvariant() switch
                    {
                        "TEXT" => valueElement.ValueKind == JsonValueKind.String,
                        "INTEGER" => valueElement.ValueKind == JsonValueKind.Number
                            && valueElement.TryGetInt64(out _),
                        "REAL" => valueElement.ValueKind == JsonValueKind.Number,
                        "BOOLEAN" => valueElement.ValueKind == JsonValueKind.True
                            || valueElement.ValueKind == JsonValueKind.False,
                        "STRING" => valueElement.ValueKind == JsonValueKind.String,
                        "INT" => valueElement.ValueKind == JsonValueKind.Number
                            && valueElement.TryGetInt64(out _),
                        // Add other specific types (e.g., DATETIME)
                        _ => false, // Default to false if type is unknown/unsupported
                    };

                    if (!typeMatch)
                    {
                        // Throw specific error if the data type string itself is unsupported
                        if (
                            !new[]
                            {
                                "TEXT",
                                "INTEGER",
                                "REAL",
                                "BOOLEAN",
                                "STRING",
                                "INT",
                            }.Contains(columnSchema.Data_type?.ToUpperInvariant())
                        )
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
