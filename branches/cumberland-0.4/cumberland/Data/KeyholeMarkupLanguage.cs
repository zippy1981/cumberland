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
using System.Xml;
using System.IO;
using System.Text;
using System.Globalization;

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

			MemoryStream ms = new MemoryStream();
            XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
			
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
			
			xtw.WriteEndElement(); // Document
			xtw.WriteEndElement(); // kml
			xtw.WriteEndDocument();
			xtw.Flush();
			
			// Now read back
            ms.Seek(0, SeekOrigin.Begin);
            TextReader tr = new StreamReader(ms);
            return tr.ReadToEnd();
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
				xtw.WriteElementString("visibility", layer.Visible ? "1" : "0");

				foreach (Feature f in layer.Data.GetFeatures(queryExtents, layer.ThemeField, layer.LabelField))
				{
					CreateFromFeature(xtw, layer, f, src, dst);
				}

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
			if (string.IsNullOrEmpty(style.Id)) return;

			xtw.WriteStartElement("Style");
			xtw.WriteAttributeString("id", style.Id);

			if (featureType == FeatureType.Polygon)
			{
				xtw.WriteStartElement("PolyStyle");
				xtw.WriteElementString("color", (style.FillStyle == FillStyle.None ? "00ffffff" : ConvertToKmlColor(style.FillColor)));
				xtw.WriteEndElement(); // PolyStyle	                                              
			}
			
			if (featureType == FeatureType.Polyline || featureType == FeatureType.Polygon)
			{
				xtw.WriteStartElement("LineStyle");
				xtw.WriteElementString("width", (style.LineStyle == LineStyle.None ? 0 : style.LineWidth).ToString(CultureInfo.InvariantCulture));
				xtw.WriteElementString("color", ConvertToKmlColor(style.LineColor));
				xtw.WriteEndElement(); // LineStyle
			}
			else if (featureType == FeatureType.Point)
			{
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
			Style style = layer.GetStyleForFeature(feature.ThemeFieldValue);

			if (style == null) return;
			
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
				xtw.WriteElementString("coordinates", string.Format("{0},{1}", 
				                                                    pt.X.ToString(CultureInfo.InvariantCulture), 
				                                                    pt.Y.ToString(CultureInfo.InvariantCulture)));
				xtw.WriteEndElement(); // Point

				xtw.WriteEndElement(); // Placemark
			}
			else if (layer.Data.SourceFeatureType == FeatureType.Polyline)
			{
				PolyLine pl = feature as PolyLine;

              	foreach (Line l in pl.Lines)
				{
					xtw.WriteStartElement("Placemark");
				
					if (style != null && !string.IsNullOrEmpty(style.Id))
					{
						xtw.WriteElementString("styleUrl", "#" + style.Id);
					}
		
					if (name != null)
					{
						xtw.WriteElementString("name", name);
					}

					xtw.WriteStartElement("LineString");
					xtw.WriteElementString("tessellate", "1");

					StringBuilder sb = new StringBuilder();

					foreach (Point pt in l.Points)
					{
						Point tpt = pt;
						if (transform)
						{
							tpt = source.Transform(destination, pt);
						}

						sb.AppendFormat("{0},{1} ", 
						                tpt.X.ToString(CultureInfo.InvariantCulture), 
						                tpt.Y.ToString(CultureInfo.InvariantCulture));
					}
					
					xtw.WriteElementString("coordinates", sb.ToString());
					xtw.WriteEndElement(); // LineString
					xtw.WriteEndElement(); // Placemark
				}
			}
			else if (layer.Data.SourceFeatureType == FeatureType.Polygon)
			{
				Polygon pg = feature as Polygon;

				bool inRing = false;
                bool clockwiseIsExterior = true;

				foreach (Ring r in pg.Rings)
				{
                    // Assumptions: 
                    // - The first ring in an polygon is an exterior ring
                    // - Interior rings will come consecutively after exterior rings
                    // - Exterior/interior rings are determined by their ring orientation (clockwise/counter)

                    // Therefore, we check the first ring's orientation and use that
                    // to decide which orientation is exterior

                    bool isClockwise = r.IsClockwise;

                    if (!inRing)
                    {
                        clockwiseIsExterior = isClockwise;
                    }
  
					if (clockwiseIsExterior == isClockwise)
					{
						// exterior ring

						if (inRing)
						{
							xtw.WriteEndElement(); // Polygon
							xtw.WriteEndElement(); // Placemark
						}

						inRing = true;
						
						xtw.WriteStartElement("Placemark");

						if (style != null && !string.IsNullOrEmpty(style.Id))
						{
							xtw.WriteElementString("styleUrl", "#" + style.Id);
						}
			
						if (name != null)
						{
							xtw.WriteElementString("name", name);
						}

						xtw.WriteStartElement("Polygon");
						xtw.WriteStartElement("outerBoundaryIs");
						xtw.WriteStartElement("LinearRing");

						xtw.WriteElementString("tessellate", "1");

						StringBuilder sb = new StringBuilder();
						foreach (Point pt in r.Points)
						{
							Point tpt = pt;
							if (transform)
							{
								tpt = source.Transform(destination, pt);
							}
	
							sb.AppendFormat("{0},{1} ", 
							                tpt.X.ToString(CultureInfo.InvariantCulture), 
							                tpt.Y.ToString(CultureInfo.InvariantCulture));
						}
						
						xtw.WriteElementString("coordinates", sb.ToString());
						
						xtw.WriteEndElement(); // LinearRing
						xtw.WriteEndElement(); // outerBoundaryIs
											}
					else
					{
						xtw.WriteStartElement("innerBoundaryIs");
						xtw.WriteStartElement("LinearRing");

						xtw.WriteElementString("tessellate", "1");

						StringBuilder sb = new StringBuilder();
						foreach (Point pt in r.Points)
						{
							Point tpt = pt;
							if (transform)
							{
								tpt = source.Transform(destination, pt);
							}
	
							sb.AppendFormat("{0},{1} ", 
							                tpt.X.ToString(CultureInfo.InvariantCulture), 
							                tpt.Y.ToString(CultureInfo.InvariantCulture));
						}
						
						xtw.WriteElementString("coordinates", sb.ToString());

						xtw.WriteEndElement(); // LinearRing
						xtw.WriteEndElement(); // innerBoundaryIs
					}
				}

				xtw.WriteEndElement(); // Polygon
				xtw.WriteEndElement(); // Placemark
				//xe.Add(node);
			}
		}
	
		#endregion
	}
}
