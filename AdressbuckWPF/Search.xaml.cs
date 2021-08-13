using System;
using System.Collections.Generic;
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


namespace AdressbuckWPF
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class Search : UserControl
    {
        public Search(RemoveButtonClickedDel removeButtonClickedDel, ComboBoxClosedDel dropDownClosedDel, TextBoxEntryDel textBoxEntryDel)
        {
            InitializeComponent();
            OnRemoveButtonClicked += removeButtonClickedDel;
            OnDropDownClosed += dropDownClosedDel;
            OnTextBoxEntry += textBoxEntryDel;
            comboBoxElement.ItemsSource = MainWindow.availableFields;
            
        }
                

        public ComboBox GetComboBox() { return this.comboBoxElement;  }        
        public TextBox GetTextBox() { return this.textBoxElement;  }
        public TextBlock GetTextBlock() { return this.textBlockElement; }
               
        public delegate void RemoveButtonClickedDel(Search search);
        public RemoveButtonClickedDel OnRemoveButtonClicked;

        public delegate void ComboBoxClosedDel(Search search);
        public ComboBoxClosedDel OnDropDownClosed;

        public delegate int TextBoxEntryDel(Search search);
        public TextBoxEntryDel OnTextBoxEntry;

        public delegate void SelectionChangedDel(ComboBox comboBox);
        public static SelectionChangedDel OnSelectionChanged; 

        private bool firstFocus = true;

        private string lastSelectedItem = string.Empty;

        private List<string> internalAvailable ;

       

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveButtonClicked(this);
        }



        private void ComboBoxElement_DropDownClosed(object sender, EventArgs e)
        {
            if(this.comboBoxElement.SelectedItem == null )
            {
                return;
            }
            internalAvailable = new List<string>();
            internalAvailable.Add(comboBoxElement.SelectedItem.ToString());
            internalAvailable.AddRange(MainWindow.availableFields);            

            OnDropDownClosed(this);
        }



        private void TextBoxElement_LostFocus(object sender, RoutedEventArgs e)
        {
            if (comboBoxElement.SelectedItem == null)
                return;
            OnTextBoxEntry(this);
        }



        private void TextBoxElement_GotFocus(object sender, RoutedEventArgs e)
        {           

            if (firstFocus)
            {
                this.textBoxElement.Text = String.Empty;
                firstFocus = false;
            }
            
        }

        

       

    }
}
