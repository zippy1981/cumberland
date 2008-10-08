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
using System.Drawing;
using System.Drawing.Imaging;

using System.Diagnostics;

using Cumberland;

namespace Cumberland.DrawMap
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Stopwatch sw = new Stopwatch();
			
			sw.Start();
			
			MapRenderer map = new MapRenderer();
			//map.Extents = new Rectangle(-115, 14, -87, 34);
			map.Extents = new Rectangle(-85, 29, -81, 31);
			//map.Extents = new Rectangle(-80777, 42799, 936488, 786156);
			
			map.Width = 400;
			map.Height = 400;
			map.Projection = "+init=epsg:4326";
			
//			AddShapefile(map, new Shapefile("/home/scottell/gis/data/world_adm0/world_adm0.shp"));
//			AddShapefile(map, new Shapefile("/home/scottell/Projects/cumberland/Cumberland.Tests/shape_eg_data/mexico/states.shp"));
//			AddShapefile(map, new Shapefile("/home/scottell/Projects/cumberland/Cumberland.Tests/shape_eg_data/mexico/roads.shp"));
//			AddShapefile(map, new Shapefile("/home/scottell/Projects/cumberland/Cumberland.Tests/shape_eg_data/mexico/cities.shp"));
			AddShapefile(map, new Shapefile("/home/scottell/gis/data/florida/cntshr/cntshr.shp"));
			AddShapefile(map, new Shapefile("/home/scottell/gis/data/florida/par_citylm_2007/par_citylm_2007.shp"));
			AddShapefile(map, new Shapefile("/home/scottell/gis/data/florida/majrds_apr08/majrds_apr08.shp"));
			
			System.Console.WriteLine("Load Time (ms): " + sw.Elapsed.TotalMilliseconds);
		
			
			Bitmap b = map.Draw();
			b.Save("/home/scottell/Desktop/test.png", ImageFormat.Png);   
			
			sw.Stop();
			
			System.Console.WriteLine("Elapsed Time (ms): " + sw.Elapsed.TotalMilliseconds);
		}
		
		static void AddShapefile(MapRenderer map, Shapefile shapefile)
		{
			Random r = new Random();
			
			Layer l = new Layer();
			l.Data = shapefile;
			l.PointSize = r.Next(5)+1;
			l.FillColor =  Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
			l.LineColor = Color.FromArgb(r.Next(155), r.Next(155), r.Next(155));
			l.LineWidth = r.Next(3)+1;
			l.Projection = "+init=epsg:3087";
			//l.LineStyle = LineStyle.None;
			map.Layers.Add(l);
		}
	}
}