namespace MSiSvInfT
{
    public class Location
    {
        #region Properties
        public int LineIndex { get; set; }
        public int WordIndex { get; set; }
        #endregion

        #region Constructor
        public Location(int lineIndex, int wordIndex)
        {
            LineIndex = lineIndex;
            WordIndex = wordIndex;
        }
        #endregion

        #region Methods
        public override string ToString() => "Строка " + (LineIndex + 1).ToString("000") + "; Слово " + (WordIndex + 1).ToString("000") + ";";
        #endregion
    }
}
