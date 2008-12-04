// TileProviderTests.cs
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

using Cumberland;
using Cumberland.Web;

using System;
using NUnit.Framework;

namespace Cumberland.Tests
{
	[TestFixture()]
	public class TileProviderTests
	{
		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestMinZoomLevel()
		{
			TileProvider t = new TileProvider(TileConsumer.GoogleMaps);
			
			t.CalculateNumberOfTilesAcross(-1);
		}
		
		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TestMaxZoomLevel()
		{
			TileProvider t = new TileProvider(TileConsumer.GoogleMaps);
			
			t.CalculateNumberOfTilesAcross(20);
		}

		[Test]
		public void TestConvertTileToQuadKey()
		{
			TileProvider t = new TileProvider(TileConsumer.VirtualEarth);
			Assert.AreEqual("213", t.ConvertTileToQuadKey(3, 5, 3));
		}
	}
}
