using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace MSiSvInfT
{
    public enum Group
    {
        P,
        M,
        C,
        T,
        NotDefined
    }

    public class Variable
    {
        #region Properties
        public Word Name { get; set; }
        public List<int> Lines { get; set; } = new List<int>();
        public List<int> Occurrences { get; set; } = new List<int>();
        public bool IsParameter { get; set; }
        public bool IsPassedByReference { get; set; }
        public Group Group { get; protected set; } = Group.NotDefined;
        public bool CanGroupBe_T { get; protected set; }
        public string GroupClarification { get; protected set; }
        public Function Function { get; set; }
        public int Spen { get; set; } = 0;        
        #endregion

        #region Constructors
        public Variable(Function function, Word name, bool isParameter = false, bool isPassedByReference = false)
        {
            Function = function;
            Name = name;
            IsParameter = isParameter;
            if (IsParameter)
                IsPassedByReference = isPassedByReference;
        }
        #endregion

        #region Methods
        public void CountSpen()
        {
            foreach (int lineIndex in Lines)
            {
                int occurrencesInLine = 0;
                Word[] words = Data.Code[lineIndex]; 
                foreach(Word word in words)
                {
                    if (word.Text == Name.Text)
                    {
                        occurrencesInLine++;
                        Spen++;
                    }
                }
                Occurrences.Add(occurrencesInLine);
            }
            Spen--;
        }

        public void SetGroup(Group group, string clarification, bool canGroupBe_T)
        {
            Group = group;
            GroupClarification = clarification;
            CanGroupBe_T = canGroupBe_T;
        }

        #region Function Grid
        private static void MarkupFunctionGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInFunctionGrid, GridUnitType.Pixel) };
            ColumnDefinition nameColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInFunctionGrid, GridUnitType.Pixel) };
            ColumnDefinition linesColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition secondSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInFunctionGrid, GridUnitType.Pixel) };
            ColumnDefinition groupColumn = new ColumnDefinition() { Width = new GridLength(Data.GroupWidth, GridUnitType.Pixel) };
            ColumnDefinition fourthSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInFunctionGrid, GridUnitType.Pixel) };
            ColumnDefinition groupClarificationColumn = new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInFunctionGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, nameColumn, firstSplitterColumn, linesColumn, secondSplitterColumn, /*infoColumn, thirdSplitterColumn,*/ groupColumn, fourthSplitterColumn, groupClarificationColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInFunctionGrid);
        }

        private static void AddElementsToFunctionGrid(ref Grid grid, Label nameLabel, TextBox linesTextBox, Label groupLabel, TextBox groupClarificationTextBox)
            => WPF_Methods.AddToGrid(ref grid, (nameLabel, 1, 1), (linesTextBox, 3, 1), (groupLabel, 5, 1), (groupClarificationTextBox, 7, 1));

        private static void MakeBordersInFunctionGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInFunctionGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInFunctionGrid, (2, 1), (4, 1), (6, 1));
        }

        public static Grid GetTemplateOfFunctionGrid(string name, string lines, string group, string groupClarification, bool needsBottomBorder, bool isSettingMaxHeight)
        {
            Grid grid = new Grid();
            MarkupFunctionGrid(ref grid, needsBottomBorder);

            TextBox nameTextBox = new TextBox() { Text = name, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };     
            TextBox linesTextBox = new TextBox() { Text = lines, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };
            TextBox groupTextBox = new TextBox() { Text = group, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };
            TextBox groupClarificationTextBox = new TextBox() { TextWrapping = TextWrapping.Wrap, Text = groupClarification, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };

            WPF_Methods.AddToGrid(ref grid, (nameTextBox, 1, 1), (linesTextBox, 3, 1), (groupTextBox, 5, 1), (groupClarificationTextBox, 7, 1));
            MakeBordersInFunctionGrid(ref grid, needsBottomBorder);
            if (isSettingMaxHeight)
                grid.MaxHeight = Data.TemplateGridMaxHeight;
            return grid;
        }
        public static Grid GetTemplateOfFunctionGrid(bool needsBottomBorder) => GetTemplateOfFunctionGrid("Имя", "Строки, в которых присутствует", "Группа", "Пояснение к выбору группы", false, true);

        public static string GetGroupName(Group group)
        {
            switch (group)
            {
                case Group.P:
                    return "P";
                case Group.M:
                    return "M";
                case Group.C:
                    return "C";
                case Group.T:
                    return "T";
                default:
                    return "-";
            }
        }
        
        private string GetLines()
        {
            for (int i = 0; i < Lines.Count; i++)
                Lines[i]++;
            string lines = string.Join(", ", Lines);
            for (int i = 0; i < Lines.Count; i++)
                Lines[i]--;
            return lines;
        }

        public Grid GetFunctionGrid(bool needsBottomBorder)
            => GetTemplateOfFunctionGrid(Name.Text, GetLines(), GetGroupName(Group), GroupClarification, needsBottomBorder, false);
        #endregion

        #region Group Grid
        private static void MarkupGroupGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInGroupGrid, GridUnitType.Pixel) };
            ColumnDefinition numberColumn = new ColumnDefinition() { Width = new GridLength(Data.NumberWidth, GridUnitType.Pixel) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInGroupGrid, GridUnitType.Pixel) };
            ColumnDefinition nameColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition secondSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInGroupGrid, GridUnitType.Pixel) };
            ColumnDefinition functionColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition thirdSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInGroupGrid, GridUnitType.Pixel) };
            ColumnDefinition groupColumn = new ColumnDefinition() { Width = new GridLength(2 * Data.GroupWidth, GridUnitType.Pixel) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInGroupGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, numberColumn, firstSplitterColumn, nameColumn, secondSplitterColumn, functionColumn, thirdSplitterColumn, groupColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInGroupGrid);
        }

        private static void MakeBordersInGroupGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInGroupGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInGroupGrid, (2, 1), (4, 1), (6, 1));
        }      
       
        public static Grid GetTemplateOfGroupGrid(string number, string name, string function, string group, bool needsBottomBorder, double fontSize)
        {
            Grid grid = new Grid();
            MarkupGroupGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = number, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox nameTextBox = new TextBox() { Text = name, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox functionTextBox = new TextBox() { Text = function, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox groupTextBox = new TextBox() { Text = group, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (nameTextBox, 3, 1), (functionTextBox, 5, 1), (groupTextBox, 7, 1));
            MakeBordersInGroupGrid(ref grid, needsBottomBorder);
            return grid;
        }

        private string MakeInfo()
        {
            if (!IsParameter)
                return "Создаётся внутри функции";
            else
            {
                if (IsPassedByReference)
                    return "Параметр, передаваемый по ссылке";
                else
                    return "Параметр функции";
            }
        }

        public Grid GetGroupGrid(bool needsBottomBorder, int number, double fontSize)
        {
            Grid grid = new Grid();
            MarkupGroupGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = number.ToString(), Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, BorderThickness = Data.NullThickness, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox nameTextBox = new TextBox() { Text = Name.Text, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, BorderThickness = Data.NullThickness, FontSize = fontSize, Cursor = Cursors.Hand, IsReadOnly = true };
            string function;
            try
            {
                function = Function.Name.Text;
            }
            catch (Exception)
            {
                function = "-";
            }
            TextBox functionTextBox = new TextBox() { Text = function, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, Cursor = Cursors.Arrow, IsReadOnly = true };
            TextBox groupTextBox = new TextBox() { Text = GetGroupName(Group), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = fontSize, Cursor = Cursors.Hand, IsReadOnly = true };

            WPF_Methods.SetToolTip(ref nameTextBox, WPF_Methods.CreateToolTip("Дополнительная информация", MakeInfo() + "\n" + "Строки, в которых используется:\n" + GetLines()));
            WPF_Methods.SetToolTip(ref groupTextBox, WPF_Methods.CreateToolTip("Пояснение к выбору группы", GroupClarification));

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (nameTextBox, 3, 1), (functionTextBox, 5, 1), (groupTextBox, 7, 1));
            MakeBordersInGroupGrid(ref grid, needsBottomBorder);
            return grid;
        }

        public static Grid GetTemplateOfGroupGrid(bool needsBottomBorder, double fontSize) => GetTemplateOfGroupGrid("N", "Переменная", "Функция", "Группа", needsBottomBorder, fontSize);
        #endregion

        #region Spen Grid
        private static void MarkupSpenGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInSpenGrid, GridUnitType.Pixel) };
            ColumnDefinition numberColumn = new ColumnDefinition() { Width = new GridLength(Data.NumberWidth, GridUnitType.Pixel) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInSpenGrid, GridUnitType.Pixel) };
            ColumnDefinition nameColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition secondSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInSpenGrid, GridUnitType.Pixel) };
            ColumnDefinition functionColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition thirdSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInSpenGrid, GridUnitType.Pixel) };
            ColumnDefinition spenColumn = new ColumnDefinition() { Width = new GridLength(Data.SpenWidth, GridUnitType.Pixel) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInSpenGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, numberColumn, firstSplitterColumn, nameColumn, secondSplitterColumn, functionColumn, thirdSplitterColumn, spenColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInSpenGrid);
        }

        private static void MakeBordersInSpenGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInSpenGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInSpenGrid, (2, 1), (4, 1), (6, 1));
        }

        public static Grid GetTemplateOfSpenGrid(string number, string name, string function, string spen, bool needsBottomBorder, double fontSize)
        {
            Grid grid = new Grid();
            MarkupSpenGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = number, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox nameTextBox = new TextBox() { Text = name, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox functionTextBox = new TextBox() { Text = function, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox spenTextBox = new TextBox() { Text = spen, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (nameTextBox, 3, 1), (functionTextBox, 5, 1), (spenTextBox, 7, 1));
            MakeBordersInSpenGrid(ref grid, needsBottomBorder);
            return grid;
        }

        private string GetLinesWithOccurrences()
        {
            for (int i = 0; i < Lines.Count; i++)
                Lines[i]++;
            string result = "";
            for (int i = 0; i < Lines.Count; i++)
            {
                result += Lines[i].ToString() + " (" + Occurrences[i].ToString() + ")";
                if (i == Lines.Count - 1)
                    result += ".";
                else
                    result += ",\n";
            }
            for (int i = 0; i < Lines.Count; i++)
                Lines[i]--;
            return result;
        }

        public Grid GetSpenGrid(bool needsBottomBorder, int number, double fontSize)
        {
            Grid grid = new Grid();
            MarkupSpenGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = number.ToString(), Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, BorderThickness = Data.NullThickness, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox nameTextBox = new TextBox() { Text = Name.Text, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, BorderThickness = Data.NullThickness, FontSize = fontSize, Cursor = Cursors.Arrow, IsReadOnly = true };
            string function;
            try
            {
                function = Function.Name.Text;
            }
            catch (Exception)
            {
                function = "-";
            }
            TextBox functionTextBox = new TextBox() { Text = function, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, Cursor = Cursors.Hand, IsReadOnly = true };
            TextBox spenTextBox = new TextBox() { Text = Spen.ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = fontSize, Cursor = Cursors.Hand, IsReadOnly = true };

            int spen;
            string caption;
            try
            {
                spen = Function.Spen;
                caption = "Спен функции";
            }
            catch (Exception)
            {
                spen = SecondTask.ConstantsSpen;
                caption = "Общий спен констант";
            }
            WPF_Methods.SetToolTip(ref spenTextBox, WPF_Methods.CreateToolTip("Номера строк и число вхождений переменной в строке", GetLinesWithOccurrences()));
            WPF_Methods.SetToolTip(ref functionTextBox, WPF_Methods.CreateToolTip(caption, spen.ToString()));

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (nameTextBox, 3, 1), (functionTextBox, 5, 1), (spenTextBox, 7, 1));
            MakeBordersInSpenGrid(ref grid, needsBottomBorder);
            return grid;
        }

        public static Grid GetTemplateOfSpenGrid(bool needsBottomBorder, double fontSize) 
            => GetTemplateOfSpenGrid("N", "Идентификатор", "Функция", "Спен", needsBottomBorder, fontSize);
        #endregion

        public override bool Equals(object obj) => Name.Equals(obj);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name.ToString();
        #endregion

        #region Operators
        public static bool operator ==(Variable first, Variable second) => first.Name == second.Name;
        public static bool operator !=(Variable first, Variable second) => first.Name != second.Name;
        #endregion
    }
}
