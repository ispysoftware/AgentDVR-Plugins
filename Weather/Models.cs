using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins
{
    // Models for OpenWeatherMap OneCall API
    public class WeatherResponse
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public Current current { get; set; }
        public string message { get; set; } // For error messages
    }

    public class Current
    {
        public long dt { get; set; }
        public long sunrise { get; set; }
        public long sunset { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public double? wind_gust { get; set; } // Optional
        public List<Weather> weather { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    // Models for OpenWeatherMap Standard API
    public class StandardWeatherResponse
    {
        public List<Weather> weather { get; set; }
        public MainInfo main { get; set; }
        public WindInfo wind { get; set; }
        public CloudsInfo clouds { get; set; }
        public long dt { get; set; }
        public SysInfo sys { get; set; }
        public int timezone { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
        public string message { get; set; } // For error messages
    }

    public class MainInfo
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class WindInfo
    {
        public double speed { get; set; }
        public int deg { get; set; }
        public double? gust { get; set; } // Optional
    }

    public class CloudsInfo
    {
        public int all { get; set; }
    }

    public class SysInfo
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public long sunrise { get; set; }
        public long sunset { get; set; }
    }
}
