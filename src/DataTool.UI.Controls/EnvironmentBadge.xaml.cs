using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataTool.UI.Controls.Helpers;

namespace DataTool.UI.Controls
{
    /// <summary>
    /// Nhãn tròn hiển thị tên môi trường với màu theo mức (Dev xanh, Test cam, Production đỏ).
    /// </summary>
    public partial class EnvironmentBadge : UserControl
    {
        public static readonly DependencyProperty EnvironmentNameProperty =
            DependencyProperty.Register(nameof(EnvironmentName), typeof(string), typeof(EnvironmentBadge),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(nameof(Level), typeof(EnvironmentLevel), typeof(EnvironmentBadge),
                new PropertyMetadata(EnvironmentLevel.Other, (d, e) => ((EnvironmentBadge)d).UpdateBrush()));

        private static readonly DependencyPropertyKey BadgeBrushPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(BadgeBrush), typeof(Brush), typeof(EnvironmentBadge),
                new PropertyMetadata(null));

        public static readonly DependencyProperty BadgeBrushProperty = BadgeBrushPropertyKey.DependencyProperty;

        public EnvironmentBadge()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
            UpdateBrush();
            Loaded += (s, e) => UpdateBrush();
        }

        public string EnvironmentName
        {
            get => (string)GetValue(EnvironmentNameProperty);
            set => SetValue(EnvironmentNameProperty, value);
        }

        public EnvironmentLevel Level
        {
            get => (EnvironmentLevel)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        public Brush BadgeBrush => (Brush)GetValue(BadgeBrushProperty);

        private void UpdateBrush() => SetValue(BadgeBrushPropertyKey, EnvironmentBrushes.Get(Level));
    }
}
