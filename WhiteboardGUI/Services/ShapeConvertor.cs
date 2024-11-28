using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WhiteboardGUI.Models;

public class ShapeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(IShape).IsAssignableFrom(objectType) || objectType == typeof(ObservableCollection<IShape>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Check if the token is an array
        if (reader.TokenType == JsonToken.StartArray)
        {
            var shapes = new ObservableCollection<IShape>();
            var shapeArray = JArray.Load(reader);

            foreach (JToken shapeDict in shapeArray)
            {
                string shapeType = shapeDict["ShapeType"]?.ToString();
                IShape shape = shapeType switch
                {
                    "Circle" => shapeDict.ToObject<CircleShape>(),
                    "Line" => shapeDict.ToObject<LineShape>(),
                    "Scribble" => shapeDict.ToObject<ScribbleShape>(),
                    "TextShape" => shapeDict.ToObject<TextShape>(),
                    "TextboxModel" => shapeDict.ToObject<TextboxModel>(),
                    _ => throw new NotSupportedException($"Shape type '{shapeType}' not supported"),
                };
                shapes.Add(shape);
            }

            return shapes;
        }

        throw new JsonReaderException("Expected a JSON array for Shapes.");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
