using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataTool.App.Services;
using DataTool.Core.Tasks;

namespace DataTool.App.ViewModels
{
    /// <summary>Một ô nhập tham số trên màn hình chạy tác vụ, sinh từ <see cref="TaskParameter"/>.</summary>
    public class TaskParameterViewModel : ObservableObject
    {
        private readonly IDialogService _dialogs;
        private string _value;
        private string _error;

        public TaskParameterViewModel(TaskParameter definition, IDialogService dialogs)
        {
            Definition = definition;
            _dialogs = dialogs;
            _value = definition.DefaultValue ?? string.Empty;
            BrowseCommand = new RelayCommand(Browse);
        }

        public TaskParameter Definition { get; }
        public string Key => Definition.Key;
        public string Label => Definition.Label;
        public string Hint => Definition.Hint;
        public string Placeholder => Definition.Placeholder;
        public TaskParameterKind Kind => Definition.Kind;

        public string Value
        {
            get => _value;
            set
            {
                if (!SetProperty(ref _value, value)) return;
                Error = null;
                OnPropertyChanged(nameof(Summary));
            }
        }

        public string Error
        {
            get => _error;
            private set => SetProperty(ref _error, value);
        }

        /// <summary>Dòng tóm tắt dưới ô danh sách mã: số mã hợp lệ, số mã trùng đã bỏ.</summary>
        public string Summary
        {
            get
            {
                if (Kind != TaskParameterKind.CodeList) return null;
                int count = CodeListParser.Parse(Value).Count;
                int duplicates = CodeListParser.CountDuplicates(Value);
                if (count == 0) return "Chưa có mã nào";
                return duplicates > 0
                    ? $"{count} mã (đã bỏ {duplicates} mã trùng)"
                    : $"{count} mã";
            }
        }

        public RelayCommand BrowseCommand { get; }

        /// <summary>Kiểm tra giá trị, hiện lỗi ngay dưới ô. Trả về true nếu hợp lệ.</summary>
        public bool Validate()
        {
            Error = Definition.Validate(Value);
            return Error == null;
        }

        private void Browse()
        {
            if (Kind != TaskParameterKind.Folder) return;
            var folder = _dialogs.PickFolder(Label, Value);
            if (!string.IsNullOrEmpty(folder)) Value = folder;
        }
    }
}
