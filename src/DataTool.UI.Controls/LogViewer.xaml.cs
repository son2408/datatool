using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Khung log nền tối, tô màu theo mức, tự cuộn xuống dòng mới nhất.
    /// Mỗi phần tử trong ItemsSource cần có các thuộc tính Timestamp, Level, Message
    /// (Level là enum hoặc chuỗi: Debug, Info, Success, Warning, Error).
    /// Nút "Sao chép" dùng ToString() của từng phần tử.
    /// </summary>
    public partial class LogViewer : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(LogViewer),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(LogViewer), new PropertyMetadata(true));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(LogViewer), new PropertyMetadata("Nhật ký"));

        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(LogViewer), new PropertyMetadata(null, OnHeaderContentChanged));

        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(LogViewer), new PropertyMetadata("Chưa có log."));

        public LogViewer()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>Tự cuộn xuống khi có dòng mới. Mặc định bật.</summary>
        public bool AutoScroll
        {
            get => (bool)GetValue(AutoScrollProperty);
            set => SetValue(AutoScrollProperty, value);
        }

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>Nội dung thêm vào thanh tiêu đề (ví dụ nút "Mở thư mục log").</summary>
        public object HeaderContent
        {
            get => GetValue(HeaderContentProperty);
            set => SetValue(HeaderContentProperty, value);
        }

        public string EmptyText
        {
            get => (string)GetValue(EmptyTextProperty);
            set => SetValue(EmptyTextProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (LogViewer)d;
            if (e.OldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= viewer.OnCollectionChanged;
            if (e.NewValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += viewer.OnCollectionChanged;
            viewer.ScrollToEnd();
        }

        // Nội dung header được hiển thị bên trong control (DataContext = chính LogViewer),
        // nên gắn DataContext của nó về DataContext bên ngoài để binding của người dùng hoạt động.
        private static void OnHeaderContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (LogViewer)d;
            if (e.NewValue is FrameworkElement element
                && element.ReadLocalValue(DataContextProperty) == DependencyProperty.UnsetValue
                && BindingOperations.GetBindingBase(element, DataContextProperty) == null)
            {
                element.SetBinding(DataContextProperty, new Binding(nameof(DataContext)) { Source = viewer });
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            if (!AutoScroll) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                int count = LogList.Items.Count;
                if (count > 0)
                    LogList.ScrollIntoView(LogList.Items[count - 1]);
            }));
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            IEnumerable source = LogList.SelectedItems.Count > 0
                ? LogList.SelectedItems.Cast<object>().OrderBy(i => LogList.Items.IndexOf(i))
                : (IEnumerable)LogList.Items;

            string text = string.Join(Environment.NewLine, source.Cast<object>().Select(i => i?.ToString()));
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                Clipboard.SetText(text);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Clipboard đang bị ứng dụng khác giữ, bỏ qua.
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            if (ItemsSource is IList list && !list.IsReadOnly && !list.IsFixedSize)
                list.Clear();
        }
    }
}
