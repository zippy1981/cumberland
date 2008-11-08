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
using System.IO;

using Cumberland;
using Cumberland.Projection;
using Cumberland.Xml.Serialization;
using Cumberland.Web;

using NDesk.Options;

namespace Cumberland.TilePyramidGenerator
{
	class MainClass
	{
		public static void Main(string[] args)
		{
#region check arguments
			
			Rectangle extents = new Rectangle();
			string path = ".";
			bool showHelp = false;
			int maxZoomLevel = 19;
			int minZoomLevel = 0;
			
			OptionSet options = new OptionSet();
			options.Add("e|extents=", 
			            "comma-delimited extents for clipping tile generation (e.g. -180,-90,180,90).  Overrides the map file ",
			            delegate (string v) { extents = ParseExtents(v); });
			options.Add("h|help",  "show this message and exit",
			            delegate (string v) { showHelp = v!= null; });
			options.Add("o|output=",
			            "the path of the where to create.  Defaults to current directory",
			            delegate (string v) { path = v; });
			options.Add("x|maxzoom=",
			            "the maximum zoom level",
			            delegate (string v) { maxZoomLevel = int.Parse(v); });
			options.Add("n|minzoom=",
			            "the minimum zoom level",
			            delegate (string v) { minZoomLevel = int.Parse(v); });
			
			List<string> rest = options.Parse(args);
			
			if (showHelp)
			{
				ShowHelp(options);
				return;
			}
			
			if (rest.Count == 0)
			{
				System.Console.WriteLine("No map provided");
				ShowHelp(options);
				return;
			}
			
			if (!File.Exists(rest[0]))
			{
				System.Console.WriteLine("Map xml file not found");
				ShowHelp(options);
				return;
			}

#endregion
			
#region get map
			
			MapSerializer ms = new MapSerializer();
			ms.AddDatabaseFeatureSourceType(typeof(Cumberland.Data.PostGIS.PostGISFeatureSource));
			ms.AddDatabaseFeatureSourceType(typeof(Cumberland.Data.SqlServer.SqlServerFeatureSource));
			
			Map map = ms.Deserialize(rest[0]);
			
			// use map extents as clipping range if not provided
			if (extents.IsEmpty && !map.Extents.IsEmpty)
			{
				extents = map.Extents;
			}
			
			// if map has a projection, reproject clipping area
			if (!extents.IsEmpty)
			{
				if (!string.IsNullOrEmpty(map.Projection))
				{
					using (ProjFourWrapper src = new ProjFourWrapper(map.Projection))
					{
						using (ProjFourWrapper dst = new ProjFourWrapper(ProjFourWrapper.SphericalMercatorProjection))
						{
							extents = new Rectangle(src.Transform(dst, extents.Min),
							                        src.Transform(dst, extents.Max));
						}
					}
				}
				else
				{
					System.Console.WriteLine(@"Warning: Your map doesn't have a projection.  
											Unless your data is in spherical mercator,  
											you will not get correct tiles");
				}
			}
			
#endregion

#region calculate total # of tiles
			
			TileProvider tp = new TileProvider(map);
			tp.DrawExceptionsOnTile = false;
			
			// calculate total number of tiles
			long totalCount = 0;
			for (int ii = minZoomLevel; ii <= maxZoomLevel; ii++)
			{
				if (extents.IsEmpty)
				{
					int across = tp.CalculateNumberOfTilesAcross(ii);
					totalCount += (Convert.ToInt64(across)*Convert.ToInt64(across));
				}
				else
				{
					System.Drawing.Rectangle r = tp.ClipRectangleAtZoomLevel(extents, ii);
					totalCount += Convert.ToInt64(r.Width+1) * Convert.ToInt64(r.Height+1);
				}
			}
			
			string info = string.Format("0{0} of {1}", new string(' ', totalCount.ToString().Length-1),totalCount);
			System.Console.Write(info);

#endregion
			
#region render tiles
			
			long current = 0;
			for (int ii = minZoomLevel; ii <= maxZoomLevel; ii++)
			{
				string tilepath = Path.Combine(path, ii.ToString());
				
				Directory.CreateDirectory(tilepath);
				
				System.Drawing.Rectangle r;
				if (extents.IsEmpty)
				{
					int across = tp.CalculateNumberOfTilesAcross(ii);
					r = new System.Drawing.Rectangle(0, 0, across, across);
				}
				else 
				{
					r = tp.ClipRectangleAtZoomLevel(extents, ii);
				}
				
				for (int x = r.Left; x <= (r.Left+r.Width); x++)
				{
					for (int y = r.Top; y <= (r.Top+r.Height); y++)
					{
						current++;
						
						// render tile and save to file
						Bitmap b = tp.DrawTile(x, y, ii);
						string tileFile = string.Format("{0}{1}{2}_{3}.png",
						              tilepath,
						              Path.DirectorySeparatorChar,
						              x,
						              y);
						b.Save(tileFile ,ImageFormat.Png);
						
						// update count in console
						Console.SetCursorPosition(0, Console.CursorTop);
						Console.Write(current.ToString());
						Console.SetCursorPosition(info.Length+1, Console.CursorTop);
					}
				}
				
				
			}
			
#endregion
			
			System.Console.WriteLine("Finished!");			
		}
		
#region helper methods
		
		static Rectangle ParseExtents(string extents)
		{
			string[] coords = extents.Split(',');
			return new Rectangle(Convert.ToDouble(coords[0]),
			                     Convert.ToDouble(coords[1]),
			                     Convert.ToDouble(coords[2]),
			                     Convert.ToDouble(coords[3]));
		}
		
		static void ShowHelp (OptionSet p)
	    {
	        Console.WriteLine ("Usage: [mono] Cumberland.TilePyramidGenerator.exe [OPTIONS]+ \"path to map file\" ");
	        Console.WriteLine ("Generates a pyramid of tile images for use for popular web mapping interfaces");
	        Console.WriteLine ();
			Console.WriteLine ("example: mono Cumberland.TilePyramidGenerator.exe  /path/to/map ");
			Console.WriteLine ();
	        Console.WriteLine ("Options:");
	        p.WriteOptionDescriptions (Console.Out);
	    }
		
#endregion
	}
}