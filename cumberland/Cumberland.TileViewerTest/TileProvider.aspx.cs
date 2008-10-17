// TileProvider.aspx.cs
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
using System.Web;
using System.Web.UI;

using System.Drawing;
using System.Drawing.Imaging;

using Cumberland;
using Cumberland.Data.Shapefile;
using Cumberland.Drawing.OpenGL;

namespace Cumberland.TileViewerTest
{

	public partial class TileProvider : System.Web.UI.Page
	{
		static object padLock = new object();
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			
			Bitmap b = null;

			// clear out response
			Response.Clear();
			Response.ContentType = "image/png";

			try
			{
				// create our map renderer
				Map map = new Map();

				// reproject to Google's spherical mercator
				map.Projection = ProjFourWrapper.SphericalMercatorProjection;
				
				// Google tile size is 256
				map.Width = map.Height = 256;
				
				// create our layer
				Layer l = new Layer();
				l.Data = new Shapefile("../Cumberland.Tests/shape_eg_data/mexico/states.shp");
				//FIXME:	//new Shapefile("/home/scottell/gis/data/world_adm0/world_adm0.shp");
				
				// data is in WGS84
				l.Projection = ProjFourWrapper.WGS84;
				
				// set up symbology
				l.LineColor = Color.RoyalBlue;
				l.FillColor = Color.AliceBlue;
				l.LineWidth = 1;
				
				map.Layers.Add(l);
	
				// our web page will provide us with the lat/long boundaries of the tile
				Point min = new Point(Convert.ToDouble(Request.QueryString["minx"]), 
				                      Convert.ToDouble(Request.QueryString["miny"]));
				Point max = new Point(Convert.ToDouble(Request.QueryString["maxx"]), 
				                      Convert.ToDouble(Request.QueryString["maxy"]));
				
				// we must reproject the extents to google's spherical mercator
				using (ProjFourWrapper prj = new ProjFourWrapper(map.Projection))
				{
					min = prj.ConvertFromLatLong(min);
					max = prj.ConvertFromLatLong(max);
				}
				
				map.Extents = new Rectangle(min, max);
				
				lock (padLock)
				{
					OpenGlMapDrawer renderer = new OpenGlMapDrawer();
				
					// draw our map
					b = renderer.Draw(map);
				}

			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);

				b = new Bitmap(256, 256);
				
				// draw the exception on the tile
				using (Graphics gr = Graphics.FromImage(b))
				{
					gr.DrawString(ex.Message,
					              new Font("Arial", 10),
					              Brushes.Red,
					              10, 
					              10);
				}
			}
			
	
			using (b)
			{
				// stream out the image
				b.Save(Response.OutputStream, ImageFormat.Png);
			}
		}	 
	}
}
