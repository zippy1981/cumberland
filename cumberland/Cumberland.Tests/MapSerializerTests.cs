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

			public List<Feature> GetFeatures (string themeField, string labelField)
			{
				throw new System.NotImplementedException();
			}
			
			public List<Feature> GetFeatures (Rectangle rectangle, string themeField, string labelField)
			{
				throw new System.NotImplementedException();
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

			public List<Feature> GetFeatures (string themeField, string labelField)
			{
				throw new System.NotImplementedException();
			}
			
			public List<Feature> GetFeatures (Rectangle rectangle, string themeField, string labelField)
			{
				throw new System.NotImplementedException();
			}

		}
		
#endregion
	
#region test fixture set up
		
		[TestFixtureSetUp]
		public void SetUp()
		{
			MapSerializer ms = new MapSerializer();
			ms.AddDatabaseFeatureSourceType<DummyDBProvider>();
			ms.AddFileFeatureSourceType<DummyFileProvider>();
			
			m1 = new Map();
			m1.Extents = new Rectangle(0,0,10,10);
			m1.Height = 123;
			m1.Width = 321;
			m1.Projection = "+init=epsg:4326";
			m1.BackgroundColor = Color.FromArgb(4,3,2,1);
			
			Layer l1 = new Layer();
			l1.Id = "l1";
			DummyDBProvider db = new DummyDBProvider();
			l1.Data = db;
			l1.Projection = "+init=epsg:2236";
			l1.Theme = ThemeType.NumericRange;
			l1.ThemeField = "MyField";
			l1.LabelField = "MyLabelField";
			l1.Visible = false;
			l1.MinScale = 99;
			l1.MaxScale = 88;
            l1.AllowDuplicateLabels = false;
			
			db.ConnectionString = "MyConnString";
			db.ForcedFeatureType = FeatureType.Polyline;
			db.ForcedSpatialType = SpatialType.Geographic;
			db.ForcedSrid = 1234;
			db.TableName = "MyTable";
			db.ForcedGeometryColumn = "MyGeoColumn";
			
			Style s1 = new Style();
			s1.Id = "MyStyle";
			s1.LineColor = Color.FromArgb(255, 180, 34, 34);
			s1.LineStyle = LineStyle.Dashed;
			s1.LineWidth = 23;
			s1.PointSize = 4;
			s1.PointSymbol = PointSymbolType.Image;
			s1.PointSymbolShape = PointSymbolShapeType.Square;
			s1.UniqueThemeValue = "MyValue";
			s1.MaxRangeThemeValue = 30000;
			s1.MinRangeThemeValue = 4;
			s1.FillStyle = FillStyle.None;
			s1.ShowLabels = true;
			s1.LabelColor = Color.FromArgb(0,1,2,3);
			s1.LabelFont = LabelFont.SansSerif;
			s1.LabelFontEmSize = 1234;
			s1.LabelPosition = LabelPosition.BottomLeft;
			s1.LabelPixelOffset = 42;
			s1.LabelDecoration = LabelDecoration.Outline;
			s1.LabelOutlineColor = Color.FromArgb(9,9,9,9);
			s1.LabelOutlineWidth = 99f;
			s1.LabelAngle = 45f;
			s1.LabelCustomFont = "font";

			s1.MinScale = 0;
			s1.MaxScale = 1;
			s1.LabelMinScale = 10;
			s1.LabelMaxScale = 100;
			s1.DrawPointSymbolOnPolyLine = true;
			s1.CalculateLabelAngleForPolyLine = false;
			s1.FillTexturePath = "../../../Cumberland.Tests/maps/images/swamps.png";
			
			s1.Simplify = true;
			s1.SimplifyTolerance = 99;
			
			s1.UniqueElseFlag = true;
			
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
//		
//		[Test, ExpectedException(typeof(NotSupportedException))]
//		public void TestMapVersionFails()
//		{
//			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Map version=\"bad\">	<Width>500</Width>	<Height>400</Height>	<Extents>-1,-4,10,10</Extents>	<Projection>+init=epsg:4326</Projection>	<Layers /></Map>";
//			
//			MapSerializer ms = new MapSerializer();
//			ms.Deserialize(new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
//		}

		[Test]
		public void TestMapBackgroundColor()
		{
			Assert.AreEqual(Color.FromArgb(4,3,2,1), m2.BackgroundColor);
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

		[Test]
		public void TestLayerLabelFieldSerialized()
		{
			Assert.AreEqual("MyLabelField",
			                m2.Layers[0].LabelField);
		}

		[Test]
		public void TestLayerVisible()
		{
			Assert.AreEqual(false, m2.Layers[0].Visible);
		}

		[Test]
		public void TestLayerMinScale()
		{
			Assert.AreEqual(99, m2.Layers[0].MinScale);
		}

		[Test]
		public void TestLayerMaxScale()
		{
			Assert.AreEqual(88, m2.Layers[0].MaxScale);
		}

        [Test]
        public void TestLayerAllowDuplicateLabels()
        {
            Assert.IsFalse(m2.Layers[0].AllowDuplicateLabels);
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
			
			Assert.IsTrue(Path.IsPathRooted(m.Layers[5].Styles[2].PointSymbolImagePath));
		}

		[Test]
		public void TestStyleIdSerialized()
		{
			Assert.AreEqual("MyStyle", m2.Layers[0].Styles[0].Id);
		}

		[Test]
		public void TestStyleFillStyle()
		{
			Assert.AreEqual(FillStyle.None, m2.Layers[0].Styles[0].FillStyle);
		}

		[Test]
		public void TestStyleShowLabels()
		{
			Assert.AreEqual(true, m2.Layers[0].Styles[0].ShowLabels);
		}

		[Test]
		public void TestStyleLabelFont()
		{
			Assert.AreEqual(LabelFont.SansSerif, m2.Layers[0].Styles[0].LabelFont);
		}

		[Test]
		public void TestStyleLabelColor()
		{
			Assert.AreEqual(Color.FromArgb(0,1,2,3), m2.Layers[0].Styles[0].LabelColor);
		}

		[Test]
		public void TestStyleLabelFontEmSize()
		{
			Assert.AreEqual(1234, m2.Layers[0].Styles[0].LabelFontEmSize);
		}

		[Test]
		public void TestStyleLabelPosition()
		{
			Assert.AreEqual(LabelPosition.BottomLeft, m2.Layers[0].Styles[0].LabelPosition);
		}

		[Test]
		public void TestStyleLabelPixelOffset()
		{
			Assert.AreEqual(42, m2.Layers[0].Styles[0].LabelPixelOffset);
		}

		[Test]
		public void TestStyleLabelDecoration()
		{
			Assert.AreEqual(LabelDecoration.Outline, m2.Layers[0].Styles[0].LabelDecoration);
		}

		[Test]
		public void TestStyleLabelOutlineColor()
		{
			Assert.AreEqual(Color.FromArgb(9,9,9,9), m2.Layers[0].Styles[0].LabelOutlineColor);
		}

		[Test]
		public void TestStyleLabelOutlineWidth()
		{
			Assert.AreEqual(99f, m2.Layers[0].Styles[0].LabelOutlineWidth);
		}

		[Test]
		public void TestStyleLabelAngleSerialized()
		{
			Assert.AreEqual(45f, m2.Layers[0].Styles[0].LabelAngle);
		}

		[Test]
		public void TestStyleMinScale()
		{
			Assert.AreEqual(0, m2.Layers[0].Styles[0].MinScale);
		}

		[Test]
		public void TestStyleMaxScale()
		{
			Assert.AreEqual(1, m2.Layers[0].Styles[0].MaxScale);
		}

		[Test]
		public void TestStyleLabelMinScale()
		{
			Assert.AreEqual(10, m2.Layers[0].Styles[0].LabelMinScale);
		}

		[Test]
		public void TestStyleLabelMaxScale()
		{
			Assert.AreEqual(100, m2.Layers[0].Styles[0].LabelMaxScale);
		}

		[Test]
		public void TestStyleLabelCustomFont()
		{
			Assert.AreEqual("font", m2.Layers[0].Styles[0].LabelCustomFont);
		}

		[Test]
		public void TestStyleDrawPointImageOnPolyLine()
		{
			Assert.AreEqual(true, m2.Layers[0].Styles[0].DrawPointSymbolOnPolyLine);
		}

		[Test]
		public void TestCalculateLabelAngleForPolyLine()
		{
			Assert.AreEqual(false, m2.Layers[0].Styles[0].CalculateLabelAngleForPolyLine);
		}

		[Test]
		public void TestFillTexturePath()
		{
			MapSerializer ms = new MapSerializer();
			Map m = ms.Deserialize("../../maps/mexico.xml");
			
			Assert.IsTrue(Path.IsPathRooted(m.Layers[4].Styles[0].FillTexturePath));
		}
		
		[Test]
		public void TestStyleSimplifySerialized()
		{
			Assert.AreEqual(true, m2.Layers[0].Styles[0].Simplify);
		}
		
		[Test]
		public void TestStyleSimplifyToleranceSerialized()
		{
			Assert.AreEqual(99, m2.Layers[0].Styles[0].SimplifyTolerance);
		}
		
		[Test]
		public void TestStyleUniqueElseFlagSerialized()
		{
			Assert.AreEqual(true, m2.Layers[0].Styles[0].UniqueElseFlag);
		}
		
#endregion
		
	}
}
