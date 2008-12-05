// KeyholeMarkupLanguage.cs
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
using System.Linq;
using System.Xml.Linq;

using Cumberland;
using Cumberland.Projection;

namespace Cumberland.Data
{
	public static class KeyholeMarkupLanguage
	{
		
		public static string CreateFromMap(Map map)
		{
			if (map == null)
			{
				throw new ArgumentNullException("map");			
			}

			Rectangle extents = map.Extents.Clone();
			
			if (!string.IsNullOrEmpty(map.Projection))
			{
				using (ProjFourWrapper src = new ProjFourWrapper(map.Projection))
				{
					using (ProjFourWrapper dst = new ProjFourWrapper(ProjFourWrapper.WGS84))
					{
						extents = new Rectangle(src.Transform(dst, extents.Min),
							                        src.Transform(dst, extents.Max));
					}
				}
			}
			
			//XNamespace ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
			
			XDocument doc = new XDocument(new XElement("kml",
			                                           new XElement("Document",
			                                                        from l in map.Layers
			                                                        where l.Data != null
			                                                        from s in l.Styles
			                                                        where !string.IsNullOrEmpty(s.Id)
			                                                        select CreateFromStyle(s, l.Data.SourceFeatureType),
			                                                        from l in map.Layers
			                                                        where l.Data != null
			                                                        select CreateFromLayer(l, extents))));

			return doc.ToString(SaveOptions.DisableFormatting);
			
		}

		#region extension methods

		static List<Point> Transform(this List<Point> pts, ProjFourWrapper source, ProjFourWrapper destination)
		{
			if (source != null && destination != null)
			{
				for (int ii=0; ii<pts.Count; ii++)
				{
					pts[ii] = source.Transform(destination, pts[ii]);
				}
			}
			
			return pts;
		}

		#endregion

		#region helper methods
		
		static XElement CreateFromLayer(Layer layer, Rectangle extents)
		{
			ProjFourWrapper src = null;
			ProjFourWrapper dst = null;
			Rectangle queryExtents = extents.Clone();

			try
			{
			
				if (!string.IsNullOrEmpty(layer.Projection) &&
				    layer.Projection != ProjFourWrapper.WGS84)
				{
					src = new ProjFourWrapper(layer.Projection);
					dst = new ProjFourWrapper(ProjFourWrapper.WGS84);

					queryExtents = new Rectangle(dst.Transform(src, queryExtents.Min),
							                        dst.Transform(src, queryExtents.Max));
				}
				
				return new XElement("Folder",
				                     new XElement("name", layer.Id),
				                     from f in layer.Data.GetFeatures(queryExtents, layer.ThemeField, layer.LabelField)
				                     select CreateFromFeature(layer, f, src, dst));
			}
			finally
			{
				if (src != null) 
				{
					src.Dispose();
					dst.Dispose();
				}
			}
		}

		static XElement CreateFromStyle(Style style, FeatureType featureType)
		{
			if (string.IsNullOrEmpty(style.Id)) return null;
			
			if (featureType == FeatureType.Polygon)
			{
				return new XElement("Style",
				                    new XAttribute("id", style.Id),
				                    new XElement("LineStyle",
				                                 new XElement("width", (style.LineStyle == LineStyle.None ? 0 : style.LineWidth)),
				                                 new XElement("color", ConvertToKmlColor(style.LineColor))),
				                    new XElement("PolyStyle",
				                                 new XElement("color", (style.FillStyle == FillStyle.None ? "00ffffff" : ConvertToKmlColor(style.FillColor)))));
				                                              
			}
			else if (featureType == FeatureType.Polyline)
			{
				return new XElement("Style",
				                    new XAttribute("id", style.Id),
				                    new XElement("LineStyle",
				                                 new XElement("width", (style.LineStyle == LineStyle.None ? 0 : style.LineWidth)),
				                                 new XElement("color", ConvertToKmlColor(style.LineColor))));
			}
			else
			{
				return new XElement("Style",
				                    new XAttribute("id", style.Id),
				                    new XElement("IconStyle",
				                                 (style.PointSymbol == PointSymbolType.Image ? new XElement("Icon", new XElement("href", style.PointSymbolImagePath)) : null),
				                                 (style.PointSymbol == PointSymbolType.Shape ? new XElement("color", ConvertToKmlColor(style.LineColor)) : null)));				
			}
		}

		static string ConvertToKmlColor(Color color)
		{
			return (String.Format("{0:X2}", color.A) +
					String.Format("{0:X2}", color.B) +
					String.Format("{0:X2}", color.G) +
					String.Format("{0:X2}", color.R)).ToLower();
		}
		
		static List<XElement> CreateFromFeature(Layer layer,
		                                        Feature feature,
		                                        ProjFourWrapper source, 
		                                        ProjFourWrapper destination)
		{
			List<XElement> xe = new List<XElement>();

			Style style = layer.GetStyleForFeature(feature.ThemeFieldValue);

			if (style == null) return null;

			string name = !string.IsNullOrEmpty(feature.LabelFieldValue) ? 
				feature.LabelFieldValue : 
					!string.IsNullOrEmpty(feature.ThemeFieldValue) ?
					feature.ThemeFieldValue : null;

			if (feature is Point)
			{
				Point pt = feature as Point;

				if (destination != null)
				{
					pt = source.Transform(destination, pt);
				}
				
				xe.Add(new XElement("Placemark", 
				                    (style != null && !string.IsNullOrEmpty(style.Id) ? new XElement("styleUrl", "#" + style.Id) : null),
				                    (name != null ? new XElement("name", name) : null),
				                    new XElement("Point",
				                                 new XElement("coordinates",
				                                              string.Format("{0},{1}", pt.X, pt.Y)))));
			}
			else if (feature is PolyLine)
			{
				PolyLine pl = feature as PolyLine;

              	foreach (Line l in pl.Lines)
				{
					xe.Add(new XElement("Placemark",
					                    (style != null && !string.IsNullOrEmpty(style.Id) ? new XElement("styleUrl", "#" + style.Id) : null),
					                    (name != null ? new XElement("name", name) : null),
					                    new XElement("LineString",
					                                 new XElement("tessellate", "1"),
					                                 new XElement("coordinates",
					                                              from pt in l.Points.Transform(source, destination)
					                                              select string.Format("{0},{1} ", pt.X, pt.Y)))));
				}
			}
			else if (feature is Polygon)
			{
				Polygon pg = feature as Polygon;

				XElement node = null;
				
				foreach (Ring r in pg.Rings)
				{
					if (r.IsClockwise)
					{
						if (node != null)
						{
							xe.Add(node);
						}
						
						node = new XElement("Placemark",
						                    (style != null && !string.IsNullOrEmpty(style.Id) ? new XElement("styleUrl", "#" + style.Id) : null),
						                    (name != null ? new XElement("name", name) : null),
						                    new XElement("Polygon",
						                                 new XElement("outerBoundaryIs",
						                                              new XElement("LinearRing",
						                                                           new XElement("tessellate", "1"),
						                                                           new XElement("coordinates",
						                                                                        from pt in r.Points.Transform(source, destination)
						                                                                        select string.Format("{0},{1} ", pt.X, pt.Y) )))));
					}
					else
					{
						node.Element("Polygon").Add(new XElement("innerBoundaryIs",
						                                          new XElement("LinearRing",				                                 
						                                                      new XElement("tessellate", "1"),
						                                                      new XElement("coordinates",
						                                                                   from pt in r.Points.Transform(source, destination)
						                                                                   select string.Format("{0},{1} ", pt.X, pt.Y) ))));
					}
				}

				xe.Add(node);
			}

			return xe;
		}
	
		#endregion
	}
}
