// MapSerializer.cs
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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Cumberland;
using Cumberland.Data;
using Cumberland.Data.Shapefile;

namespace Cumberland.Xml.Serialization
{
	public class MapSerializer
	{
#region vars
		
		List<Type> fileFeatureSourceTypes = new List<Type>();
		List<Type> dbFeatureSourceTypes = new List<Type>();
		
#endregion
		
#region ctors
		
		public MapSerializer()
		{
			AddFileFeatureSourceType(typeof(Shapefile));
		}
		
#endregion
		
#region public methods
		
		public void AddFileFeatureSourceType(Type type)
		{
			if (type.GetInterface(typeof(IFileFeatureSource).ToString()) == null)
			{
				throw new ArgumentException("does not implement 'IFileFeatureSource'", "type");
			}

			fileFeatureSourceTypes.Add(type);
		}
		
		public void AddDatabaseFeatureSourceType(Type type)
		{
			if (type.GetInterface(typeof(IDatabaseFeatureSource).ToString()) == null)
			{
				throw new ArgumentException("does not implement 'IDatabaseFeatureSource'", "type");
			}

			dbFeatureSourceTypes.Add(type);
		}
		
		public Map Deserialize(string mapPath)
		{
			using (FileStream fs = new FileStream(mapPath, FileMode.Open, FileAccess.Read))
			{
				return Deserialize(fs, mapPath);
			}
		}
		
		public Map Deserialize(Stream stream)
		{
			return Deserialize(stream, null);
		}

		Map Deserialize(Stream stream, string mapPath)
		{
			Map map = new Map();		
			//int layerIndex = 0;	
			XmlDocument doc = new XmlDocument();
			doc.Load(stream);
			
			foreach (XmlNode node  in doc.ChildNodes[1].ChildNodes)
			{
				if (node.Name == "Layers")
				{
					foreach (XmlNode lnode in node.ChildNodes)
					{
						DeserializeLayer(lnode, map, mapPath);
					}
				}
				else if (node.Name == "Extents")
				{
					map.Extents = ParseRectangle(node.InnerText);
				}
				else if (node.Name == "Projection")
				{
					map.Projection = node.InnerText;
				}
				else if (node.Name == "Width")
				{
					map.Width = int.Parse(node.InnerText);
				}
				else if (node.Name == "Height")
				{
					map.Height = int.Parse(node.InnerText);
				}
			}
			
			return map;
		}

#endregion
		
#region public static methods
		
		public static string Serialize(Map map)
		{
			MemoryStream ms = new MemoryStream();
			//StringWriter sw = new StringWriter();
			
			Serialize(ms, map);

			//return sw.ToString();
			return Encoding.UTF8.GetString(ms.GetBuffer());
		}

		public static void Serialize(Stream stream, Map map)
		{
			//XmlWriterSettings xws = new XmlWriterSettings();
			//xws.Encoding = Encoding.UTF8;
			XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
			writer.WriteStartDocument();
			writer.WriteStartElement("Map");
			
			writer.WriteElementString("Width", map.Width.ToString());
			writer.WriteElementString("Height", map.Height.ToString());
			writer.WriteElementString("Extents", PrepareRectangle(map.Extents));
			writer.WriteElementString("Projection", map.Projection);

			writer.WriteStartElement("Layers");
			
			foreach (Layer l in map.Layers)
			{
				SerializeLayer(writer, l);
			}

			writer.WriteEndElement(); // Layers
			
			writer.WriteEndElement(); // Map
			writer.WriteEndDocument();
			writer.Flush();
		}
		
#endregion
		
#region private methods
		
		void DeserializeLayer(XmlNode node, Map m, string mapPath)
		{
			Layer l = new Layer();
			Type sourceType = null;
			Type instanceType = null;
			
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Data")
				{
#region parse data
					string source = child.Attributes.GetNamedItem("sourceType").Value;
					string instance = child.Attributes.GetNamedItem("sourceInstance").Value;
					
					if (source != null && instance != null)
					{
						sourceType = Type.GetType(source);	
						
						if (sourceType == typeof(IDatabaseFeatureSource))
						{
							foreach (Type t in dbFeatureSourceTypes)
							{
								if (instance == t.ToString())
								{
									instanceType = t;
								}
							}
							
							if (instanceType == null)
							{
								throw new FormatException(string.Format("Source type '{0}' is not a supported database source", instance));
							}
						}
						else if (sourceType == typeof(IFileFeatureSource))
						{
							foreach (Type t in fileFeatureSourceTypes)
							{
								if (instance == t.ToString())
								{
									instanceType = t;
								}
							}

							if (instanceType == null)
							{
								throw new FormatException(string.Format("Source type '{0}' is not a supported file source", instance));
							}
						}
						else return; // unknown type
						
						l.Data = Activator.CreateInstance(instanceType) as IFeatureSource;
					}
					
					foreach (XmlNode dnode in child.ChildNodes)
					{
						switch (dnode.Name)
						{
						case "FilePath":
							
							if (sourceType == typeof(IFileFeatureSource))
							{
								(l.Data as IFileFeatureSource).FilePath = AnchorPath(mapPath, dnode.InnerText);
							}
							
							break;
							
						case "ConnectionString":
							
							if (sourceType == typeof(IDatabaseFeatureSource))
							{
								(l.Data as IDatabaseFeatureSource).ConnectionString = dnode.InnerText;
							}
							break;
							
						case "TableName":
							
							if (sourceType == typeof(IDatabaseFeatureSource))
							{
								(l.Data as IDatabaseFeatureSource).TableName = dnode.InnerText;
							}
							break;
						}
						
					}
#endregion
				}
				else if (child.Name == "Projection")
				{
					l.Projection = child.InnerText;
				}
				else if (child.Name == "Id")
				{
					l.Id = child.InnerText;
				}
				else if (child.Name == "Styles")
				{
					foreach (XmlNode lnode in child.ChildNodes)
					{
						DeserializeStyle(lnode, l, mapPath);
					}
				}
			}
			
			m.Layers.Add(l);
		}
		
		void DeserializeStyle(XmlNode node, Layer layer, string mapPath)
		{
			Style style = new Style();
			
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "LineWidth")
				{
					style.LineWidth = Convert.ToInt32(child.InnerText);
				}
				else if (child.Name == "PointSize")
				{
					style.PointSize = Convert.ToInt32(child.InnerText);
				}
				else if (child.Name == "LineColor")
				{
					style.LineColor = ParseColor(child.InnerText);
				}
				else if (child.Name == "FillColor")
				{
					style.FillColor = ParseColor(child.InnerText);
				}
				else if (child.Name == "LineStyle")
				{
					style.LineStyle = (LineStyle) Enum.Parse(typeof(LineStyle), child.InnerText);
				}
				else if (child.Name == "PointSymbol")
				{
					style.PointSymbol = (PointSymbolType) Enum.Parse(typeof(PointSymbolType), child.InnerText);
				}
				else if (child.Name == "PointSymbolShape")
				{
					style.PointSymbolShape = (PointSymbolShapeType) Enum.Parse(typeof(PointSymbolShapeType),
					                                                       child.InnerText);
				}
				else if (child.Name == "PointSymbolImagePath")
				{
					style.PointSymbolImagePath = AnchorPath(mapPath, child.InnerText);
				}
			}
			
			layer.Styles.Add(style);
		}
		
#endregion
		
#region private static methods
		
		static void SerializeLayer(XmlWriter writer, Layer layer)
		{
			if (layer.Data == null || layer.Data is SimpleFeatureSource) return;
			
			writer.WriteStartElement("Layer");

			// Layer properties
			writer.WriteElementString("Projection", layer.Projection);
			writer.WriteElementString("Id", layer.Id);

			
			// handle Data Element
			writer.WriteStartElement("Data");
			IFileFeatureSource ffp = layer.Data as IFileFeatureSource;
			IDatabaseFeatureSource dfp = layer.Data as IDatabaseFeatureSource;
			if (ffp != null)
			{
				// add sourceType attribute to Data element
				writer.WriteAttributeString("sourceType", typeof(IFileFeatureSource).ToString());
				writer.WriteAttributeString("sourceInstance", layer.Data.GetType().ToString());
			
				writer.WriteElementString("FilePath", ffp.FilePath);

			}
			else if (dfp != null)
			{
				// add sourceType attribute to Data element
				writer.WriteAttributeString("sourceType", typeof(IDatabaseFeatureSource).ToString());
				writer.WriteAttributeString("sourceInstance", layer.Data.GetType().ToString());
				
				writer.WriteElementString("ConnectionString", dfp.ConnectionString);
				writer.WriteElementString("TableName", dfp.TableName);
			}
			writer.WriteEndElement(); // Data
			
			// handles Styles
			writer.WriteStartElement("Styles");
			foreach (Style style in layer.Styles)
			{
				SerializeStyle(writer, style);
			}
			writer.WriteEndElement(); // Styles
			
			writer.WriteEndElement(); // Layer
		}
		
		static void SerializeStyle(XmlWriter writer, Style style)
		{
			writer.WriteStartElement("Style");
			
			writer.WriteElementString("LineStyle", Enum.GetName(typeof(LineStyle), style.LineStyle));
			writer.WriteElementString("PointSymbol", Enum.GetName(typeof(PointSymbolType), style.PointSymbol));
			writer.WriteElementString("PointSymbolShape", Enum.GetName(typeof(PointSymbolShapeType), style.PointSymbolShape));
			writer.WriteElementString("PointSymbolImagePath", style.PointSymbolImagePath);
			writer.WriteElementString("LineWidth", style.LineWidth.ToString());
			writer.WriteElementString("PointSize", style.PointSize.ToString());
			writer.WriteElementString("LineColor", PrepareColor(style.LineColor));
			writer.WriteElementString("FillColor", PrepareColor(style.FillColor));
			
			writer.WriteEndElement(); // Style
		}
		
		static string PrepareRectangle(Rectangle r)
		{
			return string.Format("{0},{1},{2},{3}", r.Min.X, r.Min.Y, r.Max.X, r.Max.Y);
		}
		
		static Rectangle ParseRectangle(string s)
		{
			string[] parts = s.Split(',');
			return new Rectangle(double.Parse(parts[0]),
			                     double.Parse(parts[1]),
			                     double.Parse(parts[2]),
			                     double.Parse(parts[3]));
		}

		static string PrepareColor(Color c)
		{
			return string.Format("{0},{1},{2},{3}", c.A, c.R, c.G, c.B);
		}
		
		static Color ParseColor(string s)
		{
			string[] p = s.Split(',');
			
			return Color.FromArgb(int.Parse(p[0]),
			                      int.Parse(p[1]),
			                      int.Parse(p[2]),
			                      int.Parse(p[3]));
		}
		
		string AnchorPath(string mapPath, string filePath)
		{
			if (filePath != null && mapPath != null && !Path.IsPathRooted(filePath))
			{
				// anchor this path to the map path
				string root = Path.GetDirectoryName(Path.GetFullPath(mapPath));
				filePath = Path.Combine(root, filePath);
			}
			
			return filePath;
		}
		
#endregion
	}
}
