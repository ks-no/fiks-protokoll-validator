using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace KS.FiksProtokollValidator.WebAPI.Logging
{
    public class CustomLogstashJsonFormatter : ITextFormatter
  {
    private static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter();

    public void Format(LogEvent logEvent, TextWriter output)
    {
      CustomLogstashJsonFormatter.FormatContent(logEvent, output);
      output.WriteLine();
    }

    private static void FormatContent(LogEvent logEvent, TextWriter output)
    {
      if (logEvent == null)
        throw new ArgumentNullException(nameof (logEvent));
      if (output == null)
        throw new ArgumentNullException(nameof (output));
      output.Write('{');
      CustomLogstashJsonFormatter.WritePropertyAndValue(output, "@timestamp", logEvent.Timestamp.ToString("o"));
      output.Write(",");
      CustomLogstashJsonFormatter.WritePropertyAndValue(output, "level", logEvent.Level.ToString());
      output.Write(",");
      CustomLogstashJsonFormatter.WritePropertyAndValue(output, "message", logEvent.MessageTemplate.Render(logEvent.Properties));
      if (logEvent.Exception != null)
      {
        output.Write(",");
        CustomLogstashJsonFormatter.WritePropertyAndValue(output, "exception", logEvent.Exception.ToString());
      }
      CustomLogstashJsonFormatter.WriteProperties(logEvent.Properties, output);
      output.Write('}');
    }

    private static void WritePropertyAndValue(
      TextWriter output,
      string propertyKey,
      string propertyValue)
    {
      JsonValueFormatter.WriteQuotedJsonString(propertyKey, output);
      output.Write(":");
      JsonValueFormatter.WriteQuotedJsonString(propertyValue, output);
    }

    private static void WriteProperties(
      IReadOnlyDictionary<string, LogEventPropertyValue> properties,
      TextWriter output)
    {
      if (properties.Any<KeyValuePair<string, LogEventPropertyValue>>())
        output.Write(",");
      string str = "";
      foreach (KeyValuePair<string, LogEventPropertyValue> property in (IEnumerable<KeyValuePair<string, LogEventPropertyValue>>) properties)
      {
        output.Write(str);
        str = ",";
        JsonValueFormatter.WriteQuotedJsonString(property.Key[0].ToString().ToLower() + property.Key.Substring(1), output);
        output.Write(':');
        CustomLogstashJsonFormatter.ValueFormatter.Format(property.Value, output);
      }
    }
  }
}