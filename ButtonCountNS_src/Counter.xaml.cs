using DesktopWPFAppLowLevelKeyboardHook;
using NintendoSpy.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace NintendoSpy
{
    /// <summary>
    /// Interaction logic for Counter.xaml
    /// </summary>
    public partial class Counter : Window
    {

        

        public Key reset = (Key)(Properties.Settings.Default.ResetKey);

        public void setKey(Key k)
        {
            reset = k;
        }

        int aPress = 0;
        int bPress = 0;
        int xPress = 0;
        int yPress = 0;
        int zPress = 0;
        int lPress = 0;
        int rPress = 0;
        int stPress = 0;
        int clPress = 0;
        int cdPress = 0;

        //Image aPng = Image.FromFile("");

        

        public LowLevelKeyboardListener _listener;

        public Counter()
        {
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
        }

        //col is background colour, flase = black, true = white
        Boolean col = Properties.Settings.Default.BkgrnColour;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;

            _listener.HookKeyboard();




            //on load settings
            
            if (col) { lightDark(col); }
            if (Properties.Settings.Default.Chroma) { BCWindow.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); }
            if (Properties.Settings.Default.n64) { n64Mode(); }
            if (Properties.Settings.Default.WideLayout) { Widen(); }

            
            
        }

        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == reset)
            {
                aPress = 0;
                bPress = 0;
                xPress = 0;
                yPress = 0;
                zPress = 0;
                lPress = 0;
                rPress = 0;
                stPress = 0;
                clPress = 0;
                cdPress = 0;
                aButton.Content = bButton.Content = xButton.Content = yButton.Content = zButton.Content = lButton.Content = rButton.Content = stButton.Content = cButton.Content = c2Button.Content = 0;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            HotkeySetter hks = new HotkeySetter(reset);
            hks.ShowDialog();

            reset = hks.resetter;
        }


        
        private void MenuItem_LightDark(object sender, RoutedEventArgs e)
        {
            col = !col;
            Properties.Settings.Default.BkgrnColour = col;
            Properties.Settings.Default.Chroma = false;
            Properties.Settings.Default.Save();
            lightDark(col);
        }

        void lightDark(bool col)
        {
            SolidColorBrush c1;
            SolidColorBrush c2;
            if (col)
            {
                c1 = new SolidColorBrush(Colors.Black);
                c2 = new SolidColorBrush(Colors.White);
                LMMode.Header = "Dark Mode";
            }
            else
            {
                c1 = new SolidColorBrush(Colors.White);
                c2 = new SolidColorBrush(Colors.Black);
                LMMode.Header = "Light Mode";
            }

            aButton.Foreground = c1;
            bButton.Foreground = c1;
            xButton.Foreground = c1;
            yButton.Foreground = c1;
            zButton.Foreground = c1;
            lButton.Foreground = c1;
            rButton.Foreground = c1;
            stButton.Foreground = c1;
            cButton.Foreground = c1;
            c2Button.Foreground = c1;
            label.Foreground = c1;
            aps.Foreground = c1;
            BCWindow.Background = c2;
        }


        private void MenuItem_Chroma(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Chroma = true;
            Properties.Settings.Default.Save();
            BCWindow.Background = new SolidColorBrush(Color.FromRgb(0,255,0));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            //e.Cancel = true;
            //Hide();
            _listener.UnHookKeyboard();
        }






        //short table
        public static int[] ticker = new int[30];
        public static int tickertracker = 1;
        public static int actionsPerSeconds;

        DispatcherTimer _timer;

        public void Start()
        {
            if (_timer != null) return;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += tick;
            _timer.Start();

            //loading the table
            for (int i = 0; i < ticker.Length; i++)
            {
                ticker[i] = 0;
            }
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }




        void tick(object sender, EventArgs e)
        {
            //this is the other important bit
            actionsPerSeconds = actionsPerSeconds - ticker[(tickertracker + 1) % 30];
            ticker[(tickertracker + 1) % 30] = 0;
            tickertracker++;
            if (tickertracker == 30) tickertracker = 0;
            aps.Content = actionsPerSeconds;
        }

        bool boa = false;
        bool bob = false;
        bool box = false;
        bool boy = false;
        bool boz = false;
        bool bol = false;
        bool bor = false;
        bool bos = false;

        bool bocl = false;
        bool bocd = false;


        bool n64interpret = false;

        public void buttonDown(String s)
        {
            //in future change this to Switch/Case
            if (n64interpret)
            {
                if (s == "b") s = "x";
                else if (s == "z") s = "b";
                else if (s == "start") s = "y";
                else if (s == "r") s = "z";
                else if (s == "cup") s = "r";
                else if (s == "cright") s = "start";

                else if (s == "x") s = null;
                else if (s == "y") s = null;
            }

            if (s == "a" && !boa)
            {
                boa = true;
                aButton.Content = ++aPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "b" && !bob)
            {
                bob = true;
                bButton.Content = ++bPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "x" && !box)
            {
                box = true;
                xButton.Content = ++xPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "y" && !boy)
            {
                boy = true;
                yButton.Content = ++yPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "z" && !boz)
            {
                boz = true;
                zButton.Content = ++zPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "l" && !bol)
            {
                bol = true;
                lButton.Content = ++lPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "r" && !bor)
            {
                bor = true;
                rButton.Content = ++rPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "start" && !bos)
            {
                bos = true;
                stButton.Content = ++stPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "cleft" && !bocl)
            {
                bocl = true;
                cButton.Content = ++clPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

            else if (s == "cdown" && !bocd)
            {
                bocd = true;
                c2Button.Content = ++cdPress;
                actionsPerSeconds++;
                ticker[tickertracker]++;
            }

        }

        public void buttonUp(string s)
        {
            if (n64interpret)
            {
                if (s == "b") s = "x";
                else if (s == "z") s = "b";
                else if (s == "start") s = "y";
                else if (s == "r") s = "z";
                else if (s == "cup") s = "r";
                else if (s == "cright") s = "start";

                else if (s == "x") s = null;
                else if (s == "y") s = null;

            }

            if (s == "a") boa = false;
            if (s == "b") bob = false;
            if (s == "x") box = false;
            if (s == "y") boy = false;
            if (s == "z") boz = false;
            if (s == "l") bol = false;
            if (s == "r") bor = false;
            if (s == "start") bos = false;
            if (s == "cleft") bocl = false;
            if (s == "cdown") bocd = false;
        }



        public void MenuItem_n64Mode(object sender, EventArgs e)
        {
            n64Mode();
        }

        void n64Mode()
        {
            if (!n64interpret)
            {
                abutn.Visibility = Visibility.Visible;
                bbutn.Visibility = Visibility.Visible;
                zbutn.Visibility = Visibility.Visible;
                sbutn.Visibility = Visibility.Visible;
                lbutn.Visibility = Visibility.Visible;
                rbutn.Visibility = Visibility.Visible;
                cubutn.Visibility = Visibility.Visible;
                cdbutn.Visibility = Visibility.Visible;
                clbutn.Visibility = Visibility.Visible;
                crbutn.Visibility = Visibility.Visible;

                abutg.Visibility = Visibility.Hidden;
                bbutg.Visibility = Visibility.Hidden;
                xbutg.Visibility = Visibility.Hidden;
                ybutg.Visibility = Visibility.Hidden;
                zbutg.Visibility = Visibility.Hidden;
                lbutg.Visibility = Visibility.Hidden;
                rbutg.Visibility = Visibility.Hidden;
                sbutg.Visibility = Visibility.Hidden;

                cButton.Visibility = Visibility.Visible;
                c2Button.Visibility = Visibility.Visible;

                n64interpret = true;
                Properties.Settings.Default.n64 = n64interpret;
                Properties.Settings.Default.Save();

                n64mi.Header = "Gamecube Buttons";
            }
            else
            {
                abutn.Visibility = Visibility.Hidden;
                bbutn.Visibility = Visibility.Hidden;
                zbutn.Visibility = Visibility.Hidden;
                sbutn.Visibility = Visibility.Hidden;
                lbutn.Visibility = Visibility.Hidden;
                rbutn.Visibility = Visibility.Hidden;
                cubutn.Visibility = Visibility.Hidden;
                cdbutn.Visibility = Visibility.Hidden;
                clbutn.Visibility = Visibility.Hidden;
                crbutn.Visibility = Visibility.Hidden;

                abutg.Visibility = Visibility.Visible;
                bbutg.Visibility = Visibility.Visible;
                xbutg.Visibility = Visibility.Visible;
                ybutg.Visibility = Visibility.Visible;
                zbutg.Visibility = Visibility.Visible;
                lbutg.Visibility = Visibility.Visible;
                rbutg.Visibility = Visibility.Visible;
                sbutg.Visibility = Visibility.Visible;

                cButton.Visibility = Visibility.Hidden;
                c2Button.Visibility = Visibility.Hidden;

                n64interpret = false;
                Properties.Settings.Default.n64 = n64interpret;
                Properties.Settings.Default.Save();

                n64mi.Header = "N64 Buttons";
            }
        }

        private void MenuItem_Wide(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.WideLayout = !Properties.Settings.Default.WideLayout;
            Properties.Settings.Default.Save();
            if (Properties.Settings.Default.WideLayout)
            {
                Widen();
                
            } else
            {
                Normalen();
            }
        }

        void Widen()
        {
            LayoutSwitch.Header = "Default Layout";
            CounterGrid.Width = 350;
            //CounterGrid.Height = 200;
            lbutg.Margin = new System.Windows.Thickness(13, 23, 0, 0);
            lButton.Margin = new System.Windows.Thickness(9, 38, 0, 0);

            abutg.Margin = new System.Windows.Thickness(74, 21, 0, 0);
            aButton.Margin = new System.Windows.Thickness(69, 38, 0, 0);

            bbutg.Margin = new System.Windows.Thickness(134, 21, 0, 0);
            bButton.Margin = new System.Windows.Thickness(129, 38, 0, 0);

            rbutg.Margin = new System.Windows.Thickness(193, 23, 0, 0);
            rButton.Margin = new System.Windows.Thickness(189, 38, 0, 0);

            sbutg.Margin = new System.Windows.Thickness(254, 25, 0, 0);
            stButton.Margin = new System.Windows.Thickness(249, 38, 0, 0);

            xbutg.Margin = new System.Windows.Thickness(15, 75, 0, 0);
            xButton.Margin = new System.Windows.Thickness(13, 96, 0, 0);

            ybutg.Margin = new System.Windows.Thickness(73, 81, 0, 0);
            yButton.Margin = new System.Windows.Thickness(69, 96, 0, 0);

            zbutg.Margin = new System.Windows.Thickness(134, 79, 0, 0);
            zButton.Margin = new System.Windows.Thickness(129, 96, 0, 0);

            label.Margin = new System.Windows.Thickness(209, 89, 0, 0);
            aps.Margin = new System.Windows.Thickness(257, 89, 0, 0);
        }

        void Normalen()
        {
            LayoutSwitch.Header = "Wide Layout";
            CounterGrid.Width = 250;
            lbutg.Margin = new System.Windows.Thickness(11, 95, 0, 0);
            lButton.Margin = new System.Windows.Thickness(50, 84, 0, 0);

            abutg.Margin = new System.Windows.Thickness(13, 22, 0, 0);
            aButton.Margin = new System.Windows.Thickness(50, 15, 0, 0);

            bbutg.Margin = new System.Windows.Thickness(13, 57, 0, 0);
            bButton.Margin = new System.Windows.Thickness(50, 49, 0, 0);

            rbutg.Margin = new System.Windows.Thickness(9, 126, 0, 0);
            rButton.Margin = new System.Windows.Thickness(50, 116, 0, 0);

            sbutg.Margin = new System.Windows.Thickness(121, 128, 0, 0);
            stButton.Margin = new System.Windows.Thickness(161, 117, 0, 0);

            xbutg.Margin = new System.Windows.Thickness(127, 20, 0, 0);
            xButton.Margin = new System.Windows.Thickness(161, 13, 0, 0);

            ybutg.Margin = new System.Windows.Thickness(122, 60, 0, 0);
            yButton.Margin = new System.Windows.Thickness(161, 49, 0, 0);

            zbutg.Margin = new System.Windows.Thickness(123, 89, 0, 0);
            zButton.Margin = new System.Windows.Thickness(161, 84, 0, 0);

            aps.Margin = new System.Windows.Thickness(77, 181, 0, 0);
            label.Margin = new System.Windows.Thickness(9, 181, 0, 0);
        }



        private void MenuItem_1000(object sender, RoutedEventArgs e)
        {
            aButton.Content = 1000;
            bButton.Content = 1000;
            xButton.Content = 1000;
            yButton.Content = 1000;
            zButton.Content = 1000;
            lButton.Content = 1000;
            rButton.Content = 1000;
            stButton.Content = 1000;
        }
    }
}
