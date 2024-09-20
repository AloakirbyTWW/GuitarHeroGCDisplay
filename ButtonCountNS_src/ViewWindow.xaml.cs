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
using System.ComponentModel;
using System.Windows.Shapes;

using NintendoSpy.Readers;


//My SHIT
using System.IO;
using System.Xml.Linq;
using System.Xml;
//using System.Drawing;



namespace NintendoSpy
{
    public partial class ViewWindow : Window, INotifyPropertyChanged
    {
        Skin _skin;
        IControllerReader _reader;
        Keybindings _keybindings;
        BlinkReductionFilter _blinkFilter = new BlinkReductionFilter();

        List<Tuple<Skin.Button, Image, LinkedList<Tuple<Image, Skin.LifeTime>>, Image>> _buttonsWithImages = new List<Tuple<Skin.Button, Image, LinkedList<Tuple<Image, Skin.LifeTime>>, Image>>();
        List<Tuple<Skin.RangeButton, Image>> _rangeButtonsWithImages = new List<Tuple<Skin.RangeButton, Image>>();
        List<Tuple<Skin.AnalogStick, Image>> _sticksWithImages = new List<Tuple<Skin.AnalogStick, Image>>();

        // The triggers images are embedded inside of a Grid element so that we can properly mask leftwards and upwards
        // without the image aligning to the top left of its element.
        List<Tuple<Skin.AnalogTrigger, Grid, LinkedList<Tuple<Image, Skin.LifeTime>>, Image>> _triggersWithGridImages = new List<Tuple<Skin.AnalogTrigger, Grid, LinkedList<Tuple<Image, Skin.LifeTime>>, Image>>();


        /// Expose the enabled status of the low-pass filter for data binding.
        public bool BlinkReductionEnabled {
            get { return _blinkFilter.Enabled; }
            set { _blinkFilter.Enabled = value; OnPropertyChanged("BlinkReductionEnabled"); }
        }


        //============================
        //this shit is important I guess
        //============================
        Counter bc = new Counter();

        static public uint flyingSpeed = 2;
        static public double hight = 4;
        static public double opacity = 1;


        public ViewWindow(Skin skin, Skin.Background skinBackground, IControllerReader reader)
        {
            //MessageBox.Show(skin.Type.Name.ToString() + "");

            /*if(skin.Type.Name.ToString().Equals("Nintendo64"))
            {
                Counter.n64Mode();
            }*/

            InitializeComponent();
            DataContext = this;

            _skin = skin;
            _reader = reader;



            ControllerGrid.Width = skinBackground.Image.PixelWidth;
            ControllerGrid.Height = skinBackground.Image.PixelHeight;

            var brush = new ImageBrush(skinBackground.Image);
            brush.Stretch = Stretch.Uniform;
            ControllerGrid.Background = brush;

            foreach (var trigger in _skin.AnalogTriggers)
            {
                var grid = getGridForAnalogTrigger(trigger);

                var flyingImages = new LinkedList<Tuple<Image, Skin.LifeTime>>();
                var flyingImage = getImageForElement(trigger.FlyingConfig);

                _triggersWithGridImages.Add(new Tuple<Skin.AnalogTrigger, Grid, LinkedList<Tuple<Image, Skin.LifeTime>>, Image>(trigger, grid, flyingImages, flyingImage));
                ControllerGrid.Children.Add(grid);
            }

            foreach (var button in _skin.Buttons)
            {
                var image = getImageForElement(button.ButtonConfig);
                image.Visibility = Visibility.Hidden;
                ControllerGrid.Children.Add(image);

                var flyingImages = new LinkedList<Tuple<Image, Skin.LifeTime>>();
                var flyingImage = getImageForElement(button.FlyingConfig);

                _buttonsWithImages.Add(new Tuple<Skin.Button, Image, LinkedList<Tuple<Image, Skin.LifeTime>>, Image>(button, image, flyingImages, flyingImage));
            }

            foreach (var button in _skin.RangeButtons) {
                var image = getImageForElement(button.Config);
                _rangeButtonsWithImages.Add(new Tuple<Skin.RangeButton, Image>(button, image));
                image.Visibility = Visibility.Hidden;
                ControllerGrid.Children.Add(image);
            }

            foreach (var stick in _skin.AnalogSticks) {
                var image = getImageForElement(stick.Config);
                _sticksWithImages.Add(new Tuple<Skin.AnalogStick, Image>(stick, image));
                ControllerGrid.Children.Add(image);
            }

            _reader.ControllerStateChanged += reader_ControllerStateChanged;
            _reader.ControllerDisconnected += reader_ControllerDisconnected;

            try {
                _keybindings = new Keybindings(Keybindings.XML_FILE_PATH, _reader);
            } catch (ConfigParseException) {
                MessageBox.Show("Error parsing keybindings.xml. Not binding any keys to gamepad inputs");
            }
        }


        static Image getImageForElement(Skin.ElementConfig config)
        {
            var img = new Image();
            img.VerticalAlignment = VerticalAlignment.Top;
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Source = config.Image;
            img.Stretch = Stretch.Fill;
            img.Margin = new Thickness(config.X, config.Y, 0, 0);
            img.Width = config.Width;
            img.Height = config.Height;
            return img;
        }

        static Grid getGridForAnalogTrigger (Skin.AnalogTrigger trigger)
        {
            var img = new Image ();

            img.VerticalAlignment = VerticalAlignment.Top;

            img.HorizontalAlignment = 
                  trigger.Direction == Skin.AnalogTrigger.DirectionValue.Left
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;

            img.VerticalAlignment = 
                  trigger.Direction == Skin.AnalogTrigger.DirectionValue.Up
                ? VerticalAlignment.Bottom
                : VerticalAlignment.Top;

            img.Source = trigger.ButtonConfig.Image;
            img.Stretch = Stretch.None;
            img.Margin = new Thickness (0, 0, 0, 0);
            img.Width = trigger.ButtonConfig.Width;
            img.Height = trigger.ButtonConfig.Height;

            var grid = new Grid ();

            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.Margin = new Thickness (trigger.ButtonConfig.X, trigger.ButtonConfig.Y, 0, 0);
            grid.Width = trigger.ButtonConfig.Width;
            grid.Height = trigger.ButtonConfig.Height;

            grid.Children.Add (img);

            return grid;
        }

        void AlwaysOnTop_Click (object sender, RoutedEventArgs e) {
            this.Topmost = !this.Topmost;
        }

        void BlinkReductionEnabled_Click (object sender, RoutedEventArgs e) {
            this.BlinkReductionEnabled = !this.BlinkReductionEnabled;
        }


        void reader_ControllerDisconnected (object sender, EventArgs e)
        {
            Close ();
        }

        void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_keybindings != null) {
                _keybindings.Finish ();
            }
            _reader.Finish ();

            //============================
            //close the button counter when NintendoSpy closes
            //============================
            bc.Close();
            bc.Stop();

            
        }


        void reader_ControllerStateChanged (IControllerReader reader, ControllerState newState)
        {
            newState = _blinkFilter.Process(newState);

            foreach (var button in _buttonsWithImages) //Binary Buttons => A / B / Y / X ... R fully / L fully
            {
                //MyShit
                var skinButton = button.Item1;
                var flyingImages = button.Item3;

                if (!newState.Buttons.ContainsKey(button.Item1.Name)) continue;

                button.Item2.Visibility = newState.Buttons[button.Item1.Name] ? Visibility.Visible : Visibility.Hidden;

                //============================
                //this is the important bit
                //============================
                //
                if (newState.Buttons[button.Item1.Name])
                {
                    bc.buttonDown(button.Item1.Name);

                    var newImage = getImageForElement(skinButton.FlyingConfig);
                    newImage.Margin = new Thickness(skinButton.FlyingConfig.X, 0, 0, 0);

                    if (button.Item1.Name == "up" || button.Item1.Name == "left" || button.Item1.Name == "down" || button.Item1.Name == "right") {
                        newImage.Height = skinButton.FlyingConfig.Height;
                    } else {
                        newImage.Height = hight;
                    }

                    newImage.Opacity = opacity;
                    ControllerGrid.Children.Add(newImage);
                    flyingImages.AddFirst(new Tuple<Image, Skin.LifeTime>(newImage, new Skin.LifeTime()));
                  
                }
                else
                {
                    bc.buttonUp(button.Item1.Name);
                }

                var removeNumber = 0;

                foreach (var flyingButton in flyingImages)
                {
                    flyingButton.Item1.Margin = new Thickness(flyingButton.Item1.Margin.Left, flyingButton.Item1.Margin.Top + flyingSpeed, 0, 0);

                    flyingButton.Item2.counter = flyingButton.Item2.counter - 1;
                    if (flyingButton.Item2.counter <= 50)
                    {
                        flyingButton.Item1.Opacity = flyingButton.Item1.Opacity - 0.02;//flyingButton.Item2.counter / 50;
                    }
                    if (flyingButton.Item2.counter <= 0)
                    {
                        ControllerGrid.Children.Remove(flyingButton.Item1);
                        removeNumber = removeNumber + 1;
                    }
                }
                for (int i = 0; i < removeNumber; i++) { flyingImages.RemoveLast(); }
            }

            foreach (var button in _rangeButtonsWithImages)  //N64 Button???? (Not usefull for GameCube Controller apparently)
            {
                if (!newState.Analogs.ContainsKey (button.Item1.Name)) continue;

                var value = newState.Analogs [button.Item1.Name];
                var visible = button.Item1.From <= value && value <= button.Item1.To;

                button.Item2.Visibility = visible ? Visibility.Visible : Visibility.Hidden ;
            }

            foreach (var stick in _sticksWithImages) //JoySicks
            {
                var skin = stick.Item1;
                var image = stick.Item2;

                float xrange = (skin.XReverse ? -1 :  1) * skin.XRange;
                float yrange = (skin.YReverse ?  1 : -1) * skin.YRange;

                var x = newState.Analogs.ContainsKey (skin.XName)
                      ? skin.Config.X + xrange * newState.Analogs [skin.XName]
                      : skin.Config.X ;

                var y = newState.Analogs.ContainsKey (skin.YName)
                      ? skin.Config.Y + yrange * newState.Analogs [skin.YName]
                      : skin.Config.Y ;
                
                image.Margin = new Thickness (x,y,0,0);
            }

            foreach (var trigger in _triggersWithGridImages) //Analog => L / R
            {
                var skin = trigger.Item1;
                var grid = trigger.Item2;
                var flyingImages = trigger.Item3;

                if (!newState.Analogs.ContainsKey(skin.Name)) continue;

                var val = newState.Analogs[skin.Name];
                if (skin.UseNegative) val *= -1;
                if (skin.IsReversed) val = 1 - val;
                if (val < 0) val = 0;


                var newImage = getImageForElement(skin.FlyingConfig);
                newImage.Margin = new Thickness(skin.FlyingConfig.X, 0, 0, 0);
                newImage.Height = hight;
                newImage.Opacity = opacity;

                switch (skin.Direction)
                {
                    case Skin.AnalogTrigger.DirectionValue.Right:
                        grid.Width = skin.ButtonConfig.Width * val;
                        newImage.Width = skin.FlyingConfig.Width * val > 3 ? skin.FlyingConfig.Width * val : 0;
                        break;

                    case Skin.AnalogTrigger.DirectionValue.Left:
                        var buttonWidth = skin.ButtonConfig.Width * val;
                        var buttonOffx = skin.ButtonConfig.Width - buttonWidth;
                        grid.Margin = new Thickness(skin.ButtonConfig.X + buttonOffx, skin.ButtonConfig.Y, 0, 0);
                        grid.Width = buttonWidth;

                        var flyingWidth = skin.FlyingConfig.Width * val;
                        var flyingOffx = skin.FlyingConfig.Width - flyingWidth;
                        newImage.Margin = new Thickness(skin.FlyingConfig.X + flyingOffx, 0, 0, 0);
                        newImage.Width = flyingWidth > 3 ? flyingWidth : 0;
                        break;

                    case Skin.AnalogTrigger.DirectionValue.Down:
                        grid.Height = skin.ButtonConfig.Height * val;
                        break;

                    case Skin.AnalogTrigger.DirectionValue.Up:
                        var height = skin.ButtonConfig.Height * val;
                        var offy = skin.ButtonConfig.Height - height;
                        grid.Margin = new Thickness(skin.ButtonConfig.X, skin.ButtonConfig.Y + offy, 0, 0);
                        grid.Height = height;
                        break;
                }


                ControllerGrid.Children.Add(newImage);
                flyingImages.AddFirst(new Tuple<Image, Skin.LifeTime>(newImage, new Skin.LifeTime()));

                var removeNumber = 0;
                foreach (var flyingButton in flyingImages)
                {
                    flyingButton.Item1.Margin = new Thickness(flyingButton.Item1.Margin.Left, flyingButton.Item1.Margin.Top + flyingSpeed, 0, 0);

                    flyingButton.Item2.counter = flyingButton.Item2.counter - 1;
                    if (flyingButton.Item1.Width > 45 && flyingButton.Item2.counter <= 50)
                    {
                        flyingButton.Item1.Opacity = 0;
                    }
                    else if (flyingButton.Item2.counter <= 50)
                    {
                        flyingButton.Item1.Opacity = flyingButton.Item1.Opacity - 0.02;//flyingButton.Item2.counter / 50;
                    }
                    if (flyingButton.Item2.counter <= 0)
                    {
                        ControllerGrid.Children.Remove(flyingButton.Item1);
                        removeNumber = removeNumber + 1;                    
                    }
                }
                for (int i = 0; i < removeNumber; i++) { flyingImages.RemoveLast(); }
                
            }
        }

        // INotifyPropertyChanged interface implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged (string propertyName) {
            if (PropertyChanged != null) PropertyChanged (this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            bc.Show();
            bc.Start();
        }
    }
}
