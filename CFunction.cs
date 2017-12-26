using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;

namespace MSiSvInfT
{
    using Split = SplitMethods;

    public class Function
    {
        #region Properties
        public List<Variable> Variables { get; set; } = new List<Variable>();
        public Word Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }      
        public bool IsResult { get; set; }
        public bool IsOnlyForOutputting { get; set; }
        public int Spen { get; set; }
        public int ParametersAmount { get; set; }
        #endregion

        #region Constructors
        protected Function() { }

        protected Function(int start, int end)
        {
            Start = start;
            End = end;
            IsResult = Contains_RETURN();
        }

        public Function(int start, int end, Word[] definition): this(start, end)
        {
            Name = definition[1];
            FindParameters(definition);
         
            FindVariables();
            FindVariablesLines();
        }

        public Function(string name, bool isReslt, bool isOnlyForOutputting, int parametersAmount)
        {
            Name = new Word(name, false, false);
            IsResult = isReslt;
            IsOnlyForOutputting = isOnlyForOutputting;
            ParametersAmount = parametersAmount;
        }
        #endregion

        #region Methods
        public void CountSpen()
        {
            Spen = 0;
            foreach (Variable variable in Variables)
                Spen += variable.Spen;
        }

        private bool Contains_RETURN()
        {
            for (int i = End; i >= Start; i--)
            {
                Word[] words = Data.Code[i]; 
                if (Word.Contains(words, Data.RETURN))
                    return true;
            }
            return false;
        }

        private void FindParameters(Word[] definition)
        {
            int parametersStart = Word.IndexOfFirst(definition, '(');
            int parametersEnd = Word.IndexOfFirst(definition, ')');
            ParametersAmount = 0;
            for (int i = parametersStart + 1; i < parametersEnd; i++)
            {
                if (definition[i] != "" && definition[i] != Data.REFERENCE.Text && definition[i] != ",")
                {
                    ParametersAmount++;
                    bool isPassedByReference = false;
                    if (definition[i - 1].Text == Data.REFERENCE.Text)
                        isPassedByReference = true;
                    Variable variable = new Variable(this, definition[i], true, isPassedByReference);
                    Variables.Add(variable);
                }
            }
        }

        protected void _FindVariables(int start)
        {
            for (int i = start; i <= End; i++)
            {
                string line = Data.Code.Lines[i];
                if (!line.Contains("\""))
                    FindVariablesInLineWithoutQuotes(i, this);
                else
                    FindVariablesInLineWithQuotes(i, this);
            }
        }
        public virtual void FindVariables() => _FindVariables(Start + 1);
      
        protected void FindVariablesInLineWithoutQuotes(Word[] words, Function function)
        {
            foreach (Word word in words)
            {
                if (word.Text.Length > 0 && word.Text[0] == '$')
                {
                    Variable potentialVariable = new Variable(function, word.RemovePossibleLastCharacter(','));
                    if (!WasVariable(potentialVariable))
                        Variables.Add(potentialVariable);
                }
            }
        }
        protected void FindVariablesInLineWithoutQuotes(int lineIndex, Function function) => FindVariablesInLineWithoutQuotes(Data.Code[lineIndex], function);

        protected void FindVariablesInLineWithQuotes(int lineIndex, Function function)
        {
            (Word[] Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(lineIndex);
            foreach ((Word[] Part, bool IsInQuotes) part in parts)
                if (!part.IsInQuotes)
                    FindVariablesInLineWithoutQuotes(part.Part, function);
            FindConstants(parts);
        }

        private void FindConstants((Word[] Part, bool IsInQuotes)[] parts)
        {
            if (parts.Length == 0 || parts[0].Part.Length == 0)
                return;
            Word[] words = parts[0].Part;
            if (words[0].Text == Data.DEFINE.Text)
                SecondTask.Constants.Add(new Constant(parts[1].Part[0].Text));
        }

        private bool WasVariable(Variable possibleVariable)
        {
            foreach (Variable variable in Variables)
                if (possibleVariable == variable)
                    return true;
            return false;
        }

        public virtual void FindVariablesLines()
        {
            for (int j = Start; j <= End; j++)
            {
                string line = Data.Code.Lines[j];
                if (line.Contains('\"'))
                    FindVariablesLinesInLineWithQuotes(j);
                else
                    FindVariablesLinesInLineWithoutQuotes(j);
            }
        }

        protected void FindVariablesLinesInLineWithoutQuotes(Word[] words, int lineIndex)
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Word.Contains(words, Variables[i].Name))
                    Variables[i].Lines.Add(lineIndex);
            }
        }
        protected void FindVariablesLinesInLineWithoutQuotes(int lineIndex) => FindVariablesLinesInLineWithoutQuotes(Data.Code[lineIndex], lineIndex);

        protected void FindVariablesLinesInLineWithQuotes(int lineIndex)
        {
            (Word[] Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(lineIndex);
            foreach ((Word[] Part, bool IsInQuotes) part in parts)
            {
                if (part.IsInQuotes && Word.Contains(part.Part, Data.DOLLAR))
                {
                    Word[] words = part.Part;
                    for (int i = 0; i < Variables.Count; i++)
                    {                
                        foreach (Word word in words)
                        {
                            if (word.Text.Contains('\\'))
                            {
                                Word[] wordParts = word.SeparateEscapeSequence();
                                if (wordParts[0].Text == Variables[i].Name.Text)
                                    Variables[i].Lines.Add(lineIndex);
                            }
                            else
                            {
                                if (word.Text == Variables[i].Name.Text)
                                    Variables[i].Lines.Add(lineIndex);
                            }
                        }
                    }                  
                }
                else
                    FindVariablesLinesInLineWithoutQuotes(part.Part, lineIndex);
            }
        }

        public void DefineIsOnlyForOutputting(ref List<Function> onlyOutputtingFunctions)
        {
            bool isOnlyOutputting = true;
            for (int i = Start + 2; i < End; i++)
            {
                Word[] words = Data.Code[i]; 
                if (Word.Contains(words, Data.ECHO.Text))
                    continue;
                if (Word.ContainsAny(words, SecondTask.GetFunctionsNames(onlyOutputtingFunctions), out int index))
                    continue;
                else
                {
                    isOnlyOutputting = false;
                    break;
                }
            }
            if (isOnlyOutputting)
            {
                onlyOutputtingFunctions.Add(this);
                IsOnlyForOutputting = true;
            }
            else
                IsOnlyForOutputting = false;
        }

        public virtual Grid GetFunctionGrid(string caption = null)
        {
            Grid grid = new Grid();
            ColumnDefinition variablesColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            grid.ColumnDefinitions.Add(variablesColumn);
            RowDefinition nameRow = new RowDefinition() { Height = new GridLength(Data.NameRowHeight, GridUnitType.Pixel) };
            RowDefinition templateRow = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star), MaxHeight = Data.TemplateGridMaxHeight };
            WPF_Methods.AddToRowsDefinitionsOfGrid(ref grid, nameRow, templateRow);
            for (int i = 0; i < Variables.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                Grid variableGrid;
                if (i != Variables.Count - 1)
                    variableGrid = Variables[i].GetFunctionGrid(false);
                else
                    variableGrid = Variables[i].GetFunctionGrid(true);
                WPF_Methods.AddToGrid(ref grid, (variableGrid, 0, i + 2));
            }
            if (Variables.Count == 0)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                WPF_Methods.AddToGrid(ref grid, (Variable.GetTemplateOfFunctionGrid("-", "-", "-", "-", true, false), 0, 2));
            }
            TextBox nameTextBox = new TextBox() { TextWrapping = TextWrapping.Wrap, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, FontSize = Data.CaptionTextFontSize, IsReadOnly = true, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            if (caption == null)
            {
                nameTextBox.Text = "Функция " + Name.Text;
                if (IsOnlyForOutputting)
                    nameTextBox.Text += " (только для вывода)";
            }
            else
                nameTextBox.Text = caption;
            WPF_Methods.AddToGrid(ref grid, (nameTextBox, 0, 0), (Variable.GetTemplateOfFunctionGrid(false), 0, 1));
            return grid;
        }

        public override bool Equals(object obj) => Name.Equals(obj);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name.ToString();
        #endregion

        #region Operators
        public static bool operator ==(Function first, Function second)
        {
            try
            {
                return first.Name == second.Name;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool operator !=(Function first, Function second)
        {
            try
            {
                return first.Name != second.Name;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
