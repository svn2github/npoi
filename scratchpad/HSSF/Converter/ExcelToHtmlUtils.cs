﻿/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */
namespace NPOI.HSSF.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.HSSF.Util;
    using NPOI.HSSF.Record.CF;
    using NPOI.Util;
    using NPOI.SS.Util;

    public class ExcelToHtmlUtils
    {
        private static short EXCEL_COLUMN_WIDTH_FACTOR = 256;
        private static int UNIT_OFFSET_LENGTH = 7;
        public static void AppendAlign(StringBuilder style, HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.CENTER:
                    style.Append("text-align: center; ");
                    break;
                case HorizontalAlignment.CENTER_SELECTION:
                    style.Append("text-align: center; ");
                    break;
                case HorizontalAlignment.FILL:
                    // XXX: shall we support fill?
                    break;
                case HorizontalAlignment.GENERAL:
                    break;
                case HorizontalAlignment.JUSTIFY:
                    style.Append("text-align: justify; ");
                    break;
                case HorizontalAlignment.LEFT:
                    style.Append("text-align: left; ");
                    break;
                case HorizontalAlignment.RIGHT:
                    style.Append("text-align: right; ");
                    break;
            }
        }
        /**
     * Creates a map (i.e. two-dimensional array) filled with ranges. Allow fast
     * retrieving {@link CellRangeAddress} of any cell, if cell is contained in
     * range.
     * 
     * @see #getMergedRange(CellRangeAddress[][], int, int)
     */
        public static CellRangeAddress[][] BuildMergedRangesMap(HSSFSheet sheet)
    {
        CellRangeAddress[][] mergedRanges = new CellRangeAddress[1][];
        for ( int m = 0; m < sheet.NumMergedRegions; m++ )
        {
            CellRangeAddress cellRangeAddress = sheet.GetMergedRegion( m );

            int requiredHeight = cellRangeAddress.LastRow + 1;
            if ( mergedRanges.Length < requiredHeight )
            {
                CellRangeAddress[][] newArray = new CellRangeAddress[requiredHeight][];
                Array.Copy( mergedRanges, 0, newArray, 0, mergedRanges.Length );
                mergedRanges = newArray;
            }

            for ( int r = cellRangeAddress.FirstRow; r <= cellRangeAddress.LastRow; r++ )
            {
                int requiredWidth = cellRangeAddress.LastColumn + 1;

                CellRangeAddress[] rowMerged = mergedRanges[r];
                if ( rowMerged == null )
                {
                    rowMerged = new CellRangeAddress[requiredWidth];
                    mergedRanges[r] = rowMerged;
                }
                else
                {
                     int rowMergedLength = rowMerged.Length;
                    if ( rowMergedLength < requiredWidth )
                    {
                        CellRangeAddress[] newRow = new CellRangeAddress[requiredWidth];
                        Array.Copy(rowMerged, 0, newRow, 0,rowMergedLength );

                        mergedRanges[r] = newRow;
                        rowMerged = newRow;
                    }
                }
               
                //Arrays.Fill( rowMerged, cellRangeAddress.FirstColumn, cellRangeAddress.LastColumn + 1, cellRangeAddress );
                for (int i = cellRangeAddress.FirstColumn; i < cellRangeAddress.LastColumn + 1; i++)
                {
                    rowMerged[i] = cellRangeAddress;
                }
            }
        }
        return mergedRanges;
    }
        public static string GetBorderStyle(BorderStyle xlsBorder)
        {
            string borderStyle;
            switch (xlsBorder)
            {
                case BorderStyle.NONE:
                    borderStyle = "none";
                    break;
                case BorderStyle.DASH_DOT:
                case BorderStyle.DASH_DOT_DOT:
                case BorderStyle.DOTTED:
                case BorderStyle.HAIR:
                case BorderStyle.MEDIUM_DASH_DOT:
                case BorderStyle.MEDIUM_DASH_DOT_DOT:
                case BorderStyle.SLANTED_DASH_DOT:
                    borderStyle = "dotted";
                    break;
                case BorderStyle.DASHED:
                case BorderStyle.MEDIUM_DASHED:
                    borderStyle = "dashed";
                    break;
                case BorderStyle.DOUBLE:
                    borderStyle = "double";
                    break;
                default:
                    borderStyle = "solid";
                    break;
            }
            return borderStyle;
        }

        public static string GetBorderWidth(BorderStyle xlsBorder)
        {
            string borderWidth;
            switch (xlsBorder)
            {
                case BorderStyle.MEDIUM_DASH_DOT:
                case BorderStyle.MEDIUM_DASH_DOT_DOT:
                case BorderStyle.MEDIUM_DASHED:
                    borderWidth = "2pt";
                    break;
                case BorderStyle.THICK:
                    borderWidth = "thick";
                    break;
                default:
                    borderWidth = "thin";
                    break;
            }
            return borderWidth;
        }

        public static string GetColor(HSSFColor color)
        {
            StringBuilder stringBuilder = new StringBuilder(7);
            stringBuilder.Append('#');
            foreach (short s in color.GetTriplet())
            {
                //if (s < 10)
                //    stringBuilder.Append('0');

                stringBuilder.Append(s.ToString("x2"));
            }
            string result = stringBuilder.ToString();

            if (result.Equals("#ffffff"))
                return "white";

            if (result.Equals("#c0c0c0"))
                return "silver";

            if (result.Equals("#808080"))
                return "gray";

            if (result.Equals("#000000"))
                return "black";

            return result;
        }
        /**
     * See <a href=
     * "http://apache-poi.1045710.n5.nabble.com/Excel-Column-Width-Unit-Converter-pixels-excel-column-width-units-td2301481.html"
     * >here</a> for Xio explanation and details
     */
        public static int GetColumnWidthInPx(int widthUnits)
        {
            int pixels = (widthUnits / EXCEL_COLUMN_WIDTH_FACTOR)
                    * UNIT_OFFSET_LENGTH;

            int offsetWidthUnits = widthUnits % EXCEL_COLUMN_WIDTH_FACTOR;
            pixels += (int)Math.Round(offsetWidthUnits / ((float)EXCEL_COLUMN_WIDTH_FACTOR / UNIT_OFFSET_LENGTH));

            return pixels;
        }
        /**
     * @param mergedRanges
     *            map of sheet merged ranges built with
     *            {@link #buildMergedRangesMap(HSSFSheet)}
     * @return {@link CellRangeAddress} from map if cell with specified row and
     *         column numbers contained in found range, <tt>null</tt> otherwise
     */
        public static CellRangeAddress GetMergedRange(
                CellRangeAddress[][] mergedRanges, int rowNumber, int columnNumber)
        {
            CellRangeAddress[] mergedRangeRowInfo = rowNumber < mergedRanges.Length ? mergedRanges[rowNumber]
                    : null;
            CellRangeAddress cellRangeAddress = mergedRangeRowInfo != null
                    && columnNumber < mergedRangeRowInfo.Length ? mergedRangeRowInfo[columnNumber]
                    : null;

            return cellRangeAddress;
        }
        public static HSSFWorkbook LoadXls(string xlsFile)
        {
            FileStream inputStream = File.Open(xlsFile, FileMode.Open);
            //FileInputStream inputStream = new FileInputStream( xlsFile );
            try
            {
                return new HSSFWorkbook(inputStream);
            }
            finally
            {
                if (inputStream != null)
                    inputStream.Close();
                inputStream = null;
                //IOUtils.closeQuietly( inputStream );
            }
        }
    }

}
