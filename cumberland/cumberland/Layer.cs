// Layer.cs
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
using System.Drawing;

using Cumberland.Data;

namespace Cumberland
{
	public enum LineStyle
	{
		None = 0x0000,
		Solid = 0x1111,
		Dashed = 0x0011,
		Dotted = 0x0101
	}
	
	public class Layer
	{
#region properties
		
		public int LineWidth {
			get {
				return lineWidth;
			}
			set {
				lineWidth = value;
			}
		}

		public int PointSize {
			get {
				return pointSize;
			}
			set {
				pointSize = value;
			}
		}

		public Color LineColor {
			get {
				return lineColor;
			}
			set {
				lineColor = value;
			}
		}

		public Color FillColor {
			get {
				return fillColor;
			}
			set {
				fillColor = value;
			}
		}

		public IFeatureSource Data {
			get {
				return data;
			}
			set {
				data = value;
			}
		}

		public LineStyle LineStyle {
			get {
				return lineStyle;
			}
			set {
				lineStyle = value;
			}
		}

		public string Projection {
			get {
				return projection;
			}
			set {
				projection = value;
			}
		}

		public string Id {
			get {
				return id;
			}
			set {
				id = value;
			}
		}
		
		int lineWidth = 1;
		
		int pointSize = 1;
		
		Color lineColor = Color.RoyalBlue;
		
		Color fillColor = Color.AliceBlue;
		
		IFeatureSource data;
		
		LineStyle lineStyle = LineStyle.Solid;
		
		string projection = null;
		
		string id;
		
#endregion
		
		public static Layer CreateFromData(IFeatureSource data, string name)
		{
			Random r = new Random();

			Layer l = new Layer();
			l.Id = name;
			l.Data = data;
			l.PointSize = 5;
			l.LineColor = Color.FromArgb(r.Next(75,150), r.Next(75,150), r.Next(75,150));
			l.FillColor =  Color.FromArgb(l.LineColor.R+100, l.LineColor.G+100, l.LineColor.B+100);
			l.LineWidth = 1;
			
			return l;
		}
	}
}
