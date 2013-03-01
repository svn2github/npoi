/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) Under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You Under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed Under the License is distributed on an "AS Is" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations Under the License.
==================================================================== */

namespace NPOI.HSSF.UserModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NPOI.DDF;
    using NPOI.HSSF.Model;
    using NPOI.HSSF.Record;
    using NPOI.HSSF.Record.Aggregates;
    using NPOI.HSSF.Record.AutoFilter;
    using NPOI.HSSF.Util;
    using NPOI.SS;
    using NPOI.SS.Formula;
    using NPOI.SS.Formula.PTG;
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;
    using System.Globalization;



    /// <summary>
    /// High level representation of a worksheet.
    /// </summary>
    /// <remarks>
    /// @author  Andrew C. Oliver (acoliver at apache dot org)
    /// @author  Glen Stampoultzis (glens at apache.org)
    /// @author  Libin Roman (romal at vistaportal.com)
    /// @author  Shawn Laubach (slaubach at apache dot org) (Just a little)
    /// @author  Jean-Pierre Paris (jean-pierre.paris at m4x dot org) (Just a little, too)
    /// @author  Yegor Kozlov (yegor at apache.org) (Autosizing columns)
    /// </remarks>
    [Serializable]
    public class HSSFSheet : NPOI.SS.UserModel.ISheet
    {

        /**
         * Used for compile-time optimization.  This is the initial size for the collection of
         * rows.  It is currently Set to 20.  If you generate larger sheets you may benefit
         * by Setting this to a higher number and recompiling a custom edition of HSSFSheet.
         */

        public const int INITIAL_CAPACITY = 20;

        /**
         * reference to the low level Sheet object
         */

        private InternalSheet _sheet;
        private Dictionary<int, NPOI.SS.UserModel.IRow> rows;
        public InternalWorkbook book;
        protected HSSFWorkbook _workbook;
        private int firstrow;
        private int lastrow;
        //private static POILogger log = POILogFactory.GetLogger(typeof(HSSFSheet));

        /// <summary>
        /// Creates new HSSFSheet - called by HSSFWorkbook to create a _sheet from
        /// scratch. You should not be calling this from application code (its protected anyhow).
        /// </summary>
        /// <param name="workbook">The HSSF Workbook object associated with the _sheet.</param>
        /// <see cref="NPOI.HSSF.UserModel.HSSFWorkbook.CreateSheet()"/>
        public HSSFSheet(HSSFWorkbook workbook)
        {
            _sheet = InternalSheet.CreateSheet();
            rows = new Dictionary<int, NPOI.SS.UserModel.IRow>();
            this._workbook = workbook;
            this.book = workbook.Workbook;
        }

        /// <summary>
        /// Creates an HSSFSheet representing the given Sheet object.  Should only be
        /// called by HSSFWorkbook when reading in an exisiting file.
        /// </summary>
        /// <param name="workbook">The HSSF Workbook object associated with the _sheet.</param>
        /// <param name="sheet">lowlevel Sheet object this _sheet will represent</param>
        /// <see cref="NPOI.HSSF.UserModel.HSSFWorkbook(NPOI.POIFS.FileSystem.DirectoryNode, bool)"/>
        public HSSFSheet(HSSFWorkbook workbook, InternalSheet sheet)
        {
            this._sheet = sheet;
            rows = new Dictionary<int, NPOI.SS.UserModel.IRow>();
            this._workbook = workbook;
            this.book = _workbook.Workbook;
            SetPropertiesFromSheet(_sheet);
        }
        /// <summary>
        /// Clones the _sheet.
        /// </summary>
        /// <param name="workbook">The _workbook.</param>
        /// <returns>the cloned sheet</returns>
        public ISheet CloneSheet(HSSFWorkbook workbook)
        {
            IDrawing iDrawing = this.DrawingPatriarch;/**Aggregate drawing records**/
            HSSFSheet sheet = new HSSFSheet(workbook, _sheet.CloneSheet());
            int pos = sheet._sheet.FindFirstRecordLocBySid(DrawingRecord.sid);
            DrawingRecord dr = (DrawingRecord)sheet._sheet.FindFirstRecordBySid(DrawingRecord.sid);
            if (null != dr)
            {
                sheet._sheet.Records.Remove(dr);
            }
            if (DrawingPatriarch != null)
            {
                HSSFPatriarch patr = HSSFPatriarch.CreatePatriarch(this.DrawingPatriarch as HSSFPatriarch, sheet);
                sheet._sheet.Records.Insert(pos, patr.GetBoundAggregate());
                sheet._patriarch = patr;
            }
            return sheet;
        }


        internal void PreSerialize()
        {
            if (_patriarch != null)
            {
                _patriarch.PreSerialize();
            }
        }
        /// <summary>
        /// Copy one row to the target row
        /// </summary>
        /// <param name="sourceIndex">index of the source row</param>
        /// <param name="targetIndex">index of the target row</param>
        public IRow CopyRow(int sourceIndex, int targetIndex)
        {
            return SheetUtil.CopyRow(this, sourceIndex, targetIndex);
        }
        /// <summary>
        /// used internally to Set the properties given a Sheet object
        /// </summary>
        /// <param name="sheet">The _sheet.</param>
        private void SetPropertiesFromSheet(InternalSheet sheet)
        {

            RowRecord row = sheet.NextRow;
            bool rowRecordsAlreadyPresent = row != null;

            while (row != null)
            {
                CreateRowFromRecord(row);

                row = sheet.NextRow;
            }

            CellValueRecordInterface[] cvals = sheet.GetValueRecords();
            long timestart = DateTime.Now.Millisecond;

            //if (log.Check(POILogger.DEBUG))
            //    log.Log(DEBUG, "Time at start of cell creating in HSSF _sheet = ",
            //        timestart);
            HSSFRow lastrow = null;

            // Add every cell to its row
            for (int i = 0; i < cvals.Length; i++)
            {
                CellValueRecordInterface cval = cvals[i];
                long cellstart = DateTime.Now.Millisecond;
                HSSFRow hrow = lastrow;

                if ((lastrow == null) || (lastrow.RowNum != cval.Row))
                {
                    hrow = (HSSFRow)GetRow(cval.Row);
                    if (hrow == null)
                    {
                        // Some tools (like Perl module SpReadsheet::WriteExcel - bug 41187) skip the RowRecords 
                        // Excel, OpenOffice.org and GoogleDocs are all OK with this, so POI should be too.
                        if (rowRecordsAlreadyPresent)
                        {
                            // if at least one row record is present, all should be present.
                            throw new Exception("Unexpected missing row when some rows already present, the file is wrong");
                        }
                        // Create the row record on the fly now.
                        RowRecord rowRec = new RowRecord(cval.Row);
                        _sheet.AddRow(rowRec);
                        hrow = CreateRowFromRecord(rowRec);
                    }
                }
                if (hrow != null)
                {
                    lastrow = hrow;
                    //if (log.Check(POILogger.DEBUG))
                    //{
                    //    if (cval is Record)
                    //    {
                    //        log.log(DEBUG, "record id = " + Integer.toHexString(((Record)cval).getSid()));
                    //    }
                    //    else
                    //    {
                    //        log.log(DEBUG, "record = " + cval);
                    //    }
                    //}

                    hrow.CreateCellFromRecord(cval);

                    //if (log.Check(POILogger.DEBUG))
                    //    log.Log(DEBUG, "record took ",DateTime.Now.Millisecond - cellstart);
                }
                else
                {
                    cval = null;
                }
            }
            //if (log.Check(POILogger.DEBUG))
            //    log.Log(DEBUG, "total _sheet cell creation took ",
            //        DateTime.Now.Millisecond - timestart);
        }
        /**
         * Gets the flag indicating whether the window should show 0 (zero) in cells containing zero value.
         * When false, cells with zero value appear blank instead of showing the number zero.
         * In Excel 2003 this option can be changed in the Options dialog on the View tab.
         * @return whether all zero values on the worksheet are displayed
         */
        public bool DisplayZeros
        {
            get
            {
                return _sheet.WindowTwo.DisplayZeros;
            }
            set
            {
                _sheet.WindowTwo.DisplayZeros = value;
            }
        }

        /// <summary>
        /// Create a new row within the _sheet and return the high level representation
        /// </summary>
        /// <param name="rownum">The row number.</param>
        /// <returns></returns>
        /// @see org.apache.poi.hssf.usermodel.HSSFRow
        /// @see #RemoveRow(HSSFRow)
        public NPOI.SS.UserModel.IRow CreateRow(int rownum)
        {
            HSSFRow row = new HSSFRow(_workbook, this, rownum);
            // new rows inherit default height from the sheet
            row.Height = (DefaultRowHeight);
            row.RowRecord.BadFontHeight = (false);
            AddRow(row, true);
            return row;
        }

        /// <summary>
        /// Used internally to Create a high level Row object from a low level row object.
        /// USed when Reading an existing file
        /// </summary>
        /// <param name="row">low level record to represent as a high level Row and Add to _sheet.</param>
        /// <returns>HSSFRow high level representation</returns>
        private HSSFRow CreateRowFromRecord(RowRecord row)
        {
            HSSFRow hrow = new HSSFRow(_workbook, this, row);

            AddRow(hrow, false);
            return hrow;
        }

        /// <summary>
        /// Remove a row from this _sheet.  All cells contained in the row are Removed as well
        /// </summary>
        /// <param name="row">the row to Remove.</param>
        public void RemoveRow(IRow row)
        {
            if (row.Sheet != this)
            {
                throw new ArgumentException("Specified row does not belong to this sheet");
            }
            foreach (ICell cell in row)
            {
                HSSFCell xcell = (HSSFCell)cell;
                if (xcell.IsPartOfArrayFormulaGroup)
                {
                    String msg = "Row[rownum=" + row.RowNum + "] contains cell(s) included in a multi-cell array formula. You cannot change part of an array.";
                    xcell.NotifyArrayFormulaChanging(msg);
                }
            }
            if (rows.Count > 0)
            {
                int key = row.RowNum;
                HSSFRow removedRow = (HSSFRow)rows[key];
                rows.Remove(key);

                if (removedRow != row)
                {
                    if (removedRow != null)
                    {
                        rows[key] = removedRow;
                    }
                    throw new InvalidOperationException("Specified row does not belong to this _sheet");
                }

                if (row.RowNum == LastRowNum)
                {
                    lastrow = FindLastRow(lastrow);
                }
                if (row.RowNum == FirstRowNum)
                {
                    firstrow = FindFirstRow(firstrow);
                }
                CellValueRecordInterface[] cellvaluerecods = _sheet.GetValueRecords();
                for (int i = 0; i < cellvaluerecods.Length; i++)
                {
                    if (cellvaluerecods[i].Row == key)
                    {
                        _sheet.RemoveValueRecord(key, cellvaluerecods[i]);
                    }
                }

                _sheet.RemoveRow(((HSSFRow)row).RowRecord);
            }
        }

        /// <summary>
        /// used internally to refresh the "last row" when the last row is Removed.
        /// </summary>
        /// <param name="lastrow">The last row.</param>
        /// <returns></returns>
        private int FindLastRow(int lastrow)
        {
            if (lastrow < 1)
            {
                return 0;
            }

            int rownum = lastrow - 1;
            NPOI.SS.UserModel.IRow r = GetRow(rownum);

            while (r == null && rownum > 0)
            {
                r = GetRow(--rownum);
            }
            if (r == null)
                return 0;
            return rownum;
        }

        /// <summary>
        /// used internally to refresh the "first row" when the first row is Removed.
        /// </summary>
        /// <param name="firstrow">The first row.</param>
        /// <returns></returns>
        private int FindFirstRow(int firstrow)
        {
            int rownum = firstrow + 1;
            NPOI.SS.UserModel.IRow r = GetRow(rownum);

            while (r == null && rownum <= LastRowNum)
            {
                r = GetRow(++rownum);
            }

            if (rownum > LastRowNum)
                return 0;

            return rownum;
        }

        /**
         * Add a row to the _sheet
         *
         * @param AddLow whether to Add the row to the low level model - false if its already there
         */

        private void AddRow(HSSFRow row, bool addLow)
        {
            rows[row.RowNum] = row;

            if (addLow)
            {
                _sheet.AddRow(row.RowRecord);
            }
            bool firstRow = rows.Count == 1;
            if (row.RowNum > LastRowNum || firstRow)
            {
                lastrow = row.RowNum;
            }
            if (row.RowNum < FirstRowNum || firstRow)
            {
                firstrow = row.RowNum;
            }
        }

        /// <summary>
        /// Returns the HSSFCellStyle that applies to the given
        /// (0 based) column, or null if no style has been
        /// set for that column
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public NPOI.SS.UserModel.ICellStyle GetColumnStyle(int column)
        {
            short styleIndex = _sheet.GetXFIndexForColAt((short)column);

            if (styleIndex == 0xf)
            {
                // None set
                return null;
            }

            ExtendedFormatRecord xf = book.GetExFormatAt(styleIndex);
            return new HSSFCellStyle(styleIndex, xf, book);
        }

        /// <summary>
        /// Returns the logical row (not physical) 0-based.  If you ask for a row that is not
        /// defined you get a null.  This is to say row 4 represents the fifth row on a _sheet.
        /// </summary>
        /// <param name="rowIndex">Index of the row to get.</param>
        /// <returns>the row number or null if its not defined on the _sheet</returns>
        public NPOI.SS.UserModel.IRow GetRow(int rowIndex)
        {
            if (!rows.ContainsKey(rowIndex))
                return null;
            return (HSSFRow)rows[rowIndex];
        }

        /// <summary>
        /// Returns the number of phsyically defined rows (NOT the number of rows in the _sheet)
        /// </summary>
        /// <value>The physical number of rows.</value>
        public int PhysicalNumberOfRows
        {
            get
            {
                return rows.Count;
            }
        }

        /// <summary>
        /// Gets the first row on the _sheet
        /// </summary>
        /// <value>the number of the first logical row on the _sheet</value>
        public int FirstRowNum
        {
            get
            {
                return firstrow;
            }
        }

        /// <summary>
        /// Gets the last row on the _sheet
        /// </summary>
        /// <value>last row contained n this _sheet.</value>
        public int LastRowNum
        {
            get
            {
                return lastrow;
            }
        }

        /// <summary>
        /// Creates a data validation object
        /// </summary>
        /// <param name="dataValidation">The data validation object settings</param>
        public void AddValidationData(IDataValidation dataValidation)
        {
            if (dataValidation == null)
            {
                throw new ArgumentException("objValidation must not be null");
            }

            HSSFDataValidation hssfDataValidation = (HSSFDataValidation)dataValidation;
            DataValidityTable dvt = _sheet.GetOrCreateDataValidityTable();

            DVRecord dvRecord = hssfDataValidation.CreateDVRecord(this);
            dvt.AddDataValidation(dvRecord);
        }

        /// <summary>
        /// Get the visibility state for a given column.F:\Gloria\�о�\�ļ���ʽ\NPOI\src\NPOI\HSSF\Util\HSSFDataValidation.cs
        /// </summary>
        /// <param name="column">the column to Get (0-based).</param>
        /// <param name="hidden">the visiblity state of the column.</param>
        public void SetColumnHidden(int column, bool hidden)
        {
            _sheet.SetColumnHidden(column, hidden);
        }

        /// <summary>
        /// Get the hidden state for a given column.
        /// </summary>
        /// <param name="column">the column to Set (0-based)</param>
        /// <returns>the visiblity state of the column;
        /// </returns>
        public bool IsColumnHidden(int column)
        {
            return _sheet.IsColumnHidden(column);
        }

        /// <summary>
        /// Set the width (in Units of 1/256th of a Char width)
        /// </summary>
        /// <param name="column">the column to Set (0-based)</param>
        /// <param name="width">the width in Units of 1/256th of a Char width</param>
        public void SetColumnWidth(int column, int width)
        {
            _sheet.SetColumnWidth(column, width);
        }

        /// <summary>
        /// Get the width (in Units of 1/256th of a Char width )
        /// </summary>
        /// <param name="column">the column to Set (0-based)</param>
        /// <returns>the width in Units of 1/256th of a Char width</returns>
        public int GetColumnWidth(int column)
        {
            return _sheet.GetColumnWidth(column);
        }

        /// <summary>
        /// Gets or sets the default width of the column.
        /// </summary>
        /// <value>The default width of the column.</value>
        public int DefaultColumnWidth
        {
            get { return _sheet.DefaultColumnWidth; }
            set { _sheet.DefaultColumnWidth = value; }
        }

        /// <summary>
        /// Get the default row height for the _sheet (if the rows do not define their own height) in
        /// twips (1/20 of  a point)
        /// </summary>
        /// <value>The default height of the row.</value>
        public short DefaultRowHeight
        {
            get { return _sheet.DefaultRowHeight; }
            set { _sheet.DefaultRowHeight = (short)value; }
        }

        /// <summary>
        /// Get the default row height for the _sheet (if the rows do not define their own height) in
        /// points.
        /// </summary>
        /// <value>The default row height in points.</value>
        public float DefaultRowHeightInPoints
        {
            get
            {
                return (float)(_sheet.DefaultRowHeight / 20.0);
            }
            set { _sheet.DefaultRowHeight = ((short)(value * 20.0)); }
        }

        /// <summary>
        /// Get whether gridlines are printed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if printed; otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Please use IsPrintGridlines instead")]
        public bool IsGridsPrinted
        {
            get { return _sheet.IsGridsPrinted; }
            set { _sheet.IsGridsPrinted = (value); }
        }


        /// <summary>
        /// Adds a merged region of cells (hence those cells form one)
        /// </summary>
        /// <param name="region">The region (rowfrom/colfrom-rowto/colto) to merge.</param>
        /// <returns>index of this region</returns>
        [Obsolete]
        public int AddMergedRegion(NPOI.SS.Util.Region region)
        {
            return _sheet.AddMergedRegion(region.RowFrom,
                    region.ColumnFrom,
                    region.RowTo,
                    region.ColumnTo);
        }
        /// <summary>
        /// adds a merged region of cells (hence those cells form one)
        /// </summary>
        /// <param name="region">region (rowfrom/colfrom-rowto/colto) to merge</param>
        /// <returns>index of this region</returns>
        public int AddMergedRegion(NPOI.SS.Util.CellRangeAddress region)
        {
            region.Validate(SpreadsheetVersion.EXCEL97);
            // throw IllegalStateException if the argument CellRangeAddress intersects with
            // a multi-cell array formula defined in this sheet
            ValidateArrayFormulas(region);

            return _sheet.AddMergedRegion(region.FirstRow,
                    region.FirstColumn,
                    region.LastRow,
                    region.LastColumn);
        }
        private void ValidateArrayFormulas(CellRangeAddress region)
        {
            int firstRow = region.FirstRow;
            int firstColumn = region.FirstColumn;
            int lastRow = region.LastRow;
            int lastColumn = region.LastColumn;
            for (int rowIn = firstRow; rowIn <= lastRow; rowIn++)
            {
                for (int colIn = firstColumn; colIn <= lastColumn; colIn++)
                {
                    HSSFRow row = (HSSFRow)GetRow(rowIn);
                    if (row == null) continue;

                    HSSFCell cell = (HSSFCell)row.GetCell(colIn);
                    if (cell == null) continue;

                    if (cell.IsPartOfArrayFormulaGroup)
                    {
                        CellRangeAddress arrayRange = cell.ArrayFormulaRange;
                        if (arrayRange.NumberOfCells > 1 &&
                                (arrayRange.IsInRange(region.FirstRow, region.FirstColumn) ||
                                  arrayRange.IsInRange(region.FirstRow, region.FirstColumn)))
                        {
                            String msg = "The range " + region.FormatAsString() + " intersects with a multi-cell array formula. " +
                                    "You cannot merge cells of an array.";
                            throw new InvalidOperationException(msg);
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Whether a record must be Inserted or not at generation to indicate that
        /// formula must be recalculated when _workbook is opened.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [force formula recalculation]; otherwise, <c>false</c>.
        /// </value>
        /// @return true if an Uncalced record must be Inserted or not at generation
        public bool ForceFormulaRecalculation
        {
            get { return _sheet.IsUncalced; }
            set { _sheet.IsUncalced = (value); }
        }

        /// <summary>
        /// Determine whether printed output for this _sheet will be vertically centered.
        /// </summary>
        /// <value><c>true</c> if [vertically center]; otherwise, <c>false</c>.</value>
        public bool VerticallyCenter
        {
            get
            {
                return _sheet.PageSettings.VCenter.VCenter;
            }
            set
            {
                _sheet.PageSettings.VCenter.VCenter = value;
            }
        }


        /// <summary>
        /// Determine whether printed output for this _sheet will be horizontally centered.
        /// </summary>
        /// <value><c>true</c> if [horizontally center]; otherwise, <c>false</c>.</value>
        public bool HorizontallyCenter
        {
            get
            {
                return _sheet.PageSettings.HCenter.HCenter;
            }
            set
            {
                _sheet.PageSettings.HCenter.HCenter = (value);
            }
        }



        /// <summary>
        /// Removes a merged region of cells (hence letting them free)
        /// </summary>
        /// <param name="index">index of the region to Unmerge</param>
        public void RemoveMergedRegion(int index)
        {
            _sheet.RemoveMergedRegion(index);
        }

        /// <summary>
        /// returns the number of merged regions
        /// </summary>
        /// <value>The number of merged regions</value>
        public int NumMergedRegions
        {
            get
            {
                return _sheet.NumMergedRegions;
            }
        }

        ///// <summary>
        ///// Gets the region at a particular index
        ///// </summary>
        ///// <param name="index">of the region to fetch</param>
        ///// <returns>the merged region (simple eh?)</returns>
        //public NPOI.SS.Util.Region GetMergedRegionAt(int index)
        //{
        //    NPOI.SS.Util.CellRangeAddress cra = GetMergedRegion(index);

        //    return new NPOI.SS.Util.Region(cra.FirstRow, (short)cra.FirstColumn,
        //            cra.LastRow, (short)cra.LastColumn);
        //}

        /// <summary>
        /// Gets the row enumerator.
        /// </summary>
        /// <returns>
        /// an iterator of the PHYSICAL rows.  Meaning the 3rd element may not
        /// be the third row if say for instance the second row is undefined.
        /// Call <see cref="NPOI.SS.UserModel.IRow.RowNum"/> on each row 
        /// if you care which one it is.
        /// </returns>
        public IEnumerator GetRowEnumerator()
        {
            return rows.Values.GetEnumerator();
        }

        /// <summary>
        /// Alias for GetRowEnumerator() to allow <c>foreach</c> loops.
        /// </summary>
        /// <returns>
        /// an iterator of the PHYSICAL rows.  Meaning the 3rd element may not
        /// be the third row if say for instance the second row is undefined.
        /// Call <see cref="NPOI.SS.UserModel.IRow.RowNum"/> on each row 
        /// if you care which one it is.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return GetRowEnumerator();
        }

        /// <summary>
        /// used internally in the API to Get the low level Sheet record represented by this
        /// Object.
        /// </summary>
        /// <value>low level representation of this HSSFSheet.</value>
        public InternalSheet Sheet
        {
            get { return _sheet; }
        }

        /// <summary>
        /// Sets the active cell.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        public void SetActiveCell(int row, int column)
        {
            this._sheet.SetActiveCellRange(row, row, column, column);
        }
        /// <summary>
        /// Sets the active cell range.
        /// </summary>
        /// <param name="firstRow">The first row.</param>
        /// <param name="lastRow">The last row.</param>
        /// <param name="firstColumn">The first column.</param>
        /// <param name="lastColumn">The last column.</param>
        public void SetActiveCellRange(int firstRow, int lastRow, int firstColumn, int lastColumn)
        {
            this._sheet.SetActiveCellRange(firstRow, lastRow, firstColumn, lastColumn);
        }
        /// <summary>
        /// Sets the active cell range.
        /// </summary>
        /// <param name="cellranges">The cellranges.</param>
        /// <param name="activeRange">The index of the active range.</param>
        /// <param name="activeRow">The active row in the active range</param>
        /// <param name="activeColumn">The active column in the active range</param>
        public void SetActiveCellRange(List<CellRangeAddress8Bit> cellranges, int activeRange, int activeRow, int activeColumn)
        {
            this._sheet.SetActiveCellRange(cellranges, activeRange, activeRow, activeColumn);
        }

        /// <summary>
        /// Gets or sets whether alternate expression evaluation is on
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [alternative expression]; otherwise, <c>false</c>.
        /// </value>
        public bool AlternativeExpression
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .AlternateExpression;
            }
            set
            {
                WSBoolRecord record = (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.AlternateExpression = value;

            }
        }

        /// <summary>
        /// whether alternative formula entry is on
        /// </summary>
        /// <value><c>true</c> alternative formulas or not; otherwise, <c>false</c>.</value>
        public bool AlternativeFormula
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .AlternateFormula;
            }
            set
            {
                WSBoolRecord record = (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.AlternateFormula = value;
            }
        }

        /// <summary>
        /// show automatic page breaks or not
        /// </summary>
        /// <value>whether to show auto page breaks</value>
        public bool Autobreaks
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .Autobreaks;
            }
            set
            {
                WSBoolRecord record =
                    (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.Autobreaks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether _sheet is a dialog _sheet
        /// </summary>
        /// <value><c>true</c> if is dialog; otherwise, <c>false</c>.</value>
        public bool Dialog
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .Dialog;
            }
            set
            {
                WSBoolRecord record =
                    (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.Dialog = (value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to Display the guts or not.
        /// </summary>
        /// <value><c>true</c> if guts or no guts (or glory); otherwise, <c>false</c>.</value>
        public bool DisplayGuts
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .DisplayGuts;
            }
            set
            {
                WSBoolRecord record =
                    (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.DisplayGuts = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether fit to page option is on
        /// </summary>
        /// <value><c>true</c> if [fit to page]; otherwise, <c>false</c>.</value>
        public bool FitToPage
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .FitToPage;
            }
            set
            {
                WSBoolRecord record =
                    (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.FitToPage = value;
            }
        }

        /// <summary>
        /// Get if row summaries appear below detail in the outline
        /// </summary>
        /// <value><c>true</c> if below or not; otherwise, <c>false</c>.</value>
        public bool RowSumsBelow
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .RowSumsBelow;
            }
            set
            {
                WSBoolRecord record =
                    (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.RowSumsBelow = value;
            }
        }

        /// <summary>
        /// Get if col summaries appear right of the detail in the outline
        /// </summary>
        /// <value><c>true</c> right or not; otherwise, <c>false</c>.</value>
        public bool RowSumsRight
        {
            get
            {
                return ((WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid))
                        .RowSumsRight;
            }
            set
            {
                WSBoolRecord record =
                    (WSBoolRecord)_sheet.FindFirstRecordBySid(WSBoolRecord.sid);

                record.RowSumsRight = (value);
            }
        }

        /// <summary>
        /// Gets or sets whether gridlines are printed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> Gridlines are printed; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrintGridlines
        {
            get { return Sheet.PrintGridlines.PrintGridlines; }
            set { Sheet.PrintGridlines.PrintGridlines = (value); }
        }

        /// <summary>
        /// Gets the print setup object.
        /// </summary>
        /// <value>The user model for the print setup object.</value>
        public NPOI.SS.UserModel.IPrintSetup PrintSetup
        {
            get { return new HSSFPrintSetup(this._sheet.PageSettings.PrintSetup); }
        }

        /// <summary>
        /// Gets the user model for the document header.
        /// </summary>
        /// <value>The Document header.</value>
        public NPOI.SS.UserModel.IHeader Header
        {
            get { return new HSSFHeader(this._sheet.PageSettings); }
        }

        /// <summary>
        /// Gets the user model for the document footer.
        /// </summary>
        /// <value>The Document footer.</value>
        public NPOI.SS.UserModel.IFooter Footer
        {
            get { return new HSSFFooter(this._sheet.PageSettings); }
        }

        /// <summary>
        /// Gets or sets whether the worksheet is displayed from right to left instead of from left to right.
        /// </summary>
        /// <value>true for right to left, false otherwise</value>
        /// <remarks>poi bug 47970</remarks>
        public bool IsRightToLeft
        {
            get
            {
                return _sheet.WindowTwo.Arabic;
            }
            set
            {
                _sheet.WindowTwo.Arabic = value;
            }
        }

        /// <summary>
        /// Note - this is not the same as whether the _sheet is focused (isActive)
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this _sheet is currently selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get { return Sheet.GetWindowTwo().IsSelected; }
            set { Sheet.GetWindowTwo().IsSelected = (value); }
        }

        /// <summary>
        /// Gets or sets a value indicating if this _sheet is currently focused.
        /// </summary>
        /// <value><c>true</c> if this _sheet is currently focused; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return Sheet.GetWindowTwo().IsActive; }
            set { Sheet.GetWindowTwo().IsActive = (value); }
        }

        /// <summary>
        /// Sets whether sheet is selected.
        /// </summary>
        /// <param name="sel">Whether to select the sheet or deselect the sheet.</param> 
        public void SetActive(bool sel)
        {
            this.Sheet.WindowTwo.IsActive = sel;
        }

        private WorksheetProtectionBlock ProtectionBlock
        {
            get
            {
                return _sheet.ProtectionBlock;
            }
        }
        /// <summary>
        /// Answer whether protection is enabled or disabled
        /// </summary>
        /// <value><c>true</c> if protection enabled; otherwise, <c>false</c>.</value>
        public bool Protect
        {
            get { return ProtectionBlock.IsSheetProtected; }
        }

        /// <summary>
        /// Gets the hashed password
        /// </summary>
        /// <value>The password.</value>
        public int Password
        {
            get
            {
                return ProtectionBlock.PasswordHash;
            }
        }

        /// <summary>
        /// Answer whether object protection is enabled or disabled
        /// </summary>
        /// <value><c>true</c> if protection enabled; otherwise, <c>false</c>.</value>
        public bool ObjectProtect
        {
            get
            {
                return ProtectionBlock.IsObjectProtected;
            }
        }

        /// <summary>
        /// Answer whether scenario protection is enabled or disabled
        /// </summary>
        /// <value><c>true</c> if protection enabled; otherwise, <c>false</c>.</value>
        public bool ScenarioProtect
        {
            get
            {
                return ProtectionBlock.IsScenarioProtected;
            }
        }
        /// <summary>
        /// Sets the protection enabled as well as the password
        /// </summary>
        /// <param name="password">password to set for protection, pass <code>null</code> to remove protection</param>
        public void ProtectSheet(String password)
        {
            ProtectionBlock.ProtectSheet(password, true, true); //protect objs&scenarios(normal)
        }
        /// <summary>
        /// Sets the zoom magnication for the _sheet.  The zoom is expressed as a
        /// fraction.  For example to express a zoom of 75% use 3 for the numerator
        /// and 4 for the denominator.
        /// </summary>
        /// <param name="numerator">The numerator for the zoom magnification.</param>
        /// <param name="denominator">The denominator for the zoom magnification.</param>
        public void SetZoom(int numerator, int denominator)
        {
            if (numerator < 1 || numerator > 65535)
                throw new ArgumentException("Numerator must be greater than 1 and less than 65536");
            if (denominator < 1 || denominator > 65535)
                throw new ArgumentException("Denominator must be greater than 1 and less than 65536");

            SCLRecord sclRecord = new SCLRecord();
            sclRecord.Numerator = ((short)numerator);
            sclRecord.Denominator = ((short)denominator);
            Sheet.SetSCLRecord(sclRecord);
        }
        /// <summary>
        /// Sets the enclosed border of region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="borderType">Type of the border.</param>
        /// <param name="color">The color.</param>
        public void SetEnclosedBorderOfRegion(CellRangeAddress region, NPOI.SS.UserModel.BorderStyle borderType, short color)
        {
            HSSFRegionUtil.SetRightBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderRight(borderType, region, this, _workbook);
            HSSFRegionUtil.SetLeftBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderLeft(borderType, region, this, _workbook);
            HSSFRegionUtil.SetTopBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderTop(borderType, region, this, _workbook);
            HSSFRegionUtil.SetBottomBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderBottom(borderType, region, this, _workbook);
        }
        /// <summary>
        /// Sets the right border of region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="borderType">Type of the border.</param>
        /// <param name="color">The color.</param>
        public void SetBorderRightOfRegion(CellRangeAddress region, NPOI.SS.UserModel.BorderStyle borderType, short color)
        {
            HSSFRegionUtil.SetRightBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderRight(borderType, region, this, _workbook);
        }

        /// <summary>
        /// Sets the left border of region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="borderType">Type of the border.</param>
        /// <param name="color">The color.</param>
        public void SetBorderLeftOfRegion(CellRangeAddress region, NPOI.SS.UserModel.BorderStyle borderType, short color)
        {
            HSSFRegionUtil.SetLeftBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderLeft(borderType, region, this, _workbook);
        }

        /// <summary>
        /// Sets the top border of region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="borderType">Type of the border.</param>
        /// <param name="color">The color.</param>
        public void SetBorderTopOfRegion(CellRangeAddress region, NPOI.SS.UserModel.BorderStyle borderType, short color)
        {
            HSSFRegionUtil.SetTopBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderTop(borderType, region, this, _workbook);
        }

        /// <summary>
        /// Sets the bottom border of region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="borderType">Type of the border.</param>
        /// <param name="color">The color.</param>
        public void SetBorderBottomOfRegion(CellRangeAddress region, NPOI.SS.UserModel.BorderStyle borderType, short color)
        {
            HSSFRegionUtil.SetBottomBorderColor(color, region, this, _workbook);
            HSSFRegionUtil.SetBorderBottom(borderType, region, this, _workbook);
        }

        /// <summary>
        /// The top row in the visible view when the _sheet is
        /// first viewed after opening it in a viewer
        /// </summary>
        /// <value>the rownum (0 based) of the top row</value>
        public short TopRow
        {
            get
            {
                return _sheet.TopRow;
            }
            set
            {
                _sheet.TopRow = value;
            }
        }

        /// <summary>
        /// The left col in the visible view when the _sheet Is
        /// first viewed after opening it in a viewer
        /// </summary>
        /// <value>the rownum (0 based) of the top row</value>
        public short LeftCol
        {
            get
            {
                return _sheet.LeftCol;
            }
            set
            {
                _sheet.LeftCol = value;
            }
        }

        /// <summary>
        /// Sets desktop window pane display area, when the
        /// file is first opened in a viewer.
        /// </summary>
        /// <param name="toprow">the top row to show in desktop window pane</param>
        /// <param name="leftcol">the left column to show in desktop window pane</param>
        public void ShowInPane(short toprow, short leftcol)
        {
            this._sheet.TopRow = (toprow);
            this._sheet.LeftCol = (leftcol);
        }

        /// <summary>
        /// Shifts the merged regions left or right depending on mode
        /// TODO: MODE , this is only row specific
        /// </summary>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <param name="n">The n.</param>
        /// <param name="IsRow">if set to <c>true</c> [is row].</param>
        protected void ShiftMerged(int startRow, int endRow, int n, bool IsRow)
        {
            List<CellRangeAddress> ShiftedRegions = new List<CellRangeAddress>();
            //move merged regions completely if they fall within the new region boundaries when they are Shifted
            for (int i = 0; i < this.NumMergedRegions; i++)
            {
                NPOI.SS.Util.CellRangeAddress merged = this.GetMergedRegion(i);

                bool inStart = (merged.FirstRow >= startRow || merged.LastRow >= startRow);
                bool inEnd = (merged.FirstRow <= endRow || merged.LastRow <= endRow);


                //dont Check if it's not within the Shifted area
                if (!(inStart && inEnd)) continue;

                //only Shift if the region outside the Shifted rows is not merged too
                //if (!ContainsCell(merged, startRow - 1, 0) && !ContainsCell(merged, endRow + 1, 0))
                if (!SheetUtil.ContainsCell(merged, startRow - 1, 0) &&
                 !SheetUtil.ContainsCell(merged, endRow + 1, 0))
                {
                    merged.FirstRow = (merged.FirstRow + n);
                    merged.LastRow = (merged.LastRow + n);
                    //have to Remove/Add it back
                    ShiftedRegions.Add(merged);
                    this.RemoveMergedRegion(i);
                    i = i - 1; // we have to back up now since we Removed one

                }

            }

            //Readd so it doesn't Get Shifted again
            IEnumerator iterator = ShiftedRegions.GetEnumerator();
            while (iterator.MoveNext())
            {
                NPOI.SS.Util.CellRangeAddress region = (NPOI.SS.Util.CellRangeAddress)iterator.Current;

                this.AddMergedRegion(region);
            }

        }
        [Obsolete]
        private static bool ContainsCell(NPOI.SS.Util.CellRangeAddress cr, int rowIx, int colIx)
        {
            if (cr.FirstRow <= rowIx && cr.LastRow >= rowIx
                    && cr.FirstColumn <= colIx && cr.LastColumn >= colIx)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Shifts rows between startRow and endRow n number of rows.
        /// If you use a negative number, it will Shift rows up.
        /// Code Ensures that rows don't wrap around.
        /// Calls ShiftRows(startRow, endRow, n, false, false);
        /// Additionally Shifts merged regions that are completely defined in these
        /// rows (ie. merged 2 cells on a row to be Shifted).
        /// </summary>
        /// <param name="startRow">the row to start Shifting</param>
        /// <param name="endRow">the row to end Shifting</param>
        /// <param name="n">the number of rows to Shift</param>
        public void ShiftRows(int startRow, int endRow, int n)
        {
            ShiftRows(startRow, endRow, n, false, false);
        }
        /// <summary>
        /// Shifts rows between startRow and endRow n number of rows.
        /// If you use a negative number, it will shift rows up.
        /// Code ensures that rows don't wrap around
        /// Additionally shifts merged regions that are completely defined in these
        /// rows (ie. merged 2 cells on a row to be shifted).
        /// TODO Might want to add bounds checking here
        /// </summary>
        /// <param name="startRow">the row to start shifting</param>
        /// <param name="endRow">the row to end shifting</param>
        /// <param name="n">the number of rows to shift</param>
        /// <param name="copyRowHeight">whether to copy the row height during the shift</param>
        /// <param name="resetOriginalRowHeight">whether to set the original row's height to the default</param>
        public void ShiftRows(int startRow, int endRow, int n, bool copyRowHeight, bool resetOriginalRowHeight)
        {
            ShiftRows(startRow, endRow, n, copyRowHeight, resetOriginalRowHeight, true);
        }
        /// <summary>
        /// Shifts rows between startRow and endRow n number of rows.
        /// If you use a negative number, it will Shift rows up.
        /// Code Ensures that rows don't wrap around
        /// Additionally Shifts merged regions that are completely defined in these
        /// rows (ie. merged 2 cells on a row to be Shifted).
        /// TODO Might want to Add bounds Checking here
        /// </summary>
        /// <param name="startRow">the row to start Shifting</param>
        /// <param name="endRow">the row to end Shifting</param>
        /// <param name="n">the number of rows to Shift</param>
        /// <param name="copyRowHeight">whether to copy the row height during the Shift</param>
        /// <param name="resetOriginalRowHeight">whether to Set the original row's height to the default</param>
        /// <param name="moveComments">if set to <c>true</c> [move comments].</param>
        public void ShiftRows(int startRow, int endRow, int n,
            bool copyRowHeight, bool resetOriginalRowHeight, bool moveComments)
        {
            int s, inc;
            if (n < 0)
            {
                s = startRow;
                inc = 1;
            }
            else if (n > 0)
            {
                s = endRow;
                inc = -1;
            }
            else
            {
                // Nothing to do
                return;
            }

            NoteRecord[] noteRecs;
            if (moveComments)
            {
                noteRecs = _sheet.GetNoteRecords();
            }
            else
            {
                noteRecs = NoteRecord.EMPTY_ARRAY;
            }

            ShiftMerged(startRow, endRow, n, true);
            _sheet.PageSettings.ShiftRowBreaks(startRow, endRow, n);

            for (int rowNum = s; rowNum >= startRow && rowNum <= endRow && rowNum >= 0 && rowNum < 65536; rowNum += inc)
            {
                HSSFRow row = (HSSFRow)GetRow(rowNum);

                // notify all cells in this row that we are going to shift them,
                // it can throw IllegalStateException if the operation is not allowed, for example,
                // if the row contains cells included in a multi-cell array formula
                if (row != null) NotifyRowShifting(row);

                HSSFRow row2Replace = (HSSFRow)GetRow(rowNum + n);
                if (row2Replace == null)
                    row2Replace = (HSSFRow)CreateRow(rowNum + n);


                // Remove all the old cells from the row we'll
                //  be writing too, before we start overwriting 
                //  any cells. This avoids issues with cells 
                //  changing type, and records not being correctly
                //  overwritten
                row2Replace.RemoveAllCells();

                // If this row doesn't exist, nothing needs to
                //  be done for the now empty destination row
                if (row == null) continue; // Nothing to do for this row


                // Fix up row heights if required
                if (copyRowHeight)
                {
                    row2Replace.Height = (row.Height);
                }
                if (resetOriginalRowHeight)
                {
                    row.Height = ((short)0xff);
                }

                // Copy each cell from the source row to
                //  the destination row
                List<ICell> cells = row.Cells;
                foreach (ICell cell in cells)
                {
                    row.RemoveCell(cell);
                    CellValueRecordInterface cellRecord = ((HSSFCell)cell).CellValueRecord;
                    cellRecord.Row = (rowNum + n);
                    row2Replace.CreateCellFromRecord(cellRecord);
                    _sheet.AddValueRecord(rowNum + n, cellRecord);

                    NPOI.SS.UserModel.IHyperlink link = cell.Hyperlink;
                    if (link != null)
                    {
                        link.FirstRow = (link.FirstRow + n);
                        link.LastRow = (link.LastRow + n);
                    }
                }
                // Now zap all the cells in the source row
                row.RemoveAllCells();

                // Move comments from the source row to the
                //  destination row. Note that comments can
                //  exist for cells which are null
                if (moveComments)
                {
                    // This code would get simpler if NoteRecords could be organised by HSSFRow.
                    HSSFPatriarch patriarch = CreateDrawingPatriarch() as HSSFPatriarch;
                    for (int i = patriarch.Children.Count - 1; i >= 0; i--)
                    {
                        HSSFShape shape = patriarch.Children[(i)];
                        if (!(shape is HSSFComment))
                        {
                            continue;
                        }
                        HSSFComment comment = (HSSFComment)shape;
                        if (comment.Row != rowNum)
                        {
                            continue;
                        }
                        comment.Row = (rowNum + n);
                    }
                }
            }
            // Re-compute the first and last rows of the sheet as needed
            if (n > 0)
            {
                // Rows are moving down
                if (startRow == firstrow)
                {
                    // Need to walk forward to find the first non-blank row
                    firstrow = Math.Max(startRow + n, 0);
                    for (int i = startRow + 1; i < startRow + n; i++)
                    {
                        if (GetRow(i) != null)
                        {
                            firstrow = i;
                            break;
                        }
                    }
                }
                if (endRow + n > lastrow)
                {
                    lastrow = Math.Min(endRow + n, SpreadsheetVersion.EXCEL97.LastRowIndex);
                }
            }
            else
            {
                // Rows are moving up
                if (startRow + n < firstrow)
                {
                    firstrow = Math.Max(startRow + n, 0);
                }
                if (endRow == lastrow)
                {
                    // Need to walk backward to find the last non-blank row
                    lastrow = Math.Min(endRow + n, SpreadsheetVersion.EXCEL97.LastRowIndex);
                    for (int i = endRow - 1; i > endRow + n; i++)
                    {
                        if (GetRow(i) != null)
                        {
                            lastrow = i;
                            break;
                        }
                    }
                }
            }
            //if (endRow == lastrow || endRow + n > lastrow) lastrow = Math.Min(endRow + n, SpreadsheetVersion.EXCEL97.LastRowIndex);
            //if (startRow == firstrow || startRow + n < firstrow) firstrow = Math.Max(startRow + n, 0);

            // Update any formulas on this _sheet that point to
            //  rows which have been moved
            int sheetIndex = _workbook.GetSheetIndex(this);
            int externSheetIndex = book.CheckExternSheet(sheetIndex);
            FormulaShifter shifter = FormulaShifter.CreateForRowShift(externSheetIndex, startRow, endRow, n);
            _sheet.UpdateFormulasAfterCellShift(shifter, externSheetIndex);

            int nSheets = _workbook.NumberOfSheets;
            for (int i = 0; i < nSheets; i++)
            {
                InternalSheet otherSheet = ((HSSFSheet)_workbook.GetSheetAt(i)).Sheet;
                if (otherSheet == this._sheet)
                {
                    continue;
                }
                int otherExtSheetIx = book.CheckExternSheet(i);
                otherSheet.UpdateFormulasAfterCellShift(shifter, otherExtSheetIx);
            }
            _workbook.Workbook.UpdateNamesAfterCellShift(shifter);
        }

        /// <summary>
        /// Inserts the chart records.
        /// </summary>
        /// <param name="records">The records.</param>
        public void InsertChartRecords(List<RecordBase> records)
        {
            int window2Loc = _sheet.FindFirstRecordLocBySid(WindowTwoRecord.sid);
            _sheet.Records.InsertRange(window2Loc, records);
        }
        private void NotifyRowShifting(HSSFRow row)
        {
            String msg = "Row[rownum=" + row.RowNum + "] contains cell(s) included in a multi-cell array formula. " +
                    "You cannot change part of an array.";
            foreach (ICell cell in row.Cells)
            {
                HSSFCell hcell = (HSSFCell)cell;
                if (hcell.IsPartOfArrayFormulaGroup)
                {
                    hcell.NotifyArrayFormulaChanging(msg);
                }
            }
        }
        /// <summary>
        /// Creates a split (freezepane). Any existing freezepane or split pane is overwritten.
        /// </summary>
        /// <param name="colSplit">Horizonatal position of split.</param>
        /// <param name="rowSplit">Vertical position of split.</param>
        /// <param name="leftmostColumn">Top row visible in bottom pane</param>
        /// <param name="topRow">Left column visible in right pane.</param>
        public void CreateFreezePane(int colSplit, int rowSplit, int leftmostColumn, int topRow)
        {
            ValidateColumn(colSplit);
            ValidateRow(rowSplit);
            if (leftmostColumn < colSplit) throw new ArgumentException("leftmostColumn parameter must not be less than colSplit parameter");
            if (topRow < rowSplit) throw new ArgumentException("topRow parameter must not be less than leftmostColumn parameter");
            Sheet.CreateFreezePane(colSplit, rowSplit, topRow, leftmostColumn);
        }

        /// <summary>
        /// Creates a split (freezepane). Any existing freezepane or split pane is overwritten.
        /// </summary>
        /// <param name="colSplit">Horizonatal position of split.</param>
        /// <param name="rowSplit">Vertical position of split.</param>
        public void CreateFreezePane(int colSplit, int rowSplit)
        {
            CreateFreezePane(colSplit, rowSplit, colSplit, rowSplit);
        }

        /// <summary>
        /// Creates a split pane. Any existing freezepane or split pane is overwritten.
        /// </summary>
        /// <param name="xSplitPos">Horizonatal position of split (in 1/20th of a point).</param>
        /// <param name="ySplitPos">Vertical position of split (in 1/20th of a point).</param>
        /// <param name="leftmostColumn">Left column visible in right pane.</param>
        /// <param name="topRow">Top row visible in bottom pane.</param>
        /// <param name="activePane">Active pane.  One of: PANE_LOWER_RIGHT,PANE_UPPER_RIGHT, PANE_LOWER_LEFT, PANE_UPPER_LEFT</param>
        public void CreateSplitPane(int xSplitPos, int ySplitPos, int leftmostColumn, int topRow, NPOI.SS.UserModel.PanePosition activePane)
        //this would have the same parameter sequence as the internal method and matches the content of the description above,
        // if this signature changed do it in ISheet, too.
        // public void CreateSplitPane(int xSplitPos, int ySplitPos, int topRow, int leftmostColumn, NPOI.SS.UserModel.PanePosition activePane)
        {
            Sheet.CreateSplitPane(xSplitPos, ySplitPos, topRow, leftmostColumn, activePane);
        }

        /// <summary>
        /// Returns the information regarding the currently configured pane (split or freeze).
        /// </summary>
        /// <value>null if no pane configured, or the pane information.</value>
        public NPOI.SS.Util.PaneInformation PaneInformation
        {
            get
            {
                return Sheet.PaneInformation;
            }
        }

        /// <summary>
        /// Gets or sets if gridlines are Displayed.
        /// </summary>
        /// <value>whether gridlines are Displayed</value>
        public bool DisplayGridlines
        {
            get { return _sheet.DisplayGridlines; }
            set { _sheet.DisplayGridlines = (value); }
        }


        /// <summary>
        /// Gets or sets a value indicating whether formulas are displayed.
        /// </summary>
        /// <value>whether formulas are Displayed</value>
        public bool DisplayFormulas
        {
            get { return _sheet.DisplayFormulas; }
            set { _sheet.DisplayFormulas = (value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether RowColHeadings are displayed.
        /// </summary>
        /// <value>
        /// 	whether RowColHeadings are displayed
        /// </value>
        public bool DisplayRowColHeadings
        {
            get { return _sheet.DisplayRowColHeadings; }
            set { _sheet.DisplayRowColHeadings = (value); }
        }

        /// <summary>
        /// Gets the size of the margin in inches.
        /// </summary>
        /// <param name="margin">which margin to get.</param>
        /// <returns>the size of the margin</returns>
        public double GetMargin(NPOI.SS.UserModel.MarginType margin)
        {
            switch (margin)
            {
                case MarginType.FooterMargin:
                    return _sheet.PageSettings.PrintSetup.FooterMargin;
                case MarginType.HeaderMargin:
                    return _sheet.PageSettings.PrintSetup.HeaderMargin;
                default:
                    return _sheet.PageSettings.GetMargin(margin);
                    break;
            }
        }

        /// <summary>
        /// Sets the size of the margin in inches.
        /// </summary>
        /// <param name="margin">which margin to get.</param>
        /// <param name="size">the size of the margin</param>
        public void SetMargin(NPOI.SS.UserModel.MarginType margin, double size)
        {
            switch (margin)
            {
                case MarginType.FooterMargin:
                    _sheet.PageSettings.PrintSetup.FooterMargin = (size);
                    break;
                case MarginType.HeaderMargin:
                    _sheet.PageSettings.PrintSetup.HeaderMargin = (size);
                    break;
                default:
                    _sheet.PageSettings.SetMargin(margin, size);
                    break;
            }
        }

        /// <summary>
        /// Sets a page break at the indicated row
        /// </summary>
        /// <param name="row">The row.</param>
        public void SetRowBreak(int row)
        {
            ValidateRow(row);
            _sheet.PageSettings.SetRowBreak(row, (short)0, (short)255);
        }

        /// <summary>
        /// Determines if there is a page break at the indicated row
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// 	<c>true</c> if [is row broken] [the specified row]; otherwise, <c>false</c>.
        /// </returns>        
        public bool IsRowBroken(int row)
        {
            return _sheet.PageSettings.IsRowBroken(row);
        }

        /// <summary>
        /// Removes the page break at the indicated row
        /// </summary>
        /// <param name="row">The row.</param>
        public void RemoveRowBreak(int row)
        {
            _sheet.PageSettings.RemoveRowBreak(row);
        }

        /// <summary>
        /// Retrieves all the horizontal page breaks
        /// </summary>
        /// <value>all the horizontal page breaks, or null if there are no row page breaks</value>
        public int[] RowBreaks
        {
            get
            {
                //we can probably cache this information, but this should be a sparsely used function
                return _sheet.PageSettings.RowBreaks;
            }
        }

        /// <summary>
        /// Retrieves all the vertical page breaks
        /// </summary>
        /// <value>all the vertical page breaks, or null if there are no column page breaks</value>
        public int[] ColumnBreaks
        {
            get
            {
                //we can probably cache this information, but this should be a sparsely used function
                return _sheet.PageSettings.ColumnBreaks;
            }
        }


        /// <summary>
        /// Sets a page break at the indicated column
        /// </summary>
        /// <param name="column">The column.</param>
        public void SetColumnBreak(int column)
        {
            ValidateColumn(column);
            _sheet.PageSettings.SetColumnBreak(column, (short)0, unchecked((short)65535));
        }

        /// <summary>
        /// Determines if there is a page break at the indicated column
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>
        /// 	<c>true</c> if [is column broken] [the specified column]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsColumnBroken(int column)
        {
            return _sheet.PageSettings.IsColumnBroken(column);
        }

        /// <summary>
        /// Removes a page break at the indicated column
        /// </summary>
        /// <param name="column">The column.</param>
        public void RemoveColumnBreak(int column)
        {
            _sheet.PageSettings.RemoveColumnBreak(column);
        }

        /// <summary>
        /// Runs a bounds Check for row numbers
        /// </summary>
        /// <param name="row">The row.</param>
        protected void ValidateRow(int row)
        {
            int maxrow = SpreadsheetVersion.EXCEL97.LastRowIndex;
            if (row > maxrow) throw new ArgumentException("Maximum row number is " + maxrow.ToString(CultureInfo.CurrentCulture));
            if (row < 0) throw new ArgumentException("Minumum row number is 0");
        }

        /// <summary>
        /// Runs a bounds Check for column numbers
        /// </summary>
        /// <param name="column">The column.</param>
        protected void ValidateColumn(int column)
        {
            int maxcol = SpreadsheetVersion.EXCEL97.LastColumnIndex;
            if (column > maxcol) throw new ArgumentException("Maximum column number is " + maxcol.ToString(CultureInfo.CurrentCulture));
            if (column < 0) throw new ArgumentException("Minimum column number is 0");
        }

        /// <summary>
        /// Aggregates the drawing records and dumps the escher record hierarchy
        /// to the standard output.
        /// </summary>
        /// <param name="fat">if set to <c>true</c> [fat].</param>
        public void DumpDrawingRecords(bool fat)
        {
            _sheet.AggregateDrawingRecords(book.DrawingManager, false);

            EscherAggregate r = (EscherAggregate)Sheet.FindFirstRecordBySid(EscherAggregate.sid);
            IList escherRecords = r.EscherRecords;
            for (IEnumerator iterator = escherRecords.GetEnumerator(); iterator.MoveNext(); )
            {
                EscherRecord escherRecord = (EscherRecord)iterator.Current;
                if (fat)
                    Console.WriteLine(escherRecord.ToString());
                else
                    escherRecord.Display(0);
            }
        }
        [NonSerialized]
        private HSSFPatriarch _patriarch;


        /// <summary>
        /// Returns the agregate escher records for this _sheet,
        /// it there is one.
        /// WARNING - calling this will trigger a parsing of the
        /// associated escher records. Any that aren't supported
        /// (such as charts and complex drawing types) will almost
        /// certainly be lost or corrupted when written out.
        /// </summary>
        /// <value>The drawing escher aggregate.</value>
        public EscherAggregate DrawingEscherAggregate
        {
            get
            {
                book.FindDrawingGroup();

                // If there's now no drawing manager, then there's
                //  no drawing escher records on the _workbook
                if (book.DrawingManager == null)
                {
                    return null;
                }

                int found = _sheet.AggregateDrawingRecords(
                        book.DrawingManager, false
                );
                if (found == -1)
                {
                    // Workbook has drawing stuff, but this _sheet doesn't
                    return null;
                }

                // Grab our aggregate record, and wire it up
                EscherAggregate agg = (EscherAggregate)_sheet.FindFirstRecordBySid(EscherAggregate.sid);
                return agg;
            }
        }

        /**
     * This will hold any graphics or charts for the sheet.
     *
     * @return the top-level drawing patriarch, if there is one, else returns null
     */
        public IDrawing DrawingPatriarch
        {
            get
            {
                _patriarch = GetPatriarch(false);
                return _patriarch;
            }
        }

        /**
         * Creates the top-level drawing patriarch.  This will have
         * the effect of removing any existing drawings on this
         * sheet.
         * This may then be used to add graphics or charts
         *
         * @return The new patriarch.
         */
        public IDrawing CreateDrawingPatriarch()
        {
            _patriarch = GetPatriarch(true);
            return _patriarch;
        }

        private HSSFPatriarch GetPatriarch(bool createIfMissing)
        {
            HSSFPatriarch patriarch = null;
            if (_patriarch != null)
            {
                return _patriarch;
            }
            DrawingManager2 dm = book.FindDrawingGroup();
            if (null == dm)
            {
                if (!createIfMissing)
                {
                    return null;
                }
                else
                {
                    book.CreateDrawingGroup();
                    dm = book.DrawingManager;
                }
            }
            EscherAggregate agg = (EscherAggregate)_sheet.FindFirstRecordBySid(EscherAggregate.sid);
            if (null == agg)
            {
                int pos = _sheet.AggregateDrawingRecords(dm, false);
                if (-1 == pos)
                {
                    if (createIfMissing)
                    {
                        pos = _sheet.AggregateDrawingRecords(dm, true);
                        agg = (EscherAggregate)_sheet.Records[pos];
                        patriarch = new HSSFPatriarch(this, agg);
                        patriarch.AfterCreate();
                        return patriarch;
                    }
                    else
                    {
                        return null;
                    }
                }
                agg = (EscherAggregate)_sheet.Records[(pos)];
            }
            return new HSSFPatriarch(this, agg);
        }

        /// <summary>
        /// Gets or sets the tab color of the _sheet
        /// </summary>
        public short TabColorIndex
        {
            get { return _sheet.TabColorIndex; }
            set { _sheet.TabColorIndex = value; }
        }

        /// <summary>
        /// Gets or sets whether the tab color of _sheet is automatic
        /// </summary>
        public bool IsAutoTabColor
        {
            get { return _sheet.IsAutoTabColor; }
            set { _sheet.IsAutoTabColor = value; }
        }

        /// <summary>
        /// Expands or collapses a column Group.
        /// </summary>
        /// <param name="columnNumber">One of the columns in the Group.</param>
        /// <param name="collapsed">true = collapse Group, false = expand Group.</param>
        public void SetColumnGroupCollapsed(int columnNumber, bool collapsed)
        {
            _sheet.SetColumnGroupCollapsed(columnNumber, collapsed);
        }

        /// <summary>
        /// Create an outline for the provided column range.
        /// </summary>
        /// <param name="fromColumn">beginning of the column range.</param>
        /// <param name="toColumn">end of the column range.</param>
        public void GroupColumn(int fromColumn, int toColumn)
        {
            _sheet.GroupColumnRange(fromColumn, toColumn, true);
        }

        /// <summary>
        /// Ungroups the column.
        /// </summary>
        /// <param name="fromColumn">From column.</param>
        /// <param name="toColumn">To column.</param>
        public void UngroupColumn(int fromColumn, int toColumn)
        {
            _sheet.GroupColumnRange(fromColumn, toColumn, false);
        }

        /// <summary>
        /// Groups the row.
        /// </summary>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        public void GroupRow(int fromRow, int toRow)
        {
            _sheet.GroupRowRange(fromRow, toRow, true);
        }

        /// <summary>
        /// Remove a Array Formula from this sheet.  All cells contained in the Array Formula range are removed as well
        /// </summary>
        /// <param name="cell">any cell within Array Formula range</param>
        /// <returns>the <see cref="ICellRange{ICell}"/> of cells affected by this change</returns>
        public ICellRange<ICell> RemoveArrayFormula(ICell cell)
        {
            if (cell.Sheet != this)
            {
                throw new ArgumentException("Specified cell does not belong to this sheet.");
            }
            CellValueRecordInterface rec = ((HSSFCell)cell).CellValueRecord;
            if (!(rec is FormulaRecordAggregate))
            {
                String ref1 = new CellReference(cell).FormatAsString();
                throw new ArgumentException("Cell " + ref1 + " is not part of an array formula.");
            }
            FormulaRecordAggregate fra = (FormulaRecordAggregate)rec;
            CellRangeAddress range = fra.RemoveArrayFormula(cell.RowIndex, cell.ColumnIndex);

            ICellRange<ICell> result = GetCellRange(range);
            // clear all cells in the range
            foreach (ICell c in result)
            {
                c.SetCellType(CellType.BLANK);
            }
            return result;
        }

        /// <summary>
        /// Also creates cells if they don't exist.
        /// </summary>
        private ICellRange<ICell> GetCellRange(CellRangeAddress range)
        {
            int firstRow = range.FirstRow;
            int firstColumn = range.FirstColumn;
            int lastRow = range.LastRow;
            int lastColumn = range.LastColumn;
            int height = lastRow - firstRow + 1;
            int width = lastColumn - firstColumn + 1;
            List<ICell> temp = new List<ICell>(height * width);
            for (int rowIn = firstRow; rowIn <= lastRow; rowIn++)
            {
                for (int colIn = firstColumn; colIn <= lastColumn; colIn++)
                {
                    IRow row = GetRow(rowIn);
                    if (row == null)
                    {
                        row = CreateRow(rowIn);
                    }
                    ICell cell = row.GetCell(colIn);
                    if (cell == null)
                    {
                        cell = row.CreateCell(colIn);
                    }
                    temp.Add(cell);
                }
            }
            return SSCellRange<ICell>.Create(firstRow, firstColumn, height, width, temp, typeof(HSSFCell));
        }

        /// <summary>
        /// Sets array formula to specified region for result.
        /// </summary>
        /// <param name="formula">text representation of the formula</param>
        /// <param name="range">Region of array formula for result</param>
        /// <returns>the <see cref="ICellRange{ICell}"/> of cells affected by this change</returns>
        public ICellRange<ICell> SetArrayFormula(String formula, CellRangeAddress range)
        {
            // make sure the formula parses OK first
            int sheetIndex = _workbook.GetSheetIndex(this);
            Ptg[] ptgs = HSSFFormulaParser.Parse(formula, _workbook, FormulaType.ARRAY, sheetIndex);
            ICellRange<ICell> cells = GetCellRange(range);

            foreach (HSSFCell c in cells)
            {
                c.SetCellArrayFormula(range);
            }
            HSSFCell mainArrayFormulaCell = (HSSFCell)cells.TopLeftCell;
            FormulaRecordAggregate agg = (FormulaRecordAggregate)mainArrayFormulaCell.CellValueRecord;
            agg.SetArrayFormula(range, ptgs);
            return cells;
        }


        /// <summary>
        /// Ungroups the row.
        /// </summary>
        /// <param name="fromRow">From row.</param>
        /// <param name="toRow">To row.</param>
        public void UngroupRow(int fromRow, int toRow)
        {
            _sheet.GroupRowRange(fromRow, toRow, false);
        }

        /// <summary>
        /// Sets the row group collapsed.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="collapse">if set to <c>true</c> [collapse].</param>
        public void SetRowGroupCollapsed(int row, bool collapse)
        {
            if (collapse)
            {
                _sheet.RowsAggregate.CollapseRow(row);
            }
            else
            {
                _sheet.RowsAggregate.ExpandRow(row);
            }
        }

        /// <summary>
        /// Sets the default column style for a given column.  POI will only apply this style to new cells Added to the _sheet.
        /// </summary>
        /// <param name="column">the column index</param>
        /// <param name="style">the style to set</param>
        public void SetDefaultColumnStyle(int column, NPOI.SS.UserModel.ICellStyle style)
        {
            _sheet.SetDefaultColumnStyle(column, style.Index);
        }

        /// <summary>
        /// Adjusts the column width to fit the contents.
        /// This Process can be relatively slow on large sheets, so this should
        /// normally only be called once per column, at the end of your
        /// Processing.
        /// </summary>
        /// <param name="column">the column index.</param>
        public void AutoSizeColumn(int column)
        {
            AutoSizeColumn(column, false);
        }

        /// <summary>
        /// Adjusts the column width to fit the contents.
        /// This Process can be relatively slow on large sheets, so this should
        /// normally only be called once per column, at the end of your
        /// Processing.
        /// You can specify whether the content of merged cells should be considered or ignored.
        /// Default is to ignore merged cells.
        /// </summary>
        /// <param name="column">the column index</param>
        /// <param name="useMergedCells">whether to use the contents of merged cells when calculating the width of the column</param>
        public void AutoSizeColumn(int column, bool useMergedCells)
        {
            double width = SheetUtil.GetColumnWidth(this, column, useMergedCells);
            if (width != -1)
            {
                width *= 256;
                int maxColumnWidth = 255 * 256; // The maximum column width for an individual cell is 255 characters

                if (width > maxColumnWidth)
                {
                    width = maxColumnWidth;
                }
                SetColumnWidth(column, (int)width);
            }
        }

        /// <summary>
        /// Checks if the provided region is part of the merged regions.
        /// </summary>
        /// <param name="mergedRegion">Region searched in the merged regions</param>
        /// <returns><c>true</c>, when the region is contained in at least one of the merged regions</returns>
        public bool IsMergedRegion(CellRangeAddress mergedRegion)
        {
            foreach (CellRangeAddress range in _sheet.MergedRecords.MergedRegions)
            {
                if (range.FirstColumn <= mergedRegion.FirstColumn
                    && range.LastColumn >= mergedRegion.LastColumn
                    && range.FirstRow <= mergedRegion.FirstRow
                    && range.LastRow >= mergedRegion.LastRow)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Gets the merged region at the specified index
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public CellRangeAddress GetMergedRegion(int index)
        {
            return _sheet.GetMergedRegionAt(index);
        }
        /// <summary>
        /// Convert HSSFFont to Font.
        /// </summary>
        /// <param name="font1">The font.</param>
        /// <returns></returns>
        public System.Drawing.Font HSSFFont2Font(HSSFFont font1)
        {
            return new System.Drawing.Font(font1.FontName, font1.FontHeightInPoints);
        }

        /// <summary>
        /// Returns cell comment for the specified row and column
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>cell comment or null if not found</returns>
        public IComment GetCellComment(int row, int column)
        {
            return FindCellComment(row, column);
        }

        /// <summary>
        /// Gets the sheet conditional formatting.
        /// </summary>
        /// <value>The sheet conditional formatting.</value>
        public ISheetConditionalFormatting SheetConditionalFormatting
        {
            get
            {
                return new HSSFSheetConditionalFormatting(this);
            }
        }
        /// <summary>
        /// Get the DVRecords objects that are associated to this _sheet
        /// </summary>
        /// <value>a list of DVRecord instances</value>
        public IList DVRecords
        {
            get
            {
                IList dvRecords = new ArrayList();
                IList records = _sheet.Records;

                for (int index = 0; index < records.Count; index++)
                {
                    if (records[index] is DVRecord)
                    {
                        dvRecords.Add(records[index]);
                    }
                }
                return dvRecords;
            }
        }
        /// <summary>
        /// Provide a reference to the parent workbook.
        /// </summary>
        public NPOI.SS.UserModel.IWorkbook Workbook
        {
            get
            {
                return _workbook;
            }
        }

        /// <summary>
        /// Returns the name of this _sheet
        /// </summary>
        public String SheetName
        {
            get
            {
                NPOI.SS.UserModel.IWorkbook wb = Workbook;
                int idx = wb.GetSheetIndex(this);
                return wb.GetSheetName(idx);
            }
        }

        /// <summary>
        /// Create an instance of a DataValidationHelper.
        /// </summary>
        /// <returns>Instance of a DataValidationHelper</returns>
        public IDataValidationHelper GetDataValidationHelper()
        {
            return new HSSFDataValidationHelper(this);
        }

        /// <summary>
        /// Enable filtering for a range of cells
        /// </summary>
        /// <param name="range">the range of cells to filter</param>
        public IAutoFilter SetAutoFilter(CellRangeAddress range)
        {
            InternalWorkbook workbook = _workbook.Workbook;
            int sheetIndex = _workbook.GetSheetIndex(this);

            NameRecord name = workbook.GetSpecificBuiltinRecord(NameRecord.BUILTIN_FILTER_DB, sheetIndex + 1);

            if (name == null)
            {
                name = workbook.CreateBuiltInName(NameRecord.BUILTIN_FILTER_DB, sheetIndex + 1);
            }

            // The built-in name must consist of a single Area3d Ptg.
            Area3DPtg ptg = new Area3DPtg(range.FirstRow, range.LastRow,
                    range.FirstColumn, range.LastColumn,
                    false, false, false, false, sheetIndex);
            name.NameDefinition = (new Ptg[] { ptg });

            AutoFilterInfoRecord r = new AutoFilterInfoRecord();
            // the number of columns that have AutoFilter enabled.
            int numcols = 1 + range.LastColumn - range.FirstColumn;
            r.NumEntries = (short)numcols;
            int idx = _sheet.FindFirstRecordLocBySid(DimensionsRecord.sid);
            _sheet.Records.Insert(idx, r);

            //create a combobox control for each column
            HSSFPatriarch p = (HSSFPatriarch)CreateDrawingPatriarch();
            for (int col = range.FirstColumn; col <= range.LastColumn; col++)
            {
                p.CreateComboBox(new HSSFClientAnchor(0, 0, 0, 0,
                        (short)col, range.FirstRow, (short)(col + 1), range.FirstRow + 1));
            }

            return new HSSFAutoFilter(this);
        }

        protected internal HSSFComment FindCellComment(int row, int column)
        {
            HSSFPatriarch patriarch = DrawingPatriarch as HSSFPatriarch;
            if (null == patriarch)
            {
                patriarch = CreateDrawingPatriarch() as HSSFPatriarch;
            }
            return LookForComment(patriarch, row, column);
        }

        private HSSFComment LookForComment(HSSFShapeContainer container, int row, int column)
        {
            foreach (Object obj in container.Children)
            {
                HSSFShape shape = (HSSFShape)obj;
                if (shape is HSSFShapeGroup)
                {
                    HSSFShape res = LookForComment((HSSFShapeContainer)shape, row, column);
                    if (null != res)
                    {
                        return (HSSFComment)res;
                    }
                    continue;
                }
                if (shape is HSSFComment)
                {
                    HSSFComment comment = (HSSFComment)shape;
                    if (comment.Column == column && comment.Row == row)
                    {
                        return comment;
                    }
                }
            }
            return null;
        }
        public CellRangeAddress RepeatingRows
        {
            get
            {
                return GetRepeatingRowsOrColums(true);
            }
            set
            {
                CellRangeAddress columnRangeRef = RepeatingColumns;
                SetRepeatingRowsAndColumns(value, columnRangeRef);
            }
        }


        public CellRangeAddress RepeatingColumns
        {
            get
            {
                return GetRepeatingRowsOrColums(false);
            }
            set
            {
                CellRangeAddress rowRangeRef = RepeatingRows;
                SetRepeatingRowsAndColumns(rowRangeRef, value);
            }
        }



        private void SetRepeatingRowsAndColumns(
            CellRangeAddress rowDef, CellRangeAddress colDef)
        {
            int sheetIndex = _workbook.GetSheetIndex(this);
            int maxRowIndex = SpreadsheetVersion.EXCEL97.LastRowIndex;
            int maxColIndex = SpreadsheetVersion.EXCEL97.LastColumnIndex;

            int col1 = -1;
            int col2 = -1;
            int row1 = -1;
            int row2 = -1;

            if (rowDef != null)
            {
                row1 = rowDef.FirstRow;
                row2 = rowDef.LastRow;
                if ((row1 == -1 && row2 != -1) || (row1 > row2)
                     || (row1 < 0 || row1 > maxRowIndex)
                     || (row2 < 0 || row2 > maxRowIndex))
                {
                    throw new ArgumentException("Invalid row range specification");
                }
            }
            if (colDef != null)
            {
                col1 = colDef.FirstColumn;
                col2 = colDef.LastColumn;
                if ((col1 == -1 && col2 != -1) || (col1 > col2)
                    || (col1 < 0 || col1 > maxColIndex)
                    || (col2 < 0 || col2 > maxColIndex))
                {
                    throw new ArgumentException("Invalid column range specification");
                }
            }

            short externSheetIndex =
              (short)_workbook.Workbook.CheckExternSheet(sheetIndex);

            bool setBoth = rowDef != null && colDef != null;
            bool removeAll = rowDef == null && colDef == null;

            HSSFName name = _workbook.GetBuiltInName(NameRecord.BUILTIN_PRINT_TITLE, sheetIndex);
            if (removeAll)
            {
                if (name != null)
                {
                    _workbook.RemoveName(name);
                }
                return;
            }
            if (name == null)
            {
                name = _workbook.CreateBuiltInName(
                    NameRecord.BUILTIN_PRINT_TITLE, sheetIndex);
            }

            List<Ptg> ptgList = new List<Ptg>();
            if (setBoth)
            {
                int exprsSize = 2 * 11 + 1; // 2 * Area3DPtg.SIZE + UnionPtg.SIZE
                ptgList.Add(new MemFuncPtg(exprsSize));
            }
            if (colDef != null)
            {
                Area3DPtg colArea = new Area3DPtg(0, maxRowIndex, col1, col2,
                        false, false, false, false, externSheetIndex);
                ptgList.Add(colArea);
            }
            if (rowDef != null)
            {
                Area3DPtg rowArea = new Area3DPtg(row1, row2, 0, maxColIndex,
                        false, false, false, false, externSheetIndex);
                ptgList.Add(rowArea);
            }
            if (setBoth)
            {
                ptgList.Add(UnionPtg.instance);
            }

            Ptg[] ptgs = ptgList.ToArray();
            //ptgList.toArray(ptgs);
            name.SetNameDefinition(ptgs);

            HSSFPrintSetup printSetup = (HSSFPrintSetup)PrintSetup;
            printSetup.ValidSettings = (false);
            SetActive(true);
        }

        private CellRangeAddress GetRepeatingRowsOrColums(bool rows)
        {
            NameRecord rec = GetBuiltinNameRecord(NameRecord.BUILTIN_PRINT_TITLE);
            if (rec == null)
            {
                return null;
            }
            Ptg[] nameDefinition = rec.NameDefinition;
            if (rec.NameDefinition == null)
            {
                return null;
            }

            int maxRowIndex = SpreadsheetVersion.EXCEL97.LastRowIndex;
            int maxColIndex = SpreadsheetVersion.EXCEL97.LastColumnIndex;

            foreach (Ptg ptg in nameDefinition)
            {

                if (ptg is Area3DPtg)
                {
                    Area3DPtg areaPtg = (Area3DPtg)ptg;

                    if (areaPtg.FirstColumn == 0
                        && areaPtg.LastColumn == maxColIndex)
                    {
                        if (rows)
                        {
                            CellRangeAddress rowRange = new CellRangeAddress(
                                areaPtg.FirstRow, areaPtg.LastRow, -1, -1);
                            return rowRange;
                        }
                    }
                    else if (areaPtg.FirstRow == 0
                      && areaPtg.LastRow == maxRowIndex)
                    {
                        if (!rows)
                        {
                            CellRangeAddress columnRange = new CellRangeAddress(-1, -1,
                                areaPtg.FirstColumn, areaPtg.LastColumn);
                            return columnRange;
                        }
                    }

                }

            }

            return null;
        }


        private NameRecord GetBuiltinNameRecord(byte builtinCode)
        {
            int sheetIndex = _workbook.GetSheetIndex(this);
            int recIndex =
              _workbook.FindExistingBuiltinNameRecordIdx(sheetIndex, builtinCode);
            if (recIndex == -1)
            {
                return null;
            }
            return _workbook.GetNameRecord(recIndex);
        }

        // Copy sheet based on logic posted by "brimars" on 2011-04-29 at http://npoi.codeplex.com/discussions/254536
        // That code was based on: http://www.coderanch.com/t/420958/open-source/Copying-sheet-excel-file-another
        // thanks to: Pierre Guilbert, 2011-04-14
        //
        // Modified again on 2012-01-09 by Paul Kratt (Fixed copied sheet corruption in C# version, 
        // added color palette merging, copy images, reassign merged colors. Color palette merge was necessary
        // for copying optimized sheets generated by MS SSRS 2008, because they only contain colors used in the document.)
        //
        // Original code comments:
        /**
         * @author jk
         * getted from http://jxls.cvs.sourceforge.net/jxls/jxls/src/java/org/jxls/util/Util.java?revision=1.8&view=markup
         * by Leonid Vysochyn 
         * and modified (adding styles copying)
         * modified by Philipp Lopmeier (replacing deprecated classes and methods, using generic types)
         */
        public HSSFSheet CopySheet()
        {
            return CopySheet(string.Concat(SheetName, " - Copy"), true);
        }

        public HSSFSheet CopySheet(String Name)
        {
            return CopySheet(Name, true);
        }

        public HSSFSheet CopySheet(Boolean CopyStyle)
        {
            return CopySheet(string.Concat(SheetName, " - Copy"), CopyStyle);
        }

        public HSSFSheet CopySheet(String Name, Boolean CopyStyle)
        {
            int maxColumnNum = 0;
            HSSFSheet newSheet = (HSSFSheet)Workbook.CreateSheet(Name);
            newSheet._sheet = Sheet.CloneSheet();
            IDictionary<Int32, HSSFCellStyle> styleMap = (CopyStyle) ? new Dictionary<Int32, HSSFCellStyle>() : null;
            for (int i = FirstRowNum; i <= LastRowNum; i++)
            {
                HSSFRow srcRow = (HSSFRow)GetRow(i);
                HSSFRow destRow = (HSSFRow)newSheet.CreateRow(i);
                if (srcRow != null)
                {
                    CopyRow(this, newSheet, srcRow, destRow, styleMap, new Dictionary<short, short>(), true);
                    if (srcRow.LastCellNum > maxColumnNum)
                    {
                        maxColumnNum = srcRow.LastCellNum;
                    }
                }
            }
            for (int i = 0; i <= maxColumnNum; i++)
            {
                newSheet.SetColumnWidth(i, GetColumnWidth(i));
            }
            newSheet.ForceFormulaRecalculation = true;
            newSheet.PrintSetup.Landscape = PrintSetup.Landscape;
            newSheet.PrintSetup.HResolution = PrintSetup.HResolution;
            newSheet.PrintSetup.VResolution = PrintSetup.VResolution;
            newSheet.SetMargin(MarginType.LeftMargin, GetMargin(MarginType.LeftMargin));
            newSheet.SetMargin(MarginType.RightMargin, GetMargin(MarginType.RightMargin));
            newSheet.SetMargin(MarginType.TopMargin, GetMargin(MarginType.TopMargin));
            newSheet.SetMargin(MarginType.BottomMargin, GetMargin(MarginType.BottomMargin));
            newSheet.PrintSetup.HeaderMargin = PrintSetup.HeaderMargin;
            newSheet.PrintSetup.FooterMargin = PrintSetup.FooterMargin;
            newSheet.Header.Left = Header.Left;
            newSheet.Header.Center = Header.Center;
            newSheet.Header.Right = Header.Right;
            newSheet.Footer.Left = Footer.Left;
            newSheet.Footer.Center = Footer.Center;
            newSheet.Footer.Right = Footer.Right;
            newSheet.PrintSetup.Scale = PrintSetup.Scale;
            newSheet.PrintSetup.FitHeight = PrintSetup.FitHeight;
            newSheet.PrintSetup.FitWidth = PrintSetup.FitWidth;
            return newSheet;
        }

        public void CopyTo(HSSFWorkbook dest, String name, Boolean copyStyle, Boolean keepFormulas)
        {
            int maxColumnNum = 0;
            HSSFSheet newSheet = (HSSFSheet)dest.CreateSheet(name);
            newSheet._sheet = Sheet.CloneSheet();
            Dictionary<short, short> paletteMap = new Dictionary<short, short>();
            if (dest.NumberOfSheets == 1)
            {
                //Replace the color palette with the palette from the source, since this is the first sheet
                dest.Workbook.CustomPalette.ClearColors();
                paletteMap = MergePalettes(Workbook as HSSFWorkbook, dest);
            }
            else if (dest != Workbook)
            {
                paletteMap = MergePalettes(Workbook as HSSFWorkbook, dest);
            }
            IDictionary<Int32, HSSFCellStyle> styleMap = (copyStyle) ? new Dictionary<Int32, HSSFCellStyle>() : null;
            for (int i = FirstRowNum; i <= LastRowNum; i++)
            {
                HSSFRow srcRow = (HSSFRow)GetRow(i);
                HSSFRow destRow = (HSSFRow)newSheet.CreateRow(i);
                if (srcRow != null)
                {
                    CopyRow(this, newSheet, srcRow, destRow, styleMap, paletteMap, keepFormulas);
                    if (srcRow.LastCellNum > maxColumnNum)
                    {
                        maxColumnNum = srcRow.LastCellNum;
                    }
                }
            }
            for (int i = 0; i < maxColumnNum; i++)
            {
                newSheet.SetColumnWidth(i, GetColumnWidth(i));
            }
            newSheet.ForceFormulaRecalculation = true;
            newSheet.PrintSetup.Landscape = PrintSetup.Landscape;
            newSheet.PrintSetup.HResolution = PrintSetup.HResolution;
            newSheet.PrintSetup.VResolution = PrintSetup.VResolution;
            newSheet.SetMargin(MarginType.LeftMargin, GetMargin(MarginType.LeftMargin));
            newSheet.SetMargin(MarginType.RightMargin, GetMargin(MarginType.RightMargin));
            newSheet.SetMargin(MarginType.TopMargin, GetMargin(MarginType.TopMargin));
            newSheet.SetMargin(MarginType.BottomMargin, GetMargin(MarginType.BottomMargin));
            newSheet.PrintSetup.HeaderMargin = PrintSetup.HeaderMargin;
            newSheet.PrintSetup.FooterMargin = PrintSetup.FooterMargin;
            newSheet.Header.Left = Header.Left;
            newSheet.Header.Center = Header.Center;
            newSheet.Header.Right = Header.Right;
            newSheet.Footer.Left = Footer.Left;
            newSheet.Footer.Center = Footer.Center;
            newSheet.Footer.Right = Footer.Right;
            newSheet.PrintSetup.Scale = PrintSetup.Scale;
            newSheet.PrintSetup.FitHeight = PrintSetup.FitHeight;
            newSheet.PrintSetup.FitWidth = PrintSetup.FitWidth;
            EscherAggregate escher = DrawingEscherAggregate;
            if (escher != null)
            {
                if (dest.Workbook.DrawingManager == null)
                {
                    dest.Workbook.CreateDrawingGroup();
                }
                EscherAggregate destEscher = newSheet.DrawingEscherAggregate;
                //Note: This logic assumes that image id's go from 1 to N in the source document. It usually does
                //Note: This logic assumes that no images are shared between sheets of the source document. If they
                //are and you're copying multiple sheets, the file may be larger than expected due to duplicates.
                IEnumerable<int> usedImages = FindUsedPictures(escher.EscherRecords);
                Dictionary<int,int> remap = new Dictionary<int, int>();
                IList pics = Workbook.GetAllPictures();
                foreach (int imgId in usedImages)
                {
                    if (imgId <= pics.Count)
                    {
                        HSSFPictureData pic = (HSSFPictureData)pics[imgId - 1];
                        int dstIdx = dest.AddPicture(pic.Data, (PictureType)pic.Format);
                        remap.Add(imgId, dstIdx);
                    }
                }
                //Apply the new image Id's the destination
                foreach (EscherRecord escherRecord in destEscher.EscherRecords)
                {
                    ApplyEscherRemap(escherRecord, remap);
                }
            }
        }

        private IEnumerable<int> FindUsedPictures(IEnumerable<EscherRecord> escherRecords)
        {
            List<int> retval = new List<int>();
            foreach (EscherRecord escherRecord in escherRecords)
            {
                GetSheetImageIds(escherRecord, retval);
            }
            return retval;
        }

        private void GetSheetImageIds(EscherRecord parent, List<int> usedIds)
        {
            foreach (EscherRecord child in parent.ChildRecords)
            {
                if (child is EscherOptRecord)
                {
                    EscherOptRecord picOpts = (EscherOptRecord)child;
                    foreach (EscherProperty eprop in picOpts.EscherProperties)
                    {
                        if (eprop.PropertyNumber == EscherProperties.BLIP__BLIPTODISPLAY)
                        {
                            //This is the picture ID property
                            int pictureId = ((EscherSimpleProperty) eprop).PropertyValue;
                            if (!usedIds.Contains(pictureId))
                            {
                                usedIds.Add(pictureId);
                            }
                            break;
                        }
                    }
                }
                if (child.ChildRecords.Count > 0)
                {
                    foreach (EscherRecord grandKid in child.ChildRecords)
                    {
                        GetSheetImageIds(grandKid, usedIds);
                    }
                }
            }
        }

        private void ApplyEscherRemap(EscherRecord parent, Dictionary<int,int> mappings)
        {
            foreach (EscherRecord child in parent.ChildRecords)
            {
                if (child is EscherOptRecord)
                {
                    EscherOptRecord picOpts = (EscherOptRecord)child;
                    foreach (EscherProperty eprop in picOpts.EscherProperties)
                    {
                        if (eprop.PropertyNumber == EscherProperties.BLIP__BLIPTODISPLAY)
                        {
                            //This is the picture ID property
                            int pictureId = ((EscherSimpleProperty)eprop).PropertyValue;
                            if (mappings.ContainsKey(pictureId))
                            {
                                ((EscherSimpleProperty) eprop).PropertyValue = mappings[pictureId];
                            }
                            break;
                        }
                    }
                }
                if (child.ChildRecords.Count > 0)
                {
                    foreach (EscherRecord grandKid in child.ChildRecords)
                    {
                        ApplyEscherRemap(grandKid, mappings);
                    }
                }
            }
        }

        private static Dictionary<short,short> MergePalettes(HSSFWorkbook source, HSSFWorkbook dest)
        {
            Dictionary<short, short> retval = new Dictionary<short, short>();
            //This is a slow way to accomplish this, but since the color limit is 56 it won't take long
            for (short i = 0; i < source.Workbook.CustomPalette.NumColors; i++)
            {
                byte[] sourceColor = source.Workbook.CustomPalette.GetColor((short)(i + PaletteRecord.FIRST_COLOR_INDEX));
                bool found = false;
                for (short j = 0; j < dest.Workbook.CustomPalette.NumColors; j++)
                {
                    byte[] destColor = dest.Workbook.CustomPalette.GetColor((short)(j + PaletteRecord.FIRST_COLOR_INDEX));
                    if (sourceColor[0] == destColor[0] && sourceColor[1] == destColor[1] && sourceColor[2] == destColor[2])
                    {
                        found = true;
                        retval.Add((short)(i + PaletteRecord.FIRST_COLOR_INDEX), (short)(j + PaletteRecord.FIRST_COLOR_INDEX));
                        break;
                    }
                }
                if (!found) //Color doesn't exist in this palette, add it
                {
                    short createdIdx = dest.Workbook.CustomPalette.NumColors;
                    dest.Workbook.CustomPalette.SetColor((short)(createdIdx + PaletteRecord.FIRST_COLOR_INDEX), sourceColor[0], sourceColor[1], sourceColor[2]);
                    retval.Add((short)(i + PaletteRecord.FIRST_COLOR_INDEX), (short)(createdIdx + PaletteRecord.FIRST_COLOR_INDEX));
                }
            }
            return retval;
        }

        private static void CopyRow(HSSFSheet srcSheet, HSSFSheet destSheet, HSSFRow srcRow, HSSFRow destRow, IDictionary<Int32, HSSFCellStyle> styleMap, Dictionary<short, short> paletteMap, bool keepFormulas)
        {
            List<SS.Util.CellRangeAddress> mergedRegions = destSheet.Sheet.MergedRecords.MergedRegions;
            destRow.Height = srcRow.Height;
            destRow.IsHidden = srcRow.IsHidden;
            destRow.RowRecord.OptionFlags = srcRow.RowRecord.OptionFlags;
            for (int j = srcRow.FirstCellNum; j <= srcRow.LastCellNum; j++)
            {
                HSSFCell oldCell = (HSSFCell)srcRow.GetCell(j);
                HSSFCell newCell = (HSSFCell)destRow.GetCell(j);
                if (srcSheet.Workbook == destSheet.Workbook)
                {
                    newCell = (HSSFCell)destRow.GetCell(j);
                }
                if (oldCell != null)
                {
                    if (newCell == null)
                    {
                        newCell = (HSSFCell)destRow.CreateCell(j);
                    }
                    CopyCell(oldCell, newCell, styleMap, paletteMap, keepFormulas);
                    CellRangeAddress mergedRegion = GetMergedRegion(srcSheet, srcRow.RowNum, (short)oldCell.ColumnIndex);
                    if (mergedRegion != null)
                    {
                        CellRangeAddress newMergedRegion = new CellRangeAddress(mergedRegion.FirstRow,
                                mergedRegion.LastRow, mergedRegion.FirstColumn, mergedRegion.LastColumn);

                        if (IsNewMergedRegion(newMergedRegion, mergedRegions))
                        {
                            mergedRegions.Add(newMergedRegion);
                        }
                    }
                }
            }
        }

        private static void CopyCell(HSSFCell oldCell, HSSFCell newCell, IDictionary<Int32, HSSFCellStyle> styleMap, Dictionary<short, short> paletteMap, Boolean keepFormulas)
        {
            if (styleMap != null)
            {
                if (oldCell.CellStyle != null)
                {
                    if (oldCell.Sheet.Workbook == newCell.Sheet.Workbook)
                    {
                        newCell.CellStyle = oldCell.CellStyle;
                    }
                    else
                    {
                        int styleHashCode = oldCell.CellStyle.GetHashCode();
                        if (styleMap.ContainsKey(styleHashCode))
                        {
                            newCell.CellStyle = styleMap[styleHashCode];
                        }
                        else
                        {
                            HSSFCellStyle newCellStyle = (HSSFCellStyle)newCell.Sheet.Workbook.CreateCellStyle();
                            newCellStyle.CloneStyleFrom(oldCell.CellStyle);
                            RemapCellStyle(newCellStyle, paletteMap); //Clone copies as-is, we need to remap colors manually
                            newCell.CellStyle = newCellStyle;
                            //Clone of cell style always clones the font. This makes my life easier
                            IFont theFont = newCellStyle.GetFont(newCell.Sheet.Workbook);
                            if (theFont.Color > 0 && paletteMap.ContainsKey(theFont.Color))
                            {
                                theFont.Color = paletteMap[theFont.Color]; //Remap font color
                            }
                            styleMap.Add(styleHashCode, newCellStyle);
                        }
                    }
                }
                else
                {
                    newCell.CellStyle = null;
                }
            }
            switch (oldCell.CellType)
            {
                case NPOI.SS.UserModel.CellType.STRING:
                    newCell.SetCellValue(oldCell.StringCellValue);
                    break;
                case NPOI.SS.UserModel.CellType.NUMERIC:
                    newCell.SetCellValue(oldCell.NumericCellValue);
                    break;
                case NPOI.SS.UserModel.CellType.BLANK:
                    newCell.SetCellType(NPOI.SS.UserModel.CellType.BLANK);
                    break;
                case NPOI.SS.UserModel.CellType.BOOLEAN:
                    newCell.SetCellValue(oldCell.BooleanCellValue);
                    break;
                case NPOI.SS.UserModel.CellType.ERROR:
                    newCell.SetCellValue(oldCell.ErrorCellValue);
                    break;
                case NPOI.SS.UserModel.CellType.FORMULA:
                    if (keepFormulas)
                    {
                        newCell.SetCellType(CellType.FORMULA);
                        newCell.CellFormula = oldCell.CellFormula;
                    }
                    else
                    {
                        try
                        {
                            newCell.SetCellType(CellType.NUMERIC);
                            newCell.SetCellValue(oldCell.NumericCellValue);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                newCell.SetCellType(CellType.STRING);
                                newCell.SetCellValue(oldCell.StringCellValue);
                            }
                            catch (Exception exInner)
                            {
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Translate color palette entries from the source to the destination sheet
        /// </summary>
        private static void RemapCellStyle(HSSFCellStyle stylish, Dictionary<short, short> paletteMap)
        {
            if (paletteMap.ContainsKey(stylish.BorderDiagonalColor))
            {
                stylish.BorderDiagonalColor = paletteMap[stylish.BorderDiagonalColor];
            }
            if (paletteMap.ContainsKey(stylish.BottomBorderColor))
            {
                stylish.BottomBorderColor = paletteMap[stylish.BottomBorderColor];
            }
            if (paletteMap.ContainsKey(stylish.FillBackgroundColor))
            {
                stylish.FillBackgroundColor = paletteMap[stylish.FillBackgroundColor];
            }
            if (paletteMap.ContainsKey(stylish.FillForegroundColor))
            {
                stylish.FillForegroundColor = paletteMap[stylish.FillForegroundColor];
            }
            if (paletteMap.ContainsKey(stylish.LeftBorderColor))
            {
                stylish.LeftBorderColor = paletteMap[stylish.LeftBorderColor];
            }
            if (paletteMap.ContainsKey(stylish.RightBorderColor))
            {
                stylish.RightBorderColor = paletteMap[stylish.RightBorderColor];
            }
            if (paletteMap.ContainsKey(stylish.TopBorderColor))
            {
                stylish.TopBorderColor = paletteMap[stylish.TopBorderColor];
            }
        }

        public static CellRangeAddress GetMergedRegion(HSSFSheet sheet, int rowNum, short cellNum)
        {
            for (int i = 0; i < sheet.NumMergedRegions; i++)
            {
                CellRangeAddress merged = sheet.GetMergedRegion(i);
                if (rowNum >= merged.FirstRow && rowNum <= merged.LastRow)
                {
                    if (cellNum >= merged.FirstColumn && cellNum <= merged.LastColumn)
                    {
                        return merged;
                    }
                }
            }
            return null;
        }

        // modified syntax from Java to C#
        private static bool AreAllTrue(params bool[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (values[i] != true)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNewMergedRegion(CellRangeAddress newMergedRegion, List<CellRangeAddress> mergedRegions)
        {
            bool isNew = true;

            // we want to check if newMergedRegion is contained inside our collection
            foreach (CellRangeAddress add in mergedRegions)
            {
                bool r1 = (add.FirstRow == newMergedRegion.FirstRow);
                bool r2 = (add.LastRow == newMergedRegion.LastRow);
                bool c1 = (add.FirstColumn == newMergedRegion.FirstColumn);
                bool c2 = (add.LastColumn == newMergedRegion.LastColumn);
                if (AreAllTrue(r1, r2, c1, c2))
                {
                    isNew = false;
                }
            }
            return isNew;
        }

        // end of POI CopySheets
    }
}