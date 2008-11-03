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

using NUnit.Framework;

namespace Cumberland.Tests
{
	[TestFixture]
	public class WellKnownTextTests
	{
		[Test]
		public void TestWKTToPoint()
		{
			string wkt = "POINT(0 0)";
			
			Point p = WellKnownText.ParsePoint(wkt);
			Assert.AreEqual(new Point(0,0), p);
		}
		
		[Test]
		public void TestWKTToPolyLine()
		{
			string wkt = "MULTILINESTRING((0 0 0,1 1 0,1 2 1),(2 3 1,3 2 1,5 4 1))";
			
			PolyLine l = WellKnownText.ParseMultiLineString(wkt);
			Assert.AreEqual(2, l.Lines.Count);
		}
		
		[Test]
		public void TestWKTPolygons()
		{
			string wkt = "MULTIPOLYGON(((0 0 0,4 0 0,4 4 0,0 4 0,0 0 0),(1 1 0,2 1 0,2 2 0,1 2 0,1 1 0)),((-1 -1 0,-1 -2 0,-2 -2 0,-2 -1 0,-1 -1 0)))";
			
			List<Feature> l = WellKnownText.ParseMultiPolygon(wkt);
			Assert.AreEqual(2, l.Count);
		}
		
		[Test]
		public void TestPolygonToWKT()
		{
			Polygon p = new Polygon();
			Ring r1 = new Ring();
			r1.Points.Add(new Point(0,0));
			r1.Points.Add(new Point(5,0));
			r1.Points.Add(new Point(5,6));
			r1.Points.Add(new Point(0,5));
			r1.Close();
			p.Rings.Add(r1);
			
			Assert.AreEqual("POLYGON((0 0,5 0,5 6,0 5,0 0))", WellKnownText.CreateFromPolygon(p));
			
			Ring r2 = new Ring();
			r2.Points.Add(new Point(1.1,1.1));
			r2.Points.Add(new Point(1.6,1.1));
			r2.Points.Add(new Point(1.4,1.4));
			r2.Points.Add(new Point(1.6,1.1));
			r2.Close();
			p.Rings.Add(r2);
			
			Assert.AreEqual("POLYGON((0 0,5 0,5 6,0 5,0 0)(1.1 1.1,1.6 1.1,1.4 1.4,1.6 1.1,1.1 1.1))", 
			                WellKnownText.CreateFromPolygon(p));
		}
		
		[Test]
		public void TestPolyLineToWKT()
		{
			PolyLine p = new PolyLine();
			Line l1 = new Line();
			l1.Points.Add(new Point(0,0));
			l1.Points.Add(new Point(5,0));
			l1.Points.Add(new Point(5,6));
			l1.Points.Add(new Point(0,5));
			p.Lines.Add(l1);
			
			Assert.AreEqual("MULTILINESTRING((0 0,5 0,5 6,0 5))", WellKnownText.CreateFromPolyLine(p));
			
			Line l2 = new Line();
			l2.Points.Add(new Point(1.1,1.1));
			l2.Points.Add(new Point(1.6,1.1));
			l2.Points.Add(new Point(1.4,1.4));
			l2.Points.Add(new Point(1.6,1.1));
			p.Lines.Add(l2);
			
			Assert.AreEqual("MULTILINESTRING((0 0,5 0,5 6,0 5)(1.1 1.1,1.6 1.1,1.4 1.4,1.6 1.1))", 
			                WellKnownText.CreateFromPolyLine(p));
		}
		
		public void TestPointToWKT()
		{
			Point p = new Point(3,4);
			Assert.AreEqual("POINT(3 4)", WellKnownText.CreateFromPoint(p));
		}
	}
}
