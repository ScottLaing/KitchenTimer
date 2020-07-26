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
using System.Windows.Shapes;

namespace KitchenTimer.Windows
{
    /// <summary>
    /// Interaction logic for SetTime.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public double TimeValue { get; set; } = 0;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SetTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
              
                TimeValue = Convert.ToDouble(this.txtSetTime2.Text);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Some error with saving: {ex.Message}.");
            }
        }
    }
}
