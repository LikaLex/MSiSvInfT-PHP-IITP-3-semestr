using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MSiSvInfT
{
    using Brackets = BracketsMethods;

    public class CodeBlock: IComparable<CodeBlock>
    {
        #region Properties
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string Operator { get; protected set; }
        public int NestingLevel { get; set; }
        public int? FramingBlockStartLine { get; set; }
        public bool WasCounted { get; set; } = false;
        public int Size { get { return EndLine - StartLine; } }
        #endregion

        #region Constructors
        public CodeBlock(int startLine, string @operator, bool isSearchingForEnd)
        {
            Operator = @operator;
            StartLine = startLine;
            if (isSearchingForEnd)
                EndLine = Brackets.GetCloseCurveBracketIndex(Data.Code.Lines, StartLine + 1);
            if (@operator == Data.DO.Text || @operator == Data.DO.Text + "{")
                EndLine++;
        }
        #endregion

        #region Methods
        public int CompareTo(CodeBlock block)
        {
            if (StartLine < block.StartLine)
                return -1;
            else if (StartLine == block.StartLine)
                return 0;
            else
                return 1;
        }

        public bool IsLineInBlock(int lineIndex)
        {
            if (StartLine <= lineIndex && EndLine >= lineIndex)
                return true;
            return false;
        }

        public bool IsLineCorrectlyIn_SWITCH_Block(int lineIndex)
        {
            if (StartLine <= lineIndex && EndLine >= lineIndex)
            {
                foreach (CodeBlock block in FirstTask.Enumerate_SWITCH_CodeBlocks())
                {
                    if (block == this)
                        continue;
                    if (block.IsLineInBlock(lineIndex) && IsLineInBlock(block.StartLine))
                        return false;
                }
                return true;
            }
            return false;
        }

        #region Grid
        private static void MarkupGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInCodeBlocksGrid, GridUnitType.Pixel) };
            ColumnDefinition numberColumn = new ColumnDefinition() { Width = new GridLength(Data.NumberWidth, GridUnitType.Pixel) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInCodeBlocksGrid, GridUnitType.Pixel) };
            ColumnDefinition operatorColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition secondSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInCodeBlocksGrid, GridUnitType.Pixel) };
            ColumnDefinition startAndEndLinesColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition thirdSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInCodeBlocksGrid, GridUnitType.Pixel) };
            ColumnDefinition nestingLevelColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInCodeBlocksGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, numberColumn, firstSplitterColumn, operatorColumn, secondSplitterColumn, startAndEndLinesColumn, thirdSplitterColumn, nestingLevelColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInCodeBlocksGrid);
        }

        private static void MakeBordersInGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInCodeBlocksGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInCodeBlocksGrid, (2, 1), (4, 1), (6, 1));
        }

        public static Grid GetTemplateOfGrid(string number, string @operator, string lines, string nestingLevel, bool needsBottomBorder, double fontSize)
        {
            Grid grid = new Grid();
            MarkupGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = number, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, VerticalAlignment = VerticalAlignment.Center };
            TextBox operatorTextBox = new TextBox() { Text = @operator, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBox linesTextBox = new TextBox() { Text = lines, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBox nestingLevelTextBox = new TextBox() { Text = nestingLevel, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (operatorTextBox, 3, 1), (linesTextBox, 5, 1), (nestingLevelTextBox, 7, 1));
            MakeBordersInGrid(ref grid, needsBottomBorder);
            return grid;
        }

        public Grid GetGrid(int number, bool needsBottomBorder, double fontSize, Brush background)
        {
            Grid grid = new Grid();
            MarkupGrid(ref grid, needsBottomBorder);

            string lines;
            if (Operator == Data.IF.Text)
            {
                List<CodeBlock> list = FirstTask.Get_IF_ConnectedBlocks(this);
                if (list.Count > 0)
                {
                    CodeBlock last = list.Last();
                    lines = (StartLine + 1).ToString() + " ... " + (last.EndLine + 1).ToString();
                }
                else
                    lines = (StartLine + 1).ToString() + " ... " + (EndLine + 1).ToString();
            }
            else
                lines = (StartLine + 1).ToString() + " ... " + (EndLine + 1).ToString();
            string @operator = Operator;
            if (Operator == Data.DO.Text)
                @operator += " " + Data.WHILE.Text;
            if (Operator == Data.QUESTION_MARK)
                @operator = "Тернарный";

            TextBox numberTextBox = new TextBox() { Text = number.ToString(), Background = background, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow };
            TextBox operatorTextBox = new TextBox() { Text = @operator, Background = background, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBox linesTextBox = new TextBox() { Text = lines, Background = background, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBox nestingLevelTextBox = new TextBox() { Text = NestingLevel.ToString(), Background = background, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, IsReadOnly = true, Cursor = Cursors.Arrow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            if (Operator == Data.SWITCH.Text)
            {
                string caption = "Уровни вложенности ветвей оператора";
                List<int> indexes = FirstTask.CASE_And_DEFAULT_LinesIndexesOf_SWITCH_Block(this);
                Grid toolTipGrid = new Grid();              
                toolTipGrid.ColumnDefinitions.Add(new ColumnDefinition());
                int toolTipGridRowCounter = 0;
                foreach (int index in indexes)
                {
                    CodeBlock block = FirstTask.GetBlockByStartLine(index);
                    toolTipGrid.RowDefinitions.Add(new RowDefinition());
                    if (index == indexes.Last())
                        WPF_Methods.AddToGrid(ref toolTipGrid, (block.GetGrid(toolTipGridRowCounter + 1, true, Data.UsualTextFontSize, Data.ToolTipTextBackground), 0, toolTipGridRowCounter++));
                    else
                        WPF_Methods.AddToGrid(ref toolTipGrid, (block.GetGrid(toolTipGridRowCounter + 1, false, Data.UsualTextFontSize, Data.ToolTipTextBackground), 0, toolTipGridRowCounter++));
                }
                WPF_Methods.SetToolTip(ref operatorTextBox, WPF_Methods.CreateToolTipWithGrid(caption, toolTipGrid));
            }

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (operatorTextBox, 3, 1), (linesTextBox, 5, 1), (nestingLevelTextBox, 7, 1));
            MakeBordersInGrid(ref grid, needsBottomBorder);
            return grid;
        }

        public static Grid GetTemplateOfGrid(bool needsBottomBorder, double fontSize)
            => GetTemplateOfGrid("N", "Оператор", "Строки", "Уровень вложенности", needsBottomBorder, fontSize);
        #endregion

        public override string ToString() => Operator + " " + (StartLine + 1).ToString() + " ... " + (EndLine + 1).ToString() + "; (" + NestingLevel.ToString() + ")";
        #endregion
    }
}
