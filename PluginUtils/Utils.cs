using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluginUtils
{
    public static class Utils
    {
        public static Exception LastException { get; set; }
        static JsonSerializerOptions JSONOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false, // Makes property name matching case-insensitive
                                                 // You can add more options here if necessary
        };

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

        public static JsonNode? PopulateResponse(string resp, object o)
        {
            // Parse the JSON as a JsonNode
            JsonNode? d = JsonNode.Parse(resp);
            if (d == null)
                return null;

            // Get the "sections" array
            JsonArray? sections = d["sections"] as JsonArray;
            if (sections == null)
                return d;

            // Iterate each section
            foreach (JsonNode? sec in sections)
            {
                if (sec == null) continue;

                // Get "items" array inside the section
                JsonArray? items = sec["items"] as JsonArray;
                if (items == null) continue;

                // Iterate each item
                foreach (JsonNode? itemNode in items)
                {
                    if (itemNode is not JsonObject item) continue;

                    // Get "bindto"
                    string? bindTo = item["bindto"]?.GetValue<string>();
                    if (bindTo == null || o == null)
                        continue;

                    // Split bindTo to handle single or multiple properties
                    string[] prop = bindTo.Split(',');

                    try
                    {
                        if (prop.Length == 1)
                        {
                            // Single property
                            var propValue = GetPropValue(o, bindTo);

                            // If type == "MultiSelect", parse propValue as JSON array (e.g. "[...]")
                            string? itemType = item["type"]?.GetValue<string>();
                            if (itemType == "MultiSelect")
                            {
                                // propValue is presumably JSON array string
                                // If it's not already JSON, you might need to build a JSON array string
                                // For example: "[1,2,3]"
                                item["value"] = propValue != null
                                    ? JsonNode.Parse(propValue.ToString() ?? "[]")
                                    : JsonNode.Parse("[]");
                            }
                            else
                            {
                                // Otherwise, just store the raw value
                                item["value"] = JsonValue.Create(propValue);
                            }

                            // nvident transformation
                            string? nv = item["nvident"]?.GetValue<string>();
                            if (!string.IsNullOrEmpty(nv))
                            {
                                string currentValueStr = item["value"]?.GetValue<string>() ?? "";
                                string transformed = NV(currentValueStr, nv);

                                if (!string.IsNullOrEmpty(transformed))
                                {
                                    // Convert the transformed string based on "type"
                                    itemType = item["type"]?.GetValue<string>();
                                    switch (itemType)
                                    {
                                        case "Boolean":
                                            if (bool.TryParse(transformed, out bool boolVal))
                                                item["value"] = JsonValue.Create(boolVal);
                                            else
                                                item["value"] = JsonValue.Create(false);
                                            break;
                                        case "Int32":
                                            if (int.TryParse(transformed, out int intVal))
                                                item["value"] = JsonValue.Create(intVal);
                                            else
                                                item["value"] = JsonValue.Create(0);
                                            break;
                                        case "Decimal":
                                        case "Single":
                                            if (decimal.TryParse(transformed, out decimal decVal))
                                                item["value"] = JsonValue.Create(decVal);
                                            else
                                                item["value"] = JsonValue.Create(0m);
                                            break;
                                        case "Select":
                                            item["value"] = JsonValue.Create(transformed);
                                            break;
                                        default:
                                            item["value"] = JsonValue.Create(transformed);
                                            break;
                                    }
                                }
                                else
                                {
                                    item["value"] = JsonValue.Create("");
                                }
                            }

                            // converter
                            string? conv = item["converter"]?.GetValue<string>();
                            if (!string.IsNullOrEmpty(conv))
                            {
                                switch (conv)
                                {
                                    case "daysofweek":
                                        // Example: "0,2,4"
                                        string? daysStr = item["value"]?.GetValue<string>();
                                        if (!string.IsNullOrEmpty(daysStr))
                                        {
                                            string[] days = daysStr.Trim(',').Split(',');
                                            // "options" is an array of something
                                            JsonArray? options = item["options"] as JsonArray;
                                            if (options != null)
                                            {
                                                int i = 0;
                                                foreach (JsonNode? optNode in options)
                                                {
                                                    if (optNode is JsonObject optObj)
                                                    {
                                                        if (days.Contains(i.ToString(CultureInfo.InvariantCulture)))
                                                        {
                                                            optObj["value"] = JsonValue.Create(true);
                                                        }
                                                    }
                                                    i++;
                                                }
                                            }
                                        }
                                        break;

                                    case "datetimetoint":
                                        // Convert stored DateTime => total minutes
                                        if (item["value"] != null &&
                                            item["value"] is JsonValue dtVal &&
                                            dtVal.TryGetValue<DateTime>(out DateTime dt))
                                        {
                                            double minutes = dt.TimeOfDay.TotalMinutes;
                                            item["value"] = JsonValue.Create(minutes);
                                        }
                                        break;

                                    case "rgbtohex":
                                        // "r,g,b" => "#RRGGBB"
                                        string? rgbStr = item["value"]?.GetValue<string>();
                                        if (!string.IsNullOrEmpty(rgbStr))
                                        {
                                            var rgbArr = rgbStr.Split(',');
                                            if (rgbArr.Length == 3)
                                            {
                                                if (short.TryParse(rgbArr[0], out short r) &&
                                                    short.TryParse(rgbArr[1], out short g) &&
                                                    short.TryParse(rgbArr[2], out short b))
                                                {
                                                    string hex = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
                                                    item["value"] = JsonValue.Create(hex);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // Multiple properties
                            // Build a JSON array from these multiple property values
                            string json = "[";
                            foreach (var s in prop)
                            {
                                var val = GetPropValue(o, s);
                                json += val?.ToString() + ",";
                            }
                            json = json.TrimEnd(',') + "]";

                            // Parse that array into a JsonNode
                            item["value"] = JsonNode.Parse(json);
                        }
                    }
                    catch (Exception ex)
                    {
                        LastException = ex;
                        item["value"] = JsonValue.Create("");
                    }
                }
            }

            return d;
        }

        public static void PopulateObject(JsonNode? jsonNode, object o)
        {
            //Console.WriteLine(jsonNode);
            if (jsonNode == null) return;

            // "sections" is expected to be a JSON array
            JsonArray? sections = jsonNode["sections"] as JsonArray;
            //Console.WriteLine(sections);
            if (sections == null) return;

            foreach (JsonNode? sec in sections)
            {
                // Each "sec" should be a JsonObject
                if (sec is not JsonObject secObj)
                    continue;

                // "items" also expected to be an array
                JsonArray? items = secObj["items"] as JsonArray;
                if (items == null) continue;

                foreach (JsonNode? itemNode in items)
                {
                    // Each "item" should be a JsonObject
                    if (itemNode is not JsonObject item)
                        continue;

                    // Check if "bindto" exists
                    JsonNode? bt = item["bindto"];
                    if (bt == null)
                        continue;

                    // Check if "value" exists
                    JsonNode? val = item["value"];
                    if (val == null)
                        continue;

                    //Console.WriteLine("populate");
                    Populate(item, o);
                }
            }
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


        /// <summary>
        /// Scales a rectangle of percentages to frame coordinates
        /// </summary>
        /// <param name="R">The percentage based rectangle</param>
        /// <param name="sz">Size of the frame</param>
        /// <returns></returns>
        public static Rectangle ScalePercentageRectangle(Rectangle R, Size sz)
        {
            double wmulti = Convert.ToDouble(sz.Width) / Convert.ToDouble(100);
            double hmulti = Convert.ToDouble(sz.Height) / Convert.ToDouble(100);

            return new Rectangle(Convert.ToInt32(R.X * wmulti),
                                        Convert.ToInt32(R.Y * hmulti), Convert.ToInt32(R.Width * wmulti),
                                        Convert.ToInt32(R.Height * hmulti));
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

                            val = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) + "," +
                                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) + "," +
                                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.Message + ": " + ex.StackTrace);
                            LastException = ex;
                        }
                        break;
                }
            }

            var props = bt.ToString().Split(',');
            if (props.Length > 1)
            {
                int i = 0;
                if (val is JsonValue jValue && jValue.TryGetValue<string>(out var stringVal))
                {
                    val = stringVal.Split(',');
                }
                foreach (string s in props)
                {
                    try
                    {
                        SetPropValue(o, s, val[i]);
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message+": "+ex.StackTrace);
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
                    //Console.WriteLine(ex.Message + ": " + ex.StackTrace);
                    LastException = ex;
                }
            }
        }
        


        public static List<Line2D> ParseTripWires(Size frameSize, string json)
        {
            var tripWireList = new List<Line2D>();
            // Configure JsonSerializer options if needed

            if (string.IsNullOrEmpty(json)) return tripWireList;

            var lp = JsonSerializer.Deserialize<List<Points>>(json, JSONOptions);
            if (lp != null)
            {
                foreach (var p in lp)
                {
                    tripWireList.Add(new Line2D(frameSize, new Point(Convert.ToInt32(p.x), Convert.ToInt32(p.y)), new Point(Convert.ToInt32(p.x2), Convert.ToInt32(p.y2))));
                }
            }
            return tripWireList;
        }

        public static List<Rectangle> ParseAreas(Size frameSize, string json)
        {
            var areaList = new List<Rectangle>();
            if (string.IsNullOrEmpty(json)) return areaList;

            var lp = JsonSerializer.Deserialize<List<Areas>>(json, JSONOptions);
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
            // Extract the actual primitive value
            object actualValue = ExtractPrimitiveValue(propValue);
            //Console.WriteLine($"{propName} = {actualValue}");

            // Navigate to the target property
            object currentObject = src;
            string[] fieldNames = propName.Split('.');

            for (int i = 0; i < fieldNames.Length - 1; i++)
            {
                string fieldName = fieldNames[i];
                PropertyInfo propInfo = currentObject.GetType().GetProperty(fieldName);
                if (propInfo == null)
                {
                    throw new ArgumentException($"Property '{fieldName}' not found on type '{currentObject.GetType().FullName}'.");
                }
                currentObject = propInfo.GetValue(currentObject, null);
                if (currentObject == null)
                {
                    throw new NullReferenceException($"Property '{fieldName}' is null.");
                }
            }

            // Get the final property to set
            PropertyInfo finalProp = currentObject.GetType().GetProperty(fieldNames[^1]);
            if (finalProp == null)
            {
                // Support example JSON with no bindings
                return;
            }

            if (actualValue == null)
            {
                // If the property is nullable, set it to null
                if (finalProp.PropertyType.IsClass || (Nullable.GetUnderlyingType(finalProp.PropertyType) != null))
                {
                    finalProp.SetValue(currentObject, null, null);
                    return;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot assign null to non-nullable property '{finalProp.Name}'.");
                }
            }

            // Determine the target type and set the value accordingly
            Type targetType = finalProp.PropertyType;
            try
            {
                object convertedValue;

                if (targetType == typeof(string))
                {
                    convertedValue = actualValue.ToString();
                }
                else if (targetType == typeof(int))
                {
                    convertedValue = Convert.ToInt32(actualValue, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(decimal))
                {
                    convertedValue = Convert.ToDecimal(actualValue, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(float))
                {
                    convertedValue = Convert.ToSingle(actualValue, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(double))
                {
                    convertedValue = Convert.ToDouble(actualValue, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(bool))
                {
                    if (actualValue is bool b)
                        convertedValue = b;
                    else
                        convertedValue = Convert.ToBoolean(actualValue, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(DateTime))
                {
                    convertedValue = Convert.ToDateTime(actualValue, CultureInfo.InvariantCulture);
                }
                else
                {
                    // For other types, attempt to deserialize using System.Text.Json
                    if (actualValue is string jsonString)
                    {
                        convertedValue = JsonSerializer.Deserialize(jsonString, targetType);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(actualValue, targetType, CultureInfo.InvariantCulture);
                    }
                }

                finalProp.SetValue(currentObject, convertedValue, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error setting property '{finalProp.Name}' with value '{actualValue}'.", ex);
            }
        }

        static object ExtractPrimitiveValue(object propValue)
        {
            if (propValue is JsonElement jsonElement)
            {
                switch (jsonElement.ValueKind)
                {
                    case JsonValueKind.String:
                        return jsonElement.GetString();
                    case JsonValueKind.Number:
                        if (jsonElement.TryGetInt32(out int intValue))
                            return intValue;
                        if (jsonElement.TryGetInt64(out long longValue))
                            return longValue;
                        if (jsonElement.TryGetDouble(out double doubleValue))
                            return doubleValue;
                        return jsonElement.GetDecimal();
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return jsonElement.GetBoolean();
                    case JsonValueKind.Null:
                        return null;
                    default:
                        // For Object and Array, return the raw JSON string or handle as needed
                        return jsonElement.GetRawText();
                }
            }
            else if (propValue is JsonNode jsonNode)
            {
                if (jsonNode is JsonValue jsonValue)
                {
                    // Attempt to extract as different primitive types
                    if (jsonValue.TryGetValue<bool>(out bool boolVal))
                        return boolVal;
                    if (jsonValue.TryGetValue<int>(out int intVal))
                        return intVal;
                    if (jsonValue.TryGetValue<long>(out long longVal))
                        return longVal;
                    if (jsonValue.TryGetValue<double>(out double doubleVal))
                        return doubleVal;
                    if (jsonValue.TryGetValue<string>(out string stringVal))
                        return stringVal;
                    // Add more types as needed
                    return jsonValue.ToJsonString(); // Fallback for unsupported types
                }
                else
                {
                    // It's a JsonObject or JsonArray
                    return jsonNode.ToJsonString(); // Or handle as needed
                }
            }
            else
            {
                // If it's already a primitive type, return it as is
                return propValue;
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
