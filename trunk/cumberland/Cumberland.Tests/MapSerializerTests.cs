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
		
		class DummyFileProvider : IFileFeatureSource, IFeatureSource
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
				return GetFeatures(null);
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures (Rectangle rectangle)
			{
				return new List<Feature>();
			}
		}
		
		class DummyDBProvider : IDatabaseFeatureSource, IFeatureSource
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
			
			public FeatureType SourceFeatureType {
				get {
					return FeatureType.Polyline;
				}
			}
	
			public Rectangle Extents {
				get {
					return new Rectangle(0,0,30,30);
				}
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures (Rectangle rectangle)
			{
				return new List<Feature>();
			}
			
			public System.Collections.Generic.List<Feature> GetFeatures ()
			{
				return GetFeatures(null);
			}

		}
		
#endregion
		
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
			l1.Data = new DummyDBProvider();
			l1.LineColor = Color.FromArgb(255, 180, 34, 34);
			l1.LineStyle = LineStyle.Dashed;
			l1.LineWidth = 23;
			l1.PointSize = 4;
			l1.Projection = "+init=epsg:2236";
			l1.PointSymbol = PointSymbolType.Image;
			l1.PointSymbolShape = PointSymbolShapeType.Square;
			m1.Layers.Add(l1);
			
			Layer l2 = new Layer();
			l2.Id = "l2";
			l2.Data = new DummyFileProvider();
			m1.Layers.Add(l2);
			
			string s = MapSerializer.Serialize(m1);
			
			m2 = ms.Deserialize(new MemoryStream(ASCIIEncoding.Default.GetBytes((s))));
		}
		
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
			m2 = ms.Deserialize(new MemoryStream(ASCIIEncoding.Default.GetBytes((x))));
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
			m2 = ms.Deserialize(new MemoryStream(ASCIIEncoding.Default.GetBytes((x))));
		}

		[Test]
		public void TestLayerOrderSerializedCorrectlyAndId()
		{
			Assert.AreEqual(m1.Layers[0].Id, m2.Layers[0].Id);
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
		public void TestLayerLineColorSerialized()
		{
			Assert.AreEqual(m1.Layers[0].LineColor,
			                m2.Layers[0].LineColor);
		}

		[Test]
		public void TestLayerLineWidthSerialized()
		{
			Assert.AreEqual(m1.Layers[0].LineWidth,
			                m2.Layers[0].LineWidth);
		}

		[Test]
		public void TestLayerLineStyleSerialized()
		{
			Assert.AreEqual(LineStyle.Dashed, 
			                m2.Layers[0].LineStyle);
			Assert.AreEqual(m1.Layers[0].LineStyle,
			                m2.Layers[0].LineStyle);
		}
		
		[Test]
		public void TestLayerPointSizeSerialized()
		{
			Assert.AreEqual(m1.Layers[0].PointSize,
			                m2.Layers[0].PointSize);
		}
		
		[Test]
		public void TestLayerProjectionSerialized()
		{
			Assert.AreEqual(m1.Layers[0].Projection,
			                m2.Layers[0].Projection);
		}
		
		[Test]
		public void TestRelativeToAbsoluteMapFilePath()
		{
			MapSerializer ms = new MapSerializer();
			Map m = ms.Deserialize("../../maps/mexico.xml");
			
			Assert.IsTrue(Path.IsPathRooted((m.Layers[0].Data as IFileFeatureSource).FilePath));
		}
		
		[Test]
		public void TestLayerPointSymbolSerialized()
		{
			Assert.AreEqual(PointSymbolType.Image,
			                m2.Layers[0].PointSymbol);
			Assert.AreEqual(m1.Layers[0].PointSymbol,
			                m2.Layers[0].PointSymbol);
		}
		
		[Test]
		public void TestLayerPointSymbolShapeSerialized()
		{
			Assert.AreEqual(PointSymbolShapeType.Square,
			                m2.Layers[0].PointSymbolShape);
			Assert.AreEqual(m1.Layers[0].PointSymbolShape,
			                m2.Layers[0].PointSymbolShape);
		}
		
		[Test]
		public void TestRelativeToAbsolutePointSymbolImagePath()
		{
			MapSerializer ms = new MapSerializer();
			Map m = ms.Deserialize("../../maps/mexico.xml");
			
			Assert.IsTrue(Path.IsPathRooted(m.Layers[2].PointSymbolImagePath));
		}
		
	}
}
