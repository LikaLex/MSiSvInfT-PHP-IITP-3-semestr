using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;

namespace MSiSvInfT
{
    public static class SpanMethods
    {
        #region Methods
        public static Span Operator(string OperatorName, Brush brush, bool IsNumerating)
        {
            Span @operator = new Span();
            @operator.Inlines.Add(OperatorName);
            if (IsNumerating)
                @operator.Inlines.Add(" (" + (FirstTask.OperatorsCount + 1).ToString() + ") ");
            @operator.FontSize = Data.CodeFontSize;
            @operator.FontWeight = FontWeights.Bold;
            @operator.Foreground = brush;
            return @operator;
        }

        public static Span String(string line, bool needsToAddLeftQuote, bool needsToAddRightQuote)
        {
            Span span = new Span();
            if (needsToAddLeftQuote)
                line = "\"" + line;
            if (needsToAddRightQuote)
                line += "\"";
            span.Inlines.Add(line);
            span.FontSize = Data.CodeFontSize;
            span.Foreground = Data.StringColor;
            return span;
        }

        public static Span Comment(string line, int? lineNumber)
        {
            Span span = new Span();
            if (lineNumber.HasValue)
                span.Inlines.Add(LineNumberSpan(lineNumber.Value));
            span.Inlines.Add(line);
            span.FontSize = Data.CodeFontSize;
            span.Background = Data.CommentColor;
            return span;
        }

        public static Span LineNumberSpan(int number)
        {
            Span span = new Span();
            if (number != 1)
                span.Inlines.Add("\n");
            span.Inlines.Add(number.ToString("000"));
            Span gapSpan = new Span();
            gapSpan.Inlines.Add("  ");
            gapSpan.Background = Data.CodeBackground;
            span.Inlines.Add(gapSpan);
            span.FontSize = Data.CodeFontSize;
            span.Background = Data.LineNumberBackground;
            return span;
        }
        #endregion
    }
}
