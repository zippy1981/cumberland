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
using System.Text;

using Cumberland;

namespace Cumberland
{
	public static class WellKnownText
	{
#region parse methods
		
		public static Feature Parse(string wkt)
		{
			string type = wkt.Substring(0, wkt.IndexOf('('));
			
			switch (type.Trim().ToUpper())
			{
			case "MULTIPOLYGON":
				return ParseMultiPolygon(wkt);
			case "MULTILINESTRING":
				return ParseMultiLineString(wkt);
			case "POLYGON":
				return ParsePolygon(wkt);
			case "LINESTRING":
				return ParseLineString(wkt);
			case "POINT":
				return ParsePoint(wkt);
			}
			
			return null;
		}
		
		public static Point ParsePoint(string wkt)
		{
			wkt = wkt.ToUpper();
			
			string[] pieces = wkt.Split('(',')');
			
			if (pieces.Length != 3)
			{
				throw new ArgumentException("Not valid WKT", "wkt");
			}
			
			if (pieces[0].Trim().ToUpper() != "POINT")
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
			
			string[] lines = wkt.Substring(idx+1, wkt.Length-idx-1).Split(new string[] {"),(", "), ("}, StringSplitOptions.None);
			
			for (int ii=0; ii < lines.Length; ii++)
			{
				string linewkt = lines[ii].Trim(new char[] {'(',')'});

				Line l = new Line();
				
				string[] coords = linewkt.Split(',');
				
				foreach (string coord in coords)
				{
					l.Points.Add(SplitPoint(coord));
				}
				
				pl.Lines.Add(l);
			}
			
			return pl;
		}

		public static Polygon ParseMultiPolygon(string wkt)
		{
			wkt = wkt.ToUpper();
						
			if (!wkt.StartsWith("MULTIPOLYGON"))
			{
				throw new ArgumentException("Invalid WKT: must start with 'MULTIPOLYGON'", "wkt");
			}

			// chop off front and end
			// then split by polygon
			int idx = wkt.IndexOf('(');
			string[] polywkts = wkt.Substring(idx+1, wkt.Length-idx-1).Split(new string[] {")),((", ")), (("}, StringSplitOptions.None);

			Polygon p = new Polygon();
			
			foreach (string polywkt in polywkts)
			{
				// split by ring
				string[] ringwkts = polywkt.Split(new string[] {"),(", "), ("}, StringSplitOptions.None);
				
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
			}
			
			return p;
			
		}

		public static Polygon ParsePolygon(string wkt)
		{
			string[] parts = wkt.Split(new string[] {"((","))"}, StringSplitOptions.None);
			
			if (parts[0].TrimEnd(' ') != "POLYGON")
			{
				throw new ArgumentException(string.Format("Unexpected WKT, expected 'POLYGON', found '{0}'",
				                                          parts[0]));
			}
				
			Polygon p = new Polygon();
			
			// split by ring
			string[] ringwkts = parts[1].Split(new string[] {"),(","), ("}, StringSplitOptions.None);
			
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
			
			return p;
		}

		public static PolyLine ParseLineString(string wkt)
		{
			string[] parts = wkt.Split(new char[] {'(',')'});
			
			if (parts[0].TrimEnd(' ') != "LINESTRING")
			{
				throw new ArgumentException(string.Format("Unexpected value '{0}'", parts[0]), "wkt");
			}
			
			PolyLine pl = new PolyLine();

			Line l = new Line();
			
			string[] coords = parts[1].Split(',');
			
			foreach (string coord in coords)
			{
				l.Points.Add(SplitPoint(coord));
			}
			
			pl.Lines.Add(l);
			
			return pl;
		}
		
#endregion
		
#region create methods
		
		public static string CreateFromRectangle(Rectangle rectangle)
		{
			Polygon p = new Polygon();
			
			Ring r = new Ring();
			r.Points.Add(rectangle.Min.Clone());
			r.Points.Add(new Point(rectangle.Min.X, rectangle.Max.Y));
			r.Points.Add(rectangle.Max.Clone());
			r.Points.Add(new Point(rectangle.Max.X, rectangle.Min.Y));
			r.Close();
			
			p.Rings.Add(r);
			
			return CreateFromPolygon(p);
		}
		
		public static string CreateFromPolygon (Polygon polygon)
		{
			StringBuilder sb = new StringBuilder();
			
			int idx = -1;
			List<StringBuilder> polys = new List<StringBuilder>();
			
			for (int ii=0; ii< polygon.Rings.Count; ii++)
			{
				Ring ring = polygon.Rings[ii];	

				if (ring.IsClockwise)
				{
					// this is an exterior ring
					// we need to create a new polygon for it
					
					polys.Add(new StringBuilder());
					idx++;
				}
				else if (idx >= 0)
				{
					polys[idx].Append(",");
				}
				
				polys[idx].Append("(");
				
				for (int jj=0; jj <ring.Points.Count; jj++)
				{
					Point point = ring.Points[jj];
					polys[idx].AppendFormat("{0} {1}{2}", point.X, point.Y, jj < ring.Points.Count-1 ? "," : string.Empty);
					
				}

				polys[idx].Append(")");
			}
			
			if (polys.Count == 1)
			{
				sb.Append("POLYGON(");
				sb.Append(polys[0].ToString());
				sb.Append(")");
			}
			else
			{
				sb.Append("MULTIPOLYGON(");
				for (int ii=0; ii<polys.Count; ii++)
				{
					sb.AppendFormat("({0}){1}", polys[ii].ToString(), ii < polys.Count-1 ? "," : string.Empty);
				}
				sb.Append(")");
			}
			
			return sb.ToString();
		}
		
		public static string CreateFromPolyLine(PolyLine polyLine)
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append("MULTILINESTRING(");
			
			for (int ii=0; ii<polyLine.Lines.Count; ii++)
			{
				Line line = polyLine.Lines[ii];
				sb.Append("(");
				
				for (int jj=0; jj <line.Points.Count; jj++)
				{
					Point point = line.Points[jj];
					sb.AppendFormat("{0} {1}{2}", point.X, point.Y, jj <line.Points.Count-1 ? "," : string.Empty);
				}

				sb.AppendFormat("){0}", ii < polyLine.Lines.Count-1 ? "," : string.Empty);
			}
			
			sb.Append(")");
			
			return sb.ToString();
		}
		
		public static string CreateFromPoint(Point point)
		{
			return string.Format("POINT({0} {1})", point.X, point.Y);
		}
			
#endregion		
		
#region private methods
		
		static Point SplitPoint(string wkt)
		{
			string[] pieces = wkt.Trim().Split(' ');
			
			// TODO: snag Z,M values if provided
			if (pieces.Length < 2)
			{
				throw new ArgumentException(string.Format( "Invalid WKT: a coordinate should be '0 0' not '{0}'", wkt), "wkt");
			}

			return new Point(Convert.ToDouble(pieces[0]),
			                 Convert.ToDouble(pieces[1]));
		}
		
#endregion
		
	}
}
