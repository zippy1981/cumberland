// SqlServerFeatureSource.cs
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
using System.Data.SqlClient;

using Cumberland;
using Cumberland.Data;
using Cumberland.Data.SimpleFeatureAccess;


namespace Cumberland.Data.SqlServer
{
	public class SqlServerFeatureSource : AbstractDatabaseFeatureSource
	{
#region vars
		
		string geometryColumn = null;
		string spatialType;
//		string connectionString;
		bool isInitialized = false;		
//		string tableName;
		FeatureType featureType = FeatureType.None;
		int srid = 0;
//		int forcedSrid = -1;
//		FeatureType forcedFeatureType = FeatureType.None;
//		SpatialType forcedSpatialType = SpatialType.Geometric;
//		string forcedGeometryColumn;

#endregion
		
#region properties
		
		public override FeatureType SourceFeatureType {
			get {
                CheckIfInitialized();

				return featureType;
			}
		}

#endregion
		
#region ctors
		
		public SqlServerFeatureSource()
		{
		}
		
		public SqlServerFeatureSource(string connectionString, string tableName)
		{
			ConnectionString = connectionString;
			TableName = tableName;
		}

		
#endregion
		
#region public methods

		public override List<Feature>GetFeatures(string themeField)
		{
			return GetFeatures(new Rectangle(), themeField);
		}

		public override List<Feature>GetFeatures (Rectangle rectangle)
		{
			return GetFeatures(rectangle, null);
		}
		
		public override List<Feature>GetFeatures()
		{
			return GetFeatures(new Rectangle());
		}

		public override List<Feature>GetFeatures (string themeField, string labelField)
		{
			return GetFeatures(new Rectangle(), themeField, labelField);
		}
		
		public override List<Feature>GetFeatures(Rectangle rectangle, string themeField)
		{
			return GetFeatures(rectangle, themeField, null);
		}	
		
		public override List<Feature>GetFeatures (Rectangle rectangle, string themeField, string labelField)
		{
			CheckIfInitialized();
			
			List<Feature> feats = new List<Feature>();
			
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.Open();
				
				string sql = string.Format("select {0}.STAsText() {2} {3} from {1}",
				                           geometryColumn, 
				                           TableName,
				                           (!string.IsNullOrEmpty(themeField) ? ", " + themeField : string.Empty),
				                           (!string.IsNullOrEmpty(labelField) ? ", " + labelField : string.Empty));

				if (!rectangle.IsEmpty)
				{
					sql += string.Format(" where {0}::STGeomFromText('{1}',{2}).STIntersects({3}) = 1",
					                     spatialType,
					                     WellKnownText.CreateFromRectangle(rectangle),
					                     srid,
					                     geometryColumn);
				}
				
				using (SqlCommand comm = new SqlCommand(sql, conn))
				{
					using (SqlDataReader dr = comm.ExecuteReader())
					{
						while (dr.Read())
						{
							string wkt = dr.GetString(0);
							Feature f = WellKnownText.Parse(wkt);

							if (f == null)
							{
								// unsupported geometry type
								continue;
							}
							
							if (!string.IsNullOrEmpty(themeField))
							{
								f.ThemeFieldValue = dr[1].ToString();
							}

							if (!string.IsNullOrEmpty(labelField))
							{
								f.LabelFieldValue = dr[themeField != null ? 2 : 1].ToString();
							}
							
							feats.Add(f);
						}
					}
				}

			}
			
			return feats;
		}
		
#endregion

#region helper methods
		
		void CheckIfInitialized()
		{
			if (!isInitialized)
			{
				InitializeFeatureSource();
			}
		}
		
		void InitializeFeatureSource()
		{
			if (string.IsNullOrEmpty(ConnectionString) || string.IsNullOrEmpty(TableName))
			{
				throw new InvalidOperationException("ConnectionString and TableName must be set to initialize");
			}

			if (ForcedSrid >= 0 && 
			    ForcedFeatureType != FeatureType.None &&
			    !string.IsNullOrEmpty(ForcedGeometryColumn))
			{
				// parameters have been provided
				
				featureType = ForcedFeatureType;
				srid = ForcedSrid;
				geometryColumn = ForcedGeometryColumn;
				
				if (ForcedSpatialType != SpatialType.None)
				{
					spatialType = ForcedSpatialType == SpatialType.Geographic ? "geography" : "geometry";
				}
				
				isInitialized = true;
				return;
			}
			
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.Open();
				
				string sql = "SELECT COLUMN_NAME, DATA_TYPE " + 
					"FROM INFORMATION_SCHEMA.COLUMNS " +
						string.Format("WHERE TABLE_NAME = '{0}'", TableName) + 
						" AND DATA_TYPE = 'geometry' OR DATA_TYPE = 'geography'";
				
				using (SqlCommand comm = new SqlCommand(sql, conn))
				{
					using (SqlDataReader dr = comm.ExecuteReader())
					{
						if (!dr.HasRows)
						{
							throw new InvalidOperationException(string.Format("Table '{0}' does not have a spatial column", TableName));
						}
								
						dr.Read();
						
						geometryColumn = dr.GetString(0);
						spatialType = dr.GetString(1);
					}
				}				

				// sniff out the feature type
				sql = string.Format("SELECT TOP 1 {0}.STGeometryType(),{0}.STSrid from {1}", geometryColumn, TableName);
				using (SqlCommand comm = new SqlCommand(sql, conn))
				{
					using (SqlDataReader dr = comm.ExecuteReader())
					{
						if (dr.HasRows)
						{
							dr.Read();
							string geometryType = dr.GetString(0);
							srid = dr.GetInt32(1);
							
							switch (geometryType.ToUpper())
							{
							case "MULTIPOLYGON":
								featureType = FeatureType.Polygon;
								break;
							case "MULTILINESTRING":
								featureType = FeatureType.Polyline;
								break;
							case "POLYGON":
								featureType = FeatureType.Polygon;
								break;
							case "LINESTRING":
								featureType = FeatureType.Polyline;
								break;
							case "POINT":
								featureType = FeatureType.Point;
								break;
							default:
								throw new NotSupportedException(string.Format("Unsupported geometry type: {0}",
								                                                  geometryType));
							}
						}
					}
				}
				
				isInitialized = true;
			}
		}

#endregion
	}
}
