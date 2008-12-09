// SphericalMercator.cs
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

namespace Cumberland.Projection
{
	internal sealed class SphericalMercatorProjector
	{
		int radius = 6378137;
		
		public double Circumference
		{
			get { return 2 * Math.PI * radius; }
		}
		
//		public Point Deproject(Point p)
//		{
//			double originShift = Circumference/2;
//			
//			// longitude = multiply meters by scale (ll/meters)
//			double lon = (p.X / (originShift)) * 180;
//			
//			// latitude:
//			// 1) multiple meters by scale (ll/meters)
//			// 2) peform mercator math 
//			double lat = (p.Y / (originShift)) * 180;
//			lat = 180 / Math.PI * // convert radians to degrees
//				(2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);
//			
//			return new Point(lon, lat);
//		}
//		
//		public Point Project(Point p)
//		{
//			double originShift = Circumference/2;
//			
//			return new Point(p.X * originShift / 180, // multiply lon by scale (meters/ll)
//			                 originShift / 180 * // multiply lat by scale (meters/ll)
//			                 Math.Log(Math.Tan((90 + p.Y)
//			                 
//		}
	}
}
