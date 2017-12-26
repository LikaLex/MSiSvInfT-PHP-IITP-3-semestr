using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MSiSvInfT
{
    public class MainFunction: Function
    {
        #region Fields
        private static Word name = new Word("Главная функция", false, false);
        #endregion

        #region Properties
        public List<int> Lines { get; set; } = new List<int>();
        #endregion

        #region Constructors
        public MainFunction()
        {
            Name = name;
        }
        #endregion

        #region Methods
        public void AddLines(int start, int end)
        {
            for (int i = start; i <= end; i++)
                Lines.Add(i);
        }

        public override void FindVariables()
        {
            foreach (int index in Lines)
            {
                string line = Data.Code.Lines[index];
                if (!line.Contains("\""))
                    FindVariablesInLineWithoutQuotes(index, this);
                else
                    FindVariablesInLineWithQuotes(index, this);
            }
        }

        public override void FindVariablesLines()
        {
            foreach (int lineIndex in Lines)
            {
                string line = Data.Code.Lines[lineIndex];
                if (line.Contains('\"'))
                    FindVariablesLinesInLineWithQuotes(lineIndex);
                else
                    FindVariablesLinesInLineWithoutQuotes(lineIndex);
            }
        }

        public override Grid GetFunctionGrid(string caption = null)
        {
            Grid functionGrid = base.GetFunctionGrid(Name.Text);
            Grid constantsGrid = new Grid();
            constantsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition nameRow = new RowDefinition() { Height = new GridLength(Data.NameRowHeight, GridUnitType.Pixel) };
            constantsGrid.RowDefinitions.Add(nameRow);
            for (int i = 0; i < SecondTask.Constants.Count; i++)
            {
                constantsGrid.RowDefinitions.Add(new RowDefinition());
                bool needsBottomBorder = false;
                if (i == SecondTask.Constants.Count - 1)
                    needsBottomBorder = true;
                Grid constGrid = SecondTask.Constants[i].GetFunctionGrid(needsBottomBorder);
                WPF_Methods.AddToGrid(ref constantsGrid, (constGrid, 0, i + 1));
            }

            TextBox nameTextBox = new TextBox() { TextWrapping = TextWrapping.Wrap, Background = Data.ApplicationBackground, BorderThickness = new Thickness(0), FontSize = Data.CaptionTextFontSize, Text = "Константы", IsReadOnly = true, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            WPF_Methods.AddToGrid(ref constantsGrid, (nameTextBox, 0, 0));

            Grid grid = new Grid();
            DockPanel panel = new DockPanel();
            panel.Children.Add(functionGrid);
            panel.Children[0].SetValue(DockPanel.DockProperty, Dock.Top);
            constantsGrid.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(constantsGrid);
            panel.Children[1].SetValue(DockPanel.DockProperty, Dock.Bottom);
            WPF_Methods.AddToGrid(ref grid, (panel, 0, 0));
            return grid;
        }
        #endregion
    }
}
