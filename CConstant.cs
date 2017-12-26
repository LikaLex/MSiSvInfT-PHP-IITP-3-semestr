namespace MSiSvInfT
{
    public class Constant : Variable
    {
        #region Constructors
        public Constant(Word name) : base(null, name, false) { }
        #endregion

        #region Methods
        public override bool Equals(object obj) => Name.Equals(obj);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name.ToString();
        #endregion

        #region Operators
        public static bool operator ==(Constant first, Constant second) => first.Name == second.Name;
        public static bool operator !=(Constant first, Constant second) => first.Name != second.Name;
        #endregion
    }
}
