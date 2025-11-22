using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ProjectHub.Application.Common;
using ColumnEntity = ProjectHub.Domain.Entities.Columns; 

namespace ProjectHub.Application.Validation
{
    
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

            
            foreach (var property in jsonData.RootElement.EnumerateObject())
            {
                var columnName = property.Name;
                var valueElement = property.Value;

                
                var columnSchema = schema.FirstOrDefault(c =>
                    c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                );

                if (columnSchema == null)
                {
                    
                    throw new ArgumentException(
                        $"Column '{columnName}' does not exist in the table schema."
                    );
                }

                var schemaColType = columnSchema.Data_type?.ToUpperInvariant();

                
                
                if (schemaColType == "FORMULA")
                {
                    throw new ArgumentException(
                        $"Cannot provide a value for calculated column '{columnSchema.Name}'."
                    );
                }

                
                if (!columnSchema.Is_nullable && valueElement.ValueKind == JsonValueKind.Null)
                {
                    
                    if (columnSchema.PrimaryKeyType != "AUTO_INCREMENT")
                    {
                        throw new ArgumentException($"Column '{columnSchema.Name}' cannot be null.");
                    }
                }

                
                if (valueElement.ValueKind != JsonValueKind.Null)
                {
                    bool typeMatch = ColumnTypeHelper.IsValidJsonValue(
                        schemaColType ?? string.Empty,
                        valueElement
                    );

                    if (!typeMatch)
                    {
                        
                        if (!ColumnTypeHelper.GetKnownDataTypes().Contains(schemaColType))
                        {
                            throw new ArgumentException(
                                $"Unsupported data type '{columnSchema.Data_type}' defined in schema for column '{columnSchema.Name}'."
                            );
                        }

                        
                        throw new ArgumentException(
                            $"Data type mismatch for column '{columnSchema.Name}'. Expected '{columnSchema.Data_type}' but received value '{valueElement}' which is of type '{valueElement.ValueKind}'."
                        );
                    }
                }
            }
        }
    }
}
