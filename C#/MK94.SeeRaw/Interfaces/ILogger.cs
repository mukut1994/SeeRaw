using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MK94.SeeRaw.Interfaces
{
    public interface ILogger : INotifyPropertyChanged, IDisposable
    {
        string Message { get; }
        List<ILogger> Children { get; }

        ILogger CreateChild(string message = null);
        void RemoveSelf();
        ILogger WithMessage(string messaage);

        IEnumerable<T> EnumerateWithProgress<T>(List<T> collection);

        void Serialize(Serializer serializer, Utf8JsonWriter writer, RendererContext context, bool serializeNulls)
        {
            void writeILogger(ILogger log)
            {
                writer.WriteString("type", "log");
                writer.WriteString("message", Message);

                if (log.Children.Any())
                    writeChildren(log.Children);
            }

            void writeChildren(List<ILogger> children)
            {
                writer.WriteStartArray("children");

                foreach (var child in children)
                {
                    writer.WriteStartObject();

                    writeILogger(child);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            };

            writeILogger(this);
        }
    }
}
