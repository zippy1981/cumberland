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
				// hack to get an opengl context
				Glut.glutInit();
				Glut.glutCreateWindow(string.Empty);
				Glut.glutHideWindow();
				
				// test for fbo support
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
				
				// acquire the proper space
				Gl.glViewport(0, 0, width, height);
				
				// clear to white
				Gl.glClearColor(1f, 1f, 1f, 0f);
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
				
				// switch to projection matrix
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glLoadIdentity();
				
				Rectangle r = extents.Clone();
				//Rectangle r = new Rectangle(-115, 14, -87, 34);

				// set aspect ratio to image to avoid distortion
				r.AspectRatioOfWidth = width / height;

				// set projection matrix to our extents				
				Gl.glOrtho(r.Min.X, r.Max.X, r.Min.Y, r.Max.Y, 0, 1);
				
				// switch back to model view
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				
				// 2d image
				Gl.glDisable(Gl.GL_DEPTH_TEST);
				
				// render here
				Render();
				
				// testing
				// Draw top triangle
//				Gl.glBegin(Gl.GL_TRIANGLES);
//				Gl.glColor3d(0.0, 0.0, 1.0);
//				Gl.glVertex2i(0, 0);
//				Gl.glColor3d(0.0, 1.0, 0);
//				Gl.glVertex2i(200, 400);
//				Gl.glColor3d(1, 0.0, 0);
//				Gl.glVertex2i(400, 0);
//				Gl.glEnd();
						
				// acquire the pixels from openGL and draw to bitmap
				b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				Gl.glReadPixels(0, 0, width, height, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bd.Scan0);
				b.UnlockBits(bd);
				b.RotateFlip(RotateFlipType.Rotate180FlipX);
				
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
							Gl.glVertex2d(p.X, p.Y);			
						}
					
					    Gl.glEnd();
					
						break;
					
					case Shapefile.ShapeType.PolyLine:

					    Gl.glEnable(Gl.GL_LINE_SMOOTH);
					    Gl.glLineWidth(3f);
					
					    for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
							PolyLine pol = (PolyLine) shp.features[ii];
							for (int jj=0; jj < pol.Lines.Count; jj++)
							{
								//FIXME: Convert to 'AS'
								Line r = (Line) pol.Lines[jj];
							
							    Gl.glBegin(Gl.GL_LINES);
  							    Gl.glColor3d(0.2, 0.2, 0.2);
								
							    for (int kk = 0; kk < r.points.Count; kk++)
								{	
									//FIXME: Convert to 'AS'
									Point pt = (Point) r.points[kk];
								    Gl.glVertex2d(pt.X, pt.Y);
								}
							
							    Gl.glEnd();
							}
						}
					
					    Gl.glDisable(Gl.GL_LINE_SMOOTH);
					
						break;
					
					case Shapefile.ShapeType.Polygon:
					
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
						
						    for (int ll = 0; ll < 2; ll++)
						    {
								Polygon po = (Polygon) shp.features[ii];
								for (int jj=0; jj < po.Rings.Count; jj++)
								{
									if (ll == 0)
									{
									    Gl.glBegin(Gl.GL_POLYGON);
									    Gl.glColor3d(0.6, 0.8, 0.7);
									}
									else
										
									{
									Gl.glEnable(Gl.GL_LINE_SMOOTH);
										Gl.glBegin(Gl.GL_LINES);
									
									    Gl.glColor3d(0,0,0);
									}
								
								    //FIXME: Convert to 'AS'
									Ring r = (Ring) po.Rings[jj];

									for (int kk = 0; kk < r.Points.Count; kk++)
									{
										//FIXME: Convert to 'AS'
										Point pt = (Point) r.Points[kk];
									    Gl.glVertex2d(pt.X, pt.Y);
									}
	
	 							    Gl.glEnd();
								
									if (ll == 1)
									{
										Gl.glDisable(Gl.GL_LINE_SMOOTH);
									}
								
								}
							
						    }
						}
					
						break;
				}
			}

		}
	}
}
