using System.Windows.Media;
using System.Windows.Documents;

namespace MSiSvInfT
{
    using Spans = SpanMethods;

    public static class SubsidiaryMethodsForSearchingOperators
    {
        private static void AddOperator(Word @operator, Brush color, ref Span span, bool isNumerating)
        {
            SelectOperator(@operator, color, ref span, isNumerating);
            FirstTask.OperatorsCount++;
        }
        public static void AddOperator(Word @operator, Brush color, ref Span span) => AddOperator(@operator, color, ref span, Data.IsNumeratingOperators);

        private static void SelectOperator(Word @operator, Brush color, ref Span span, bool isNumerating)
            => span.Inlines.Add(Spans.Operator(@operator.ToString(), color, isNumerating));
        public static void SelectOperator(Word @operator, Brush color, ref Span span)
            => SelectOperator(@operator, color, ref span, false);

        public static bool ConditionalCompare(Word word, string OPERATOR, ref Span span, int lineNumber)
        {
            if (word.Text == OPERATOR)
            {
                AddOperator(word, Data.ConditionalOperatorsColor, ref span);
                return true;
            }
            if (word.Text.Contains(OPERATOR + "("))
            {
                AddOperator(word, Data.ConditionalOperatorsColor, ref span);
                return true;
            }
            return false;
        }

    }
}
