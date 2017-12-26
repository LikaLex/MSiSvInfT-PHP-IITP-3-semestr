using System.Collections.Generic;

namespace MSiSvInfT
{
    public static class Operators
    {
        #region Constructor
        static Operators()
        {
            CombineOperators();
        }
        #endregion

        #region Fields
        private static string[] Unique = new string[]
        {
            "~",
            "@",
            "??",
        };

        private static string[] With_Up = new string[]
        {
            "^=",
            "^"
        };

        private static string[] With_Plus = new string[]
        {
            "+=",
            "++",
            "+"
        };

        private static string[] With_Minus = new string[]
        {
            "-=",
            "--",
            "-"
        };

        private static string[] With_Multiply = new string[]
        {
            "**=",
            "**",
            "*=",
            "*"
        };

        private static string[] With_Divide = new string[]
        {
            "/=",
            "/"
        };

        private static string[] With_Percent = new string[]
        {
            "%=",
            "%"
        };

        private static string[] With_Dot = new string[]
        {
            ".=",
            "."
        };

        private static string[] With_Equal = new string[]
        {
            "===",
            "**=",
            "!==",
            "<<=",
            ">>=",
            "<=>",
            "==",
            "!=",
            "<=",
            ">=",
            ".=",
            "+=",
            "*=",
            "/=",
            "%=",
            "&=",
            "|=",
            "^=",
            "="
        };

        private static string[] With_Exclamation = new string[]
        {
            "!==",
            "!=",
            "!"
        };

        private static string[] With_Ampersand = new string[]
        {
            "&&",
            "&=",
            "&"
        };

        private static string[] With_VerticalLine = new string[]
        {
            "||",
            "|=",
            "|"
        };

        private static string[] With_Less = new string[]
        {
            "<<=",
            "<=>",
            "<<",
            "<>",
            "<=",
            "<"
        };

        private static string[] With_Bigger = new string[]
        {
            ">>=",
            "<=>",
            ">>",
            "<>",
            ">=",
            ">"
        };

        public static List<string[]> Symbolic = new List<string[]>();

        public static string[] WordWithoutBrackets = new string[] {
            "and",
            "or",
            "xor",
            "instanceof",
            "break",
            "continue",
            "return",
            "require",
            "include",
            "require_once",
            "include_once",
            "goto"
        };

        public static string[] WordWithBrackets = new string[]
        {
            "foreach",
            "require",
            "include",
            "declare",
            "require_once",
            "include_once",
        };

        public static string[] SingleWords = new string[]
        {
            "break",
            "continue",
            "return",
        };
        #endregion

        #region Methods
        private static void CombineOperators()
        {
            Symbolic.Add(Unique);
            Symbolic.Add(With_Up);
            Symbolic.Add(With_Plus);
            Symbolic.Add(With_Minus);
            Symbolic.Add(With_Multiply);
            Symbolic.Add(With_Divide);
            Symbolic.Add(With_Percent);
            Symbolic.Add(With_Dot);
            Symbolic.Add(With_Equal);
            Symbolic.Add(With_Exclamation);
            Symbolic.Add(With_Ampersand);
            Symbolic.Add(With_VerticalLine);
            Symbolic.Add(With_Less);
            Symbolic.Add(With_Bigger);
        }
        #endregion
    }
}
