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

        /// <summary>Mở cửa sổ quản lý cấu hình. Trả về true nếu người dùng đã lưu.</summary>
        bool ShowConfiguration(ConfigurationViewModel viewModel);

        void OpenFolder(string path);
    }
}
