using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Serilog.Core;
using Serilog.Events;

class MessageDestructuringPolicy : IDestructuringPolicy
{
    const string HiddenMask = "********";

    /// <summary>
    /// Destructure a protobuf message into a Serilog structured log entry.
    /// This also allows masking out fields marked as hidden.
    /// </summary>
    /// <inheritdoc />
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        if (value is not IMessage message)
        {
            result = null;
            return false;
        }

        result = LogMessage(propertyValueFactory, message);
        return true;
    }

    StructureValue LogMessage(ILogEventPropertyValueFactory propertyValueFactory, IMessage message)
    {
        var structureProperties = new List<LogEventProperty>();

        foreach (var fieldDescriptor in message.Descriptor.Fields.InDeclarationOrder())
        {
            var oneOfDescriptor = fieldDescriptor.ContainingOneof;
            if (oneOfDescriptor != null &&
                oneOfDescriptor.Accessor.GetCaseFieldDescriptor(message) != fieldDescriptor)
            {
                continue;
            }

            var value = fieldDescriptor.Accessor.GetValue(message);

            LogEventPropertyValue logEventPropertyValue;
            if (fieldDescriptor.IsMap)
            {
                logEventPropertyValue = LogMap(propertyValueFactory, value, fieldDescriptor.IsHidden());
            }
            else if (fieldDescriptor.IsRepeated)
            {
                logEventPropertyValue = LogRepeated(propertyValueFactory, value, fieldDescriptor.IsHidden());
            }
            else
            {
                logEventPropertyValue = LogField(propertyValueFactory, value, fieldDescriptor.IsHidden());
            }

            structureProperties.Add(new LogEventProperty(fieldDescriptor.Name, logEventPropertyValue));
        }

        return new StructureValue(structureProperties);
    }

    DictionaryValue LogMap(ILogEventPropertyValueFactory propertyValueFactory, object value, bool isHidden)
    {
        var dict = (IDictionary)value;
        var logEventPropertyValues = new Dictionary<ScalarValue, LogEventPropertyValue>(dict.Count);

        foreach (DictionaryEntry entry in dict)
        {
            var key = new ScalarValue(LogField(propertyValueFactory, entry.Key, isHidden));
            var val = LogField(propertyValueFactory, entry.Value, isHidden);
            logEventPropertyValues.Add(key, val);
        }

        return new DictionaryValue(logEventPropertyValues);
    }

    SequenceValue LogRepeated(ILogEventPropertyValueFactory propertyValueFactory, object value, bool isHidden)
    {
        var list = (IList)value;
        var logEventPropertyValues = new LogEventPropertyValue[list.Count];

        for (var i = 0; i < list.Count; ++i)
        {
            logEventPropertyValues[i] = LogField(propertyValueFactory, list[i], isHidden);
        }

        return new SequenceValue(logEventPropertyValues);
    }

    LogEventPropertyValue LogField(ILogEventPropertyValueFactory propertyValueFactory, object? value, bool isHidden)
    {
        if (value is IMessage message)
        {
            return LogMessage(propertyValueFactory, message);
        }

        if (isHidden)
        {
            value = HiddenMask;
        }

        return propertyValueFactory.CreatePropertyValue(value);
    }
}
