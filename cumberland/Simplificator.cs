// 
// Simplificator.cs
//  
// Author:
//       Scott Ellington <scott.ellington@gmail.com>
// 
// Copyright (c) 2009 Scott Ellington
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

namespace Cumberland
{
	internal static class Simplificator
	{
		static public List<Point> Simplify(List<Point> points, double tolerance, bool isPolygon)
		{
			return RecursiveSimplify(points, tolerance, isPolygon, true);
		}
		
		static List<Point> RecursiveSimplify(List<Point> points, 
		                                     double tolerance, 
		                                     bool isPolygon, 
		                                     bool isTopLevel)
		{
			double maxOrthoDistance = 0;
			int index = 0;
			
			for (int ii = 1; ii < points.Count - 1; ii++)
			{
				double orthoDistance = points[ii].Distance(points[points.Count-1], points[0]);

				if (orthoDistance > maxOrthoDistance)
				{
					maxOrthoDistance = orthoDistance;
					index = ii;
				}
			}
			
			if (maxOrthoDistance >= tolerance)
			{
				List<Point> pts1 = RecursiveSimplify(points.GetRange(0, index), 
				                                     tolerance,
				                                     isPolygon,
				                                     false);
				List<Point> pts2 = RecursiveSimplify(points.GetRange(index, points.Count-index), 
				                                     tolerance,
				                                     isPolygon,
				                                     false);
				
				pts1.RemoveAt(pts1.Count-1);
				pts1.AddRange(pts2);
				
				return pts1;
			}
			else
			{
				List<Point> result = new List<Point>();
				result.Add(points[0]);
				
				if (isTopLevel && isPolygon)
				{
					result.Add(points[index]);
				}
				
				result.Add(points[points.Count-1]);
				
				return result;
			}
		}
	}
}
