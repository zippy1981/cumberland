// Polygon.cs
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
    public class Polygon : Feature
    {
		// TODO: expose extents
		Point min, max;
		
#region Properties
		
		public List<Ring> Rings {
			get {
				return rings;
			}
		}
        List<Ring> rings = new List<Ring>();
		
#endregion
		
#region ctors
		
        public Polygon(double xmin, double ymin, double xmax, double ymax)
        {
			min = new Point(xmin, ymin);
			max = new Point(xmax, ymax);
		}
		
#endregion

#region Methods
		
		public bool Intersects(Point p)
		{
			if (p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y)
			   	return true;
			else return false;
		}
		
#endregion
		
    }
}
