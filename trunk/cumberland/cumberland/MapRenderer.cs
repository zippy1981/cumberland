
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

using Cumberland.GluWrap;

namespace Cumberland 
{
	public class MapRenderer
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

#region Methods
				
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
				int glutTime = Glut.glutGet(Glut.GLUT_ELAPSED_TIME);
				//System.Console.WriteLine(glutTime);
				if (glutTime == 0)
				{		
					Glut.glutInit();
				}
				
				Glut.glutCreateWindow(string.Empty);
				Glut.glutHideWindow();
				
				// test for fbo support
				string glext = Gl.glGetString(Gl.GL_EXTENSIONS);
				if (glext != null && !glext.Contains("GL_EXT_framebuffer_object"))
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
				
//				Gl.glGetIntegerv(Gl.GL_MAX_SAMPLES_EXT, temp);
//				System.Console.WriteLine(temp[0]);
				
				// allocate memory
				Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_RGBA, width, height);
				//Gl.glRenderbufferStorageMultisampleEXT(Gl.GL_RENDERBUFFER_EXT, 16, Gl.GL_RGBA, width, height);
				//Gl.glRenderbufferStorageMultisampleCoverageNV(Gl.GL_RENDERBUFFER_EXT, 4, 16, Gl.GL_RGBA, width, height);
				
				// attach render buffer to fbo
				Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_RENDERBUFFER_EXT, colorBuffer);

				// check state
				if (Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT) != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
				{
					throw new InvalidOperationException("An error has occured setting up Framebuffers");
				}
				
				// acquire the proper space
				Gl.glViewport(0, 0, width, height);
				
				// clear out
				Gl.glClearColor(1f, 1f, 1f, 0f);
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
				
				// switch to projection matrix
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glLoadIdentity();
				
				Rectangle r = extents.Clone();

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
						
				// acquire the pixels from openGL and draw to bitmap
				b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				Gl.glReadPixels(0, 0, width, height, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bd.Scan0);
				b.UnlockBits(bd);
				b.RotateFlip(RotateFlipType.Rotate180FlipX);
				
				// check for openGL errors
				int errorcode;
				if ((errorcode = Gl.glGetError()) != Gl.GL_NO_ERROR)
				{
					throw new InvalidOperationException("OpenGL reports an error: " + GluWrap.GluMethods.gluErrorString(errorcode));
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
			ProjFourWrapper dst = null;
			
			try
			{
				// instantiate map projection
				if (!string.IsNullOrEmpty(Projection))
				{
					dst = new ProjFourWrapper(Projection);
				}
								
				foreach (Layer layer in layers)
				{
					if (layer.Data == null)
					{
						continue;
					}
					
					Shapefile shp = layer.Data;
					
					if (shp.Features.Count == 0)
					{
						continue;
					}
	
					ProjFourWrapper src = null;
					
					try
					{
						// instantiate layer projection
						bool reproject = false;
						if (dst != null && !string.IsNullOrEmpty(layer.Projection))
						{
							src = new ProjFourWrapper(layer.Projection);
							reproject = true;
						}
					
						if (shp.Shapetype == Shapefile.ShapeType.Point)
					    {	
		#region handle point rendering
							
							Gl.glPointSize(layer.PointSize);
						    Gl.glBegin(Gl.GL_POINTS);
							
							Gl.glColor4ub(layer.FillColor.R, layer.FillColor.G, layer.FillColor.B, layer.FillColor.A);
						
							for (int ii=0; ii < shp.Features.Count; ii++)
							{
								Point p = shp.Features[ii] as Point;
									
								if (reproject)
								{
									p = src.Transform(dst, p);
								}
									
								Gl.glVertex2d(p.X, p.Y);			
							}
						
						    Gl.glEnd();
	
		#endregion
						}
						else if (shp.Shapetype == Shapefile.ShapeType.PolyLine)
						{
		#region Handle line rendering
							
							if (layer.LineStyle == LineStyle.None)
							{
								continue;
							}
							
							// enable anti-aliasing					
						    Gl.glEnable(Gl.GL_LINE_SMOOTH);
							Gl.glEnable(Gl.GL_BLEND);
							
							if (layer.LineStyle != LineStyle.Solid)
							{
								Gl.glEnable(Gl.GL_LINE_STIPPLE);
								Gl.glLineStipple(3, (short) layer.LineStyle);
							}
							
							// to get the anti-aliased line to properly blend with the background color,
							// we tell OpenGL to:
							// multiply the R,G,B values of the line times it's alpha value 
							// this alters the color by it's opacity
							// next, we multiply the R,G,B values of the background times 1-the line's alpha value
							// this applies the remaining alpha value to the background color
							// (i.e. if the line is fully opaque, we want no interaction from the background color)
							// the alpha values are preserved as is
							// lastly, the two colors are merged (added together)
							// http://glprogramming.com/red/chapter06.html#name1
							Gl.glBlendFuncSeparate(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA, Gl.GL_ONE, Gl.GL_ONE);
							
							Gl.glLineWidth(layer.LineWidth);
						
						    for (int ii=0; ii < shp.Features.Count; ii++)
							{
								PolyLine pol = (PolyLine) shp.Features[ii];
								for (int jj=0; jj < pol.Lines.Count; jj++)
								{
									Line r = pol.Lines[jj] as Line;
								
								    Gl.glBegin(Gl.GL_LINES);
									Gl.glColor4ub(layer.LineColor.R, layer.LineColor.G, layer.LineColor.B, layer.LineColor.A);
									
									// GL_LINES requires two points
									Point prev = r.Points[0];
									if (reproject)
									{
										prev = src.Transform(dst, prev);
									}
									
								    for (int kk = 1; kk < r.Points.Count; kk++)
									{	
										Point p = r.Points[kk];
										
										if (reproject)
										{
											p = src.Transform(dst, p);
										}
										
									    Gl.glVertex2d(prev.X, prev.Y);
										Gl.glVertex2d(p.X, p.Y);
										
										prev = p;
									}
								
								    Gl.glEnd();
								}
							}
						
							Gl.glDisable(Gl.GL_BLEND);
						    Gl.glDisable(Gl.GL_LINE_SMOOTH);
							
							if (layer.LineStyle != LineStyle.Solid)
							{
								Gl.glDisable(Gl.GL_LINE_STIPPLE);
							}
							
		#endregion
						}	
						else if (shp.Shapetype == Shapefile.ShapeType.Polygon)
						{
							IntPtr tess = IntPtr.Zero;
							
							try
							{
								tess = GluMethods.gluNewTess();
						
								Glu.TessBeginCallback tessBegin = new Tao.OpenGl.Glu.TessBeginCallback(Gl.glBegin);
								Glu.TessEndCallback tessEnd = new Tao.OpenGl.Glu.TessEndCallback(Gl.glEnd);
								Glu.TessVertexCallback tessVert = new Tao.OpenGl.Glu.TessVertexCallback(Gl.glVertex3dv);
										
								GluMethods.gluTessCallback(tess, Glu.GLU_TESS_BEGIN, tessBegin);
								GluMethods.gluTessCallback(tess, Glu.GLU_TESS_END, tessEnd);
								GluMethods.gluTessCallback(tess, Glu.GLU_TESS_VERTEX, tessVert);
								
								for (int ii=0; ii < shp.Features.Count; ii++)
								{
									Polygon po = (Polygon) shp.Features[ii];

#region tessalate and render polygon fill
									
									for (int jj = 0; jj < po.Rings.Count; jj++)
								    {
										Ring r = po.Rings[jj];
										
										// FIXME: not working.  maybe need to use multisampling
										if (layer.LineStyle == LineStyle.None)
										{
											Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
											Gl.glEnable(Gl.GL_BLEND);
											Gl.glBlendFuncSeparate(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA, Gl.GL_ONE, Gl.GL_ONE);
										}
										
										// TODO: use display lists for interactive viewer
										//int tl = Gl.glGenLists(1);
									    
									    Gl.glColor4ub(layer.FillColor.R, layer.FillColor.G, layer.FillColor.B, layer.FillColor.A);
									
										//Gl.glNewList(tl, Gl.GL_COMPILE);
										
										GluMethods.gluTessBeginPolygon(tess, IntPtr.Zero);
										GluMethods.gluTessBeginContour(tess);
										
										for (int kk = 1; kk < r.Points.Count; kk++)
										{
											Point p = r.Points[kk];
											
											if (reproject)
											{
												p = src.Transform(dst, p);
											}
											
											double[] data = new double[] {p.X, p.Y};
											GluMethods.gluTessVertex(tess, data, data );
										}	
										
										GluMethods.gluTessEndContour(tess);
										GluMethods.gluTessEndPolygon(tess);
										
										if (layer.LineStyle == LineStyle.None)
										{
											Gl.glDisable(Gl.GL_BLEND);
											Gl.glDisable(Gl.GL_POLYGON_SMOOTH);
										}
	
										//Gl.glEndList();
										
		#endregion
										
			#region draw polygon outline
			
										if (layer.LineStyle != LineStyle.None)
										{
									
											if (layer.LineStyle != LineStyle.Solid)
											{
												Gl.glEnable(Gl.GL_LINE_STIPPLE);
												Gl.glLineStipple(1, (short) layer.LineStyle);
											}
											
											Gl.glEnable(Gl.GL_LINE_SMOOTH);
											Gl.glEnable(Gl.GL_BLEND);
											
											//Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
											//Gl.glBlendFunc(Gl.GL_SRC_ALPHA_SATURATE, Gl.GL_ONE_MINUS_SRC_ALPHA);
											Gl.glBlendFuncSeparate(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA, Gl.GL_ONE, Gl.GL_ONE);
											
											Gl.glLineWidth(layer.LineWidth);
											
										    Gl.glBegin(Gl.GL_LINES);
										    Gl.glColor4ub(layer.LineColor.R, layer.LineColor.G, layer.LineColor.B, layer.LineColor.A);
												
											// GL_LINES requires two points 
											Point prev = r.Points[0];
											if (reproject)
											{
												prev = src.Transform(dst, prev);
											}
											
										    for (int kk = 1; kk < r.Points.Count; kk++)
											{	
												Point p = r.Points[kk];
												
												if (reproject)
												{
													p = src.Transform(dst, p);
												}
												
											    Gl.glVertex2d(prev.X, prev.Y);
												Gl.glVertex2d(p.X, p.Y);
												
												prev = p;
											}							
				
				 							Gl.glEnd();
											Gl.glDisable(Gl.GL_BLEND);
											Gl.glDisable(Gl.GL_LINE_SMOOTH);
											
											if (layer.LineStyle != LineStyle.Solid)
											{
												Gl.glDisable(Gl.GL_LINE_STIPPLE);
											}								
											
										}
			#endregion
								    }
								}
								
							}
							finally
							{
								if (tess != IntPtr.Zero) 
								{
									GluMethods.gluDeleteTess(tess);
								}
							}
						}
					}
					finally
					{
						// dispose of proj4
						if (src != null)
						{
							src.Dispose();
						}
					}
				}
			}
			finally
			{
				// dispose of proj4
				if (dst != null)
				{
					dst.Dispose();
				}
			}
		}
#endregion
	}
}
