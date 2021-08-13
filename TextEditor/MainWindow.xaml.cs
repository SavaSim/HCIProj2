using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;

namespace TextEditor
{
    /// <summary>
   
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            cmbColorPicker.SelectedColor = (rtbEditor.Foreground as SolidColorBrush).Color;

        }
        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //odje je bold
            object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            //odje je italic
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            //odje je underline
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            //odje je familija fontova
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cmbFontFamily.SelectedItem = temp;

            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cmbFontSize.SelectedItem = temp;

            wordCount();

            try
            {
                cmbColorPicker.SelectedColor = (rtbEditor.Selection.GetPropertyValue(Inline.ForegroundProperty) as SolidColorBrush).Color;//exception
                TextRange t = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void Date_Executed(object sender, RoutedEventArgs e)
        {
            rtbEditor.CaretPosition.InsertTextInRun(DateTime.Now.ToString());
            rtbEditor.CaretPosition = rtbEditor.CaretPosition.GetPositionAtOffset(DateTime.Now.ToString().Length);
        }
        private int countWords(string s)
        {
            int counter = 0;
            char[] delimiters = new char[] { ' ', '\r', '\t', '\n' };
            if (String.IsNullOrWhiteSpace(s))
            {
                counter = 0;
            }
            else
            {
                counter = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
            }

            return counter;
        }

        private void wordCount()
        {
            TextRange textRange = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            int counter = countWords(textRange.Text);
            StatusBarTextBlock.Text = counter.ToString();
        }
        private void New_Executed(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save this document before exiting?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Rtf);
                }
            }
            rtbEditor.Document.Blocks.Clear();
        }
        private void FindAndReplace(object sender, RoutedEventArgs e)
        {

              string keyword = findTextBox.Text;
              string newString = replaceTextBox.Text;
              TextRange text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
              TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
              while (current!=null)
	          {
                string textInRun = current.GetTextInRun(LogicalDirection.Forward);
                if (!string.IsNullOrWhiteSpace(textInRun))
                {
                  int index = textInRun.IndexOf(keyword);
                  if (index!=-1)
                  {
                    TextPointer selectionStart = current.GetPositionAtOffset(index,LogicalDirection.Forward);
                    TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length,LogicalDirection.Forward);
                    TextRange selection = new TextRange(selectionStart, selectionEnd);
                    selection.Text = newString;
                    
                    rtbEditor.Selection.Select(selection.Start, selection.End);
                    rtbEditor.Focus();
                  }
                }
                current = current.GetNextContextPosition(LogicalDirection.Forward);
	          }
            findAndReplace.IsChecked = false;
        }
        public void cmbColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(cmbColorPicker.SelectedColor != null)
            {
                SolidColorBrush sb = new SolidColorBrush((Color)cmbColorPicker.SelectedColor);
                rtbEditor.Selection.ApplyPropertyValue(Inline.ForegroundProperty, sb);
            }
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Rtf);
            }
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save this document before opening another one?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Rtf);
                }

                // otvaranje posle save-a
                OpenFileDialog dlg1 = new OpenFileDialog();
                dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dlg1.FileName, FileMode.Open);
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Rtf);
                }
            }
            else
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Rtf);
                }
            }
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }
        private void cmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
       
            if (cmbFontSize.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.SelectedItem);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save this document before exiting?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
                if (dlg.ShowDialog() == true)
                {
                    FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                    TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Rtf);
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
    }
}
