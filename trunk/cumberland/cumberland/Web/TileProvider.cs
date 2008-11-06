// TileProvider.cs
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

using Cumberland;
using Cumberland.Drawing;
using Cumberland.Projection;

namespace Cumberland.Web
{
	public enum TileConsumer
	{
		Google
	}
	
	public class TileProvider
	{
		TileConsumer consumer = TileConsumer.Google;
		int tileSize = 256;
		bool drawExceptionsOnTile = true;
		
		Map map;
		
		public TileConsumer Consumer {
			get {
				return consumer;
			}
		}

		public bool DrawExceptionsOnTile {
			get {
				return drawExceptionsOnTile;
			}
			set {
				drawExceptionsOnTile = value;
			}
		}
		
		public TileProvider(Map map)
		{
			this.map = map;
			
			// reproject to Google's spherical mercator
			map.Projection = ProjFourWrapper.SphericalMercatorProjection;
			
			// set to tile size
			map.Width = map.Height = tileSize;
		}
		
		public Bitmap DrawTile(int x, int y, int zoomLevel)
		{
			Bitmap b;
			
			try
			{
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
				if (!DrawExceptionsOnTile) throw;
				
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
						msg = msg.Insert(ii*numCharsPerRow, "\r\n");
					}

					gr.DrawString(msg,
					              new Font("Arial", 10, GraphicsUnit.Pixel),
					              Brushes.Red,
					              10, 
					              10);
				}
			}
			
			return b;
		}
	}
}
