// 
// WellKnownBinary.cs
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

namespace Cumberland.Data.SimpleFeatureAccess
{
	enum WkbGeometryType 
	{
		WkbPoint                = 1,
		WkbLineString           = 2,
		WkbPolygon              = 3,
		WkbTriangle             = 17,
		WkbMultiPoint           = 4,
		WkbMultiLineString      = 5,
		WkbMultiPolygon         = 6,
		WkbGeometryCollection   = 7,
		WkbPolyhedralSurface    = 15,
		WkbTIN                  = 16,
		WkbPointZ               = 1001,
		WkbLineStringZ          = 1002,
		WkbPolygonZ             = 1003,
		WkbTrianglez            = 1017,
		WkbMultiPointZ          = 1004,
		WkbMultiLineStringZ     = 1005,
		WkbMultiPolygonZ        = 1006,
		WkbGeometryCollectionZ  = 1007,
		WkbPolyhedralSurfaceZ   = 1015,
		WkbTINZ                 = 1016,
		WkbPointM               = 2001,
		WkbLineStringM          = 2002,
		WkbPolygonM             = 2003,
		WkbTriangleM            = 2017,
		WkbMultiPointM          = 2004,
		WkbMultiLineStringM     = 2005,
		WkbMultiPolygonM        = 2006,
		WkbGeometryCollectionM  = 2007,
		WkbPolyhedralSurfaceM   = 2015,
		WkbTINM                 = 2016,
		WkbPointZM              = 3001,
		WkbLineStringZM         = 3002,
		WkbPolygonZM            = 3003,
		WkbTriangleZM           = 3017,
		WkbMultiPointZM         = 3004,
		WkbMultiLineStringZM    = 3005,
		WkbMultiPolygonZM       = 3006,
		WkbGeometryCollectionZM = 3007,
		WkbPolyhedralSurfaceZM  = 3015,
		WkbTinZM                = 3016
	}

	
	public static class WellKnownBinary
	{
		// implements a subset of well-known binary as defined by ogc simple feature access:
		// http://www.opengeospatial.org/standards/sfa
		
		public static Feature Parse(byte[] wkb)
		{
			//byte order = wkb[0];

			uint type = BitConverter.ToUInt32(wkb, 1);
			
			switch ((WkbGeometryType) type)
			{
				case WkbGeometryType.WkbPoint:
					return new Point(BitConverter.ToDouble(wkb, 5),
					                 BitConverter.ToDouble(wkb, 13));
				case WkbGeometryType.WkbPolygon:
					return ParsePolygon(wkb);
				case WkbGeometryType.WkbMultiLineString:
					return ParsePolyLine(wkb);
				case WkbGeometryType.WkbMultiPolygon:
					return ParseMultiPolygon(wkb);
				default:
					throw new NotSupportedException(string.Format("{0} is not a supported Wkb type",
					                                              (WkbGeometryType) type));
			}
		}
		
		static Polygon ParsePolygon(byte[] wkb)
		{
			Polygon p = new Polygon();
			int idx = 0;
			ParsePolygon(wkb, p, ref idx);
			return p;
		}
		
		static void ParsePolygon(byte[] wkb, Polygon p, ref int idx)
		{
			// skip order and type
			idx += 5;
			
			uint numRings = BitConverter.ToUInt32(wkb, idx);
			idx+=4;
			
			for (int ii=0; ii < numRings; ii++)
			{
				Ring r = new Ring();
				
				uint numPoints = BitConverter.ToUInt32(wkb, idx);
				idx+=4;
								
				for (int jj=0; jj < numPoints; jj++)
				{
					r.Points.Add(new Point(BitConverter.ToDouble(wkb, idx),
					                       BitConverter.ToDouble(wkb, idx+=8)));
					idx+=8;
				}
				
				p.Rings.Add(r);
			}
		}
		
		static Polygon ParseMultiPolygon(byte[] wkb)
		{
			Polygon p = new Polygon();
			
			int idx = 5;
			
			uint numPolygons = BitConverter.ToUInt32(wkb, idx);
			idx+=4;
			
			for (int ii=0; ii<numPolygons; ii++)
			{
				ParsePolygon(wkb, p, ref idx);
			}
			
			return p;
		}
		
		static PolyLine ParsePolyLine(byte[] wkb)
		{
			PolyLine p = new PolyLine();
			
			int idx = 5;

			uint numLines = BitConverter.ToUInt32(wkb, idx);
			idx+=4;
			
			for (int ii=0; ii < numLines; ii++)
			{
				Line l = new Line();
				
				idx+=5; // skip byte order and type
				
				uint numPoints = BitConverter.ToUInt32(wkb, idx);
				idx+=4;
				
				for (int jj=0; jj < numPoints; jj++)
				{
					l.Points.Add(new Point(BitConverter.ToDouble(wkb, idx),
					                       BitConverter.ToDouble(wkb, idx+=8)));
					idx+=8;
				}
				
				p.Lines.Add(l);
			}
			
			return p;
		}
	}
}
