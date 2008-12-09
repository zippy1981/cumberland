// WellKnownTextTests.cs
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
using Cumberland;
using Cumberland.Data.WellKnownText;

using NUnit.Framework;

namespace Cumberland.Tests
{
	[TestFixture]
	public class WellKnownTextTests
	{
#region vars
		
		string pointWkt = "POINT(0 0)";
		string multiLineStringWkt = "MULTILINESTRING((0 0 0,1 1 0,1 2 1),(2 3 1,3 2 1,5 4 1))";		
		string multiPolygonWkt = "MULTIPOLYGON(((0 0 0,4 0 0,4 4 0,0 4 0,0 0 0),(1 1 0,2 1 0,2 2 0,1 2 0,1 1 0)),((-1 -1 0,-1 -2 0,-2 -2 0,-2 -1 0,-1 -1 0)))";
		string polygonWkt = "POLYGON((0 0,0 5,5 6,5 0,0 0),(1.1 1.1,1.6 1.1,1.4 1.4,1.1 1.6,1.1 1.1))";
		string lineStringWkt = "LINESTRING(0 0 0,1 1 0,1 2 1)";
		
#endregion
		
#region parse tests
		
		[Test]
		public void TestParsePoint()
		{
			Point p = SimpleFeatureAccess.ParsePoint(pointWkt);
			Assert.AreEqual(new Point(0,0), p);
		}
		
		[Test]
		public void TestParseMultiLineString()
		{
			PolyLine l = SimpleFeatureAccess.ParseMultiLineString(multiLineStringWkt);
			Assert.AreEqual(2, l.Lines.Count);
		}
		
		[Test]
		public void TestParseMultiPolygon()
		{
			Polygon p = SimpleFeatureAccess.ParseMultiPolygon(multiPolygonWkt);
			Assert.AreEqual(3, p.Rings.Count);
		}
		
		[Test]
		public void TestParsePolygon()
		{
			Polygon p = SimpleFeatureAccess.ParsePolygon(polygonWkt);
			Assert.AreEqual(2, p.Rings.Count);
		}
		
		[Test]
		public void TestParseLineString()
		{
			PolyLine l = SimpleFeatureAccess.ParseLineString(lineStringWkt);
			Assert.AreEqual(3, l.Lines[0].Points.Count);
		}
		
		[Test]
		public void TestParse()
		{
			Assert.AreEqual(typeof(Point), 
			                SimpleFeatureAccess.Parse(pointWkt).GetType());
			
			Assert.AreEqual(typeof(Polygon),
			                SimpleFeatureAccess.Parse(polygonWkt).GetType());
			
			Assert.AreEqual(typeof(Polygon),
			                SimpleFeatureAccess.Parse(multiPolygonWkt).GetType());
			
			Assert.AreEqual(typeof(PolyLine),
			                SimpleFeatureAccess.Parse(lineStringWkt).GetType());
			
			Assert.AreEqual(typeof(PolyLine),
			                SimpleFeatureAccess.Parse(multiLineStringWkt).GetType());
		}
		
#endregion
		
#region create tests
		
		[Test]
		public void TestCreateFromPolygon()
		{
			Polygon p = new Polygon();
			Ring r1 = new Ring();
			r1.Points.Add(new Point(0,0));
			r1.Points.Add(new Point(0, 5));
			r1.Points.Add(new Point(5,6));
			r1.Points.Add(new Point(5,0));
			r1.Close();
			p.Rings.Add(r1);
			
			Assert.AreEqual("POLYGON((0 0,0 5,5 6,5 0,0 0))", SimpleFeatureAccess.CreateFromPolygon(p));
			
			// add hole
			Ring r2 = new Ring();
			r2.Points.Add(new Point(1.1,1.1));
			r2.Points.Add(new Point(1.6,1.1));
			r2.Points.Add(new Point(1.4,1.4));
			r2.Points.Add(new Point(1.1,1.6));
			r2.Close();
			p.Rings.Add(r2);
			
			Assert.AreEqual("POLYGON((0 0,0 5,5 6,5 0,0 0),(1.1 1.1,1.6 1.1,1.4 1.4,1.1 1.6,1.1 1.1))", 
			                SimpleFeatureAccess.CreateFromPolygon(p));
			
			// add another poly
			Ring r3 = new Ring();
			r3.Points.Add(new Point(10,10));
			r3.Points.Add(new Point(10,15));
			r3.Points.Add(new Point(15,15));
			r3.Points.Add(new Point(15,10));
			r3.Close();
			p.Rings.Add(r3);
			
			Assert.AreEqual("MULTIPOLYGON(((0 0,0 5,5 6,5 0,0 0),(1.1 1.1,1.6 1.1,1.4 1.4,1.1 1.6,1.1 1.1)),((10 10,10 15,15 15,15 10,10 10)))", 
			                SimpleFeatureAccess.CreateFromPolygon(p));
		}
		
		[Test]
		public void TestCreateFromPolyLine()
		{
			PolyLine p = new PolyLine();
			Line l1 = new Line();
			l1.Points.Add(new Point(0,0));
			l1.Points.Add(new Point(5,0));
			l1.Points.Add(new Point(5,6));
			l1.Points.Add(new Point(0,5));
			p.Lines.Add(l1);
			
			Assert.AreEqual("MULTILINESTRING((0 0,5 0,5 6,0 5))", SimpleFeatureAccess.CreateFromPolyLine(p));
			
			Line l2 = new Line();
			l2.Points.Add(new Point(1.1,1.1));
			l2.Points.Add(new Point(1.6,1.1));
			l2.Points.Add(new Point(1.4,1.4));
			l2.Points.Add(new Point(1.6,1.1));
			p.Lines.Add(l2);
			
			Assert.AreEqual("MULTILINESTRING((0 0,5 0,5 6,0 5),(1.1 1.1,1.6 1.1,1.4 1.4,1.6 1.1))", 
			                SimpleFeatureAccess.CreateFromPolyLine(p));
		}
		
		[Test]
		public void TestCreateFromPoint()
		{
			Point p = new Point(3,4);
			Assert.AreEqual("POINT(3 4)", SimpleFeatureAccess.CreateFromPoint(p));
		}
		
		[Test]
		public void TestCreateFromRectangle()
		{
			Rectangle r = new Rectangle(0,0,15,15);
			
			Assert.AreEqual("POLYGON((0 0,0 15,15 15,15 0,0 0))",
			                SimpleFeatureAccess.CreateFromRectangle(r));
		}

#endregion
	}
}
