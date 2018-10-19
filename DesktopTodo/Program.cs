using DesktopTodo.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DesktopTodo
{
    class Program
    {
        static PointF StartingPoint { get; } = new PointF(1000f, 200f);
        static SizeF MaxSize { get; } = new SizeF(620f, 620f);
        static RectangleF MaxWorkAreaRectangle { get; } = new RectangleF(StartingPoint, MaxSize);

        static void Main(string[] args)
        {
            Task[] tasks = new Task[]
            {
                new Task{ Name ="test", Priority= Priority.Imp},
                new Task{ Name ="test", Priority= Priority.Warn},
                new Task{ Name ="test", Priority= Priority.BAU},
            };

            File.WriteAllText("todo.json", JsonConvert.SerializeObject(tasks));

            var test = JsonConvert.DeserializeObject<Task[]>(File.ReadAllText("todo.json"));

            var bitmap = (Bitmap)Resources.Todo;

            string data = "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.";

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawRectangles(new Pen(Brushes.Red), new RectangleF[] { MaxWorkAreaRectangle });
                using (Font arialFont = new Font("Arial", 12))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var s in GetDisplaystring(data, graphics, arialFont))
                        stringBuilder.AppendLine(s);

                    graphics.DrawString(stringBuilder.ToString(), arialFont, Brushes.Gray, MaxWorkAreaRectangle);
                }
            }

            bitmap.Save("modified.png");//save the image file
        }

        private static IEnumerable<string> GetDisplaystring(string data, Graphics graphics, Font font)
        {
            if (graphics.MeasureString(data, font).Width <= MaxSize.Width)
                return new string[] { data };

            List<string> splitString = new List<string>();

            var stringSplit1 = data.Substring(0, (int)(data.Length / 2));
            var stringSplit2 = data.Substring(stringSplit1.Length);

            var sizeSplit1 = Math.Ceiling(graphics.MeasureString(stringSplit1, font).Width);
            if (sizeSplit1 == Math.Ceiling(MaxSize.Width))
            {
                splitString.Add(stringSplit1);
                splitString.AddRange(GetDisplaystring(stringSplit2, graphics, font));
            }
            else if (sizeSplit1 < Math.Ceiling(MaxSize.Width))
            {
                foreach (var c in stringSplit2.ToCharArray())
                {
                    stringSplit1 += c;
                    var splitSize = Math.Ceiling(graphics.MeasureString(stringSplit1, font).Width);

                    if (splitSize == Math.Ceiling(MaxSize.Width))
                    {
                        splitString.Add(stringSplit1);

                        stringSplit2 = data.Substring(stringSplit1.Length);
                        splitString.AddRange(GetDisplaystring(stringSplit2, graphics, font));
                        break;
                    }
                    else if (splitSize > Math.Ceiling(MaxSize.Width))
                    {
                        stringSplit1 = stringSplit1.Remove(stringSplit1.Length - 1);
                        splitString.Add(stringSplit1);
                        stringSplit2 = data.Substring(stringSplit1.Length);
                        splitString.AddRange(GetDisplaystring(stringSplit2, graphics, font));
                        break;
                    }
                }
            }
            else
            {
                var result = GetDisplaystring(stringSplit1, graphics, font).ToList();
                if (Math.Ceiling(graphics.MeasureString(result.Last(), font).Width) == Math.Ceiling(MaxSize.Width))
                {
                    splitString.AddRange(result);
                    splitString.AddRange(GetDisplaystring(stringSplit2, graphics, font));
                }
                else
                {
                    var lastItem = result.Last();
                    result.RemoveAt(result.Count - 1);
                    splitString.AddRange(result);
                    splitString.AddRange(GetDisplaystring(string.Concat(lastItem, stringSplit2), graphics, font));

                }

            }

            return splitString;
        }
    }

    enum Priority
    {
        Imp,
        Warn,
        BAU
    }

    class Task
    {
        [JsonRequired]
        public string Name { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public Priority Priority { get; set; }
    }
}
