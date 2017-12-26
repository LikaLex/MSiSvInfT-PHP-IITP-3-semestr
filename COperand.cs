using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSiSvInfT
{
    public interface ILocationable
    {
        List<Location> Locations { get; set; }
    }

    public class CodeElement: ILocationable, IComparable<CodeElement>
    {
        #region Properties
        public Word Name { get; set; }
        public List<Location> Locations { get; set; }
        #endregion

        #region Constructors
        public CodeElement(Word name, int lineIndex, int wordIndex) : this(name, new Location(lineIndex, wordIndex)) { }

        public CodeElement(Word name, Location location)
        {
            Name = name;
            Locations = new List<Location>() { location };
        }
        #endregion

        #region Methods
        public int CompareTo(CodeElement other)
        {
            int othersLocationsAmount = other.Locations.Count;
            if (Locations.Count > othersLocationsAmount)
                return -1;
            else if (Locations.Count < othersLocationsAmount)
                return 1;
            else return 0;
        }

        public override string ToString() => Name.Text;
        #endregion
    }

    public class Operand: CodeElement
    {
        #region Constructors
        public Operand(Word name, int lineIndex, int wordIndex) : base(name, new Location(lineIndex, wordIndex)) { }
        public Operand(Word name, Location location): base(name, location) { }
        #endregion

        #region Methods
        public Grid GetOperandGrid(bool needsBottomBorder, int number, double fontSize)
        {
            Grid grid = new Grid();
            ThirdTask.MarkupOperatorOrOperandGrid(ref grid, needsBottomBorder);

            TextBox numberTextBox = new TextBox() { Text = (number + 1).ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };
            TextBox operandTextBox = new TextBox() { Text = Name.Text, Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true };
            TextBox f2iTextBox = new TextBox() { Text = Locations.Count.ToString(), Background = Data.ApplicationBackground, BorderThickness = Data.NullThickness, TextWrapping = TextWrapping.Wrap, FontSize = fontSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsReadOnly = true, Cursor = Cursors.Hand };

            WPF_Methods.SetToolTip(ref f2iTextBox, WPF_Methods.CreateToolTip("Позиции операнда", ThirdTask.GetLocations(this)));

            WPF_Methods.AddToGrid(ref grid, (numberTextBox, 1, 1), (operandTextBox, 3, 1), (f2iTextBox, 5, 1));
            ThirdTask.MakeBordersInOperatorOrOperandGrid(ref grid, needsBottomBorder);
            return grid;
        }
        #endregion
    }
}
