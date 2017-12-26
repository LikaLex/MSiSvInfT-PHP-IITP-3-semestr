using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSiSvInfT
{
    public enum OperatorType
    {
        NoOperands,
        OperandMayBeInBrackets,
        CanBeOneOperand,
        Unary_OperandAtLeft,
        Unary_OperandAtRight,
        Unary_OperandAtRightMayBeInBrackets,
        Binary,
        Ternary,
        Function,
        NotDefined
    }

    public class Operator: CodeElement
    {
        #region Properties
        public OperatorType Type { get; set; }
        #endregion

        #region Constructors
        public Operator(Word name, Location firstLocation): base (name, firstLocation)
        {
            SetType();           
        }
        
        public Operator(Word name, Location firstLocation, OperatorType type): base(name, firstLocation)
        {
            Type = type;
        }
        #endregion

        #region Methods            
        private void SetType()
        {
            if (IsFunction())
            {
                Type = OperatorType.Function;
                return;
            }
            switch (Name.Text)
            {
                case "break":
                case "continue":
                case "goto":
                case "for":
                case ";":
                case "(":
                case "@":
                case "foreach":
                case "declare":
                    Type = OperatorType.NoOperands;
                    break;
                case "if":
                case "switch":
                case "while":
                case "do":
                    Type = OperatorType.OperandMayBeInBrackets;
                    break;
                case "return":
                    Type = OperatorType.CanBeOneOperand;
                    break;   
                case "!":
                case "~":
                case "instanceof":                
                    Type = OperatorType.Unary_OperandAtRight;
                    break;
                case "+":
                case "-":
                case "++":
                case "--":
                    Type = OperatorType.NotDefined;
                    break;
                case "echo":
                case "include":
                case "require":
                case "include_once":
                case "require_once":
                    Type = OperatorType.Unary_OperandAtRightMayBeInBrackets;
                    break;
                case "?":
                    Type = OperatorType.Ternary;
                    break;                
                default:
                    Type = OperatorType.Binary;
                    break;
            }
        }

        private bool IsFunction()
        {
            Word[] names = SecondTask.GetFunctionsNames(SecondTask.Functions);
            foreach (Word name in names)
                if (name.Text == Name.Text)
                    return true;
            names = SecondTask.GetFunctionsNames(SecondTask.SystemFunctions);
            foreach (Word name in names)
                if (name.Text == Name.Text)
                    return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                if (this == null)
                    return true;
                else
                    return false;
            }
            else
            {
                try
                {
                    Operator @operator = (Operator)obj;
                    return this == @operator;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }
        }

        public override int GetHashCode() => Name.GetHashCode();

        public string GetOperatorName()
        {
            string operatorName = Name.Text;
            if (Name.Text == Data.QUESTION_MARK.Text)
                operatorName += ":";
            else if (Name.Text == Data.DO.Text)
                operatorName += " while";
            else if (Name.Text == "(")
                operatorName += " )";
            else if (Name.Text == "[")
                operatorName += " ]";
            return operatorName;
        }

        public Grid GetOperatorGrid(bool needsBottomBorder, int number, double fontSize)
        {
            Grid grid = new Grid();
            ThirdTask.MarkupOperatorOrOperandGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = (number + 1).ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };
            TextBox operatorTextBox = new TextBox() { Text = GetOperatorName(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };
            TextBox f1jTextBox = new TextBox() { Text = Locations.Count.ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true, Cursor = Cursors.Hand };

            WPF_Methods.SetToolTip(ref f1jTextBox, WPF_Methods.CreateToolTip("Позиции оператора", ThirdTask.GetLocations(this)));

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (operatorTextBox, 3, 1), (f1jTextBox, 5, 1));
            ThirdTask.MakeBordersInOperatorOrOperandGrid(ref grid, needsBottomBorder);
            return grid;
        }
        #endregion

        #region Operators
        public static bool operator ==(Operator first, Operator second)
        {
            if (first == null && second == null)
                return true;
            if (first == null || second == null)
                return false;
            if (first.Name.Text == second.Name.Text && first.Type == second.Type)
                return true;
            else
                return false;
        }
        public static bool operator !=(Operator first, Operator second) => !(first == second);
        #endregion
    }
}
