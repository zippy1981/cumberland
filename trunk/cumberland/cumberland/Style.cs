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
		None,
		Solid,
		Dashed,
		Dotted
	}

	public enum FillStyle
	{
		None,
		Solid,
		Texture
	}

	public enum LabelFont
	{
		None,
		Serif,
		SansSerif,
		Monospace,
		Custom
	}

	public enum LabelPosition
	{
		None,
		Center,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left,
		TopLeft,
		Top,
		TopRight
	}

	public enum LabelDecoration
	{
		None,
		Outline
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
		string id;
		FillStyle fillStyle = FillStyle.Solid;
		
		LabelFont labelFont = LabelFont.Serif;
		bool showLabels = false;
		Color labelColor = Color.Black;
		float labelFontEmSize = 10;
		LabelPosition labelPosition = LabelPosition.Center;
		int labelPixelOffset = 5;
		LabelDecoration labelDecoration = LabelDecoration.None;
		Color labelOutlineColor = Color.White;
		float labelOutlineWidth = 1f;
		float labelAngle = 0f;
		string labelCustomFont = null;

		double minScale = double.MinValue;
		double maxScale = double.MaxValue;
		double labelMinScale = double.MinValue;
		double labelMaxScale = double.MaxValue;
		
		bool calculateLabelAngleForPolyLine = true;

		bool drawPointSymbolOnPolyLine = false;
		string fillTexturePath = null;
		
		bool simplify = false;
		double simplifyTolerance = 1;
		
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

		public string Id {
			get {
				return id;
			}
			set {
				id = value;
			}
		}

		public FillStyle FillStyle {
			get {
				return fillStyle;
			}
			set {
				fillStyle = value;
			}
		}

		public bool ShowLabels {
			get {
				return showLabels;
			}
			set {
				showLabels = value;
			}
		}

		public LabelFont LabelFont {
			get {
				return labelFont;
			}
			set {
				labelFont = value;
			}
		}

		public Color LabelColor {
			get {
				return labelColor;
			}
			set {
				labelColor = value;
			}
		}

		public float LabelFontEmSize {
			get {
				return labelFontEmSize;
			}
			set {
				labelFontEmSize = value;
			}
		}

		public LabelPosition LabelPosition {
			get {
				return labelPosition;
			}
			set {
				labelPosition = value;
			}
		}

		public int LabelPixelOffset {
			get {
				return labelPixelOffset;
			}
			set {
				labelPixelOffset = value;
			}
		}

		public float LabelOutlineWidth {
			get {
				return labelOutlineWidth;
			}
			set {
				labelOutlineWidth = value;
			}
		}

		public Color LabelOutlineColor {
			get {
				return labelOutlineColor;
			}
			set {
				labelOutlineColor = value;
			}
		}

		public LabelDecoration LabelDecoration {
			get {
				return labelDecoration;
			}
			set {
				labelDecoration = value;
			}
		}

		public float LabelAngle {
			get {
				return labelAngle;
			}
			set {
				labelAngle = value;
			}
		}

		public double MinScale {
			get {
				return minScale;
			}
			set {
				minScale = value;
			}
		}

		public double MaxScale {
			get {
				return maxScale;
			}
			set {
				maxScale = value;
			}
		}

		public double LabelMinScale {
			get {
				return labelMinScale;
			}
			set {
				labelMinScale = value;
			}
		}

		public double LabelMaxScale {
			get {
				return labelMaxScale;
			}
			set {
				labelMaxScale = value;
			}
		}

		public string LabelCustomFont {
			get {
				return labelCustomFont;
			}
			set {
				labelCustomFont = value;
			}
		}

		public bool DrawPointSymbolOnPolyLine {
			get {
				return drawPointSymbolOnPolyLine;
			}
			set {
				drawPointSymbolOnPolyLine = value;
			}
		}

		public bool CalculateLabelAngleForPolyLine {
			get {
				return calculateLabelAngleForPolyLine;
			}
			set {
				calculateLabelAngleForPolyLine = value;
			}
		}

		public string FillTexturePath {
			get {
				return fillTexturePath;
			}
			set {
				fillTexturePath = value;
			}
		}

		public double SimplifyTolerance {
			get {
				return simplifyTolerance;
			}
			set {
				simplifyTolerance = value;
			}
		}

		public bool Simplify {
			get {
				return simplify;
			}
			set {
				simplify = value;
			}
		}
		
#endregion

		public Style Clone()
		{
			Style s = new Style();

			s.LineWidth = this.LineWidth;
			s.PointSize = this.PointSize;
			s.LineColor = this.LineColor;
			s.FillColor = this.FillColor;
			s.LineStyle = this.LineStyle;
			s.PointSymbol = this.PointSymbol;
			s.PointSymbolImagePath = this.PointSymbolImagePath;
			s.UniqueThemeValue = this.UniqueThemeValue;
			s.MinRangeThemeValue = this.MinRangeThemeValue;
			s.MaxRangeThemeValue = this.MaxRangeThemeValue;
			s.Id = this.Id;
			s.FillStyle = this.FillStyle;
			s.ShowLabels = this.ShowLabels;
			s.LabelFont = this.LabelFont;
			s.LabelColor = this.LabelColor;
			s.LabelFontEmSize = this.LabelFontEmSize;
			s.LabelPosition = this.LabelPosition;
			s.LabelPixelOffset = this.LabelPixelOffset;
			s.LabelOutlineWidth = this.LabelOutlineWidth;
			s.LabelOutlineColor = this.LabelOutlineColor;
			s.LabelDecoration = this.LabelDecoration;
			s.LabelAngle = this.LabelAngle;
			s.MinScale = this.MinScale;
			s.MaxScale = this.MaxScale;
			s.LabelMinScale = this.LabelMinScale;
			s.LabelMaxScale = this.LabelMaxScale;
			s.labelCustomFont = this.LabelCustomFont;
			s.DrawPointSymbolOnPolyLine = this.DrawPointSymbolOnPolyLine;
			s.CalculateLabelAngleForPolyLine = this.CalculateLabelAngleForPolyLine;
			s.FillTexturePath = this.FillTexturePath;
			s.Simplify = this.Simplify;
			s.SimplifyTolerance = this.SimplifyTolerance;
			
			return s;
		}
	}
}
