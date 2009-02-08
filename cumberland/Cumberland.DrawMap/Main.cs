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
using System.IO;
using System.Reflection;

using Cumberland;
using Cumberland.Data.Shapefile;
using Cumberland.Data.PostGIS;
using Cumberland.Data.SqlServer;

using Cumberland.Drawing;
using Cumberland.Projection;
using Cumberland.Xml.Serialization;

using NDesk.Options;

namespace Cumberland.DrawMap
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			IMapDrawer drawer = new MapDrawer();
			
			bool showHelp = false;
			string path = "out.png";
			int w = -1;
			int h = -1;
			Rectangle extents = new Rectangle();
			bool showVersion = false;
			
			OptionSet options = new OptionSet();
			options.Add("e|extents=", 
			            "comma-delimited extents (e.g. -180,-90,180,90) ",
			            delegate (string v) { extents = ParseExtents(v); });
			options.Add("h|help",  "show this message and exit",
			            delegate (string v) { showHelp = v!= null; });
			options.Add("o|output=",
			            "the path of the PNG image to create",
			            delegate (string v) { path = v; });
			options.Add("w|width=",
			            "the width of the image in pixels",
			            delegate (string v) { w = int.Parse(v); });
			options.Add("t|height=",
			            "the height of the image in pixels",
			            delegate (string v) { h = int.Parse(v); });
			options.Add("v|version",
			            "shows the version and exits", 
			            delegate (string v) { showVersion = v != null; });
		
			List<string> rest = options.Parse(args);

			if (showVersion)
			{
				System.Console.WriteLine("Version " + 
				                         System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
				return;
			}
			
			if (showHelp)
			{
				ShowHelp(options);
				return;
			}
			
			if (rest.Count == 0)
			{
				System.Console.WriteLine("No map specified");
				ShowHelp(options);
				return;
			}

			// search in the local directory for espg files 
			// so Windows ppl don't have to have it installed
            ProjFourWrapper.CustomSearchPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			MapSerializer ms = new MapSerializer();
			ms.AddDatabaseFeatureSourceType(typeof(PostGISFeatureSource));
			ms.AddDatabaseFeatureSourceType(typeof(SqlServerFeatureSource));
			
			Map map = ms.Deserialize(rest[0]);
			if (w > 0) map.Width = w;
			if (h > 0) map.Height = h;
			if (!extents.IsEmpty) map.Extents = extents;

			System.Console.WriteLine(map.Layers.Count + " Layer(s) loaded");
			
			Bitmap b = drawer.Draw(map);
			
			if (System.IO.Path.GetExtension(path) != ".png")
			{
				path += ".png";
			}
			
			b.Save(path, ImageFormat.Png);   
		}
		
		static void ShowHelp (OptionSet p)
	    {
	        Console.WriteLine ("Usage: [mono] drawmap.exe [OPTIONS]+ \"path to map file\" ");
	        Console.WriteLine ("Draws a map");
	        Console.WriteLine ();
			Console.WriteLine ("example: mono drawmap.exe -o=my.png /path/to/map ");
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
	}
}