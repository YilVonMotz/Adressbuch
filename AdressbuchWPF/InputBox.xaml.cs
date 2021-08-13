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

namespace AdressbuchWPF
{
    /// <summary>
    /// Interaktionslogik für InputBox.xaml
    /// </summary>
    public partial class InputBox : UserControl
    {
        private string value = string.Empty;
        private string label;
        public bool TextChanged { get; set; }

        public InputBox()
        {
            InitializeComponent();
        }

        public void SetLabel(string text)
        {
            label = text;
            this.InputBox_TextBlock.Text = text;
        }

        public string GetLabel()
        {
            return label;
        }
        

        public string GetInputText()
        {
            return value;
        }

        private void InputBox_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextChanged = true;
            value = ((TextBox)sender).Text;            
        }
    }
}
