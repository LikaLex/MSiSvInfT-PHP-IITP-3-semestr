using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;

namespace MSiSvInfT
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private OpenFileDialog dialog = new OpenFileDialog();
        private WindowLoading loading;
        private WindowJilbsMetric JilbsMetric;
        private WindowChapinsMetric ChapinsMetric;
        private WindowSpenMetric SpenMetric;
        private WindowHolstedMetrics HolstedMetrics;
        #endregion

        #region Events
        private event Action FileSelected;
        private event Action TasksPerformed;
        private event Action StartPerformingTasks;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            StartPerformingTasks += StartPerformingTasksHandler;
            TasksPerformed += TasksPerformedHandler;

            Data.Code.StartCodeChanging += StartCodeChanging_Handler;
            Data.Code.EndCodeChanging += EndCodeChanging_Handler;

            WPF_Methods.SetToolTip(ref RTBScrollViewer, "Для начала и завершения редактирования кода произведите двойной щелчок", Brushes.Linen);
        }
        #endregion

        #region Methods
        private void SetDesignations()
        {
            ConditionalOperatorDesignation.LeftWidth = 60;
            ConditionalOperatorDesignation.LeftForeground = Data.ConditionalOperatorsColor;
            ConditionalOperatorDesignation.LeftBackground = Data.CodeBackground;
            ConditionalOperatorDesignation.LeftText = "switch";
            ConditionalOperatorDesignation.RightText = "Условные операторы";

            OtherOperatorDesignation.LeftWidth = 60;
            OtherOperatorDesignation.LeftForeground = Data.OtherOperatorsColor;
            OtherOperatorDesignation.LeftBackground = Data.CodeBackground;
            OtherOperatorDesignation.LeftText = "+=";
            OtherOperatorDesignation.RightText = "Прочие операторы";

            StringDesignation.LeftWidth = 60;
            StringDesignation.LeftForeground = Data.StringColor;
            StringDesignation.LeftBackground = Data.CodeBackground;
            StringDesignation.LeftText = "\"abc\"";
            StringDesignation.RightText = "Строковые литералы";

            CommentDesignation.LeftWidth = 60;
            CommentDesignation.LeftBackground = Data.CommentColor;
            CommentDesignation.LeftText = "// ...";
            CommentDesignation.RightText = "Комментарии";

            ConditionalBlockDesignation.LeftWidth = 100;
            ConditionalBlockDesignation.LeftBackground = Data.BlockBackground;
            ConditionalBlockDesignation.LeftText = "if (...) { ... }";
            ConditionalBlockDesignation.RightText = "Блоки условных операторов";

            MaxNestedBlockDesignation.LeftWidth = 100;
            MaxNestedBlockDesignation.RightWidth = 300;
            MaxNestedBlockDesignation.LeftBackground = Data.MaxNestedBlockBackground;
            MaxNestedBlockDesignation.Left.HorizontalContentAlignment = HorizontalAlignment.Left;
            MaxNestedBlockDesignation.LeftText = "if (...) { ... }";
            MaxNestedBlockDesignation.RightText = "Условный оператор с максимальным уровнем вложенности";
        }

        private void SetFileDialog()
        {
            dialog.Filter = "Файлы php | *.php";
            if (File.Exists(Data.FILE))
            {
                try
                {
                    string[] data = File.ReadAllLines(Data.FILE);
                    if (data.Length != 1)
                        throw new FileFormatException();
                    string file = data[0];
                    if (!File.Exists(file))
                        throw new FileNotFoundException();
                    if (Path.GetExtension(file) != ".php")
                        throw new FileFormatException();
                    dialog.InitialDirectory = Path.GetDirectoryName(file);
                    dialog.FileName = file;                    
                }
                catch (Exception)
                {
                    SetDialogWithoutPreviousData();
                    return;
                }
                FileSelected?.Invoke();
            }
            else
                SetDialogWithoutPreviousData();         
        }
       
        private void SetDialogWithoutPreviousData()
        {
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
                FileSelected?.Invoke();
            else
                Application.Current.Shutdown();
        }
       
        private void PerformFirstTask()
        {
            FirstTask.Perform();
            JilbsMetric = new WindowJilbsMetric(FirstTask.GetConditionalOperatorsGrid());
            JilbsMetric.Closing += CloseHandler;           
        }

        private void PerformSecondTask()
        {
            SecondTask.Perform();         
            ChapinsMetric = new WindowChapinsMetric(Mode.GroupGrid);          
            ChapinsMetric.Closing += CloseHandler;
            SpenMetric = new WindowSpenMetric();
            SpenMetric.Closing += CloseHandler;
        }

        private void PerformThirdTask()
        {
            ThirdTask.Perform();
            HolstedMetrics = new WindowHolstedMetrics(ThirdTask.GetOperatorsGrid(), ThirdTask.GetOperatorsInLinesGrid(), ThirdTask.GetOperandsGrid(), ThirdTask.GetOperandsInLinesGrid());
            HolstedMetrics.Closing += CloseHandler;
        }

        private void PerformTasks()
        {
            Data.Errors = new List<string>();
            TBCaption.Foreground = Brushes.Black;
            TBCaption.Cursor = Cursors.Arrow;
            SetVisibilityOfButtons(Visibility.Visible);
            try
            {
                PerformFirstTask();
            }
            catch(Exception ex)
            {
                ButtonJilbMetric.Visibility = Visibility.Hidden;
                Data.Errors.Add(ex.Message);
            }
            try
            {
                PerformSecondTask();
            }
            catch(Exception ex)
            {
                ButtonSpenMetric.Visibility = Visibility.Hidden;
                ButtonChapinMetric.Visibility = Visibility.Hidden;
                Data.Errors.Add(ex.Message);
            }
            try
            {
                PerformThirdTask();
            }
            catch(Exception ex)
            {
                ButtonHolstedMetrics.Visibility = Visibility.Hidden;
                Data.Errors.Add(ex.Message);
            }
            if (Data.Errors.Count != 0)
            {
                TBCaption.Foreground = Brushes.Red;
                TBCaption.Cursor = Cursors.Hand;
            }
        }

        private void ShowWindow(Window window)
        {
            window.Visibility = Visibility.Visible;
            window.Activate();
        }

        private void SetVisibilityOfButtons(Visibility visibility)
        {
            ButtonJilbMetric.Visibility = visibility;
            ButtonSpenMetric.Visibility = visibility;
            ButtonChapinMetric.Visibility = visibility;
            ButtonHolstedMetrics.Visibility = visibility;
            ButtonSelectFile.Visibility = visibility;
        }
        #endregion

        #region Events Handlers
        private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
                FileSelected?.Invoke();
        }

        private void FileSelectedEventHandler()
        {
            StartPerformingTasks?.Invoke();
            File.WriteAllLines(Data.FILE, new string[] { dialog.FileName });
            TBFileName.Text = dialog.FileName;
            try
            {
                Data.Code.Lines = File.ReadAllLines(dialog.FileName);
            }
            catch (Exception ex)
            {
                TasksPerformed?.Invoke();
                MessageBox.Show("Произошла ошибка при форматировании кода:\n\"" + ex.Message + "\"", "Ошибка при форматировании кода");
                Data.Code.LoadFromFileForChanging(TBFileName.Text);
            }

            PerformTasks();
            TasksPerformed?.Invoke();       
        }             

        private void IsNumeratingOperators_Click(object sender, RoutedEventArgs e)
        {
            bool previous = Data.IsNumeratingOperators;
            Data.IsNumeratingOperators = IsNumeratingOperators.IsChecked;
            if (previous != Data.IsNumeratingOperators)
                PerformTasks();
        }

        private void CloseHandler(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ((Window)sender).Visibility = Visibility.Collapsed;
        }
                 
        private void Window_Closed(object sender, EventArgs e)
        {           
            try
            {
                JilbsMetric.Close();
                ChapinsMetric.Close();
                SpenMetric.Close();
                HolstedMetrics.Close();
            }
            catch (NullReferenceException) { }
            Application.Current.Shutdown();
        }

        private void StartPerformingTasksHandler()
        {
            loading = new WindowLoading();
            loading.Show();
        }

        private void TasksPerformedHandler() => loading.Close();
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RTBScrollViewer.Content = Data.Code.RTBCode;
            SetDesignations();
            Data.IsNumeratingOperators = IsNumeratingOperators.IsChecked;

            FileSelected += FileSelectedEventHandler;
            SetFileDialog();
        }

        private void ButtonJilbMetric_Click(object sender, RoutedEventArgs e) => ShowWindow(JilbsMetric);
        private void ButtonSpenMetric_Click(object sender, RoutedEventArgs e) => ShowWindow(SpenMetric);
        private void ButtonChapinMetric_Click(object sender, RoutedEventArgs e) => ShowWindow(ChapinsMetric);
        private void ButtonHolstedMetrics_Click(object sender, RoutedEventArgs e) => ShowWindow(HolstedMetrics);

        private void StartCodeChanging_Handler(object sender, EventArgs e)
        {
            SetVisibilityOfButtons(Visibility.Hidden);
            ButtonPerformTasks.Visibility = Visibility.Visible;
        }

        private void EndCodeChanging_Handler(object sender, EventArgs e)
        {
            SetVisibilityOfButtons(Visibility.Visible);
            ButtonPerformTasks.Visibility = Visibility.Hidden;
            Data.Code.RTBCode.IsReadOnly = true;
            Data.Code.RTBCode.Cursor = Cursors.Arrow;
            string fileName = TBFileName.Text;
            File.Delete(fileName);
            using (FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                TextRange text = new TextRange(Data.Code.RTBCode.Document.ContentStart, Data.Code.RTBCode.Document.ContentEnd);
                text.Save(fs, DataFormats.Text);
            }
            FileSelectedEventHandler();
        }

        private void ButtonPerformTasks_Click(object sender, RoutedEventArgs e) => EndCodeChanging_Handler(sender, e);

        private void TBCaption_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (Data.Errors.Count == 0)
                return;
            string errorsMessage = string.Join("\n", Data.Errors.ToArray());
            MessageBox.Show(errorsMessage, "Ошибки");
        }
        #endregion
    }
}
