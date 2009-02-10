// Ring.cs
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

using System.Collections.Generic;

namespace Cumberland
{
	
	/// <summary>
	/// A polygon that is not self-intersecting
	/// </summary>
    public class Ring
    {
#region properties
		
        List<Point> points = new List<Point>();
        
		public bool IsClockwise
		{
			get
			{
				return CalculateArea() >= 0;
			}
		}
		
		public bool IsClosed
		{
			get
			{
				return points.Count > 3 && points[0] == points[points.Count-1];
			}
		}
		
        public List<Point> Points {
        	get {
        		return points;
        	}
        }
		
#endregion
		
#region ctor
		
        public Ring()
		{
		}
		
#endregion
		
#region public methods
		
		public double CalculateArea()
		{
			double area = 0;
			
			for (int ii=0; ii<points.Count-1; ii++)
			{
				area += (points[ii].X * points[ii+1].Y) - (points[ii+1].X * points[ii].Y);
			}
			
			// flip the sign as clockwise is the positive here	
			return (area / 2) * -1;
		}
		
		public Rectangle CalculateBounds()
		{
			double minx, miny;
			minx = miny = double.MaxValue;
			double maxx, maxy;
			maxx = maxy = double.MinValue;
			
			foreach (Point p in Points)
			{
				if (p.X < minx)
				{
					minx = p.X;
				}
				
				if (p.X > maxx)
				{
					maxx = p.X;
				}
				
				if (p.Y < miny)
				{
					miny = p.Y;
				}
				
				if (p.Y > maxy)
				{
					maxy = p.Y;
				}
			}
			
			return new Rectangle(minx, miny, maxx, maxy);
		}
		
		public void Close()
		{
			if (points.Count > 2 && !IsClosed)
			{
				points.Add(points[0].Clone());
			}
		}
		
		public bool Validate()
		{
			//TODO: test for intersections? duplicate points?
			return points.Count > 2;
		}
		
		public Ring Simplify(double tolerance)
		{
			Ring r = new Ring();
			r.points = new List<Point>(this.points);
			
			if (r.IsClosed)
			{
				r.points.RemoveAt(r.Points.Count-1);
			}
			
			r.points = Simplificator.Simplify(r.points, tolerance, true);
			
			r.Close();
			
			return r;
		}
		
#endregion
    }
}
