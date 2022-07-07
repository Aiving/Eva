using Eva.Util.Attributes;
using Eva.Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

// See the ReadMe.html for additional information
public class ObjectInspector
{
    private int _level;
    private readonly int? _maxLevel;
    private readonly int _indentSize;
    private readonly StringBuilder _stringBuilder;
    private readonly List<int> _hashListOfFoundElements;

    private ObjectInspector(int indentSize, int? maxLevel)
    {
        _indentSize = indentSize;
        _maxLevel = maxLevel;
        _stringBuilder = new StringBuilder();
        _hashListOfFoundElements = new List<int>();
    }

    public static string Inspect(object element, int indentSize, int maxLevel)
    {
        ObjectInspector instance = new(indentSize, maxLevel);
        return instance.DumpElement(element);
    }

#nullable enable
    private string DumpElement(object? element)
    {
        if (element is null or ValueType or string)
        {
            Write(FormatValue(element));
        }
        else
        {
            Type objectType = element.GetType();

            if (_level > _maxLevel)
            {
                // Write("[{0}]", objectType.GetFriendlyName());
                return _stringBuilder.ToString();
            }

            // if(!typeof(IEnumerable).IsAssignableFrom(objectType)) {
            Write("{{{0}}}", objectType.GetFriendlyName());
            _hashListOfFoundElements.Add(element.GetHashCode());
            _level++;
            // }

            if (element is IEnumerable enumerableElement)
            {
                foreach (object item in enumerableElement)
                {
                    Type itemType = item.GetType();
                    if (_level > _maxLevel - 1 && !itemType.IsAssignableFrom(typeof(string)) && !itemType.IsValueType)
                    {
                        Write("{{{0}}}", itemType.GetFriendlyName());
                    }
                    else
                    {
                        if (item is IEnumerable && item is not string)
                        {
                            _level++;
                            DumpElement(item);
                            _level--;
                        }
                        else
                        {
                            if (!AlreadyTouched(item)) DumpElement(item);
                            else Write("{{{0}}} <-- bidirectional reference found", item.GetType().GetFriendlyName());
                        }
                    }
                }
            }
            else
            {
                MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                MethodInfo[] propertyMethods = members
                  .Where((member) => member is PropertyInfo)
                  .Cast<PropertyInfo>()
                  .SelectMany((property) => new MethodInfo?[] { property.GetMethod, property.SetMethod })
                  .Where((method) => method != null)
                  .Cast<MethodInfo>()
                  .ToArray();
                foreach (var memberInfo in members)
                {
                    FieldInfo? fieldInfo = memberInfo as FieldInfo;
                    PropertyInfo? propertyInfo = memberInfo as PropertyInfo;
                    MethodInfo? methodInfo = memberInfo as MethodInfo;

                    if (fieldInfo == null && propertyInfo == null && methodInfo == null) continue;

                    // Skip typeof(object) members
                    if (memberInfo.DeclaringType == typeof(object)) continue;

                    InspectHiddenAttribute? hiddenAttribute = memberInfo.GetCustomAttribute<InspectHiddenAttribute>();
                    if (hiddenAttribute != null)
                    {
                        Write("{0}: [Hidden]", memberInfo.Name);
                        continue;
                    }

                    if (methodInfo != null)
                    {
                        if (methodInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null) continue;
                        if (propertyMethods.Contains(methodInfo)) continue;

                        string methodParameters = string.Join(", ", methodInfo.GetParameters().Select((parameter) =>
                        {
                            StringBuilder builder = new();

                            if (parameter.IsIn) builder.Append("in ");
                            if (parameter.IsOut) builder.Append("out ");
                            if (parameter.IsRetval) builder.Append("retval ");

                            builder.Append(parameter.ParameterType.GetFriendlyName());
                            if (parameter.IsOptional) builder.Append('?');

                            string? name = parameter.Name;
                            if (name != null)
                            {
                                builder.Append(' ');
                                builder.Append(parameter.Name);
                            }

                            return builder.ToString();
                        }));

                        Write("{0} {1}({2})", methodInfo.ReturnType.GetFriendlyName(), methodInfo.Name, methodParameters);
                    }
                    else if (fieldInfo != null || propertyInfo != null)
                    {
                        Type type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                        object? value = null;
                        Exception? getException = null;
                        if (propertyInfo != null)
                        {
                            // Try to get property value
                            try
                            {
                                value = propertyInfo.GetValue(element, null);
                            }
                            catch (Exception exception)
                            {
                                getException = exception.InnerException ?? exception;
                            }
                        }
                        else if (fieldInfo != null)
                        {
                            value = fieldInfo.GetValue(element);
                        }

                        if (getException != null)
                        {
                            Write("{0} = [Exception: {1}]", memberInfo.Name, $"{getException.GetType()}: {getException.Message}");
                        }
                        else
                        {
                            if (type.IsValueType || type == typeof(string))
                            {
                                Write("{0} = {1}", memberInfo.Name, FormatValue(value));
                            }
                            else
                            {
                                bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                                Write("{0} = {1}", memberInfo.Name, isEnumerable ? $"[{type.GetFriendlyName()}]" : $"{{{type.GetFriendlyName()}}}");

                                bool alreadyTouched = !isEnumerable && AlreadyTouched(value);
                                _level++;
                                if (!alreadyTouched) DumpElement(value);
                                else Write("{{{0}}} <-- bidirectional reference found", value.GetType().GetFriendlyName());
                                _level--;
                            }
                        }
                    }
                }
            }

            if (!typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                _level--;
            }
        }

        return _stringBuilder.ToString();
    }

    private bool AlreadyTouched(object? value)
    {
        if (value == null) return false;

        int hash = value.GetHashCode();
        return _hashListOfFoundElements.Any(existingHash => existingHash == hash);
    }

    private void Write(string value, params object[]? args)
    {
        string space = new(' ', _level * _indentSize);

        if (args != null) value = string.Format(value, args);

        _stringBuilder.AppendLine(space + value);
    }

    private static string FormatValue(object? o)
    {
        string result = o switch
        {
            null => "null",
            DateTime time => time.ToShortDateString(),
            string => $"\"{o.ToString()?.Replace("\"", "\\\"") ?? "null"}\"",
            '\0' => string.Empty,
            ValueType => o.ToString() ?? "null",
            IEnumerable => "[Enumeration]",
            _ => $"{{{o.GetType().GetFriendlyName()}}}"
        };

        return result
          .Replace("{", "{{")
          .Replace("}", "}}");
    }
}