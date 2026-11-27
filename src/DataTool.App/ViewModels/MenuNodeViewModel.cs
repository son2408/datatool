using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DataTool.App.Models;
using MaterialDesignThemes.Wpf;

namespace DataTool.App.ViewModels
{
    /// <summary>Một nút trên cây menu: nhóm (có con) hoặc tác vụ (lá).</summary>
    public class MenuNodeViewModel : ObservableObject
    {
        private bool _isExpanded = true;
        private bool _isVisible = true;
        private TaskRunViewModel _runViewModel;
        private readonly Func<TaskDescriptor, TaskRunViewModel> _runFactory;

        public MenuNodeViewModel(string title, TaskDescriptor task, Func<TaskDescriptor, TaskRunViewModel> runFactory)
        {
            Title = title;
            Task = task;
            _runFactory = runFactory;
        }

        public string Title { get; }
        public TaskDescriptor Task { get; }
        public ObservableCollection<MenuNodeViewModel> Children { get; } = new ObservableCollection<MenuNodeViewModel>();

        public bool IsTask => Task != null;
        public PackIconKind Icon => Task?.Icon ?? PackIconKind.FolderOutline;

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            private set => SetProperty(ref _isVisible, value);
        }

        /// <summary>ViewModel chạy tác vụ, tạo một lần để giữ log và trạng thái khi chuyển qua lại.</summary>
        public TaskRunViewModel RunViewModel => Task == null ? null : _runViewModel ?? (_runViewModel = _runFactory(Task));

        /// <summary>Lọc theo từ khoá; trả về true nếu nút này hoặc con của nó khớp.</summary>
        public bool ApplyFilter(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                foreach (var child in Children) child.ApplyFilter(null);
                IsVisible = true;
                return true;
            }

            bool selfMatch = Title.IndexOf(keyword.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0;
            bool anyChild = false;
            foreach (var child in Children)
                anyChild |= child.ApplyFilter(selfMatch ? null : keyword);

            IsVisible = selfMatch || anyChild;
            if (anyChild) IsExpanded = true;
            return IsVisible;
        }

        /// <summary>Dựng cây menu từ đường dẫn "Nhóm/Nhóm con/Tác vụ" của từng tác vụ.</summary>
        public static IList<MenuNodeViewModel> BuildTree(IEnumerable<TaskDescriptor> tasks, Func<TaskDescriptor, TaskRunViewModel> runFactory)
        {
            var roots = new List<MenuNodeViewModel>();

            foreach (var task in tasks.OrderBy(t => t.MenuPath, StringComparer.CurrentCulture))
            {
                var parts = task.MenuPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                IList<MenuNodeViewModel> level = roots;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    var group = level.FirstOrDefault(n => !n.IsTask && n.Title == parts[i]);
                    if (group == null)
                    {
                        group = new MenuNodeViewModel(parts[i], null, runFactory);
                        level.Add(group);
                    }
                    level = group.Children;
                }

                level.Add(new MenuNodeViewModel(parts[parts.Length - 1], task, runFactory));
            }

            return roots;
        }
    }
}
