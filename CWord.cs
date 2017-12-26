using System;
using System.Collections.Generic;
using System.Linq;

namespace MSiSvInfT
{
    using Line = LineAnalyseMethods;
    using Split = SplitMethods;

    public class Word
    {
        #region Properties
        public string Text { get; set; }
        public bool IsSpaceAfter { get; set; }
        public bool CanSplit { get; set; }
        #endregion

        #region Constructors
        public Word(string text, bool isSpaceAfter, bool canSplit)
        {
            Text = text;
            IsSpaceAfter = isSpaceAfter;
            CanSplit = canSplit;
        }

        public Word(char symbol): this(symbol.ToString(), false, false) { }
        #endregion

        #region Methods
        public override string ToString()
        {
            if (IsSpaceAfter)
                return Text + " ";
            else
                return Text;
        }

        #region Words
        public static int IndexOfFirst(Word[] words,Word word)
        {
            for (int i = 0; i < words.Length; i++)
                if (Line.TrimmedLine(words[i].Text) == word.Text)
                    return i;
            throw new ArgumentException("\"" + word + "\" не содержится в массиве");
        }

        public static string ToLine(Word[] words, int firstIndex, int lastIndex)
        {
            string result = "";
            for (int i = firstIndex; i <= lastIndex; i++)
            {
                result += words[i];
                if (words[i].IsSpaceAfter && result[result.Length - 1] != ' ')
                    result += " ";             
            }
            return result;
        }
        public static string ToLine(Word[] words, int firstIndex) => ToLine(words, firstIndex, words.Length - 1);

        public static bool Contains(Word[] words, Word word)
        {
            foreach (Word _word in words)
                if (_word.Text == word.Text)
                    return true;
            return false;
        }

        public static bool Contains(Word[] words, Word word, out int firstIndex)
        {
            for (int i = 0; i < words.Length; i++)
                if (words[i].Text == word.Text)
                {
                    firstIndex = i;
                    return true;
                }
            firstIndex = -1;
            return false;
        }

        public static List<int> AllIndexesOfContainings(Word[] words, Word word)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < words.Length; i++)
                if (word.Text == words[i].Text)
                    indexes.Add(i);
            return indexes;
        }

        public static bool ContainsAny(Word[] givenWords, Word[] wantedWords, out int indexOfFirstMetWord)
        {
            for (int i = 0; i < givenWords.Length; i++)
                foreach (Word word in wantedWords)
                    if (givenWords[i] == word)
                    {
                        indexOfFirstMetWord = i;
                        return true;
                    }
            indexOfFirstMetWord = -1;
            return false;
        }

        public static bool IsBlock(Word[] words)
        {
            try
            {
                if (LastNotEmptyWord(words).Text != Data.SEMICOLON.Text)
                    return ContainsBlockOperator(words);
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }    
        }

        public static int GetWordWithColonIndex(Word[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Text.Contains(Data.COLON.Text))
                    return i;
            }
            throw new ArgumentException();
        }

        public static bool ContainsBlockOperator(Word[] words)
        {
            TrimWords(ref words);
            if (words.Length == 0)
                return false;
            foreach (string @operator in Data.BlockOperators)
                if (words[0].Text == @operator)
                    return true;
            return false;
        }
     
        public static Word FirstNotEmptyWord(Word[] words, int start)
        {
            if (start >= words.Length)
                throw new ArgumentOutOfRangeException("Массив слов короче указанной длины");
            for (int i = start; i < words.Length; i++)
                if (Line.TrimmedLine(words[i].Text) != "")
                    return words[i];
            throw new ArgumentOutOfRangeException("Массив слов является пустым");
        }
        public static Word FirstNotEmptyWord(Word[] words) => FirstNotEmptyWord(words, 0);

        public static Word LastNotEmptyWord(Word[] words, int start)
        {
            if (start >= words.Length)
                throw new ArgumentOutOfRangeException("Массив слов короче указанной длины");
            for (int i = start; i >= 0; i--)
                if (Line.TrimmedLine(words[i].Text) != "")
                    return words[i];
            throw new ArgumentOutOfRangeException("Массив слов является пустым");
        }
        public static Word LastNotEmptyWord(Word[] words) => LastNotEmptyWord(words, words.Length - 1);

        public static void TrimWords(ref Word[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (Line.TrimmedLine(words[i].Text) == "")
                {
                    Split.Remove(ref words, i);
                    i--;
                }
            }
        }

        public static void RemoveEmptyWords(ref Word[] words)
        {
            for (int i = 0; i < words.Length - 1; i++)
                if (words[i] == "")
                    Split.Remove(ref words, i);
            if (words[words.Length - 1] == "")
                Array.Resize(ref words, words.Length - 1);
        }

        public Word RemovePossibleLastCharacter(char possibleCharacter)
        {
            if (Text.Length == 0)
                return this;
            if (Text[Text.Length - 1] != possibleCharacter)
                return this;
            string text = Text.Substring(0, Text.Length - 1);
            return new Word(text, IsSpaceAfter, CanSplit);
        }

        public Word[] SeparateEscapeSequence()
        {
            if (!Text.Contains('\\'))
                return new Word[] { this };
            Word[] parts = new Word[2];
            int start = Text.IndexOf('\\');
            parts[0] = new Word(Text.Substring(0, start), false, CanSplit);
            parts[1] = new Word(Text.Substring(start), IsSpaceAfter, CanSplit);
            return parts;
        }

        public static Word[] SubArray(Word[] array, int firstIndex, int lastIndex)
        {
            Word[] subArray = new Word[lastIndex - firstIndex + 1];
            for (int i = firstIndex, j = 0; i <= lastIndex; i++, j++)
                subArray[j] = array[i];
            return subArray;
        }
        public static Word[] SubArray(Word[] array, int firstIndex) => SubArray(array, firstIndex, array.Length - 1);

        public static bool ContainsString(Word[] words, string @string)
        {
            foreach (Word word in words)
                if (word.Text.Contains(@string))
                    return true;
            return false;
        }
        #endregion

        public override bool Equals(object obj)
        {
            Word word;
            try
            {
                word = (Word)obj;
                return this == word;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public override int GetHashCode() => Text.GetHashCode();
        #endregion

        #region Operators
        public static explicit operator string(Word word) => word.Text;

        public static implicit operator Word(char symbol) => new Word(symbol);
        public static implicit operator Word(string @string) => new Word(@string, false, true);

        public static bool operator ==(Word first, Word second)
        {
            try
            {
                return first.Text == second.Text;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool operator !=(Word first, Word second) => !(first == second);
        #endregion
    }
}
