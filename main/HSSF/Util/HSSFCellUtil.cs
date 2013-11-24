/* ====================================================================
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

namespace NPOI.HSSF.Util
{
    using System;
    using System.Collections;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;

    /// <summary>
    /// Various utility functions that make working with a cells and rows easier.  The various
    /// methods that deal with style's allow you to Create your HSSFCellStyles as you need them.
    /// When you apply a style change to a cell, the code will attempt to see if a style already
    /// exists that meets your needs.  If not, then it will Create a new style.  This is to prevent
    /// creating too many styles.  there is an upper limit in Excel on the number of styles that
    /// can be supported.
    /// @author     Eric Pugh epugh@upstate.com
    /// </summary>
    public class HSSFCellUtil
    {
        public const string ALIGNMENT = "alignment";
        public const string BORDER_BOTTOM = "borderBottom";
        public const string BORDER_LEFT = "borderLeft";
        public const string BORDER_RIGHT = "borderRight";
        public const string BORDER_TOP = "borderTop";
        public const string BOTTOM_BORDER_COLOR = "bottomBorderColor";
        public const string DATA_FORMAT = "dataFormat";
        public const string FILL_BACKGROUND_COLOR = "fillBackgroundColor";
        public const string FILL_FOREGROUND_COLOR = "fillForegroundColor";
        public const string FILL_PATTERN = "fillPattern";
        public const string FONT = "font";
        public const string HIDDEN = "hidden";
        public const string INDENTION = "indention";
        public const string LEFT_BORDER_COLOR = "leftBorderColor";
        public const string LOCKED = "locked";
        public const string RIGHT_BORDER_COLOR = "rightBorderColor";
        public const string ROTATION = "rotation";
        public const string TOP_BORDER_COLOR = "topBorderColor";
        public const string VERTICAL_ALIGNMENT = "verticalAlignment";
        public const string WRAP_TEXT = "wrapText";

        private static UnicodeMapping[] unicodeMappings;

        static HSSFCellUtil()
        {
            unicodeMappings = new UnicodeMapping[15];
            unicodeMappings[0] = um("alpha", "\u03B1");
            unicodeMappings[1] = um("beta", "\u03B2");
            unicodeMappings[2] = um("gamma", "\u03B3");
            unicodeMappings[3] = um("delta", "\u03B4");
            unicodeMappings[4] = um("epsilon", "\u03B5");
            unicodeMappings[5] = um("zeta", "\u03B6");
            unicodeMappings[6] = um("eta", "\u03B7");
            unicodeMappings[7] = um("theta", "\u03B8");
            unicodeMappings[8] = um("iota", "\u03B9");
            unicodeMappings[9] = um("kappa", "\u03BA");
            unicodeMappings[10] = um("lambda", "\u03BB");
            unicodeMappings[11] = um("mu", "\u03BC");
            unicodeMappings[12] = um("nu", "\u03BD");
            unicodeMappings[13] = um("xi", "\u03BE");
            unicodeMappings[14] = um("omicron", "\u03BF");
        }

        private class UnicodeMapping
        {

            public String entityName;
            public String resolvedValue;

            public UnicodeMapping(String pEntityName, String pResolvedValue)
            {
                entityName = "&" + pEntityName + ";";
                resolvedValue = pResolvedValue;
            }
        }

        private HSSFCellUtil()
        {
            // no instances of this class
        }

        /// <summary>
        /// Get a row from the spreadsheet, and Create it if it doesn't exist.
        /// </summary>
        /// <param name="rowCounter">The 0 based row number</param>
        /// <param name="sheet">The sheet that the row is part of.</param>
        /// <returns>The row indicated by the rowCounter</returns>
        public static NPOI.SS.UserModel.IRow GetRow(int rowCounter, HSSFSheet sheet)
        {
            NPOI.SS.UserModel.IRow row = sheet.GetRow(rowCounter);
            if (row == null)
            {
                row = sheet.CreateRow(rowCounter);
            }

            return row;
        }


        /// <summary>
        /// Get a specific cell from a row. If the cell doesn't exist,
        /// </summary>
        /// <param name="row">The row that the cell is part of</param>
        /// <param name="column">The column index that the cell is in.</param>
        /// <returns>The cell indicated by the column.</returns>
        public static NPOI.SS.UserModel.ICell GetCell(NPOI.SS.UserModel.IRow row, int column)
        {
            NPOI.SS.UserModel.ICell cell = row.GetCell(column);

            if (cell == null)
            {
                cell = row.CreateCell(column);
            }
            return cell;
        }


        /// <summary>
        /// Creates a cell, gives it a value, and applies a style if provided
        /// </summary>
        /// <param name="row">the row to Create the cell in</param>
        /// <param name="column">the column index to Create the cell in</param>
        /// <param name="value">The value of the cell</param>
        /// <param name="style">If the style is not null, then Set</param>
        /// <returns>A new HSSFCell</returns>
        public static NPOI.SS.UserModel.ICell CreateCell(NPOI.SS.UserModel.IRow row, int column, String value, HSSFCellStyle style)
        {
            NPOI.SS.UserModel.ICell cell = GetCell(row, column);

            cell.SetCellValue(new HSSFRichTextString(value));
            if (style != null)
            {
                cell.CellStyle = (style);
            }

            return cell;
        }


        /// <summary>
        /// Create a cell, and give it a value.
        /// </summary>
        /// <param name="row">the row to Create the cell in</param>
        /// <param name="column">the column index to Create the cell in</param>
        /// <param name="value">The value of the cell</param>
        /// <returns>A new HSSFCell.</returns>
        public static NPOI.SS.UserModel.ICell CreateCell(NPOI.SS.UserModel.IRow row, int column, String value)
        {
            return CreateCell(row, column, value, null);
        }


        /// <summary>
        /// Take a cell, and align it.
        /// </summary>
        /// <param name="cell">the cell to Set the alignment for</param>
        /// <param name="workbook">The workbook that is being worked with.</param>
        /// <param name="align">the column alignment to use.</param>
        public static void SetAlignment(ICell cell, HSSFWorkbook workbook, short align)
        {
            SetCellStyleProperty(cell, workbook, ALIGNMENT, align);
        }

        /// <summary>
        /// Take a cell, and apply a font to it
        /// </summary>
        /// <param name="cell">the cell to Set the alignment for</param>
        /// <param name="workbook">The workbook that is being worked with.</param>
        /// <param name="font">The HSSFFont that you want to Set...</param>
        public static void SetFont(ICell cell, HSSFWorkbook workbook, HSSFFont font)
        {
            SetCellStyleProperty(cell, workbook, FONT, font);
        }

        /**
         *  This method attempt to find an already existing HSSFCellStyle that matches
         *  what you want the style to be. If it does not find the style, then it
         *  Creates a new one. If it does Create a new one, then it applies the
         *  propertyName and propertyValue to the style. This is necessary because
         *  Excel has an upper limit on the number of Styles that it supports.
         *
         *@param  workbook               The workbook that is being worked with.
         *@param  propertyName           The name of the property that is to be
         *      changed.
         *@param  propertyValue          The value of the property that is to be
         *      changed.
         *@param  cell                   The cell that needs it's style changes
         *@exception  NestableException  Thrown if an error happens.
         */
        public static void SetCellStyleProperty(NPOI.SS.UserModel.ICell cell, HSSFWorkbook workbook, String propertyName, Object propertyValue)
        {
            NPOI.SS.UserModel.ICellStyle originalStyle = cell.CellStyle;
            NPOI.SS.UserModel.ICellStyle newStyle = null;
            Hashtable values = GetFormatProperties(originalStyle);
            values[propertyName] = propertyValue;

            // index seems like what  index the cellstyle is in the list of styles for a workbook.
            // not good to compare on!
            short numberCellStyles = workbook.NumCellStyles;

            for (short i = 0; i < numberCellStyles; i++)
            {
                NPOI.SS.UserModel.ICellStyle wbStyle = workbook.GetCellStyleAt(i);
                Hashtable wbStyleMap = GetFormatProperties(wbStyle);

                if (wbStyleMap.Equals(values))
                {
                    newStyle = wbStyle;
                    break;
                }
            }

            if (newStyle == null)
            {
                newStyle = workbook.CreateCellStyle();
                SetFormatProperties(newStyle, workbook, values);
            }

            cell.CellStyle = (newStyle);
        }

        /// <summary>
        /// Returns a map containing the format properties of the given cell style.
        /// </summary>
        /// <param name="style">cell style</param>
        /// <returns>map of format properties (String -&gt; Object)</returns>
        private static Hashtable GetFormatProperties(NPOI.SS.UserModel.ICellStyle style)
        {
            Hashtable properties = new Hashtable();
            PutShort(properties, ALIGNMENT, (short)style.Alignment);
            PutShort(properties, BORDER_BOTTOM, (short)style.BorderBottom);
            PutShort(properties, BORDER_LEFT, (short)style.BorderLeft);
            PutShort(properties, BORDER_RIGHT, (short)style.BorderRight);
            PutShort(properties, BORDER_TOP, (short)style.BorderTop);
            PutShort(properties, BOTTOM_BORDER_COLOR, style.BottomBorderColor);
            PutShort(properties, DATA_FORMAT, style.DataFormat);
            PutShort(properties, FILL_BACKGROUND_COLOR, style.FillBackgroundColor);
            PutShort(properties, FILL_FOREGROUND_COLOR, style.FillForegroundColor);
            PutShort(properties, FILL_PATTERN, (short)style.FillPattern);
            PutShort(properties, FONT, style.FontIndex);
            PutBoolean(properties, HIDDEN, style.IsHidden);
            PutShort(properties, INDENTION, style.Indention);
            PutShort(properties, LEFT_BORDER_COLOR, style.LeftBorderColor);
            PutBoolean(properties, LOCKED, style.IsLocked);
            PutShort(properties, RIGHT_BORDER_COLOR, style.RightBorderColor);
            PutShort(properties, ROTATION, style.Rotation);
            PutShort(properties, TOP_BORDER_COLOR, style.TopBorderColor);
            PutShort(properties, VERTICAL_ALIGNMENT, (short)style.VerticalAlignment);
            PutBoolean(properties, WRAP_TEXT, style.WrapText);
            return properties;
        }

        /// <summary>
        /// Sets the format properties of the given style based on the given map.
        /// </summary>
        /// <param name="style">The cell style</param>
        /// <param name="workbook">The parent workbook.</param>
        /// <param name="properties">The map of format properties (String -&gt; Object).</param>
        private static void SetFormatProperties(
                NPOI.SS.UserModel.ICellStyle style, HSSFWorkbook workbook, Hashtable properties)
        {
            style.Alignment = (NPOI.SS.UserModel.HorizontalAlignment)GetShort(properties, ALIGNMENT);
            style.BorderBottom = (NPOI.SS.UserModel.BorderStyle)GetShort(properties, BORDER_BOTTOM);
            style.BorderLeft = (NPOI.SS.UserModel.BorderStyle)GetShort(properties, BORDER_LEFT);
            style.BorderRight = (NPOI.SS.UserModel.BorderStyle)GetShort(properties, BORDER_RIGHT);
            style.BorderTop = (NPOI.SS.UserModel.BorderStyle)GetShort(properties, BORDER_TOP);
            style.BottomBorderColor = (GetShort(properties, BOTTOM_BORDER_COLOR));
            style.DataFormat = (GetShort(properties, DATA_FORMAT));
            style.FillBackgroundColor = (GetShort(properties, FILL_BACKGROUND_COLOR));
            style.FillForegroundColor = (GetShort(properties, FILL_FOREGROUND_COLOR));
            style.FillPattern = (NPOI.SS.UserModel.FillPattern)GetShort(properties, FILL_PATTERN);
            style.SetFont(workbook.GetFontAt(GetShort(properties, FONT)));
            style.IsHidden = (GetBoolean(properties, HIDDEN));
            style.Indention = (GetShort(properties, INDENTION));
            style.LeftBorderColor = (GetShort(properties, LEFT_BORDER_COLOR));
            style.IsLocked = (GetBoolean(properties, LOCKED));
            style.RightBorderColor = (GetShort(properties, RIGHT_BORDER_COLOR));
            style.Rotation = (GetShort(properties, ROTATION));
            style.TopBorderColor = (GetShort(properties, TOP_BORDER_COLOR));
            style.VerticalAlignment = (NPOI.SS.UserModel.VerticalAlignment)GetShort(properties, VERTICAL_ALIGNMENT);
            style.WrapText = (GetBoolean(properties, WRAP_TEXT));
        }

        /// <summary>
        /// Utility method that returns the named short value form the given map.
        /// Returns zero if the property does not exist, or is not a {@link Short}.
        /// </summary>
        /// <param name="properties">The map of named properties (String -&gt; Object)</param>
        /// <param name="name">The property name.</param>
        /// <returns>property value, or zero</returns>
        private static short GetShort(Hashtable properties, String name)
        {
            Object value = properties[name];
            if (value is short)
            {
                return (short)value;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Utility method that returns the named boolean value form the given map.
        /// Returns false if the property does not exist, or is not a {@link Boolean}.
        /// </summary>
        /// <param name="properties">map of properties (String -&gt; Object)</param>
        /// <param name="name">The property name.</param>
        /// <returns>property value, or false</returns>
        private static bool GetBoolean(Hashtable properties, String name)
        {
            Object value = properties[name];
            if (value is Boolean)
            {
                return ((Boolean)value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Utility method that Puts the named short value to the given map.
        /// </summary>
        /// <param name="properties">The map of properties (String -&gt; Object).</param>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        private static void PutShort(Hashtable properties, String name, short value)
        {
            properties[name] = value;
        }

        /// <summary>
        /// Utility method that Puts the named boolean value to the given map.
        /// </summary>
        /// <param name="properties">map of properties (String -&gt; Object)</param>
        /// <param name="name">property name</param>
        /// <param name="value">property value</param>
        private static void PutBoolean(Hashtable properties, String name, bool value)
        {
            properties[name] = value;
        }

        /// <summary>
        /// Looks for text in the cell that should be unicode, like alpha; and provides the
        /// unicode version of it.
        /// </summary>
        /// <param name="cell">The cell to check for unicode values</param>
        /// <returns>transalted to unicode</returns>
        public static ICell TranslateUnicodeValues(ICell cell)
        {

            String s = cell.RichStringCellValue.String;
            bool foundUnicode = false;
            String lowerCaseStr = s.ToLower();

            for (int i = 0; i < unicodeMappings.Length; i++)
            {
                UnicodeMapping entry = unicodeMappings[i];
                String key = entry.entityName;
                if (lowerCaseStr.IndexOf(key, StringComparison.Ordinal) != -1)
                {
                    s = s.Replace(key, entry.resolvedValue);
                    foundUnicode = true;
                }
            }
            if (foundUnicode)
            {
                cell.SetCellValue(new HSSFRichTextString(s));
            }
            return cell;
        }

        private static UnicodeMapping um(String entityName, String resolvedValue)
        {
            return new UnicodeMapping(entityName, resolvedValue);
        }
    }
}