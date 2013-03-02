using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SplashCook
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 4)
			{
				PrintHelp();
				return;
			}

			var inputPath = args[0];
			var outputPath = args[1] == "GUID" ? Guid.NewGuid() + ".png" : args[1];
			var showInstantly = args[2].EndsWith("yes");
			var toppings = args.Skip(3).Select(Topping.FromSting).ToArray();
			var bitmapFrame = Render(inputPath, toppings);

			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(bitmapFrame);
			using (var stream = File.OpenWrite(outputPath))
				encoder.Save(stream);

			if (showInstantly)
				Process.Start(outputPath);
		}

		static BitmapFrame Render(string inputPath, IEnumerable<Topping> toppings)
		{
			var fileInfo = new FileInfo(inputPath);
			var bitmap = BitmapFrame.Create(new Uri(fileInfo.FullName));
			var visual = new DrawingVisual();

			using (DrawingContext drawingContext = visual.RenderOpen())
			{
				drawingContext.DrawImage(
					imageSource:bitmap, 
					rectangle:new Rect(x:0, y:0, width:bitmap.Width, height:bitmap.Height));

				foreach (var t in toppings)
				{
					var topRightPoint = new Point(t.X, t.Y);
					
					var formattedText = new FormattedText(t.Text,
						CultureInfo.InvariantCulture,
						FlowDirection.LeftToRight, 
						typeface:new Typeface(
							fontFamily:new FontFamily(t.FontName), 
							style:FontStyles.Normal, 
							weight:t.FontWeight == "Bold" ? FontWeights.Bold : FontWeights.Normal,
							stretch:FontStretches.Normal), 
							emSize:t.FontSize, 
							foreground:(Brush)new BrushConverter().ConvertFrom(t.FontColorHex), 
							numberSubstitution:null, textFormattingMode:TextFormattingMode.Ideal);					

					drawingContext.DrawText(formattedText, 
						origin:new Point(topRightPoint.X, topRightPoint.Y));
				}
			}

			var renderTargetBitmap = new RenderTargetBitmap(
				bitmap.PixelWidth,
				bitmap.PixelHeight,
				bitmap.DpiX,
				bitmap.DpiY,
				PixelFormats.Default);
			renderTargetBitmap.Render(visual);
			return BitmapFrame.Create(renderTargetBitmap);
		}

		static void PrintHelp()
		{
			Console.WriteLine(@"Console utility for Splash Screen bitmap generation with custom provided text. Usage:
splash_cook.exe input-file output-file flags ... [args] ...

input-file		: Relative path for original image
output-file		: Relative path for resulting image

Flags :
--show-result=[yes|no]	: shows external image viewer with result

Args should be specified at least one, or many, in format as strict as such:
X,Y,FONT_NAME,SIZE,WEIGHT,#HEX_COLOR,TEXT_WITH_UNDERSCORES

Specifics:
X,Y		:two coordinates relative to top left corner (0,0 is top left)
WEIGHT		:Bold or Normal
TEXT		:Replace spaces with underscores
#HEX_COLOR	:4 bytes, e.g. #00000000

Examples:

splash_cook.exe ..\Res\SplashTemplate.png ..\Res\Splash.png --show-result=no 304,4,Verdana,30,Bold,#1a004780,Hello 100,500,Comic_Sans,5,Bold,#00000000,Brothers_and_sisters");
		}

		class Topping
		{
			public string FontColorHex;

			public string FontName;

			public int FontSize;

			public string FontWeight;

			public string Text;

			public int X;

			public int Y;

			public static Topping FromSting(string @string)
			{
				const string test = @"304,4,Century_Gothic,30,Bold,#1abb4780,TEXT_BLA_BLA_BLA111222.dfmv-#@";
				var rgx = new Regex(@"(\d+),(\d+),(\w+),(\d+),(Bold|Normal),(#[a-fA-F0-9]{8}),(.+)");
				Match m = rgx.Match(@string);
				if (!m.Success)
					throw new ArgumentException(string.Format("Regex {0} does not match!", @string));

				return new Topping
				{
					X = int.Parse(m.Groups[1].Value),
					Y = int.Parse(m.Groups[2].Value),
					FontName = m.Groups[3].Value.Replace('_', ' '),
					FontSize = int.Parse(m.Groups[4].Value),
					FontWeight = m.Groups[5].Value,
					FontColorHex = m.Groups[6].Value,
					Text = m.Groups[7].Value.Replace('_', ' '),					
				};
			}
		}
	}
}