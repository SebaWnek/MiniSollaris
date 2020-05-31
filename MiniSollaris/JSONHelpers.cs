using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MiniSollaris
{
    public static class JSONHelpers
    {

        /// <summary>
        /// Serializes Solar System to JSON for future use
        /// </summary>
        /// <param name="system">Solar system object to be serialized</param>
        /// <param name="path">Path to file</param>
        public static void Serialize(this SolarSystem system, string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new EclipseConverter());
            string jsonString = JsonSerializer.Serialize(system.Objects, options);
            File.WriteAllText(path, jsonString);
        }
        public static void Serialize(this List<CelestialObject> system, string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new EclipseConverter());
            string jsonString = JsonSerializer.Serialize(system, options);
            File.WriteAllText(path, jsonString);
        }
        /// <summary>
        /// Deserializes Solar System from JSON file.
        /// Intended to be used only in class constructor!
        /// </summary>
        /// <param name="path">Path to JSON file</param>
        /// <returns>Array of Celestial Objects</returns>
        public static CelestialObject[] Deserialize(string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EclipseConverter());
            CelestialObject[] result;
            string jsonString = File.ReadAllText(path);
            result = JsonSerializer.Deserialize<CelestialObject[]>(jsonString, options);
            return result;
        }

        public class EclipseConverter : JsonConverter<Ellipse>
        {
            public override Ellipse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                Ellipse ellipse = new Ellipse();
                var converter = new BrushConverter();
                reader.Read();
                reader.Read();
                ellipse.Fill = (Brush)converter.ConvertFromString(reader.GetString());
                reader.Read();
                reader.Read();
                ellipse.Width = ellipse.Height = reader.GetInt32();
                reader.Read();
                return ellipse;
            }

            public override void Write(Utf8JsonWriter writer, Ellipse value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Color", value.Fill.ToString());
                writer.WriteNumber("Size", value.Width);
                writer.WriteEndObject();
            }
        }
    }
}
