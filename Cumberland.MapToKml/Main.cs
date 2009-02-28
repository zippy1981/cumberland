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
using System.IO;
using System.Reflection;
using System.Text;

using NDesk.Options;

using Cumberland;
using Cumberland.Data;
using Cumberland.Data.PostGIS;
using Cumberland.Projection;
using Cumberland.Xml.Serialization;

namespace Cumberland.MapToKml
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			bool showHelp = false;
			bool showVersion = false;
			
			// search in the local directory for espg files 
			// so Windows ppl don't have to have it installed
            ProjFourWrapper.CustomSearchPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			MapSerializer ms = new MapSerializer();
			ms.AddDatabaseFeatureSourceType<PostGISFeatureSource>();
			ms.AddDatabaseFeatureSourceType<Cumberland.Data.SqlServer.SqlServerFeatureSource>();

			OptionSet options = new OptionSet();

			options.Add("h|help",  "show this message and exit",
			            delegate (string v) { showHelp = v!= null; });
			options.Add("v|version",
			            "Displays the version",
			            delegate(string v) { showVersion = v != null; });

			options.Parse(args);

			if (showVersion)
			{
				System.Console.WriteLine("Version " + 
				                         System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
				return;
			}

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}
			
			if (args.Length == 0)
			{
				Console.WriteLine("No Map file specified");
				ShowHelp(options);
				return;
			}

            if (args.Length == 1)
            {
                Console.WriteLine("No output file specified");
				ShowHelp(options);
                return;
            }
			
			Map map = ms.Deserialize(args[0]);

            File.WriteAllText(args[1],
                KeyholeMarkupLanguage.CreateFromMap(map),
                Encoding.UTF8);
		}

		static void ShowHelp (OptionSet p)
	    {
	        Console.WriteLine ("Usage: [mono] map2kml.exe [OPTIONS]+ \"path to map file\" \"path to output kml file\" ");
	        Console.WriteLine ("Exports a Cumberland map xml file to a kml document");
	        Console.WriteLine ();
			Console.WriteLine ("example: [mono] map2kml.exe /path/to/map.xml /path/to/ouput.kml");
			Console.WriteLine ();
	        Console.WriteLine ("Options:");
	        p.WriteOptionDescriptions (Console.Out);
	    }
	}
}