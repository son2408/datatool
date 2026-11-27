using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataTool.Core.Provinces;
using YamlDotNet.RepresentationModel;

namespace DataTool.Configuration
{
    /// <summary>
    /// Đọc và ghi file cấu hình tỉnh. Khi ghi, chỉ thay phần giá trị trên dòng của từng khoá,
    /// nên comment, thứ tự khoá và dòng trống trong file gốc được giữ nguyên.
    /// </summary>
    public static class ProvinceYaml
    {
        private static readonly Regex KeyLine = new Regex(@"^(?<key>[A-Za-z0-9_\-\.]+)\s*:(?<rest>.*)$", RegexOptions.Compiled);

        /// <summary>Đọc các cặp khoá/giá trị ở cấp ngoài cùng, theo thứ tự trong file.</summary>
        public static List<KeyValuePair<string, string>> Parse(string text)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            var stream = new YamlStream();
            using (var reader = new StringReader(text))
                stream.Load(reader);

            if (stream.Documents.Count == 0 || !(stream.Documents[0].RootNode is YamlMappingNode root))
                return result;

            foreach (var entry in root.Children)
            {
                string key = (entry.Key as YamlScalarNode)?.Value;
                if (string.IsNullOrEmpty(key)) continue;
                string value = entry.Value is YamlScalarNode scalar ? scalar.Value ?? string.Empty : entry.Value.ToString();
                result.Add(new KeyValuePair<string, string>(key, value));
            }
            return result;
        }

        /// <summary>
        /// Ghi giá trị mới vào nội dung file gốc. Khoá đã có: thay giá trị tại chỗ.
        /// Khoá chưa có: thêm vào cuối file.
        /// </summary>
        public static string Update(string originalText, IEnumerable<KeyValuePair<string, string>> values)
        {
            var pending = values.ToList();
            string newline = (originalText ?? string.Empty).Contains("\r\n") ? "\r\n" : "\n";
            var lines = (originalText ?? string.Empty).Replace("\r\n", "\n").Split('\n').ToList();
            var written = new HashSet<string>(StringComparer.Ordinal);
            var map = pending.GroupBy(p => p.Key).ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.Ordinal);

            for (int i = 0; i < lines.Count; i++)
            {
                var m = KeyLine.Match(lines[i]);
                if (!m.Success) continue;
                string key = m.Groups["key"].Value;
                if (!map.TryGetValue(key, out var value) || written.Contains(key)) continue;

                string comment = ExtractTrailingComment(m.Groups["rest"].Value);
                lines[i] = key + ": " + FormatValue(key, value) + comment;
                written.Add(key);
            }

            var missing = pending.Where(p => !written.Contains(p.Key)).Select(p => p.Key).Distinct().ToList();
            if (missing.Count > 0)
            {
                while (lines.Count > 0 && lines[lines.Count - 1].Trim().Length == 0) lines.RemoveAt(lines.Count - 1);
                foreach (var key in missing)
                    lines.Add(key + ": " + FormatValue(key, map[key]));
                lines.Add(string.Empty);
            }

            return string.Join(newline, lines);
        }

        /// <summary>Nội dung file mới theo bố cục chuẩn (có comment nhóm).</summary>
        public static string CreateTemplate(IEnumerable<KeyValuePair<string, string>> values)
        {
            var map = values.GroupBy(p => p.Key).ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.Ordinal);
            string V(string key) => key + ": " + FormatValue(key, map.TryGetValue(key, out var v) ? v : string.Empty);

            var sb = new StringBuilder();
            sb.AppendLine(V(ProvinceKeys.ServiceKey));
            sb.AppendLine(V(ProvinceKeys.RepositoryBackend));
            sb.AppendLine("#solr");
            sb.AppendLine(V(ProvinceKeys.SolrUri));
            sb.AppendLine(V(ProvinceKeys.SolrKey));
            sb.AppendLine();
            sb.AppendLine("#mongodb");
            sb.AppendLine(V(ProvinceKeys.VdmsConnectionString));
            sb.AppendLine(V(ProvinceKeys.LogConnectionString));
            sb.AppendLine();
            sb.AppendLine("#vdfs");
            sb.AppendLine(V(ProvinceKeys.VdfsApiEndpoint));
            sb.AppendLine(V(ProvinceKeys.VdfsTag));
            sb.AppendLine(V(ProvinceKeys.VdfsRabbitUri));
            sb.AppendLine(V(ProvinceKeys.VdfsExchange));
            sb.AppendLine();
            sb.AppendLine("#service_clients");
            sb.AppendLine(V(ProvinceKeys.RepoEndpoint));
            sb.AppendLine(V(ProvinceKeys.IsNewRepo));
            sb.AppendLine();
            sb.AppendLine("#rabbitmq");
            sb.AppendLine(V(ProvinceKeys.RabbitUri));

            var extras = map.Keys.Where(k => !ProvinceKeys.All.Contains(k)).ToList();
            if (extras.Count > 0)
            {
                sb.AppendLine();
                foreach (var key in extras) sb.AppendLine(V(key));
            }
            return sb.ToString();
        }

        /// <summary>Chuỗi luôn đặt trong ngoặc kép; is_new_repo và giá trị true/false/số ghi trần.</summary>
        internal static string FormatValue(string key, string value)
        {
            value = value ?? string.Empty;
            bool isBool = value == "true" || value == "false";
            if (key == ProvinceKeys.IsNewRepo)
                return isBool ? value : "false";
            if (isBool || (value.Length > 0 && value.All(char.IsDigit)))
                return value;
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        // Giữ comment cuối dòng dạng `key: "value"  # ghi chú`
        private static string ExtractTrailingComment(string rest)
        {
            bool inSingle = false, inDouble = false;
            for (int i = 0; i < rest.Length; i++)
            {
                char c = rest[i];
                if (c == '\\' && inDouble) { i++; continue; }
                if (c == '"' && !inSingle) inDouble = !inDouble;
                else if (c == '\'' && !inDouble) inSingle = !inSingle;
                else if (c == '#' && !inSingle && !inDouble && i > 0 && char.IsWhiteSpace(rest[i - 1]))
                {
                    int start = i;
                    while (start > 0 && char.IsWhiteSpace(rest[start - 1])) start--;
                    return rest.Substring(start).TrimEnd();
                }
            }
            return string.Empty;
        }
    }
}
