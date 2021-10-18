using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PluginUtils
{
    public static class Utils
    {
        public static Exception LastException { get; set; }

        public static bool TaskRunning(Task t)
        {
            if (t == null)
                return false;
            try
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                        return false;

                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        public static dynamic PopulateResponse(string resp, object o)
        {
            dynamic d = JsonConvert.DeserializeObject(resp);
            foreach (var sec in d.sections)
            {
                if (sec.items != null)
                {
                    foreach (var item in sec.items)
                    {
                        var bt = item["bindto"];
                        if (bt != null && o != null)
                        {
                            string[] prop = bt.ToString().Split(',');
                            if (prop.Length == 1)
                            {
                                try
                                {
                                    if (item["type"] == "MultiSelect")
                                        item["value"] = JToken.Parse(GetPropValue(o, bt.ToString()).ToString());
                                    else
                                    {
                                        item["value"] = GetPropValue(o, bt.ToString());
                                    }
                                    var nv = item["nvident"];
                                    if (nv != null)
                                    {
                                        item["value"] = NV(item["value"].ToString(), nv.ToString());
                                        if (item["value"] != "")
                                        {
                                            if (item["type"] == "Boolean")
                                                item["value"] = Convert.ToBoolean(item["value"]);
                                            if (item["type"] == "Int32")
                                                item["value"] = Convert.ToInt32(item["value"]);
                                            if (item["type"] == "Decimal" || item["type"] == "Single")
                                                item["value"] = Convert.ToDecimal(item["value"]);
                                            if (item["type"] == "Select")
                                                item["value"] = Convert.ToString(item["value"]);
                                        }
                                    }
                                    var conv = item["converter"];
                                    if (conv != null)
                                    {
                                        switch ((string)conv)
                                        {
                                            case "daysofweek":
                                                string[] days = item["value"].ToString().Trim(',').Split(',');
                                                int i = 0;
                                                foreach (var opt in item.options)
                                                {
                                                    if (days.Contains(i.ToString(CultureInfo.InvariantCulture)))
                                                    {
                                                        opt["value"] = true;
                                                    }
                                                    i++;
                                                }
                                                break;
                                            case "datetimetoint":
                                                var dt = (DateTime)item["value"];
                                                item["value"] = dt.TimeOfDay.TotalMinutes;
                                                break;
                                            case "rgbtohex":
                                                var rgb = (string)item["value"].ToString();
                                                var rgbarr = rgb.Split(',');
                                                if (rgbarr.Length == 3)
                                                {
                                                    item["value"] = "#" + (Convert.ToInt16(rgbarr[0])).ToString("X2") +
                                                                    (Convert.ToInt16(rgbarr[1])).ToString("X2") +
                                                                    (Convert.ToInt16(rgbarr[2])).ToString("X2");
                                                }

                                                break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LastException = ex;
                                    item["value"] = "";
                                }
                            }
                            else
                            {
                                string json = prop.Aggregate("[", (current, s) => current + (GetPropValue(o, s) + ","));
                                json = json.Trim(',');
                                json += "]";
                                item["value"] = JToken.Parse(json);
                            }
                        }
                    }
                }
            }
            return d;
        }

        private static string NV(string source, string name)
        {
            if (string.IsNullOrEmpty(source))
                return "";
            name = name.ToLower().Trim();
            string[] settings = source.Split(',');
            foreach (string[] nv in settings.Select(s => s.Split('=')).Where(nv => nv[0].ToLower().Trim() == name))
            {
                return nv[1];
            }
            return "";
        }

        private static object GetPropValue(object src, string propName)
        {
            object currentObject = src;
            string[] fieldNames = propName.Split('.');

            foreach (string fieldName in fieldNames)
            {
                // Get type of current record 
                Type curentRecordType = currentObject.GetType();
                PropertyInfo property = curentRecordType.GetProperty(fieldName);

                if (property != null)
                {
                    currentObject = property.GetValue(currentObject, null);
                }
                else
                {
                    return null;
                }
            }
            return currentObject;
        }

        public static void PopulateObject(dynamic d, object o)
        {
            foreach (var sec in d.sections)
            {
                foreach (var item in sec.items)
                {
                    var bt = item["bindto"];
                    if (bt != null)
                    {
                        var val = item["value"];
                        if (val != null)
                        {
                            Populate(item, o);
                        }
                    }
                }
            }
        }

        static void Populate(dynamic item, object o)
        {
            var bt = item["bindto"];
            var val = item["value"];
            var conv = item["converter"];
            var nvident = item["nvident"];

            if (conv != null)
            {
                switch ((string)conv)
                {
                    case "daysofweek":
                        string dow = "";
                        int i = 0;
                        foreach (var opt in item.options)
                        {
                            if (opt.value == true)
                            {
                                dow += i.ToString(CultureInfo.InvariantCulture) + ",";
                            }
                            i++;
                        }
                        dow = dow.Trim(',');
                        val = dow;
                        break;
                    case "datetimetoint":
                        TimeSpan ts = TimeSpan.FromMinutes(Convert.ToInt64(val));
                        val = DateTime.MinValue.Add(ts);
                        break;
                    case "rgbtohex":
                        try
                        {
                            //convert back to rgb

                            var hex = (string)item["value"].ToString();
                            if (hex.StartsWith("#"))
                                hex = hex.Substring(1);

                            if (hex.Length != 6) throw new Exception("Color not valid");

                            val = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber)+","+
                                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) + "," +
                                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

                        }
                        catch (Exception ex)
                        {
                            LastException = ex;
                        }
                        break;
                }
            }

            var props = bt.ToString().Split(',');
            if (props.Length > 1)
            {
                int i = 0;
                if (val.Type.ToString() == "String")
                {
                    val = val.ToString().Split(',');
                }
                foreach (string s in props)
                {
                    try
                    {
                        SetPropValue(o, s, val[i]);
                    }
                    catch (Exception ex)
                    {
                        LastException = ex;
                    }
                    i++;
                }
            }
            else
            {
                if (nvident != null)
                {
                    var nv = nvident.ToString();
                    var nvstring = GetPropValue(o, props[0]).ToString();
                    val = NVSet(nvstring, nv, val.ToString());
                }
                try
                {
                    SetPropValue(o, props[0], val);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                }
            }
        }
        private struct Points
        {
            public float x, y, x2, y2;
        }

        private struct Areas
        {
            public float x, y, w, h;
        }
        public static List<Line2D> ParseTripWires(Size frameSize, string json)
        {
            var tripWireList = new List<Line2D>();
            var lp = JsonConvert.DeserializeObject<List<Points>>(json);
            foreach (var p in lp)
            {
                tripWireList.Add(new Line2D(frameSize, new Point(Convert.ToInt32(p.x), Convert.ToInt32(p.y)), new Point(Convert.ToInt32(p.x2), Convert.ToInt32(p.y2))));
            }
            return tripWireList;
        }

        public static List<Rectangle> ParseAreas(Size frameSize, string json)
        {
            var areaList = new List<Rectangle>();
            var lp = JsonConvert.DeserializeObject<List<Areas>>(json);
            foreach (var p in lp)
            {

                var start = ScalePercentToFrame(new Point(Convert.ToInt32(p.x), Convert.ToInt32(p.y)), frameSize);
                var sz = ScalePercentToFrame(new Point(Convert.ToInt32(p.w), Convert.ToInt32(p.h)), frameSize);
                areaList.Add(new Rectangle(start, new Size(sz.X, sz.Y)));
            }
            return areaList;
        }

        internal static Point ScalePercentToFrame(Point p, Size sz)
        {
            var x = (p.X / 100d) * sz.Width;
            var y = (p.Y / 100d) * sz.Height;

            return new Point(Convert.ToInt16(x), Convert.ToInt16(y));
        }

        public static Rectangle[] ImageZones(string zoneMap, Size imageSize)
        {
            if (zoneMap.Length > 0)
            {
                double wmulti = Convert.ToDouble(imageSize.Width) / Convert.ToDouble(100);
                double hmulti = Convert.ToDouble(imageSize.Height) / Convert.ToDouble(100);

                var l = new List<Rectangle>();
                int x = 0, y = 0;
                var p = 5d;
                int ylim = 48;

                double pcx = (p / 320d) * 100d;
                double pcy = (p / 240d) * 100d;
                int rx = Convert.ToInt32(pcx * wmulti);
                int ry = Convert.ToInt32(pcy * hmulti);
                foreach (var c in zoneMap)
                {
                    if (c != '0')
                    {
                        l.Add(new Rectangle(Convert.ToInt32(x * pcx * wmulti), Convert.ToInt32(y * pcy * hmulti), rx, ry));
                    }
                    y++;
                    if (y == ylim)
                    {
                        x++;
                        y = 0;
                    }
                }
                return l.ToArray();
            }

            return new[] { new Rectangle(0, 0, imageSize.Width, imageSize.Height) };
        }

        public static bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        //given a zone map, image size and point return zone at the location
        public static char GetZone(Point p, Size imageSize, string zoneMap)
        {
            if (imageSize.Width > 0 && imageSize.Height > 0)
            {
                var x = Convert.ToInt32(Math.Floor((Convert.ToDouble(p.X) / imageSize.Width) * 64d));
                var y = Convert.ToInt32(Math.Floor((Convert.ToDouble(p.Y) / imageSize.Height) * 48d));
                int ind = Convert.ToInt32(x * 48d + y);
                //convert p to index in zonemap
                if (ind <= zoneMap.Length)
                {
                    return zoneMap[ind];
                }

            }
            return '0';

        }

        static void SetPropValue(object src, string propName, object propValue)
        {
            object currentObject = src;
            string[] fieldNames = propName.Split('.');

            for (int i = 0; i < fieldNames.Length - 1; i++)
            {
                string fieldName = fieldNames[i];
                currentObject = currentObject.GetType().GetProperty(fieldName).GetValue(currentObject, null);
            }
            var val = currentObject.GetType().GetProperty(fieldNames[fieldNames.Length - 1]);
            if (val == null) return; //support example json with no bindings

            var t = val.PropertyType.Name;
            switch (t)
            {
                case "String":
                    val.SetValue(currentObject, propValue.ToString(), null);
                    break;
                case "Int32":
                    val.SetValue(currentObject, Convert.ToInt32(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Decimal":
                    val.SetValue(currentObject, Convert.ToDecimal(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Single":
                    val.SetValue(currentObject, Convert.ToSingle(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Double":
                    val.SetValue(currentObject, Convert.ToDouble(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Boolean":
                    val.SetValue(currentObject, Convert.ToBoolean(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "DateTime":
                    val.SetValue(currentObject, Convert.ToDateTime(propValue, CultureInfo.InvariantCulture), null);
                    break;
                default:
                    throw new Exception("missing conversion (" + t + ")");
            }

        }

        static string NVSet(string source, string name, string value)
        {
            if (source == null) source = "";

            name = name.ToLower().Trim();

            string[] settings = source.Split(',');
            bool isset = false;
            for (int i = 0; i < settings.Length; i++)
            {
                if (settings[i].ToLower().StartsWith(name + "="))
                {
                    settings[i] = name + "=" + value;
                    isset = true;
                    break;
                }
            }
            if (!isset)
            {
                var l = settings.ToList();
                l.Add(name + "=" + value);
                settings = l.ToArray();
            }
            return string.Join(",", settings);
        }


    }
}
