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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

using Cumberland;
using Cumberland.Data.Shapefile;

namespace Cumberland.Data.SqlServer.Loader
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			string connectionString = args[0];
			
			string path = args[1];
			
			Shapefile.Shapefile shp = new Shapefile.Shapefile(path);
			DBaseIIIFile dbf = new DBaseIIIFile(Path.GetDirectoryName(path) + 
			                                    Path.DirectorySeparatorChar + 
			                                    Path.GetFileNameWithoutExtension(path) + 
			                                    ".dbf");
			
			int srid = 4326;
			string idColumn = "gid";
			string geomColumn = "the_geom";
			string tableName = Path.GetFileNameWithoutExtension(path);
			StringBuilder sql = new StringBuilder();
			
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
				
			sql.AppendFormat(",{0} geometry,", geomColumn);
			
			sql.AppendFormat("CONSTRAINT PK_{0} PRIMARY KEY CLUSTERED ({1} ASC) ON [PRIMARY]", tableName, idColumn);
			
			sql.Append(")");

			
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				
				SqlCommand command = new SqlCommand(sql.ToString(), connection);
				command.ExecuteNonQuery();
				
				int idx = 0;
				foreach (Feature f in shp.GetFeatures())
		        {
					sql = new StringBuilder();
					sql.AppendFormat("insert into {0} values (", tableName);
	
					for (int ii=0; ii < dbf.Records.Columns.Count; ii++)
					{
						DataColumn dc = dbf.Records.Columns[ii];
						object row = dbf.Records.Rows[idx][ii];
						
						if (dc.DataType == typeof(DateTime))
					    {
							sql.AppendFormat("CAST('{0}' AS date)", ((DateTime)row).ToShortDateString());
						}
						else if (dc.DataType == typeof(double))
						{
							sql.Append(row.ToString());
						}
					    else if (dc.DataType == typeof(bool))
						{
							sql.AppendFormat("{0}", ((bool)row) ? 1 : 0);
						}
						else
						{
							sql.AppendFormat("'{0}'", row.ToString().Replace("'", "''"));
						}
						
						sql.Append(", ");
					}
					
					string wkt = string.Empty;
					if (shp.SourceFeatureType == FeatureType.Polygon)
					{
						wkt = WellKnownText.CreateFromPolygon(f as Polygon);
					}
					else if (shp.SourceFeatureType == FeatureType.Point)
					{
						wkt = WellKnownText.CreateFromPoint(f as Point);
					}
					else
					{
						wkt = WellKnownText.CreateFromPolyLine(f as PolyLine);
					}
					
					sql.AppendFormat("geometry::STGeomFromText('{0}', {1})", wkt, srid);
	
					sql.Append(")");
					
					idx++;
					
					command = new SqlCommand(sql.ToString(), connection);
					command.ExecuteNonQuery();
				}
				
				System.Console.WriteLine("Creating spatial index...");
				
				// create spatial index
				sql = new StringBuilder();
				sql.AppendFormat("CREATE SPATIAL INDEX {0}_sidx", tableName);
				sql.AppendFormat(" ON {0}({1})", tableName, geomColumn);
				sql.Append(" USING GEOMETRY_GRID WITH ( ");
				sql.AppendFormat("BOUNDING_BOX = (xmin={0}, ymin={1}, xmax={2}, ymax={3}),", 
				                 shp.Extents.Min.X,
				                 shp.Extents.Min.Y,
				                 shp.Extents.Max.X,
				                 shp.Extents.Max.Y);
				sql.Append(" GRIDS = (LEVEL_1 = LOW, LEVEL_2 = LOW, LEVEL_3 = HIGH, LEVEL_4 = HIGH), CELLS_PER_OBJECT = 16)");
				
				command = new SqlCommand(sql.ToString(), connection);
				command.ExecuteNonQuery();
			}
		}
	}
}