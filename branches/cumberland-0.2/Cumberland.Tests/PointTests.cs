// PointTests.cs
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
	public class PointTests
	{		
		[Test]
		public void TestEqualityOperator()
		{
			Point p1 = new Point(1,1);
			Point p2 = new Point(1, 1);
			
			Assert.IsTrue(p1 == p2);
		}
		
		[Test]
		public void TestInequalityOperator()
		{
			Point p1 = new Point(1,1);
			Point p2 = new Point(1,3);
			
			Assert.IsTrue(p1 != p2);
		}
		
		[Test]
		public void TestEqualsMethod()
		{
			Point p1 = new Point(1,1);
			Point p2 = new Point(1,1);
			
			Assert.AreEqual(p1, p2);
			Assert.IsTrue(p1.Equals(p2));
			Assert.IsTrue(p1.Equals((object)p2));
		}
		
		[Test]
		public void TestCalculateBounds()
		{
			Point p = new Point(1,1);
			
			Assert.AreEqual(new Rectangle(p.X,p.Y,p.X,p.Y), p.CalculateBounds());
		}
		
		[Test]
		public void TestOpEqualityWithNull()
		{
			Point p1 = new Point(10,10);
			
			Assert.IsFalse(p1 == null);
			Assert.IsFalse(null == p1);
		}
	}
}
