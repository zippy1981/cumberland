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

using Cumberland;

namespace Cumberland.DrawMap
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			MapRenderer map = new MapRenderer();
			
			map.Layers.Add(new Shapefile("/home/scottell/gis/data/world_adm0/world_adm0.shp"));
			map.Layers.Add(new Shapefile("/home/scottell/Projects/cumberland/Cumberland.Tests/shape_eg_data/mexico/states.shp"));
			map.Layers.Add(new Shapefile("/home/scottell/Projects/cumberland/Cumberland.Tests/shape_eg_data/mexico/roads.shp"));
			map.Layers.Add(new Shapefile("/home/scottell/Projects/cumberland/Cumberland.Tests/shape_eg_data/mexico/cities.shp"));
			
			Bitmap b = map.Draw();
			
			b.Save("/home/scottell/Desktop/test.png", ImageFormat.Png);
		}
	}
}