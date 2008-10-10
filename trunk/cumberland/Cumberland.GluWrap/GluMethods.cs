// MyClass.cs
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
using System.Runtime.InteropServices;
using System.Security;

using Tao.OpenGl;

namespace Cumberland.GluWrap
{
	public static class GluMethods
	{
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback(IntPtr tess, int which, [In] Glu.TessBeginCallback func);
		
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr gluNewTess();
		
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern void gluDeleteTess(IntPtr tess);
		
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback([In] IntPtr tess, int which, [In] Glu.TessEndCallback func);
		
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback([In] IntPtr tess, int which, [In] Glu.TessVertexCallback func);
		
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback([In] IntPtr tess, int which, [In] Glu.TessErrorCallback func);
		
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback([In] IntPtr tess, int which, [In] TessVertexCallback1 func);
		
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback([In] IntPtr tess, int which, [In] Glu.TessVertexDataCallback func);
		
		[DllImport("glu32.dll", EntryPoint="gluTessCallback"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessCallback([In] IntPtr tess, int which, [In] TessVertexDataCallback1 func);

		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessBeginPolygon([In] IntPtr tess, [In] IntPtr data);
		
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessBeginContour([In] IntPtr tess);
		
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        //public static extern void gluTessVertex([In] IntPtr tess, [In] double[] location, [In] double[] data);
		public static extern void gluTessVertex([In] IntPtr tess,[In] IntPtr vertex,[In] IntPtr data);
		
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessEndContour([In] IntPtr tess);
		
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessEndPolygon([In] IntPtr tess);
		
		[DllImport("glu32.dll", EntryPoint="gluErrorString"), SuppressUnmanagedCodeSecurity]
		private static extern IntPtr gluErrorStringUnsafe(int errorCode);
	  	
		[DllImport("glu32.dll"), SuppressUnmanagedCodeSecurity]
        public static extern void gluTessProperty([In] IntPtr tess, int which, double data);
		
		public delegate void TessVertexCallback1([MarshalAs(UnmanagedType.LPArray, SizeConst=3),In] double[] vertexData);
		
		public delegate void TessVertexDataCallback1([MarshalAs(UnmanagedType.LPArray, SizeConst=3),In] double[] vertexData, IntPtr polygonData);
		
	  	public static string gluErrorString(int errorCode) 
		{
			return Marshal.PtrToStringAnsi(gluErrorStringUnsafe(errorCode));
		}	
	}
}
