// MapSerializerTests.cs
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

using NUnit.Framework;

using Cumberland;
using Cumberland.Data;
using Cumberland.Xml.Serialization;

namespace Cumberland.Tests
{
	[TestFixture()]
	public class MapSerializerTests
	{
		Map m1, m2;
		
#region dummy providers
		
		class DummyFileProvider : IFileFeatureSource
		{
			public string FilePath {
				get {
					return f;
				}
				set {
					f = value;
				}
			}	
			string f = "MyFile";

			public FeatureType SourceFeatureType {
				get {
					return FeatureType.Polygon;
				}
			}
	
			public Rectangle Extents {
				get {
					return new Rectangle(0,0,20,20);
				}
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures ()
			{
				return new List<Feature>();
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures (Rectangle rectangle)
			{
				return GetFeatures();				
			}
			
			public List<Feature> GetFeatures(string themeField)
			{
				return GetFeatures();
			}
			
			public List<Feature> GetFeatures(Cumberland.Rectangle rectangle, string themeField)
			{
				return GetFeatures();
			}
		
		}
		
		class DummyDBProvider : IDatabaseFeatureSource
		{
			public string TableName
			{
				get { return tableName; }
				set { tableName = value; }
			}
			
			public string ConnectionString
			{
				get { return connectionString; }
				set { connectionString = value; }
			}
			
			string tableName = "MyTable";
			string connectionString = "myConnectionString";
			int forcedSrid = 4326;
			FeatureType forcedFeatureType = FeatureType.Polygon;
			SpatialType forcedSpatialType = SpatialType.None;
			string forcedGeometryColumn;
			
			public FeatureType SourceFeatureType {
				get {
					return FeatureType.Polyline;
				}
			}
	
			public string ForcedGeometryColumn {
				get {
					return forcedGeometryColumn;
				}
				set {
					forcedGeometryColumn = value;
				}
			}
			
			public Rectangle Extents {
				get {
					return new Rectangle(0,0,30,30);
				}
			}
			
			public int ForcedSrid {
				get {
					return forcedSrid;
				}
				set {
					forcedSrid = value;
				}
			}
	
			public FeatureType ForcedFeatureType {
				get {
					return forcedFeatureType;
				}
				set {
					forcedFeatureType = value;
				}
			}
			
			public SpatialType ForcedSpatialType {
				get {
					return forcedSpatialType;
				}
				set {
					forcedSpatialType = value;
				}
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures ()
			{
				return new List<Feature>();
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures (Rectangle rectangle)
			{
				return GetFeatures();				
			}
			
			public List<Feature> GetFeatures(string themeField)
			{
				return GetFeatures();
			}
			
			public List<Feature> GetFeatures(Cumberland.Rectangle rectangle, string themeField)
			{
				return GetFeatures();
			}

		}
		
#endregion
	
#region test fixture set up
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			MapSerializer ms = new MapSerializer();
			ms.AddDatabaseFeatureSourceType(typeof(DummyDBProvider));
			ms.AddFileFeatureSourceType(typeof(DummyFileProvider));
			
			m1 = new Map();
			m1.Extents = new Rectangle(0,0,10,10);
			m1.Height = 123;
			m1.Width = 321;
			m1.Projection = "+init=epsg:4326";
			
			Layer l1 = new Layer();
			l1.Id = "l1";
			DummyDBProvider db = new DummyDBProvider();
			l1.Data = db;
			l1.Projection = "+init=epsg:2236";
			l1.Theme = ThemeType.NumericRange;
			l1.ThemeField = "MyField";
			db.ConnectionString = "MyConnString";
			db.ForcedFeatureType = FeatureType.Polyline;
			db.ForcedSpatialType = SpatialType.Geographic;
			db.ForcedSrid = 1234;
			db.TableName = "MyTable";
			db.ForcedGeometryColumn = "MyGeoColumn";
			
			Style s1 = new Style();
			s1.LineColor = Color.FromArgb(255, 180, 34, 34);
			s1.LineStyle = LineStyle.Dashed;
			s1.LineWidth = 23;
			s1.PointSize = 4;
			s1.PointSymbol = PointSymbolType.Image;
			s1.PointSymbolShape = PointSymbolShapeType.Square;
			s1.UniqueThemeValue = "MyValue";
			s1.MaxRangeThemeValue = 30000;
			s1.MinRangeThemeValue = 4;
			l1.Styles.Add(s1);
			
			m1.Layers.Add(l1);
			
			Layer l2 = new Layer();
			l2.Id = "l2";
			l2.Data = new DummyFileProvider();
			m1.Layers.Add(l2);
			
			string s = MapSerializer.Serialize(m1);
			
			m2 = ms.Deserialize(new MemoryStream(UTF8Encoding.UTF8.GetBytes((s))));
		}
		
#endregion
		
#region test map properties serialized
		
		[Test()]
		public void TestMapExtentsSerialized()
		{	
			Assert.AreEqual(m1.Extents, m2.Extents);
		}
		
		[Test]
		public void TestMapHeightSerialized()
		{
			Assert.AreEqual(m1.Height, m2.Height);
		}
		
		[Test]
		public void TestMapWidthSerialized()
		{
			Assert.AreEqual(m1.Width, m2.Width);
		}
		
		[Test]
		public void TestMapProjectionSerialized()
		{
			Assert.AreEqual(m1.Projection, m2.Projection);
		}
		
		[Test, ExpectedException(typeof(NotSupportedException))]
		public void TestMapVersionFails()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Map version=\"bad\">	<Width>500</Width>	<Height>400</Height>	<Extents>-1,-4,10,10</Extents>	<Projection>+init=epsg:4326</Projection>	<Layers /></Map>";
			
			MapSerializer ms = new MapSerializer();
			ms.Deserialize(new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
		}
		
#endregion
		
#region test add unsuppored feature sources
		
		[Test, ExpectedException(typeof(FormatException))]
		public void TestAddUnsupportedFileFeatureSource()
		{
			Map m = new Map();
			Layer l = new Layer();
			l.Id = "l";
			l.Data = new DummyFileProvider();
			m.Layers.Add(l);
			
			string x = MapSerializer.Serialize(m);
			
			MapSerializer ms = new MapSerializer();
			
			// should fail as this not a supported provider
			m2 = ms.Deserialize(new MemoryStream(UTF8Encoding.UTF8.GetBytes((x))));
		}
		
		[Test, ExpectedException(typeof(FormatException))]
		public void TestAddUnsupportedDatabaseFeatureSource()
		{
			Map m = new Map();
			Layer l = new Layer();
			l.Id = "l";
			l.Data = new DummyDBProvider();
			m.Layers.Add(l);
			
			string x = MapSerializer.Serialize(m);
			
			MapSerializer ms = new MapSerializer();
			
			// should fail as this not a supported provider
			m2 = ms.Deserialize(new MemoryStream(UTF8Encoding.UTF8.GetBytes((x))));
		}

#endregion
		
#region test layer properties serialized
		
		[Test]
		public void TestLayerOrderSerializedCorrectlyAndId()
		{
			Assert.AreEqual(m1.Layers[0].Id, m2.Layers[0].Id);
		}
		
		[Test]
		public void TestLayerProjectionSerialized()
		{
			Assert.AreEqual(m1.Layers[0].Projection,
			                m2.Layers[0].Projection);
		}
		
		[Test]
		public void TestLayerThemeSerialized()
		{
			Assert.AreEqual(ThemeType.NumericRange,
			                m2.Layers[0].Theme);
		}
		
		[Test]
		public void TestLayerThemeFieldSerialized()
		{
			Assert.AreEqual("MyField",
			                m2.Layers[0].ThemeField);
		}
				
#endregion
		
#region test data source serialized

		[Test]
		public void TestRelativeToAbsoluteMapFilePath()
		{
			MapSerializer ms = new MapSerializer();
			Map m = ms.Deserialize("../../maps/mexico.xml");
			
			Assert.IsTrue(Path.IsPathRooted((m.Layers[0].Data as IFileFeatureSource).FilePath));
		}
		
		[Test]
		public void TestSimpleFeatureSourceSerialization()
		{
			Map m = new Map();
			Layer l = new Layer();
			
			SimpleFeatureSource sfs = new SimpleFeatureSource(FeatureType.Polygon);
			Polygon p = new Polygon();
			
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(0,5));
			r.Points.Add(new Point(5,5));
			r.Points.Add(new Point(5,0));
			r.Close();
			p.Rings.Add(r);
			
			Ring hole = new Ring();
			hole.Points.Add(new Point(1,1));
			hole.Points.Add(new Point(2,1));
			hole.Points.Add(new Point(2,2));
			hole.Points.Add(new Point(2,1));
			hole.Close();
			p.Rings.Add(hole);
			
			sfs.Features.Add(p);
			
			l.Data = sfs;
			m.Layers.Add(l);
			
			MapSerializer ms = new MapSerializer();
			string s = MapSerializer.Serialize(m);
			
			Map m2 = ms.Deserialize(new MemoryStream(UTF8Encoding.UTF8.GetBytes((s))));	
			
			Assert.AreEqual(1, m2.Layers.Count);
			Assert.AreEqual(1, (m2.Layers[0].Data as SimpleFeatureSource).Features.Count);
			Assert.AreEqual(2, ((m2.Layers[0].Data as SimpleFeatureSource).Features[0] as Polygon).Rings.Count);
		}
		
		[Test]
		public void TestFileFeatureSourceFilePathSerialized()
		{
			Assert.AreEqual((m1.Layers[1].Data as IFileFeatureSource).FilePath,
			                (m2.Layers[1].Data as IFileFeatureSource).FilePath);
		}

		[Test]
		public void TestDatabaseFeatureSourceTableNameSerialized()
		{
			Assert.AreEqual((m1.Layers[0].Data as IDatabaseFeatureSource).TableName,
			                (m2.Layers[0].Data as IDatabaseFeatureSource).TableName);
		}

		[Test]
		public void TestDatabaseFeatureSourceConnectionStringSerialized()
		{
			Assert.AreEqual((m1.Layers[0].Data as IDatabaseFeatureSource).ConnectionString,
			                (m2.Layers[0].Data as IDatabaseFeatureSource).ConnectionString);
		}
		
		[Test]
		public void TestDatabaseFeatureSourceForcedSridSerialized()
		{
			Assert.AreEqual((m1.Layers[0].Data as IDatabaseFeatureSource).ForcedSrid,
			                (m2.Layers[0].Data as IDatabaseFeatureSource).ForcedSrid);
		}

		[Test]
		public void TestDatabaseFeatureSourceForcedFeatureTypeSerialized()
		{
			Assert.AreEqual((m1.Layers[0].Data as IDatabaseFeatureSource).ForcedFeatureType,
			                (m2.Layers[0].Data as IDatabaseFeatureSource).ForcedFeatureType);
		}

		[Test]
		public void TestDatabaseFeatureSourceForcedSpatialTypeSerialized()
		{
			Assert.AreEqual((m1.Layers[0].Data as IDatabaseFeatureSource).ForcedSpatialType,
			                (m2.Layers[0].Data as IDatabaseFeatureSource).ForcedSpatialType);
		}

		[Test]
		public void TestDatabaseFeatureSourceForcedGeometryColumnSerialized()
		{
			Assert.AreEqual((m1.Layers[0].Data as IDatabaseFeatureSource).ForcedGeometryColumn,
			                (m2.Layers[0].Data as IDatabaseFeatureSource).ForcedGeometryColumn);
		}
		
#endregion
				
#region test style serialized
		
		[Test]
		public void TestStyleLineColorSerialized()
		{
			Assert.AreEqual(m1.Layers[0].Styles[0].LineColor,
			                m2.Layers[0].Styles[0].LineColor);
		}

		[Test]
		public void TestStyleLineWidthSerialized()
		{
			Assert.AreEqual(m1.Layers[0].Styles[0].LineWidth,
			                m2.Layers[0].Styles[0].LineWidth);
		}

		[Test]
		public void TestStyleLineStyleSerialized()
		{
			Assert.AreEqual(LineStyle.Dashed, 
			                m2.Layers[0].Styles[0].LineStyle);
			Assert.AreEqual(m1.Layers[0].Styles[0].LineStyle,
			                m2.Layers[0].Styles[0].LineStyle);
		}
		
		[Test]
		public void TestStylePointSizeSerialized()
		{
			Assert.AreEqual(m1.Layers[0].Styles[0].PointSize,
			                m2.Layers[0].Styles[0].PointSize);
		}
		
		[Test]
		public void TestStyleUniqueThemeValueSerialized()
		{
			Assert.AreEqual("MyValue",
			                m2.Layers[0].Styles[0].UniqueThemeValue);
		}
		
		[Test]
		public void TestStyleMaxRangeThemeValueSerialized()
		{
			Assert.AreEqual(30000,
			                m2.Layers[0].Styles[0].MaxRangeThemeValue);
		}
		
		[Test]
		public void TestStyleMinRangeThemeValueSerialized()
		{
			Assert.AreEqual(4,
			                m2.Layers[0].Styles[0].MinRangeThemeValue);
		}

		
		[Test]
		public void TestStylePointSymbolSerialized()
		{
			Assert.AreEqual(PointSymbolType.Image,
			                m2.Layers[0].Styles[0].PointSymbol);
			Assert.AreEqual(m1.Layers[0].Styles[0].PointSymbol,
			                m2.Layers[0].Styles[0].PointSymbol);
		}
		
		[Test]
		public void TestStylePointSymbolShapeSerialized()
		{
			Assert.AreEqual(PointSymbolShapeType.Square,
			                m2.Layers[0].Styles[0].PointSymbolShape);
			Assert.AreEqual(m1.Layers[0].Styles[0].PointSymbolShape,
			                m2.Layers[0].Styles[0].PointSymbolShape);
		}
		
		[Test]
		public void TestRelativeToAbsoluteStylePointSymbolImagePath()
		{
			MapSerializer ms = new MapSerializer();
			Map m = ms.Deserialize("../../maps/mexico.xml");
			
			Assert.IsTrue(Path.IsPathRooted(m.Layers[4].Styles[2].PointSymbolImagePath));
		}

#endregion
		
	}
}
