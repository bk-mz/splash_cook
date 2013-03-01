using System;
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
		class Topping
		{
			public static Topping FromSting(string @string)
			{
				const string test = @"304,4,Century_Gothic,30,Bold,#1a4780,TEXT_BLA_BLA_BLA111222.dfmv-#@";
				//\d+.\d+,\w+,\d+,Bold|Normal,[a-fA-F0-9]{6}
				var rgx = new Regex(@"(\d+),(\d+),(\w+),(\d+),(Bold|Normal),(#[a-fA-F0-9]{6}),(.+)");
				var m = rgx.Match(@string);
				var topRightPoint = new Point(
					x:int.Parse(m.Groups[1].Value), 
					y:int.Parse(m.Groups[2].Value));
				var fontFamily = new FontFamily(m.Groups[3].Value.Replace('_', ' '));
				var emSize = int.Parse(m.Groups[4].Value);
				var fontWeight = m.Groups[5].Value == "Bold" ? FontWeights.Bold : FontWeights.Normal; // todo parse
				var typeFace = new Typeface(fontFamily, FontStyles.Normal, fontWeight, FontStretches.Normal);
				var brush = (Brush)new BrushConverter().ConvertFrom(m.Groups[6].Value);				
				var formattedText = new FormattedText(m.Groups[7].Value.Replace('_', ' '), 
					CultureInfo.InvariantCulture, 
					FlowDirection.LeftToRight, 
					typeFace, emSize, brush);
				return new Topping {FormattedText = formattedText, TopRightPoint = topRightPoint};
			}
			
			public Point TopRightPoint;
			public FormattedText FormattedText;
		}

		static void Main(string[] args)
		{
			if (args.Length <= 4)
			{
				PrintHelp();
				return;
			}

			var InputPath = args[0];
			var OutputPath = args[1];
			var showInstantly = args[2].EndsWith("yes");
			var IsBitmap = InputPath.EndsWith(".bmp");
			var toppings = args.Skip(3).Select(Topping.FromSting).ToArray();

			var fileInfo = new FileInfo(InputPath);
			var originalImageSource = BitmapFrame.Create(new Uri(fileInfo.FullName));
			var visual = new DrawingVisual();

			using (var drawingContext = visual.RenderOpen())
			{
				drawingContext.DrawImage(originalImageSource, new Rect(0, 0, originalImageSource.PixelWidth, originalImageSource.PixelHeight));
				foreach (var topping in toppings)
				{
					var point = new Point(topping.TopRightPoint.X - topping.FormattedText.Width, topping.TopRightPoint.Y);
					drawingContext.DrawText(topping.FormattedText, point);
				}				
			}

			var renderTargetBitmap = new RenderTargetBitmap(
				originalImageSource.PixelWidth, 
				originalImageSource.PixelHeight, 
				originalImageSource.DpiX, 
				originalImageSource.DpiY, 
				PixelFormats.Pbgra32);
			renderTargetBitmap.Render(visual);
			var bitmapFrame = BitmapFrame.Create(renderTargetBitmap);
			BitmapEncoder encoder = IsBitmap ? (BitmapEncoder)new BmpBitmapEncoder() : new PngBitmapEncoder();

			encoder.Frames.Add(bitmapFrame);
			using (var stream = File.OpenWrite(OutputPath))
				encoder.Save(stream);

			if (showInstantly)
				Process.Start(OutputPath);
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

Examples:

splash_cook.exe ..\Res\SplashTemplate.png ..\Res\Splash.png --show-result=no 304,4,Verdana,30,Bold,#1a4780,Hello 100,500,Comic_Sans,5,Bold,#000000,Brothers_and_sisters");
		}
	}

}
