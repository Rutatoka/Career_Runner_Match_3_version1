using RuStore.UnityInstallReferrer.Model;
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace RuStore.UnityInstallReferrer.Internal {

    public class BaseJsonSerializer : IJsonSerializer {

        public string Serialize<T>(T value, bool prettyPrint = false) =>
            Serialize(value, prettyPrint, 0);

        private string Serialize(object obj, bool prettyPrint, int indentLevel) {
            if (obj == null) return "null";

            if (obj is string str) return $"\"{EscapeJsonString(str)}\"";
            if (obj is Enum enumValue) return $"\"{ToCamelCase(enumValue.ToString())}\"";
            if (IsNumeric(obj)) return obj.ToString();
            if (obj is bool boolValue) return boolValue.ToString().ToLower();
            if (obj is ICollection collection) return SerializeCollection(collection, prettyPrint, indentLevel);
            if (IsBaseValue(obj)) return SerializeBaseValue(obj);
            if (obj is DateTime) return SerializeDateTime(obj);

            return SerializeObject(obj, prettyPrint, indentLevel);
        }

        private string SerializeDateTime(object obj) {
            var dateString = (obj as DateTime?)?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return $"\"{dateString}\"";
        }

        private string SerializeCollection(ICollection collection, bool prettyPrint, int indentLevel) {
            if (collection.Count == 0) return "[]";

            var sb = new StringBuilder();
            sb.Append("[");

            string indent = prettyPrint ? GetIndent(indentLevel + 1) : "";
            bool first = true;

            foreach (object item in collection) {
                if (!first) sb.Append(",");
                first = false;

                if (prettyPrint) {
                    sb.AppendLine();
                    sb.Append(indent);
                }

                sb.Append(Serialize(item, prettyPrint, indentLevel + 1));
            }

            if (prettyPrint) {
                sb.AppendLine();
                sb.Append(GetIndent(indentLevel));
            }

            sb.Append("]");

            return sb.ToString();
        }

        private string SerializeObject(object obj, bool prettyPrint, int indentLevel) {
            Type type = obj.GetType();
            var sb = new StringBuilder();
            sb.Append("{");

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // Для вложенных классов добавляем поле "type" с именем типа
            bool isNested = type.IsNested;
            if (!isNested && properties.Length == 0) return "{}";

            string indent = prettyPrint ? GetIndent(indentLevel + 1) : "";
            bool first = true;

            if (isNested) {
                if (prettyPrint) {
                    sb.AppendLine();
                    sb.Append(indent);
                }
                sb.Append($"\"type\":\"{type.Name}\"");
                first = false;
            }

            foreach (PropertyInfo prop in properties) {
                object value = prop.GetValue(obj);
                
                // Пропускаем null значения и пустые строки
                if (value == null || (value is string strValue && string.IsNullOrEmpty(strValue))) {
                    continue;
                }

                if (!first) sb.Append(",");
                first = false;

                if (prettyPrint) {
                    sb.AppendLine();
                    sb.Append(indent);
                }

                sb.Append($"\"{prop.Name}\":");
                if (prettyPrint) sb.Append(" ");

                sb.Append(Serialize(value, prettyPrint, indentLevel + 1));
            }

            if (prettyPrint) {
                sb.AppendLine();
                sb.Append(GetIndent(indentLevel));
            }

            sb.Append("}");

            return sb.ToString();
        }

        private string SerializeBaseValue(object obj) {
            // Сериализуем BaseValue<T> как примитив (значение свойства value)
            Type type = obj.GetType();
            PropertyInfo valueProperty = type.GetProperty("value");
            if (valueProperty != null) {
                object value = valueProperty.GetValue(obj);
                return Serialize(value, false, 0);
            }
            return SerializeObject(obj, false, 0);
        }

        private bool IsNumeric(object value) =>
            value is int or long or float or double or decimal;

        private static bool IsBaseValue(object obj) {
            if (obj == null) return false;
            Type type = obj.GetType();
            while (type != null) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseValue<>))
                    return true;
                type = type.BaseType;
            }

            return false;
        }

        private static string GetIndent(int level) =>
            new string(' ', level * 4);

        private static string ToCamelCase(string value) {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        private static string EscapeJsonString(string value) {
            if (string.IsNullOrEmpty(value)) return value;
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
