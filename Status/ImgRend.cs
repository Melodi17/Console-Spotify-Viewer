using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Crayon;

namespace Status
{
    public class ImgRend
    {
        private static string[] _AsciiChars = { "█", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
        public static int Size = 20;
        public static string DrawMode = "fill";
        public static Color Invert(Color c) => Color.FromArgb(Invert(c.R), Invert(c.G), Invert(c.B));

        public static byte Invert(byte b)
        {
            return (byte)(b + 128);
        }
        public static void Sweep(SpotifyRoot root)
        {
            Console.SetCursorPosition(0, 0);
            Bitmap bmp = new("temp.png");
            Bitmap resizedBmp = GetResizedImage(bmp, Size);
            Console.Write(ConvertToAscii(resizedBmp));
            Color c = AverageColors(bmp);
            //DrawMode = new Random().Next(5) == 0 ? "gray" : "fill";
            var bn = c.GetBrightness();
            if (bn <= 0.1)
                c = Invert(c);

            List<string> lines = new();
            lines.Add(DateTime.Now.ToString("hh:mm:ss dddd dd/MM/yyyy"));
            lines.Add(null);
            lines.Add(root.item.name + (root.item.@explicit ? Output.Rgb(150, 150, 150).Bold().Text("[E] ") : "") + (root.is_playing ? "" : Output.Rgb(150, 150, 150).Bold().Text(" (Paused)")));
            lines.Add(root.item.album.name);
            lines.Add(string.Join(", ", root.item.artists.Select(x => x.name)));
            lines.Add(null);

            int max = 50;
            float percent = (float)root.progress_ms / (float)root.item.duration_ms;
            int bars = (int)(percent * (float)max);
            lines.Add($"{Output.Blue().Bold().Text(new string('-', bars))}{Output.Rgb(150, 150, 150).Bold().Text(new string('-', max - bars))}");

            Console.CursorTop = (Size / 4) - (lines.Count / 2);
            foreach (var item in lines)
            {
                if (item != null)
                {
                    Console.CursorLeft = Size + 2;
                    Console.Write(Output.Rgb(c.R, c.G, c.B).Text(item));
                    Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
                }
                Console.CursorTop++;
            }

            bmp.Dispose();
            resizedBmp.Dispose();
        }

        public static Color AverageColors(Bitmap image)
        {
            Color main = Color.Black;
            bool run = false;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color c = image.GetPixel(x, y);
                    if (!run)
                    {
                        main = c;
                        run = true;
                    }
                    else
                        main = main.Blend(c, (image.Width * image.Height) / 225);
                }
            }
            return main;
        }
        public static string ConvertToAscii(Bitmap image)
        {
            bool toggle = false;
            StringBuilder sb = new();

            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    Color pixelColor = image.GetPixel(w, h);
                    //Average out the RGB components to find the Gray Color
                    int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color grayColor = Color.FromArgb(red, green, blue);

                    //Use the toggle flag to minimize height-wise stretch
                    if (!toggle)
                    {
                        int index = (grayColor.R * 10) / 255;
                        int colIndex = (int)pixelColor.A * 10 / 225;
                        if (pixelColor == Color.Transparent || pixelColor.A < 50)
                        {
                            sb.Append(Output.Black(" "));
                        }
                        else
                        {
                            if (DrawMode == "fill")
                            {
                                sb.Append(Output.Rgb(pixelColor.R, pixelColor.G, pixelColor.B).Text(_AsciiChars[0]));
                            }
                            else if (DrawMode == "gray")
                            {
                                sb.Append(Output.Rgb(pixelColor.R, pixelColor.G, pixelColor.B).Text(_AsciiChars[index]));
                            }
                            else if (DrawMode == "opacity")
                            {
                                sb.Append(Output.Rgb(pixelColor.R, pixelColor.G, pixelColor.B).Text(_AsciiChars[10 - (colIndex - 1)]));
                            }
                        }
                    }
                }
                if (!toggle)
                {
                    sb.Append('\n');
                    toggle = true;
                }
                else
                {
                    toggle = false;
                }
            }

            return sb.ToString();
        }
        static Bitmap GetResizedImage(Bitmap inputBitmap, int asciiWidth)
        {
            int asciiHeight = 0;
            //Calculate the new Height of the image from its width
            asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width);

            //Create a new Bitmap and define its resolution
            Bitmap result = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((Image)result);
            //The interpolation mode produces high quality images 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return result;
        }
    }
    public static class ColorExtensions
    {
        /// <summary>Blends the specified colors together.</summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="color"/> to keep,
        /// “on top of” <paramref name="backColor"/>.</param>
        /// <returns>The blended colors.</returns>
        public static Color Blend(this Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
    }
}
