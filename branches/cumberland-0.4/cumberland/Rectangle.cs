// Rectangle.cs
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

namespace Cumberland
{
	public class Rectangle : IEquatable<Rectangle>
	{
#region static properties
		
		public static Rectangle GeographicWorldExtents
		{
			get { return new Rectangle(-180,-90,180,90); }
		}
		
#endregion
		
#region properties
		
		Point min;
		
		public Point Min {
			get {
				return min;
			}
			set {
				min = value;
			}
		}

		public Point Max {
			get {
				return max;
			}
			set {
				max = value;
			}
		}
		
		Point max;
		
		public Point Center {
			get {
				return new Point(Width/2 + min.X, 
				                 Height/2 + min.Y);
			}
			set {
				double w = Width;
				double h = Height;
				
				min.X = value.X - w/2;
				min.Y = value.Y - h/2;
				max.X = value.X + w/2;
				max.Y = value.Y + h/2;
			}
		}
		
		public double Width {
			get {
				return max.X-min.X;
			}
			set {
				if (Width == value) return;

				Point center = Center;
				
				max.X = center.X + value / 2;
				min.X = center.X - value / 2;				
			}
		}
		
		public double Height {
			get {
				return max.Y-min.Y;
			}
			set
			{
				if (Height == value) return;
				
				Point center = Center;
						
				max.Y = center.Y + value / 2;
				min.Y = center.Y - value / 2;
			}
		}
		
		public double AspectRatioOfWidth {
			get {
				return Width / Height;
			}
			set {
				if (value == AspectRatioOfWidth) return;
			
				if (value > AspectRatioOfWidth)
				{
					Width = value * Height;
				}
				else
				{
					Height = Width / value;
				}
			}
		}
		
		public bool IsEmpty
		{
			get
			{
				return Min.X > Max.X || Min.Y > Max.Y;
			}
		}
		
		public double Area
		{
			get
			{
				return Width * Height;
			}
		}
		
#endregion
		
#region Constructors
		
		/// <summary>
		/// Creates an empty rectangle
		/// </summary>
		public Rectangle()
		{
			min = new Point(double.MaxValue, double.MaxValue);
			max = new Point(double.MinValue, double.MinValue);
		}
		
		public Rectangle(double minx, double miny, double maxx, double maxy)
		{
			min = new Point(minx, miny);
			max = new Point(maxx, maxy);
		}
		
		public Rectangle(Point minPoint, Point maxPoint)
		{
			min = minPoint;
			max = maxPoint;
		}
		
		
#endregion
	
#region operator overrides
		
		public static bool operator == (Rectangle r1, Rectangle r2)
		{
			if (object.ReferenceEquals(r1, null))
			{	
				return object.ReferenceEquals(r2, null);
			}
			
			return !object.ReferenceEquals(r2, null) && r1.Min == r2.Min && r1.Max == r2.Max; 
		}
		   
		public static bool operator != (Rectangle r1, Rectangle r2)
		{
			return !(r1 == r2);
		}

#endregion
		
#region overrides
		
		public override int GetHashCode()
		{
			return Min.GetHashCode() ^ Max.GetHashCode();
		}


		public override bool Equals(object obj)
		{
			Rectangle r = obj as Rectangle;
			
			return this == r;
		}
		
		public override string ToString ()
		{
			return string.Format("{{minx:{0}, miny:{1} maxx:{2} maxy:{3}}}", min.X, min.Y, max.X, max.Y);
		}
		
#endregion
		
#region public methods
		
		/// <summary>
		/// Creates a deep copy
		/// </summary>
		/// <returns>
		/// A <see cref="Rectangle"/>
		/// </returns>
		public virtual Rectangle Clone()
		{
			return new Rectangle(min.X, min.Y, max.X, max.Y);
		}
	
		public static Rectangle Union(Rectangle a, Rectangle b)
		{
			return new Rectangle(a.Min.X < b.Min.X ? a.Min.X : b.Min.X,
			                     a.Min.Y < b.Min.Y ? a.Min.Y : b.Min.Y,
			                     a.Max.X > b.Max.X ? a.Max.X : b.Max.X,
			                     a.Max.Y > b.Max.Y ? a.Max.Y : b.Max.Y);
		}
		
		public virtual bool Overlaps(Rectangle r)
		{
			bool xoverlaps = false;
			
			if (Min.X >= r.Min.X && 
			    Max.X <= r.Max.X)
			{
				// within or equal
				xoverlaps = true;
			}
			else if (Min.X < r.Min.X && 
			         Max.X >= r.Min.X)
			{
				// this.Min.X is less but this.Max.X overlaps r
				xoverlaps = true;
			}
			else if (Max.X > r.Max.X && 
			         Min.X <= r.Max.X)
			{
				// this.Max.X is more but this.Min.Y overlaps r
				xoverlaps = true;
			}
			
			// if false, failed on x-axis
			if (!xoverlaps) return xoverlaps;
			
			bool yoverlaps = false;
			
			if (Min.Y >= r.Min.Y &&
			    Max.Y <= r.Max.Y)
			{
				yoverlaps = true;
			}
			else if (Min.Y < r.Min.Y &&
			         Max.Y >= r.Min.Y)
			{
				yoverlaps = true;
			}
			else if (Max.Y > r.Max.Y &&
			         Min.Y <= r.Max.Y)
			{
				yoverlaps = true;
			}
			
			return yoverlaps;
		}

		public virtual bool Equals(Rectangle r)
		{
			return this == r;
		}

		public static Rectangle Intersect(Rectangle a, Rectangle b)
		{
			return new Rectangle(Math.Max(a.Min.X, b.Min.X),
			                     Math.Max(a.Min.Y, b.Min.Y),
			                     Math.Min(a.Max.X, b.Max.X),
			                     Math.Min(a.Max.Y, b.Max.Y));
		}
		
#endregion
	}
}