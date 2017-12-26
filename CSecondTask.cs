using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSiSvInfT
{
    using Split = SplitMethods;
    using Line = LineAnalyseMethods;
    using Brackets = BracketsMethods;

    public static class SecondTask
    {
        #region Properties
        public static MainFunction MainFunction { get; set; }
        public static List<Constant> Constants { get; set; }
        public static int ConstantsSpen { get; set; }
        public static List<Function> Functions { get; set; }
        public static List<Function> SystemFunctions { get; } = new List<Function>()
        {
            new Function("echo", false, true, 1),
            new Function("readline", true, false, 0),
            new Function("settype", true, false, 2),
            new Function("count", true, false, 1),
            new Function("rand", true, false, 2),
            new Function("define", true, false, 2),
            new Function("microtime", true, false, 1)
        };

        private static List<Variable>[] Groups;
        #endregion

        #region Methods
        private static void CreateFunctions()
        {
            Functions = new List<Function>();
            Constants = new List<Constant>();
            MainFunction = new MainFunction();
            for (int i = 0; i < Data.Code.Lines.Length; i++)
            {
                Word[] words = Data.Code[i]; 
                if (words.Length == 0)
                    continue;
                if (Split.IsTag(words[0].Text))
                    continue;
                if (words[0].Text == "")
                    continue;
                if (Word.Contains(words, Data.FUNCTION))
                {
                    int end = Brackets.GetCloseCurveBracketIndex(Data.Code.Lines, i + 1);
                    Functions.Add(new Function(i, end, words));
                    i = end;
                }
                else
                {                    
                    int end = MainFunctionEnd(i);
                    MainFunction.AddLines(i, end);
                    i = end;
                }
            }

            List<Function> onlyOutputtingFunctions = GetFunctionsThatAreOnlyForOutput();
            foreach (Function function in Functions)
                function.DefineIsOnlyForOutputting(ref onlyOutputtingFunctions);
        }

        private static void CreateMainFunctionAndConstants()
        {
            Functions.Add(MainFunction);
            MainFunction.FindVariables();
            MainFunction.FindVariablesLines();
            FindConstantsLines();
        }

        public static Word[] GetFunctionsNames(List<Function> functions)
        {
            Word[] array = new Word[functions.Count];
            for (int i = 0; i < functions.Count; i++)
                array[i] = functions[i].Name;
            return array;
        }

        private static Function _GetFunctionByName(Word name, List<Function> list, out bool wasFind)
        {
            foreach (Function function in list)
                if (function.Name.Text == name.Text)
                {
                    wasFind = true;
                    return function;
                }
            wasFind = false;
            return null;
        }

        public static Function GetFunctionByName(Word name)
        {
            Function function = _GetFunctionByName(name, Functions, out bool wasFind);
            if (!wasFind)
                function = _GetFunctionByName(name, SystemFunctions, out wasFind);
            return function;
        }

        public static List<Function> GetFunctionsThatAreOnlyForOutput()
        {
            List<Function> list = new List<Function>();
            foreach (Function function in Functions)
                if (function.IsOnlyForOutputting)
                    list.Add(function);
            foreach (Function function in SystemFunctions)
                if (function.IsOnlyForOutputting)
                    list.Add(function);
            return list;
        }

        #region Control Variables
        private static void FindControlVariables()
        {
            foreach (Function function in Functions)
                FindControlVariablesInList(function.Variables);
            FindControlVariablesInList(Constants);
        }

        private static void FindControlVariablesInList<T>(List<T> variables)
            where T: Variable
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Group != Group.NotDefined)
                    continue;
                foreach (int lineIndex in variables[i].Lines)
                {
                    if (Line.IsControlVariableInLine(variables[i], out string clarification, lineIndex))
                    {
                        variables[i].SetGroup(Group.C, clarification, false);
                        break;
                    }
                }
            }
        }
        #endregion

        #region P Variables
        private static void Find_P_Variables()
        {
            foreach (Function function in Functions)
            {
                foreach (Variable variable in function.Variables)
                {
                    if (variable.Group != Group.NotDefined)
                        continue;
                    if (variable.IsParameter)
                    {
                        if (variable.IsPassedByReference)
                        {
                            if (!IsModifyingVariable(variable, 0))
                                variable.SetGroup(Group.P, "Параметр, содержащий исходную информацию и не модифицируемый в функции.", true);
                        }
                        else
                        {
                            variable.SetGroup(Group.P, "Параметр, содержащий исходную информацию и не модифицируемый в функции.", true);
                            continue;
                        }
                    }
                
                    if (IsInputtingDataToVariable(variable, out int indexOfLineNumberWithDataInputting))
                    {
                        if (!IsModifyingVariable(variable, indexOfLineNumberWithDataInputting))
                            variable.SetGroup(Group.P, "Переменная, содержащая информацию, вводимую функцией \'readline\' в строке " + (variable.Lines[indexOfLineNumberWithDataInputting] + 1).ToString() + ".", false);
                    }
                }
            }
        }

        private static bool IsInputtingDataToVariable(Variable variable, out int indexOfLineNumberWithDataInputting)
        {
            indexOfLineNumberWithDataInputting = -2;
            for (int i = 0; i < variable.Lines.Count; i++)
            {
                if (Line.IsLineWithInputtingDataToVariable(variable.Lines[i], variable))
                {
                    indexOfLineNumberWithDataInputting = i;
                    return true;
                }
            }
            return false;
        }

        private static bool IsModifyingVariable(Variable variable, int indexOfLineNumberWithDataInputting)
        {
            for (int i = indexOfLineNumberWithDataInputting + 1; i < variable.Lines.Count; i++)
                if (Line.IsSettingValueToVariable(variable.Lines[i], variable))
                    return true;
            return false;
        }
        #endregion

        #region Modifable Variables
        private static void FindModifableVariables()
        {
            foreach (Function function in Functions)
                FindModifableVariablesInList(function.Variables, function);
            foreach (Constant constant in Constants)
            {
                if (constant.Group == Group.NotDefined)
                    constant.SetGroup(Group.M, "Костанта, не являющаяся управляющей переменной", false);
            }
        }

        private static void FindModifableVariablesInList(List<Variable> variables, Function function)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Group != Group.NotDefined)
                    continue;
                if (variables[i].IsParameter && !variables[i].IsPassedByReference)
                    continue;

                int lineWithStartValueIndex = FindLineWithStartValueIndex(variables[i]);
                if (TryFindLineWithValueSettingToVariable(lineWithStartValueIndex, lineWithStartValueIndex, variables[i]))
                    continue;

                if (!variables[i].IsParameter)
                    variables[i].SetGroup(Group.M, "Вводимая в функции переменная, не являющаяся управляющей переменной.", true);
            }
        }

        private static bool TryFindLineWithValueSettingToVariable(int realLineWithStartValueIndex, int lineWithStartValueIndex, Variable variable)
        {
            for (int j = lineWithStartValueIndex + 1; j < variable.Lines.Count; j++)
            {
                if (Line.IsSettingValueToVariable(variable.Lines[j], variable))
                {
                    if (FirstTask.IsLineInBlock(variable.Lines[lineWithStartValueIndex], out CodeBlock block))
                    {
                        List<CodeBlock> connectedBlocks = FirstTask.Get_IF_ConnectedBlocks(block);
                        if (IsLineIn_IF_ConnectedBlock(connectedBlocks, variable.Lines[j]))
                            return TryFindLineWithValueSettingToVariable(realLineWithStartValueIndex, j, variable);
                    }
                    string clarification;
                    if (variable.IsParameter)
                        clarification = "Начальное значение изменяется в строке " + (variable.Lines[j] + 1).ToString() + ".";
                    else
                        clarification = "Начальное значение задается в строке " + (variable.Lines[realLineWithStartValueIndex] + 1).ToString() + ", изменяется в строке " + (variable.Lines[j] + 1).ToString() + ".";
                    variable.SetGroup(Group.M, clarification, true);
                    return true;
                }
            }
            return false;
        }

        private static bool IsLineIn_IF_ConnectedBlock(List<CodeBlock> list, int lineIndex)
        {
            foreach (CodeBlock block in list)
                if (block.IsLineInBlock(lineIndex))
                    return true;
            return false;
        }

        private static bool AreComputationsAfterLine(Function function, int lineIndex, out int indexOfLineWithComputation)
        {
            for (int i = lineIndex + 1; i <= function.End; i++)
            {
                if (Line.AreComputationsInLine(i))
                {
                    indexOfLineWithComputation = i;
                    return true;
                }
            }
            indexOfLineWithComputation = -1;
            return false;
        }

        private static bool IsOutputtingVariable(Variable variable, out int lineWithOutputIndex, out Function function)
        {
            foreach (int index in variable.Lines)
            {
                Word[] words = Data.Code[index]; //
                if (Line.IsOutputtingFunctionWithVariableInLine(index, variable, out function))
                {
                    lineWithOutputIndex = index;
                    return true;
                }
            }
            lineWithOutputIndex = -1;
            function = null;
            return false;
        }

        private static int FindLineWithStartValueIndex(Variable variable)
        {
            if (variable.IsParameter)
                return 0;
            else
            {
                for (int j = 0; j < variable.Lines.Count; j++)
                {
                    if (Line.IsSettingValueToVariable(variable.Lines[j], variable))
                        return j;
                }
                return 0;
            }
        }
        #endregion

        #region Parasitic Variables
        private static void FindParasiticVariables()
        {
            foreach (Function function in Functions)
            {
                foreach (Variable variable in function.Variables)
                {
                    if (variable.Group == Group.P && variable.CanGroupBe_T)
                    {
                        if (!IsUsingOnlyInFunctionsWithoutResult(variable))
                            continue;

                        if (IsUsingInCreatingString(variable, out int lineIndex))
                        {
                            variable.SetGroup(Group.P, variable.GroupClarification + " Используется для создания строкового выражения в строке " + (lineIndex + 1).ToString() + ".", false);
                            continue;
                        }
                        if (IsUsingInOutputtingFunctions(variable, out lineIndex, out Function _function))
                        {
                            variable.SetGroup(Group.P, variable.GroupClarification + " Выводится в строке " + (lineIndex + 1).ToString() + " функцией \'" + _function.Name.Text + "\'.", false);
                            continue;
                        }
                        if (IsUsingInSettingValueForOtherVariable(variable, out lineIndex))
                        {
                            variable.SetGroup(Group.P, variable.GroupClarification + " Используется в операторе \'" + Data.EQUAL.Text + "\' в строке " + (lineIndex + 1).ToString() + ".", false);
                            continue;
                        }
                        variable.SetGroup(Group.T, "Не модифицируется и не участвует в вычислениях, так как используется только для вызова функций, не возвращающих значения.", false);
                    }
                    else if (variable.Group == Group.M && variable.CanGroupBe_T)
                        Check_M_Variable(function, variable);
                    else if (variable.Group == Group.NotDefined)
                        variable.SetGroup(Group.T, "Не подпадает под описание остальных групп.", false);
                }
            }
        }

        private static bool IsUsingInSettingValueForOtherVariable(Variable variable, out int index)
        {
            foreach (int lineIndex in variable.Lines)
                if (Line.IsUsingInSettingValueForOtherVariableInLine(lineIndex, variable))
                {
                    index = lineIndex;
                    return true;
                }
            index = -1;
            return false;
        }

        private static bool IsUsingInOutputtingFunctions(Variable variable, out int index, out Function function)
        {
            for (int i = 0; i < variable.Lines.Count; i++)
            {
                if (i == 0 && variable.IsParameter)
                    continue;
                if (Line.IsOutputtingFunctionWithVariableInLine(variable.Lines[i], variable, out function))
                {
                    index = variable.Lines[i];
                    return true;
                }
            }
            index = -1;
            function = null;
            return false;
        }

        private static bool IsUsingOnlyInFunctionsWithoutResult(Variable variable)
        {
            int start = 0;
            if (variable.IsParameter)
                start = 1;
            for (int i = start; i < variable.Lines.Count; i++)
            {
                Word[] words = Data.Code[variable.Lines[i]]; //
                if (Word.ContainsAny(words, GetFunctionsNames(Functions), out int index) || Word.ContainsAny(words, GetFunctionsNames(SystemFunctions), out index))
                {
                    Word functionName = words[index];
                    Function function = GetFunctionByName(functionName);
                    if (function.IsResult)
                        return false;
                }
            }
            return true;
        }

        private static bool IsUsingInCreatingString(Variable variable, out int index)
        {
            foreach (int lineIndex in variable.Lines)
            {
                string line = Data.Code.Lines[lineIndex];
                if (!line.Contains('\"'))
                    continue;
                (Word[] Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(lineIndex);
                foreach((Word[] Part, bool IsInQuotes) part in parts)
                {
                    if (!part.IsInQuotes)
                        continue;
                    Word[] words = part.Part;
                    foreach (Word word in words)
                        if (Word.Contains(word.SeparateEscapeSequence(), variable.Name))
                        {
                            index = lineIndex;
                            return true;
                        }
                }
            }
            index = -1;
            return false;
        }

        private static void Check_M_Variable(Function function, Variable variable)
        {
            int lastUsing = variable.Lines.Last();
            if (AreComputationsAfterLine(function, lastUsing, out int indexOfLineWithComputations))
            {
                string clarification = variable.GroupClarification + " Не участвует в последующих (после строки " + (lastUsing + 1).ToString() + ") вычислениях";
                bool isUsingIn_ECHO = IsUsingIn_ECHO(variable, out int ECHO_LineIndex);
                if (!IsOutputtingVariable(variable, out int lineWithOutputIndex, out Function outputtingFunction) && !isUsingIn_ECHO)
                {
                    clarification += " и не выводится.";
                    variable.SetGroup(Group.T, clarification, false);
                }
                else
                {
                    if (isUsingIn_ECHO)
                    {
                        clarification += ", но выводится в строке " + (ECHO_LineIndex + 1).ToString() + " функцией \'" + Data.ECHO.Text + "\'.";
                        variable.SetGroup(Group.M, clarification, false);
                    }
                    else
                    {
                        clarification += ", но выводится в строке " + (lineWithOutputIndex + 1).ToString() + " функцией \'" + outputtingFunction.Name.Text + "\'.";
                        variable.SetGroup(Group.M, clarification, false);
                    }
                }
            }
        }

        private static bool IsUsingIn_ECHO(Variable variable, out int index)
        {
            foreach (int lineIndex in variable.Lines)
            {
                Word[] words = Data.Code[lineIndex]; 
                if (Word.Contains(words, Data.ECHO))
                {
                    index = lineIndex;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        #endregion

        private static int MainFunctionEnd(int startLine)
        {
            for (int i = startLine; i < Data.Code.Lines.Length; i++)
            {
                Word[] words = Data.Code[i]; 
                if (words.Length == 0)
                    continue;
                if (Split.IsTag(words[0].Text))
                    return i - 1;
                if (words[0].Text == Data.FUNCTION.Text)
                    return i - 1;
            }
            Data.ThrowError("Не удалось найти конец главной функции");
            return 0;
        }

        private static void FindConstantsLines()
        {
            for (int i = 0; i < Data.Code.Lines.Length; i++)
            {
                Word[] words = Data.Code[i]; //
                for (int j = 0; j < Constants.Count; j++)
                {
                    if (Word.Contains(words, Constants[j].Name))
                        Constants[j].Lines.Add(i);
                }
            }
        }

        public static void Perform()
        {
            try { CreateFunctions(); }
            catch (Exception) { throw new Exception("Не удалось выделить описания функций"); }

            try { CreateMainFunctionAndConstants(); }
            catch (Exception) { throw new Exception("Не удалось выделить главную функцию и константы"); }

            try { FindControlVariables(); }
            catch (Exception) { Data.Errors.Add("Не удалось найти управляющие переменные"); }

            try { Find_P_Variables(); }
            catch (Exception) { Data.Errors.Add("Не удалось найти переменные группы P"); }

            try { FindModifableVariables(); }
            catch (Exception) { Data.Errors.Add("Не удалось найти модифицируемые переменные"); }

            try { FindParasiticVariables(); }
            catch (Exception) { Data.Errors.Add("Не удалось найти паразитные переменные"); }

            try { PackVariablesToGroups(); }
            catch (Exception) { Data.Errors.Add("Не удалось разделить переменные по группам"); }

            try { CountVariablesSpens(); }
            catch (Exception) { Data.Errors.Add("Не удалось вычислить спен"); }
        }

        #region Spen Metric
        private static void CountVariablesSpens()
        {
            foreach (Function function in Functions)
            {
                foreach (Variable variable in function.Variables)
                    variable.CountSpen();
                function.CountSpen();
            }
            ConstantsSpen = 0;
            foreach (Constant constant in Constants)
            {
                constant.CountSpen();
                ConstantsSpen += constant.Spen;
            }
        }

        public static int CommonSpen()
        {
            int result = 0;
            foreach (Function function in Functions)
                foreach (Variable variable in function.Variables)
                    result += variable.Spen;
            foreach (Constant constant in Constants)
                result += constant.Spen;
            return result;
        }

        public static Grid GetSpenGrid()
        {
            int rowCounter = 0;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            RowDefinition resultCaptionRow = new RowDefinition() { Height = new GridLength(Data.NameRowHeight, GridUnitType.Pixel) };
            RowDefinition emptyRow = new RowDefinition() { Height = new GridLength(Data.EmptyRowHeight, GridUnitType.Pixel) };
            RowDefinition templateRow = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star), MaxHeight = Data.TemplateGridMaxHeight };
            WPF_Methods.AddToRowsDefinitionsOfGrid(ref grid, resultCaptionRow, emptyRow, templateRow);

            TextBox captionTextBox = new TextBox() { Text = "Спен программы: " + CommonSpen().ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, FontSize = Data.CaptionTextFontSize, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true, Cursor = Cursors.Arrow };
            WPF_Methods.AddToGrid(ref grid, (captionTextBox, 0, rowCounter++));
            rowCounter++;
            WPF_Methods.AddToGrid(ref grid, (Variable.GetTemplateOfSpenGrid(true, Data.TemplateTextFontSize), 0, rowCounter++));
            foreach (Function function in Functions)
            { 
                for (int i = 0; i < function.Variables.Count; i++) 
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    Grid variableGrid;
                    if (i == function.Variables.Count - 1)
                    {
                        variableGrid = function.Variables[i].GetSpenGrid(true, rowCounter - 2, Data.UsualTextFontSize);
                        variableGrid.Margin = Data.SpenGridMargin;
                    }
                    else
                        variableGrid = function.Variables[i].GetSpenGrid(false, rowCounter - 2, Data.UsualTextFontSize);
                    WPF_Methods.AddToGrid(ref grid, (variableGrid, 0, rowCounter++));
                }
            }
            for (int i = 0; i < Constants.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                if (i == Constants.Count - 1)
                    WPF_Methods.AddToGrid(ref grid, (Constants[i].GetSpenGrid(true, rowCounter - 2, Data.UsualTextFontSize), 0, rowCounter++));
                else
                    WPF_Methods.AddToGrid(ref grid, (Constants[i].GetSpenGrid(false, rowCounter - 2, Data.UsualTextFontSize), 0, rowCounter++));
            }
            return grid;
        }
        #endregion

        #region Chapins Metric
        private static void PackVariablesToGroups()
        {
            Groups = new List<Variable>[] {
                new List<Variable>(),
                new List<Variable>(),
                new List<Variable>(),
                new List<Variable>() };
            foreach (Function function in Functions)
                foreach (Variable variable in function.Variables)
                    Groups[(int)variable.Group].Add(variable);
            foreach (Constant constant in Constants)
                Groups[(int)constant.Group].Add(constant);
        }

        private static double GetChapinMetric()
            => Groups[(int)Group.P].Count + 2 * Groups[(int)Group.M].Count + 3 * Groups[(int)Group.C].Count + 0.5 * Groups[(int)Group.T].Count;

        public static Grid GetGroupsGrid()
        {
            int rowCounter = 0;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition resultCaptionRow = new RowDefinition() { Height = new GridLength(Data.NameRowHeight, GridUnitType.Pixel) };
            RowDefinition resultRow = new RowDefinition() { Height = new GridLength(Data.ResultRowHeight, GridUnitType.Pixel) };
            RowDefinition emptyRow = new RowDefinition() { Height = new GridLength(Data.EmptyRowHeight, GridUnitType.Pixel) };
            RowDefinition templateRow = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star), MaxHeight = Data.TemplateGridMaxHeight };
            WPF_Methods.AddToRowsDefinitionsOfGrid(ref grid, resultCaptionRow, resultRow, emptyRow, templateRow);

            TextBox captionTextBox = new TextBox() { Text = "Полная метрика Чепина", Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, FontSize = Data.CaptionTextFontSize, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true, Cursor = Cursors.Arrow };
            WPF_Methods.AddToGrid(ref grid, (captionTextBox, 0, rowCounter++));
            TextBox resultTextBox = new TextBox() { Text = "Q = 1" + Data.Multiply + "p + 2" + Data.Multiply + "m + 3" + Data.Multiply + "c + 0,5" + Data.Multiply + "t = 1" + Data.Multiply + Groups[(int)Group.P].Count.ToString() + " + 2" + Data.Multiply + Groups[(int)Group.M].Count.ToString() + " + 3" + Data.Multiply + Groups[(int)Group.C].Count.ToString() + " + 0,5" + Data.Multiply + Groups[(int)Group.T].Count.ToString() + " = " + GetChapinMetric().ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, FontSize = Data.CaptionTextFontSize, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true, Cursor = Cursors.Arrow };
            WPF_Methods.AddToGrid(ref grid, (resultTextBox, 0, rowCounter++));
            rowCounter++;
            WPF_Methods.AddToGrid(ref grid, (Variable.GetTemplateOfGroupGrid(true, Data.TemplateTextFontSize), 0, rowCounter++));
            for (int i = (int)Group.P; i < (int)Group.NotDefined; i++)
            {
                WPF_Methods.AddToRowsDefinitionsOfGrid(ref grid, new RowDefinition(), new RowDefinition());
                TextBox groupTextBox = new TextBox() { Text = "Группа " + Variable.GetGroupName((Group)i), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, FontSize = Data.CaptionTextFontSize, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true, Cursor = Cursors.Arrow };
                WPF_Methods.AddToGrid(ref grid, (groupTextBox, 0, rowCounter++));
                for (int j = 0;  j < Groups[i].Count; j++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    if (j == Groups[i].Count - 1)
                        WPF_Methods.AddToGrid(ref grid, (Groups[i][j].GetGroupGrid(true, j + 1, Data.UsualTextFontSize), 0, rowCounter++));
                    else
                        WPF_Methods.AddToGrid(ref grid, (Groups[i][j].GetGroupGrid(false, j + 1, Data.UsualTextFontSize), 0, rowCounter++));
                }
            }
            return grid;
        }
        #endregion
        #endregion
    }
}
