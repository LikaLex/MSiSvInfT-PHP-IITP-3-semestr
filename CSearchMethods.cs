using System.Collections.Generic;
using System.Windows.Documents;

namespace MSiSvInfT
{
    using Subsidiary = SubsidiaryMethodsForSearchingOperators;

    public static class SearchInOperatorsCategories
    {
        #region Methods
        public static bool InSymbolicOperators(Word word, ref Span span)
        {
            foreach (string[] operators in Operators.Symbolic)
            {
                foreach (string @operator in operators)
                    if (word.Text == @operator)
                    {
                        Subsidiary.AddOperator(word, Data.OtherOperatorsColor, ref span);
                        return true;
                    }
            }
            return false;
        }

        public static bool InSingleWordsOperators(Word word, ref Span span)
        {
            foreach (string @operator in Operators.SingleWords)
                if (word.Text == @operator + Data.SEMICOLON.Text)
                {
                    Subsidiary.AddOperator(word, Data.OtherOperatorsColor, ref span);
                    return true;
                }
            return false;
        }

        public static bool InWordOperatorsWithoutBrackets(Word word, ref Span span)
        {
            foreach (string @operator in Operators.WordWithoutBrackets)
                if (word.Text == @operator)
                {
                    Subsidiary.AddOperator(word, Data.OtherOperatorsColor, ref span);
                    return true;
                }
            return false;
        }

        public static bool InWordOperatorsWithBrackets(Word word, ref Span span)
        {
            foreach (string @operator in Operators.WordWithBrackets)
            {
                if (word.Text == @operator)
                {
                    Subsidiary.AddOperator(word, Data.OtherOperatorsColor, ref span);
                    return true;
                }
            }
            return false;
        }

        public static bool For_DO(Word word, ref Span span)
        {
            if (word.Text == Data.DO.Text)
            {
                Subsidiary.SelectOperator(word, Data.ConditionalOperatorsColor, ref span);
                return true;
            }
            return false;
        }

        public static bool ForSquareBrackets(Word word, ref Span span)
        {
            if (word.Text == "[")
            {
                Subsidiary.AddOperator(word, Data.OtherOperatorsColor, ref span);
                return true;
            }
            if (word.Text == "]")
            {
                Subsidiary.SelectOperator(word, Data.OtherOperatorsColor, ref span);
                return true;
            }
            return false;
        }

        private static bool NextColonIndex(int index, Word[] words, ref List<int> colons)
        {
            int colonIndex = -1;
            for (int j = index + 1; j < words.Length; j++)
            {
                if (words[j].Text == Data.COLON.Text)
                    if (!colons.Contains(j))
                        colonIndex = j;
            }
            if (colonIndex == -1)
                return false;
            else
            {
                colons.Add(colonIndex);
                return true;
            }
        }

        public static bool ForTernaryOperator(int index, Word[] words, ref Span span, ref List<int> colons)
        {
            if (words[index].Text == Data.QUESTION_MARK.Text && NextColonIndex(index, words, ref colons))
            {
                Subsidiary.AddOperator(words[index], Data.ConditionalOperatorsColor, ref span);
                return true;
            }
            return false;
        }

        public static bool CompareWithConditionalOperators(Word word, ref Span span, int lineNumber)
        {
            if (Subsidiary.ConditionalCompare(word, Data.IF.Text, ref span, lineNumber)
                || Subsidiary.ConditionalCompare(word, Data.SWITCH.Text, ref span, lineNumber)
                || Subsidiary.ConditionalCompare(word, Data.FOR.Text, ref span, lineNumber)
                || Subsidiary.ConditionalCompare(word, Data.WHILE.Text, ref span, lineNumber))
                return true;           

            if (word == Data.ELSE.Text)
            {
                Subsidiary.SelectOperator(word, Data.ConditionalOperatorsColor, ref span);
                return true;
            }

            if (word == Data.ELSEIF.Text)
            {
                Subsidiary.SelectOperator(Data.ELSEIF.Text, Data.ConditionalOperatorsColor, ref span);
                return true;
            }
            return false;
        }
        #endregion
    }
}
