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

using Tao.OpenGl;
using Tao.FreeGlut;


namespace Cumberland 
{
	public class MapRenderer
	{
#region Properties
		int width = 400;
		int height = 400;
		
		public List<Shapefile> Layers {
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
		
		List<Shapefile> layers = new List<Shapefile>();
		
		Rectangle extents = new Rectangle(-180, -90, 180, 90);
		
		
		
		
#endregion
		
		public Bitmap Draw()
		{
			int[] temp = new int[1];
			int fbo = -1;
			int depthBuffer = -1;
			int colorBuffer = -1;
			
			Bitmap b = null;
			
			try
			{
				Glut.glutInit();
				Glut.glutCreateWindow("Salmon Viewer");
				Glut.glutHideWindow();
				
				if (!Gl.glGetString(Gl.GL_EXTENSIONS).Contains("GL_EXT_framebuffer_object"))
				{
					throw new NotSupportedException("Your video card does not support frame buffers");	
				}
				
				// get frame buffer from openGL
				Gl.glGenFramebuffersEXT(1, temp);
				fbo = temp[0];
				
				// bind this so that rendering occurs on fbo
				Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, fbo);
				
				// create, bind, and associate our depth buffer to the frame buffer			
//				Gl.glGenRenderbuffersEXT(1, temp);
//				depthBuffer = temp[0];
//				Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, depthBuffer);
//				Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_DEPTH_COMPONENT, width, height);	
//				Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_RENDERBUFFER_EXT, depthBuffer);
								
				//  get render buffer from opengl
				Gl.glGenRenderbuffersEXT(1, temp);
				colorBuffer = temp[0];

				// bind it
				Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, colorBuffer);
				
				// allocate memory
				Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_RGBA, width, height);
				
				// attach render buffer to fbo
				Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_RENDERBUFFER_EXT, colorBuffer);

				// check state
				if (Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT) != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
				{
					throw new InvalidOperationException("This video card may not support Framebuffers");
				}
				
				// clear to white
				Gl.glClearColor(1f, 1f, 1f, 0f);
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
				
				
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glLoadIdentity();
				//Gl.glOrtho(0, width, 0, height, 0, 1);
				Gl.glOrtho(extents.Min.X, extents.Max.X, extents.Min.Y, extents.Max.Y, 0, 1);
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				
				Gl.glDisable(Gl.GL_DEPTH_TEST);
				
				// render here
				Render();
				
				// Draw top triangle
				Gl.glBegin(Gl.GL_TRIANGLES);
				Gl.glColor3d(0.0, 0.0, 1.0);
				Gl.glVertex2i(60, 200);
				Gl.glColor3d(0.0, 1.0, 0);
				Gl.glVertex2i(200, 340);
				Gl.glColor3d(1, 0.0, 0);
				Gl.glVertex2i(340, 200);
				Gl.glEnd();
							
				//int[] arr = new int[160000];
				//b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				b = new Bitmap(width, height);
				//BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				//Gl.glPixelStorei(Gl.GL_PACK_ALIGNMENT, 3);
				//Gl.glReadPixels(0, 0, width, height, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bd.Scan0);
				Gl.glReadPixels(0, 0, width, height, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, bd.Scan0);
				b.UnlockBits(bd);
				//b.RotateFlip(RotateFlipType.Rotate180FlipX);
				
//				for (int ii=0; ii<100; ii++)
//				{
//					System.Console.Write(arr[ii] + " ");
//				}
				
				int errorcode;
				if ((errorcode = Gl.glGetError()) != Gl.GL_NO_ERROR)
				{
					throw new InvalidOperationException("OpenGL reports an error.  Code: " + errorcode);
				}
			}
			finally
			{
				// free memory
				if (fbo >= 0) Gl.glDeleteFramebuffersEXT(1, new int[] { fbo });
				if (depthBuffer >= 0) Gl.glDeleteRenderbuffersEXT(1, new int[] { depthBuffer });
				if (colorBuffer >= 0) Gl.glDeleteRenderbuffersEXT(1, new int[] { colorBuffer });
			}
			
			return b;
		}
		
		public void Render()
		{
//			double xratio = width / Math.Abs(env.Max.X - env.Min.X);
//			double yratio = height / Math.Abs(env.Max.Y - env.Min.Y);	
		
			foreach (Shapefile shp in layers)
			{
				if (shp.features.Count == 0)
				{
					continue;
				}

				uint ctest = 0;
				switch (shp.Shapetype)
				{
					case Shapefile.ShapeType.Point:
					
					    Gl.glBegin(Gl.GL_POINTS);
					
					    Gl.glColor3d(1, 0, 0);
					
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							//FIXME: Convert to 'AS'
							Point p = (Point) shp.features[ii];
//							if (p.X >= env.Min.X && p.X <= env.Max.X && p.Y >= env.Min.Y && p.Y <= env.Max.Y)
//							{		
//								ctest++;
//								//FIXME: Redundancy with all cases.  move ToMapPoint
//								//int px = Convert.ToInt32( (p.X - env.Min.X) * xratio);						
//								//int py = Convert.ToInt32( height - ((p.Y - env.Min.Y) * yratio));
//								//pixmap.DrawRectangle (this.Style.BlackGC, true, px, py, 2, 2);
//								//pxt.DrawRectangle (darea.Style.BlackGC, true, px, py, 2, 2);
							
							    Gl.glVertex2d(p.X, p.Y);
//							}					
						}
					
					    Gl.glEnd();
					
						break;
					
					case Shapefile.ShapeType.PolyLine:

					    for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
							PolyLine pol = (PolyLine) shp.features[ii];
							for (int jj=0; jj < pol.Lines.Count; jj++)
							{
								//FIXME: Convert to 'AS'
								Line r = (Line) pol.Lines[jj];
								//Gdk.Point[] pts = new Gdk.Point[env.points.Count];
							
							    Gl.glBegin(Gl.GL_LINES);
  							    Gl.glColor3d(0.2, 0.2, 0.2);
								
							    for (int kk = 0; kk < r.points.Count; kk++)
								{	
									//FIXME: Convert to 'AS'
									Point pt = (Point) r.points[kk];
									//FIXME: Redundancy with all cases.  move ToMapPoint
									//int pox = Convert.ToInt32( (pt.X - env.Min.X) * xratio);						
									//int poy = Convert.ToInt32( height - ((pt.Y - env.Min.Y) * yratio));
									//pts[kk] = new Gdk.Point(pox,poy);
								
								    Gl.glVertex2d(pt.X, pt.Y);
								}
							
							    Gl.glEnd();
							}
						}
						break;
					
					case Shapefile.ShapeType.Polygon:
					
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
						
							Polygon po = (Polygon) shp.features[ii];
							for (int jj=0; jj < po.Rings.Count; jj++)
							{
							    //System.Console.Write("i");
							    Gl.glBegin(Gl.GL_POLYGON);
							    Gl.glColor3d(0.6, 0.8, 0.7);
							
							    //FIXME: Convert to 'AS'
								Ring r = (Ring) po.Rings[jj];
								//Gdk.Point[] pts = new Gdk.Point[env.points.Count];
								for (int kk = 0; kk < r.Points.Count; kk++)
								{
									//FIXME: Convert to 'AS'
									Point pt = (Point) r.Points[kk];
									//FIXME: Redundancy with all cases.  move ToMapPoint
//									int pox = Convert.ToInt32( (pt.X - env.Min.X) * xratio);						
//									int poy = Convert.ToInt32( height - ((pt.Y - env.Min.Y) * yratio));
									//pts[kk] = new Gdk.Point(pox,poy);
								
								    Gl.glVertex2d(pt.X, pt.Y);
								}

 							    Gl.glEnd();
							
							}
						}
					
						break;
				}
			}

		}
	}
}
