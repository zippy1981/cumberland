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
using System.Linq;
using System.Xml.Linq;

using Cumberland;

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
			
			//XNamespace ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
			
			XDocument doc = new XDocument(new XElement("kml",
			                                           new XElement("Document",					                              
			                                                        from l in map.Layers
			                                                        where l.Data != null			                                                       
			                                                        select CreateFromLayer(l))));

			return doc.ToString(SaveOptions.DisableFormatting);
			
		}

		static XElement CreateFromLayer(Layer layer)
		{
			return new XElement("Folder",
			                     new XElement("name", layer.Id),
			                     from f in layer.Data.GetFeatures()
			                     select CreateFromFeature(f));
		}

		static List<XElement> CreateFromFeature(Feature feature)
		{
			List<XElement> xe = new List<XElement>();

			if (feature is Point)
			{
				Point pt = feature as Point;
				
				xe.Add(new XElement("Placemark", 
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
				                  new XElement("LineString",
				                               new XElement("coordinates",
					                                              from pt in l.Points
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
						                    new XElement("Polygon",
						                                 new XElement("outerBoundaryIs",
						                                              new XElement("LinearRing",
						                                                            new XElement("coordinates",
						                                                                        from pt in r.Points
						                                                                        select string.Format("{0},{1} ", pt.X, pt.Y) )))));
					}
					else
					{
						node.Element("Polygon").Add(new XElement("innerBoundaryIs",
						                                          new XElement("LinearRing",
						                                                            new XElement("coordinates",
						                                                                        from pt in r.Points
						                                                                        select string.Format("{0},{1} ", pt.X, pt.Y) ))));
					}
				}

				xe.Add(node);
			}

			return xe;
		}
	}
}
