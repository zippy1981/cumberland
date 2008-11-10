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
	public enum ThemeType
	{
		None,
		Unique,
		NumericRange
	}
	
	public class Layer
	{
#region properties
		
		public IFeatureSource Data {
			get {
				return data;
			}
			set {
				data = value;
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

		public List<Style> Styles {
			get {
				return styles;
			}
		}

		public ThemeType Theme {
			get {
				return themeType;
			}
			set {
				themeType = value;
			}
		}

		public string ThemeField {
			get {
				return themeField;
			}
			set {
				themeField = value;
			}
		}

		IFeatureSource data;
		
		string projection = null;
		
		string id;
		
		List<Style> styles = new List<Style>();

		ThemeType themeType = ThemeType.None;
		
		string themeField = null;
		
#endregion
	}
}
