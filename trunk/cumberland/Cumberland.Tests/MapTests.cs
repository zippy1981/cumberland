// MapTests.cs
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
using NUnit.Framework;

using Cumberland;
using Cumberland.Data;

namespace Cumberland.Tests
{
	[TestFixture()]
	public class MapTests
	{
		
//		[Test()]
//		public void TestAddLayerByFeatureTypeAndExtentsWithPointBeforePolygon()
//		{
//			Map m = new Map();
//			m.Layers.Add(CreateLayer(FeatureType.Polygon));
//			
//			Assert.AreEqual(1,m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Point)));
//		}
//
//		[Test()]
//		public void TestAddLayerByFeatureTypeAndExtentsWithPointBeforePolyline()
//		{
//			Map m = new Map();
//			m.Layers.Add(CreateLayer(FeatureType.Polyline));
//			
//			Assert.AreEqual(1,m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Point)));
//		}
//		
//		[Test()]
//		public void TestAddLayerByFeatureTypeAndExtentsWithPolylineAfterPointAndBeforePolygon()
//		{
//			Map m = new Map();
//			m.Layers.Add(CreateLayer(FeatureType.Polygon));
//			m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Point));
//			
//			Assert.AreEqual(1, m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Polyline)));
//		}
//
//		[Test()]
//		public void TestAddLayerByFeatureTypeAndExtentsWithPolygonAfterPointAndPolyline()
//		{
//			Map m = new Map();
//			m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Polyline));
//			m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Point));
//			
//			Assert.AreEqual(0, m.AddLayerByFeatureTypeAndExtents(CreateLayer(FeatureType.Polygon)));
//		}
//		
//		[Test()]
//		public void TestAddLayerByFeatureTypeAndExtentsWithFeatureExtents()
//		{
//			Layer bigger = CreateLayer(FeatureType.Polyline);
//			PolyLine pl = new PolyLine();
//			Line l = new Line();
//			l.Points.Add(new Point(0,0));
//			l.Points.Add(new Point(10,10));
//			pl.Lines.Add(l);
//			(bigger.Data as SimpleFeatureSource).Features.Add(pl);
//			
//			Layer smaller = CreateLayer(FeatureType.Polyline);
//			PolyLine pl2 = new PolyLine();
//			Line l2 = new Line();
//			l2.Points.Add(new Point(0,0));
//			l2.Points.Add(new Point(10,10));
//			pl2.Lines.Add(l2);
//			(smaller.Data as SimpleFeatureSource).Features.Add(pl2);
//			
//			Map m = new Map();
//			
//			m.AddLayerByFeatureTypeAndExtents(bigger);
//			
//			Assert.AreEqual(1, m.AddLayerByFeatureTypeAndExtents(smaller));
//		}
//			
//		
//		Layer CreateLayer(FeatureType ft)
//		{
//			Layer l = new Layer();
//			l.Data = new SimpleFeatureSource(ft);
//			return l;
//		}
	}
}
