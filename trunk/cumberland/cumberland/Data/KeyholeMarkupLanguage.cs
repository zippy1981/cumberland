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
//using System.Linq;
//using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text;

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

			// reproject map extents to wgs 84 for Google Earth
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
			StringWriter tw = new StringWriter();
			XmlTextWriter xtw = new XmlTextWriter(tw);

			xtw.WriteStartDocument();
			xtw.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
			xtw.WriteStartElement("Document");

			foreach (Layer l in map.Layers)
			{
				if (l.Data ==  null) continue;
				
				foreach (Style s in l.Styles)
				{
					CreateFromStyle(xtw, s, l.Data.SourceFeatureType);
				}
			}

			foreach (Layer l in map.Layers)
			{
				if (l.Data == null) continue;

				CreateFromLayer(xtw, l, extents);
			}
			
//			XDocument doc = new XDocument(new XElement("kml",
//			                                           new XElement("Document",
//			                                                        from l in map.Layers
//			                                                        where l.Data != null
//			                                                        from s in l.Styles
//			                                                        where !string.IsNullOrEmpty(s.Id)
//			                                                        select CreateFromStyle(s, l.Data.SourceFeatureType),
//			                                                        from l in map.Layers
//			                                                        where l.Data != null
//			                                                        select CreateFromLayer(l, extents))));

			xtw.WriteEndElement(); // Document
			xtw.WriteEndElement(); // kml
			xtw.WriteEndDocument();
			
			//return doc.ToString(SaveOptions.DisableFormatting);
			return tw.ToString();
		}

		#region helper methods
		
		static void CreateFromLayer(XmlTextWriter xtw, Layer layer, Rectangle extents)
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

				xtw.WriteStartElement("Folder");
				xtw.WriteElementString("name", layer.Id);

				foreach (Feature f in layer.Data.GetFeatures(queryExtents, layer.ThemeField, layer.LabelField))
				{
					CreateFromFeature(xtw, layer, f, src, dst);
				}
				
//				return new XElement("Folder",
//				                     new XElement("name", layer.Id),
//				                     from f in layer.Data.GetFeatures(queryExtents, layer.ThemeField, layer.LabelField)
//				                     select CreateFromFeature(layer, f, src, dst));

				xtw.WriteEndElement(); // Folder
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

		static void CreateFromStyle(XmlTextWriter xtw, Style style, FeatureType featureType)
		{
			if (string.IsNullOrEmpty(style.Id)) return null;

			xtw.WriteStartElement("Style");
			xtw.WriteElementString("id", style.Id);

			if (featureType == FeatureType.Polygon)
			{
				
//				return new XElement("Style",
//				                    new XAttribute("id", style.Id),
//				                    new XElement("LineStyle",
//				                                 new XElement("width", (style.LineStyle == LineStyle.None ? 0 : style.LineWidth)),
//				                                 new XElement("color", ConvertToKmlColor(style.LineColor))),
//				                    new XElement("PolyStyle",
//				                                 new XElement("color", (style.FillStyle == FillStyle.None ? "00ffffff" : ConvertToKmlColor(style.FillColor)))));

				xtw.WriteStartElement("PolyStyle");
				xtw.WriteElementString("color", (style.FillStyle == FillStyle.None ? "00ffffff" : ConvertToKmlColor(style.FillColor)));
				xtw.WriteEndElement(); // PolyStyle
				                                              
			}
			
			if (featureType == FeatureType.Polyline || featureType == FeatureType.Polygon)
			{
//				return new XElement("Style",
//				                    new XAttribute("id", style.Id),
//				                    new XElement("LineStyle",
//				                                 new XElement("width", (style.LineStyle == LineStyle.None ? 0 : style.LineWidth)),
//				                                 new XElement("color", ConvertToKmlColor(style.LineColor))));

				xtw.WriteStartElement("LineStyle");
				xtw.WriteElementString("width", (style.LineStyle == LineStyle.None ? 0 : style.LineWidth));
				xtw.WriteElementString("color", ConvertToKmlColor(style.LineColor));
				xtw.WriteEndElement(); // LineStyle
			}
			else if (featureType == FeatureType.Point)
			{
//				return new XElement("Style",
//				                    new XAttribute("id", style.Id),
//				                    new XElement("IconStyle",
//				                                 (style.PointSymbol == PointSymbolType.Image ? new XElement("Icon", new XElement("href", style.PointSymbolImagePath)) : null),
//				                                 (style.PointSymbol == PointSymbolType.Shape ? new XElement("color", ConvertToKmlColor(style.LineColor)) : null)));

				xtw.WriteStartElement("IconStyle");

				if (style.PointSymbol == PointSymbolType.Image)
				{
					xtw.WriteStartElement("Icon");
					xtw.WriteElementString("href", style.PointSymbolImagePath);
					xtw.WriteEndElement(); // Icon
				}
				else if (style.PointSymbol == PointSymbolType.Shape)
				{
					xtw.WriteElementString("color", ConvertToKmlColor(style.LineColor));
				}

				xtw.WriteEndElement(); // IconStyle
			}

			xtw.WriteEndElement(); // Style
		}

		static string ConvertToKmlColor(Color color)
		{
			return (String.Format("{0:X2}", color.A) +
					String.Format("{0:X2}", color.B) +
					String.Format("{0:X2}", color.G) +
					String.Format("{0:X2}", color.R)).ToLower();
		}
		
		static void CreateFromFeature(XmlTextWriter xtw,
		                              Layer layer,
		                              Feature feature,
		                              ProjFourWrapper source,
		                              ProjFourWrapper destination)
		{
			//List<XElement> xe = new List<XElement>();

			Style style = layer.GetStyleForFeature(feature.ThemeFieldValue);

			string name = !string.IsNullOrEmpty(feature.LabelFieldValue) ? 
				feature.LabelFieldValue : 
					!string.IsNullOrEmpty(feature.ThemeFieldValue) ?
					feature.ThemeFieldValue : null;

			bool transform = destination != null;
			
			if (layer.Data.SourceFeatureType == FeatureType.Point)
			{
				Point pt = feature as Point;

				if (transform)
				{
					pt = source.Transform(destination, pt);
				}
				
//				xe.Add(new XElement("Placemark", 
//				                    (style != null && !string.IsNullOrEmpty(style.Id) ? new XElement("styleUrl", "#" + style.Id) : null),
//				                    (name != null ? new XElement("name", name) : null),
//				                    new XElement("Point",
//				                                 new XElement("coordinates",
//				                                              string.Format("{0},{1}", pt.X, pt.Y)))));
				xtw.WriteStartElement("Placemark");
				
				if (style != null && !string.IsNullOrEmpty(style.Id))
				{
					xtw.WriteElementString("styleUrl", "#" + style.Id);
				}
	
				if (name != null)
				{
					xtw.WriteElementString("name", name);
				}
				
				xtw.WriteStartElement("Point");
				xtw.WriteElementString("coordinates", string.Format("{0},{1}", pt.X, pt.Y));
				xtw.WriteEndElement(); // Point

				xtw.WriteEndElement(); // Placemark
			}
			else if (layer.Data.SourceFeatureType == FeatureType.Polyline)
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

					xtw.WriteStartElement("Placemark");
				
					if (style != null && !string.IsNullOrEmpty(style.Id))
					{
						xtw.WriteElementString("styleUrl", "#" + style.Id);
					}
		
					if (name != null)
					{
						xtw.WriteElementString("name", name);
					}

					xtw.WriteElementString("tessellate", "1");

					StringBuilder sb = new StringBuilder();

					foreach (Point pt in l.Points)
					{
						Point tpt = pt;
						if (transform)
						{
							tpt = source.Transform(destination, pt);
						}

						sb.AppendFormat("{0},{1} ", pt.X, pt.Y);
					}
					
					xtw.WriteElementString("coordinates", sb.ToString());
					
					xtw.WriteEndElement(); // Placemark
				}
			}
			else if (layer.Data.SourceFeatureType == FeatureType.Polygon)
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
		}
	
		#endregion
	}
}
