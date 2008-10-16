// Title:	Matrix.cs
// Author: 	Scott Ellington <scott.ellington@gmail.com>
//
// Copyright (C) 2006 Scott Ellington and authors
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;

namespace Cumberland.Drawing.OpenGL
{
	public static class OpenGLMatrixHelper
	{
		const double ATR = .01745;
		
		public static float[] GetIdentity ()
		{
			return new float[] {1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1};
		}

		public static float[] Inverse ( float[] m )
		{
			int swap;
			float t;
			float[,] temp = new float[4,4];
			float[] inv = GetIdentity();

			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					temp[i,j] = m[(i * 4) + j];

			for (int i = 0; i < 4; i++)
			{
				// Look for largest element in column
				swap = i;
				for (int j = i + 1; j < 4; j++)
					if (Math.Abs(temp[j,i]) > Math.Abs(temp[i,i]))
						swap = j;

				if (swap != i)
				{
					// Swap rows.
					for (int k = 0; k < 4; k++)
					{
						t = temp[i,k];
						temp[i,k] = temp[swap,k];
						temp[swap,k] = t;

						t = inv[(i * 4) + k];
						inv[(i * 4) + k] = inv[(swap * 4) + k];
						inv[(swap * 4) + k] = t;
					}
				}
				
				if (temp[i,i] == 0)
					throw new Exception ("Matrix is Math.Singular");

				t = temp[i,i];
				for (int k = 0; k < 4; k++)
				{    
					temp[i,k] /= t;
					inv[(i * 4) + k] /= t;
				}
				for (int j = 0; j < 4; j++)
				{
					if (j != i)
					{
						t = temp[j,i];
						for (int k = 0; k < 4; k++)
						{
							temp[j,k] -= temp[i,k] * t;
							inv[(j * 4) + k] -= inv[(i * 4) + k] * t;
						}
					}
				}
			}
			return inv;
		}

		public static float[] Transform (float rotX, float rotY, float rotZ, float transX, float transY, float transZ )
		{
			float[] myMatrix = new float [16];
			
			float A,B,C,D,E,F,AD,BD;
			A = (float) Math.Cos ( rotX * ATR );
			B = (float) Math.Sin ( rotX * ATR );
			C = (float) Math.Cos ( rotY * ATR );
			D = (float) Math.Sin ( rotY * ATR );
			E = (float) Math.Cos ( rotZ * ATR );
			F = (float) Math.Sin ( rotZ * ATR );

			AD = A * D;
			BD = B * D;

			myMatrix[0]  =   C * E;
			myMatrix[1]  =  -C * F;
			myMatrix[2]  =  -D;
			myMatrix[4]  = -BD * E + A * F;
			myMatrix[5]  =  BD * F + A * E;
			myMatrix[6]  =  -B * C;
			myMatrix[8]  =  AD * E + B * F;
			myMatrix[9]  = -AD * F + B * E;
			myMatrix[10] =   A * C;
			myMatrix[3] = myMatrix[7] = myMatrix[11] = 0;
			myMatrix[12] = transX;
			myMatrix[13] = transY;
			myMatrix[14] = transZ;
			myMatrix[15] =  1;

			return myMatrix;
		}
	}
}