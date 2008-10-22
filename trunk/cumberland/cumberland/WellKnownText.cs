// WellKnownText.cs
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

namespace Cumberland
{
	
	
	public static class WellKnownText
	{
		public static Point ParsePoint(string wkt)
		{
			wkt = wkt.ToUpper();
			
			string[] pieces = wkt.Split('(',')');
			
			if (pieces.Length != 3)
			{
				throw new ArgumentException("Not valid WKT", "wkt");
			}
			
			if (pieces[0] != "POINT")
			{
				throw new ArgumentException("Invalid WKT: should start with 'POINT'", "wkt");
			}
			
			return SplitPoint(pieces[1]);
		}

		public static PolyLine ParseMultiLineString(string wkt)
		{
			PolyLine pl = new PolyLine();
			
			wkt = wkt.ToUpper();
			
			if (!wkt.StartsWith("MULTILINESTRING"))
			{
				throw new ArgumentException("Invalid WKT: must start with 'MULTILINESTRING'", "wkt");
			}
			
			int idx = wkt.IndexOf('(');
			
			string[] lines = wkt.Substring(idx+1, wkt.Length-idx-1).Split('(',')');
			
			foreach (string line in lines)
			{
				if (line == "," || line == string.Empty) continue;
				
				Line l = new Line();
				
				string[] coords = line.Split(',');
				
				foreach (string coord in coords)
				{
					l.Points.Add(SplitPoint(coord));
				}
				
				pl.Lines.Add(l);
			}
			
			return pl;
		}

		public static List<Feature> ParseMultiPolygon(string wkt)
		{
			wkt = wkt.ToUpper();
			
			List<Feature> polys = new List<Feature>();
			
			if (!wkt.StartsWith("MULTIPOLYGON"))
			{
				throw new ArgumentException("Invalid WKT: must start with 'MULTIPOLYGON'", "wkt");
			}

			// chop off front and end
			// then split by polygon
			int idx = wkt.IndexOf('(');
			string[] polywkts = wkt.Substring(idx+1, wkt.Length-idx-1).Split(new string[] {")),(("}, StringSplitOptions.None);
			
			foreach (string polywkt in polywkts)
			{
				Polygon p = new Polygon();
				
				// split by ring
				string[] ringwkts = polywkt.Split(new string[] {"),("}, StringSplitOptions.None);
				
				for (int ii=0; ii<ringwkts.Length; ii++)
				{
					// trim off parens
					string ringwkt =  ringwkts[ii].Trim(new char[] {'(',')'});
					
					Ring r = new Ring();
					
					// split by coordinate
					string[] coords = ringwkt.Split(',');
					
					foreach (string coord in coords)
					{
						r.Points.Add(SplitPoint(coord));
					}
					
					if (!r.IsClosed) r.Close();
					
					p.Rings.Add(r);
				}
				
				polys.Add(p);
			}
			
			return polys;
			
		}
		
		static Point SplitPoint(string wkt)
		{
			string[] pieces = wkt.Split(' ');
			
			// TODO: snag Z,M values if provided
			if (pieces.Length < 2)
			{
				throw new ArgumentException(string.Format( "Invalid WKT: a coordinate should be '0 0' not '{0}'", wkt), "wkt");
			}

			return new Point(Convert.ToDouble(pieces[0]),
			                 Convert.ToDouble(pieces[1]));
		}
		
	}
}
