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
using System.Text;

using Cumberland;
using Cumberland.Drawing;
using Cumberland.Projection;

namespace Cumberland.Web
{
	public enum TileConsumer
	{
		None,
		GoogleMaps,
		TileMapService,
		VirtualEarth
	}
	
	public class TileProvider
	{
		
		#region vars
		
		TileConsumer consumer = TileConsumer.GoogleMaps;
		int tileSize = 256;
		bool drawExceptionsOnTile = true;
		int maxZoomLevel = 19;
		int minZoomLevel = 0;
		Rectangle worldExtents = new Rectangle();
//		Map map;
		int bleedInPixels = 0;
		
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
					throw new InvalidOperationException("Changing the maximum zoom level is not permitted for this tile consumer");
				}

				maxZoomLevel = value;
			}
		}

		public int BleedInPixels {
			get {
				return bleedInPixels;
			}
			set {
				if (value < 0)
				{
					throw new ArgumentException("BleedInPixels cannot be less than zero");
				}
				
				bleedInPixels = value;
			}
		}

		#endregion
		
		#region ctors

		[Obsolete("A map is no longer accepted in the constructor", true)]
		public TileProvider(Map map) : this(TileConsumer.GoogleMaps)
		{
		}
		
		public TileProvider(TileConsumer consumer) : this(consumer, 
		                                                  (consumer == TileConsumer.TileMapService ? Rectangle.GeographicWorldExtents : null))
		{
		}
		
		public TileProvider(TileConsumer consumer, Rectangle worldExtents)
		{
			if (consumer == TileConsumer.None)
			{
				throw new ArgumentException("'None' is not a valid consumer.", "consumer");
			}
			
			this.consumer = consumer;
			
			if (consumer == TileConsumer.GoogleMaps || consumer == TileConsumer.VirtualEarth)
			{
				if (worldExtents != null)
				{
					throw new ArgumentException("World extents for this tile consumer is fixed and cannot be set (must be null)", "worldExtents");
				}

				if (consumer == TileConsumer.VirtualEarth)
				{
					minZoomLevel = 1;
					maxZoomLevel = 23;
				}
				
				// get projection to grab circumference
				SphericalMercatorProjector prj = new SphericalMercatorProjector();
				
				// set mercator origin (center of the map (0,0))
				double origin = prj.Circumference/2;
				
				this.worldExtents = new Rectangle(-origin, -origin, origin, origin);
			}
			else if (consumer == TileConsumer.TileMapService)
			{
				this.worldExtents = worldExtents;
			}
		}
		
		#endregion

		#region public methods

		[Obsolete("Drawing a tile now requires a map", true)]
		public Bitmap DrawTile(int x, int y, int zoomLevel)
		{
			throw new InvalidOperationException("drawing a tile now requires a map");
		}

		
		public double CalculateMapUnitsPerPixel(int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			// calculate number of tiles across
			int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
			
			// get meters/pixel resolution for zoomlevel
			return (worldExtents.Width / tileSize) / numTiles;
		}
		
		public System.Drawing.Rectangle ClipRectangleAtZoomLevel(Rectangle rectangle, int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			// calculate number of tiles across
			int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
			
			// get meters/pixel resolution for zoomlevel
			double resolution = CalculateMapUnitsPerPixel(zoomLevel);

			// restrict tile calculation to within world extents
			Rectangle drawableExtents = Rectangle.Intersect(worldExtents, rectangle);
			
			// - first translate merc points to origin so they will all be positive
			// - then convert to pixel coordinates 
			//   (for google, Y pixel value must be reversed b/c google's origin is top-left, ours is bottom-right
			// - divide by tile size to figure out which tile we're in

			int minx = Convert.ToInt32(Math.Floor((drawableExtents.Min.X - worldExtents.Min.X) / (resolution * tileSize)));
			int miny, maxy;
			
			if (consumer == TileConsumer.GoogleMaps || consumer == TileConsumer.VirtualEarth)
			{
				miny = Convert.ToInt32(Math.Floor((tileSize*numTiles - 
				                                       ((drawableExtents.Max.Y - worldExtents.Min.Y) / resolution)) / tileSize));
			}
			else
			{
				miny = Convert.ToInt32(Math.Floor((drawableExtents.Min.Y - worldExtents.Min.Y) / (resolution * tileSize)));
			}
			
			int maxx = Convert.ToInt32(Math.Floor((drawableExtents.Max.X - worldExtents.Min.X) / (resolution * tileSize)));
			
			if (consumer == TileConsumer.GoogleMaps || consumer == TileConsumer.VirtualEarth)
			{
				maxy = Convert.ToInt32(Math.Floor((tileSize*numTiles - 
				                                       ((drawableExtents.Min.Y - worldExtents.Min.Y) / resolution)) / tileSize));
			}
			else
			{
				maxy = Convert.ToInt32(Math.Floor((drawableExtents.Max.Y - worldExtents.Min.Y) / (resolution * tileSize)));
			}
			
			return new System.Drawing.Rectangle(minx, miny, (maxx-minx), (maxy-miny));
		}
		
		public int CalculateNumberOfTilesAcross(int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
			return Convert.ToInt32(Math.Pow(2, zoomLevel));
		}

		public Bitmap DrawTile(Map map, int x, int y, int zoomLevel)
		{
			Bitmap b;
			string prj = null;
										
			if (consumer == TileConsumer.GoogleMaps || consumer == TileConsumer.VirtualEarth)
			{
				// reproject to spherical mercator
				prj = ProjFourWrapper.SphericalMercatorProjection;
			}
			
			try
			{
				CheckZoomLevel(zoomLevel);
				
				// calculate number of tiles across
				int numTiles = CalculateNumberOfTilesAcross(zoomLevel);
				
				// get map units/pixel resolution for zoomlevel
				double resolution = CalculateMapUnitsPerPixel(zoomLevel);

				// set to tile size
				int adjTileSize = tileSize + (bleedInPixels * 2);

				// google tiles origin is top left
				int tileymin, tileymax;
				if (consumer == TileConsumer.GoogleMaps || consumer == TileConsumer.VirtualEarth)
				{
					tileymin = numTiles - y - 1;
					tileymax = numTiles - y;
				}
				else
				{
					tileymin = y;
					tileymax = y + 1;
				}	

				double mapUnitsOffset = bleedInPixels * resolution;
				
				// - convert pixels to map units 
				// - translate to origin
				// - offset if we need to draw a bleed
				Rectangle extents = new Rectangle((tileSize*x) * resolution + worldExtents.Min.X - (mapUnitsOffset),
				                            (tileSize*tileymin) * resolution + worldExtents.Min.Y - (mapUnitsOffset), 
				                            (tileSize*(x+1)) * resolution + worldExtents.Min.X + (mapUnitsOffset),
				                            (tileSize*tileymax) * resolution + worldExtents.Min.Y + (mapUnitsOffset));
				
				MapDrawer renderer = new MapDrawer();
			
				// draw our map
				b = renderer.Draw(map,
				                  extents,
				                  prj,
				                  adjTileSize,
				                  adjTileSize);

				if (bleedInPixels > 0)
				{
					// crop to get our tile
					using (Bitmap old = b)
					{
						b = old.Clone(new System.Drawing.Rectangle(bleedInPixels, 
						                      bleedInPixels, 
						                      tileSize,
						                      tileSize),
						        System.Drawing.Imaging.PixelFormat.DontCare);
					}
				}
				
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
						msg = msg.Insert(ii*numCharsPerRow, Environment.NewLine);
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

		public string ConvertTileToQuadKey(int x, int y, int zoomLevel)
		{
			CheckZoomLevel(zoomLevel);
			
            StringBuilder quadKey = new StringBuilder();
			
            for (int i = zoomLevel; i >= minZoomLevel; i--)
            {
                char digit = '0';
				
                int mask = 1 << (i - 1);
				
                if ((x & mask) != 0)
                {
                    digit++;
                }
                if ((y & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
			
            return quadKey.ToString();
		}
		
		#endregion

		#region helper methods
		
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

		#endregion
	}
}
