
// Copyright (c) 2008 Scott Ellington and authors
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

using System;
using System.Collections.Generic;

namespace Cumberland.Data
{	
	public abstract class AbstractDatabaseFeatureSource : IDatabaseFeatureSource
	{
		string connectionString, tableName;
		int forcedSrid = -1;
		FeatureType forcedFeatureType = FeatureType.None;
		string forcedGeometryColumn;
		SpatialType forcedSpatialType = SpatialType.Geometric;
		
		public virtual string TableName {
			get {
				return tableName;
			}
			set {
				tableName = value;
			}
		}
		
		public virtual int ForcedSrid {
			get {
				return forcedSrid;
			}
			set {
				forcedSrid = value;
			}
		}
		
		public virtual SpatialType ForcedSpatialType {
			get {
				return forcedSpatialType;
			}
			set {
				forcedSpatialType = value;
			}
		}
		
		public virtual string ForcedGeometryColumn {
			get {
				return forcedGeometryColumn;
			}
			set {
				forcedGeometryColumn = value;
			}
		}
		
		public virtual FeatureType ForcedFeatureType {
			get {
				return forcedFeatureType;
			}
			set {
				forcedFeatureType = value;
			}
		}
		
		public virtual string ConnectionString {
			get {
				return connectionString;
			}
			set {
				connectionString = value;
			}
		}

		public abstract FeatureType SourceFeatureType
		{
			get;
		}

		public virtual Rectangle Extents {
			get 
			{
				Rectangle r = new Rectangle();
				
				foreach (Feature f in GetFeatures())
				{
					Rectangle.Union(r, f.CalculateBounds());
				}

				return r;
			}
		}
		
		public abstract List<Feature> GetFeatures ();
		
		public abstract List<Feature> GetFeatures (Rectangle rectangle);
		
		public abstract List<Feature> GetFeatures (string themeField);
		
		public abstract List<Feature> GetFeatures (Rectangle rectangle, string themeField);
		
		public abstract List<Feature> GetFeatures (string themeField, string labelField);
		
		public abstract List<Feature> GetFeatures (Rectangle rectangle, string themeField, string labelField); 
	}
}
