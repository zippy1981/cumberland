// PostGIS.cs
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

using Cumberland;
using Cumberland.Data;

using Npgsql;

namespace Cumberland.Data.PostGIS
{
	internal enum GeometryType
	{
		None,
		MultilineString,
		MultiPolygon
	}
	
	public class PostGIS : IFeatureSource
	{
		string connectionString, tableName, geometryColumn;
		int srid;
		
		delegate Feature ParseWKT(string wkt);
		
		public FeatureType SourceFeatureType {
			get {
				return featureType;
			}
		}
		FeatureType featureType = FeatureType.None;

		GeometryType geometryType = GeometryType.None;
		
		ParseWKT parseWKTHandler;
		
		public PostGIS(string connection, string table)
		{
			connectionString = connection;
			tableName = table;
			
			using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
			{
				string sql = string.Format("select f_geometry_column, srid, type from geometry_columns where f_table_name = '{0}'",
				                           tableName);
				
				//System.Console.WriteLine(sql);
				
				using (NpgsqlCommand comm = new NpgsqlCommand(sql, conn))
				{
					conn.Open();
					
					using (NpgsqlDataReader dr = comm.ExecuteReader())
					{
						if (!dr.HasRows)
						{
							throw new ArgumentException(string.Format("row in geometry_columns for table '{0}' not found", tableName), "table");
						}
						
						dr.Read();
						geometryColumn = dr.GetString(0);
						srid = dr.GetInt32(1);
						
						switch (dr.GetString(2).ToUpper())
						{
							
						case "MULTILINESTRING":
							
							featureType = FeatureType.Polyline;
							geometryType = GeometryType.MultilineString;
							parseWKTHandler = WellKnownText.ParseMultiLineString;
							
							break;
							
						case "MULTIPOLYGON":
							
							featureType = FeatureType.Polygon;
							geometryType = GeometryType.MultiPolygon;
							//parseWKTHandler = WellKnownText.ParseMultiPolygon;
							
							break;
							
						default:
							throw new NotSupportedException(string.Format("'{0}' is not a supported geometry type", dr.GetString(2)));
							
						}
						
					}
				}

			}
		}

		public List<Feature> GetFeatures (Rectangle rectangle)
		{
			List<Feature> feats = new List<Feature>();
			
			using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
			{
				conn.Open();
				
				string sql = string.Format("select astext({0}) from {1} where {0} && SetSRID('BOX3D({2} {3}, {4} {5})'::box3d, {6})",
				                           geometryColumn, 
				                           tableName,
				                           rectangle.Min.X,
				                           rectangle.Min.Y,
				                           rectangle.Max.X,
				                           rectangle.Max.Y,
				                           srid);

				//System.Console.WriteLine(sql);
				
				using (NpgsqlCommand comm = new NpgsqlCommand(sql, conn))
				{
					using (NpgsqlDataReader dr = comm.ExecuteReader())
					{
						while (dr.Read())
						{
							if (geometryType == GeometryType.MultiPolygon)
							{
								feats.AddRange(WellKnownText.ParseMultiPolygon(dr.GetString(0)));
							}
							else
								feats.Add(parseWKTHandler(dr.GetString(0)));
						}
					}
				}

			}
			
			return feats;
		}
	}
}
