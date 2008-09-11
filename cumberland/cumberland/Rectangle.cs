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

namespace Cumberland
{
    class Rectangle
    {
        public double x;
        public double y;
        public double width;
        public double height;
        
        public Rectangle(double tx, double ty, double twidth, double theight)
        {
            this.x = tx;
            this.y = ty;
            this.width = twidth;
            this.height = theight;
        }

		public static bool operator == (Rectangle p1, Rectangle p2)
		{
			if (p1.x == p2.x && p1.y == p2.y && 
				p1.width == p2.width && p1.height == p2.height) 
				return true;
			else return false;
		}
		   
		public static bool operator != (Rectangle p1, Rectangle p2)
		{
			if (p1.x != p2.x || p1.y != p2.y ||
				p1.width != p2.width || p1.height != p2.height) 
				return true;
			else return false;
		}
    }
}
