﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.Xml;

namespace NintendoSpy
{
    public class Skin
    {
        public class ElementConfig {
            public BitmapImage Image;
            public uint X, Y, Width, Height;
        }

        public class LifeTime {
            public uint counter = 400; 
        }

        public class Background {
            public string Name { get; set; }
            public BitmapImage Image { get; set; }
        }

        public class Button {
            public ElementConfig ButtonConfig;
            public string Name;
            public ElementConfig FlyingConfig;
            public bool WasPressed = false;
        }

        public class RangeButton {
            public ElementConfig Config;
            public string Name;
            public float From, To;
        }

        public class AnalogStick {
            public ElementConfig Config;
            public string XName, YName;
            public uint XRange, YRange;
            public bool XReverse, YReverse;
        }

        public class AnalogTrigger {
            public enum DirectionValue { Up, Down, Left, Right }
            public ElementConfig ButtonConfig;
            public string Name;
            public DirectionValue Direction;
            public bool IsReversed;
            public bool UseNegative;
            public ElementConfig FlyingConfig;
        }


        public string Name { get; private set; }
        public string Author { get; private set; }
        public InputSource Type { get; private set; }


        List <Background> _backgrounds = new List <Background> ();
        public IReadOnlyList <Background> Backgrounds { get { return _backgrounds; } }

        List <Button> _buttons = new List <Button> ();
        public IReadOnlyList <Button> Buttons { get { return _buttons; } }

        List <RangeButton> _rangeButtons = new List <RangeButton> ();
        public IReadOnlyList <RangeButton> RangeButtons { get { return _rangeButtons; } }

        List <AnalogStick> _analogSticks = new List <AnalogStick> ();
        public IReadOnlyList <AnalogStick> AnalogSticks { get { return _analogSticks; } }

        List <AnalogTrigger> _analogTriggers = new List <AnalogTrigger> ();
        public IReadOnlyList <AnalogTrigger> AnalogTriggers { get { return _analogTriggers; } }


    // ----------------------------------------------------------------------------------------------------------------

        Skin (string folder)
        {
            var skinPath = Path.Combine (Environment.CurrentDirectory, folder);

            if (! File.Exists (Path.Combine (skinPath, "skin.xml"))) {
                throw new ConfigParseException ("Could not find skin.xml for skin at '"+folder+"'.");
            }
            var doc = XDocument.Load (Path.Combine (skinPath, "skin.xml"));

            Name = readStringAttr (doc.Root, "name");
            Author = readStringAttr (doc.Root, "author");
            Type = InputSource.ALL.First (x => x.TypeTag == readStringAttr (doc.Root, ("type")));

            if (Type == null) {
                throw new ConfigParseException ("Illegal value specified for skin attribute 'type'.");
            }

            var bgElems = doc.Root.Elements ("background");

            if (bgElems.Count() < 1) {
                throw new ConfigParseException ("Skin must contain at least one background.");
            }

            foreach (var elem in bgElems) {
                _backgrounds.Add (new Background {
                    Name = readStringAttr (elem, "name"),
                    Image = loadImage (skinPath, readStringAttr (elem, "image"))
                });
            }

            foreach (var elem in doc.Root.Elements ("button")) {
                _buttons.Add(new Button
                {
                    ButtonConfig = parseStandardConfig(skinPath, elem),
                    Name = readStringAttr(elem, "name"),
                    FlyingConfig = parseFlyingConfig(skinPath, elem),
                });
            }

            foreach (var elem in doc.Root.Elements ("rangebutton"))
            {
                var from = readFloatConfig (elem, "from");
                var to = readFloatConfig (elem, "to");

                if (from > to) throw new ConfigParseException ("Rangebutton 'from' field cannot be greater than 'to' field.");

                _rangeButtons.Add (new RangeButton {
                    Config = parseStandardConfig (skinPath, elem),
                    Name = readStringAttr (elem, "name"),
                    From = from,
                    To = to
                });
            }

            foreach (var elem in doc.Root.Elements ("stick")) {
                _analogSticks.Add (new AnalogStick {
                    Config = parseStandardConfig (skinPath, elem),
                    XName = readStringAttr (elem, "xname"),
                    YName = readStringAttr (elem, "yname"),
                    XRange = readUintAttr (elem, "xrange"),
                    YRange = readUintAttr (elem, "yrange"),
                    XReverse = readBoolAttr (elem, "xreverse"),
                    YReverse = readBoolAttr (elem, "yreverse")
                });
            }

            foreach (var elem in doc.Root.Elements ("analog"))
            {
                var directionAttrs = elem.Attributes ("direction");
                if (directionAttrs.Count() < 1) throw new ConfigParseException ("Element 'analog' needs attribute 'direction'.");

                AnalogTrigger.DirectionValue dir;

                switch (directionAttrs.First().Value) {
                    case "up": dir = AnalogTrigger.DirectionValue.Up; break;
                    case "down": dir = AnalogTrigger.DirectionValue.Down; break;
                    case "left": dir = AnalogTrigger.DirectionValue.Left; break;
                    case "right": dir = AnalogTrigger.DirectionValue.Right; break;
                    default: throw new ConfigParseException ("Element 'analog' attribute 'direction' has illegal value. Valid values are 'up', 'down', 'left', 'right'.");
                }

                _analogTriggers.Add (new AnalogTrigger {
                    ButtonConfig = parseStandardConfig (skinPath, elem),
                    Name = readStringAttr (elem, "name"),
                    Direction = dir,
                    IsReversed = readBoolAttr (elem, "reverse"),
                    UseNegative = readBoolAttr (elem, "usenegative"),
                    FlyingConfig = parseFlyingConfig(skinPath, elem),
                });
            }
        }

        static string readStringAttr (XElement elem, string attrName)
        {
            var attrs = elem.Attributes (attrName);
            if (attrs.Count() == 0) throw new ConfigParseException ("Required attribute '"+attrName+"' not found on element '"+elem.Name+"'.");
            return attrs.First().Value;
        }

        static float readFloatConfig (XElement elem, string attrName)
        {
            float ret;
            if (! float.TryParse (readStringAttr (elem, attrName), out ret)) {
                throw new ConfigParseException ("Failed to parse number for property '"+attrName+"' in element '"+elem.Name+"'.");
            }
            return ret;
        }

        static uint readUintAttr (XElement elem, string attrName)
        {
            uint ret;
            if (! uint.TryParse (readStringAttr (elem, attrName), out ret)) {
                throw new ConfigParseException ("Failed to parse number for property '"+attrName+"' in element '"+elem.Name+"'.");
            }
            return ret;
        }

        static BitmapImage loadImage (string skinPath, string fileName)
        {
            try {
                return new BitmapImage (new Uri (Path.Combine (skinPath, fileName)));
            } catch (Exception e) {
                throw new ConfigParseException ("Could not load image '"+fileName+"'.", e);
            }
        }

        static ElementConfig parseStandardConfig (string skinPath, XElement elem)
        {
            var imageAttr = elem.Attributes ("image");
            if (imageAttr.Count() == 0) throw new ConfigParseException ("Attribute 'image' missing for element '"+elem.Name+"'.");

            var image = loadImage (skinPath, imageAttr.First().Value);

            uint width = (uint)image.PixelWidth;
            var widthAttr = elem.Attributes("width");
            if (widthAttr.Count() > 0) width = uint.Parse (widthAttr.First().Value);

            uint height = (uint)image.PixelHeight;
            var heightAttr = elem.Attributes("height");
            if (heightAttr.Count() > 0) height = uint.Parse (heightAttr.First().Value);

            uint x = readUintAttr (elem, "x");
            uint y = readUintAttr (elem, "y");

            return new ElementConfig {
                X = x, Y = y, Image = image, Width = width, Height = height
            };
        }

        static ElementConfig parseFlyingConfig(string skinPath, XElement elem)
        {
            var imageAttr = elem.Attributes("flyingImage");
            if (imageAttr.Count() == 0) throw new ConfigParseException("Attribute 'flyingImage' missing for element '" + elem.Name + "'.");

            var image = loadImage(skinPath, readStringAttr(elem, "flyingImage"));

            uint width = (uint)image.PixelWidth;
            var widthAttr = elem.Attributes("flyingWidth");
            if (widthAttr.Count() > 0) width = uint.Parse(widthAttr.First().Value);

            uint height = (uint)image.PixelHeight;
            var heightAttr = elem.Attributes("flyingHeight");
            if (heightAttr.Count() > 0) height = uint.Parse(heightAttr.First().Value);

            uint x = readUintAttr(elem, "flyingX");
            uint y = readUintAttr(elem, "flyingY");

            return new ElementConfig
            {
                X = x,
                Y = y,
                Image = image,
                Width = width,
                Height = height
            };
        }

        static bool readBoolAttr (XElement elem, string attrName, bool dfault = false) {
            var attrs = elem.Attributes (attrName);
            if (attrs.Count() == 0) return dfault;
            if (attrs.First().Value == "true") return true;
            if (attrs.First().Value == "false") return false;
            return dfault;
        }

    // ----------------------------------------------------------------------------------------------------------------

        public class LoadResults {
            public List <Skin> SkinsLoaded;
            public List <string> ParseErrors;
        }

        static public LoadResults LoadAllSkinsFromParentFolder (string path)
        {
            var skins = new List <Skin> ();
            var errs = new List <string> ();

            foreach (var skinDir in Directory.GetDirectories(path)) {
                try {
                    var skin = new Skin (skinDir);
                    skins.Add (skin);
                }
                catch (ConfigParseException e) {
                    errs.Add (skinDir + " :: " + e.Message);
                }
            }

            return new LoadResults {
                SkinsLoaded = skins,
                ParseErrors = errs
            };
        }
    }
}
