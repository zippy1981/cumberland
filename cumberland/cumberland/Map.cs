
// MapRenderer.cs
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

using System.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using System.Runtime.InteropServices;

using Cumberland.Data;

namespace Cumberland 
{
	public class Map
	{
#region Properties
		
		int width = 400;
		int height = 400;
		
		public List<Layer> Layers {
			get {
				return layers;
			}
			set {
				layers = value;
			}
		}

		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}
		
		public int Height {
			get {
				return height;
			}
			set {
				height = value;
			}
		}

		public Rectangle Extents {
			get {
				return extents;
			}
			set {
				extents = value;
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
		
		List<Layer> layers = new List<Layer>();
		
		Rectangle extents = new Rectangle(-180, -90, 180, 90);

		string projection = null;
		
#endregion
		
#region public methods
		
//		public int AddLayerByFeatureTypeAndExtents(Layer layer)
//		{
//			if (layer == null)
//			{
//				throw new ArgumentNullException("layer");
//			}
//			
//			if (layer.Data == null)
//			{
//				Layers.Add(layer);
//				return Layers.Count-1;
//			}
//			
//			FeatureType ft = layer.Data.SourceFeatureType;
//			
//			//default is top
//			int idx_to_add = Layers.Count;
//
//			for (int ii = Layers.Count - 1; ii >= 0; ii--)
//			{
//				Layer next = Layers[ii];
//
//				// if the next layer is of a type that we want to be 
//				// in front of, insert here
//				// else, keep checking...
//				if (InsertAheadOf(ft, next.Data.SourceFeatureType))
//				{
//					break;
//				}
//				else if (ft == next.Data.SourceFeatureType)
//				{
//					// the two layers are the same type
//					// so place our layer here if it has smaller area
//					// else, keep going...
//					if (layer.Data.Extents.Area <= next.Data.Extents.Area)
//					{
//						break;
//					}
//				}
//				
//				// we are at least going to insert here
//				idx_to_add = ii;
//			}
//		
//			this.Layers.Insert (idx_to_add, layer);
//			
//			return idx_to_add;
//		}
		
#endregion
		
#region private methods
		
//		static bool InsertAheadOf(FeatureType current, FeatureType next)
//		{
//			if (current == FeatureType.None) 
//			{
//				return true;
//			}
//			else if (current == FeatureType.Point && 
//			         next != FeatureType.None) 
//			{
//				return true;
//			}
//			else if (current == FeatureType.Polyline && 
//			         next == FeatureType.Polygon)
//			{
//				return true;
//			}
//			
//			return false;
//		}
#endregion
	}
}
