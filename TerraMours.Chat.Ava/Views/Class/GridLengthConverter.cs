using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.Views.Class {
    public class GridLengthConverter : JsonConverter<GridLength> {
        public override GridLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject) {
                throw new JsonException();
            }

            double value = 0;
            GridUnitType gridUnitType = GridUnitType.Pixel;

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject) {
                    return new GridLength(value, gridUnitType);
                }

                if (reader.TokenType == JsonTokenType.PropertyName) {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName) {
                        case "Value":
                            value = reader.GetDouble();
                            break;
                        case "GridUnitType":
                            gridUnitType = (GridUnitType)reader.GetInt32();
                            break;
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteNumber("Value", value.Value);
            writer.WriteNumber("GridUnitType", (int)value.GridUnitType);
            writer.WriteEndObject();
        }
    }
}
