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
		None,
		GoogleMaps,
		TileMapService
	}
	
	public class TileProvider
	{
		
#region vars
		
		TileConsumer consumer = TileConsumer.GoogleMaps;
		int tileSize = 256;
		bool drawExceptionsOnTile = true;
		int maxZoomLevel = 19;
		int minZoomLevel = 0;
		Rectangle extents = new Rectangle();
		Map map;
		
#endregion
		
#region properties
		
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

		public int MinZoomLevel {
			get {
				return minZoomLevel;
			}
			set {
				
				if (consumer != TileConsumer.TileMapService)
				{
					throw new InvalidOperationException("Changing the minimum zoom level is not permitted for this tile consumer");
				}
				
				minZoomLevel = value;
			}
		}

		public int MaxZoomLevel {
			get {
				return maxZoomLevel;
			}
			set {
				
				if (consumer != TileConsumer.TileMapService)
				{
					throw new InvalidOperationException("Changing the minimum zoom level is not permitted for this tile consumer");
				}

				maxZoomLevel = value;
			}
		}

#endregion
		
#region ctors
		
		public TileProvider(Map map) : this(map, TileConsumer.GoogleMaps)
		{
		}
		
		public TileProvider(Map map, TileConsumer consumer)
		{
			if (consumer == TileConsumer.None)
			{
				throw new ArgumentException("'None' is not a valid consumer.", "consumer");
			}
			
			this.consumer = consumer;
			this.map = map;
							
			// set to tile size
			map.Width = map.Height = tileSize;
			
			if (consumer == TileConsumer.GoogleMaps)
			{
				// reproject to Google's spherical mercator
				map.Projection = ProjFourWrapper.SphericalMercatorProjection;
				
				// get projection to grab circumference
				SphericalMercatorProjector prj = new SphericalMercatorProjector();
				
				// set mercator origin (center of the map (0,0))
				double origin = prj.Circumference/2;
				
				extents = new Rectangle(-origin, -origin, origin, origin);
			}
			else if (consumer == TileConsumer.TileMapService)
			{
				extents = map.Extents.Clone();
			}
		}
		
#endregion
		
		public double CalculateMapUnitsPerPixel(int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			// calculate number of tiles across
			int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
			
			// get meters/pixel resolution for zoomlevel
			return (extents.Width / tileSize) / numTiles;
		}
		
		public System.Drawing.Rectangle ClipRectangleAtZoomLevel(Rectangle rectangle, int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			// calculate number of tiles across
			int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
			
			// get meters/pixel resolution for zoomlevel
			double resolution = CalculateMapUnitsPerPixel(zoomLevel);
			
			// - first translate merc points to origin so they will all be positive
			// - then convert to pixel coordinates 
			//   (for google, Y pixel value must be reversed b/c google's origin is top-left, ours is bottom-right
			// - divide by tile size to figure out which tile we're in

			int minx = Convert.ToInt32(Math.Floor((rectangle.Min.X - extents.Min.X) / (resolution * tileSize)));
			int miny, maxy;
			
			if (consumer == TileConsumer.GoogleMaps)
			{
				miny = Convert.ToInt32(Math.Floor((tileSize*numTiles - 
				                                       ((rectangle.Max.Y - extents.Min.Y) / resolution)) / tileSize));
			}
			else
			{
				miny = Convert.ToInt32(Math.Floor((rectangle.Min.Y - extents.Min.Y) / (resolution * tileSize)));
			}
			
			int maxx = Convert.ToInt32(Math.Floor((rectangle.Max.X - extents.Min.X) / (resolution * tileSize)));
			
			if (consumer == TileConsumer.GoogleMaps)
			{
				maxy = Convert.ToInt32(Math.Floor((tileSize*numTiles - 
				                                       ((rectangle.Min.Y - extents.Min.Y) / resolution)) / tileSize));
			}
			else
			{
				maxy = Convert.ToInt32(Math.Floor((rectangle.Max.Y - extents.Min.Y) / (resolution * tileSize)));
			}
			
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
				double resolution = CalculateMapUnitsPerPixel(zoomLevel);

				// google tiles origin is top left
				int tileymin, tileymax;
				if (consumer == TileConsumer.GoogleMaps)
				{
					tileymin = numTiles - y - 1;
					tileymax = numTiles - y;
				}
				else
				{
					tileymin = y;
					tileymax = y + 1;
				}	
				
				// convert pixels to meters and translate to origin
				map.Extents = new Rectangle((tileSize*x) * resolution + extents.Min.X,
				                            (tileSize*tileymin) * resolution + extents.Min.Y, 
				                            (tileSize*(x+1)) * resolution + extents.Min.X,
				                            (tileSize*tileymax) * resolution + extents.Min.Y);
				
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
			if (zoomLevel > maxZoomLevel || zoomLevel < MinZoomLevel)
			{
				throw new ArgumentOutOfRangeException("zoomLevel", 
				                                      string.Format("Zoom level must within {0} and {1} (got {2})",
				                                                    MinZoomLevel,
				                                                    maxZoomLevel,
				                                                    zoomLevel));
			}
		}
	}
}
