// Shapefile.cs
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
using System.IO;
using System.Collections;

namespace Cumberland 
{
    public class Shapefile 
	{
        public Point min;
        public Point max;
		private uint filelength;
        public uint shapetype;
		public uint version;
		public string filename;
		//spublic int stamp;

		//FIXME: temporary location
		//public Gdk.Color[] colors;
		public byte[] forecolor, backcolor;
		
		public ArrayList features;

        public enum ShapeTypes
        {
            Null        = 0,
            Point       = 1,
            PolyLine    = 3,
            Polygon     = 5,
            MultiPoint  = 8,
            PointZ      = 11,
            PolyLineZ   = 13,
            PolygonZ    = 15,
            MultiPointZ = 18,
            PointM      = 21,
            PolyLineM   = 23,
            PolygonM    = 25,
            MultiPointM = 28,
            MultiPatch  = 31
        }

        public Shapefile(string fname)
        {
	        FileStream file;

			file = new FileStream(fname, FileMode.Open, FileAccess.Read); 
			features = new ArrayList();
           	
			BinaryReader str = new BinaryReader(file);
            str.BaseStream.Seek(0, SeekOrigin.Begin); 
			
            ReadFileHeader(str);
			ReadShapeRecords(str);
			
            file.Close();
			
            filename = fname.Substring(fname.LastIndexOf('/')+1);
            
            //FIXME: temporary location
			Random randomgen = new Random();
			forecolor = new byte[4];
			backcolor = new byte[4];
			randomgen.NextBytes(forecolor);
			randomgen.NextBytes(backcolor);
			//colors = new Gdk.Color[2];
			//colors[0] = new Gdk.Color(backcolor[0], backcolor[1],backcolor[2]);
			//colors[1] = new Gdk.Color(forecolor[0], forecolor[1],forecolor[2]);
		}

        private uint FlipEndian(uint iin)
        {
            byte[] temp = BitConverter.GetBytes(iin);
            Array.Reverse(temp);
            return BitConverter.ToUInt32(temp,0);
        }

        private void ReadFileHeader(BinaryReader stream)
        {
            // grab the File Code
            uint filecode = FlipEndian(stream.ReadUInt32());

            if (filecode != 9994)
            {
                Console.WriteLine("ERROR: This file does not appear to be a shapefile");
            }

            // the next 20 bytes are junk
            byte[] junk = new byte[20];
            stream.Read(junk,0,junk.Length);

            // grab the file length
            filelength = FlipEndian(stream.ReadUInt32());
            Console.WriteLine("INFO: File Length (in 16-bit words) is " + filelength);

            // get version
            version = stream.ReadUInt32();
            //Console.WriteLine("INFO: Version is " + version);

            // get shape type
            shapetype = stream.ReadUInt32();
            Console.WriteLine("INFO: ShapeType is " + shapetype);

            // get extents
            double xmin, ymin, zmin, mmin, xmax, ymax, zmax, mmax;
            xmin = stream.ReadDouble();
            ymin = stream.ReadDouble();
            xmax = stream.ReadDouble();
            ymax = stream.ReadDouble();
            zmin = stream.ReadDouble();
            zmax = stream.ReadDouble();
            mmin = stream.ReadDouble();
            mmax = stream.ReadDouble();
            min = new Point(xmin, ymin, zmin, mmin);
            max = new Point(xmax, ymax, zmax, mmax);
            
            Console.WriteLine("INFO: Extents: (" + min.X + "," + min.Y + "," + min.Z + 
                   "," + min.M + ") (" + max.X + "," + max.Y + "," + max.Z + "," + max.M + ")");
        }

		private void ReadShapeRecords(BinaryReader stream)
		{
		   	uint loc = 50;  // current position in file
		   
			while (loc < filelength)
			{
				uint recordNum = FlipEndian(stream.ReadUInt32());
				uint recordLen = FlipEndian(stream.ReadUInt32());
				uint recordShp = stream.ReadUInt32();
				
				//Console.WriteLine("INFO: Record # " + recordNum + " has length " + recordLen +
				//	  				" and type " + recordShp);  

				// Chop off 32 bit from our remaining record because we read the shape type
				uint dataleft = recordLen - 2;
				switch (recordShp)
				{
				   	case 0:
					   	// Null Object, nothing to read in
					   	Console.WriteLine("INFO: Null Shape Found");
						break;
					case 1:
						// Read in Point object
						Point p = new Point(stream.ReadDouble(), stream.ReadDouble());
						p.Id = recordNum;
						features.Add(p);
						break;
					case 3:
						// Read in PolyLine object
					   	PolyLine po = getPolyLine(stream, dataleft);
						po.Id = recordNum;
						features.Add(po);						
						break;
					case 5:
						// Read in Polygon object
					   	Polygon pol = getPolygon(stream, dataleft);
						pol.Id = recordNum;
						features.Add(pol);						
						break;
					default:
						// Anything unsupported gets dumped
						//Console.WriteLine("INFO: Unsupported Shape Type");
						byte[] data = new byte[dataleft * 2];
						stream.Read(data, 0, data.Length);
						break;
				}
				
				// log distance we have traversed in file: record length + header size
				loc += recordLen + 4;
			}
		}
		
		private Polygon getPolygon(BinaryReader stream, uint dlen)
		{
			double xmin = stream.ReadDouble();
			double ymin = stream.ReadDouble();
			double xmax = stream.ReadDouble();
			double ymax = stream.ReadDouble();

			uint numParts = stream.ReadUInt32();
			uint numPoints = stream.ReadUInt32();

			uint[] parts = new uint[numParts];
			int ii;
			for (ii=0; ii < numParts; ii++)
				parts[ii] = stream.ReadUInt32();

			Ring[] rings = new Ring[numParts];
			for (ii=0; ii < rings.Length; ii++)
			   	rings[ii] = new Ring();
			
			Polygon po = new Polygon(xmin, ymin, xmax, ymax);

			ii=0;
			for (int jj=0; jj < numPoints; jj++)
			{
				if (ii < parts.Length-1)
				   	if (parts[ii+1] == jj)
					{
						po.AddRing(rings[ii]);
					   	ii++;
					}
				Point p = new Point(stream.ReadDouble(), stream.ReadDouble());
				rings[ii].AddPoint(p);				
			}
			po.AddRing(rings[ii]);
			return po;
		}
		
		private PolyLine getPolyLine(BinaryReader stream, uint dlen)
		{
			double xmin = stream.ReadDouble();
			double ymin = stream.ReadDouble();
			double xmax = stream.ReadDouble();
			double ymax = stream.ReadDouble();

			uint numParts = stream.ReadUInt32();
			uint numPoints = stream.ReadUInt32();

			uint[] parts = new uint[numParts];
			int ii;
			for (ii=0; ii < numParts; ii++)
				parts[ii] = stream.ReadUInt32();

			Line[] lines = new Line[numParts];
			for (ii=0; ii < lines.Length; ii++)
			   	lines[ii] = new Line();
			
			PolyLine po = new PolyLine(xmin, ymin, xmax, ymax);

			ii=0;
			for (int jj=0; jj < numPoints; jj++)
			{
				if (ii < parts.Length-1)
				   	if (parts[ii+1] == jj)
					{
						po.AddLine(lines[ii]);
					   	ii++;
					}
				Point p = new Point(stream.ReadDouble(), stream.ReadDouble());
				lines[ii].AddPoint(p);				
			}
			po.AddLine(lines[ii]);
			return po;

		}
		
	/*	public static void Main(string[] args)
		{
		   string f;
		   if (args.Length == 0)
			  f = "data/district_bnd.shp";
		   else 
			  f = args[0];

		   Shapefile sf = new Shapefile(f);
		   Console.WriteLine("*** End of File Load***");

		   //Point p = (Point) ((Polygon) sf.features[0]).min;
		   //Console.WriteLine(sf.features[0].GetType());

		   //Feature fea = (Feature) sf.features[0];
		   //Console.WriteLine(sf.features.Count + " records " + (sf.Query(new Point(100000,3300000))).Count);
			ArrayList al = sf.Query(new Point(100000,3300000));
			if (al.Count > 0) Console.WriteLine(((Feature) al[0]).ID);
		}*/
    }
}

