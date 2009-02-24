// 
// WellKnownBinaryTests.cs
//  
// Author:
//       Scott Ellington <scott.ellington@gmail.com>
// 
// Copyright (c) 2009 Scott Ellington
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

using System;
using System.Text;

using NUnit.Framework;

using Cumberland.Data.SimpleFeatureAccess;

namespace Cumberland.Tests
{
	
	
	[TestFixture]
	public class WellKnownBinaryTests
	{
		
		[Test()]
		public void TestPoint()
		{
			string wkb = "0101000000000000000000F03F000000000000F03F";
			
			Assert.AreEqual(new Point(1,1),
			                WellKnownBinary.Parse(ToByteArray(wkb)));
		}
		
		[Test]
		public void TestPolygon()
		{
			string wkb = "0103000000010000000D000000FFFFFF3F3BD258C0F5FFFFFF76243340FFFFFF3F8DC858C00B000060B91D3340FEFFFF7F02C258C0FFFFFF3FB70F3340FDFFFFBFAFBD58C0030000A0CA163340020000801BBC58C0F4FFFF7F45233340000000A0EBBD58C0060000405D4E334000000060D0C158C00B0000C0495F33400400000007C258C0FEFFFF1F56743340FFFFFF3F75C558C000000000D0793340FEFFFF7F3CC758C0FBFFFFDF738A3340020000805DCE58C005000080E4673340FDFFFFBFCBD558C009000080975B3340FFFFFF3F3BD258C0F5FFFFFF76243340";
			
			Assert.AreEqual(13, ((Polygon) WellKnownBinary.Parse(ToByteArray(wkb))).Rings[0].Points.Count);
		}

		[Test]
		public void TestPolyLine()
		{
			string wkb = "010500000001000000010200000002000000070000A0FDDE5CC0FBFFFF7FF35440400B00006039DC5CC004000060DF4E4040";
			
			Assert.AreEqual(2, ((PolyLine) WellKnownBinary.Parse(ToByteArray(wkb))).Lines[0].Points.Count);
		}

		
		
		public static byte[] ToByteArray(String hexString)
		{
			int numberChars = hexString.Length;
			
			byte[] bytes = new byte[numberChars / 2];
			
			for (int i = 0; i < numberChars; i += 2)
			{
					bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			}
			
			return bytes;
		}
	}
}
