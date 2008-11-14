// Style.cs
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
using System.Drawing;

namespace Cumberland
{
#region public enums
	
	public enum PointSymbolType
	{
		None,
		Shape,
		Image
	}
	
	public enum PointSymbolShapeType
	{
		None,
		Circle,
		Square
	}
	
	public enum LineStyle
	{
		None = 0x0000,
		Solid = 0x1111,
		Dashed = 0x0011,
		Dotted = 0x0101
	}
	
#endregion
	
	public class Style
	{
#region vars
		
		PointSymbolType pointSymbol = PointSymbolType.Shape;
		PointSymbolShapeType pointShape = PointSymbolShapeType.Circle;
		string pointImagePath = null;
		int lineWidth = 1;
		int pointSize = 1;
		Color lineColor = Color.RoyalBlue;
		Color fillColor = Color.AliceBlue;
		LineStyle lineStyle = LineStyle.Solid;
		string uniqueThemeValue = null;
		double maxRangeThemeValue = 1;
		double minRangeThemeValue = 0;
		
#endregion

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
		
		public LineStyle LineStyle {
			get {
				return lineStyle;
			}
			set {
				lineStyle = value;
			}
		}
		
		public PointSymbolType PointSymbol {
			get {
				return pointSymbol;
			}
			set {
				pointSymbol = value;
			}
		}

		public PointSymbolShapeType PointSymbolShape {
			get {
				return pointShape;
			}
			set {
				pointShape = value;
			}
		}

		public string PointSymbolImagePath {
			get {
				return pointImagePath;
			}
			set {
				pointImagePath = value;
			}
		}

		public string UniqueThemeValue {
			get {
				return uniqueThemeValue;
			}
			set {
				uniqueThemeValue = value;
			}
		}

		public double MinRangeThemeValue {
			get {
				return minRangeThemeValue;
			}
			set {
				minRangeThemeValue = value;
			}
		}

		public double MaxRangeThemeValue {
			get {
				return maxRangeThemeValue;
			}
			set {
				maxRangeThemeValue = value;
			}
		}
		
#endregion
	}
}
