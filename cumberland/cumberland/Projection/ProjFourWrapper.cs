// ProjFourWrapper.cs
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

using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

namespace Cumberland.Projection
{
    public class ProjFourWrapper : IDisposable, IProjector
    {
		const double DegreesToRadians = Math.PI / 180;
		
		public static readonly string SphericalMercatorProjection  = "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs";
		
		public static readonly string WGS84 = "+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs";
		
        IntPtr projPJ;
		
		[StructLayout(LayoutKind.Sequential)]
	    struct UV
	    {
	        public double U;
	        public double V;
	
	        public UV(double u, double v)
	        {
	            this.U = u;
	            this.V = v;
	        }
		}
		
#region p/invokes
		
//        [DllImport("proj.dll")]
//        static extern IntPtr pj_init(int argc, string[] args);

		[DllImport("proj.dll")]
		static extern IntPtr pj_init_plus(string args);
		
        [DllImport("proj.dll")]
        static extern void pj_free(IntPtr projPJ);

        [DllImport("proj.dll")]
        static extern UV pj_fwd(UV uv, IntPtr projPJ);

        [DllImport("proj.dll")]
        static extern UV pj_inv(UV uv, IntPtr projPJ);
		
		[DllImport("proj.dll")]
        static extern IntPtr pj_get_errno_ref();

		[DllImport("proj.dll")]
        static extern IntPtr pj_strerrno(int errno);
		
		[DllImport("proj.dll")]
        static extern int pj_is_latlong(IntPtr projPJ);
		
		[DllImport("proj.dll")]
        static extern IntPtr pj_get_release();
		
		[DllImport("proj.dll")]
        static extern int pj_transform( IntPtr src, IntPtr dst, int point_count, int point_offset, double[] x, double[] y, double[] z );

		[DllImport("proj.dll")]
        static extern int pj_is_geocent(IntPtr projPJ);
		
		
#endregion
		
#region properties
		
		public bool IsLatLong
		{
			get { return pj_is_latlong(projPJ) > 0 ? true : false; }				
		}
		
		public bool IsGeocentric
		{
			get { return pj_is_geocent(projPJ) > 0 ? true : false; }	
		}

		public static string Version
		{
			get { return Marshal.PtrToStringAnsi(pj_get_release()); }
		}
		
#endregion
		
#region contructors
		
		private ProjFourWrapper()
		{
		}		
		
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initParameters">Proj.4 style list of options.
        /// </param>
        public ProjFourWrapper(string initParameters)
        {
			projPJ = pj_init_plus(initParameters);
            //projPJ = pj_init(initParameters.Length, initParameters);
			
            if (projPJ == IntPtr.Zero)
			{
                throw new InvalidOperationException("Proj4 failed to initialize: " + GetError());
			}
        }
		
		public ProjFourWrapper(int epsg) : this(PrepareEPSGCode(epsg))
		{
		}
		
		
#endregion

#region public methods
		
        public Point Deproject(Point point)
        {
            UV uv = pj_fwd(new UV(point.X * DegreesToRadians, point.Y * DegreesToRadians), projPJ);
			
			return new Point(uv.U, uv.V);
        }

        public Point Project(Point point)
        {
			UV uv = pj_inv(new UV(point.X, point.Y), projPJ);
			
			return new Point(uv.U / DegreesToRadians, uv.V / DegreesToRadians);
        }

		public Point Transform(ProjFourWrapper destinationProj, Point point)
		{
			double[] x = new double[] { point.X };
			double[] y = new double[] { point.Y };
			
			if (this.IsLatLong)
			{
				x[0] = x[0] * DegreesToRadians;
				y[0] = y[0] * DegreesToRadians;
			}
			
			int errno = pj_transform(this.projPJ, destinationProj.projPJ, 1, 0, x, y, new double[] {0}) ;
			if (errno != 0)
			{
				throw new InvalidOperationException("Proj4 transform failed: " + GetError() + " " + point.ToString());
			}
			
			if (destinationProj.IsLatLong)
			{
				x[0] = x[0] / DegreesToRadians;
				y[0] = y[0] / DegreesToRadians;
			}
			
			return new Point(x[0], y[0]);
		}
		
//		public void Transform(ProjFourWrapper destinationProj, Polygon polygon)
//		{
//			
//			foreach (Ring r in polygon.Rings)
//			{
//				for (int ii=0; ii<r.Points.Count; ii++)
//				{
//					r.Points[ii] = this.Transform(destinationProj, r.Points[ii]);
//				}
//			}		
//		}
//		
//		public void Transform(ProjFourWrapper destinationProj, PolyLine polyline)
//		{
//			foreach (Line line in polyline.Lines)
//			{
//				for (int ii=0; ii<line.Points.Count; ii++)
//				{
//					line.Points[ii] = this.Transform(destinationProj, line.Points[ii]);
//				}
//			}
//		}

		public static string GetError()
		{
			return Marshal.PtrToStringAnsi(pj_strerrno(Marshal.ReadInt32(pj_get_errno_ref())));
		}
		
#endregion
		
#region public static methods
		
		public static string PrepareEPSGCode(int epsg)
		{
			return "+init=epsg:" + epsg;
		}
		
#endregion
		
#region dispose pattern
		
        public void Dispose()
        {
			Dispose(true);
			GC.SuppressFinalize(this);
        }
		
		protected virtual void Dispose(bool disposing)
		{
			if (projPJ != IntPtr.Zero)
            {
                pj_free(projPJ);
                projPJ = IntPtr.Zero;
            }
		}
		
		~ProjFourWrapper()
		{
			Dispose(false);
		}
		
#endregion
    }
}