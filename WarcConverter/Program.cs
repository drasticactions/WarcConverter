using Shaman.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using Shaman.Dom;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.Globalization;
using System.Drawing;
using Shaman.Scraping;
using MiiverseArchive.Context;
using MiiverseArchive.Entities.Post;
using System.Windows.Forms;
namespace WarcConverter
{
    public class ColorShirt {
        public string ShirtColor { get; set; }

        public int Total { get; set; }
    }


    class Program
    {
        private static Color GetClosestColor(Color[] colorArray, Color baseColor)
        {
            var colors = colorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
            var min = colors.Min(x => x.Diff);
            return colors.Find(x => x.Diff == min).Value;
        }

        private static int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;
            return a * a + r * r + g * g + b * b;
        }

        static void Main(string[] args)
        {
            Shaman.Runtime.SingleThreadSynchronizationContext.Run(MainImageColor);
        }

        public static async Task MainImageColor()
        {
            List<Color> targets = new List<Color>();
            targets.Add(Color.Blue);
            targets.Add(Color.DarkBlue);
            targets.Add(Color.LightBlue);
            targets.Add(Color.Yellow);
            targets.Add(Color.Green);
            targets.Add(Color.Pink);
            targets.Add(Color.White);
            targets.Add(Color.Black);
            targets.Add(Color.Red);
            targets.Add(Color.Purple);
            targets.Add(Color.Orange);
            var colorArray = targets.ToArray();
            var items = WarcItem.ReadIndex(@"C:\WebsiteDumps\site-useravatars-surprised-miiverse.nintendo.net\index.cdx").Where(x => x.ContentType.Contains("image") && x.Url.Contains("cdn"));
            var shirtList = new List<ColorShirt>();
            foreach (var item in items)
            {
                using (var itemStream = item.OpenStream())
                using (var bitmap = (Bitmap)Bitmap.FromStream(itemStream))
                {
                    var color = bitmap.GetPixel(47, 95);
                    var closest = GetClosestColor(colorArray, color);
                    var shirt = shirtList.FirstOrDefault(n => n.ShirtColor == closest.Name);
                    if (shirt == null)
                    {
                        shirtList.Add(new ColorShirt() { ShirtColor = closest.Name, Total = 1 });
                    }
                    else
                    {
                        shirt.Total = shirt.Total + 1;
                    }
                }
            }

            File.WriteAllText("colorShirt.json", JsonConvert.SerializeObject(shirtList));
        }

        public static async Task MainAsyncUsers()
        {
            //if (!File.Exists("output2.txt"))
            //    File.Create("output2.txt");
            //var emotionList = new List<string>() { "happy", "like", "surprised", "fustrated", "puzzled" };
            var directories = Directory.EnumerateDirectories("site-users-userlist");
            foreach(var directory in directories)
            {
                Console.WriteLine(directory);
                var items = WarcItem.ReadIndex($"{directory}/index.cdx").Where(x => x.ContentType.Contains("text/html")).ToList();
                foreach (var item in items)
                {
                    using (var content = item.OpenStream())
                    {
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.Load(content, System.Text.Encoding.UTF8);
                        var postNode = doc.DocumentNode.Descendants("div").FirstOrDefault(n => n.GetAttributeValue("class", "") == "icon-container");
                        if (postNode == null) continue;
                        var avatar = postNode.Descendants("img").FirstOrDefault();
                        if (avatar == null) continue;
                        var avatarLink = avatar.GetAttributeValue("src", "");
                        File.AppendAllText("output.txt", avatarLink + System.Environment.NewLine);
                    }
                }
            }
        }

        public static async Task MainAsyncLink()
        {
            var items = WarcItem.ReadIndex("site/index.cdx").Where(x => x.ContentType.Contains("text/html")).ToList();
            var context = new MiiverseContext("", "", "");
            var url = "https://image.miiverse.nintendo.net/a/posts/AYMHAAABAABtUKlQN2DnMA/painting";
            foreach (var item in items)
            {
                using (var content = item.OpenStream())
                {
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.Load(content, System.Text.Encoding.UTF8);

                    if (doc.DocumentNode.OuterHtml.Contains(url))
                        Console.WriteLine(item.Url);
                }
            }
        }

        public static async Task MainAsyncPosts()
        {
            var items = WarcItem.ReadIndex("site/index.cdx").Where(x => x.ContentType.Contains("text/html")).ToList();
            var context = new MiiverseContext("", "", "");
            var posts = new List<Post>();
            foreach (var item in items)
            {
                using (var content = item.OpenStream())
                {
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.Load(content, System.Text.Encoding.UTF8);
                    var postNode = doc.DocumentNode.Descendants("section").FirstOrDefault(n => n.GetAttributeValue("id", "") == "post-content");
                    if (postNode == null) continue;
                    posts.Add(context.ParsePost(postNode));
                }
            }

            File.WriteAllText("test.json", JsonConvert.SerializeObject(posts, Formatting.Indented));
        }


        public static async Task MainAsyncImagesMesh()
        {
            var items = WarcItem.ReadIndex("site/index.cdx").Where(x => x.ContentType.Contains("image") && x.Url.Contains("cdn")).ToList();
            string finalImage = @"FinalImageWhite.png";
            int nIndex = 0;
            int width = 0;
            var testHeight = 96;
            int height = 3840;
            List<int> imageHeights = new List<int>();
            foreach (var item in items)
            {
                if (testHeight > height)
                {
                    testHeight = 0;
                    width = width + 96;
                }
                else
                {
                    testHeight = testHeight + 96;
                }
            }
            Bitmap img3 = new Bitmap(3840, 2160);
            Graphics g = Graphics.FromImage(img3);
            g.Clear(Color.White);
            width = 0;
            testHeight = 0;
            for (var i = 0; i < items.Count(); i++)
            {
                var item = items[i];
                Image img = Image.FromStream(item.OpenStream());
                g.DrawImage(img, new Rectangle(width, testHeight, 96, 96));
                if (testHeight > height)
                {
                    testHeight = 0;
                    width = width + 96;
                }
                else
                {
                    testHeight = testHeight + 96;
                }
                Console.WriteLine($"{width} - {testHeight}");
                img.Dispose();
            }
            g.Dispose();
            img3.Save(finalImage, System.Drawing.Imaging.ImageFormat.Png);
            img3.Dispose();
        }

        public static async Task MainAsyncImageDownload()
        {
            var items = WarcItem.ReadIndex("site/index.cdx").Where(x => x.ContentType.Contains("image")).ToList();
            foreach(var item in items)
            {
                var filename = "files" + new Uri(item.Url).LocalPath;
                FileInfo fileInfo = new FileInfo(filename);
                if (!fileInfo.Exists)
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                using (var stream = item.OpenStream())
                {
                    using (var test = File.Create(FixImageFilename(filename, item.ContentType)))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(test);
                    }
                }
            }

        }

        public static string FixImageFilename(string filename, string contenttype)
        {
            switch (contenttype)
            {
                case "image/png":
                    return filename.Contains(".png") ? filename : filename + ".png";
                case "image/gif":
                    return filename.Contains(".gif") ? filename : filename + ".gif";
                case "image/jpeg":
                case "image/jpg":
                    return filename.Contains(".jpg") ? filename : filename + ".jpg";
                case "image/x-icon":
                    return filename.Contains(".ico") ? filename : filename + ".ico";
                default:
                    throw new Exception("Unknown Type!");
            }
        }
    }
}
