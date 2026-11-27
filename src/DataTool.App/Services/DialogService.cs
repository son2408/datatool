using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using DataTool.App.ViewModels;
using DataTool.App.Views;
using DataTool.UI.Controls;

namespace DataTool.App.Services
{
    public class DialogService : IDialogService
    {
        public bool Confirm(string title, string message, string confirmText = "Đồng ý")
            => ConfirmDialog.Show(ActiveWindow, title, message, DialogKind.Question, confirmText);

        public bool ConfirmDanger(string title, string message, string requiredText = null, string confirmText = "Xác nhận")
            => ConfirmDialog.Show(ActiveWindow, title, message, DialogKind.Danger, confirmText, "Huỷ", requiredText);

        public void ShowInfo(string title, string message)
            => ConfirmDialog.ShowMessage(ActiveWindow, title, message, DialogKind.Information);

        public void ShowError(string title, string message)
            => ConfirmDialog.ShowMessage(ActiveWindow, title, message, DialogKind.Error);

        public string Prompt(string title, string message, string defaultValue = "", Func<string, string> validate = null)
            => InputDialog.Show(ActiveWindow, title, message, defaultValue, validate);

        public string PickFolder(string title, string initialFolder)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = title;
                dialog.ShowNewFolderButton = true;
                if (!string.IsNullOrEmpty(initialFolder) && Directory.Exists(initialFolder))
                    dialog.SelectedPath = initialFolder;
                return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        public void ShowConfiguration(ConfigurationViewModel viewModel)
        {
            var window = new ConfigurationWindow { DataContext = viewModel, Owner = ActiveWindow };
            window.ShowDialog();
        }

        public void OpenFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError("Không mở được thư mục", ex.Message);
            }
        }

        public void OpenFile(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", "\"" + path + "\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError("Không mở được file", ex.Message);
            }
        }

        private static Window ActiveWindow
        {
            get
            {
                var app = Application.Current;
                return app?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? app?.MainWindow;
            }
        }
    }
}
