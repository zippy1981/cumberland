// DBaseIIITests.cs
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
using System.Data;

using NUnit.Framework;

using Cumberland.Data.Shapefile;

namespace Cumberland.Tests
{
	[TestFixture()]
	public class DBaseIIITests
	{
		[Test()]
		public void TestFile()
		{
			DBaseIIIFile db = new DBaseIIIFile("../../shape_eg_data/mexico/states.dbf");
			
			Assert.AreEqual(new DateTime(1996, 4, 30), db.LastUpdated);
			
			Assert.AreEqual(3, db.Records.Columns.Count);
			
			Assert.AreEqual(typeof(double), db.Records.Columns[0].DataType);
			Assert.AreEqual("AREA", db.Records.Columns[0].ColumnName);
			
			Assert.AreEqual(typeof(string), db.Records.Columns[1].DataType);
			Assert.AreEqual("CODE", db.Records.Columns[1].ColumnName);
			
			Assert.AreEqual(typeof(string), db.Records.Columns[2].DataType);
			Assert.AreEqual("NAME", db.Records.Columns[2].ColumnName);
			
			Assert.AreEqual(32, db.Records.Rows.Count);
			
			Assert.AreEqual(28002.325, db.Records.Rows[0][0]);
			Assert.AreEqual("MX02", db.Records.Rows[0][1]);
			Assert.AreEqual("Baja California Norte", db.Records.Rows[0][2].ToString().Trim());

			Assert.AreEqual(27564.808, db.Records.Rows[db.Records.Rows.Count-1][0]);
			Assert.AreEqual("MX30", db.Records.Rows[db.Records.Rows.Count-1][1]);
			Assert.AreEqual("Veracruz-Llave", db.Records.Rows[db.Records.Rows.Count-1][2].ToString().Trim());
		}
		
		[Test()]
		public void TestAnotherFile()
		{
			DBaseIIIFile db = new DBaseIIIFile("../../shape_eg_data/mexico/cities.dbf");			
			
			Assert.AreEqual(new DateTime(1996, 4, 30), db.LastUpdated);

			Assert.AreEqual(4, db.Records.Columns.Count);

			Assert.AreEqual(36, db.Records.Rows.Count);
		}
	}
}
