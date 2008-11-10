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
	public class PostGISFeatureSource : IDatabaseFeatureSource
	{
#region vars
		
		string connectionString, tableName, geometryColumn;
		int srid;
		bool isInitialized = false;
		FeatureType featureType = FeatureType.None;
		int forcedSrid = -1;
		FeatureType forcedFeatureType = FeatureType.None;
		string forcedGeometryColumn;
		
#endregion

#region Properties
		
		public FeatureType SourceFeatureType {
			get {
				CheckIfInitialized();
				
				return featureType;
			}
		}
		
		public string TableName {
			get {
				return tableName;
			}
			set {
				tableName = value;
			}
		}

		public string ConnectionString {
			get {
				return connectionString;
			}
			set {
				connectionString = value;
			}
		}

		public int ForcedSrid {
			get {
				return forcedSrid;
			}
			set {
				forcedSrid = value;
			}
		}

		public FeatureType ForcedFeatureType {
			get {
				return forcedFeatureType;
			}
			set {
				forcedFeatureType = value;
			}
		}

		public SpatialType ForcedSpatialType {
			get {
				return SpatialType.Geometric;
			}
			set {
				throw new NotSupportedException("PostGIS only provides a geometric data type");
			}
		}

		public string ForcedGeometryColumn {
			get {
				return forcedGeometryColumn;
			}
			set {
				forcedGeometryColumn = value;
			}
		}

#endregion

#region ctors
		
		public PostGISFeatureSource()
		{
		}
		
		public PostGISFeatureSource(string connectionString, string tableName)
		{
			ConnectionString = connectionString;
			TableName = tableName;
		}

#endregion
		
#region Public methods

		public List<Feature> GetFeatures(string themeField)
		{
			return GetFeatures(new Rectangle(), themeField);
		}
		
		public List<Feature> GetFeatures (Rectangle rectangle)
		{
			return GetFeatures(rectangle, null);
		}
		
		public List<Feature> GetFeatures()
		{
			return GetFeatures(new Rectangle());
		}

		public List<Feature> GetFeatures(Rectangle rectangle, string themeField)
		{
			CheckIfInitialized();
			
			List<Feature> feats = new List<Feature>();
			
			using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
			{
				conn.Open();
				
				string sql = string.Format("select astext({0}) as {0} {2} from {1}",
				                           geometryColumn, 
				                           tableName,
				                           (themeField != null ? ", " + themeField : string.Empty));

				if (!rectangle.IsEmpty)
				{
					sql += string.Format(" where {0} && SetSRID('BOX3D({2} {3}, {4} {5})'::box3d, {6})",
					                     geometryColumn, 
				                           tableName,
				                           rectangle.Min.X,
				                           rectangle.Min.Y,
				                           rectangle.Max.X,
				                           rectangle.Max.Y,
				                           srid);
				}
				
				using (NpgsqlCommand comm = new NpgsqlCommand(sql, conn))
				{
					using (NpgsqlDataReader dr = comm.ExecuteReader())
					{
						while (dr.Read())
						{
							Feature f = WellKnownText.Parse(dr.GetString(0));
							if (themeField != null)
							{
								f.ThemeFieldValue = dr[1].ToString();
							}
							feats.Add(f);
						}
					}
				}

			}
			
			return feats;
		}
		
#endregion

#region private methods
		
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

				isInitialized = true;
				return;
			}

			using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
			{
				string sql = string.Format("select f_geometry_column, srid, type from geometry_columns where f_table_name = '{0}'",
				                           tableName);
				
				conn.Open();
				
				using (NpgsqlCommand comm = new NpgsqlCommand(sql, conn))
				{
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
							
							break;
							
						case "MULTIPOLYGON":
							
							featureType = FeatureType.Polygon;
							
							break;
							
						case "POINT":
							
							featureType = FeatureType.Point;
							
							break;
							
						default:
							throw new NotSupportedException(string.Format("'{0}' is not a supported geometry type", dr.GetString(2)));
							
						}
						
					}
				}
			}		
			
			isInitialized = true;
		}
		
#endregion
		
	}
}
