// Main.cs
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
using System.Collections.Generic;
using System.Drawing;

using Cumberland;
using Cumberland.Data.PostGIS;
using Cumberland.Data.Shapefile;
using Cumberland.Drawing.OpenGL;
using Cumberland.Projection;
using Cumberland.Xml.Serialization;

using NDesk.Options;

using Tao.FreeGlut;
using Tao.OpenGl;


namespace Cumberland.InteractiveMap
{
	class MainClass
	{
		// angle to radian
		const double ATR = .01745;
		
		static Map map;
		static OpenGlMapDrawer renderer;
		
		static double[] rot = new double[] {0,0,0}; /* Amount to rotate */
		static double[] eye = new double[] {0,0,0}; /* Position of our eye or camera */
		static double[] light = new double[] {200, 100, 40}; 

		static bool[] keydown = new bool [256];
		
		static int pace = 1;
		
		static bool started = false;
		static int lastX, lastY;
		
		// the field of view y angle:
		// a small value will make the view fish-eyed, i.e. zoom in/out fast
		// 45-90 is normal 
		static double fovy = 90;
		
		
		public static void Main(string[] args)
		{
			renderer = new OpenGlMapDrawer();
			
#region initialize map configuration
			
			bool showHelp = false;
			
			Rectangle ext = new Rectangle();
			int w = -1;
			int h = -1;
			
			OptionSet options = new OptionSet();
			options.Add("e|extents=", 
			            "comma-delimited extents (e.g. -180,-90,180,90) ",
			            delegate (string v) { ext = ParseExtents(v); });
			options.Add("h|help",  "show this message and exit",
			            delegate (string v) { showHelp = v!= null; });
			options.Add("w|width=",
			            "the width of the image in pixels",
			            delegate (string v) { w = int.Parse(v); });
			options.Add("t|height=",
			            "the height of the image in pixels",
			            delegate (string v) { h = int.Parse(v); });       
		
			List<string> rest = options.Parse(args);

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}

			MapSerializer ms = new MapSerializer();
			ms.AddDatabaseFeatureSourceType(typeof(PostGISFeatureSource));
			
			map = ms.Deserialize(rest[0]);
			if (w>0) map.Width = w;
			if (h>0) map.Height = h;
			if (!ext.IsEmpty) map.Extents = ext;
			
			// set initial "eye" point in 3d space
			Point c = map.Extents.Center;
			eye[0] = Convert.ToSingle(c.X);
			eye[1] = Convert.ToSingle(c.Y);
			
			// we need to expand our extents so that the aspect ratio is the same as the viewport
			Cumberland.Rectangle rec = map.Extents.Clone();
			rec.AspectRatioOfWidth = Convert.ToDouble(map.Width) / Convert.ToDouble(map.Height);
			
			// trigonometry to acquire z coordinate
			// by using the right triangle created by:
			// 1) the center point of the map, 
			// 2) the top-center of the map
			// 3) the eye
			// there we can calculate the side (z-value) by
			// b = a/Tan(A)
			// FIXME: not quite right
			eye[2] = (rec.Height/2) / Math.Tan(fovy/2); 
			
#endregion
			
#region init freeglut
			
				// instantiate GLUT for our windowing provider
			Glut.glutInit();
			//Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_RGB | Glut.GLUT_ALPHA | Glut.GLUT_DEPTH);
			Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_RGB | Glut.GLUT_DEPTH);
			Glut.glutInitWindowSize( map.Width, map.Height );
			Glut.glutCreateWindow("Cumberland Map Viewer");

#endregion

#region init OpenGL
			
			// don't enable depth.  looks worse
			//Gl.glEnable ( Gl.GL_DEPTH_TEST );
			
			// enable fill
			Gl.glPolygonMode ( Gl.GL_FRONT_AND_BACK, Gl.GL_FILL );	
			//Gl.glPolygonMode ( Gl.GL_FRONT_AND_BACK, Gl.GL_LINE );

			// set background to white
			Gl.glClearColor( 1.0f, 1.0f, 1.0f, 1.0f );
			
			// smooth shading
			//Gl.glShadeModel ( Gl.GL_SMOOTH );
			
			// enable culling (improves performance i think)
			//Gl.glEnable(Gl.GL_CULL_FACE);
			//Gl.glCullFace(Gl.GL_BACK);
			
			//Gl.glBlendFunc(Gl.GL_SRC_ALPHA_SATURATE, Gl.GL_ONE)

			// enable lighting
//			Gl.glEnable( Gl.GL_LIGHT0 );
//			Gl.glEnable( Gl.GL_LIGHTING );
//			Gl.glLightModeli( Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);

#endregion
			
			// instantiate GLUT event handlers
			Glut.glutDisplayFunc(new Glut.DisplayCallback(Display));
			Glut.glutIdleFunc(new Glut.IdleCallback (Idle) );
			Glut.glutKeyboardFunc(new Glut.KeyboardCallback(Keyboard));
			Glut.glutKeyboardUpFunc(new Glut.KeyboardUpCallback(KeyboardUp));
			Glut.glutReshapeFunc(new Glut.ReshapeCallback(Reshape));
			Glut.glutMotionFunc (new Glut.MotionCallback (Motion) );
			
			// start loop and wait for user input
			Glut.glutMainLoop();
		}
		
#region helper methods
		
		static void ShowHelp (OptionSet p)
	    {
	        Console.WriteLine ("Usage: [mono] Cumberland.DrawMap.exe [OPTIONS]+ [\"path to shapefile\",epsg/\"proj4 string\"]+ ");
	        Console.WriteLine ("Draws a map based on the given layers and options.");
	        Console.WriteLine ();
			Console.WriteLine ("example: mono Cumberland.DrawMap.exe -o=my.png -p=4326 \"path/to/shape.shp\",3087  ");
			Console.WriteLine ();
	        Console.WriteLine ("Options:");
	        p.WriteOptionDescriptions (Console.Out);
	    }
		
		static Rectangle ParseExtents(string extents)
		{
			string[] coords = extents.Split(',');
			return new Rectangle(Convert.ToDouble(coords[0]),
			                     Convert.ToDouble(coords[1]),
			                     Convert.ToDouble(coords[2]),
			                     Convert.ToDouble(coords[3]));
		}
		
//		static string ParseProjection(string v)
//		{
//			int epsg;
//			if (int.TryParse(v, out epsg))
//			{
//				return "+init=epsg:" + v;
//			}
//			else return v;
//		}		

		static void Clamp ()
		{
			for (int i = 0; i < 3; i ++)
				if (rot[i] >= 360 || rot[i] < -360)
					rot[i] = 0;
		}
		
#endregion
		
#region FreeGlut callbacks
		
		static void Display() 
		{
			// clear out the current OpenGL context
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			//Gl.glDisable(Gl.GL_BLEND);
			//Gl.glDisable(Gl.GL_POLYGON_SMOOTH);

			// create our view matrix and load into OpenGL
			double[] matrix = MatrixHelper.Transform ( rot[0], rot[1], rot[2], eye[0], eye[1], eye[2] );
			matrix = MatrixHelper.Inverse ( matrix );
			Gl.glLoadMatrixd ( matrix );

			// set the lighting
			//Gl.glLightfv( Gl.GL_LIGHT0, Gl.GL_POSITION, light);
			
			// render
			renderer.Render(map, true);
			
			// flush out what in the OpenGL context
			Gl.glFlush ();
			
			// we are double buffering
			Glut.glutSwapBuffers ();
		}
		
		static void Keyboard(byte key, int x, int y) { keydown [ key ] = true; }
		
		static void KeyboardUp(byte key, int x, int y) { keydown [ key ] = false; }

		static void Idle ()
		{
			// go through the keys and deal with user input
			for ( int ii=0; ii < keydown.Length ; ii++ )
			{
				if ( keydown [ii] )
				{
					switch ( (char) ii) 
					{
						case 'x':
							eye[1]++;
							break;
						case 'z':
							eye[1]--;
							break;
						case 'w':
							eye[0] -= (float) Math.Sin (rot[1]*ATR) * pace;
							eye[2] -= (float) Math.Cos (rot[1]*ATR) * pace;
							break;
						case 's':
							eye[0] += (float) Math.Sin(rot[1]*ATR) * pace;
							eye[2] += (float) Math.Cos(rot[1]*ATR) * pace;
							break;
						case 'a':
							rot[1] += pace;
							break;
						case 'd':
							rot[1] -= pace;
							break;
						case '=':
							light[0]++;
							break;
						case '-':
							light[0]--;
							break;
						case (char) 27:
							Environment.Exit(0);
							break;
						default:
							break;
					}
				}
			}
			
			// render the model
			Glut.glutPostRedisplay();	
		}
		
		static void Reshape(int w, int h) 
		{
			Gl.glViewport(0, 0, w, h);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			Glu.gluPerspective (fovy, Convert.ToDouble(map.Width) / Convert.ToDouble(map.Height), 1, 9999);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
		}
		
		static void Motion ( int x, int y )
		{
			// changes rotation
			if (started && x > 0 && y > 0 && x < map.Width && y < map.Height)
			{
				rot[1] -= (float) (x-lastX)/3;
				rot[0] += (float) (y-lastY)/3;
			}
			else started = true;

			// track last val
			lastY = y;
			lastX = x;
			
			// keep rotation values sane
			Clamp ();

			// redraw
			Glut.glutPostRedisplay();	
		}
		
#endregion
	

	}
}