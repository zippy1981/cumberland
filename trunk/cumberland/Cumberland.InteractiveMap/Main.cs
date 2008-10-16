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
using Cumberland.Drawing.OpenGL;

using NDesk.Options;

using Tao.FreeGlut;
using Tao.OpenGl;


namespace Cumberland.InteractiveMap
{
	class MainClass
	{
		// angle to radian
		const double ATR = .01745;
		
		static OpenGLMap map;
		
		static float[] rot = new float[] {0,0,0}; /* Amount to rotate */
		static float[] eye = new float[] {0,0,0}; /* Position of our eye or camera */
		static float[] light = new float[] {200, 100, 40}; 

		static bool[] keydown = new bool [256];
		
		static int pace = 1;
		
		static bool started = false;
		static int lastX, lastY;
		
		public static void Main(string[] args)
		{
#region initialize map configuration
			
			map = new OpenGLMap();
			map.Width = 400;
			map.Height = 400;
			map.Projection = ProjFourWrapper.WGS84;
			
			bool showHelp = false;
			
			OptionSet options = new OptionSet();
			options.Add("e|extents=", 
			            "comma-delimited extents (e.g. -180,-90,180,90) ",
			            delegate (string v) { map.Extents = ParseExtents(v); });
			options.Add("h|help",  "show this message and exit",
			            delegate (string v) { showHelp = v!= null; });
			options.Add("w|width=",
			            "the width of the image in pixels",
			            delegate (string v) { map.Width = int.Parse(v); });
			options.Add("t|height=",
			            "the height of the image in pixels",
			            delegate (string v) { map.Height = int.Parse(v); });
			options.Add("p|proj=",
			            "the output projection. can be a quoted proj4 string or an epsg code",
			            delegate (string v) { map.Projection = ParseProjection(v); });
			            
		
			List<string> rest = options.Parse(args);

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}

			Random r = new Random();
			
			foreach (string arg in rest)
			{
				string[] layerArgs = arg.Split(',');

				Layer l = new Layer();
				l.Data = new Shapefile(layerArgs[0]);
				l.PointSize = r.Next(5)+1;
				l.FillColor =  Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
				l.LineColor = Color.FromArgb(r.Next(155), r.Next(155), r.Next(155));
				//l.FillColor = Color.Blue;
				// l.LineColor = Color.RoyalBlue;
				l.LineWidth = 1; //r.Next(3)+1;
				
				if (layerArgs.Length > 1)
				{
					l.Projection = ParseProjection(layerArgs[1]);;
				}
				
				//l.LineStyle = LineStyle.None;
				
				map.Layers.Add(l);
			}
			
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
			
			// enable depth
			Gl.glEnable ( Gl.GL_DEPTH_TEST );
			
			// enable fill
			Gl.glPolygonMode ( Gl.GL_FRONT_AND_BACK, Gl.GL_FILL );	
			//Gl.glPolygonMode ( Gl.GL_FRONT_AND_BACK, Gl.GL_LINE );

			// set background to white
			Gl.glClearColor( 1.0f, 1.0f, 1.0f, 1.0f );
			
			// smooth shading
			Gl.glShadeModel ( Gl.GL_SMOOTH );
			
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
		
		static string ParseProjection(string v)
		{
			int epsg;
			if (int.TryParse(v, out epsg))
			{
				return "+init=epsg:" + v;
			}
			else return v;
		}		

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
			float[] matrix = Matrix.Transform ( rot[0], rot[1], rot[2], eye[0], eye[1], eye[2] );
			matrix = Matrix.Inverse ( matrix );
			Gl.glLoadMatrixf ( matrix );

			// set the lighting
			//Gl.glLightfv( Gl.GL_LIGHT0, Gl.GL_POSITION, light);
			
			// render
			map.Render(true);
			
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
			//Glu.gluPerspective(30.0, (float) w / (float) h, 1.0, 20.0);
			Glu.gluPerspective (90, map.Width / map.Height, 1, 9999);
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