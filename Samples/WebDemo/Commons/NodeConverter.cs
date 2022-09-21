using Microsoft.PowerFx.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebDemo.Commons
{
    public class NodeConverter<T> : JsonConverter<T> 
    {
        private readonly ISet<String> excludeProperty;
        public NodeConverter()
        {
            this.excludeProperty = new HashSet<string>()
            {
                "IsValid",
                "Namespace",
                "Count",
                "Parent",
                "Span"
            };
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var props = value.GetType().GetProperties().ToList();
            writer.WriteStartObject();
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, null, options);
                    break;
                default:
                    {
                        foreach (var prop in props)
                        {
                            if (!excludeProperty.Contains(prop.Name))
                            {
                                writer.WritePropertyName(prop.Name);
                                JsonSerializer.Serialize(writer, prop.GetValue(value), prop.PropertyType,
                                    options);
                            }
                        }
                        break;
                    }
            }
            writer.WriteEndObject();
        }
    }
}
