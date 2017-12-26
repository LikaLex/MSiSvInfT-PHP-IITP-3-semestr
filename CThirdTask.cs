using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MSiSvInfT
{
    using Split = SplitMethods;

    public static class ThirdTask
    {
        #region Properties
        public static List<Operator> Operators { get; set; }
        public static List<List<Operator>> OperatorsInLines { get; set; }
        public static List<Operand> Operands { get; set; }
        public static List<List<Operand>> OperandsInLines { get; set; }

        public static int AllOperatorsAmount { get; private set; }
        public static int UniqueOperatorsAmount { get; private set; }
        public static int AllOperandsAmount { get; private set; }
        public static int UniqueOperandsAmount { get; private set; }

        public static int ProgramsDictionary { get; private set; }
        public static int ProgramsLength { get; private set; }
        public static double ProgramsVolume { get; private set; }
        #endregion

        #region Methods

        #region Find Operators
        private static void FindOperators()
        {
            Operators = new List<Operator>();
            for (int i = 0; i < Data.Code.Lines.Length; i++)
            {
                if (Data.Code.Lines[i].Contains('\"'))
                {
                    (Word[] Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(i);
                    int wordOffset = 0;
                    foreach ((Word[] Part, bool IsInQuotes) part in parts)
                    {
                        if (part.IsInQuotes)
                            SearchForOperatorsInWordsInQuotes(part.Part, i, wordOffset);
                        else
                            SearchForOperatorsInWords(part.Part, i, wordOffset);
                        wordOffset += part.Part.Length + 1;
                    }
                }
                else
                {
                    Word[] words = Data.Code[i];
                    SearchForOperatorsInWords(words, i);                 
                }
            }
            Operators.Sort();
        }

        private static void SearchForOperatorsInWords(Word[] words, int lineIndex, int wordOffset = 0)
        {
            for (int j = 0; j < words.Length; j++)
            {
                if (Data.Operators.Contains(words[j].Text))
                    AddOperator(words[j], lineIndex, wordOffset + j);
                else if (words[j].Text == "&")
                {
                    if (!Word.Contains(words, Data.FUNCTION))
                        AddOperator(words[j], lineIndex, wordOffset + j);
                }
                else if (words[j].Text == "do")
                    AddOperator(words[j], lineIndex, wordOffset + j);
                else if (words[j].Text == "while")
                {
                    if (!Word.Contains(words, Data.SEMICOLON))
                        AddOperator(words[j], lineIndex, wordOffset + j);
                }
                else if (words[j].Text == "(")
                {
                    if (!IsBracketForFunctionOrConditionalOperator(words, j))
                        AddOperator(words[j], lineIndex, wordOffset + j);
                }
                if (Word.Contains(SecondTask.GetFunctionsNames(SecondTask.Functions), words[j]))
                {
                    if (!Word.Contains(words, Data.FUNCTION))
                        AddOperator(words[j], lineIndex, wordOffset + j);
                }
            }
        }

        private static void SearchForOperatorsInWordsInQuotes(Word[] words, int lineIndex, int wordOffset)
        {
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].Text == "[")
                {
                    if (words[i - 1].Text[0] == '$')
                        AddOperator(words[i], lineIndex, wordOffset + i);
                }
            }
        }

        private static bool IsBracketForFunctionOrConditionalOperator(Word[] words, int bracketIndex)
        {
            if (bracketIndex == 0)
                return false;
            foreach (Function function in SecondTask.Functions)
                if (function.Name.Text == words[bracketIndex - 1])
                    return true;
            foreach (Function function in SecondTask.SystemFunctions)
                if (function.Name.Text == words[bracketIndex - 1])
                    return true;
            foreach (string str in Data.BlockOperators)
                if (str == words[bracketIndex - 1])
                    return true;
            return false;
        }

        private static void AddOperator(Word name, int lineIndex, int wordIndex)
        {
            if (ContainsOperator(name, out Operator @operator))
                @operator.Locations.Add(new Location(lineIndex, wordIndex));
            else
                Operators.Add(new Operator(name, new Location(lineIndex, wordIndex)));
        }

        private static bool ContainsOperator(Word name, out Operator @operator)
        {
            foreach (Operator _operator in Operators)
                if (_operator.Name.Text == name.Text)
                {
                    @operator = _operator;
                    return true;
                }
            @operator = null;
            return false;
        }

        private static int GetAllOperatorsCount()
        {
            int result = 0;
            foreach (Operator @operator in Operators)
                result += @operator.Locations.Count;
            return result;
        }
        #endregion

        #region Find Operators In Lines
        private static void FindOperatorsInLines()
        {
            OperatorsInLines = new List<List<Operator>>();
            for (int i = 0; i < Data.Code.Lines.Length; i++)
                OperatorsInLines.Add(new List<Operator>());
            foreach (Operator @operator in Operators)
            {
                foreach (Location location in @operator.Locations)
                {
                    Operator singleOperator = new Operator(@operator.Name, location);
                    if (singleOperator.Type == OperatorType.NotDefined)
                        singleOperator.Type = GetOperatorType(singleOperator);
                    OperatorsInLines[location.LineIndex].Add((singleOperator));
                }
            }
            for (int i = 0; i < Data.Code.Lines.Length; i++)
                OperatorsInLines[i].Sort(delegate (Operator first, Operator second) {
                    if (first.Locations[0].WordIndex < second.Locations[0].WordIndex)
                        return -1;
                    else
                        return 1;
                });
        }

        private static OperatorType GetOperatorType(Operator @operator)
        {
            if (@operator.Name.Text == "+" || @operator.Name.Text == "-")
                return PLUS_Or_MINUS_Type(@operator);
            else if (@operator.Name.Text == "++" || @operator.Name.Text == "--")
            {
                if (Is_IN_Or_DECREMENT_OperatorAfterOperand(@operator))
                    return OperatorType.Unary_OperandAtLeft;
                else
                    return OperatorType.Unary_OperandAtRight;
            }
            else
                throw new NotImplementedException();
        }

        private static OperatorType PLUS_Or_MINUS_Type(Operator @operator)
        {
            Word[] words = Data.Code[@operator.Locations[0].LineIndex];
            Word previousWord = words[@operator.Locations[0].WordIndex - 1];
            if (Word.Contains(Data.PreviousWordsForUnary_MINUS_Or_PLUS, previousWord) || previousWord.Text.Contains('='))
                return OperatorType.Unary_OperandAtRight;
            else
                return OperatorType.Binary;
        }

        private static bool Is_IN_Or_DECREMENT_OperatorAfterOperand(Operator @operator)
        {
            if (@operator.Locations[0].WordIndex == 0)
                return false;
            Word[] words = Data.Code[@operator.Locations[0].LineIndex];
            if (words[@operator.Locations[0].WordIndex - 1].Text[0] == '$')
                return true;
            else
                return false;
        }
        #endregion
             
        #region Find Operands In Lines
        private static void FindOperandsInLines()
        {
            OperandsInLines = new List<List<Operand>>();
            for (int i = 0; i < OperatorsInLines.Count; i++)
                OperandsInLines.Add(new List<Operand>());
            for (int i = 0; i < OperatorsInLines.Count; i++)
            {
                if (OperatorsInLines[i].Count == 0 || ContainsOnlyOperatorsWithoutOperands(OperatorsInLines[i]))
                {
                    if (Data.IsCountingCaseValueAsOperand)
                    {
                        if (Word.Contains(Data.Code[i], Data.CASE, out int CASE_Index))
                            OperandsInLines[i].Add(new Operand(Data.Code[i][CASE_Index + 1], new Location(i, CASE_Index + 1)));
                        continue;
                    }
                    else
                        continue;
                }
                if (Data.Code.Lines[i].Contains('\"'))
                {
                    FindOperandsInWordsInQuotes(OperatorsInLines[i], i);
                    FindStringsAsOperands(i);
                }
                else
                    FindOperandsInWords(OperatorsInLines[i], Data.Code[i], i, 0);
            }
        }

        private static void FindStringsAsOperands(int lineIndex)
        {
            (string Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(Data.Code.Lines[lineIndex]);
            int quotesCounter = 0;
            List<int> quotesPositions = Split.QuotesPositions(Data.Code[lineIndex]);
            foreach ((string Part, bool IsInQuotes) part in parts)
                if (part.IsInQuotes)
                {
                    Operand @string = new Operand('\"' + part.Part + '\"', new Location(lineIndex, quotesPositions[quotesCounter]));
                    OperandsInLines[lineIndex].Add(@string);
                    quotesCounter += 2;
                }
        }

        private static bool ContainsOnlyOperatorsWithoutOperands(List<Operator> operators)
        {
            foreach (Operator @operator in operators)
            {
                if (@operator.Type != OperatorType.NoOperands && @operator.Type != OperatorType.Function)
                    return false;
                if (@operator.Type == OperatorType.Function)
                {
                    Function function = SecondTask.GetFunctionByName(@operator.Name);
                    if (function.ParametersAmount > 0)
                        return false;
                }
            }
            return true;
        }

        private static void FindOperandsInWordsInQuotes(List<Operator> allOperators, int lineIndex)
        {
            (Word[] Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(lineIndex);
            int wordOffset = 0;
            foreach ((Word[] Part, bool IsInQuotes) part in parts)
            {
                List<Operator> operatorsInPart = GetOperatorsBetween(allOperators, wordOffset, wordOffset + part.Part.Length + 1);
                if (part.IsInQuotes)
                {
                    if (operatorsInPart.Count > 0)
                    {
                        foreach (Operator @operator in operatorsInPart)
                        {
                            if (@operator.Name.Text == "[")
                            {
                                int operatorIndex = @operator.Locations[0].WordIndex - wordOffset;
                                Operand left = new Operand(part.Part[operatorIndex - 1], lineIndex, @operator.Locations[0].WordIndex - 1);
                                Operand right = new Operand(part.Part[operatorIndex + 1], lineIndex, @operator.Locations[0].WordIndex + 1);
                                OperandsInLines[lineIndex].Add(left);
                                OperandsInLines[lineIndex].Add(right);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < part.Part.Length; i++)
                        {
                            if (part.Part[i].Text[0] == '$')
                            {
                                Operand operand = new Operand(part.Part[i].Text, new Location(lineIndex, wordOffset + i));
                                OperandsInLines[lineIndex].Add(operand);
                            }
                        }
                    }
                }
                else
                    FindOperandsInWords(operatorsInPart, part.Part, lineIndex, wordOffset);
                wordOffset += part.Part.Length + 1;
            }
        }

        private static List<Operator> GetOperatorsBetween(List<Operator> allOperators, int startIndex, int endIndex)
        {
            List<Operator> list = new List<Operator>();
            foreach (Operator @operator in allOperators)
                if (@operator.Locations[0].WordIndex >= startIndex && @operator.Locations[0].WordIndex < endIndex)
                    list.Add(@operator);
            return list;
        }

        private static void FindOperandsInWords(List<Operator> operators, Word[] words, int lineIndex, int wordOffset)
        {
            bool[] isUsed = new bool[words.Length];
            for (int i = 0; i < words.Length; i++)
                isUsed[i] = false;
            for (int i = 0; i < operators.Count; i++)
                isUsed[operators[i].Locations[0].WordIndex - wordOffset] = true;
            for (int i = 0; i < words.Length; i++)
                if (Word.Contains(Data.CanNotBeOperands, words[i]))
                    isUsed[i] = true;
            for (int i = 0; i < words.Length; i++)
            {
                if (isUsed[i] == false)
                {
                    Operand operand = new Operand(words[i], lineIndex, wordOffset + i);
                    OperandsInLines[lineIndex].Add(operand);
                }
            }
        }    
        #endregion

        #region Find Operands
        private static void FindOperands()
        {
            Operands = new List<Operand>();
            for (int i = 0; i < OperandsInLines.Count; i++)
            {
                foreach (Operand operand in OperandsInLines[i])
                {
                    if (ContainsOperand(operand.Name, out Operand existingOperand))
                        existingOperand.Locations.Add(operand.Locations[0]);
                    else
                        Operands.Add(new Operand(operand.Name, operand.Locations[0]));
                }
            }
            Operands.Sort();
        }

        private static bool ContainsOperand(Word name, out Operand operand)
        {
            foreach (Operand _operand in Operands)
                if (_operand.Name.Text == name.Text)
                {
                    operand = _operand;
                    return true;
                }
            operand = null;
            return false;
        }

        private static int GetAllOperandsCount()
        {
            int result = 0;
            foreach (Operand operand in Operands)
                result += operand.Locations.Count;
            return result;
        }
        #endregion

        #region Calculations
        private static void CalculateHolstedsMetrics()
        {
            AllOperatorsAmount = GetAllOperatorsCount();
            UniqueOperatorsAmount = Operators.Count;
            AllOperandsAmount = GetAllOperandsCount();
            UniqueOperandsAmount = Operands.Count;

            ProgramsDictionary = UniqueOperatorsAmount + UniqueOperandsAmount;
            ProgramsLength = AllOperatorsAmount + AllOperandsAmount;
            ProgramsVolume = ProgramsLength * Math.Log(ProgramsDictionary, 2);
        }
        #endregion

        #region Single Operators And Operands Grids
        private static Grid GetSingleOperatorsGrid(int lineIndex)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            int rowCounter = 0;
            foreach (Operator @operator in OperatorsInLines[lineIndex])
            {
                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock TB = new TextBlock() { Text = @operator.GetOperatorName(), Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                WPF_Methods.AddToGrid(ref grid, (TB, 0, rowCounter++));
            }
            return grid;
        }

        private static Grid GetSingleOperandsGrid(int lineIndex)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            int rowCounter = 0;
            foreach (Operand operand in OperandsInLines[lineIndex])
            {
                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock TB = new TextBlock() { Text = operand.Name.Text, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                WPF_Methods.AddToGrid(ref grid, (TB, 0, rowCounter++));
            }
            return grid;
        }
        #endregion

        #region Operators In Lines Grid
        private static void MarkupOperatorsInLineGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition numberColumn = new ColumnDefinition() { Width = new GridLength(Data.NumberWidth, GridUnitType.Pixel) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition operatorColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, numberColumn, firstSplitterColumn, operatorColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInOperatorsGrid);
        }

        private static void MakeBordersInOperatorsInLineGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInOperatorsGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInOperatorsGrid, (2, 1));
        }

        private static Grid GetTemplateOfOperatorsInLineGrid(string number, string @operator, bool needsBottomBorder, bool isSettingMaxHeight)
        {
            Grid grid = new Grid();
            MarkupOperatorsInLineGrid(ref grid, needsBottomBorder);

            TextBlock numberTextBlock = new TextBlock() { Text = number, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBlock operatorTextBlock = new TextBlock() { Text = @operator, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            WPF_Methods.AddToGrid(ref grid, (numberTextBlock, 1, 1), (operatorTextBlock, 3, 1));
            MakeBordersInOperatorsInLineGrid(ref grid, needsBottomBorder);
            if (isSettingMaxHeight)
                grid.MaxHeight = Data.TemplateGridMaxHeight;
            return grid;
        }

        private static Grid GetOperatorsInLineGrid(int lineIndex, bool needsBottomBorder)
        {
            Grid grid = new Grid();
            MarkupOperatorsInLineGrid(ref grid, needsBottomBorder);

            TextBlock numberTextBlock = new TextBlock() { Text = (lineIndex + 1).ToString(), Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid singleOperatorsGrid = GetSingleOperatorsGrid(lineIndex);

            WPF_Methods.AddToGrid(ref grid, (numberTextBlock, 1, 1), (singleOperatorsGrid, 3, 1));
            MakeBordersInOperatorsInLineGrid(ref grid, needsBottomBorder);
            return grid;
        }

        public static Grid GetOperatorsInLinesGrid()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            int rowCounter = 0;
            grid.RowDefinitions.Add(new RowDefinition());
            WPF_Methods.AddToGrid(ref grid, (GetTemplateOfOperatorsInLineGrid("N", "Операторы", true, true), 0, rowCounter++));
            for (int i = 0; i < OperatorsInLines.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                if (i == OperatorsInLines.Count - 1)
                    WPF_Methods.AddToGrid(ref grid, (GetOperatorsInLineGrid(i, true), 0, rowCounter++));
                else
                    WPF_Methods.AddToGrid(ref grid, (GetOperatorsInLineGrid(i, false), 0, rowCounter++));
            }
            return grid;
        }
        #endregion

        #region Operands In Lines Grid
        private static void MarkupOperandsInLineGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition numberColumn = new ColumnDefinition() { Width = new GridLength(Data.NumberWidth, GridUnitType.Pixel) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition operandColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, numberColumn, firstSplitterColumn, operandColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInOperatorsGrid);
        }

        private static void MakeBordersInOperandsInLineGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInOperatorsGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInOperatorsGrid, (2, 1));
        }

        private static Grid GetTemplateOfOperandsInLineGrid(string number, string operand, bool needsBottomBorder, bool isSettingMaxHeight)
        {
            Grid grid = new Grid();
            MarkupOperandsInLineGrid(ref grid, needsBottomBorder);

            TextBlock numberTextBlock = new TextBlock() { Text = number, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBlock operandTextBlock = new TextBlock() { Text = operand, Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            WPF_Methods.AddToGrid(ref grid, (numberTextBlock, 1, 1), (operandTextBlock, 3, 1));
            MakeBordersInOperandsInLineGrid(ref grid, needsBottomBorder);
            if (isSettingMaxHeight)
                grid.MaxHeight = Data.TemplateGridMaxHeight;
            return grid;
        }
       
        private static Grid GetOperandsInLineGrid(int lineIndex, bool needsBottomBorder)
        {
            Grid grid = new Grid();
            MarkupOperandsInLineGrid(ref grid, needsBottomBorder);

            TextBlock numberTextBlock = new TextBlock() { Text = (lineIndex + 1).ToString(), Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid singleOperandGrid = GetSingleOperandsGrid(lineIndex);

            WPF_Methods.AddToGrid(ref grid, (numberTextBlock, 1, 1), (singleOperandGrid, 3, 1));
            MakeBordersInOperandsInLineGrid(ref grid, needsBottomBorder);
            return grid;
        }

        public static Grid GetOperandsInLinesGrid()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            int rowCounter = 0;
            grid.RowDefinitions.Add(new RowDefinition());
            WPF_Methods.AddToGrid(ref grid, (GetTemplateOfOperatorsInLineGrid("N", "Операнды", true, true), 0, rowCounter++));
            for (int i = 0; i < OperatorsInLines.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                if (i == OperatorsInLines.Count - 1)
                    WPF_Methods.AddToGrid(ref grid, (GetOperandsInLineGrid(i, true), 0, rowCounter++));
                else
                    WPF_Methods.AddToGrid(ref grid, (GetOperandsInLineGrid(i, false), 0, rowCounter++));
            }
            return grid;
        }
        #endregion       

        #region Operators And Operands Grid
        public static void MarkupOperatorOrOperandGrid(ref Grid grid, bool needsBottomBorder)
        {
            ColumnDefinition leftBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition numberColumn = new ColumnDefinition() { Width = new GridLength(Data.NumberWidthInHolstedMetricsGrids, GridUnitType.Pixel) };
            ColumnDefinition firstSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition centralColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition secondSplitterColumn = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            ColumnDefinition amountColumn = new ColumnDefinition() { Width = new GridLength(Data.AmountWidthInHolstedMetricsGrids, GridUnitType.Pixel) };
            ColumnDefinition rightBorder = new ColumnDefinition() { Width = new GridLength(Data.GridBorderWidthInOperatorsGrid, GridUnitType.Pixel) };
            WPF_Methods.AddToColumnsDefinitionsOfGrid(ref grid, leftBorder, numberColumn, firstSplitterColumn, centralColumn, secondSplitterColumn, amountColumn, rightBorder);
            WPF_Methods.MakeRowWithBordersInGrid(ref grid, needsBottomBorder, Data.GridBorderWidthInOperatorsGrid);
        }

        public static void MakeBordersInOperatorOrOperandGrid(ref Grid grid, bool needsBottomBorder)
        {
            WPF_Methods.MakeBorderRectangles(ref grid, needsBottomBorder, Data.GridBorderWidthInOperatorsGrid);
            WPF_Methods.MakeGridSplitters(ref grid, Data.GridBorderWidthInOperatorsGrid, (2, 1), (4, 1));
        }

        private static Grid GetTemplateOfOperatorOrOperandGrid((string Text, FontVariants Variant)[] number, (string Text, FontVariants Variant)[] center, (string Text, FontVariants Variant)[] amount, bool needsBottomBorder, bool isSettingMaxHeight)
        {
            Grid grid = new Grid();
            MarkupOperatorOrOperandGrid(ref grid, needsBottomBorder);

            TextBlock numberTextBlock = new TextBlock() { Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center};
            TextBlock centralTextBlock = new TextBlock() { Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            TextBlock amountTextBlock = new TextBlock() { Background = Data.ApplicationBackground, TextWrapping = TextWrapping.Wrap, FontSize = Data.TemplateTextFontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            WPF_Methods.AddToGrid(ref grid, (numberTextBlock, 1, 1), (centralTextBlock, 3, 1), (amountTextBlock, 5, 1));
            WPF_Methods.AddTextToTextBlock(ref numberTextBlock, number);
            WPF_Methods.AddTextToTextBlock(ref centralTextBlock, center);
            WPF_Methods.AddTextToTextBlock(ref amountTextBlock, amount);
            MakeBordersInOperatorOrOperandGrid(ref grid, needsBottomBorder);
            if (isSettingMaxHeight)
                grid.MaxHeight = Data.TemplateGridMaxHeight;
            return grid;
        }
        private static Grid GetTemplateOfOperatorsGrid(bool needsBottomBorder)
        {
            (string Text, FontVariants Variant)[] number = new(string Text, FontVariants Variant)[] { ("j", FontVariants.Normal) };
            (string Text, FontVariants Variant)[] central = new(string Text, FontVariants Variant)[] { ("Оператор", FontVariants.Normal) };
            (string Text, FontVariants Variant)[] amount = new(string Text, FontVariants Variant)[] { ("f", FontVariants.Normal), ("1j", FontVariants.Subscript) };
            return GetTemplateOfOperatorOrOperandGrid(number, central, amount, needsBottomBorder, true);
        }
        private static Grid GetTemplateOfOperandsGrid(bool needsBottomBorder)
        {
            (string Text, FontVariants Variant)[] number = new(string Text, FontVariants Variant)[] { ("i", FontVariants.Normal) };
            (string Text, FontVariants Variant)[] central = new(string Text, FontVariants Variant)[] { ("Операнд", FontVariants.Normal) };
            (string Text, FontVariants Variant)[] amount = new(string Text, FontVariants Variant)[] { ("f", FontVariants.Normal), ("2i", FontVariants.Subscript) };
            return GetTemplateOfOperatorOrOperandGrid(number, central, amount, needsBottomBorder, true);
        }

        public static Grid GetEndOfOperatorsGrid()
        {
            (string Text, FontVariants Variant)[] number = new(string Text, FontVariants Variant)[] { (Data.Eta.ToString(), FontVariants.Normal), ("1", FontVariants.Subscript), (" = " + UniqueOperatorsAmount.ToString(), FontVariants.Normal) };
            (string Text, FontVariants Variant)[] central = new(string Text, FontVariants Variant)[] { ("", FontVariants.Normal) };
            (string Text, FontVariants Variant)[] amount = new(string Text, FontVariants Variant)[] { ("N", FontVariants.Normal), ("1", FontVariants.Subscript), (" = " + AllOperatorsAmount.ToString(), FontVariants.Normal) };
            return GetTemplateOfOperatorOrOperandGrid(number, central, amount, true, true);
        }

        public static Grid GetEndOfOperandsGrid()
        {
            (string Text, FontVariants Variant)[] number = new(string Text, FontVariants Variant)[] { (Data.Eta.ToString(), FontVariants.Normal), ("2", FontVariants.Subscript), (" = " + UniqueOperandsAmount.ToString(), FontVariants.Normal) };
            (string Text, FontVariants Variant)[] central = new(string Text, FontVariants Variant)[] { ("", FontVariants.Normal) };
            (string Text, FontVariants Variant)[] amount = new(string Text, FontVariants Variant)[] { ("N", FontVariants.Normal), ("2", FontVariants.Subscript), (" = " + AllOperandsAmount.ToString(), FontVariants.Normal) };
            return GetTemplateOfOperatorOrOperandGrid(number, central, amount, true, true);
        }
            
        public static Grid GetOperatorsGrid()
        {
            int rowCounter = 0;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            WPF_Methods.AddToGrid(ref grid, (GetTemplateOfOperatorsGrid(false), 0, rowCounter++));
            for (int i = 0; i < Operators.Count; i++)
            {
                WPF_Methods.AddToRowsDefinitionsOfGrid(ref grid, new RowDefinition());
                if (i == Operators.Count - 1)
                    WPF_Methods.AddToGrid(ref grid, (Operators[i].GetOperatorGrid(true, i, Data.UsualTextFontSize), 0, rowCounter++));
                else
                    WPF_Methods.AddToGrid(ref grid, (Operators[i].GetOperatorGrid(false, i, Data.UsualTextFontSize), 0, rowCounter++));
            }
            return grid;
        }

        public static Grid GetOperandsGrid()
        {
            int rowCounter = 0;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            WPF_Methods.AddToGrid(ref grid, (GetTemplateOfOperandsGrid(false), 0, rowCounter++));
            for (int i = 0; i < Operands.Count; i++)
            {
                WPF_Methods.AddToRowsDefinitionsOfGrid(ref grid, new RowDefinition());
                if (i == Operands.Count - 1)
                    WPF_Methods.AddToGrid(ref grid, (Operands[i].GetOperandGrid(true, i, Data.UsualTextFontSize), 0, rowCounter++));
                else
                    WPF_Methods.AddToGrid(ref grid, (Operands[i].GetOperandGrid(false, i, Data.UsualTextFontSize), 0, rowCounter++));
            }
            return grid;
        }
        #endregion

        public static string GetLocations<T>(T obj) where T : ILocationable
        {
            string result = "";
            int columns = obj.Locations.Count / 30 + 1;
            int columnCounter = 0;
            for (int i = 0; i < obj.Locations.Count; i++)
            {
                if (columnCounter != 0)
                    result += "\t";
                result += obj.Locations[i].ToString();
                columnCounter++;
                if (columnCounter == columns)
                {
                    if (i != obj.Locations.Count - 1)
                        result += "\n";
                    columnCounter = 0;
                }
            }
            return result;
        }

        public static void Perform()
        {
            try { FindOperators(); }
            catch (Exception) { throw new Exception("Не удалось найти операторы и их местоположение"); }

            try { FindOperatorsInLines(); }
            catch (Exception) { throw new Exception("Не удалось найти операторы по строкам"); }

            try { FindOperandsInLines(); }
            catch (Exception) { throw new Exception("Не удалось найти операнды по строкам"); }

            try { FindOperands(); }
            catch (Exception) { throw new Exception("Не удалось найти операнды и их местоположение"); }

            try { CalculateHolstedsMetrics(); }
            catch (Exception) { throw new Exception("Не удалось вычислить метрики Холстеда"); }
        }
        #endregion
    }
}
