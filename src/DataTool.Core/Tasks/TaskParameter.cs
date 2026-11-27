using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTool.Core.Tasks
{
    /// <summary>Loại ô nhập của tham số; giao diện tự chọn control phù hợp.</summary>
    public enum TaskParameterKind
    {
        /// <summary>Một dòng chữ.</summary>
        Text,

        /// <summary>Danh sách mã, ngăn cách bằng dấu phẩy, chấm phẩy, khoảng trắng hoặc xuống dòng.</summary>
        CodeList,

        /// <summary>Đường dẫn thư mục (có nút chọn thư mục).</summary>
        Folder
    }

    /// <summary>
    /// Mô tả một tham số người dùng nhập trước khi chạy tác vụ.
    /// Tác vụ khai báo danh sách tham số, màn hình chạy tự sinh form; giá trị nhận lại qua ITaskContext.Parameters[Key].
    /// </summary>
    public sealed class TaskParameter
    {
        public TaskParameter(string key, string label, TaskParameterKind kind = TaskParameterKind.Text)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Thiếu key của tham số.", nameof(key));
            Key = key;
            Label = label ?? key;
            Kind = kind;
        }

        public string Key { get; }
        public string Label { get; }
        public TaskParameterKind Kind { get; }

        /// <summary>Chú thích ngắn cạnh nhãn.</summary>
        public string Hint { get; set; }

        /// <summary>Chữ mờ gợi ý trong ô nhập.</summary>
        public string Placeholder { get; set; }

        public bool IsRequired { get; set; } = true;

        public string DefaultValue { get; set; }

        /// <summary>Kiểm tra giá trị; trả về thông báo lỗi hoặc null nếu hợp lệ.</summary>
        public string Validate(string value)
        {
            bool empty = Kind == TaskParameterKind.CodeList
                ? CodeListParser.Parse(value).Count == 0
                : string.IsNullOrWhiteSpace(value);

            if (empty) return IsRequired ? $"Chưa nhập \"{Label}\"." : null;
            return null;
        }
    }

    /// <summary>Tách chuỗi "12345, 12346;12347" thành danh sách mã (bỏ khoảng trắng, bỏ trùng, giữ thứ tự).</summary>
    public static class CodeListParser
    {
        private static readonly char[] Separators = { ',', ';', ' ', '\t', '\r', '\n' };

        public static IReadOnlyList<string> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return text.Split(Separators, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => s.Length > 0 && seen.Add(s))
                       .ToList();
        }

        /// <summary>Số mã bị trùng (đã bỏ khi Parse).</summary>
        public static int CountDuplicates(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            int all = text.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Count(s => s.Trim().Length > 0);
            return all - Parse(text).Count;
        }
    }
}
