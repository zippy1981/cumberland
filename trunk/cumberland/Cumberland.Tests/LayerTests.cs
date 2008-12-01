// LayerTests.cs
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
	public class LayerTests
	{
		[Test()]
		public void TestGetRangeStyleForFeature()
		{
			Layer l = new Layer();
			l.Theme = ThemeType.NumericRange;
			Style s = new Style();
			s.MaxRangeThemeValue = 4;
			s.MinRangeThemeValue = 3;
			s.Id = "MyStyle";
			l.Styles.Add(s);

			Assert.AreEqual("MyStyle", l.GetRangeStyleForFeature("3").Id);
			Assert.IsNull(l.GetRangeStyleForFeature("1"));
			Assert.IsNull(l.GetRangeStyleForFeature("NAN"));
			Assert.IsNull(l.GetRangeStyleForFeature("5"));
		}

		[Test()]
		public void TestGetUniqueStyleForFeature()
		{
			Layer l = new Layer();
			l.Theme = ThemeType.Unique;
			Style s = new Style();
			s.UniqueThemeValue = "MyValue";
			s.Id = "MyStyle";
			l.Styles.Add(s);

			Assert.AreEqual("MyStyle", l.GetUniqueStyleForFeature("MyValue").Id);
			Assert.IsNull(l.GetUniqueStyleForFeature("Nope"));
		}
	}
}
