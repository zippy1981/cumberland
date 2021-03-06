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
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Types;
using System.Text;

using Cumberland.Data.Shapefile;
using Cumberland.Data.SimpleFeatureAccess;

using NDesk.Options;

namespace Cumberland.Data.SqlServer.Loader
{
	internal static class MainClass
	{


		// application variables
		private static int srid = 0;
		private static string idColumn = "gid";
		private static string geomColumn = "the_geom";
		private static string tableName = null;
		private static bool createIndex = false;
		private static bool useGeography = false;
		private static bool dropTable = false;
		private static bool append = false;
		private static bool verbose = false;
		private static Encoding encoding = null;
		private static List<string> columnList;

		public static void Main(string[] args)
		{
			bool showHelp = false;
			bool showVersion = false;

			#region handle parameters and configuration

			// set up parameters
			OptionSet options = new OptionSet
			{
				{
					"s|srid=", "The Spatial Reference ID (SRID).  If not specified it defaults to -1.",
					delegate(string v) { srid = int.Parse(v); }
				},
				{"g|geometry_column=", "The name of the geometry column", delegate(string v) { geomColumn = v; }},
				{"t|table_name=", "The table name to use", delegate(string v) { tableName = v; }},
				{
					"k|key_column=", "The name of the identity column to create for a primary key",
					delegate(string v) { idColumn = v; }
				},
				{"i|index", "Create a spatial index", delegate(string v) { createIndex = v != null; }},
				{"l|latlong", "Add spatial data as geography type", delegate(string v) { useGeography = v != null; }},
				{
					"a|append", "Append data.  If not specified, table will be created",
					delegate(string v) { append = v != null; }
				},
				{
					"dropTable", "Drop table if it exists. Mutually exclusive to -a|--append.",
					delegate(string d) { dropTable = d != null; }
				},
				{"h|help", "show this message and exit", delegate(string v) { showHelp = v != null; }},
				{"v|version", "shows the version and exits", delegate(string v) { showVersion = v != null; }},
				{
					"e|encoding=", "Specifies the encoding to use for reading the DBF file (e.g. \"utf-32\")",
					delegate(string v) { encoding = Encoding.GetEncoding(v); }
				},
				{"verbose", "Verbose output", delegate(string v) { verbose = v != null; }}
			};

			// parse the command line args
			List<string> rest = options.Parse(args);

			if (showVersion)
			{
				Console.WriteLine("Version " +
										 System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
				return;
			}

			if (showHelp)
			{
				ShowHelp(options);
				return;
			}

			if (rest.Count == 0)
			{
				Console.WriteLine("Error: A connection string is required");
				ShowHelp(options);
				return;
			}

			if (rest.Count == 1)
			{
				Console.WriteLine("Error: No path to shapefile provided");
				ShowHelp(options);
				return;
			}

			var connectionString = rest[0];
			string path = rest[1];

			if (string.IsNullOrEmpty(tableName))
			{
				// use the shapefile name as table name if none provided
				tableName = Path.GetFileNameWithoutExtension(path);
				if (verbose) Console.WriteLine("Table name not specified. Defaulting to {0}", tableName);
			}

			#endregion

			#region load shp and dbf

			var tmr = new Stopwatch();
			if (verbose) Console.Write("Opening {0} . . .", path);
			tmr.Start();
			Shapefile.Shapefile shp = new Shapefile.Shapefile(path);
			if (verbose) Console.WriteLine("Done ({0})", tmr.Elapsed);

			var pathDbf = Path.Combine(Path.GetDirectoryName(path),
				Path.GetFileNameWithoutExtension(path)) +
						  ".dbf";
			if (verbose) Console.Write("Opening {0} . . .", pathDbf);
			tmr.Reset();
			tmr.Start();
			DBaseIIIFile dbf = new DBaseIIIFile(pathDbf, encoding);
			if (verbose) Console.WriteLine("Done ({0})", tmr.Elapsed);
			tmr.Stop();
			#endregion

			#region drop/create table

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				try
				{
					if (verbose) Console.Write("Connection to {0} . . .", connection.Database);
					tmr.Start();
					connection.Open();
					if (verbose) Console.WriteLine("Complete ({0})", tmr.Elapsed);
					tmr.Stop();
				}
				catch (SqlException ex)
				{
					Console.Error.WriteLine("Error connecting to {0}. Exception details:", connectionString);
					Console.Error.WriteLine(ex.Message);
					Environment.Exit(1);
				}
				StringBuilder sql = new StringBuilder();

				if (!append)
				{
					if (dropTable)
					{
						if (verbose) Console.Write("Dropping table {0} . . . ", tableName);
						sql.AppendFormat(
@"IF OBJECT_ID('{0}', 'U') IS NOT NULL
BEGIN
    DROP TABLE [{0}]; 
END;"
							, tableName);
						using (var cmd = connection.CreateCommand())
						{
							cmd.CommandText = sql.ToString();
							cmd.ExecuteNonQuery();
						}

					}

					if (verbose) Console.Write("Creating table {0} . . . ", tableName);
					sql = new StringBuilder();
					sql.AppendFormat("create table {0} (", tableName);
					sql.AppendFormat("{0} int identity(1,1) not null", idColumn);

					foreach (DataColumn dc in dbf.Records.Columns)
					{
						sql.Append(",");

						string dataType = "nvarchar(256)";

						if (dc.DataType == typeof(DateTime))
						{
							dataType = "date";
						}
						else if (dc.DataType == typeof(double))
						{
							dataType = "real";
						}
						else if (dc.DataType == typeof(bool))
						{
							dataType = "bit";

						}

						sql.AppendFormat("{0} {1}", dc.ColumnName, dataType);
					}

					sql.AppendFormat(",{0} {1},", geomColumn, useGeography ? "geography" : "geometry");
					sql.Append(")");

					using (var command = new SqlCommand(sql.ToString(), connection))
					{
						try
						{
							command.ExecuteNonQuery();
						}
						catch (SqlException ex)
						{
							Console.Error.WriteLine(ex.Message);
							Environment.Exit(2);
						}
					}
					if (verbose) Console.WriteLine("Done");
				}
			#endregion

				using (var command = connection.CreateCommand())
				{
					columnList = (from DataColumn column in dbf.Records.Columns select column.ColumnName).ToList();
					sql = new StringBuilder();
					sql.AppendFormat("insert into {0} ({1}, {2}) values (", tableName, string.Join(", ", columnList), geomColumn);
					foreach (DataColumn dc in dbf.Records.Columns)
					{
						sql.AppendFormat("@{0}, ", dc.ColumnName);
						command.Parameters.Add(string.Format("@{0}", dc.ColumnName), dc.DataType);
					}

					sql.AppendFormat("@{0});", geomColumn);
					command.CommandText = sql.ToString();
					var geoParam = new SqlParameter(string.Format("@{0}", geomColumn), SqlDbType.Udt);
					geoParam.UdtTypeName = useGeography ? "geography" : "geometry";
					command.Parameters.Add(geoParam);

					#region insert rows

					int idx = 0;

					foreach (Feature f in shp.GetFeatures())
					{
						if (verbose && idx % 1000 == 0)
						{
							Console.WriteLine("Inserting shape {0}", idx);
						}
						var row = dbf.Records.Rows[idx];
						InsertShape(verbose, row, command, shp, f, useGeography, geomColumn, srid);
						idx++;
					}
				}

					#endregion

				#region create primary key

				using (var command = connection.CreateCommand())
				{
					command.CommandText =
						string.Format("ALTER TABLE [{0}] ADD CONSTRAINT PK_{0} PRIMARY KEY CLUSTERED ({1} ASC) ON [PRIMARY]",
							tableName, idColumn);
					command.ExecuteNonQuery();
				}

				#endregion create primary key

				#region create spatial index

				if (createIndex)
				{
					if (verbose) Console.WriteLine("Creating Spatial index");
					sql = new StringBuilder();
					sql.AppendFormat("CREATE SPATIAL INDEX {0}_sidx", tableName);
					sql.AppendFormat(" ON {0}({1})", tableName, geomColumn);
					sql.AppendFormat(" USING {0}_GRID WITH ( ", useGeography ? "GEOGRAPHY" : "GEOMETRY");

					if (!useGeography)
					{
						sql.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
										 "BOUNDING_BOX = (xmin={0}, ymin={1}, xmax={2}, ymax={3}),",
										 shp.Extents.Min.X,
										 shp.Extents.Min.Y,
										 shp.Extents.Max.X,
										 shp.Extents.Max.Y);
					}

					sql.Append(" GRIDS = (LEVEL_1 = LOW, LEVEL_2 = LOW, LEVEL_3 = HIGH, LEVEL_4 = HIGH), CELLS_PER_OBJECT = 16)");
					using (var command = new SqlCommand(sql.ToString(), connection))
					{
						command.CommandTimeout = 0; // no timeout 
						command.ExecuteNonQuery();
					}
				}
				else if (verbose) Console.WriteLine("Skipping spatial index creation");

				#endregion
			}
		}

		private static void InsertShape(bool verbose, DataRow row, SqlCommand command, Shapefile.Shapefile shp, Feature f,
			bool useGeography, string geomColumn, int srid)
		{
			foreach (var dc in columnList)
			{
				object field = row[dc];
				command.Parameters["@" + dc].Value = field;
			}

			string wkt;
			if (shp.SourceFeatureType == FeatureType.Polygon)
			{
				var polygon = (Polygon)f;

				// Sql Server 2008 ring order is reverse of shapefiles
				// http://blogs.msdn.com/edkatibah/archive/2008/08/19/working-with-invalid-data-and-the-sql-server-2008-geography-data-type-part-1b.aspx
				foreach (Ring r in polygon.Rings)
				{
					r.Points.Reverse();
				}

				wkt = WellKnownText.CreateFromPolygon(polygon, PolygonHoleStrategy.InteriorToLeft);
			}
			else if (shp.SourceFeatureType == FeatureType.Point)
			{
				wkt = WellKnownText.CreateFromPoint(f as Point);
			}
			else
			{
				wkt = WellKnownText.CreateFromPolyLine(f as PolyLine);
			}

			if (useGeography)
			{
				command.Parameters["@" + geomColumn].Value = SqlGeography.STGeomFromText(new SqlChars(wkt), srid);
			}
			else
			{
				command.Parameters["@" + geomColumn].Value = SqlGeometry.STGeomFromText(new SqlChars(wkt), srid);
			}

			try
			{
				command.ExecuteNonQuery();
			}
			catch (SqlException ex)
			{
				Console.Error.WriteLine(ex.Message);
				Environment.Exit(2);
			}
		}

		static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: shp2sqlserver.exe [OPTIONS]+ \"connectionString\" \"path to shapefile\" ");
			Console.WriteLine("Loads a shapefile into Microsoft SQL Server 2008");
			Console.WriteLine();
			Console.WriteLine("example: shp2sqlserver.exe \"Data Source=.\\SQLExpress2008;Initial Catalog=spatialtest;Integrated Security=true\" myshape.shp");
			Console.WriteLine();
			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}
	}
}