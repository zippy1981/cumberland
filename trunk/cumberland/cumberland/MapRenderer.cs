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
//using Gtk;
//using Gdk;

// obviously this isn't done

/*
namespace Cumberland 
{
	public class MapGdkCanvas : DrawingArea
	{
		Gdk.Pixmap pixmap;
		MainWindow mainWin;
	
		public MapGdkCanvas (MainWindow mw) : base ()
		{
			mainWin = mw;
		
			this.SetSizeRequest (mainWin.da_width, mainWin.da_height);
			this.ExposeEvent += new ExposeEventHandler (ExposeCanvas);
			this.ConfigureEvent += new ConfigureEventHandler (ConfigureCanvas);
			this.ButtonPressEvent += new ButtonPressEventHandler (MyButtonPressEvent);			
			this.MotionNotifyEvent += new MotionNotifyEventHandler (MouseOverCanvas);
			this.AddEvents((int)EventMask.ExposureMask |
							   (int)EventMask.LeaveNotifyMask |
							   (int)EventMask.ButtonPressMask |
							   (int)EventMask.PointerMotionMask |
							   (int)EventMask.PointerMotionHintMask);
		}

		void ExposeCanvas (object obj, ExposeEventArgs args)
		{
			//Console.WriteLine("ExposeEvent");
			// Redraw stored pixmap of map
			Gdk.Rectangle area = args.Event.Area;
			args.Event.Window.DrawDrawable (this.Style.WhiteGC, pixmap,
					area.X, area.Y,
					area.X, area.Y,
					area.Width, area.Height);
		}

		void ConfigureCanvas (object obj, ConfigureEventArgs args)
		{
			//Console.WriteLine("ConfigureEvent");
			Gdk.EventConfigure ev = args.Event;
			Gdk.Window window = ev.Window;
			Gdk.Rectangle allocation = this.Allocation;

			if (pixmap == null)
			{
				pixmap = new Gdk.Pixmap (window, this.Allocation.Width,
					this.Allocation.Height, -1);
				pixmap.DrawRectangle (this.Style.WhiteGC, true, 0, 0,
					this.Allocation.Width, this.Allocation.Height);
			}
			else if (mainWin.da_width != allocation.Width || 
				mainWin.da_height != allocation.Height)
			{
				pixmap = new Gdk.Pixmap (window, this.Allocation.Width,
					this.Allocation.Height, -1);
				pixmap.DrawRectangle (this.Style.WhiteGC, true, 0, 0,
					this.Allocation.Width, this.Allocation.Height);
				
				// FIXME: Should resizing map affect extents/ratio?
				//da_width = allocation.Width;
				//da_height = allocation.Height;
				// Resizing should roll up and affect the app properties
				mainWin.AdjustExtents();
				DrawMap();
			}
			else
			{
				args.Event.Window.DrawDrawable (this.Style.WhiteGC, pixmap,
					-1, -1,
					-1, -1,
					-1, -1);
			}			
		}
	
		public void ClearCanvas()
		{
			//pixmap = new Gdk.Pixmap (window, this.Allocation.Width,
			//	this.Allocation.Height, -1);
			pixmap.DrawRectangle (this.Style.WhiteGC, true, 0, 0,
				this.Allocation.Width, this.Allocation.Height);	
			QueueDraw();					
		}

		void MouseOverCanvas (object obj, MotionNotifyEventArgs args)
		{
			if (mainWin.ActiveTool > 0)
				ParentWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Crosshair);

			int x, y;
			//Gdk.ModifierType state;
			Gdk.EventMotion ev = args.Event;
			Gdk.Window window = ev.Window;

			if (ev.IsHint) {
				Gdk.ModifierType s;
				window.GetPointer (out x, out y, out s);
				//state = s;
			} else {
				x = (int) ev.X;
				y = (int) ev.Y;
				//state = ev.State;
			}
			
			//TODO: Convert to Map Coords
			//Console.WriteLine(x + " " + y);
			mainWin.sb.Push(1,"x: " + x + " y:" + y);
		}	

		void MyButtonPressEvent (object obj, ButtonPressEventArgs args)
		{
			//Console.WriteLine ("<<ButtonPressEvent>>");
			if (mainWin.shps.Count == 0) return;

			Gdk.EventButton e = args.Event;
			Appomattox.Point max = mainWin.max;
			Appomattox.Point min = mainWin.min;
			
			// Grab the width and height in map coordinates
			double mwidth = Math.Abs(max.X - min.X);
			double mheight = Math.Abs(max.Y - min.Y);
	
			// calculate the ratio of map to image coordinates
			double xratio = mwidth / mainWin.image_width;
			double yratio = mheight / mainWin.image_height;

			// calculate the map coordinates of the pixel coords
			double mx = min.X + (e.X * xratio);
			double my = min.Y + ((mainWin.image_height - e.Y) * yratio);

			if (e.Button == 1)
			{ 
				switch (mainWin.ActiveTool)
				{
					case (uint) MainWindow.GisTools.ZoomIn:
						mwidth /= 2;
						mheight /= 2;
						break;
					case (uint) MainWindow.GisTools.ZoomOut:
						mwidth *= 2;
						mheight *= 2;
						break;
				}

				// TODO: Write function to update extents.
				min.X = mx - mwidth/2;
				min.Y = my - mheight/2;
				max.X = mx + mwidth/2;
				max.Y = my + mheight/2;
	
				DrawMap();
			}
		}
		
		public void DrawMap()
		{

			if (mainWin.shps.Count == 0) 
			{
				ClearCanvas();
				return;
			}

			Appomattox.Point max = mainWin.max;
			Appomattox.Point min = mainWin.min;
			
			double height = mainWin.image_height;
			double width = mainWin.image_width;
			
			// FIXME: These will only change with resizing or new map coords
			// So move.
			double xratio = width / Math.Abs(max.X - min.X);
			double yratio = height / Math.Abs(max.Y - min.Y);
			
			pixmap.DrawRectangle (this.Style.WhiteGC, true, 0, 0,
			this.Allocation.Width, this.Allocation.Height);		
		
			foreach (Shapefile shp in mainWin.shps)
			{
				if (shp.features.Count == 0) continue;

				//Console.WriteLine(shp.filename);

				uint ctest = 0;
				switch (shp.shapetype)
				{
					case 1:
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							//FIXME: Convert to 'AS'
							Appomattox.Point p = (Appomattox.Point) shp.features[ii];
							if (p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y)
							{		
								ctest++;
								//FIXME: Redundancy with all cases.  move ToMapPoint
								int px = Convert.ToInt32( (p.X - min.X) * xratio);						
								int py = Convert.ToInt32( height - ((p.Y - min.Y) * yratio));
								pixmap.DrawRectangle (this.Style.BlackGC, true, px, py, 2, 2);
								//pxt.DrawRectangle (darea.Style.BlackGC, true, px, py, 2, 2);
							}					
						}
						break;
					case 3:
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
							PolyLine pol = (PolyLine) shp.features[ii];
							for (int jj=0; jj < pol.lines.Count; jj++)
							{
								//FIXME: Convert to 'AS'
								Line r = (Line) pol.lines[jj];
								Gdk.Point[] pts = new Gdk.Point[r.points.Count];
								for (int kk = 0; kk < r.points.Count; kk++)
								{	
									//FIXME: Convert to 'AS'
									Appomattox.Point pt = (Appomattox.Point) r.points[kk];
									//FIXME: Redundancy with all cases.  move ToMapPoint
									int pox = Convert.ToInt32( (pt.X - min.X) * xratio);						
									int poy = Convert.ToInt32( height - ((pt.Y - min.Y) * yratio));
									pts[kk] = new Gdk.Point(pox,poy);
								}
								pixmap.DrawLines(this.Style.BlackGC, pts);
								//pxt.DrawLines(darea.Style.BlackGC, pts);
							}
							//Console.WriteLine(Convert.ToDouble(ii)/Convert.ToDouble(shp.features.Count));
							//pb.Fraction = Convert.ToDouble(ii)/Convert.ToDouble(shp.features.Count);
						}
						break;							
					case 5:
						for (int ii=0; ii < shp.features.Count; ii++)
						{
							ctest++;
							
							Polygon po = (Polygon) shp.features[ii];
							for (int jj=0; jj < po.rings.Count; jj++)
							{
								//FIXME: Convert to 'AS'
								Ring r = (Ring) po.rings[jj];
								Gdk.Point[] pts = new Gdk.Point[r.points.Count];
								for (int kk = 0; kk < r.points.Count; kk++)
								{
									//FIXME: Convert to 'AS'
									Appomattox.Point pt = (Appomattox.Point) r.points[kk];
									//FIXME: Redundancy with all cases.  move ToMapPoint
									int pox = Convert.ToInt32( (pt.X - min.X) * xratio);						
									int poy = Convert.ToInt32( height - ((pt.Y - min.Y) * yratio));
									pts[kk] = new Gdk.Point(pox,poy);
								}

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
								pixmap.DrawPolygon(this.Style.BlackGC, false, pts);
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
			QueueDraw();
		}
	}
}
*/