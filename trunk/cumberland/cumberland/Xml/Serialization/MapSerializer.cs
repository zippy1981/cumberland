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
		
		List<Type> fileFeatureProviders = new List<Type>();
		List<Type> dbFeatureProviders = new List<Type>();
		
#endregion
		
#region ctors
		
		public MapSerializer()
		{
			AddFileFeatureProvider(typeof(Shapefile));
		}
		
#endregion
		
#region public methods
		
		public void AddFileFeatureProvider(Type type)
		{
			if (type.GetInterface(typeof(IFileFeatureProvider).ToString()) == null)
			{
				throw new ArgumentException("does not implement 'IFileFeatureProvider'", "type");
			}

			fileFeatureProviders.Add(type);
		}
		
		public void AddDBFeatureProvider(Type type)
		{
			if (type.GetInterface(typeof(IDBFeatureProvider).ToString()) == null)
			{
				throw new ArgumentException("does not implement 'IDBFeatureProvider'", "type");
			}

			dbFeatureProviders.Add(type);
		}
		
		public Map Deserialize(string path)
		{
			return Deserialize(new FileStream(path, FileMode.Open, FileAccess.Read));
		}
		
		public Map Deserialize(Stream stream)
		{
			Map map = new Map();		
			int layerIndex = 0;	
			XmlDocument doc = new XmlDocument();
			doc.Load(stream);
			
			foreach (XmlNode node  in doc.ChildNodes[1].ChildNodes)
			{
				if (node.Name == "Layers")
				{
					foreach (XmlNode lnode in node.ChildNodes)
					{
						DeserializeLayer(lnode, map, layerIndex++);
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
			}
			
			return map;
		}

#endregion
		
#region public static methods
		
		public static string Serialize(Map map)
		{
			MemoryStream ms = new MemoryStream();
			//StringWriter sw = new StringWriter();
			
			XmlWriterSettings xws = new XmlWriterSettings();
			xws.Encoding = Encoding.UTF8;
			XmlWriter writer = XmlWriter.Create(ms, xws);
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

			//return sw.ToString();
			return Encoding.UTF8.GetString(ms.GetBuffer());
		}

#endregion
		
#region private methods
		
		void DeserializeLayer(XmlNode node, Map m, int layerIndex)
		{
			Layer l = new Layer();
			Type providerType = null;
			Type instanceType = null;
			
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Data")
				{
#region parse data
					string provider = child.Attributes.GetNamedItem("providerType").Value;
					string instance = child.Attributes.GetNamedItem("providerInstance").Value;
					
					if (provider != null && instance != null)
					{
						providerType = Type.GetType(provider);	
						
						if (providerType == typeof(IDBFeatureProvider))
						{
							foreach (Type t in dbFeatureProviders)
							{
								if (instance == t.ToString())
								{
									instanceType = t;
								}
							}
						}
						else if (providerType == typeof(IFileFeatureProvider))
						{
							foreach (Type t in fileFeatureProviders)
							{

								if (instance == t.ToString())
								{
									instanceType = t;
								}
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
							
							if (providerType == typeof(IFileFeatureProvider))
							{
								(l.Data as IFileFeatureProvider).FilePath = dnode.InnerText;
							}
							
							break;
							
						case "ConnectionString":
							
							if (providerType == typeof(IDBFeatureProvider))
							{
								(l.Data as IDBFeatureProvider).ConnectionString = dnode.InnerText;
							}
							break;
							
						case "TableName":
							
							if (providerType == typeof(IDBFeatureProvider))
							{
								(l.Data as IDBFeatureProvider).TableName = dnode.InnerText;
							}
							break;
						}
						
					}
#endregion
				}
				else if (child.Name == "LineWidth")
				{
					l.LineWidth = Convert.ToInt32(child.InnerText);
				}
				else if (child.Name == "PointSize")
				{
					l.PointSize = Convert.ToInt32(child.InnerText);
				}
				else if (child.Name == "LineColor")
				{
					l.LineColor = ParseColor(child.InnerText);
				}
				else if (child.Name == "FillColor")
				{
					l.FillColor = ParseColor(child.InnerText);
				}
				else if (child.Name == "Projection")
				{
					l.Projection = child.InnerText;
				}
				else if (child.Name == "Id")
				{
					l.Id = child.InnerText;
				}
				else if (child.Name == "LineStyle")
				{
					l.LineStyle = (LineStyle) Enum.Parse(typeof(LineStyle), child.InnerText);
				}
			}
			
			m.Layers.Insert(layerIndex, l);
		}
		
#endregion
		
#region private static methods
		
		static void SerializeLayer(XmlWriter writer, Layer layer)
		{
			if (layer.Data == null || layer.Data is SimpleFeatureSource) return;
			
			writer.WriteStartElement("Layer");
			writer.WriteElementString("LineWidth", layer.LineWidth.ToString());
			writer.WriteElementString("PointSize", layer.PointSize.ToString());
			writer.WriteElementString("LineColor", PrepareColor(layer.LineColor));
			writer.WriteElementString("FillColor", PrepareColor(layer.FillColor));
			writer.WriteElementString("Projection", layer.Projection);
			writer.WriteElementString("Id", layer.Id);
			writer.WriteElementString("LineStyle", Enum.GetName(typeof(LineStyle), layer.LineStyle));
			writer.WriteStartElement("Data");

			IFileFeatureProvider ffp = layer.Data as IFileFeatureProvider;
			IDBFeatureProvider dfp = layer.Data as IDBFeatureProvider;
			if (ffp != null)
			{
				// add providerType attribute to Data element
				writer.WriteAttributeString("providerType", typeof(IFileFeatureProvider).ToString());
				writer.WriteAttributeString("providerInstance", layer.Data.GetType().ToString());
				
				writer.WriteElementString("FilePath", ffp.FilePath);

			}
			else if (dfp != null)
			{
				// add providerType attribute to Data element
				writer.WriteAttributeString("providerType", typeof(IDBFeatureProvider).ToString());
				writer.WriteAttributeString("providerInstance", layer.Data.GetType().ToString());
				
				writer.WriteElementString("ConnectionString", dfp.ConnectionString);
				writer.WriteElementString("TableName", dfp.TableName);
			}
			
			writer.WriteEndElement(); // Data
			writer.WriteEndElement(); // Layer
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
		
#endregion
	}
}
