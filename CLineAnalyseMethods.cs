using System.Collections.Generic;

namespace MSiSvInfT
{
    using Brackets = BracketsMethods;
    using Split = SplitMethods;

    public static class LineAnalyseMethods
    {
        #region Methods
        public static string TrimmedLine(string line)
        {
            string trimmedLine = line.Trim(' ', '\t', '\n');
            trimmedLine = trimmedLine.Replace('\t', ' ');
            return trimmedLine;
        }

        public static void Replace(ref string line, string replacedString, string replacement)
        {
            int index = line.IndexOf(replacedString);
            while (index != -1)
            {
                line = line.Remove(index, replacedString.Length);
                line = line.Insert(index, replacement);
                index = line.IndexOf(replacedString, index);
            }
        }

        public static string GetOffset(string line)
        {
            int i = 0;
            string result = "";
            if (line == null)
                return result;
            while (i < line.Length)
            {
                if (line[i] == '\t')
                    result += "\t";
                else if (line[i] == ' ')
                    result += " ";
                else
                    break;
                i++;
            }
            Replace(ref result, "    ", "\t");
            return result;
        }

        public static int CharCount(string line, char character)
        {
            int count = 0;
            int index = -1;
            do
            {
                index = line.IndexOf(character, index + 1);
                if (index != -1)
                    count++;
            } while (index != -1 && index != line.Length - 1);
            return count;
        }

        public static bool IsStartsWith(string line, string start)
        {
            if (line.IndexOf(start) == 0)
            {
                if (line.Length == start.Length)
                    return true;
                if (line[start.Length] == ' ' || line[start.Length] == '(')
                    return true;
            }
            return false;
        }

        public static bool IsPositionInQuotes(string line, int position)
        {
            if (position == 0)
                return false;

            string tempLine = line;
            int pseudoQuote = tempLine.IndexOf("\\\"");
            while (pseudoQuote != -1)
                tempLine = tempLine.Remove(pseudoQuote, 2);

            string[] parts = tempLine.Split('\"');
            bool isInQuotes = false;
            int counter = 0, i = 0;
            while (counter < position)
            {
                counter += parts[i].Length;
                if (!isInQuotes)
                    isInQuotes = true;
                else
                    isInQuotes = false;
                i++;
            }
            return !isInQuotes;
        }

        public static bool IsPositionInRoundBrackets(string line, int position)
        {
            List<int> openRoundBrackets = OpenRoundBracketsPositions(line);
            foreach(int startIndex in openRoundBrackets)
            {
                int closeIndex = Brackets.GetCloseBracketPosition(line, '(', startIndex);
                if (startIndex < position && position < closeIndex)
                    return true;
            }
            return false;
        }

        private static List<int> OpenRoundBracketsPositions(string line)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < line.Length; i++)
                if (line[i] == '(')
                    list.Add(i);
            return list;
        }

        public static bool Is_SWITCH_Label(string trimmedLine)
        {
            if (IsStartsWith(trimmedLine, Data.CASE.Text)
                 || IsStartsWith(trimmedLine, Data.DEFAULT.Text)
                 || IsStartsWith(trimmedLine, Data.DEFAULT_WithColon.Text))
                return true;
            return false;
        }

        public static int CloseQuoteIndex(Word[] words, int openQuoteIndex)
        {
            for (int i = openQuoteIndex + 1; i < words.Length; i++)
            {
                if (words[i].Text == "\"")
                    if (words[i - 1].Text != "\\")
                        return i;
            }
            Data.ThrowError("Кавычки расставлены неверно в строке \'" + Word.ToLine(words, 0) + "\'");
            return 0;
        }

        public static string FindSimilarPartWithoutGaps((string Part, bool IsInQuotes)[] parts, string stringWithExtraGaps)
        {
            foreach ((string Part, bool IsInQuotes) part in parts)
                if (part.IsInQuotes && IsSimilarExceptGaps(part.Part, stringWithExtraGaps))
                    return part.Part;
            Data.ThrowError("Не удалось найти соответствующую строку для \'" + stringWithExtraGaps + "\'");
            return null;
        }

        private static bool IsSimilarExceptGaps(string withoutGaps, string withGaps)
        {            
            withoutGaps = TrimmedLine(withoutGaps);
            withGaps = TrimmedLine(withGaps);
            int withoutGapsLastIndex = withoutGaps.Length - 1;
            int withGapsLastIndex = withGaps.Length - 1;
            for (int i = 0, j = 0; ; i++, j++)
            {
                if (withGaps[j] == ' ' && withoutGaps[i] != ' ')
                    j++;
                if (withGaps[j] != withoutGaps[i])
                    return false;
                if (j == withGapsLastIndex && i == withoutGapsLastIndex)
                    break;
                if (j == withGapsLastIndex && i != withoutGapsLastIndex)
                    return false;
                if (j != withGapsLastIndex && i == withoutGapsLastIndex)
                    return false;
            }
            return true;
        }

        #region Search For Control Variables
        private static bool ContainsCondition(Word[] words, out int conditionalOperatorIndex)
        {
            for (int i = 0; i < words.Length; i++)
            {
                string text = words[i].Text;
                if (text == Data.IF.Text || text == Data.SWITCH.Text || text == Data.FOR.Text || text == Data.WHILE.Text || text == Data.CASE.Text)
                {
                    conditionalOperatorIndex = i;
                    return true;
                }
            }
            conditionalOperatorIndex = -1;
            return false;
        }

        private static void GetConditionsStartAndEnd(Word[] words, int conditionalOperatorIndex, out int start, out int end)
        {
            start = conditionalOperatorIndex + 1;
            end = words.Length - 1;
        }

        private static void Get_FOR_ConditionBetweenSemicolons(Word[] words, int FOR_ConditionStart, int FOR_ConditionEnd, out int start, out int end)
        {
            start = end = -1;
            for (int i = FOR_ConditionStart; i <= FOR_ConditionEnd; i++)
            {
                if (words[i].Text == Data.SEMICOLON.Text)
                {
                    if (start == -1)
                        start = i;
                    else
                    {
                        end = i;
                        return;
                    }
                }
            }
        }

        private static bool IsControlVariableInCondition(Word[] words, int conditionStart, int conditionEnd, Variable variable)
        {
            Word[] condition = Word.SubArray(words, conditionStart, conditionEnd);
            return Word.Contains(condition, variable.Name);
        }

        private static bool ContainsFunctionCalling(Word[] words, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                foreach (Function function in SecondTask.Functions)
                    if (function.Name.Text == words[i].Text)
                        return true;
                foreach (Function function in SecondTask.SystemFunctions)
                    if (function.Name.Text == words[i].Text)
                        return true;
            }
            return false;
        }

        private static bool SkipWords(Word[] words, Function function, ref int i)
        {
            if (function.Name.Text == words[i].Text)
            {
                i = Brackets.GetCloseBracketPosition(words, '(', i + 1);
                return true;
            }
            return false;
        }

        private static List<Word> GetConditionWithoutFunctionsCallings(Word[] words, int conditionStart, int conditionEnd)
        {
            List<Word> wordsWithoutFunctionsCallings = new List<Word>();
            for (int i = conditionStart; i <= conditionEnd; i++)
            {
                foreach (Function function in SecondTask.Functions)
                {
                    if (SkipWords(words, function, ref i))
                        continue;
                }
                foreach (Function function in SecondTask.SystemFunctions)
                {
                    if (SkipWords(words, function, ref i))
                        continue;
                }
                wordsWithoutFunctionsCallings.Add(words[i]);
            }
            return wordsWithoutFunctionsCallings;
        }

        private static bool ContainsVariableNotAsArray(Word[] words, int start, int end, Variable variable)
        {
            for (int i = start; i <= end; i++)
                if (words[i].Text == variable.Name.Text)
                {
                    if (i != end)
                    {
                        if (words[i + 1] != "[")
                            return true;
                    }
                }
            return false;
        }

        private static bool IsControlVariableIn_FOR_Condition(Word[] words, int conditionalOperatorIndex, Variable variable)
        {
            GetConditionsStartAndEnd(words, conditionalOperatorIndex, out int FOR_ConditionStart, out int FOR_ConditionEnd);
            Get_FOR_ConditionBetweenSemicolons(words, FOR_ConditionStart, FOR_ConditionEnd, out int conditionStart, out int conditionEnd);
            return IsControlVariableInCondition(words, conditionStart, conditionEnd, variable);
        }

        private static bool IsControlVariableIn_CASE_Condition(Word[] words, int conditionalOperatorIndex, Variable variable)
        {
            if (words[conditionalOperatorIndex + 1].Text == variable.Name.Text)
                return true;
            return false;
        }

        private static bool IsControlVariableIn_IF_SWITCH_WHILE_Condition(Word[] words, int conditionalOperatorIndex, Variable variable)
        {
            GetConditionsStartAndEnd(words, conditionalOperatorIndex, out int conditionStart, out int conditionEnd);
            return IsControlVariableInCondition(words, conditionStart, conditionEnd, variable);
        }

        public static bool IsControlVariableInLine(Variable variable, out string clarification, int lineIndex)
        {
            Word[] words = Data.Code[lineIndex]; //
            bool containsCondition = ContainsCondition(words, out int conditionalOperatorIndex);
            if (containsCondition)
            {
                if (words[conditionalOperatorIndex].Text == Data.FOR.Text)
                {
                    clarification = "Используется в условии цикла \'" + Data.FOR.Text + "\' в строке " + (lineIndex + 1).ToString() + ".";
                    return IsControlVariableIn_FOR_Condition(words, conditionalOperatorIndex, variable);
                }
                else if (words[conditionalOperatorIndex].Text == Data.CASE.Text)
                {
                    clarification = "Используется в ветви \'" + Data.CASE.Text + "\' оператора \'" + Data.SWITCH.Text + "\' в строке " + (lineIndex + 1).ToString() + ".";
                    return IsControlVariableIn_CASE_Condition(words, conditionalOperatorIndex, variable);
                }
                else
                {
                    if (words[conditionalOperatorIndex].Text == Data.WHILE.Text)
                    {
                        clarification = "Используется в условии цикла \'" + Data.WHILE.Text + "\' (\'" + Data.DO.Text + " " + Data.WHILE.Text + "\') в строке " + (lineIndex + 1).ToString() + ".";
                    }
                    else
                        clarification = "Используется в условии оператора \'" + words[conditionalOperatorIndex].Text + "\' в строке " + (lineIndex + 1).ToString() + ".";
                    return IsControlVariableIn_IF_SWITCH_WHILE_Condition(words, conditionalOperatorIndex, variable);
                }
            }
            else
            {
                if (FirstTask.LinesWith[(int)O.TERNARY].Contains(lineIndex))
                {
                    Word[][] leftParts = Split.GetLeftPartsOfTernaryOperators(words);
                    foreach (Word[] part in leftParts)
                        if (Word.Contains(part, variable.Name))
                        {
                            clarification = "Используется в условии тернарного оператора в строке " + (lineIndex + 1).ToString() + ".";
                            return true;
                        }
                    clarification = "";
                    return false;
                }
                else
                {
                    clarification = "";
                    return false;
                }
            }
        }
        #endregion

        #region Search For P Variables
        public static bool IsLineWithInputtingDataToVariable(int lineIndex, Variable variable)
        {
            Word[] words = Data.Code[lineIndex]; //
            if (Word.Contains(words, Data.READLINE) && Word.ContainsAny(words, Data.EQUAL_AndCombinedOperators, out int indexOfFirstOperator))
                if (words[indexOfFirstOperator - 1].Text == variable.Name.Text)
                    return true;
            return false;
        }
        #endregion

        #region Search For Modifable Variables
        public static bool IsSettingValueToVariable(int lineIndex, Variable variable)
        {
            Word[] words = Data.Code[lineIndex]; //
            if (Word.ContainsAny(words, Data.EQUAL_AndCombinedOperators, out int indexOfFirstMetOperator))
            {
                Word[] leftPart = Word.SubArray(words, 0, indexOfFirstMetOperator - 1);
                if (Word.Contains(leftPart, '[', out int openSquareBracketIndex))
                {
                    if (leftPart[openSquareBracketIndex - 1].Text == variable.Name.Text)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (leftPart.Length == 1 && leftPart[0].Text == variable.Name.Text)
                        return true;
                    else
                        return false;
                }
            }
            else
            {
                if (Word.ContainsAny(words, Data.IN_or_DECREMENT_Operators, out indexOfFirstMetOperator))
                {
                    if (indexOfFirstMetOperator != 0)
                    {
                        if (words[indexOfFirstMetOperator - 1].Text == variable.Name || words[indexOfFirstMetOperator + 1].Text == variable.Name)                                                                                                                                                                                        
                            return true;
                        return false;
                    }
                    else
                    {                       
                        if (words[1].Text == variable.Name)
                            return true;
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        private static bool Is_DE_Or_INCREMENT_NearVariable(Word[] words, Word word)
        {
            List<int> indexes = Word.AllIndexesOfContainings(words, word);
            foreach (int index in indexes)
                if (index > 0 && words[index - 1].Text[0] == '$')
                    return true;
            return false;
        }

        public static bool AreComputationsInLine(int lineIndex)
        {
            (Word[] Part, bool isInQuotes)[] parts = Split.SeparateLineWithQuotes(lineIndex);
            bool result = false;
            foreach ((Word[] Part, bool isInQuotes) part in parts)
            {
                if (part.isInQuotes)
                    result = result || AreComputationsInLineWithQuotes(part.Part);
                else
                    result = result || AreComputationsInLineWithoutQuotes(part.Part);
                if (result)
                    return true;
            }
            return result;
        }

        private static bool AreComputationsInLineWithQuotes(Word[] words)
            => Is_DE_Or_INCREMENT_NearVariable(words, Data.DECREMENT) || Is_DE_Or_INCREMENT_NearVariable(words, Data.INCREMENT);

        private static bool AreComputationsInLineWithoutQuotes(Word[] words)
        {
            bool result = Word.Contains(words, Data.EQUAL);
            if (result)
                return true;
            result = result || Word.Contains(words, Data.INCREMENT);
            if (result)
                return true;
            result = result || Word.Contains(words, Data.DECREMENT);
            return result;
        }
        #endregion

        #region Search For Parasitic Variables
        public static bool IsOutputtingFunctionWithVariableInLine(int lineIndex, Variable variable, out Function function)
        {
            function = null;
            Word[] words = Data.Code[lineIndex]; 
            if (Word.ContainsAny(words, SecondTask.GetFunctionsNames(SecondTask.GetFunctionsThatAreOnlyForOutput()), out int index))
            {
                function = SecondTask.GetFunctionByName(words[index]);
                if (words[index + 1].Text == "(")
                {
                    GetConditionsStartAndEnd(words, index, out int parametersStart, out int parametersEnd);
                    Word[] parameters = Word.SubArray(words, parametersStart, parametersEnd);
                    if (Word.Contains(parameters, variable.Name))
                        return true;
                }
                else
                {
                    if (words[index + 1].Text == variable.Name.Text)
                        return true;
                }
            }
            return false;
        }

        public static bool IsUsingInSettingValueForOtherVariableInLine(int lineIndex, Variable variable)
        {
            Word[] words = Data.Code[lineIndex]; 
            if (Word.Contains(words, Data.EQUAL_AndCombinedOperators[0], out int EQUAL_Index))
            {
                Word[] rightPart = Word.SubArray(words, EQUAL_Index + 1);
                if (Word.Contains(rightPart, variable.Name))
                    return true;
            }
            return false;
        }
        #endregion
        #endregion
    }
}
