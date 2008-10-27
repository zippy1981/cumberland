// AbstractFeatureSource.cs
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
using System.Xml.Serialization;

namespace Cumberland.Data
{
	[XmlInclude(typeof(SimpleFeatureSource)), XmlInclude(typeof(Shapefile.Shapefile))]
	public abstract class AbstractFeatureSource : IFeatureSource
	{
		
		public abstract FeatureType SourceFeatureType {
			get;
		}

		public abstract Cumberland.Rectangle Extents {
			get;
		}

		public abstract System.Collections.Generic.List<Cumberland.Feature> GetFeatures (Cumberland.Rectangle rectangle);
	}
}
