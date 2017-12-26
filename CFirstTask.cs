using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Controls;

namespace MSiSvInfT
{
    using Spans = SpanMethods;
    using Search = SearchInOperatorsCategories;
    using Split = SplitMethods;
    using Line = LineAnalyseMethods;
    using Subsidiary = SubsidiaryMethodsForSearchingOperators;
    using Brackets = BracketsMethods;

    public enum O
    {
        IF,
        ELSEIF,
        ELSE,
        SWITCH,
        FOR,
        WHILE,
        DO_WHILE,
        CASE,
        DEFAULT,
        TERNARY,
        Last
    }

    public static class FirstTask
    {
        #region Properties
        public static List<CodeBlock>[] Blocks { get; set; } = new List<CodeBlock>[(int)O.Last];
        public static List<CodeBlock> SortedBlocks { get; set; }
        public static List<int>[] LinesWith { get; set; } = new List<int>[(int)O.Last];
        public static CodeBlock MaxNestedCodeBlock { get; private set; } = null;
        public static int MaxNestingLevel { get; private set; } = 0;
        public static int OperatorsCount { get; set; }
        #endregion

        #region Constructor
        static FirstTask()
        {
            for (int i = 0; i < (int)O.Last; i++)
            {
                LinesWith[i] = new List<int>();
                Blocks[i] = new List<CodeBlock>();
            }
        }
        #endregion

        #region Methods

        #region Lines With Block Operators
        public static void ClearLists()
        {
            for (int i = 0; i < (int)O.Last; i++)
                LinesWith[i].Clear();
        }

        private static void SearchForLinesWithConditionalOperators()
        {
            ClearLists();
            for (int i = 0; i < Data.Code.Lines.Length; i++)
            {
                Word[] words = Data.Code[i];
                if (words.Length == 0)
                    continue;
                string first = words[0].Text;
                if (first == Data.IF.Text)
                    LinesWith[(int)O.IF].Add(i);
                else if (first == Data.ELSEIF.Text)
                    LinesWith[(int)O.ELSEIF].Add(i);
                else if (first == Data.ELSE.Text)
                    LinesWith[(int)O.ELSE].Add(i);
                else if (first == Data.SWITCH.Text)
                    LinesWith[(int)O.SWITCH].Add(i);
                else if (first == Data.FOR.Text)
                    LinesWith[(int)O.FOR].Add(i);
                else if (first == Data.WHILE.Text && !Word.Contains(words, Data.SEMICOLON))
                    LinesWith[(int)O.WHILE].Add(i);
                else if (first == Data.DO.Text)
                    LinesWith[(int)O.DO_WHILE].Add(i);
                else if (first == Data.CASE.Text)
                    LinesWith[(int)O.CASE].Add(i);
                else if (first == Data.DEFAULT.Text)
                    LinesWith[(int)O.DEFAULT].Add(i);
                else if (Word.Contains(words, Data.QUESTION_MARK) && Word.Contains(words, Data.COLON))
                    LinesWith[(int)O.TERNARY].Add(i);
            }
        }
        #endregion

        #region Nestings
        private static void FindBlocksNestings()
        {
            SortedBlocks = GetSortedBlocks();
            foreach (CodeBlock block in SortedBlocks)
            {
                if (block.Operator != Data.CASE.Text && block.Operator != Data.DEFAULT.Text && block.Operator != Data.QUESTION_MARK.Text)
                    SetNestingLevel(block, 0, 1);
                else if (block.Operator == Data.QUESTION_MARK)
                {
                    int nestings = TernaryOperatorNestings(Data.Code[block.StartLine], 0);
                    SetNestingLevel(block, nestings, nestings + 1);
                }
                else
                    SetNestingLevelFor_CASE_Or_DEFAULT(block);
            }
        }

        private static void SetNestingLevelFor_CASE_Or_DEFAULT(CodeBlock block)
        {
            foreach (CodeBlock SWITCH_Block in Enumerate_SWITCH_CodeBlocks())
            {
                if (SWITCH_Block.IsLineCorrectlyIn_SWITCH_Block(block.StartLine))
                {
                    List<int> CASE_And_DEFAULT_Indexes = CASE_And_DEFAULT_LinesIndexesOf_SWITCH_Block(SWITCH_Block);
                    int blockIndex = CASE_And_DEFAULT_Indexes.IndexOf(block.StartLine);
                    block.NestingLevel = blockIndex + SWITCH_Block.NestingLevel;
                    if (block.Operator == Data.DEFAULT.Text)
                        block.NestingLevel--;
                    return;
                }
            }
            Data.ThrowError("Ошибка в блоках оператора \'switch\'");
        }

        public static List<int> CASE_And_DEFAULT_LinesIndexesOf_SWITCH_Block(CodeBlock SWITCH_Block)
        {
            List<int> list = new List<int>();
            for (int i = SWITCH_Block.StartLine + 1; i <= SWITCH_Block.EndLine; i++)
            {
                if (Data.Code[i].Length == 0)
                    continue;
                if (Data.Code[i][0].Text == Data.SWITCH.Text)
                {
                    i = Brackets.GetCloseCurveBracketIndex(Data.Code.Lines, i + 1);
                }
                if (Data.Code[i][0].Text == Data.CASE.Text || Data.Code[i][0].Text == Data.DEFAULT.Text)
                    list.Add(i);
            }
            return list;
        }

        public static IEnumerable<CodeBlock> Enumerate_SWITCH_CodeBlocks()
        {
            foreach (CodeBlock block in SortedBlocks)
                if (block.Operator == Data.SWITCH)
                    yield return block;
        }

        private static void SetNestingLevel(CodeBlock block, int levelIfNoFramingBlock, int addition)
        {
            CodeBlock framingBlock = GetFramingBlockForUsualBlock(block);
            if (framingBlock == null)
            {
                block.NestingLevel = levelIfNoFramingBlock;
                block.FramingBlockStartLine = null;
            }
            else
            {
                block.NestingLevel = framingBlock.NestingLevel + addition;
                block.FramingBlockStartLine = framingBlock.StartLine;
            }
        }   

        private static CodeBlock GetFramingBlockForUsualBlock(CodeBlock block)
        {
            int? skipCounter = null;
            for (int i = block.StartLine - 1; i >= 0; i--)
            {
                Word[] words = Data.Code[i];
                if (words.Length == 0)
                    continue;
                if (words[0].Text == "}")
                {
                    if (skipCounter.HasValue)
                        skipCounter++;
                    else
                        skipCounter = 1;
                }
                if (words[0].Text == "{")
                {
                    if (!skipCounter.HasValue || skipCounter.Value == 0)
                    {
                        Word[] definition = Data.Code[i - 1];
                        if (Word.Contains(definition, Data.FUNCTION))
                            return null;
                        else
                            return GetBlockByStartLine(i - 1);
                    }
                    else
                        skipCounter--;
                }
                if (words[0].Text == Data.CASE.Text && (!skipCounter.HasValue || skipCounter.Value == 0))
                    return GetBlockByStartLine(i);
            }
            return null;
        }

        public static CodeBlock GetBlockByStartLine(int startLine)
        {
            foreach (CodeBlock block in SortedBlocks)
                if (block.StartLine == startLine)
                    return block;
            return null;
        }

        private static void SetMaxNestingLevel()
        {
            MaxNestedCodeBlock = SortedBlocks[0];
            foreach (CodeBlock block in SortedBlocks)
                if (block.NestingLevel > MaxNestedCodeBlock.NestingLevel)
                    MaxNestedCodeBlock = block;
            MaxNestingLevel = MaxNestedCodeBlock.NestingLevel;
        } 
        #endregion

        #region Code Blocks
        private static List<CodeBlock> GetSortedBlocks()
        {
            List<CodeBlock> list = new List<CodeBlock>();
            foreach (List<CodeBlock> blocks in Blocks)
                foreach (CodeBlock block in blocks)
                    list.Add(block);
            list.Sort();
            return list;
        }

        private static CodeBlock GetCodeBlock(int startLine)
        {
            foreach (List<CodeBlock> blocks in Blocks)
                foreach (CodeBlock block in blocks)
                    if (block.StartLine == startLine)
                        return block;
            throw new IndexOutOfRangeException();
        }

        public static bool IsLineInBlock(int lineNumber)
        {
            foreach (List<CodeBlock> blocks in Blocks)
                foreach (CodeBlock block in blocks)
                    if (block.StartLine <= lineNumber && block.EndLine >= lineNumber)
                        return true;
            return false;
        }

        public static bool IsLineInBlock(int lineNumber, out CodeBlock codeBlock)
        {
            List<CodeBlock> blocksWithLine = new List<CodeBlock>();
            foreach (List<CodeBlock> blocks in Blocks)
                foreach (CodeBlock block in blocks)
                    if (block.StartLine <= lineNumber && block.EndLine >= lineNumber)
                        blocksWithLine.Add(block);
            if (blocksWithLine.Count == 0)
            {
                codeBlock = null;
                return false;
            }
            codeBlock = GetLeastCodeBlock(blocksWithLine);
            return true;
        }

        private static CodeBlock GetLeastCodeBlock(List<CodeBlock> blocks)
        {
            int minimalSize = blocks[0].Size;
            int indexOfMinimal = 0;
            for (int i = 1; i < blocks.Count; i++)
            {
                if (blocks[i].Size < minimalSize)
                {
                    minimalSize = blocks[i].Size;
                    indexOfMinimal = i;
                }
            }
            return blocks[indexOfMinimal];
        }

        public static bool IsLineInCycleBlock(int lineNumber, out string cycle)
        {
            foreach (List<CodeBlock> blocks in Blocks)
                foreach (CodeBlock block in blocks)
                    if (block.IsLineInBlock(lineNumber))
                        if (block.Operator == Data.FOR.Text || block.Operator == Data.WHILE.Text || block.Operator == Data.DO.Text)
                        {
                            cycle = block.Operator;
                            if (cycle == Data.DO.Text)
                                cycle += " " + Data.WHILE.Text;
                            return true;
                        }
            cycle = "";
            return false;
        }

        private static bool FindCodeBlockByStartLine(int startLine, List<CodeBlock> blocks, out CodeBlock result)
        {
            foreach (CodeBlock block in blocks)
                if (block.StartLine == startLine)
                {
                    result = block;
                    return true;
                }
            result = null;
            return false;
        }

        public static List<CodeBlock> Get_IF_ConnectedBlocks(CodeBlock block)
        {
            List<CodeBlock> blocks = new List<CodeBlock>();
            if (block.Operator != Data.IF.Text)
                return blocks;
            bool wasBlock = false;
            int startLine = block.EndLine + 1;
            do
            {
                wasBlock = FindCodeBlockByStartLine(startLine, Blocks[(int)O.ELSEIF], out CodeBlock ELSEIF_Block);
                if (wasBlock)
                {
                    blocks.Add(ELSEIF_Block);
                    startLine = ELSEIF_Block.EndLine + 1;
                }
            } while (wasBlock);
            if (FindCodeBlockByStartLine(startLine, Blocks[(int)O.ELSE], out CodeBlock ELSE_Block))
                blocks.Add(ELSE_Block);
            return blocks;
        }

        private static int Find_CASE_EndLine(int startLine)
        {
            int? skipCounter = null;
            for (int i = startLine + 1; i < Data.Code.Lines.Length; i++)
            {
                if (Word.ContainsAny(Data.Code[i], Data.SWITCH_Parts, out int index))
                {
                    if (!skipCounter.HasValue || skipCounter.Value == 0)
                        return i - 1;
                }
                if (Word.Contains(Data.Code[i], '{'))
                {
                    if (skipCounter.HasValue)
                        skipCounter++;
                    else
                        skipCounter = 1;
                }
                if (Word.Contains(Data.Code[i], '}'))
                {
                    if (!skipCounter.HasValue || skipCounter.Value == 0)
                        return i - 1;
                    else
                        skipCounter--;
                }
            }
            Data.ThrowError("Не удалось найти конец ветви \'case\' в строке " + (startLine + 1).ToString());
            return 0;
        }

        private static int Find_DEFAULT_EndLine(int startLine)
        {
            int? skipCounter = null;
            for (int i = startLine + 1; i < Data.Code.Lines.Length; i++)
            {
                if (Word.Contains(Data.Code[i], '{'))
                {
                    if (skipCounter.HasValue)
                        skipCounter++;
                    else
                        skipCounter = 1;
                }
                if (Word.Contains(Data.Code[i], '}'))
                {
                    if (!skipCounter.HasValue || skipCounter.Value == 0)
                        return i - 1;
                    else
                        skipCounter--;
                }
            }
            Data.ThrowError("Не удалось найти конец ветви \'default\' в строке " + (startLine + 1).ToString());
            return 0;
        }

        private static void GetBlock(int i)
        {
            Blocks[i] = new List<CodeBlock>();
            foreach (int start in LinesWith[i])
            {
                Word[] words = Data.Code[start];
                if (i == (int)O.TERNARY)
                {
                    CodeBlock block = new CodeBlock(start, Data.QUESTION_MARK.Text, false) { EndLine = start };
                    Blocks[i].Add(block);
                }
                else if (i == (int)O.CASE)
                {
                    CodeBlock block = new CodeBlock(start, Data.CASE.Text, false) { EndLine = Find_CASE_EndLine(start) };
                    Blocks[i].Add(block);
                }
                else if (i == (int)O.DEFAULT)
                {
                    CodeBlock block = new CodeBlock(start, Data.DEFAULT.Text, false) { EndLine = Find_DEFAULT_EndLine(start) };
                    Blocks[i].Add(block);
                }
                else
                    Blocks[i].Add(new CodeBlock(start, words[0].Text, true));
            }
        }

        private static void DistinguishBlocks()
        {
            for (int i = 0; i < (int)O.Last; i++)
                GetBlock(i);
        }
        #endregion        
            
        #region Search For Operators
        private static void MakeGapsAfterWords(Word[] words)
        {
            for (int i = 1; i < words.Length; i++)
            {
                switch (words[i].Text)
                {
                    case "(":
                        words[i].IsSpaceAfter = false;
                        if (Word.Contains(Data.BLOCK_Operators, words[i - 1]))
                            words[i - 1].IsSpaceAfter = true;
                        else
                            words[i - 1].IsSpaceAfter = false;
                        break;
                    case "[":
                        words[i].IsSpaceAfter = false;
                        words[i - 1].IsSpaceAfter = false;
                        break;
                    case ")":
                    case "]":
                        words[i - 1].IsSpaceAfter = false;
                        if (i + 1 == words.Length)
                            break;
                        if (words[i + 1].Text == Data.SEMICOLON.Text)
                            words[i].IsSpaceAfter = false;
                        else
                            words[i].IsSpaceAfter = true;
                        break;
                    case "\"":  
                        words[i - 1].IsSpaceAfter = true;
                        words[i].IsSpaceAfter = false;
                        break;
                    case "&":
                    case "!":
                        words[i].IsSpaceAfter = false;
                        break;
                    case "return":
                        words[i].IsSpaceAfter = true;
                        break;
                    case "++":
                    case "--":
                        if (i + 1 == words.Length)
                        {
                            words[i - 1].IsSpaceAfter = false;
                            break;
                        }
                        if (words[i + 1].Text[0] == '$')
                            words[i].IsSpaceAfter = false;
                        else
                            words[i - 1].IsSpaceAfter = false;
                        break;
                    case ",":
                    case ";":
                        words[i - 1].IsSpaceAfter = false;
                        break;
                }
            }
        }

        private static void SearchForAllOperatorsInLineWithoutQuotes(Word[] words, int lineNumber, ref Span span)
        {
            MakeGapsAfterWords(words);
            bool wasFunction = false;

            bool needsCheckingForTernaryOperator = Word.ContainsString(words, Data.QUESTION_MARK.Text);
            List<int> colonsIndexes = new List<int>();   

            for (int i = 0; i < words.Length; i++)
            {              
                if (wasFunction)
                {
                    if (words[i] == "{")
                        wasFunction = false;
                    else
                    {
                        span.Inlines.Add(words[i].ToString());
                        continue;
                    }
                }

                if (Split.IsTag(words[i].Text))
                {
                    span.Inlines.Add(words[i].ToString());
                    continue;
                }

                if (words[i].Text == Data.FUNCTION.Text)
                {
                    wasFunction = true;
                    span.Inlines.Add(words[i].ToString());
                    continue;
                }

                if (Search.InSymbolicOperators(words[i], ref span))
                    continue;

                if (Search.InSingleWordsOperators(words[i], ref span))
                    continue;

                if (Search.InWordOperatorsWithoutBrackets(words[i], ref span))
                    continue;

                if (Search.InWordOperatorsWithBrackets(words[i], ref span))
                    continue;

                if (Search.For_DO(words[i], ref span))
                    continue;

                if (Search.ForSquareBrackets(words[i], ref span))
                    continue;

                if (Search.CompareWithConditionalOperators(words[i], ref span, lineNumber))
                    continue;

                if (needsCheckingForTernaryOperator)
                    if (Search.ForTernaryOperator(i, words, ref span, ref colonsIndexes))
                        continue;

                if (words[i].Text == "(" || words[i].Text == "{" || words[i].Text == "[")
                    words[i].IsSpaceAfter = false;
                if (Line.TrimmedLine(words[i].Text) == "")
                    words[i].IsSpaceAfter = false;
                if (needsCheckingForTernaryOperator)
                {
                    if (!colonsIndexes.Contains(i))
                        span.Inlines.Add(words[i].ToString());
                    else
                        Subsidiary.SelectOperator(words[i], Data.ConditionalOperatorsColor, ref span);
                }
                else
                    span.Inlines.Add(words[i].ToString());
            }
        }

        private static void SearchForAllOperatorsInLineWithQuotes(string line, int lineNumber, ref Span span)
        {
            (Word[] Part, bool IsInQuotes)[] parts = Split.SeparateLineWithQuotes(lineNumber);
            for (int j = 0; j < parts.Length; j++)
            {
                if (parts[j].IsInQuotes)
                {
                    if (IsPartInQuotesContainsSquareBrackets(parts[j].Part))
                    {
                        List<int> squareBracketsOperators = SquareBracketsOperatorsIndexes(parts[j].Part);
                        span.Inlines.Add(Spans.String("\"", false, false));
                        for (int i = 0; i < parts[j].Part.Length; i++)
                        {
                            if (squareBracketsOperators.Contains(i))
                                span.Inlines.Add(Spans.Operator(parts[j].Part[i].ToString(), Data.OtherOperatorsColor, Data.IsNumeratingOperators));
                            else
                                span.Inlines.Add(Spans.String(parts[j].Part[i].ToString(), false, false));
                        }
                        span.Inlines.Add(Spans.String("\"", false, false));
                    }
                    else
                        span.Inlines.Add(Spans.String(Line.FindSimilarPartWithoutGaps(Split.SeparateLineWithQuotes(Data.Code.Lines[lineNumber]), Word.ToLine(parts[j].Part, 0)), true, true));                  
                }
                else
                    SearchForAllOperatorsInLineWithoutQuotes(parts[j].Part, lineNumber, ref span);
            }
        }

        private static bool IsSquareBracketOperator(Word[] words, int index)
        {
            if (words[index].Text == "[")
                if (words[index - 1].Text[0] == '$')
                    return true;
            return false;
        }

        private static bool IsPartInQuotesContainsSquareBrackets(Word[] words)
        {
            for (int i = 1; i < words.Length; i++)
                if (IsSquareBracketOperator(words, i))
                    return true;
            return false;
        }

        private static List<int> SquareBracketsOperatorsIndexes(Word[] words)
        {
            List<int> list = new List<int>();
            for (int i = 1; i < words.Length; i++)
            {
                if (IsSquareBracketOperator(words, i))
                {
                    list.Add(i);
                    OperatorsCount++;
                    list.Add(Brackets.GetCloseBracketPosition(words, '[', i));
                }
            }
            return list;
        }

        private static bool SplitBySingleLineComment(string line, ref Span span, ref int i)
        {
            int commentStart = CommentOutsideOfQuotes(line, "//");
            if (commentStart != -1)
            {
                Split.LineByComment(line, out string code, out string comment, commentStart);
                SearchForAllOperatorsInLineWithoutComments(code, ref span, i);
                span.Inlines.Add(Spans.Comment(comment, null));
            }
            else
                SearchForAllOperatorsInLineWithQuotes(line, i, ref span);
            return true;
        }

        private static bool SplitByMultiLineComment(string line, ref Span span, ref int i)
        {
            int commentStart = CommentOutsideOfQuotes(line, Data.MULTILINE_COMMENT_Start.Text);
            if (commentStart != -1)
            {
                Split.LineByComment(Data.Code.Lines[i], out string code, out string comment, commentStart);
                SearchForAllOperatorsInLineWithoutComments(code, ref span, i);
                span.Inlines.Add(Spans.Comment(comment, null));

                int commentEnd;
                i--;
                int start = i + 1;
                do
                {
                    i++;
                    commentEnd = CommentOutsideOfQuotes(Data.Code.Lines[i], Data.MULTILINE_COMMENT_End.Text);
                    if (commentEnd != -1)
                        break;
                    else
                    {
                        if (i != start)
                            span.Inlines.Add(Spans.Comment(Data.Code.Lines[i], i + 1));
                    }
                } while (!Data.Code.Lines[i].Contains(Data.MULTILINE_COMMENT_End.Text));

                Split.LineByComment(Data.Code.Lines[i], out string endComment, out string startLine, commentEnd + 2);
                span.Inlines.Add(Spans.Comment(endComment, i + 1));
                if (!SearchForAllOperatorsInLineWithPossibleComments(startLine, ref span, ref i))
                    SearchForAllOperatorsInLineWithoutComments(startLine, ref span, i);
                return true;
            }
            else
                SearchForAllOperatorsInLineWithoutComments(line, ref span, i);
            return true;
        }

        private static bool SearchForAllOperatorsInLineWithPossibleComments(string line, ref Span span, ref int i)
        {
            if (line.Contains(Data.SINGLELINE_COMMENT.Text) && !line.Contains(Data.MULTILINE_COMMENT_Start.Text))
                return SplitBySingleLineComment(line, ref span, ref i);

            if (line.Contains(Data.MULTILINE_COMMENT_Start.Text) && !line.Contains(Data.SINGLELINE_COMMENT.Text))
                return SplitByMultiLineComment(line, ref span, ref i);

            if (line.Contains(Data.MULTILINE_COMMENT_Start.Text) && line.Contains(Data.SINGLELINE_COMMENT.Text))
            {
                int multiLineStart = CommentOutsideOfQuotes(line, Data.MULTILINE_COMMENT_Start.Text);
                int singleLineStart = CommentOutsideOfQuotes(line, Data.SINGLELINE_COMMENT.Text);

                if (singleLineStart == -1 && multiLineStart >= 0)
                    return SplitByMultiLineComment(line, ref span, ref i);
                else if (singleLineStart >= 0 && multiLineStart == -1)
                    return SplitBySingleLineComment(line, ref span, ref i);
                else if (singleLineStart == -1 && multiLineStart == -1)
                {
                    SearchForAllOperatorsInLineWithQuotes(line, i, ref span);
                    return true;
                }
                else
                {
                    if (multiLineStart < singleLineStart)
                        return SplitByMultiLineComment(line, ref span, ref i);
                    else
                        return SplitBySingleLineComment(line, ref span, ref i);
                }
            }
            return false;
        }

        private static void SearchForAllOperatorsInLineWithoutComments(string line, ref Span span, int lineNumber)
        {
            if (!line.Contains("\""))
                SearchForAllOperatorsInLineWithoutQuotes(Data.Code[lineNumber], lineNumber, ref span);
            else
                SearchForAllOperatorsInLineWithQuotes(line, lineNumber, ref span);
        }

        private static int CommentOutsideOfQuotes(string codeLine, string commentType)
        {
            int commentStart = 0;
            do
            {
                int previous = commentStart;
                commentStart = codeLine.IndexOf(commentType, commentStart);
                if (commentStart == previous)
                {
                    if (Line.IsPositionInQuotes(codeLine, commentStart))
                        return -1;
                    else
                        return commentStart;
                }
                if (commentStart == -1)
                    return -1;
            } while (Line.IsPositionInQuotes(codeLine, commentStart));
            return commentStart;
        }

        private static void SearchForAllOperatorsInCode()
        {
            double offset = Data.Code.RTBCode.VerticalOffset;
            Data.Code.RTBCode.Document.Blocks.Clear();
            Paragraph paragraph = new Paragraph() { Margin = Data.NullThickness };
            OperatorsCount = 0;
            int i;
            for (i = 0; i < Data.Code.Lines.Length; i++)
            {
                paragraph.Inlines.Add(Spans.LineNumberSpan(i + 1));
                Span span = new Span();
                span.Inlines.Add(Data.Code.GetTabulation(i));
                if (IsLineInBlock(i, out CodeBlock block))
                {
                    if (block.NestingLevel == MaxNestingLevel)
                        span.Background = Data.MaxNestedBlockBackground;
                    else
                        span.Background = Data.BlockBackground;
                }
                if (!SearchForAllOperatorsInLineWithPossibleComments(Data.Code.Lines[i], ref span, ref i))
                    SearchForAllOperatorsInLineWithoutComments(Data.Code.Lines[i], ref span, i);
                paragraph.Inlines.Add(span);
            }
            Data.Code.RTBCode.Document = new FlowDocument(paragraph);
            Data.Code.RTBCode.ScrollToVerticalOffset(offset);
        }
        #endregion

        #region Ternary Operators
        private static int TernaryOperatorsAmount()
        {
            int amount = 0;
            foreach (int lineIndex in LinesWith[(int)O.TERNARY])
                amount += TernaryOperatorsCount(lineIndex);
            return amount;
        }

        public static int TernaryOperatorsCount(int lineIndex)
        {
            Word[] words = Data.Code[lineIndex];
            int questionMarks = 0;
            int colons = 0;
            foreach (Word word in words)
            {
                if (word.Text == Data.QUESTION_MARK.Text)
                    questionMarks++;
                else if (word.Text == Data.COLON.Text)
                    colons++;
            }
            if (colons != questionMarks)
                Data.ThrowError("Тернарные операторы неправильно вложены");
            else
                return questionMarks;
            return 0;
        }

        private static int TernaryOperatorsMaxNesting(int lineIndex)
        {
            Word[] words = Data.Code[lineIndex];
            return TernaryOperatorNestings(words, 0);
        }

        private static int TernaryOperatorNestings(Word[] ternaryOperator, int level)
        {
            Word[][] parts = Split.GetPartsOfTernaryOperator(ternaryOperator);
            int left = level;
            if (Word.Contains(parts[1], Data.QUESTION_MARK))
                left = TernaryOperatorNestings(parts[1], level + 1);
            int right = level;
            if (Word.Contains(parts[2], Data.QUESTION_MARK))
                right = TernaryOperatorNestings(parts[2], level + 1);
            return left > right ? left : right;
        }

        private static int GetTernaryOperatorBlockStartLine(int indexOfLineWithTernaryOperator)
        {
            for (int i = indexOfLineWithTernaryOperator - 1; i >= 0; i--)
            {
                Word[] words = Data.Code[i];
                if (Word.ContainsAny(words, Data.BLOCK_Operators, out int index))
                    return i;
            }
            return -1;
        }

        private static int MaxNestingsInTernaryOperators(int blockStartLine)
        {
            int nestings = 0;
            foreach (int lineIndex in LinesWith[(int)O.TERNARY])
            {
                int temp = 0;
                if (GetTernaryOperatorBlockStartLine(lineIndex) == blockStartLine)
                    temp = TernaryOperatorsMaxNesting(lineIndex);
                if (temp > nestings)
                    nestings = temp;
            }
            return nestings;
        }
        #endregion

        #region Results
        public static int GetAbsoluteDifficulty() 
            => LinesWith[(int)O.IF].Count + LinesWith[(int)O.SWITCH].Count + LinesWith[(int)O.FOR].Count + LinesWith[(int)O.WHILE].Count + LinesWith[(int)O.DO_WHILE].Count + TernaryOperatorsAmount();

        public static string GetRelativeDifficulty()
        {
            float result = GetAbsoluteDifficulty() / (float)OperatorsCount;
            result *= 100;
            int inPercents;
            if (result - (int)result >= 0.5)
                inPercents = (int)result + 1;
            else
                inPercents = (int)result;
            return inPercents.ToString() + " %";
        }
        #endregion

        #region Conditional Operators Grid
        public static Grid GetConditionalOperatorsGrid()
        {
            int rowCounter = 0;
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            WPF_Methods.AddToGrid(ref grid, (CodeBlock.GetTemplateOfGrid(false, Data.TemplateTextFontSize), 0, rowCounter++));
            for (int i = 0; i < SortedBlocks.Count; i++)
            {
                if (SortedBlocks[i].Operator == Data.ELSEIF.Text || SortedBlocks[i].Operator == Data.ELSE.Text || SortedBlocks[i].Operator == Data.CASE.Text || SortedBlocks[i].Operator == Data.DEFAULT.Text)
                    continue;
                grid.RowDefinitions.Add(new RowDefinition());
                if (i == SortedBlocks.Count - 1)
                    WPF_Methods.AddToGrid(ref grid, (SortedBlocks[i].GetGrid(rowCounter, true, Data.UsualTextFontSize, Data.ApplicationBackground), 0, rowCounter++));
                else
                    WPF_Methods.AddToGrid(ref grid, (SortedBlocks[i].GetGrid(rowCounter, false, Data.UsualTextFontSize, Data.ApplicationBackground), 0, rowCounter++));
            }
            return grid;
        }
        #endregion

        public static void Perform()
        {
            try { SearchForLinesWithConditionalOperators(); }
            catch(Exception) { throw new Exception("Не удалось найти строки с условными операторами"); }

            try { DistinguishBlocks(); }
            catch (Exception) { throw new Exception("Не удалось выделить блоки условных операторов"); }

            try { FindBlocksNestings(); }
            catch (Exception) { Data.Errors.Add("Не удалось найти уровни вложенности условных операторов"); }

            try { SetMaxNestingLevel(); }
            catch (Exception) { Data.Errors.Add("Не удалось установить максимальный уровень вложенности"); }

            try { SearchForAllOperatorsInCode(); }
            catch (Exception) { Data.Errors.Add("Не удалось найти все операторы в коде"); }
        }

        #endregion
    }
}
