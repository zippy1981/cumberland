// DBaseIIIFile.cs
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
using System.IO;
using System.Text;

using System.Data;

namespace Cumberland.Data.Shapefile
{
	public class DBaseIIIFile
	{
		DateTime lastUpdated;
		
		public DateTime LastUpdated {
			get {
				return lastUpdated;
			}
		}

		public DataTable Records {
			get {
				return records;
			}
		}
		
		int recordCount;
	    int colCount;
		
		DataTable records;

        Encoding encoding = Encoding.Default;

		public DBaseIIIFile(string file) : this(file, null)
		{
        }

        public DBaseIIIFile(string file, Encoding encoding)
        {
            if (encoding != null)
            {
                this.encoding = encoding;
            }

			records = new DataTable(Path.GetFileNameWithoutExtension(file));
			
			using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				BinaryReader br = new BinaryReader(fs);

				List<FieldDescriptor> cols = ReadTableFileHeader(br);
			    colCount = cols.Count;
				
				ReadTableRecords(br, cols);

//				if (br.ReadByte() != 26)
//				{
//					throw new FormatException("DBF (dBase) file not properly terminated");
//				}
			}
		}	
		
		List<FieldDescriptor> ReadTableFileHeader(BinaryReader br)
		{
			byte type = br.ReadByte();
			
			if (type != 3)
			{
				throw new FormatException("This does not appear to be a .DBF (dBase) file");
			}
			
			byte year = br.ReadByte();
			byte month = br.ReadByte();
			byte day = br.ReadByte();
			
			lastUpdated = new DateTime(year + 1900, month, day);
			recordCount = br.ReadInt32();
			
			br.ReadInt16(); // short numberBytesHeader = 
			br.ReadInt16(); // short numberBytesRecord = 
			
			br.ReadBytes(3); // Reserved
			br.ReadBytes(13); // Reserved for dBASE III PLUS on a LAN. 
			br.ReadBytes(4); // Reserved

			List<FieldDescriptor> fields = new List<FieldDescriptor>();
			
			while (br.PeekChar() != 13)
			{
				FieldDescriptor fd = new FieldDescriptor();
				fd.Name = encoding.GetString(br.ReadBytes(11));
				fd.Type = Convert.ToChar(br.ReadByte());
				
				DataColumn dc = new DataColumn(fd.Name.TrimEnd('\0'));

				bool skip = false;
				
				switch (fd.Type)
				{
					case 'C':
						dc.DataType = typeof(string);
						break;
					case 'D':
						dc.DataType = typeof(DateTime);
						break;
					case 'N':
						dc.DataType = typeof(double);
						break;
					case 'L':
						dc.DataType = typeof(bool);
						break;
					case 'M':
						dc.DataType = typeof(string);
						break;
					case 'F':
						dc.DataType = typeof(float);
						break;
				}

				br.ReadBytes(4); // Field data address
				fd.Length = br.ReadByte();
				br.ReadByte(); // byte fieldDecimalCount = 
				
				br.ReadBytes(2); // Reserved for dBASE III PLUS on a LAN. 
				br.ReadByte(); // Work area ID
				br.ReadBytes(2); // Reserved for dBASE III PLUS on a LAN. 
				br.ReadByte();  // SET FIELDS flag. 
				br.ReadBytes(8); // Reserved bytes

				if (skip) continue;
				
				if (fd.Type != 'M')
				{
					records.Columns.Add(dc);
				}
				
				fields.Add(fd);
			}
			
			br.ReadByte();  // read the field terminator
			
			return fields;
		}

		void ReadTableRecords(BinaryReader br, List<FieldDescriptor> cols)
		{
			// iterate rows
			for (int ii=0; ii<recordCount; ii++)
			{
				br.ReadByte(); // deleted byte
				
				DataRow row = records.NewRow();

				// iterate columns
                for (int jj=0; jj<colCount; jj++)
				{
					FieldDescriptor fd = cols[jj];

					switch (fd.Type)
					{
						
					case 'C':

						string cValue = encoding.GetString(br.ReadBytes(fd.Length)).Trim();

                        if (string.IsNullOrEmpty(cValue))
                        {
                            row[jj] = DBNull.Value;
                        }
                        else
                        {
                            row[jj] = cValue;
                        }

						break;
						
					case 'D':
						
                        string dtValue = encoding.GetString(br.ReadBytes(8));
                        DateTime date;

                        if (!DateTime.TryParseExact(dtValue, 
                            "yyyyMMdd",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out date))
                        {
                            row[jj] = DBNull.Value;
                        }
                        else
                        {
                            row[jj] = date;
						}

						break;
						
					case 'N':
					case 'F':

						string numValue = encoding.GetString(br.ReadBytes(fd.Length));
                        double num;

                        if (!double.TryParse(numValue, out num))
                        {
                            row[jj] = DBNull.Value;
                        }
                        else
                        {
                            row[jj] = num;
                        }

						break;
						
					case 'L':

						char logical = Convert.ToChar(br.ReadByte());
						if (logical == 'y' || logical == 'Y' || logical == 't' | logical == 'T')
						{
							row[jj] = true;
						}
						else if (logical == 'n' || logical == 'N' || logical == 'f' || logical == 'F')
						{
							row[jj] = false;
						}
						else
						{
							row[jj] = DBNull.Value;
						}
						
						break;
						
					case 'M':
						
						br.ReadBytes(10); // ignore 
						break;
						
					}
				}
				
				records.Rows.Add(row);
			}
		}

		internal sealed class FieldDescriptor
		{
			public string Name;
			public char Type;
			public int Length;
		}
		
	}
}
