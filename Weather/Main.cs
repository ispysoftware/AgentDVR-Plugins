using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using PluginUtils;
using SixLabors.Fonts;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace Plugins
{
    public class Main : PluginBase, ICamera
    {
        private Font _messageFont;
        private bool _needUpdate = true;

        private string[] _weather = new[] {"Unavailable"};
        private DateTime _lastWeatherUpdate = DateTime.MinValue;
        private Color _foreGround = Color.White, _backGround = Color.Black;
        private bool _gustLimit, _tempLimit, _statusLimit;
        private string _error = "";


        public Main():base()
        {
            
            //get cross platform font family
            string[] fontfams = new[] { "Verdana", "Arial", "Helvetica", "Geneva", "FreeMono", "DejaVu Sans"};


            var ff = false;
            FontFamily fam;
            foreach (var fontfam in fontfams)
            {
                if (SystemFonts.Collection.TryGet(fontfam, out fam))
                {
                    ff = true;
                    break;
                }
            }
            if (!ff)
                fam = SystemFonts.Collection.Families.First();

            _messageFont = SystemFonts.CreateFont(fam.Name, 20, FontStyle.Regular);
        }

        public string Supports
        {
            get
            {
                return "video";
            }
        }

        public override List<string> GetCustomEvents()
        {
            return new List<string>() { "High Temp", "Gust", "Status" };
        }

        public override void SetConfiguration(string json)
        {
            base.SetConfiguration(json);
            _needUpdate = true;
            
        }

        public override void ProcessAgentEvent(string ev)
        {
        }

        public enum OverlayLocation
        {
            None,
            TopLeft,
            TopMiddle,
            TopRight,
            BottomLeft,
            BottomMiddle,
            BottomRight,
            Left,
            Middle,
            Right
        }

        internal static Point GetOverlayLocation(System.Drawing.Size image, System.Drawing.Size sz, OverlayLocation loc)
        {
            var p = new Point(0, 0);
            switch (loc)
            {
                case OverlayLocation.TopMiddle:
                    p.X = image.Width / 2 - sz.Width / 2;
                    break;
                case OverlayLocation.TopRight:
                    p.X = image.Width - sz.Width;
                    break;
                case OverlayLocation.BottomLeft:
                    p.Y = image.Height - sz.Height;
                    break;
                case OverlayLocation.BottomMiddle:
                    p.Y = image.Height - sz.Height;
                    p.X = image.Width / 2 - sz.Width / 2;
                    break;
                case OverlayLocation.BottomRight:
                    p.Y = image.Height - sz.Height;
                    p.X = image.Width - sz.Width;
                    break;
                case OverlayLocation.Left:
                    p.Y = image.Height / 2 - sz.Height / 2;
                    p.X = 0;
                    break;
                case OverlayLocation.Middle:
                    p.Y = image.Height / 2 - sz.Height / 2;
                    p.X = image.Width / 2 - sz.Width / 2;
                    break;
                case OverlayLocation.Right:
                    p.Y = image.Height / 2 - sz.Height / 2;
                    p.X = image.Width - sz.Width;
                    break;
            }
            return p;
        }

        public string[] Weather
        {
            get
            {
                if (_lastWeatherUpdate < DateTime.UtcNow.AddSeconds(0-ConfigObject.UpdateFrequency))
                {
                    _lastWeatherUpdate = DateTime.UtcNow;
                    _ = UpdateWeather();
                }
                var w = _weather.ToList();
                if (!string.IsNullOrEmpty(_error))
                    w.Add("Error: "+_error);
                return w.ToArray();
            }
        }

        private Image _currentIcon = null;

        private Image Icon
        {
            get
            {
                lock(this)
                {
                    return _currentIcon;
                }
            }
            set
            {
                lock(this)
                {
                    _currentIcon = value;
                }
            }
        }

        private string SpeedUnit
        {
            get
            {
                switch (ConfigObject.Units)
                {
                    case "imperial":
                        return " mph";
                    default:
                        return " m/s";
                }
            }
        }

        private string TempUnit
        {
            get
            {
                switch (ConfigObject.Units)
                {
                    case "imperial":
                        return " °F";
                    case "standard":
                        return " °K";
                    default:
                        return " °C";
                }
            }
        }
        private bool HasKey(dynamic d, string key)
        {
            try
            {
                return d[key] != null;
            }
            catch
            {
                return false;
            }
        }
        static readonly HttpClient client = new HttpClient();

        private async Task UpdateWeather()
        {
            string url = "";
            if (!string.IsNullOrEmpty(ConfigObject.URL))
            {
                url = ConfigObject.URL;
            }
            else
            {
                if (!string.IsNullOrEmpty(ConfigObject.APIKey))
                {
                    var latlng = ConfigObject.LatLng.Split(',');
                    if (latlng.Length > 1)
                    {
                        //3.0 not working
                        string apiver = ConfigObject.APIversion;
                        url = $"https://api.openweathermap.org/data/{apiver}/onecall?lat={latlng[0]}&lon={latlng[1]}&exclude=minutely,hourly,daily&appid={ConfigObject.APIKey}&units={ConfigObject.Units}";
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var c = await client.GetStringAsync(url);
                    dynamic data = JsonConvert.DeserializeObject(c);

                    if (HasKey(data, "message"))
                    {
                        //error
                        _weather = new string[] { "error: " + data["message"].ToString() };
                        Icon = null;
                    }
                    else
                    {
                        string icn = "";
                        string main = "unknown";
                        string description = "unknown";
                        string gust = "";
                        string wind = "unknown";
                        string temp ="", feelsLike="", humidity="", uvi="";
                        int wind_deg = 0;

                        if (HasKey(data, "current"))
                        {
                            icn = data.current.weather[0].icon.ToString();
                            main = data.current.weather[0].main.ToString();
                            description = data.current.weather[0].description.ToString();

                            wind = data.current.wind_speed.ToString() + SpeedUnit;
                            wind_deg = Convert.ToInt32(data.current.wind_deg.ToString());
                            double dGust = -1;
                            if (HasKey(data, "current.wind_gust")) //not always available
                            {
                                gust = data.current.wind_gust.ToString() + SpeedUnit;
                                double.TryParse(data.current.wind_gust.ToString(), out dGust);
                            }

                            temp = data.current.temp.ToString() + TempUnit;


                            double.TryParse(data.current.temp.ToString(), out double dTemp);

                            if (dGust > ConfigObject.GustLimit)
                            {
                                if (!_gustLimit)
                                    Results.Add(new ResultInfo("Gust", wind));
                                _gustLimit = true;
                            }
                            else
                                _gustLimit = false;


                            if (dTemp > ConfigObject.TempLimit)
                            {
                                if (!_tempLimit)
                                    Results.Add(new ResultInfo("High Temp", temp));
                                _tempLimit = true;
                            }
                            else
                            {
                                _tempLimit = true;
                            }

                            if (ConfigObject.StatusEvent.ToLowerInvariant() == main.ToLowerInvariant())
                            {
                                if (!_statusLimit)
                                    Results.Add(new ResultInfo("Status", main));
                                _statusLimit = true;
                            }
                            else
                                _statusLimit = false;


                            feelsLike = data.current.feels_like.ToString() + TempUnit;
                            humidity = data.current.humidity.ToString() + "%";
                            uvi = "";
                            if (HasKey(data.current, "uvi")) //not always available
                                uvi = data.current.uvi.ToString();                           
                        }
                        else
                        {
                            icn = data.weather[0].icon.ToString();
                            main = data.weather[0].main.ToString();
                            description = data.weather[0].description.ToString();
                            wind = data.wind.speed.ToString() + SpeedUnit;                            
                            temp = data.main.temp.ToString() + TempUnit;

                            double dGust = -1;
                            if (HasKey(data, "wind.gust")) //not always available
                            {
                                gust = data.wind.gust.ToString() + SpeedUnit;
                                double.TryParse(data.wind.gust.ToString(), out dGust);
                            }

                            if (dGust > ConfigObject.GustLimit)
                            {
                                if (!_gustLimit)
                                    Results.Add(new ResultInfo("Gust", wind));
                                _gustLimit = true;
                            }
                            else
                                _gustLimit = false;

                            wind_deg =Convert.ToInt32(data.wind.deg.ToString());
                            double.TryParse(data.main.temp.ToString(), out double dTemp);

                            if (dTemp > ConfigObject.TempLimit)
                            {
                                if (!_tempLimit)
                                    Results.Add(new ResultInfo("High Temp", temp));
                                _tempLimit = true;
                            }
                            else
                            {
                                _tempLimit = true;
                            }

                            if (ConfigObject.StatusEvent.ToLowerInvariant() == main.ToLowerInvariant())
                            {
                                if (!_statusLimit)
                                    Results.Add(new ResultInfo("Status", main));
                                _statusLimit = true;
                            }
                            else
                                _statusLimit = false;


                            feelsLike = data.main.feels_like.ToString() + TempUnit;
                            humidity = data.main.humidity.ToString() + "%";
                            uvi = "";
                        }

                        string format = ConfigObject.Format.Replace("\r\n", "\n");
                        if (format.Contains("{icon}"))
                            Icon = Image.Load(ResourceLoader.GetResourceBytes(icn + ".png"));
                        else
                            Icon = null;

                        format = format.Replace("{icon}", "");
                        format = format.Replace("{main}", main);
                        format = format.Replace("{description}", description);
                        format = format.Replace("{wind}", wind);
                        format = format.Replace("{windDeg}", wind_deg.ToString());
                        format = format.Replace("{windDir}", GetDirectionString(wind_deg));
                        format = format.Replace("{gust}", gust);
                        format = format.Replace("{temp}", temp);
                        format = format.Replace("{feelsLike}", feelsLike);
                        format = format.Replace("{humidity}", humidity);
                        format = format.Replace("{uvi}", uvi);


                        _weather = format.Split('\n');

                        _error = "";
                    }
                }
                catch (Exception ex)
                {
                    //service down?
                    _error = ex.Message;
                    //_weather = new[] { ex.Message };
                    //Icon = null;
                }
            }
                
        }

        public static string GetDirectionString(double degrees)
        {
            string[] directions = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

            // Convert degrees to a value between 0 and 360
            degrees %= 360;

            // Determine the index of the direction array to use
            int index = (int)((degrees / 22.5) + 0.5);

            // Get the direction string for the index
            return directions[index % 16];
        }

        public Size MeasureText(string[] txtArr, int xPadding, int yPadding, out int[] lineWidths, out int[] lineHeights)
        {
            lineWidths = new int[txtArr.Length];
            lineHeights = new int[txtArr.Length];
            int w = 0, h = 0;

            for (int i = 0; i < txtArr.Length; i++)
            {
                FontRectangle size = TextMeasurer.MeasureAdvance(txtArr[i], new TextOptions(_messageFont));
                lineWidths[i] = (int)size.Width + xPadding;
                lineHeights[i] = (int)size.Height + yPadding;
                w = Math.Max(lineWidths[i], w);
                h += lineHeights[i];
            }
            return new Size(w, h);
        }

        public void ProcessVideoFrame(IntPtr frame, System.Drawing.Size imageSize, int channels, int stride)
        {
            if (_needUpdate)
            {
                _messageFont = SystemFonts.CreateFont(_messageFont.Name, ConfigObject.FontSize, FontStyle.Regular);
                if (!string.IsNullOrEmpty(ConfigObject.Background))
                {
                    if (!Color.TryParse(ConfigObject.Background, out _backGround))
                        _backGround = Color.Black;
                }
                else
                    _backGround = Color.Black;

                if (!string.IsNullOrEmpty(ConfigObject.Foreground))
                {
                    if (!Color.TryParse(ConfigObject.Foreground, out _foreGround))
                        _foreGround = Color.White;
                }
                else
                    _foreGround = Color.White;

                _needUpdate = false;

                _gustLimit = _tempLimit = _statusLimit = false;
                _lastWeatherUpdate = DateTime.MinValue;//update weather information
            }
            unsafe
            {
                using (var image = Image.WrapMemory<Bgr24>(frame.ToPointer(), stride * imageSize.Height, imageSize.Width, imageSize.Height))
                {
                    string[] weather = Weather;
                    var icn = Icon;
                    var size = MeasureText(Weather, 3, 3, out int[] lw, out int[] lh);

                    System.Drawing.Size sz = new System.Drawing.Size((int)size.Width + (icn?.Width??0), Math.Max((int)size.Height+3, icn?.Height??0));
                    var pos = GetOverlayLocation(imageSize, sz, (OverlayLocation)ConfigObject.Position);
                    var box = new Rectangle(pos.X,pos.Y, sz.Width, sz.Height);
                    if (ConfigObject.DisplayBackground)
                    {                    
                        image.Mutate(x => x.Fill(_backGround, box));
                    }
                    var y = box.Y+3;
                    for (var i = 0; i < lw.Length; i++)
                    {
                        image.Mutate(x => x.DrawText(weather[i], _messageFont, _foreGround, new PointF(box.X + (icn?.Width ?? 3), y)));
                        y += lh[i];
                    }
                    if (icn!= null)
                    {
                        image.Mutate(x=>x.DrawImage(icn,new Point(box.X, box.Y), 1.0f));
                    }
                }
            }
        }

        ~Main()
        {
            Dispose(false);
        }
    }
}
