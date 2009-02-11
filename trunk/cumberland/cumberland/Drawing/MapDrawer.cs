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
using System.Drawing.Text;

using Cumberland;
using Cumberland.Data;
using Cumberland.Projection;


namespace Cumberland.Drawing
{
	public class MapDrawer : IMapDrawer
	{
		#region inner classes
		
		sealed class LabelRequest
		{
			public Style Style;
			public string Label;
			public System.Drawing.Point Coord;
			public float ForcedLabelAngle;

			public LabelRequest(Style s, string l, System.Drawing.Point c)
			{
				Style = s;
				Label = l;
				Coord = c;
				ForcedLabelAngle = s.LabelAngle;
			}

			public LabelRequest(Style s, string l, System.Drawing.Point c, float a) : this(s,l,c)
			{
				ForcedLabelAngle = a;
			}
		}

		#endregion

		#region vars
		
		List<LabelRequest> labels = new List<LabelRequest>();
		SmoothingMode smoothing = SmoothingMode.HighQuality;
		TextRenderingHint textRenderingHint = TextRenderingHint.AntiAlias;

		#endregion
		
		delegate void PointDrawer(Style s, Graphics g, System.Drawing.Point p);
		
		delegate Style StyleForFeatureFinder(Layer l, string fieldValue, double scale);
		
#region Properties
		
		
		
		public SmoothingMode Smoothing {
			get {
				return smoothing;
			}
			set {
				smoothing = value;
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
				return textRenderingHint;
			}
			set {
				textRenderingHint = value;
			}
		}
		
#endregion		

#region IMapDrawer methods

		public Bitmap Draw (Map map)
		{
			return Draw(map, map.Extents, map.Projection, map.Width, map.Height);
		}
		
		public Bitmap Draw (Map map, Cumberland.Rectangle extents, string projection, int width, int height)
		{			
			ProjFourWrapper dst = null;
			Graphics g = null;

			try
			{
				#region setup drawing
				
				Bitmap b = new Bitmap(width, height);
				g = Graphics.FromImage(b);

				if (map.BackgroundColor.A > 0)
				{
					g.Clear(map.BackgroundColor);
				}
				
				// set antialiasing mode
				g.SmoothingMode = Smoothing;
				g.TextRenderingHint = textRenderingHint;
				
				// we need to convert map points to pixel points 
				// so let's do as much of this at once:
				// calculate map rectangle based on image width/height
				Rectangle envelope = extents.Clone();
				// set aspect ratio to image
				envelope.AspectRatioOfWidth = width / height;
				// get the scale
				double scale = envelope.Width / width;
				double displayScale = scale;
			
				// instantiate output projection
				if (!string.IsNullOrEmpty(projection))
				{
					dst = new ProjFourWrapper(projection);

					if (!string.IsNullOrEmpty(map.Projection) &&
					    projection != map.Projection)
					{
						// we want to use the map projection for min/max scale on layers/styles/etc.
						using (ProjFourWrapper orig = new ProjFourWrapper(map.Projection))
						{
							Point p1 = dst.Transform(orig, extents.Min);
							Point p2 = dst.Transform(orig, extents.Max);
							
							displayScale = Math.Abs(p1.X-p2.X) / width;
						}
					}
				}
				else if (!string.IsNullOrEmpty(map.Projection))
				{
					dst = new ProjFourWrapper(map.Projection);
				}
				
				#endregion

				int idx = -1;
				foreach (Layer layer in map.Layers)
				{
					idx++;

					if (!layer.Visible || 
					    layer.Data == null ||
					    displayScale > layer.MaxScale ||
					    displayScale < layer.MinScale)
					{
						continue;
					}
					
					ProjFourWrapper src = null;
									
					try
					{
						#region set up layer drawing
						
						Rectangle layerEnvelope = envelope.Clone();
						
						// instantiate layer projection
						bool reproject = false;
						if (dst != null && !string.IsNullOrEmpty(layer.Projection))
						{
							src = new ProjFourWrapper(layer.Projection);
							reproject = true;
														
							// reproject extents to our source for querying
							layerEnvelope = new Rectangle(dst.Transform(src, layerEnvelope.Min),
							                        dst.Transform(src, layerEnvelope.Max));
						}

						string themeField = null;
						
						// query our data
						List<Feature> features = null;
						if (layer.Theme == ThemeType.None)
						{
							features = layer.Data.GetFeatures(layerEnvelope, null, layer.LabelField);
						}
						else
						{
							// check for valid field
							themeField = layer.ThemeField;
							if (string.IsNullOrEmpty(themeField))
							{
								throw new MapConfigurationException("Layer has been set for theming, but no ThemeField was provided");
							}
							
							features = layer.Data.GetFeatures(layerEnvelope, themeField, layer.LabelField);
						}
						
						if (features.Count == 0)
						{
							continue;
						}

						if (layer.Styles.Count == 0)
						{
							throw new MapConfigurationException("Layer lacks a Style");
						}

						// set up a delegate for getting style based on theme type
						StyleForFeatureFinder getStyle = GetBasicStyleForFeature;
						
						if (layer.Theme == ThemeType.Unique)
						{
							getStyle = GetUniqueStyleForFeature;
						}
						else if (layer.Theme == ThemeType.NumericRange)
						{
							getStyle = GetRangeStyleForFeature;
						}

						#endregion
						
						if (layer.Data.SourceFeatureType == FeatureType.Point)
					    {	
							#region handle point rendering
					
							for (int ii=0; ii < features.Count; ii++)
							{
								Point p = features[ii] as Point;
								
								Style style = getStyle(layer, p.ThemeFieldValue, displayScale);
								
								if (style == null)
								{
									continue;
								}
								
								PointDrawer drawPoint = GetPointDrawer(style);
								
								if (reproject)
								{
									// convert our data point to the map projection
									p = src.Transform(dst, p);
								}
								
								// convert our map projected point to a pixel point
								System.Drawing.Point pp = ConvertMapToPixel(envelope, scale, p);

								drawPoint(style, g, pp);

								if (style.ShowLabels && 
								    displayScale <= style.LabelMaxScale &&
								    displayScale >= style.LabelMinScale)
								{
									labels.Add(new LabelRequest(style, p.LabelFieldValue, pp));
								}
							}

							#endregion
						}
						else if (layer.Data.SourceFeatureType == FeatureType.Polyline)
						{
							#region Handle line rendering
							
//							if (layer.Theme == ThemeType.None &&
//							    getStyle(layer, null, displayScale).LineStyle == LineStyle.None)
//							{
//								// skip the layer
//								continue;
//							}
							
						    for (int ii=0; ii < features.Count; ii++)
							{
								PolyLine pol = (PolyLine) features[ii];
								
								Style style = getStyle(layer, pol.ThemeFieldValue, displayScale);
								
								if (style == null || 
								    style.LineStyle == LineStyle.None ||
								    displayScale > style.MaxScale ||
								    displayScale < style.MinScale)
								{
									continue;
								}
								
								for (int jj=0; jj < pol.Lines.Count; jj++)
								{
									Line r = pol.Lines[jj];
								
									if (style.Simplify)
									{
										r = r.Simplify(style.SimplifyTolerance);
									}
									
									System.Drawing.Point[] ppts = new System.Drawing.Point[r.Points.Count];

									int segmentIdx = 1;
									double maxSegment = 0;
									
								    for (int kk = 0; kk < r.Points.Count; kk++)
									{	
										Point p = r.Points[kk];
										
										if (reproject)
										{
											p = src.Transform(dst, p);
										}
										
										ppts[kk] = ConvertMapToPixel(envelope, scale, p);

										if ((style.DrawPointSymbolOnPolyLine ||
										    (style.ShowLabels &&
										    scale <= style.LabelMaxScale &&
										    scale >= style.LabelMinScale)) && 
										    kk > 0)
										{
											// find the segment with the longest length to pin a label to
											
											double seg = CalculateSegmentLength(r.Points[kk-1], r.Points[kk]);
											
											if (seg > maxSegment)
											{
												maxSegment = seg;
												segmentIdx = kk;
											}
										}
									}
								
									g.DrawLines(ConvertLayerToPen(style), ppts);

									if (style.DrawPointSymbolOnPolyLine ||
									    (style.ShowLabels &&
									    pol.LabelFieldValue != null &&
									    displayScale <= style.LabelMaxScale &&
									    displayScale >= style.LabelMinScale))
									{
										Point start = r.Points[segmentIdx-1];
										Point end = r.Points[segmentIdx];
										
										// pin our label to the center point of the line
										Point polyCenter = new Point((start.X+end.X)/2, (start.Y+end.Y)/2);
										//Point polyCenter = r.CalculateBounds().Center;

										// transform (if neccessary) the convert to pixel point
										System.Drawing.Point plpt = ConvertMapToPixel(envelope,
										                                              scale,
										                                              (reproject ? src.Transform(dst, polyCenter) : polyCenter));
										
										if (style.DrawPointSymbolOnPolyLine)
										{
											GetPointDrawer(style)(style, g, plpt);
										}
										
										if (style.ShowLabels)
										{
											
											float angle = style.LabelAngle;
											
											if (style.CalculateLabelAngleForPolyLine)
											{
												// calculate the slope of this line and use as our label angle
												angle = CalculateSegmentAngle(start, end);
											}
											
											labels.Add(new LabelRequest(style,
											                            pol.LabelFieldValue,
											                            plpt,
											                            angle));
										}
									}
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
								
								Style style = getStyle(layer, po.ThemeFieldValue, displayScale);								
								
								if (style == null)
								{
									continue;
								}
								
								GraphicsPath gp = new GraphicsPath(FillMode.Alternate);
							
								for (int jj = 0; jj < po.Rings.Count; jj++)
							    {
									Ring r = po.Rings[jj];
									
									if (style.Simplify)
									{
										r = r.Simplify(style.SimplifyTolerance);
									}
									
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

									gp.AddPolygon(ppts);
								}

								
								if (style.FillStyle != FillStyle.None)
								{
									Brush fillBrush;

									if (style.FillStyle == FillStyle.Texture)
									{
										if (string.IsNullOrEmpty(style.FillTexturePath))
										{
											throw new MapConfigurationException("A style with FillStyle.Texture must have a FillTexturePath");
										}
										
										fillBrush = new TextureBrush(new Bitmap(style.FillTexturePath));
									}
									else
									{
										fillBrush = new SolidBrush(style.FillColor);										
									}
									
									g.FillPath(fillBrush, gp);
								}

								if (style.LineStyle != LineStyle.None)
								{
									g.DrawPath(ConvertLayerToPen(style), gp);
								}

								if (style.ShowLabels && 
								    po.LabelFieldValue != null && 
								    displayScale <= style.LabelMaxScale &&
								    displayScale >= style.LabelMinScale)
								{
									Point polyCenter = po.CalculateBounds().Center;
									
									labels.Add(new LabelRequest(style,
									                            po.LabelFieldValue,
									                            ConvertMapToPixel(envelope,
									                                              scale,
									                                              (reproject ? src.Transform(dst, polyCenter): polyCenter))));
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

				foreach (LabelRequest lr in labels)
				{
					DrawLabel(g, lr.Style, lr.Coord, lr.Label, lr.ForcedLabelAngle);
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

		#region get style methods
		
		static Style GetBasicStyleForFeature(Layer l, string fieldValue, double scale)
		{
			return l.GetBasicStyleForFeature(fieldValue, scale, true);
		}
		
		static Style GetUniqueStyleForFeature(Layer l, string fieldValue, double scale)
		{
			return l.GetUniqueStyleForFeature(fieldValue, scale, true);
		}
		
		static Style GetRangeStyleForFeature(Layer l, string fieldValue, double scale)
		{
			return l.GetRangeStyleForFeature(fieldValue, scale, true);
		}

		#endregion

		#region Draw Point methods

		static void DrawNoPoint(Style style, Graphics g, System.Drawing.Point pp)
		{
		}
		
		static void DrawSquarePoint(Style style, Graphics g, System.Drawing.Point pp)
		{
			System.Drawing.Rectangle r = new System.Drawing.Rectangle(
			                pp.X - (style.PointSize/2),
			                pp.Y - (style.PointSize/2),
			                style.PointSize,
			                style.PointSize);

			if (style.FillStyle != FillStyle.None)
			{
				g.FillRectangle(new SolidBrush(style.FillColor), r);
			}
			
			if (style.LineStyle != LineStyle.None)
			{
				g.DrawRectangle(ConvertLayerToPen(style), r);
			}
		}
		
		static void DrawCirclePoint(Style style, Graphics g, System.Drawing.Point pp)
		{
			System.Drawing.Rectangle r = new System.Drawing.Rectangle(pp.X - (style.PointSize/2),
			                pp.Y - (style.PointSize/2),
			                style.PointSize,
			                style.PointSize);

			if (style.FillStyle != FillStyle.None)
			{
				g.FillEllipse(new SolidBrush(style.FillColor), r);
			}
			
			if (style.LineStyle != LineStyle.None)
			{
				g.DrawEllipse(ConvertLayerToPen(style),r);	
			}
		}
		
		static void DrawImageOnPoint(Style style, Graphics g, System.Drawing.Point pp)
		{
			Bitmap b = new Bitmap(style.PointSymbolImagePath);
			
			g.DrawImageUnscaled(b,
			                    pp.X - b.Width/2,
			                    pp.Y - b.Height/2);
		}

		#endregion

		static PointDrawer GetPointDrawer(Style style)
		{
			if (style.PointSymbol == PointSymbolType.Shape)
			{
				if (style.PointSymbolShape == PointSymbolShapeType.Square)
				{
					return DrawSquarePoint;
				}
				else if (style.PointSymbolShape == PointSymbolShapeType.Circle)
				{
					return DrawCirclePoint;
				}
				else return DrawNoPoint;
			}
			else if (style.PointSymbol == PointSymbolType.Image)
			{
				if (string.IsNullOrEmpty(style.PointSymbolImagePath))
				{
					throw new MapConfigurationException("PointSymbolImagePath cannot be empty for PointSymbolType.Image");
				}
				    
				return DrawImageOnPoint;
			}
			else return DrawNoPoint;	
		}
		
		static System.Drawing.Point ConvertMapToPixel(Rectangle r, double scale, Point p)
		{
			return new System.Drawing.Point(Convert.ToInt32((p.X - r.Min.X) / scale),
			                                Convert.ToInt32((r.Max.Y - p.Y) / scale)); // image origin is top left	
		}
		
		static Pen ConvertLayerToPen(Style style)
		{
			Pen p = new Pen(style.LineColor, style.LineWidth);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;

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
		
		static void DrawLabel(Graphics g, Style s, System.Drawing.Point p, string label, float forcedLabelAngle)
		{		
			FontFamily ff = FontFamily.GenericSansSerif;
			if (s.LabelFont == LabelFont.None)
			{
				return;
			}
			if (s.LabelFont == LabelFont.Serif)
			{
				ff = FontFamily.GenericSerif;
			}
			else if (s.LabelFont == LabelFont.Monospace)
			{
				ff = FontFamily.GenericMonospace;
			}
			else if (s.LabelFont == LabelFont.Custom)
			{
				ff = new FontFamily(s.LabelCustomFont);
			}

			Font font = new Font(ff, s.LabelFontEmSize);

			System.Drawing.Point labelPt = new System.Drawing.Point(0, 0);
			
			SizeF size = g.MeasureString(label, font);
			switch (s.LabelPosition)
			{
				case LabelPosition.None:
					return;
				case LabelPosition.Center:
					labelPt.X -= Convert.ToInt32(size.Width/2);
					labelPt.Y -= Convert.ToInt32(size.Height/2);
					break;
				case LabelPosition.BottomLeft:
					labelPt.X -= Convert.ToInt32(size.Width) + s.LabelPixelOffset;
					labelPt.Y += s.LabelPixelOffset;
					break;					
				case LabelPosition.Bottom:
					labelPt.X -= Convert.ToInt32(size.Width/2);
					labelPt.Y += s.LabelPixelOffset;
					break;
				case LabelPosition.BottomRight:  // default
					labelPt.Y += s.LabelPixelOffset;					
					labelPt.X += s.LabelPixelOffset;
					break;
				case LabelPosition.Right:
					labelPt.X += s.LabelPixelOffset;
					labelPt.Y -= Convert.ToInt32(size.Height/2);
					break;
				case LabelPosition.TopRight:
					labelPt.X += s.LabelPixelOffset;
					labelPt.Y -= Convert.ToInt32(size.Height) + s.LabelPixelOffset;
					break;
				case LabelPosition.Top:
					labelPt.X -= Convert.ToInt32(size.Width/2);
					labelPt.Y -= Convert.ToInt32(size.Height) + s.LabelPixelOffset;
					break;
				case LabelPosition.TopLeft:
					labelPt.X -= Convert.ToInt32(size.Width) + s.LabelPixelOffset;
					labelPt.Y -= Convert.ToInt32(size.Height) + s.LabelPixelOffset;
					break;
				case LabelPosition.Left:
					labelPt.X -= Convert.ToInt32(size.Width) + s.LabelPixelOffset;
					labelPt.Y -= Convert.ToInt32(size.Height/2);
					break;
			}

			g.TranslateTransform(p.X, p.Y);
			g.RotateTransform(forcedLabelAngle);
			
			if (s.LabelDecoration == LabelDecoration.Outline)
			{
				StringFormat sf = new StringFormat();
				sf.Alignment = StringAlignment.Center;
				sf.LineAlignment = StringAlignment.Center;// draw the text to a path
				
				GraphicsPath gp = new GraphicsPath();
				gp.AddString(label, 
				             ff, 
				             (int)FontStyle.Bold, 
				             s.LabelFontEmSize, 
				             new RectangleF(new PointF(labelPt.X, labelPt.Y), size), 
				             sf);
				
				// HACK: to avoid GdipWidenPath not implemented in mono
				if (Type.GetType ("Mono.Runtime") != null)
				{
					g.FillPath(new SolidBrush(s.LabelColor), gp);
					g.DrawPath(new Pen(s.LabelOutlineColor, s.LabelOutlineWidth), gp);
				}
				else
				{
					// create outline path
					GraphicsPath outlinePath = (GraphicsPath)gp.Clone();
					Pen pen = new Pen(s.LabelOutlineColor, s.LabelOutlineWidth);
					outlinePath.Widen(pen);
					
					g.FillPath(new SolidBrush(s.LabelOutlineColor), outlinePath);
					g.FillPath(new SolidBrush(s.LabelColor), gp);
				}
			}
			else
			{
				g.DrawString(label, font, new SolidBrush(s.LabelColor), labelPt);
			}

			g.ResetTransform();
		}

		static double CalculateSegmentLength(Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(p2.X-p1.X, 2) + Math.Pow(p2.Y-p1.Y, 2));
		}

		static float CalculateSegmentAngle(Point p1, Point p2)
		{
			// - calculate the slope of the line
			// - calculate the arctangent of the slope to get angle
			// - convert the angle from radians to degrees
			// - flip the sign
			return -Convert.ToSingle(Math.Atan((Convert.ToSingle(p2.Y)-Convert.ToSingle(p1.Y))/
			                 (Convert.ToSingle(p2.X)-Convert.ToSingle(p1.X))) * (180/Math.PI) );
		}
		
#endregion
	}
}
