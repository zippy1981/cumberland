// Point.cs
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

namespace Cumberland
{
    public class Point : Feature
    {
        private double x, y, z, m;
        
#region properties
		
		public double X	{

			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}
		
		
		public double Y	{

			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}
		
		public double Z	{

			get
			{
				return z;
			}
			set
			{
				z = value;
			}
		}
		
		public double M	{

			get
			{
				return m;
			}
			set
			{
				m = value;
			}
		}
		
#endregion
		
#region ctors
		
        public Point(double tx, double ty)
        {
            X = tx;
            Y = ty;
        }
        
        public Point(double tx, double ty, double tz)
        {
            X = tx;
            Y = ty;
            Z = tz;
        }
        
        public Point(double tx, double ty, double tz, double tm)
        {
            X = tx;
            Y = ty;
            Z = tz;
            M = tm;
        }

#endregion
		
#region operator overrides
		
		public static bool operator == (Point p1, Point p2)
		{
			if (p1.X == p2.X && p1.Y == p2.Y) return true;
			else return false;
		}
		   
		public static bool operator != (Point p1, Point p2)
		{
			if (p1.X != p2.X || p1.Y != p2.Y) return true;
			else return false;
		}

#endregion
		
#region overrides
		
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}


		public override bool Equals(object ob)
		{
			if (ob is Point)
			{
				Point p = (Point) ob;
				return this == p;
			}
			else return false;
		}
		
		public bool Intersects(Point p)
		{
			return this == p;
		}

#endregion

		public Point Clone()
		{
			return new Point(x, y, z, m);

		}
		
		public override string ToString ()
		{
			return string.Format("{{x:{0} y:{1} z:{2} m:{3}}}", x, y, z, m);
		}

    }
}
