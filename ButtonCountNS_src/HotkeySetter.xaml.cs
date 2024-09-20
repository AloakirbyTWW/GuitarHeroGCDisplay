using DesktopWPFAppLowLevelKeyboardHook;
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
using System.Windows.Navigation;

namespace NintendoSpy
{
    /// <summary>
    /// Interaction logic for HotkeySetter.xaml
    /// </summary>
    public partial class HotkeySetter : Window
    {
        public Key resetter;


        public HotkeySetter(Key r)
        {
            InitializeComponent();
            resetter = r;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            hklabel.Content = e.Key;
            resetter = e.Key;
            //MessageBox.Show(e.Key.GetHashCode() + "");
            Properties.Settings.Default.ResetKey = e.Key.GetHashCode();
            Properties.Settings.Default.Save();

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
    }
}