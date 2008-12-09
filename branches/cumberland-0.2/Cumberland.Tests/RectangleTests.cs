// RectangleTests.cs
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
using Cumberland;
using NUnit.Framework;

namespace Cumberland.Tests
{
	[TestFixture]	
	public class RectangleTests
	{
		[Test]
		public void TestUnionAndEquals()
		{
			Rectangle r1 = new Rectangle(0, 0, 10, 10);
			Rectangle r2 = new Rectangle(-5, 5, 5, 15);
			
			// simple math 
			// TODO: should test for more union cases
			Assert.AreEqual(new Rectangle(-5,0,10,15), Rectangle.Union(r1,r2));	
		}
		
		[Test]
		public void TestIsEmpty()
		{
			Rectangle r = new Rectangle(5, 5, 0, 0);
			
			Assert.IsTrue(r.IsEmpty);
		}
		
		[Test]
		public void TestWidthGetter()
		{
			Rectangle r = new Rectangle(-5,-5, 5, 5);
			
			// simple math
			Assert.AreEqual(10, r.Width);
		}
		
		[Test]
		public void TestHeightGetter()
		{
			Rectangle r = new Rectangle(-5,-5, 5, 5);
			
			// simple math
			Assert.AreEqual(10, r.Height);
		}
		
		[Test]
		public void TestCenterGetter()
		{
			Rectangle r = new Rectangle(-5,-5, 5, 5);
			
			// simple math
			Assert.AreEqual(new Point(0,0), r.Center);
		}
		
		[Test]
		public void TestAspectRatioOfWidthGetter()
		{
			Rectangle r = new Rectangle(0,0,10,5);
			
			// simple math
			Assert.AreEqual(2, r.AspectRatioOfWidth);
		}
		
		[Test]
		public void TestCenterSetter()
		{
			Rectangle r = new Rectangle(0,0,2,2); // center is 1,1
			
			r.Center = new Point(2,2);
			
			Assert.AreEqual(new Rectangle(1,1,3,3), r);
		}
		
		[Test]
		public void TestClone()
		{
			// ensure a deep copy
			Rectangle r = new Rectangle(0,0,2,2);
			
			Rectangle r2 = r.Clone();
			
			r.Max.X = 4;
			
			Assert.IsFalse(r == r2);
		}
		
		[Test]
		public void TestWidthSetter()
		{
			Rectangle r = new Rectangle(1,1,3,3);
			
			r.Width = 4;
			
			Assert.AreEqual(new Rectangle(0,1,4,3), r);
		}
		
		[Test]
		public void TestHeightSetter()
		{
			Rectangle r = new Rectangle(1,1,3,3);
			
			r.Height = 4;
			
			Assert.AreEqual(new Rectangle(1,0,3,4), r);
		}
		
		[Test] 
		public void TestAreaGetter()
		{
			Rectangle r = new Rectangle(0,0,2,2);
			
			Assert.AreEqual(4, r.Area);
		}
		
		[Test]
		public void TestUnionWithEmpty()
		{
			Rectangle r = new Rectangle();
			
			Rectangle r2 = new Rectangle(0,0,10,10);
			
			Assert.AreEqual(r2, Rectangle.Union(r,r2));
		}
		
		[Test]
		public void TestOverlaps()
		{
			Rectangle r1 = new Rectangle(-4,-4,-2,-2);
			Rectangle r2 = new Rectangle(0,0,2,2);
			
			// bottom-left
			Assert.IsFalse(r1.Overlaps(r2));
			
			//left
			r1.Min.Y = 0;
			r1.Max.Y = 2;
			Assert.IsFalse(r1.Overlaps(r2));
			
			// left overlapping
			r1.Max.X = 1;
			Assert.IsTrue(r1.Overlaps(r2));
			
			//top-left
			r1.Max.X = -2;
			r1.Min.Y = 3;
			r1.Max.Y = 5;
			Assert.IsFalse(r1.Overlaps(r2));
			
			//top
			r1.Min.X = 0;
			r1.Max.X = 2;
			Assert.IsFalse(r1.Overlaps(r2));
			
			// top-overlaps
			r1.Min.Y = 1;
			Assert.IsTrue(r1.Overlaps(r2));
			
			// top right
			r1.Min.Y = 3;
			r1.Min.X = 3;
			r1.Max.X = 5;
			Assert.IsFalse(r1.Overlaps(r2));
			
			// right
			r1.Min.Y = 0;
			r1.Max.Y = 2;
			Assert.IsFalse(r1.Overlaps(r2));
			
			// right-overlaps
			r1.Min.X = 1;
			Assert.IsTrue(r1.Overlaps(r2));
			
			//bottom-right
			r1.Min.X = 3;
			r1.Min.Y = -5;
			r1.Max.Y = -1;
			Assert.IsFalse(r1.Overlaps(r2));
			
			// bottom 
			r1.Min.X = 0;
			r1.Max.X = 2;
			Assert.IsFalse(r1.Overlaps(r2));
			
			// bottom overlaps
			r1.Max.Y = 1;
			Assert.IsTrue(r1.Overlaps(r2));
			
			// equal
			Assert.IsTrue(r2.Overlaps(r2.Clone()));
			
			// within
			r1 = new Rectangle(.5, .5, 1.5, 1.5);
			Assert.IsTrue(r1.Overlaps(r2));
			
			// overlaps empty
			Assert.IsFalse(r1.Overlaps(new Rectangle()));
		}
		
		[Test]
		public void TestOpEqualityWithNull()
		{
			Rectangle r1 = new Rectangle(0,0,10,10);
			
			Assert.IsFalse(r1 == null);
			Assert.IsFalse(null == r1);
		}

		[Test]
		public void TestEquals()
		{
			Rectangle r1 = new Rectangle(0,0,1,1);
			Rectangle r2 = new Rectangle(0,0,1,1);

			Assert.IsTrue(r1.Equals(r2));
			Assert.IsTrue(r1.Equals((object)r2));
		}
	}
}
