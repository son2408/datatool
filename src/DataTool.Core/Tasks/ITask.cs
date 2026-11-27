using System.Threading.Tasks;

namespace DataTool.Core.Tasks
{
    /// <summary>
    /// Một chức năng xử lý dữ liệu. Thêm chức năng mới chỉ cần tạo class implement interface này
    /// (hoặc kế thừa <see cref="TaskBase"/>) có constructor không tham số; menu tự sinh theo MenuPath.
    /// </summary>
    public interface ITask
    {
        /// <summary>Đường dẫn menu, phân cấp bằng "/". Ví dụ "VBDLIS/Publish code".</summary>
        string MenuPath { get; }

        /// <summary>Mô tả ngắn hiển thị trên màn hình chạy.</summary>
        string Description { get; }

        /// <summary>true nếu tác vụ thay đổi/xoá dữ liệu: bắt xác nhận, Production phải gõ lại tên môi trường.</summary>
        bool IsDestructive { get; }

        /// <summary>Tên biểu tượng Material Design (ví dụ "RocketLaunchOutline"); để trống dùng mặc định.</summary>
        string Icon { get; }

        /// <summary>Thứ tự trong nhóm menu (nhỏ đứng trước).</summary>
        int Order { get; }

        /// <summary>Phần xử lý. Dùng context.Log, context.ReportProgress, context.CancellationToken.</summary>
        Task ExecuteAsync(ITaskContext context);
    }

    /// <summary>Lớp cơ sở có sẵn giá trị mặc định, chỉ cần override MenuPath và ExecuteAsync.</summary>
    public abstract class TaskBase : ITask
    {
        public abstract string MenuPath { get; }
        public virtual string Description => string.Empty;
        public virtual bool IsDestructive => false;
        public virtual string Icon => null;
        public virtual int Order => 0;

        public abstract Task ExecuteAsync(ITaskContext context);

        /// <summary>Tên hiển thị (phần cuối của MenuPath).</summary>
        public string Title => TaskPath.GetTitle(MenuPath);
    }

    public static class TaskPath
    {
        public const char Separator = '/';

        public static string[] Split(string menuPath) =>
            (menuPath ?? string.Empty).Split(new[] { Separator }, System.StringSplitOptions.RemoveEmptyEntries);

        public static string GetTitle(string menuPath)
        {
            var parts = Split(menuPath);
            return parts.Length == 0 ? string.Empty : parts[parts.Length - 1].Trim();
        }

        public static string GetGroup(string menuPath, string separator = " › ")
        {
            var parts = Split(menuPath);
            return parts.Length <= 1 ? string.Empty : string.Join(separator, parts, 0, parts.Length - 1);
        }
    }
}
