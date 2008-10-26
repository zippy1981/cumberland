// RingTests.cs
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

namespace Cumberland.Tests
{
	[TestFixture()]
	public class RingTests
	{
		[Test()]
		public void TestIsClosed()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(1,0));
			r.Points.Add(new Point(1,1));
			r.Points.Add(new Point(0,1));
			r.Points.Add(new Point(0,0));
			
			Assert.IsTrue(r.IsClosed);
		}

		
		[Test()]
		public void TestNotIsClosed()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(1,0));
			r.Points.Add(new Point(1,1));
			r.Points.Add(new Point(0,1));
			          
			Assert.IsFalse(r.IsClosed);
		}

		
		[Test()]
		public void TestIsClockwise()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(0,1));
			r.Points.Add(new Point(1,1));
			r.Points.Add(new Point(1,0));
			r.Close();
			          
			Assert.IsTrue(r.IsClockwise);
		}


		[Test()]
		public void TestNotIsClockwise()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(1,0));
			r.Points.Add(new Point(1,1));
			r.Points.Add(new Point(0,1));
			r.Close();
			          
			Assert.IsFalse(r.IsClockwise);
		}
		
		[Test()]
		public void TestCalculateArea()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(0,1));
			r.Points.Add(new Point(1,1));
			r.Points.Add(new Point(1,0));
			r.Close();
			          
			Assert.AreEqual(1, r.CalculateArea());
		}
		
		[Test()]
		public void TestCalculateBounds()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0.5,0));
			r.Points.Add(new Point(0,0.5));
			r.Points.Add(new Point(0.5,1));
			r.Points.Add(new Point(1,0.5));
			r.Close();
			          
			Assert.AreEqual(new Rectangle(0,0,1,1), r.CalculateBounds());
		}

		[Test()]
		public void TestClose()
		{
			Ring r = new Ring();
			r.Points.Add(new Point(0,0));
			r.Points.Add(new Point(0,1));
			r.Points.Add(new Point(1,1));
			r.Points.Add(new Point(1,0));

			Assert.IsFalse(r.IsClosed);
			
			r.Close();
			          
			Assert.IsTrue(r.IsClosed);
		}
		
				[Test()]
		public void TestValidate()
		{
			Ring r = new Ring();
			          
			Assert.IsFalse(r.Validate());
		}
	}
}
