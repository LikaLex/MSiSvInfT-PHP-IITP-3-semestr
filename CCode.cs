using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;

namespace MSiSvInfT
{
    using Split = SplitMethods;
    using Line = LineAnalyseMethods;
    using Brackets = BracketsMethods;

    public class Code
    {
        #region Fields
        private string[] lines;
        private Word[][] linesInWords;
        private bool isCodeNormalized = false;
        #endregion

        #region Events
        public event EventHandler StartCodeChanging;
        public event EventHandler EndCodeChanging;
        #endregion

        #region Properties      
        public RichTextBox RTBCode { get; protected set; }

        public string[] Lines
        {
            get { return lines; }
            set
            {
                isCodeNormalized = false;
                lines = value;

                try { DeleteEmptyLinesAtTheEnd(ref lines); }
                catch (Exception) { throw new Exception("Не удалось удалить пустые строки в конце файла"); }

                try { CombineLinesWithRoundBrackets(); }
                catch(Exception) { throw new Exception("Число открывающих и закрывающих круглых скобок не совпадает"); }

                try { SplitLinesBySemicolon(); }
                catch (Exception) { throw new Exception("Не удалось выделить строки, оканчивающиеся \';\'"); }

                try { CurveBracketsToSingleLine(); }
                catch (Exception) { throw new Exception("Не удалось вынести фигурные скобки в отдельные строки"); }

                try { CASE_ToSingleLine(); }
                catch (Exception) { throw new Exception("Не удалось отформатировать блоки \'case\' оператора \'switch\'"); }

                try { CarryFromConditionToNewLine(); }
                catch (Exception) { throw new Exception("Не удалось разделить строки, содержащие условные операторы"); }

                try { AddCurveBracketsTo_DO_WHILE(); }
                catch (Exception) { throw new Exception("Не удалось добавить фигурные скобки к телу цикла \'do while\'"); }

                try { AddCurveBrackets(); }
                catch (Exception) { throw new Exception("Не удалось добавить недостающие фигурные скобки"); }

                try { MakeTabulation(); }
                catch (Exception) { Data.Errors.Add("Не удалось произвести табуляцию строк"); }

                try { SplitLines(); }
                catch (Exception) { throw new Exception("Не удалось разбить строки на слова"); }
            }
        }      
        #endregion

        #region Indexators
        public Word[] this[int lineIndex]
        {
            get
            {
                if (!isCodeNormalized)
                    Data.ThrowError("Код не отформатирован");
                return linesInWords[lineIndex];
            }
        }
        #endregion

        #region Constructor
        public Code()
        {
            RTBCode = new RichTextBox() { FontSize = Data.CodeFontSize, Background = Data.CodeBackground, BorderThickness = new Thickness(3), Width = 2000, Cursor = Cursors.Arrow, FontFamily = new FontFamily("Consolas"), IsReadOnly = true, AcceptsTab = true };
            RTBCode.MouseDoubleClick += RTBCode_MouseDoubleClick;
        }
        #endregion

        #region Methods   
        public static void DeleteEmptyLinesAtTheEnd(ref string[] _lines)
        {
            int emptyLinesCount = 0;
            for (int i = _lines.Length - 1; i >= 0; i--)
            {
                if (Line.TrimmedLine(_lines[i]) == "")
                    emptyLinesCount++;
                else
                    break;
            }
            Array.Resize(ref _lines, _lines.Length - emptyLinesCount);
        }

        private void SplitLines()
        {
            linesInWords = new Word[Lines.Length][];
            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Line.TrimmedLine(Lines[i]);
                int singleLineCommentStart = line.IndexOf(Data.SINGLELINE_COMMENT.Text);
                if (singleLineCommentStart >= 0)
                    line = line.Substring(0, singleLineCommentStart);
                int multiLineCommentStart = line.IndexOf(Data.MULTILINE_COMMENT_Start.Text);
                if (multiLineCommentStart >= 0)
                {
                    SkipMultiLineComment(ref i, multiLineCommentStart, line);
                    continue;
                }
                linesInWords[i] = Split.SeparateLine(line);
                Word.TrimWords(ref linesInWords[i]);
            }
            isCodeNormalized = true;
        }

        private void SkipMultiLineComment(ref int i, int multiLineCommentStart, string line)
        {
            line = line.Substring(0, multiLineCommentStart);
            linesInWords[i] = Split.SeparateLine(line);
            int multiLineCommentEnd;
            do
            {
                i++;
                multiLineCommentEnd = lines[i].IndexOf(Data.MULTILINE_COMMENT_End.Text);
                linesInWords[i] = new Word[0];
            } while (multiLineCommentEnd == -1);
            line = Line.TrimmedLine(lines[i]);
            linesInWords[i] = Split.SeparateLine(line.Substring(multiLineCommentEnd + Data.MULTILINE_COMMENT_End.Text.Length));
        }        

        private void CombineLinesWithRoundBrackets()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                while (Line.CharCount(lines[i], '(') != Line.CharCount(lines[i], ')'))
                {
                    lines[i] += lines[i + 1];
                    Split.Remove(ref lines, i + 1);
                }
            }
        }

        private void SplitLinesBySemicolon()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd(' ', '\t');
                if (lines[i].Contains(';'))
                {
                    int position = lines[i].IndexOf(';');
                    if (position != lines[i].Length - 1)
                        if (!Line.IsPositionInRoundBrackets(lines[i], position)
                            && !Line.IsPositionInQuotes(lines[i], position)
                            && !lines[i].Contains("//"))
                        {
                            string[] parts = new string[2];
                            parts[0] = lines[i].Substring(0, position + 1);
                            parts[1] = lines[i].Substring(position + 1);
                            Split.InsertInsteadOf(i, ref lines, parts);
                        }
                }
            }
        }

        private void CarryFromConditionToNewLine()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string offset = "";
                if (i != 0)
                    offset = Line.GetOffset(lines[i - 1]);

                Word[] words = Split.SeparateLine(Line.TrimmedLine(lines[i]));
                Word.TrimWords(ref words);
                if (words.Length == 0)
                    continue;
                if (Word.Contains(words, Data.DO))
                {
                    string[] parts = new string[2];
                    int DO_Position = Word.IndexOfFirst(words, Data.DO);
                    parts[0] = offset + Word.ToLine(words, 0, DO_Position);
                    parts[1] = Word.ToLine(words, DO_Position + 1);
                    if (Line.TrimmedLine(parts[1]) == "")
                        Array.Resize(ref parts, 1);
                    Split.InsertInsteadOf(i, ref lines, parts);
                }
                else if (Word.ContainsBlockOperator(words))
                {
                    if (words[0].Text == Data.WHILE.Text)
                    {
                        int closeRoundBracket = Brackets.GetCloseBracketPosition(words, '(');
                        if (closeRoundBracket != words.Length - 1 && words[closeRoundBracket + 1].Text == ";")
                            continue;
                    }
                    int closeBracketPosition = Brackets.GetCloseBracketPosition(words, '(');
                    string[] parts = new string[2];
                    parts[0] = offset + Word.ToLine(words, 0, closeBracketPosition);
                    parts[1] = offset + "\t" + Word.ToLine(words, closeBracketPosition + 1);
                    if (Line.TrimmedLine(parts[1]) == "")
                        continue;
                    Split.InsertInsteadOf(i, ref lines, parts);
                }
                else if (words[0].Text == Data.ELSE.Text)
                {
                    string[] parts = new string[2];
                    parts[0] = offset + Data.ELSE.Text;
                    parts[1] = offset + "\t" + Word.ToLine(words, 1);
                    if (Line.TrimmedLine(parts[1]) == "")
                        continue;
                    Split.InsertInsteadOf(i, ref lines, parts);
                }
            }
        }

        #region Tabulation

        public string GetTabulation(int lineIndex) => Line.GetOffset(Lines[lineIndex]);

        #region SWITCH Tabulation
        private void TabulateSwitch(ref int i, string startOffset)
        {
            int start = i;
            int end = Brackets.GetCloseCurveBracketIndex(lines, start);
            for (i = i + 1; i < end; i++)
            {
                string trimmedCurrent = Line.TrimmedLine(lines[i]);
                string trimmedPrevious = Line.TrimmedLine(lines[i - 1]);
                if (Line.Is_SWITCH_Label(trimmedCurrent))
                    lines[i] = startOffset + trimmedCurrent;
                else if (Line.Is_SWITCH_Label(trimmedPrevious))
                    lines[i] = startOffset + "\t" + trimmedCurrent;
                else
                    TabulateLine(ref i);
            }
        }

        private void CombineLines(ref int i)
        {
            while (!lines[i].Contains(Data.COLON.Text))
            {
                if (lines[i][lines[i].Length - 1] != ' ')
                    lines[i] += " ";
                lines[i] += Line.TrimmedLine(lines[i + 1]);
                Split.Remove(ref lines, i + 1);
            }
            i--;
        }

        private void Split_CASE_Or_DEFAULT(Word[] words, int j)
        {
            int endIndex = Word.GetWordWithColonIndex(words);
            if (endIndex != words.Length - 1)
            {
                string[] parts = new string[2];
                parts[0] = Word.ToLine(words, 0, endIndex);
                parts[1] = Line.GetOffset(parts[0]) + "\t" + Word.ToLine(words, endIndex + 1);
                Split.InsertInsteadOf(j, ref lines, parts);
            }
        }

        private void CASE_ToSingleLine()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                Word[] words = Split.SeparateLine(lines[i]);
                Word.TrimWords(ref words);
                if (words.Length == 0)
                    continue;
                int nested_SWITCH = 0;
                bool wasNestedSwitch = false;
                if (Line.TrimmedLine(Word.FirstNotEmptyWord(words).Text) == Data.SWITCH.Text)
                {
                    i++;
                    int SWITCH_End = Brackets.GetCloseCurveBracketIndex(lines, i);
                    for (int j = i + 1; j < SWITCH_End; j++)
                    {
                        words = Split.SeparateLine(lines[j]);
                        string firstWord;
                        try
                        {
                            firstWord = Line.TrimmedLine(Word.FirstNotEmptyWord(words).Text);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        if (firstWord == Data.CASE.Text)
                        {
                            if (Word.IndexOfFirst(words, Data.CASE) == words.Length - 1)
                            {
                                CombineLines(ref j);
                                break;
                            }
                            else
                                Split_CASE_Or_DEFAULT(words, j);
                        }
                        else if (firstWord == Data.DEFAULT.Text)
                        {
                            if (Word.IndexOfFirst(words, Data.DEFAULT) == words.Length - 1)
                            {
                                CombineLines(ref j);
                                break;
                            }
                            else
                                Split_CASE_Or_DEFAULT(words, j);
                        }
                        else if (firstWord == Data.DEFAULT_WithColon.Text)
                        {
                            if (Word.IndexOfFirst(words, Data.DEFAULT_WithColon) == words.Length - 1)
                            {
                                CombineLines(ref j);
                                break;
                            }
                            else
                                Split_CASE_Or_DEFAULT(words, j);
                        }
                        else if ((firstWord == Data.SWITCH.Text || firstWord.Contains(Data.SWITCH_WithBracket.Text)) && !wasNestedSwitch)
                        {
                            nested_SWITCH = j;
                            wasNestedSwitch = true;
                        }
                    }
                    i = SWITCH_End;
                    if (wasNestedSwitch)
                        i = nested_SWITCH;
                }
            }
        }
        #endregion

        private void TabulateLine(ref int i)
        {
            string previousOffset = Line.GetOffset(lines[i - 1]);
            string trimmedPreviousLine = Line.TrimmedLine(lines[i - 1]);
            string trimmedCurrent = Line.TrimmedLine(lines[i]);
            if (Line.IsStartsWith(trimmedPreviousLine, Data.SWITCH.Text))
            {
                TabulateSwitch(ref i, previousOffset);
                return;
            }
            if (trimmedPreviousLine == "{")
                lines[i] = previousOffset + "\t" + trimmedCurrent;
            else if (trimmedCurrent == "}")
            {
                string newOffset = String.Copy(previousOffset);
                try
                {
                    newOffset = newOffset.Remove(newOffset.Length - 1, 1);
                }
                catch (Exception)
                {
                    newOffset = "";
                }
                lines[i] = newOffset + trimmedCurrent;
            }
            else
                lines[i] = previousOffset + trimmedCurrent;
        }

        private void MakeTabulation()
        {
            lines[0] = Line.TrimmedLine(lines[0]);
            for (int i = 1; i < lines.Length; i++)
                TabulateLine(ref i);
            for (int i = 0; i < lines.Length; i++)
            {
                if (Lines[i].Contains(Data.SWITCH.Text + " ") || Lines[i].Contains(Data.SWITCH_WithBracket.Text))
                {
                    string offset = Line.GetOffset(Lines[i]);
                    Lines[i + 1] = offset + Line.TrimmedLine(Lines[i + 1]);
                    int closeBracket = Brackets.GetCloseCurveBracketIndex(Lines, i + 1);
                    Lines[closeBracket] = offset + Line.TrimmedLine(Lines[closeBracket]);
                }
            }
        }
        #endregion

        #region CurveBrackets
        private bool IsSingleLineBlock(int lineNumber)
        {
            if (Line.TrimmedLine(lines[lineNumber + 1]) == "{")
                return false;
            else
                return true;
        }

        private void _AddCurveBrackets(ref int i, ref int start)
        {
            string offset = Line.GetOffset(lines[i]);
            string[] parts = new string[3];
            parts[0] = lines[i];
            parts[1] = offset + "{";
            i++;
            start = i;
            parts[2] = offset + "\t" + Line.TrimmedLine(lines[i]);
            Split.Remove(ref lines, i);

            Split.InsertInsteadOf(i - 1, ref lines, parts);

            i++;
            int IF_Without_ELSE = 0;
            Word[] words = Split.SeparateLine(Line.TrimmedLine(lines[i]));
            while (Word.IsBlock(words))
            {
                if (Word.FirstNotEmptyWord(words).Text == Data.IF.Text)
                    IF_Without_ELSE++;
                if (IsSingleLineBlock(i))
                    i++;
                else
                    i = Brackets.GetCloseCurveBracketIndex(lines, i + 1);
                words = Split.SeparateLine(Line.TrimmedLine(lines[i]));
            }
            bool wasSingleLine_ELSE = false;
            for (; IF_Without_ELSE > 0; IF_Without_ELSE--)
            {
                if (Line.TrimmedLine(lines[i + 1]) == Data.ELSE.Text)
                {
                    if (IsSingleLineBlock(i))
                    {
                        i++;
                        wasSingleLine_ELSE = true;
                    }
                    else
                    {
                        i = Brackets.GetCloseCurveBracketIndex(lines, i + 1);
                        wasSingleLine_ELSE = false;
                    }
                }
            }
            if (wasSingleLine_ELSE)
                i++;
            parts[0] = lines[i];
            parts[1] = offset + "}";
            parts[2] = lines[i + 1];
            Split.Remove(ref lines, i + 1);
            Split.InsertInsteadOf(i, ref lines, parts);
            i = start;
        }

        private void AddCurveBracketsTo_DO_WHILE()
        {
            int start = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                Word[] words = Split.SeparateLine(Line.TrimmedLine(lines[i]));
                if (words.Length > 0 && words[0].Text == Data.DO.Text)
                {
                    if (Line.TrimmedLine(lines[i + 1]) != "{")
                        _AddCurveBrackets(ref i, ref start);
                }
            }
        }

        private void AddCurveBrackets()
        {
            int start = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                Word[] words = Split.SeparateLine(Line.TrimmedLine(lines[i]));
                if (Word.IsBlock(words))
                {
                    if (IsSingleLineBlock(i))
                        _AddCurveBrackets(ref i, ref start);
                }
                else
                {
                    Word first;
                    try
                    {
                        first = Word.FirstNotEmptyWord(words);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        continue;
                    }
                    if (first.Text == Data.ELSE.Text && Line.TrimmedLine(lines[i + 1]) != "{")
                        _AddCurveBrackets(ref i, ref start);
                }
            }
        }

        private void SplitBySeparator(string separator, int lineIndex)
        {
            if (Line.TrimmedLine(lines[lineIndex]) == separator)
                return;           
            string previousLine = null;
            if (lineIndex != 0)
                previousLine = lines[lineIndex - 1];
            if (lines[lineIndex].Contains(separator))
                Split.InsertInsteadOf(lineIndex, ref lines, Split.DistinguishSeparator(separator, lines[lineIndex], previousLine));
        }

        private void CurveBracketsToSingleLine()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (Line.TrimmedLine(Lines[i]).IndexOf(Data.SINGLELINE_COMMENT.Text) == 0)
                    continue;
                SplitBySeparator("{", i);
                SplitBySeparator("}", i);
            }
        }
        #endregion

        private void SetRTBCode(string[] _lines)
        {
            double offset = RTBCode.VerticalOffset;
            RTBCode.Document.Blocks.Clear();
            Paragraph paragraph = new Paragraph() { Margin = Data.NullThickness };
            for (int i = 0; i < _lines.Length; i++)
                paragraph.Inlines.Add(_lines[i] + "\n");
            RTBCode.Document = new FlowDocument(paragraph);
            RTBCode.ScrollToVerticalOffset(offset);
        }

        public void LoadFromFileForChanging(string fileName)
        {
            string[] fileLines = File.ReadAllLines(fileName);
            SetRTBCode(fileLines);
            StartCodeChanging?.Invoke(null, null);
            RTBCode.IsReadOnly = false;
            RTBCode.Cursor = Cursors.IBeam;
        }        

        #region Events Handlers
        private void RTBCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RTBCode.IsReadOnly)
            {
                SetRTBCode(Lines);
                StartCodeChanging?.Invoke(sender, e);
                RTBCode.IsReadOnly = false;
                RTBCode.Cursor = Cursors.IBeam;
            }
            else
                EndCodeChanging?.Invoke(sender, e);
        }
        #endregion
        #endregion
    }
}
