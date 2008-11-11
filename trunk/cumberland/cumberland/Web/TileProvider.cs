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
		GoogleMaps
	}
	
	public class TileProvider
	{
		TileConsumer consumer = TileConsumer.GoogleMaps;
		int tileSize = 256;
		bool drawExceptionsOnTile = true;
		
		double circumference = 0;
		double origin;
		
		int maxZoomLevel = 19;
		int minZoomLevel = 0;
		
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
			
			// get projection to grab circumference
			SphericalMercatorProjector prj = new SphericalMercatorProjector();
			circumference = prj.Circumference;
			
			// set mercator origin (center of the map (0,0))
			origin = prj.Circumference/2;
		}
		
		public System.Drawing.Rectangle ClipRectangleAtZoomLevel(Rectangle rectangle, int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			// calculate number of tiles across
			int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
			
			// get meters/pixel resolution for zoomlevel
			double resolution = (circumference / tileSize) / numTiles;
			
			// - first translate merc points to origin so they will all be positive
			// - then convert to pixel coordinates 
			//   (Y pixel value must be reversed b/c google's origin is top-left, ours is bottom-right
			// - divide by tile size to figure out which tile we're in
			
			int minx = Convert.ToInt32(Math.Floor((rectangle.Min.X + origin) / (resolution * tileSize)));
			int miny = Convert.ToInt32(Math.Floor((tileSize*numTiles - 
			                                       ((rectangle.Max.Y + origin) / resolution)) / tileSize));
			int maxx = Convert.ToInt32(Math.Floor((rectangle.Max.X + origin) / (resolution * tileSize)));
			int maxy = Convert.ToInt32(Math.Floor((tileSize*numTiles - 
			                                       ((rectangle.Min.Y + origin) / resolution)) / tileSize));
			
			return new System.Drawing.Rectangle(minx, miny, (maxx-minx), (maxy-miny));
		}
		
		public int CalculateNumberOfTilesAcross(int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			return Convert.ToInt32(Math.Pow(2, zoomLevel));
		}
		
		public Bitmap DrawTile(int x, int y, int zoomLevel)
		{
			Bitmap b;
			
			try
			{
				CheckZoomLevel(zoomLevel);
				
				// calculate number of tiles across
				int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
				
				// get meters/pixel resolution for zoomlevel
				double resolution = (circumference / tileSize) / numTiles;
				
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
		
		void CheckZoomLevel(int zoomLevel)
		{
			if (zoomLevel > maxZoomLevel || zoomLevel < minZoomLevel)
			{
				throw new ArgumentOutOfRangeException("zoomLevel", 
				                                      string.Format("Zoom level must within {0} and {1} (got {2})",
				                                                    minZoomLevel,
				                                                    maxZoomLevel,
				                                                    zoomLevel));
			}
		}
	}
}
