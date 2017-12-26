using System;
using System.Collections.Generic;
using System.Linq;

namespace MSiSvInfT
{
    using Line = LineAnalyseMethods;

    public static class SplitMethods
    {
        #region Methods
        public static string[] WordByOperator(string word, string @operator)
        {
            string[] parts = new string[2];
            int operatorStart = word.IndexOf(@operator);
            parts[0] = word.Substring(0, operatorStart);
            int secondPartStart = operatorStart + @operator.Length;
            parts[1] = word.Substring(secondPartStart, word.Length - secondPartStart);
            return parts;
        }

        public static void LineByComment(string codeLine, out string line, out string comment, int commentStart)
        {
            line = codeLine.Substring(0, commentStart);
            comment = codeLine.Substring(commentStart);
        }

        public static string Offset(string line, string previousLine)
        {
            if (previousLine == null)
                previousLine = "";
            string usual = Line.GetOffset(previousLine);
            if (Line.TrimmedLine(previousLine) == "{")
                return usual + "\t";
            string trimmedLine = Line.TrimmedLine(line);
            if (trimmedLine == "}" || trimmedLine.IndexOf(Data.ELSE.Text) == 0)
                try
                {
                    return usual.Remove(usual.Length - 1, 1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return usual;
                }
            return usual;
        }

        public static string[] DistinguishSeparator(string separator, string line, string previousLine)
        {
            int separatorStart = line.IndexOf(separator);

            string[] parts = new string[3];
            parts[0] = line.Substring(0, separatorStart);
            parts[1] = line.Substring(separatorStart, separator.Length);
            parts[2] = line.Substring(separatorStart + separator.Length);

            string[] offsets = new string[3];
            if (previousLine != null)
                offsets[0] = Line.GetOffset(previousLine);
            else
                offsets[0] = "";

            if (parts[2] == "")
                Remove(ref parts, 2);
            if (parts[0] == "")
                Remove(ref parts, 0);
            parts[0] = Offset(parts[0], previousLine) + parts[0].TrimStart('\t', ' ');
            for (int i = 1; i < parts.Length; i++)
                parts[i] = Offset(parts[i], parts[i - 1]) + parts[i].TrimStart('\t', ' ');
            return parts;
        }

        private static bool BySyllable(string syllable, int index, ref Word[] words)
        {
            if (words[index].Text == syllable)
            {
                words[index].CanSplit = false;
                words[index].IsSpaceAfter = true;
                return true;
            }
            if (!words[index].Text.Contains(syllable))
                return false;
            Word[] parts = new Word[3];
            int position = words[index].Text.IndexOf(syllable);
            parts[0] = new Word(words[index].Text.Substring(0, position), true, true);
            parts[1] = new Word(syllable, true, false);
            parts[2] = new Word(words[index].Text.Substring(position + syllable.Length), words[index].IsSpaceAfter, true);
            InsertInsteadOf(index, ref words, parts);

            SeparateWord(index + 2, ref words);
            SeparateWord(index, ref words);
            return true;
        }

        public static void InsertInsteadOf<T>(int position, ref T[] array, params T[] items)
        {
            Array.Resize(ref array, array.Length + items.Length - 1);
            for (int i = array.Length - items.Length; i > position; i--)
                array[i + items.Length - 1] = array[i];
            for (int i = 0; i < items.Length; i++)
                array[position + i] = items[i];
        }

        public static bool IsTag(string word)
        {
            if (word.Contains(Data.TAG_Start.Text) || word.Contains(Data.TAG_End.Text))
                return true;
            return false;
        }

        private static void AddSpaces(string @operator, int index, ref Word[] words)
        {
            if (@operator != Data.INCREMENT.Text && @operator != Data.DECREMENT.Text)
            {
                if (index != 0)
                {
                    int previous = index - 1;
                    if (words[previous].Text != "(" && words[previous].Text != "[" && words[previous].Text != "{")
                        words[previous].IsSpaceAfter = true;
                }
                if (index != words.Length - 1)
                {
                    int next = index + 1;
                    if (words[next].Text != "(" && words[next].Text != "[" && words[next].Text != "{")
                        words[index].IsSpaceAfter = true;
                }
            }
        }

        private static void SeparateWord(int index, ref Word[] words)
        {
            if (!words[index].CanSplit || IsTag((string)words[index]))
                return;

            foreach (string sequence in Data.EscapeSequences)
                if (BySyllable(sequence, index, ref words))
                    return;

            foreach (string character in Data.Characters)
                if (BySyllable(character, index, ref words))
                    return;

            foreach (string bracket in Data.Brackets)
                if (BySyllable(bracket, index, ref words))
                    return;

            foreach (string[] operators in Operators.Symbolic)
                foreach (string @operator in operators)
                    if (BySyllable(@operator, index, ref words))
                    {
                        AddSpaces(@operator, index, ref words);
                        return;
                    }
        }

        public static void Remove<T>(ref T[] array, int index)
        {
            for (int j = index + 1; j < array.Length; j++)
                array[j - 1] = array[j];
            Array.Resize(ref array, array.Length - 1);
        }

        private static void RemoveEmptyWords(ref Word[] words)
        {
            for (int i = 0; i < words.Length - 1; i++)
                if (words[i] == "")
                    Remove(ref words, i);
            if (words[words.Length - 1] == "")
                Array.Resize(ref words, words.Length - 1);
        }

        public static Word[] SeparateLine(string _line)
        {
            string line;
            int commentIndex = _line.IndexOf("//");
            if (commentIndex != -1)
                LineByComment(_line, out line, out string comment, commentIndex);
            else
                line = _line;
            string[] _words = line.Split(' ');
            Word[] words = new Word[_words.Length];
            for (int i = 0; i < _words.Length; i++)
                words[i] = new Word(_words[i], true, true);
            for (int i = 0; i < words.Length; i++)
                SeparateWord(i, ref words);
            RemoveEmptyWords(ref words);
            return words;
        }

        public static List<int> QuotesPositions(Word[] line)
        {
            List<int> positions = new List<int>();
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].Text == "\"")
                {
                    if (i != 0)
                    {
                        if (line[i - 1].Text != "\\")
                            positions.Add(i);
                    }
                    else
                        positions.Add(i);
                }
            }
            return positions;
        }

        private static List<int> QuotesPositions(string line)
        {
            List<int> positions = new List<int>();
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\"')
                {
                    if (i != 0)
                    {
                        if (line[i - 1] != '\\')
                            positions.Add(i);
                    }
                    else
                        positions.Add(i);
                }
            }
            return positions;
        }

        private static bool IsEven(int i)
        {
            if (i % 2 == 0)
                return true;
            else
                return false;
        }

        public static (Word[] Part, bool IsInQuotes)[] SeparateLineWithQuotes(int lineIndex)
        {
            Word[] words = Data.Code[lineIndex];
            if (!Word.Contains(words, Data.QUOTE))
                return new (Word[] Part, bool IsInQuotes)[] { (words, false) };
            List<int> quotesPositions = QuotesPositions(words);
            (Word[] Part, bool IsInQuotes)[] parts = new(Word[] Part, bool IsInQuotes)[quotesPositions.Count + 1];
            parts[0] = (Word.SubArray(words, 0, quotesPositions[0] - 1), false);
            for (int i = 0; i < quotesPositions.Count - 1; i++)
            {
                parts[i + 1] = (Word.SubArray(words, quotesPositions[i] + 1, quotesPositions[i + 1] - 1), IsEven(i));
                Word.TrimWords(ref parts[i + 1].Part);
            }
            int start = quotesPositions.Last() + 1;
            Word[] lastPart = Word.SubArray(words, start); 
            parts[parts.Length - 1] = (lastPart, !(parts[parts.Length - 2].IsInQuotes));
            return parts;
        }

        public static (string Part, bool IsInQuotes)[] SeparateLineWithQuotes(string line)
        {
            if (!line.Contains(Data.QUOTE.Text))
                return new (string Part, bool IsInQuotes)[] { (line, false) };
            List<int> quotesPositions = QuotesPositions(line);
            (string Part, bool IsInQuotes)[] parts = new (string Part, bool IsInQuotes)[quotesPositions.Count + 1];
            parts[0] = (line.Substring(0, quotesPositions[0] - 1), false);
            for (int i = 0; i < quotesPositions.Count - 1; i++)
            {
                int startIndex = quotesPositions[i] + 1;
                int endIndex = quotesPositions[i + 1] - 1;
                string substring = line.Substring(startIndex, endIndex - startIndex + 1);
                parts[i + 1] = (substring, IsEven(i));
            }
            int start = quotesPositions.Last() + 1;
            string lastPart = line.Substring(start);
            parts[parts.Length - 1] = (lastPart, !(parts[parts.Length - 2].IsInQuotes));
            return parts;
        }

        public static Word[][] GetLeftPartsOfTernaryOperators(Word[] words)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < words.Length; i++)
                if (words[i].Text == Data.QUESTION_MARK.Text)
                    indexes.Add(i);
            Word[][] parts = new Word[indexes.Count][];
            for (int i = 0; i < indexes.Count; i++)
            {
                int leftBorder = indexes[i];
                while (leftBorder > 0)
                {
                    leftBorder--;
                    if (words[leftBorder].Text == Data.QUESTION_MARK.Text || words[leftBorder].Text == Data.COLON.Text)
                        break;
                }
                int start = leftBorder == 0 ? 0 : leftBorder + 1;
                parts[i] = new Word[indexes[i] - start];
                for (int j = start, k = 0; j < indexes[i]; j++, k++)
                    parts[i][k] = words[j];
            }
            return parts;
        }

        private static int GetColonIndex(Word[] words)
        {
            int? colons = null;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Text == Data.QUESTION_MARK.Text)
                {
                    if (colons.HasValue)
                        colons++;
                    else
                        colons = 1;
                }
                if (words[i].Text == Data.COLON)
                    colons--;
                if (colons.HasValue && colons.Value == 0)
                    return i;
            }
            Data.ThrowError("Тернарные операторы вложены неправильно");
            return 0;
        }

        public static Word[][] GetPartsOfTernaryOperator(Word[] words)
        {           
            Word.Contains(words, Data.QUESTION_MARK, out int questionMarkIndex);
            int colonIndex = GetColonIndex(words);
            return new Word[3][]
            {
                Word.SubArray(words, 0, questionMarkIndex - 1),
                Word.SubArray(words, questionMarkIndex + 1, colonIndex - 1),
                Word.SubArray(words, colonIndex + 1)
            };
        }
        #endregion
    }
}
