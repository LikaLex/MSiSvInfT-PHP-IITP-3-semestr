using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;

namespace MSiSvInfT
{
    class WPF_Methods
    {
        #region Methods
        public static void AddToRowsDefinitionsOfGrid(ref Grid grid, params RowDefinition[] rowsDefinitions)
        {
            foreach (RowDefinition rowDefinition in rowsDefinitions)
                grid.RowDefinitions.Add(rowDefinition);
        }

        public static void AddToColumnsDefinitionsOfGrid(ref Grid grid, params ColumnDefinition[] columnsDefinitions)
        {
            foreach (ColumnDefinition columnDefinition in columnsDefinitions)
                grid.ColumnDefinitions.Add(columnDefinition);
        }

        public static void AddToGrid(ref Grid grid, params (UIElement, int column, int row)[] items)
        {
            foreach((UIElement element, int column, int row) item in items)
                AddToGrid(ref grid, item.column, item.row, item.element);
        }

        private static void AddToGrid(ref Grid grid, int column, int row, UIElement element)
        {
            grid.Children.Add(element);
            element.SetValue(Grid.RowProperty, row);
            element.SetValue(Grid.ColumnProperty, column);
        }

        public static void MakeGridSplitters(ref Grid grid, double width, params (int column, int row)[] cells)
        {
            foreach ((int column, int row) cell in cells)
                MakeVerticalGridSplitter(ref grid, cell.column, cell.row, width);
        }

        public static void SetCellSpans(UIElement element, int columnSpan, int rowSpan)
        {
            if (columnSpan > 0)
                element.SetValue(Grid.ColumnSpanProperty, columnSpan);
            if (rowSpan > 0)
                element.SetValue(Grid.RowSpanProperty, rowSpan);
        }

        private static void MakeBorderRectangle(ref Grid grid, int column, int row, int columnSpan, int rowSpan, double width)
        {
            Rectangle rectangle = new Rectangle() { Stroke = Brushes.Black, StrokeThickness = width };
            AddToGrid(ref grid, column, row, rectangle);
            SetCellSpans(rectangle, columnSpan, rowSpan);
        }

        private static void MakeVerticalGridSplitter(ref Grid grid, int column, int row, double width)
        {
            GridSplitter splitter = new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch, Background = Brushes.Black, Width = width };
            AddToGrid(ref grid, column, row, splitter);
        }

        public static void SetLabelContentWithUnderlining(ref Label label, string content)
        {
            if (content.Contains('_'))
            {
                content = content.Replace("_", "__");
                label.Content = content;
            }
            else
                label.Content = content;
        }

        public static void SetPropertyToControls(DependencyProperty property, object value, params Control[] controls)
        {
            foreach (Control control in controls)
                control.SetValue(property, value);
        }

        private static ToolTip CreateToolTip(string label, Brush background)
        {
            ToolTip ToolTip = new ToolTip() { Background = background };
            StackPanel ToolTipStackPanel = new StackPanel();
            TextBlock ToolTipLabel = new TextBlock() { Text = label, Background = background, FontSize = Data.CaptionTextFontSize, HorizontalAlignment = HorizontalAlignment.Center };           
            ToolTipStackPanel.Children.Add(ToolTipLabel);
            ToolTip.Content = ToolTipStackPanel;
            ToolTip.Visibility = Visibility.Collapsed;
            return ToolTip;
        }

        public static ToolTip CreateToolTip(string caption, string text)
        {
            ToolTip ToolTip = new ToolTip() { Background = Data.ToolTipTextBackground };
            StackPanel ToolTipStackPanel = new StackPanel();
            TextBox ToolTipCaption = new TextBox() { Text = caption, Background = Data.ToolTipCaptionBackground, FontSize = Data.CaptionTextFontSize, BorderThickness = Data.NullThickness, IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center };
            TextBox ToolTipText = new TextBox() { Text = text, Background = Data.ToolTipTextBackground, FontSize = Data.UsualTextFontSize, BorderThickness = Data.NullThickness, IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
            ToolTipStackPanel.Children.Add(ToolTipCaption);
            ToolTipStackPanel.Children.Add(ToolTipText);
            ToolTip.Content = ToolTipStackPanel;
            ToolTip.Visibility = Visibility.Collapsed;
            return ToolTip;
        }

        public static ToolTip CreateToolTipWithGrid(string caption, Grid grid)
        {
            ToolTip ToolTip = new ToolTip() { Background = Data.ToolTipTextBackground };
            StackPanel ToolTipStackPanel = new StackPanel();
            TextBox ToolTipCaption = new TextBox() { Text = caption, Background = Data.ToolTipCaptionBackground, FontSize = Data.CaptionTextFontSize, BorderThickness = Data.NullThickness, IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center };
            ToolTipStackPanel.Children.Add(ToolTipCaption);
            ToolTipStackPanel.Children.Add(grid);
            ToolTip.Content = ToolTipStackPanel;
            ToolTip.Visibility = Visibility.Collapsed;
            return ToolTip;
        }

        public static void MakeRowWithBordersInGrid(ref Grid grid, bool needsBottomBorder, double borderWidth)
        {
            RowDefinition topBorder = new RowDefinition() { Height = new GridLength(borderWidth, GridUnitType.Pixel) };
            AddToRowsDefinitionsOfGrid(ref grid, topBorder, new RowDefinition());
            if (needsBottomBorder)
            {
                RowDefinition bottomBorder = new RowDefinition() { Height = new GridLength(borderWidth, GridUnitType.Pixel) };
                grid.RowDefinitions.Add(bottomBorder);
            }
        }

        public static void MakeBorderRectangles(ref Grid grid, bool needsBottomBorder, double borderWidth)
        {
            MakeBorderRectangle(ref grid, 0, 0, grid.ColumnDefinitions.Count, 1, borderWidth);
            MakeBorderRectangle(ref grid, 0, 1, 1, 1, borderWidth);
            MakeBorderRectangle(ref grid, grid.ColumnDefinitions.Count - 1, 1, 1, 1, borderWidth);
            if (needsBottomBorder)
                MakeBorderRectangle(ref grid, 0, 2, grid.ColumnDefinitions.Count, 1, borderWidth);
        }

        public static void AddTextToTextBlock(ref TextBlock TB, params (string Text, FontVariants Variant)[] texts)
        {
            TB.FontFamily = new FontFamily("Palatino Linotype");
            foreach ((string Text, FontVariants Variant) text in texts)
            {
                Run part = new Run(text.Text);
                part.Typography.Variants = text.Variant;
                TB.Inlines.Add(part);
            }
        }

        public static void SetToolTip<T>(ref T control, ToolTip toolTip) where T : FrameworkElement
        {
            control.ToolTip = toolTip;
            control.PreviewMouseUp += ToolTipMouseUpHandler;
            control.MouseLeave += ToolTipMouseLeaveHandler;
            control.Cursor = Cursors.Hand;
        }

        public static void SetToolTip<T>(ref T control, string label, Brush background) where T : FrameworkElement
            => SetToolTip(ref control, CreateToolTip(label, background));
        #endregion

        #region Events Handlers
        private static void ToolTipMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            ToolTip ToolTip;
            if (sender.GetType() == typeof(Span))
            {
                Span span = (Span)sender;
                ToolTip = (ToolTip)span.ToolTip;
            }
            else
            {
                FrameworkElement element = (FrameworkElement)sender;
                ToolTip = (ToolTip)element.ToolTip;
            }
            if (ToolTip.Visibility == Visibility.Visible)
            {
                ToolTip.Visibility = Visibility.Collapsed;
                ToolTip.IsOpen = false;
            }
            else
            {
                ToolTip.Visibility = Visibility.Visible;
                ToolTip.IsOpen = true; ;
            }
        }

        private static void ToolTipMouseLeaveHandler(object sender, EventArgs e)
        {
            ToolTip ToolTip;
            if (sender.GetType() == typeof(Span))
            {
                Span span = (Span)sender;
                ToolTip = (ToolTip)span.ToolTip;
            }
            else
            {
                FrameworkElement element = (FrameworkElement)sender;
                ToolTip = (ToolTip)element.ToolTip;
            }
            if (ToolTip.Visibility == Visibility.Visible)
            {
                ToolTip.Visibility = Visibility.Collapsed;
                ToolTip.IsOpen = false;
            }
        }       
        #endregion
    }
}
