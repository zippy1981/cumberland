// MapDrawer.cs
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
using System.Drawing.Drawing2D;

using Cumberland;
using Cumberland.Data;
using Cumberland.Projection;


namespace Cumberland.Drawing
{
	public class MapDrawer : IMapDrawer
	{
		
		delegate void DrawPoint(Style s, Graphics g, System.Drawing.Point p);
		
#region Properties
		
		SmoothingMode smoothing = SmoothingMode.HighQuality;
		
		public SmoothingMode Smoothing {
			get {
				return smoothing;
			}
			set {
				smoothing = value;
			}
		}
		
#endregion		

#region IMapDrawer methods
		
		public Bitmap Draw (Map map)
		{
			ProjFourWrapper dst = null;
			
			
			Graphics g = null;
			
			try
			{
				Bitmap b = new Bitmap(map.Width, map.Height);
				g = Graphics.FromImage(b);
				
				// set antialiasing mode
				g.SmoothingMode = Smoothing;
				
				// we need to convet map points to pixel points 
				// so let's do as much of this at once:
				// calculate map rectangle based on image width/height
				Rectangle envelope = map.Extents.Clone();
				// set aspect ratio to image
				envelope.AspectRatioOfWidth = map.Width / map.Height;
				// get the scale
				double scale = envelope.Width / map.Width;
			
				// instantiate map projection
				if (!string.IsNullOrEmpty(map.Projection))
				{
					dst = new ProjFourWrapper(map.Projection);
				}
								
				int idx = -1;
				foreach (Layer layer in map.Layers)
				{
					idx++;
					
					if (layer.Data == null)
					{
						continue;
					}
					
					ProjFourWrapper src = null;
									
					try
					{
						Rectangle extents = map.Extents.Clone();
						
						// instantiate layer projection
						bool reproject = false;
						if (dst != null && !string.IsNullOrEmpty(layer.Projection))
						{
							src = new ProjFourWrapper(layer.Projection);
							reproject = true;
							
							// reproject extents to our source for querying
							extents = new Rectangle(dst.Transform(src, extents.Min),
							                        dst.Transform(src, extents.Max));
						}

						// query our data
						List<Feature> features = layer.Data.GetFeatures(extents);
						
						if (features.Count == 0)
						{
							continue;
						}
					
						if (layer.Styles.Count == 0)
						{
							throw new MapConfigurationException("Layer lacks a Style");
						}
					
						Style style = layer.Styles[0];
						
						if (layer.Data.SourceFeatureType == FeatureType.Point)
					    {	

		#region handle point rendering
					
							DrawPoint drawPoint = null;
					
							if (style.PointSymbol == PointSymbolType.Shape)
							{
								if (style.PointSymbolShape == PointSymbolShapeType.Square)
								{
									drawPoint = DrawSquarePoint;
								}
								else if (style.PointSymbolShape == PointSymbolShapeType.Circle)
								{
									drawPoint = DrawCirclePoint;
								}
								else continue;
							}
							else if (style.PointSymbol == PointSymbolType.Image)
							{
								if (string.IsNullOrEmpty(style.PointSymbolImagePath))
								{
									throw new MapConfigurationException("PointSymbolImagePath cannot be empty for PointSymbolType.Image");
								}
								    
								drawPoint = DrawImageOnPoint;
							}
							else continue;
							
							for (int ii=0; ii < features.Count; ii++)
							{
								Point p = features[ii] as Point;
									
								if (reproject)
								{
									// convert our data point to the map projection
									p = src.Transform(dst, p);
								}
								
								// convert our map projected point to a pixel point
								System.Drawing.Point pp = ConvertMapToPixel(envelope, scale, p);

								drawPoint(style, g, pp);
							}

	
		#endregion
						}
						else if (layer.Data.SourceFeatureType == FeatureType.Polyline)
						{
		#region Handle line rendering
							
							if (style.LineStyle == LineStyle.None)
							{
								continue;
							}
							
						    for (int ii=0; ii < features.Count; ii++)
							{
								PolyLine pol = (PolyLine) features[ii];
								for (int jj=0; jj < pol.Lines.Count; jj++)
								{
									Line r = pol.Lines[jj] as Line;
								
									System.Drawing.Point[] ppts = new System.Drawing.Point[r.Points.Count];
									
								    for (int kk = 0; kk < r.Points.Count; kk++)
									{	
										Point p = r.Points[kk];
										
										if (reproject)
										{
											p = src.Transform(dst, p);
										}
										
										ppts[kk] = ConvertMapToPixel(envelope, scale, p);
									}
								
									g.DrawLines(ConvertLayerToPen(style), ppts);

								}
							}
		#endregion
						}	
						else if (layer.Data.SourceFeatureType == FeatureType.Polygon)
						{
#region polygon rendering
							for (int ii=0; ii < features.Count; ii++)
							{
								Polygon po = features[ii] as Polygon;		
									
								for (int jj = 0; jj < po.Rings.Count; jj++)
							    {
									Ring r = po.Rings[jj];
									
									System.Drawing.Point[] ppts = new System.Drawing.Point[r.Points.Count];
									
									//TODO: Support holes!
									
									for (int kk = 0; kk < r.Points.Count; kk++)
									{
										Point p = r.Points[kk];
										
										if (reproject)
										{
											p = src.Transform(dst, p);
										}
										
										ppts[kk] = ConvertMapToPixel(envelope, scale, p);

									}	
									
									g.FillPolygon(new SolidBrush(style.FillColor), ppts);
									
									if (style.LineStyle != LineStyle.None)
									{
										g.DrawPolygon(ConvertLayerToPen(style), ppts);
									}
								}
							}
#endregion
						}
					}
					finally
					{
						// dispose of proj4
						if (src != null)
						{
							src.Dispose();
						}
					}
					
				}
				
				return b;
			}
			finally
			{
				// dispose of proj4
				if (dst != null)
				{
					dst.Dispose();
				}
				
				if (g != null)
				{
					g.Dispose();
				}
			}
		}
		
#endregion

#region helper methods
		
		void DrawSquarePoint(Style style, Graphics g, System.Drawing.Point pp)
		{
			g.FillRectangle(new SolidBrush(style.FillColor), 
			                pp.X - (style.PointSize/2),
			                pp.Y - (style.PointSize/2),
			                style.PointSize,
			                style.PointSize);	
		}
		
		void DrawCirclePoint(Style style, Graphics g, System.Drawing.Point pp)
		{
			g.FillEllipse(new SolidBrush(style.FillColor),
			                pp.X - (style.PointSize/2),
			                pp.Y - (style.PointSize/2),
			                style.PointSize,
			                style.PointSize);	
		}
		
		void DrawImageOnPoint(Style style, Graphics g, System.Drawing.Point pp)
		{
			Bitmap b = new Bitmap(style.PointSymbolImagePath);
			
			g.DrawImageUnscaled(b,
			                    pp.X - b.Width/2,
			                    pp.Y - b.Height/2);
		}
		
		System.Drawing.Point ConvertMapToPixel(Rectangle r, double scale, Point p)
		{
			return new System.Drawing.Point(Convert.ToInt32((p.X - r.Min.X) / scale),
			                                Convert.ToInt32((r.Max.Y - p.Y) / scale)); // image origin is top left	
		}
		
		Pen ConvertLayerToPen(Style style)
		{
			Pen p = new Pen(style.LineColor, style.LineWidth);
			
			if (style.LineStyle == LineStyle.Dashed)
			{
				p.DashStyle = DashStyle.Dash;
			}
			else if (style.LineStyle == LineStyle.Dotted)
			{
				p.DashStyle = DashStyle.Dot;
			}
				
			return p;
		}

#endregion
	}
}
