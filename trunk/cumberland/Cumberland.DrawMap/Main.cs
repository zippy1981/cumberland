// Main.cs
//
// Copyright (c) 2008 Scott Ellington and Authors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

using Cumberland;

using NDesk.Options;

namespace Cumberland.DrawMap
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			MapRenderer map = new MapRenderer();
			map.Width = 400;
			map.Height = 400;
			map.Projection = ProjFourWrapper.WGS84;
			
			bool showHelp = false;
			string path = "out.png";
			
			OptionSet options = new OptionSet();
			options.Add("e|extents=", 
			            "comma-delimited extents (e.g. -180,-90,180,90) ",
			            delegate (string v) { map.Extents = ParseExtents(v); });
			options.Add("h|help",  "show this message and exit",
			            delegate (string v) { showHelp = v!= null; });
			options.Add("o|output=",
			            "the path of the PNG image to create",
			            delegate (string v) { path = v; });
			options.Add("w|width=",
			            "the width of the image in pixels",
			            delegate (string v) { map.Width = int.Parse(v); });
			options.Add("t|height=",
			            "the height of the image in pixels",
			            delegate (string v) { map.Height = int.Parse(v); });
			options.Add("p|proj=",
			            "the output projection. can be a quoted proj4 string or an epsg code",
			            delegate (string v) { map.Projection = ParseProjection(v); });
			            
		
			List<string> rest = options.Parse(args);

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}
			
			Stopwatch sw = new Stopwatch();
			
			sw.Start();

			Random r = new Random();
			
			foreach (string arg in rest)
			{
				string[] layerArgs = arg.Split(',');

				Layer l = new Layer();
				l.Data = new Shapefile(layerArgs[0]);
				l.PointSize = r.Next(5)+1;
				l.FillColor =  Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
				l.LineColor = Color.FromArgb(r.Next(155), r.Next(155), r.Next(155));
				l.LineWidth = 1; //r.Next(3)+1;
				
				if (layerArgs.Length > 1)
				{
					l.Projection = ParseProjection(layerArgs[1]);;
				}
				
				//l.LineStyle = LineStyle.None;
				
				map.Layers.Add(l);
			}
			
			System.Console.WriteLine("Load Time (ms): " + sw.Elapsed.TotalMilliseconds);
		
			Bitmap b = map.Draw();
			
			if (System.IO.Path.GetExtension(path) != ".png")
			{
				path += ".png";
			}
			
			b.Save(path, ImageFormat.Png);   
			
			sw.Stop();
			
			System.Console.WriteLine("Elapsed Time (ms): " + sw.Elapsed.TotalMilliseconds);
		}
		
		static void ShowHelp (OptionSet p)
	    {
	        Console.WriteLine ("Usage: [mono] Cumberland.DrawMap.exe [OPTIONS]+ [\"path to shapefile\",epsg/\"proj4 string\"]+ ");
	        Console.WriteLine ("Draws a map based on the given layers and options.");
	        Console.WriteLine ();
			Console.WriteLine ("example: mono Cumberland.DrawMap.exe -o=my.png -p=4326 \"path/to/shape.shp\",3087  ");
			Console.WriteLine ();
	        Console.WriteLine ("Options:");
	        p.WriteOptionDescriptions (Console.Out);
	    }
		
		static Rectangle ParseExtents(string extents)
		{
			string[] coords = extents.Split(',');
			return new Rectangle(Convert.ToDouble(coords[0]),
			                     Convert.ToDouble(coords[1]),
			                     Convert.ToDouble(coords[2]),
			                     Convert.ToDouble(coords[3]));
		}
		
		static string ParseProjection(string v)
		{
			int epsg;
			if (int.TryParse(v, out epsg))
			{
				return "+init=epsg:" + v;
			}
			else return v;
		}		
	}
}