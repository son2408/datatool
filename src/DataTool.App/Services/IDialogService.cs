using DataTool.App.ViewModels;

namespace DataTool.App.Services
{
    /// <summary>Lớp trung gian để ViewModel hiện hộp thoại mà không phụ thuộc vào View.</summary>
    public interface IDialogService
    {
        bool Confirm(string title, string message, string confirmText = "Đồng ý");

        /// <summary>Xác nhận thao tác nguy hiểm. requiredText khác null thì phải gõ lại đúng chuỗi đó.</summary>
        bool ConfirmDanger(string title, string message, string requiredText = null, string confirmText = "Xác nhận");

        void ShowInfo(string title, string message);
        void ShowError(string title, string message);

        /// <summary>Hỏi một dòng chữ; trả về null nếu huỷ. validate trả về thông báo lỗi hoặc null.</summary>
        string Prompt(string title, string message, string defaultValue = "", System.Func<string, string> validate = null);

        /// <summary>Chọn thư mục; trả về null nếu huỷ.</summary>
        string PickFolder(string title, string initialFolder);

        /// <summary>Mở cửa sổ cấu hình tỉnh (chặn đến khi đóng).</summary>
        void ShowConfiguration(ConfigurationViewModel viewModel);

        void OpenFolder(string path);
        void OpenFile(string path);
    }
}
