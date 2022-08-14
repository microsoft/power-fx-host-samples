using Microsoft.PowerFx.Syntax;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebDemo.Commons
{
    public class TexlNodeConverter : JsonConverter<TexlNode>
    {
        public override TexlNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TexlNode value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (TexlNode)null, options);
                    break;
                default:
                {
                    var type = value.GetType();
                    JsonSerializer.Serialize(writer, value, type, options);
                    break;
                }
            }
        }
    }
}
