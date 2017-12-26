using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace MSiSvInfT
{
    public static class Data
    {
        #region Properties
        public static Brush ConditionalOperatorsColor { get; } = Brushes.Blue;
        public static Brush OtherOperatorsColor { get; } = Brushes.Green;
        public static Brush BlockBackground { get; } = Brushes.DarkSeaGreen;
        public static Brush LineNumberBackground { get; } = Brushes.Wheat;
        public static Brush CodeBackground { get; } = Brushes.White;
        public static Brush CommentColor { get; } = Brushes.Silver;
        public static Brush MaxNestedBlockBackground { get; } = Brushes.Gold;
        public static Brush StringColor { get; } = Brushes.Brown;
        public static Brush ApplicationBackground { get; } = Brushes.Azure;
        public static Brush ToolTipCaptionBackground { get; } = Brushes.Aquamarine;
        public static Brush ToolTipTextBackground { get; } = Brushes.Lavender;

        public static double GridBorderWidthInFunctionGrid { get; } = 5;
        public static double GridBorderWidthInGroupGrid { get; } = 1;
        public static double GridBorderWidthInSpenGrid { get; } = 1;
        public static double GridBorderWidthInCodeBlocksGrid { get; } = 1;
        public static double GridBorderWidthInOperatorsGrid { get; } = 1;
        public static double NumberWidth { get; } = 30;
        public static double NumberWidthInHolstedMetricsGrids { get; } = 50;
        public static double AmountWidthInHolstedMetricsGrids { get; } = 50;
        public static double GroupWidth { get; } = 40;
        public static double SpenWidth { get; } = 50;

        public static double NameRowHeight { get; } = 30;
        public static double TemplateGridMaxHeight { get; } = 60;
        public static double ResultRowHeight { get; } = 50;
        public static double EmptyRowHeight { get; } = 30;

        public static double CaptionTextFontSize { get; } = 20;
        public static double TemplateTextFontSize { get; } = 18;
        public static double UsualTextFontSize { get; } = 15;
        public static double ResultTextFontSize { get; } = 18;
        public static double CodeFontSize { get; } = 16;      

        public static bool IsNumeratingOperators { get; set; } = false;
        public static bool IsCountingCaseValueAsOperand { get; set; } = true;

        public static List<string> Errors = new List<string>();
        #endregion

        #region Fields
        public static readonly Thickness NullThickness = new Thickness(0);
        public static readonly Thickness SpenGridMargin = new Thickness(0, 0, 0, 10);

        public static readonly char Multiply = '\u2219';
        public static readonly char Eta = '\u03B7';

        public static readonly Word SEMICOLON = new Word(";", false, false);
        public static readonly Word COLON = new Word(':');
        public static readonly Word REFERENCE = new Word('&');
        public static readonly Word DOLLAR = new Word('$');
        public static readonly Word QUESTION_MARK = new Word('?');
        public static readonly Word QUOTE = new Word('\"');

        public static readonly Word IF = new Word("if", false, false);
        public static readonly Word ELSEIF = new Word("elseif", false, false);
        public static readonly Word ELSE = new Word("else", false, false);
        public static readonly Word DO = new Word("do", false, false);
        public static readonly Word WHILE = new Word("while", false, false);
        public static readonly Word FOR = new Word("for", false, false);
        public static readonly Word SWITCH = new Word("switch", false, false);
        public static readonly Word SWITCH_WithBracket = new Word("switch(", false, false);
        public static readonly Word CASE = new Word("case", false, false);
        public static readonly Word DEFAULT = new Word("default", false, false);
        public static readonly Word DEFAULT_WithColon = new Word("default:", false, false);

        public static readonly Word RETURN = new Word("return", false, false);
        public static readonly Word FUNCTION = new Word("function", false, false);
        public static readonly Word TAG_Start = new Word("<?", false, false);
        public static readonly Word TAG_End = new Word("?>", false, false);

        public static readonly Word DEFINE = new Word("define", false, false);
        public static readonly Word ECHO = new Word("echo", false, false);
        public static readonly Word READLINE = new Word("readline", false, false);

        public static readonly Word EQUAL = new Word('=');
        public static readonly Word DOT_EQUAL = new Word(".=", false, false);
        public static readonly Word PLUS_EQUAL = new Word("+=", false, false);
        public static readonly Word MINUS_EQUAL = new Word("-=", false, false);
        public static readonly Word MULTIPLY_EQUAL = new Word("*=", false, false);
        public static readonly Word DIVIDE_EQUAL = new Word("/=", false, false);
        public static readonly Word PERCENT_EQUAL = new Word("%=", false, false);

        public static readonly Word INCREMENT = new Word("++", false, false);
        public static readonly Word DECREMENT = new Word("--", false, false);
   
        public static readonly Word SINGLELINE_COMMENT = new Word("//", false, false);
        public static readonly Word MULTILINE_COMMENT_Start = new Word("/*", false, false);
        public static readonly Word MULTILINE_COMMENT_End = new Word("*/", false, false);
        
        public static readonly Word[] EQUAL_AndCombinedOperators = new Word[]
        {
            EQUAL,
            DOT_EQUAL,
            PLUS_EQUAL,
            MINUS_EQUAL,
            MULTIPLY_EQUAL,
            DIVIDE_EQUAL,
            PERCENT_EQUAL
        };
        public static readonly Word[] IN_or_DECREMENT_Operators = new Word[] { INCREMENT, DECREMENT };
        public static readonly Word[] BLOCK_Operators = new Word[] { IF, ELSEIF, ELSE, SWITCH, DO, WHILE, FOR, CASE };
        public static readonly Word[] SWITCH_Parts = new Word[] { CASE, DEFAULT };
        public static readonly Word[] PreviousWordsForUnary_MINUS_Or_PLUS = new Word[]
        {
            '(',
            '[',
            '?',
            ':',
            '<',
            '>',
            "<>",
            '*',
            "**",
            '/',
            '%',
            "and",
            "or",
            "xor",
            "return",
        };
        public static readonly Word[] CanNotBeOperands = new Word[] {
            "<?php",
            "?>",
            "{",
            "}",
            "(",
            ")",
            "]",
            ":",
            ",",
            "case",
            "elseif",
            "else",
            "while"
        };
        public static readonly string[] EscapeSequences = new string[] { "\t", "\n", "\r", "\v", "\\e", "\f", "\\$" };
        public static readonly string[] Brackets = new string[] { "(", ")", "[", "]", "{", "}" };
        public static readonly string[] Characters = new string[] { ",", ";", ":", "\"", "\\", "?" };
        public static readonly string[] BlockOperators = new string[]
        {
            IF.Text,
            ELSEIF.Text,
            FOR.Text,
            WHILE.Text,
            SWITCH.Text
        };
        public static readonly string[] Operators = new string[]
        {
            "^", 
            "^-", 
            "~", 
            "@", 
            "+=", 
            "++", 
            "+", 
            "-=", 
            "--", 
            "-", 
            "**", 
            "**=", 
            "*=", 
            "*", 
            "/=", 
            "/", 
            "%=", 
            "%", 
            ".=", 
            ".", 
            "===",
            "!==",
            "<=>",
            "==",
            "!=",
            "<=",
            ">=",
            "=", 
            "!", 
            "&=", 
            "&&", 
            "||", 
            "|=", 
            "|", 
            "<<", 
            "<<=", 
            "<>",
            "<",
            ">>", 
            ">>=", 
            ">",
            "and", 
            "or", 
            "xor", 
            "instanceof", 
            "break", 
            "continue", 
            "return", 
            "require", 
            "include", 
            "goto", 
            "foreach", 
            "declare", 
            "require_once", 
            "include_once", 
            "if", 
            "for", 
            "switch", 
            "?", 
            "??", 
            ";", 
            "echo", 
            "readline", 
            "settype", 
            "count", 
            "rand", 
            "define", 
            "microtime",
            "["
            // &
            // ( 
            // while 
            // do 
        };

        public static readonly Code Code = new Code();

        public const string FILE = "info.dat";
        #endregion

        #region Methods
        public static void ThrowError(string message)
        {
            Errors.Add(message);
            throw new Exception(message);
        }
        #endregion
    }
}
