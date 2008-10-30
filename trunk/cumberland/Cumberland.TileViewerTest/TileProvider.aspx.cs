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
using Cumberland.Xml.Serialization;


namespace Cumberland.TileViewerTest
{

	public partial class TileProvider : System.Web.UI.Page
	{
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			
			Bitmap b = null;
			
			// Google tile size is 256
			int tileSize = 256;

			// clear out response
			Response.Clear();
			Response.ContentType = "image/png";

			try
			{
				MapSerializer ms = new MapSerializer();
				ms.AddDBFeatureProvider(typeof(PostGISFeatureSource));

				Map map = ms.Deserialize(Request.QueryString["map"]);

				// reproject to Google's spherical mercator
				map.Projection = ProjFourWrapper.SphericalMercatorProjection;
			
				// set to tile size
				map.Width = map.Height = tileSize;
				
				// acquire the tile index and zoom level
				int x = Convert.ToInt32(Request.QueryString["x"]);
				int y = Convert.ToInt32(Request.QueryString["y"]);
				int zoomLevel = Convert.ToInt32(Request.QueryString["zl"]);
				
				// calculate number of tiles across
				int numTiles = Convert.ToInt32(Math.Pow(2, zoomLevel));
				
				// get projection to grab circumference
				SphericalMercatorProjector prj = new SphericalMercatorProjector();
				
				// get meters/pixel resolution for zoomlevel
				double resolution = (prj.Circumference / tileSize) / numTiles;
				
				// mercator origin
				double origin = prj.Circumference/2;

				// convert pixels to meters and translate to origin
				map.Extents = new Rectangle((tileSize*x) * resolution - origin,
				                            (tileSize*(numTiles-y-1)) * resolution - origin, // google tiles are top left
				                            (tileSize*(x+1)) * resolution - origin,
				                            (tileSize*(numTiles-y)) * resolution - origin);
				
				MapDrawer renderer = new MapDrawer();
			
				// draw our map
				b = renderer.Draw(map);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);

				b = new Bitmap(tileSize, tileSize);
				
				// draw the exception on the tile
				using (Graphics gr = Graphics.FromImage(b))
				{
					string msg = ex.Message;
					int numCharsPerRow = (tileSize) / 5;
					int numLineBreaks = (int) Math.Ceiling(Convert.ToSingle(msg.Length) / 
					                                       Convert.ToSingle(numCharsPerRow));
	
					for (int ii=0; ii < numLineBreaks; ii++)
					{
						System.Console.WriteLine(ii);
						msg = msg.Insert(ii*numCharsPerRow, "\r\n");
					}

					gr.DrawString(msg,
					              new Font("Arial", 10, GraphicsUnit.Pixel),
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
