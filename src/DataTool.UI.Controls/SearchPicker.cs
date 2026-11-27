using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Ô chọn một mục có tìm kiếm: bấm vào mở danh sách thả xuống, gõ để lọc,
    /// dùng phím ↑ ↓ Enter Esc. Dùng được cho mọi danh sách (tỉnh, tác vụ...),
    /// chỉ cần ItemsSource, SelectedItem và DisplayMemberPath.
    /// </summary>
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartSearch, Type = typeof(TextBox))]
    [TemplatePart(Name = PartList, Type = typeof(ListBox))]
    public class SearchPicker : Control
    {
        private const string PartPopup = "PART_Popup";
        private const string PartSearch = "PART_Search";
        private const string PartList = "PART_List";

        private Popup _popup;
        private TextBox _search;
        private ListBox _list;
        private ListCollectionView _view;

        static SearchPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchPicker), new FrameworkPropertyMetadata(typeof(SearchPicker)));
        }

        #region Dependency properties

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchPicker),
                new PropertyMetadata(null, (d, e) => ((SearchPicker)d).RebuildView()));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SearchPicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((SearchPicker)d).UpdateSelectedText()));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(SearchPicker),
                new PropertyMetadata(null, (d, e) => ((SearchPicker)d).UpdateSelectedText()));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(SearchPicker), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), typeof(SearchPicker), new PropertyMetadata(PackIconKind.MapMarkerOutline));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchPicker), new PropertyMetadata("Chưa chọn"));

        public static readonly DependencyProperty SearchHintProperty =
            DependencyProperty.Register(nameof(SearchHint), typeof(string), typeof(SearchPicker), new PropertyMetadata("Tìm kiếm..."));

        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(SearchPicker), new PropertyMetadata("Không tìm thấy"));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchPicker),
                new PropertyMetadata(string.Empty, (d, e) => ((SearchPicker)d).ApplyFilter()));

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(SearchPicker),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((SearchPicker)d).OnDropDownChanged((bool)e.NewValue)));

        public static readonly DependencyProperty DropDownWidthProperty =
            DependencyProperty.Register(nameof(DropDownWidth), typeof(double), typeof(SearchPicker), new PropertyMetadata(280d));

        private static readonly DependencyPropertyKey SelectedTextPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(SelectedText), typeof(string), typeof(SearchPicker), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedTextProperty = SelectedTextPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey FilteredViewPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(FilteredView), typeof(ICollectionView), typeof(SearchPicker), new PropertyMetadata(null));
        public static readonly DependencyProperty FilteredViewProperty = FilteredViewPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey HasResultsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasResults), typeof(bool), typeof(SearchPicker), new PropertyMetadata(true));
        public static readonly DependencyProperty HasResultsProperty = HasResultsPropertyKey.DependencyProperty;

        public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
        public string DisplayMemberPath { get => (string)GetValue(DisplayMemberPathProperty); set => SetValue(DisplayMemberPathProperty, value); }
        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
        public PackIconKind Icon { get => (PackIconKind)GetValue(IconProperty); set => SetValue(IconProperty, value); }
        public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
        public string SearchHint { get => (string)GetValue(SearchHintProperty); set => SetValue(SearchHintProperty, value); }
        public string EmptyText { get => (string)GetValue(EmptyTextProperty); set => SetValue(EmptyTextProperty, value); }
        public string SearchText { get => (string)GetValue(SearchTextProperty); set => SetValue(SearchTextProperty, value); }
        public bool IsDropDownOpen { get => (bool)GetValue(IsDropDownOpenProperty); set => SetValue(IsDropDownOpenProperty, value); }
        public double DropDownWidth { get => (double)GetValue(DropDownWidthProperty); set => SetValue(DropDownWidthProperty, value); }

        /// <summary>Chữ hiển thị của mục đang chọn (theo DisplayMemberPath).</summary>
        public string SelectedText => (string)GetValue(SelectedTextProperty);

        /// <summary>Danh sách đã lọc theo SearchText, hiển thị trong ô thả xuống.</summary>
        public ICollectionView FilteredView => (ICollectionView)GetValue(FilteredViewProperty);

        public bool HasResults => (bool)GetValue(HasResultsProperty);

        #endregion

        public override void OnApplyTemplate()
        {
            if (_search != null) _search.PreviewKeyDown -= OnSearchKeyDown;
            if (_list != null)
            {
                _list.PreviewMouseLeftButtonUp -= OnListClick;
                _list.PreviewKeyDown -= OnSearchKeyDown;
            }
            if (_popup != null) _popup.Opened -= OnPopupOpened;

            base.OnApplyTemplate();

            _popup = GetTemplateChild(PartPopup) as Popup;
            _search = GetTemplateChild(PartSearch) as TextBox;
            _list = GetTemplateChild(PartList) as ListBox;

            if (_search != null) _search.PreviewKeyDown += OnSearchKeyDown;
            if (_list != null)
            {
                _list.PreviewMouseLeftButtonUp += OnListClick;
                _list.PreviewKeyDown += OnSearchKeyDown;
            }
            if (_popup != null) _popup.Opened += OnPopupOpened;
        }

        private void RebuildView()
        {
            var source = ItemsSource;
            if (source == null)
            {
                _view = null;
            }
            else
            {
                var list = source as IList ?? source.Cast<object>().ToList();
                _view = new ListCollectionView(list) { Filter = Matches };
            }
            SetValue(FilteredViewPropertyKey, _view);
            UpdateHasResults();
            UpdateSelectedText();
        }

        private bool Matches(object item)
        {
            var text = SearchText;
            if (string.IsNullOrWhiteSpace(text)) return true;
            var display = GetDisplayText(item);
            return display != null && display.IndexOf(text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyFilter()
        {
            if (_view == null) return;
            _view.Refresh();
            UpdateHasResults();
            if (_list != null && !_view.IsEmpty && (_list.SelectedItem == null || !_view.Contains(_list.SelectedItem)))
            {
                _list.SelectedIndex = 0;
            }
        }

        private void UpdateHasResults()
        {
            SetValue(HasResultsPropertyKey, _view != null && !_view.IsEmpty);
        }

        private void UpdateSelectedText()
        {
            SetValue(SelectedTextPropertyKey, SelectedItem == null ? null : GetDisplayText(SelectedItem));
        }

        private string GetDisplayText(object item)
        {
            if (item == null) return null;
            var path = DisplayMemberPath;
            if (string.IsNullOrEmpty(path)) return item.ToString();
            var prop = TypeDescriptor.GetProperties(item).Find(path, false);
            return prop?.GetValue(item)?.ToString();
        }

        private void OnDropDownChanged(bool open)
        {
            if (open)
            {
                SearchText = string.Empty;
                UpdateSelectedText();
                if (_list != null) _list.SelectedItem = SelectedItem;
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (_search == null) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _search.Focus();
                Keyboard.Focus(_search);
                if (_list?.SelectedItem != null) _list.ScrollIntoView(_list.SelectedItem);
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void OnListClick(object sender, MouseButtonEventArgs e)
        {
            var container = ItemsControl.ContainerFromElement(_list, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (container != null) Commit(container.DataContext);
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (_list == null) return;
            switch (e.Key)
            {
                case Key.Down:
                    if (_list.SelectedIndex < _list.Items.Count - 1) _list.SelectedIndex++;
                    _list.ScrollIntoView(_list.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (_list.SelectedIndex > 0) _list.SelectedIndex--;
                    if (_list.SelectedItem != null) _list.ScrollIntoView(_list.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (_list.SelectedItem != null) Commit(_list.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    IsDropDownOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void Commit(object item)
        {
            SelectedItem = item;
            IsDropDownOpen = false;
        }
    }
}
