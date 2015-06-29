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
using System.Globalization;
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
		static readonly string currentVersion = "0.1";
		
#endregion
		
#region ctors
		
		public MapSerializer()
		{
			AddFileFeatureSourceType<Shapefile>();
		}
		
#endregion
		
#region public methods
		
		public void AddFileFeatureSourceType<T>() where T : IFileFeatureSource 
		{
			fileFeatureSourceTypes.Add(typeof(T));
		}
		
		public void AddDatabaseFeatureSourceType<T>() where T : IDatabaseFeatureSource
		{
			dbFeatureSourceTypes.Add(typeof(T));
		}
		
		[Obsolete("Use AddFileFeatureSourceType<T>() instead")]
		public void AddFileFeatureSourceType(Type type)
		{
			if (type.GetInterface(typeof(IFileFeatureSource).ToString()) == null)
			{
				throw new ArgumentException("does not implement 'IFileFeatureSource'", "type");
			}

			fileFeatureSourceTypes.Add(type);
		}
		
		[Obsolete("Use AddDatabaseFeatureSourceType<T>() instead")]
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
			
			XmlNode mapNode = null;
			foreach(XmlNode node in doc.ChildNodes)
			{
				if (node.LocalName == "Map")
				{
					mapNode = node;
					break;
				}
			}
			
			if (mapNode == null)
			{
				throw new FormatException("'Map' node not found.  Must be at root level of xml");
			}
			
			// check for version attribute.  if none just try
//			XmlNode versionNode = mapNode.Attributes.GetNamedItem("version");
//			if (versionNode != null)
//			{
//				string version = versionNode.Value;
//				
//				if (version != currentVersion)
//				{
//					throw new NotSupportedException(string.Format("Version '{0}' is not supported.  Only '{1}'", 
//					                                              version,
//					                                              currentVersion));
//				}
//			}
			
			foreach (XmlNode node  in mapNode.ChildNodes)
			{
				string name = node.LocalName;
				
				if (name == "Layers")
				{
					foreach (XmlNode lnode in node.ChildNodes)
					{
						DeserializeLayer(lnode, map, mapPath);
					}
				}
				else if (name == "Extents")
				{
					map.Extents = ParseRectangle(node.InnerText);
				}
				else if (name == "Projection")
				{
					map.Projection = node.InnerText;
				}
				else if (name == "Width")
				{
					map.Width = int.Parse(node.InnerText,
					                      CultureInfo.InvariantCulture);
				}
				else if (name == "Height")
				{
					map.Height = int.Parse(node.InnerText,
					                       CultureInfo.InvariantCulture);
				}
				else if (name == "BackgroundColor")
				{
					map.BackgroundColor = ParseColor(node.InnerText);
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
			//return Encoding.UTF8.GetString(ms.GetBuffer());

			// Now read back
			ms.Seek(0, SeekOrigin.Begin);
			TextReader tr = new StreamReader(ms);
			return tr.ReadToEnd();
		}

		public static void Serialize(Stream stream, Map map)
		{
			//XmlWriterSettings xws = new XmlWriterSettings();
			//xws.Encoding = Encoding.UTF8;
			XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
			writer.WriteStartDocument();
			writer.WriteStartElement("Map");
			writer.WriteAttributeString("version", currentVersion);
			
			writer.WriteElementString("Width", 
			                          map.Width.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Height", 
			                          map.Height.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Extents", 
			                          PrepareRectangle(map.Extents));
			
			if (!string.IsNullOrEmpty(map.Projection)) 
			{
				writer.WriteElementString("Projection", map.Projection);
			}
			
			if (map.BackgroundColor != Color.Empty)
			{
				writer.WriteElementString("BackgroundColor",
				                          PrepareColor(map.BackgroundColor));
			}

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
				string name = child.LocalName;
				
				if (name == "Data")
				{
#region parse data
					string source = child.Attributes.GetNamedItem("sourceType").Value;
					string instance = child.Attributes.GetNamedItem("sourceInstance").Value;
					
					if (source != null && instance != null)
					{
						sourceType = Type.GetType(source);	
						
						if (sourceType == typeof(SimpleFeatureSource))
						{
							if (child.ChildNodes.Count < 1)
							{
								throw new FormatException("SimpleFeatureSource data type must have one child node");
							}
							
							XmlSerializer xs = new XmlSerializer(typeof(SimpleFeatureSource));
							l.Data = (SimpleFeatureSource) xs.Deserialize(new MemoryStream(UTF8Encoding.UTF8.GetBytes(child.InnerXml)));
							continue;
						}
						else if (sourceType == typeof(IDatabaseFeatureSource))
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

							l.Data = Activator.CreateInstance(instanceType) as IFeatureSource;
							
							foreach (XmlNode dnode in child.ChildNodes)
							{
								IDatabaseFeatureSource dfs = (l.Data as IDatabaseFeatureSource);
								switch (dnode.LocalName)
								{
							
								case "ConnectionString":
									
									dfs.ConnectionString = dnode.InnerText;
									break;
									
								case "TableName":
									
									dfs.TableName = dnode.InnerText;
									break;
									
								case "ForcedSrid":
									
									dfs.ForcedSrid = int.Parse(dnode.InnerText,
									                           CultureInfo.InvariantCulture);
									break;
									
								case "ForcedFeatureType":
									
									dfs.ForcedFeatureType = (FeatureType) Enum.Parse(typeof(FeatureType), dnode.InnerText);
									break;
									
								case "ForcedSpatialType":
									
									dfs.ForcedSpatialType = (SpatialType) Enum.Parse(typeof(SpatialType), dnode.InnerText);
									break;
									
								case "ForcedGeometryColumn":
									
									dfs.ForcedGeometryColumn = dnode.InnerText;
									break;
									
								}	
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
								throw new FormatException(string.Format("Source type '{0}' is not a supported file source", 
								                                        instance));
							}
							
							l.Data = Activator.CreateInstance(instanceType) as IFeatureSource;
							
							foreach (XmlNode dnode in child.ChildNodes)
							{
								if (dnode.LocalName == "FilePath")
								{	
									(l.Data as IFileFeatureSource).FilePath = AnchorPath(mapPath, dnode.InnerText);
								
								}
							}
						}
						else return; // unknown type
					}
#endregion
				}
				else if (name == "Projection")
				{
					l.Projection = child.InnerText;
				}
				else if (name == "Id")
				{
					l.Id = child.InnerText;
				}
				else if (name == "Styles")
				{
					foreach (XmlNode lnode in child.ChildNodes)
					{
						DeserializeStyle(lnode, l, mapPath);
					}
				}
				else if (name == "Theme")
				{
					l.Theme = (ThemeType) Enum.Parse(typeof(ThemeType), child.InnerText);
				}
				else if (name == "ThemeField")
				{
					l.ThemeField = child.InnerText;
				}
				else if (name == "LabelField")
				{
					l.LabelField = child.InnerText;
				}
				else if (name == "Visible")
				{
					l.Visible = bool.Parse(child.InnerText);
				}
				else if (name == "MaxScale")
				{
					l.MaxScale = double.Parse(child.InnerText,
					                          CultureInfo.InvariantCulture);
				}
				else if (name == "MinScale")
				{
					l.MinScale = double.Parse(child.InnerText,
					                          CultureInfo.InvariantCulture);
				}
                else if (name == "AllowDuplicateLabels")
                {
                    l.AllowDuplicateLabels = bool.Parse(child.InnerText);
                }
			}
			
			m.Layers.Add(l);
		}
		
		static void DeserializeStyle(XmlNode node, Layer layer, string mapPath)
		{
			Style style = new Style();
			
			foreach (XmlNode child in node.ChildNodes)
			{
				string name = child.LocalName;
				
				if (name == "LineWidth")
				{
					style.LineWidth = Convert.ToInt32(child.InnerText,
					                                  CultureInfo.InvariantCulture);
				}
				else if (name == "PointSize")
				{
					style.PointSize = Convert.ToInt32(child.InnerText,
					                                  CultureInfo.InvariantCulture);
				}
				else if (name == "LineColor")
				{
					style.LineColor = ParseColor(child.InnerText);
				}
				else if (name == "FillColor")
				{
					style.FillColor = ParseColor(child.InnerText);
				}
				else if (name == "LineStyle")
				{
					style.LineStyle = (LineStyle) Enum.Parse(typeof(LineStyle), 
					                                         child.InnerText);
				}
				else if (name == "PointSymbol")
				{
					style.PointSymbol = (PointSymbolType) Enum.Parse(typeof(PointSymbolType), 
					                                                 child.InnerText);
				}
				else if (name == "PointSymbolShape")
				{
					style.PointSymbolShape = (PointSymbolShapeType) Enum.Parse(typeof(PointSymbolShapeType),
					                                                       child.InnerText);
				}
				else if (name == "PointSymbolImagePath")
				{
					style.PointSymbolImagePath = AnchorPath(mapPath, 
					                                        child.InnerText);
				}
				else if (name == "MaxRangeThemeValue")
				{
					style.MaxRangeThemeValue = double.Parse(child.InnerText,
					                                        CultureInfo.InvariantCulture);
				}
				else if (name == "MinRangeThemeValue")
				{
					style.MinRangeThemeValue = double.Parse(child.InnerText,
					                                        CultureInfo.InvariantCulture);
				}
				else if (name == "UniqueThemeValue")
				{
					style.UniqueThemeValue = child.InnerText;
				}
				else if (name == "Id")
				{
					style.Id = child.InnerText;
				}
				else if (name == "FillStyle")
				{
					style.FillStyle = (FillStyle) Enum.Parse(typeof(FillStyle), 
					                                         child.InnerText);
				}
				else if (name == "LabelFont")
				{
					style.LabelFont = (LabelFont) Enum.Parse(typeof(LabelFont), 
					                                         child.InnerText);
				}
				else if (name == "ShowLabels")
				{
					style.ShowLabels = bool.Parse(child.InnerText);
				}
				else if (name == "LabelColor")
				{
					style.LabelColor = ParseColor(child.InnerText);
				}
				else if (name == "LabelFontEmSize")
				{
					style.LabelFontEmSize = float.Parse(child.InnerText,
					                                    CultureInfo.InvariantCulture);
				}
				else if (name == "LabelPosition")
				{
					style.LabelPosition = (LabelPosition) Enum.Parse(typeof(LabelPosition), 
					                                                 child.InnerText);
				}
				else if (name == "LabelPixelOffset")
				{
					style.LabelPixelOffset = int.Parse(child.InnerText,
					                                   CultureInfo.InvariantCulture);
				}
				else if (name == "LabelDecoration")
				{
					style.LabelDecoration = (LabelDecoration) Enum.Parse(typeof(LabelDecoration), 
					                                                     child.InnerText);
				}
				else if (name == "LabelOutlineColor")
				{
					style.LabelOutlineColor = ParseColor(child.InnerText);
				}
				else if (name == "LabelOutlineWidth")
				{
					style.LabelOutlineWidth = float.Parse(child.InnerText,
					                                      CultureInfo.InvariantCulture);
				}
				else if (name == "LabelAngle")
				{
					style.LabelAngle = float.Parse(child.InnerText,
					                               CultureInfo.InvariantCulture);
				}
				else if (name == "MinScale")
				{
					style.MinScale = double.Parse(child.InnerText,
					                              CultureInfo.InvariantCulture);
				}
				else if (name == "MaxScale")
				{
					style.MaxScale = double.Parse(child.InnerText,
					                              CultureInfo.InvariantCulture);
				}
				else if (name == "LabelMinScale")
				{
					style.LabelMinScale = double.Parse(child.InnerText,
					                                   CultureInfo.InvariantCulture);
				}
				else if (name == "LabelMaxScale")
				{
					style.LabelMaxScale = double.Parse(child.InnerText,
					                                   CultureInfo.InvariantCulture);
				}
				else if (name == "LabelCustomFont")
				{
					style.LabelCustomFont = child.InnerText;
				}
				else if (name == "DrawPointSymbolOnPolyLine")
				{
					style.DrawPointSymbolOnPolyLine = bool.Parse(child.InnerText);
				}
				else if (name == "CalculateLabelAngleForPolyLine")
				{
					style.CalculateLabelAngleForPolyLine = bool.Parse(child.InnerText);
				}
				else if (name == "FillTexturePath")
				{
					style.FillTexturePath = AnchorPath(mapPath, child.InnerText);
				}			
				else if (name == "Simplify")
				{
					style.Simplify = bool.Parse(child.InnerText);
				}
				else if (name == "SimplifyTolerance")
				{
					style.SimplifyTolerance = double.Parse(child.InnerText,
                        CultureInfo.InvariantCulture);
				}
				else if (name == "UniqueElseFlag")
				{
					style.UniqueElseFlag = bool.Parse(child.InnerText);
				}
			}
			
			layer.Styles.Add(style);
		}
		
#endregion
		
#region private static methods
		
		static void SerializeLayer(XmlWriter writer, Layer layer)
		{
			if (layer.Data == null) return;
			
			writer.WriteStartElement("Layer");

			// Layer properties
			if (!string.IsNullOrEmpty(layer.Projection)) 
			{
				writer.WriteElementString("Projection", layer.Projection);
			}
			
			if (!string.IsNullOrEmpty(layer.Id))
			{
				writer.WriteElementString("Id", layer.Id);
			}
			
			if (layer.Theme != ThemeType.None) 
			{
				writer.WriteElementString("Theme", Enum.GetName(typeof(ThemeType), layer.Theme));
			}
			
			if (!string.IsNullOrEmpty(layer.ThemeField)) 
			{
				writer.WriteElementString("ThemeField", layer.ThemeField);
			}
			
			if (!string.IsNullOrEmpty(layer.LabelField)) 
			{
				writer.WriteElementString("LabelField", layer.LabelField);
			}
			
			writer.WriteElementString("Visible", layer.Visible.ToString());
			
			if (layer.MinScale > double.MinValue) 
			{
				writer.WriteElementString("MinScale", layer.MinScale.ToString(CultureInfo.InvariantCulture));
			}
			
			if (layer.MaxScale < double.MaxValue) 
			{
				writer.WriteElementString("MaxScale", layer.MaxScale.ToString(CultureInfo.InvariantCulture));
			}

            if (!layer.AllowDuplicateLabels)
            {
                writer.WriteElementString("AllowDuplicateLabels", layer.AllowDuplicateLabels.ToString());
            }
			
			// handle Data Element
			writer.WriteStartElement("Data");
			IFileFeatureSource ffp = layer.Data as IFileFeatureSource;
			IDatabaseFeatureSource dfp = layer.Data as IDatabaseFeatureSource;
			SimpleFeatureSource sfs = layer.Data as SimpleFeatureSource;
			
			if (sfs != null)
			{
				// add sourceType attribute to Data element
				writer.WriteAttributeString("sourceType", typeof(SimpleFeatureSource).ToString());
				writer.WriteAttributeString("sourceInstance", typeof(SimpleFeatureSource).ToString());
				
				XmlSerializer xs =  new XmlSerializer(typeof(SimpleFeatureSource));
				xs.Serialize(writer, sfs);
			}
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
				writer.WriteAttributeString("sourceType", 
				                            typeof(IDatabaseFeatureSource).ToString());
				writer.WriteAttributeString("sourceInstance", 
				                            layer.Data.GetType().ToString());
				
				writer.WriteElementString("ConnectionString", 
				                          dfp.ConnectionString);
				writer.WriteElementString("TableName", 
				                          dfp.TableName);
				
				if (dfp.ForcedSrid >= 0)
				{
					writer.WriteElementString("ForcedSrid", 
					                          dfp.ForcedSrid.ToString(CultureInfo.InvariantCulture));
				}
				
				if (dfp.ForcedSpatialType != SpatialType.None)
				{
					writer.WriteElementString("ForcedSpatialType", 
					                          Enum.GetName(typeof(SpatialType), 
					                                       dfp.ForcedSpatialType));
				}
				
				if (dfp.ForcedFeatureType != FeatureType.None)
				{
					writer.WriteElementString("ForcedFeatureType", 
					                          Enum.GetName(typeof(FeatureType), 
					                                       dfp.ForcedFeatureType));
				}
				
				if (!string.IsNullOrEmpty(dfp.ForcedGeometryColumn))
				{
					writer.WriteElementString("ForcedGeometryColumn", 
					                          dfp.ForcedGeometryColumn);
				}
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
			
			writer.WriteElementString("LineStyle", 
			                          Enum.GetName(typeof(LineStyle), 
			                                       style.LineStyle));
			writer.WriteElementString("PointSymbol", 
			                          Enum.GetName(typeof(PointSymbolType), 
			                                       style.PointSymbol));
			writer.WriteElementString("PointSymbolShape", 
			                          Enum.GetName(typeof(PointSymbolShapeType), 
			                                       style.PointSymbolShape));
			if (!string.IsNullOrEmpty(style.PointSymbolImagePath))
			{
				writer.WriteElementString("PointSymbolImagePath", 
				                          style.PointSymbolImagePath);
			}
			
			writer.WriteElementString("LineWidth", 
			                          style.LineWidth.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("PointSize", 
			                          style.PointSize.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("LineColor", 
			                          PrepareColor(style.LineColor));
			writer.WriteElementString("FillColor", 
			                          PrepareColor(style.FillColor));
			
			if (!string.IsNullOrEmpty(style.UniqueThemeValue)) 
			{
				writer.WriteElementString("UniqueThemeValue", 
				                          style.UniqueThemeValue);
			}
			
			if (style.MaxRangeThemeValue < double.MaxValue)
			{
				writer.WriteElementString("MaxRangeThemeValue", 
				                          style.MaxRangeThemeValue.ToString(CultureInfo.InvariantCulture));
			}
			
			if (style.MinRangeThemeValue > double.MinValue)
			{
				writer.WriteElementString("MinRangeThemeValue", 
				                          style.MinRangeThemeValue.ToString(CultureInfo.InvariantCulture));
			}
			
			if (!string.IsNullOrEmpty(style.Id))
			{
				writer.WriteElementString("Id", style.Id);
			}
			
			writer.WriteElementString("FillStyle", 
			                          Enum.GetName(typeof(FillStyle), 
			                                       style.FillStyle));

			writer.WriteElementString("LabelFont", 
			                          Enum.GetName(typeof(LabelFont), 
			                                       style.LabelFont));
			writer.WriteElementString("ShowLabels", 
			                          style.ShowLabels.ToString());
			writer.WriteElementString("LabelColor", 
			                          PrepareColor(style.LabelColor));
			writer.WriteElementString("LabelFontEmSize", 
			                          style.LabelFontEmSize.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("LabelPosition", 
			                          Enum.GetName(typeof(LabelPosition), 
			                                       style.LabelPosition));
			writer.WriteElementString("LabelPixelOffset", 
			                          style.LabelPixelOffset.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("LabelDecoration", 
			                          Enum.GetName(typeof(LabelDecoration), 
			                                       style.LabelDecoration));
			writer.WriteElementString("LabelOutlineColor", 
			                          PrepareColor(style.LabelOutlineColor));
			writer.WriteElementString("LabelOutlineWidth", 
			                          style.LabelOutlineWidth.ToString(CultureInfo.InvariantCulture));
			if (style.LabelAngle != 0) 
			{
				writer.WriteElementString("LabelAngle", 
				                          style.LabelAngle.ToString(CultureInfo.InvariantCulture));
			}
			
			if (!string.IsNullOrEmpty(style.LabelCustomFont)) 
			{
				writer.WriteElementString("LabelCustomFont", 
				                          style.LabelCustomFont);
			}

			if (style.MinScale > double.MinValue) 
			{
				writer.WriteElementString("MinScale", 
				                          style.MinScale.ToString(CultureInfo.InvariantCulture));
			}
			
			if (style.MaxScale < double.MaxValue)
			{
				writer.WriteElementString("MaxScale", 
				                          style.MaxScale.ToString(CultureInfo.InvariantCulture));
			}

			if (style.LabelMinScale > double.MinValue)
			{
				writer.WriteElementString("LabelMinScale",
				                          style.LabelMinScale.ToString(CultureInfo.InvariantCulture));
			}
			
			if (style.LabelMaxScale < double.MaxValue) 
			{
				writer.WriteElementString("LabelMaxScale",
				                          style.LabelMaxScale.ToString(CultureInfo.InvariantCulture));
			}
			
			if (style.DrawPointSymbolOnPolyLine) 
			{
				writer.WriteElementString("DrawPointSymbolOnPolyLine", 
				                          style.DrawPointSymbolOnPolyLine.ToString());
			}
			
			if (!style.CalculateLabelAngleForPolyLine) 
			{
				writer.WriteElementString("CalculateLabelAngleForPolyLine", 
				                          style.CalculateLabelAngleForPolyLine.ToString());
			}

			if (!string.IsNullOrEmpty(style.FillTexturePath)) 
			{
				writer.WriteElementString("FillTexturePath", 
				                          style.FillTexturePath);
			}
			
			if (style.Simplify)
			{
				writer.WriteElementString("Simplify", style.Simplify.ToString());
				writer.WriteElementString("SimplifyTolerance", style.SimplifyTolerance.ToString());
			}
			
			if (style.UniqueElseFlag)
			{
				writer.WriteElementString("UniqueElseFlag", style.UniqueElseFlag.ToString());
			}
			
			writer.WriteEndElement(); // Style
		}
		
		static string PrepareRectangle(Rectangle r)
		{
			return string.Format("{0},{1},{2},{3}", 
			                     r.Min.X.ToString(CultureInfo.InvariantCulture), 
			                     r.Min.Y.ToString(CultureInfo.InvariantCulture), 
			                     r.Max.X.ToString(CultureInfo.InvariantCulture), 
			                     r.Max.Y.ToString(CultureInfo.InvariantCulture));
		}
		
		static Rectangle ParseRectangle(string s)
		{
			string[] parts = s.Split(',');
			return new Rectangle(double.Parse(parts[0], CultureInfo.InvariantCulture),
			                     double.Parse(parts[1], CultureInfo.InvariantCulture),
			                     double.Parse(parts[2], CultureInfo.InvariantCulture),
			                     double.Parse(parts[3], CultureInfo.InvariantCulture));
		}

		static string PrepareColor(Color c)
		{
			return string.Format("{0},{1},{2},{3}", 
			                     c.A.ToString(CultureInfo.InvariantCulture), 
			                     c.R.ToString(CultureInfo.InvariantCulture), 
			                     c.G.ToString(CultureInfo.InvariantCulture), 
			                     c.B.ToString(CultureInfo.InvariantCulture));
		}
		
		static Color ParseColor(string s)
		{
			string[] p = s.Split(',');
			
			return Color.FromArgb(int.Parse(p[0], CultureInfo.InvariantCulture),
			                      int.Parse(p[1], CultureInfo.InvariantCulture),
			                      int.Parse(p[2], CultureInfo.InvariantCulture),
			                      int.Parse(p[3], CultureInfo.InvariantCulture));
		}
		
		static string AnchorPath(string mapPath, string filePath)
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
