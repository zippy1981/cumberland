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
using Cumberland.Data.PostGIS;
using Cumberland.Data.Shapefile;
using Cumberland.Drawing;
using Cumberland.Projection;


namespace Cumberland.TileViewerTest
{

	public partial class TileProvider : System.Web.UI.Page
	{
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
				
				// create our layers
				
				Layer polygonLayer = new Layer();
				polygonLayer.Data = new Shapefile("../Cumberland.Tests/shape_eg_data/mexico/states.shp");
				polygonLayer.Projection = ProjFourWrapper.WGS84; // data is in WGS84
				polygonLayer.LineColor = Color.RoyalBlue;
				polygonLayer.FillColor = Color.AliceBlue;
				polygonLayer.LineWidth = 1;
				//polygonLayer.LineStyle = LineStyle.Dotted;
				map.Layers.Add(polygonLayer);
				
				Layer lineLayer = new Layer();
				lineLayer.Data = new Shapefile("../Cumberland.Tests/shape_eg_data/mexico/roads.shp");
				lineLayer.Projection = ProjFourWrapper.WGS84; // data is in WGS84
				lineLayer.LineColor = Color.Green;
				lineLayer.LineWidth = 2;
				map.Layers.Add(lineLayer);
				
				Layer pointLayer = new Layer();
				pointLayer.Data = new Shapefile("../Cumberland.Tests/shape_eg_data/mexico/cities.shp");
				pointLayer.Projection = ProjFourWrapper.WGS84;
				pointLayer.PointSize = 4;
				pointLayer.FillColor = Color.Red;
				map.Layers.Add(pointLayer);
				
//				Layer pgLayer = new Layer();
//				pgLayer.Data = new PostGIS("Server=127.0.0.1;Port=5432;User Id=pguser;Password=pgpublic;Database=florida;", 
//				                           "counties");
//				pgLayer.Projection = ProjFourWrapper.PrepareEPSGCode(3087);
//				pgLayer.LineColor = Color.Orange;
//				map.Layers.Add(pgLayer);
	
				// acquire the tile index and zoom level
				int x = Convert.ToInt32(Request.QueryString["x"]);
				int y = Convert.ToInt32(Request.QueryString["y"]);
				int zoomLevel = Convert.ToInt32(Request.QueryString["zl"]);
				
				// calculate number of tiles across
				int numTiles = Convert.ToInt32(Math.Pow(2, zoomLevel));
				
				// get projection to grab circumference
				SphericalMercatorProjector prj = new SphericalMercatorProjector();
				
				// get meters/pixel resolution for zoomlevel
				double resolution = (prj.Circumference / 256) / numTiles;
				
				// mercator origin
				double origin = prj.Circumference/2;

				// convert pixels to meters and translate to origin
				map.Extents = new Rectangle((256*x) * resolution - origin,
				                            (256*(numTiles-y-1)) * resolution - origin, // google tiles are top left
				                            (256*(x+1)) * resolution - origin,
				                            (256*(numTiles-y)) * resolution - origin);
				
				MapDrawer renderer = new MapDrawer();
			
				// draw our map
				b = renderer.Draw(map);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);

				b = new Bitmap(256, 256);
				
				// draw the exception on the tile
				using (Graphics gr = Graphics.FromImage(b))
				{
					//string msg = ex.Message;
					//if (msg.Length*10 > b.Width - 20) msg = msg.Insert(b.Width/10-2, "\r\n");
					
					//System.Console.Write(msg);
					
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
			
			Response.Flush();
		}	
	}
}
