namespace MSiSvInfT
{
    using Line = LineAnalyseMethods;

    public static class BracketsMethods
    {
        #region Methods
        private static char CloseBracket(char openBracket)
        {
            if (openBracket == '(')
                return ')';
            else
                return (char)(openBracket + 2);
        }

        public static int GetCloseBracketPosition(Word[] words, char _openBracket) 
            => GetCloseBracketPosition(words, _openBracket, Word.IndexOfFirst(words, new Word(_openBracket)));

        public static int GetCloseBracketPosition(Word[] words, char _openBracket, int openBracketIndex)
        {
            int j = openBracketIndex + 1;
            Word closeBracket = CloseBracket(_openBracket);
            Word openBracket = _openBracket;
            int openCount = 1;
            for (; j < words.Length; j++)
            {
                if (words[j].Text == openBracket.Text)
                    openCount++;
                if (words[j].Text == closeBracket.Text)
                    openCount--;
                if (openCount == 0)
                    return j;
            }
            Data.ThrowError("Число открывающих и закрывающих скобок в строке не совпадает");
            return 0;
        }

        public static int GetCloseBracketPosition(string line, char _openBracket, int openBracketIndex)
        {           
            int openCount = 1;
            char _closeBracket = CloseBracket(_openBracket);
            for (int j = openBracketIndex + 1; j < line.Length; j++)
            {
                if (line[j] == _openBracket)
                    openCount++;
                if (line[j] == _closeBracket)
                    openCount--;
                if (openCount == 0)
                    return j;
            }
            Data.ThrowError("Число открывающих и закрывающих скобок в строке не совпадает");
            return 0;
        }

        public static int GetCloseCurveBracketIndex(string[] lines, int lineWithOpen)
        {
            int open = 1;
            for (int i = lineWithOpen + 1; i < lines.Length; i++)
            {
                string trimmedLine = Line.TrimmedLine(lines[i]);
                if (trimmedLine == "{")
                    open++;
                else if (trimmedLine == "}")
                    open--;
                if (open == 0)
                    return i;
            }
            Data.ThrowError("Фигурные скобки расставлены неверно");
            return 0;
        }

        private static void CurveBracketsCount(string[] lines, out int open, out int close, int startLine = 0)
        {
            open = 0;
            close = 0;
            for (int i = startLine; i < lines.Length; i++)
            {
                if (lines[i] == null)
                    continue;
                string trimmedLine = Line.TrimmedLine(lines[i]);
                if (lines[i] == "{")
                    open++;
                if (lines[i] == "}")
                    close++;
            }
            return;
        }
        #endregion
    }
}
