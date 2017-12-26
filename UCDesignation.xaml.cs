using System.Windows.Controls;
using System.Windows.Media;

namespace MSiSvInfT
{
    /// <summary>
    /// Логика взаимодействия для UCDesignation.xaml
    /// </summary>
    public partial class UCDesignation : UserControl
    {
        #region Properties
        public Brush LeftForeground
        {
            get { return Left.Foreground; }
            set { Left.Foreground = value; }
        }

        public Brush LeftBackground
        {
            get { return Left.Background; }
            set { Left.Background = value; }
        }

        public Brush RightForeground
        {
            get { return Right.Foreground; }
            set { Right.Foreground = value; }
        }

        public Brush RightBackground
        {
            get { return Right.Background; }
            set { Right.Background = value; }
        }

        public string LeftText
        {
            get { return Left.Text; }
            set { Left.Text = value; }
        }

        public string RightText
        {
            get { return Right.Text; }
            set { Right.Text = value; }
        }

        public double LeftWidth
        {
            get { return Left.Width; }
            set { Left.Width = value; }
        }

        public double RightWidth
        {
            get { return Right.Width; }
            set { Right.Width = value; }
        }
        #endregion

        #region Constructor
        public UCDesignation()
        {
            InitializeComponent();
            Right.Background = Center.Background;
        }
        #endregion
    }
}
