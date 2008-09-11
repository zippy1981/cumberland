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
		List<Shapefile> layers = new List<Shapefile>();
		
		public List<Shapefile> Layers {
			get {
				return layers;
			}
			set {
				layers = value;
			}
		}
	
		public Bitmap Draw(int width, int height)
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
				
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glLoadIdentity();
				Gl.glOrtho(0, width, 0, height, 0, 1);
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				
				if (!Gl.glGetString(Gl.GL_EXTENSIONS).Contains("GL_EXT_framebuffer_object"))
				{
					throw new NotSupportedException("Your video card does not support frame buffers");	
				}
				
//				// set up and bind frame buffer
//				Gl.glGenFramebuffersEXT(1, temp);
//				fbo = temp[0];
//				System.Console.WriteLine(fbo);
//				Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, fbo);
//	
//				// create, bind, and associate our depth buffer to the frame buffer			
////				Gl.glGenRenderbuffersEXT(1, temp);
////				depthBuffer = temp[0];
////				Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, depthBuffer);
////				Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_DEPTH_COMPONENT, width, height);	
////				Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_RENDERBUFFER_EXT, depthBuffer);
//								
//				// create, bind, and associate our color buffer to the frame buffer	
//				Gl.glGenRenderbuffersEXT(1, temp);
//				colorBuffer = temp[0];
//				Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, colorBuffer);
//				Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_RGBA, width, height);
//				Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_RENDERBUFFER_EXT, colorBuffer);
//				
//				System.Console.WriteLine(Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT));
//				System.Console.WriteLine( "error: " + Gl.glGetError());
//
//				
//				// check state
//				if (Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT) != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
//					throw new InvalidOperationException("This video card may not support Framebuffers");

				//Gl.glClearColor(1f, 1f, 1f, 1f);
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
				
				// render here
				Render(layers);
				
				Gl.glFlush(); 
				
				b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				//Gl.glPixelStorei(Gl.GL_PACK_ALIGNMENT, 3);
				Gl.glReadPixels(0, 0, width, height, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, bd.Scan0);
				b.UnlockBits(bd);
				b.RotateFlip(RotateFlipType.Rotate180FlipX);
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
		
		public void Render(List<Shapefile> shapes)
		{
			Rectangle env = new Rectangle(0,0, 10, 10);
			
			double height = 100;
			double width = 100;
			
			// FIXME: These will only change with resizing or new map coords
			// So move.
			double xratio = width / Math.Abs(env.Max.X - env.Min.X);
			double yratio = height / Math.Abs(env.Max.Y - env.Min.Y);
			
//			pixmap.DrawRectangle (this.Style.WhiteGC, true, 0, 0,
//			this.Allocation.Width, this.Allocation.Height);		
		
			foreach (Shapefile shp in shapes)
			{
				if (shp.features.Count == 0)
				{
					continue;
				}

				uint ctest = 0;
				switch (shp.Shapetype)
				{
					case Shapefile.ShapeType.Point:
					
					    Gl.glBegin(Gl.GL_POINT);
					
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							//FIXME: Convert to 'AS'
							Point p = (Point) shp.features[ii];
							if (p.X >= env.Min.X && p.X <= env.Max.X && p.Y >= env.Min.Y && p.Y <= env.Max.Y)
							{		
								ctest++;
								//FIXME: Redundancy with all cases.  move ToMapPoint
								//int px = Convert.ToInt32( (p.X - env.Min.X) * xratio);						
								//int py = Convert.ToInt32( height - ((p.Y - env.Min.Y) * yratio));
								//pixmap.DrawRectangle (this.Style.BlackGC, true, px, py, 2, 2);
								//pxt.DrawRectangle (darea.Style.BlackGC, true, px, py, 2, 2);
							
							    Gl.glVertex2d(p.X, p.Y);
							}					
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
								//pixmap.DrawLines(this.Style.BlackGC, pts);
								//pxt.DrawLines(darea.Style.BlackGC, pts);
							
							    Gl.glEnd();
							}
							//Console.WriteLine(Convert.ToDouble(ii)/Convert.ToDouble(shp.features.Count));
							//pb.Fraction = Convert.ToDouble(ii)/Convert.ToDouble(shp.features.Count);
						}
						break;
					
					case Shapefile.ShapeType.Polygon:
					
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
						
							Polygon po = (Polygon) shp.features[ii];
							for (int jj=0; jj < po.Rings.Count; jj++)
							{
							    Gl.glBegin(Gl.GL_POLYGON);
							
							    //FIXME: Convert to 'AS'
								Ring r = (Ring) po.Rings[jj];
								//Gdk.Point[] pts = new Gdk.Point[env.points.Count];
								for (int kk = 0; kk < r.Points.Count; kk++)
								{
									//FIXME: Convert to 'AS'
									Point pt = (Point) r.Points[kk];
									//FIXME: Redundancy with all cases.  move ToMapPoint
									int pox = Convert.ToInt32( (pt.X - env.Min.X) * xratio);						
									int poy = Convert.ToInt32( height - ((pt.Y - env.Min.Y) * yratio));
									//pts[kk] = new Gdk.Point(pox,poy);
								
								    Gl.glVertex2d(pt.X, pt.Y);
								}

 							    Gl.glEnd();
							
								//Gdk.Color red_color = new Gdk.Color (0xff, 0, 0);
								//Gdk.GC poly_gc = new Gdk.GC(pixmap);
								//poly_gc.Background = new Gdk.Color (0xff, 0, 0);
								//oly_gc.Foreground = new Gdk.Color (0xff, 0, 0);
					            //Gdk.GC gc = new Gdk.GC (pixmap);
           						//Gdk.Color[] colors = new Gdk.Color[2];
           					 	//colors[0] = new Gdk.Color (0xff, 0, 0);
           					 	//colors[0] = new Gdk.Color (shp.backcolor[0], shp.backcolor[1], shp.backcolor[2]);
           					 	//colors[1] = new Gdk.Color (shp.forecolor[0], shp.forecolor[1], shp.forecolor[2]);            					 
						       	//bool[] suc = new bool[2];
				        	    // Use the system colormap, easy.
				            	//Gdk.Colormap colormap = Gdk.Colormap.System;
					            //colormap.AllocColor (ref red_color, true, true);
				     			//colormap.AllocColors (shp.colors, 2, true, true, suc);
				               	//gc.Foreground = shp.colors[0];
				               	//gc.Background = colors[1];
				               	//gc.Fill = Fill.Solid;
								//pixmap.DrawPolygon(this.Style.BlackGC, false, pts);
								//pixmap.DrawPolygon(gc, true, pts);
								//gc.Foreground = shp.colors[1];
								//pixmap.DrawPolygon(gc, false, pts);
								//pxt.DrawPolygon(gc, false, pts);
							}
						}
					
						break;
						
				}
				//Console.WriteLine(ctest + " Features Drawn");
				//Console.Write(px + "," + py + " ");
			}

		}
	}
}
