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

namespace NPOI.HSSF.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using NPOI.HSSF.UserModel;
    using NPOI.HPSF;
    using NPOI.SS.Util;
    using NPOI.SS.UserModel;
    using NPOI.SS.Formula.Eval;
    using NPOI.Util;
    using NPOI.HSSF.Util;
    using NPOI.SS;

    public class ExcelToHtmlConverter
    {
        POILogger logger = POILogFactory.GetLogger(typeof(ExcelToHtmlConverter));
        public ExcelToHtmlConverter()
        {
            XmlDocument doc = new XmlDocument();
            htmlDocumentFacade = new HtmlDocumentFacade(doc);
            cssClassTable = htmlDocumentFacade.GetOrCreateCssClass("table", "t",
                    "border-collapse:collapse;border-spacing:0;");
        }
        protected static int GetColumnWidth(HSSFSheet sheet, int columnIndex)
        {
            return ExcelToHtmlUtils.GetColumnWidthInPx(sheet.GetColumnWidth(columnIndex));
        }
        //private HSSFDataFormatter _formatter = new HSSFDataFormatter();
        private DataFormatter _formatter = new DataFormatter();
        private string cssClassContainerCell = null;

        private string cssClassContainerDiv = null;

        private string cssClassTable;

        private Dictionary<short, string> excelStyleToClass = new Dictionary<short, string>();

        private HtmlDocumentFacade htmlDocumentFacade;

        private bool outputColumnHeaders = true;
        /// <summary>
        /// 是否输出列头
        /// </summary>
        public bool OutputColumnHeaders
        {
            get { return outputColumnHeaders; }
            set { outputColumnHeaders = value; }
        }
        private bool outputHiddenColumns = false;
        /// <summary>
        /// 是否输出隐藏的列
        /// </summary>
        public bool OutputHiddenColumns
        {
            get { return outputHiddenColumns; }
            set { outputHiddenColumns = value; }
        }
        private bool outputHiddenRows = false;
        /// <summary>
        /// 是否输出隐藏的行
        /// </summary>
        public bool OutputHiddenRows
        {
            get { return outputHiddenRows; }
            set { outputHiddenRows = value; }
        }
        private bool outputLeadingSpacesAsNonBreaking = true;
        /// <summary>
        /// 是否输出文本前的空格
        /// </summary>
        public bool OutputLeadingSpacesAsNonBreaking
        {
            get { return outputLeadingSpacesAsNonBreaking; }
            set { outputLeadingSpacesAsNonBreaking = value; }
        }
        private bool outputRowNumbers = true;
        /// <summary>
        /// 是否输出行号
        /// </summary>
        public bool OutputRowNumbers
        {
            get { return outputRowNumbers; }
            set { outputRowNumbers = value; }
        }
        private bool useDivsToSpan = false;

        /// <summary>
        /// 在跨列的单元格使用DIV标记
        /// </summary>
        public bool UseDivsToSpan
        {
            get { return useDivsToSpan; }
            set { useDivsToSpan = value; }
        }
        public static XmlDocument Process(string xlsFile)
        {
            HSSFWorkbook workbook = ExcelToHtmlUtils.LoadXls(xlsFile);
            ExcelToHtmlConverter excelToHtmlConverter = new ExcelToHtmlConverter();
            excelToHtmlConverter.ProcessWorkbook(workbook);
            return excelToHtmlConverter.Document;
        }
        public XmlDocument Document
        {
            get
            {
                return htmlDocumentFacade.Document;
            }
        }
        public void ProcessWorkbook(HSSFWorkbook workbook)
        {
            SummaryInformation summaryInformation = workbook.SummaryInformation;
            if (summaryInformation != null)
            {
                ProcessDocumentInformation(summaryInformation);
            }

            if (UseDivsToSpan)
            {
                // prepare CSS classes for later usage
                this.cssClassContainerCell = htmlDocumentFacade
                        .GetOrCreateCssClass("td", "c",
                                "padding:0;margin:0;align:left;vertical-align:top;");
                this.cssClassContainerDiv = htmlDocumentFacade.GetOrCreateCssClass(
                        "div", "d", "position:relative;");
            }
            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                HSSFSheet sheet = (HSSFSheet)workbook.GetSheetAt(s);
                ProcessSheet(sheet);
            }

            htmlDocumentFacade.UpdateStylesheet();
        }

        protected void ProcessSheet(HSSFSheet sheet)
        {
            ProcessSheetHeader(htmlDocumentFacade.Body, sheet);

            int physicalNumberOfRows = sheet.PhysicalNumberOfRows;
            if (physicalNumberOfRows <= 0)
                return;

            XmlElement table = htmlDocumentFacade.CreateTable();
            table.SetAttribute("class", cssClassTable);

            XmlElement tableBody = htmlDocumentFacade.CreateTableBody();

            CellRangeAddress[][] mergedRanges = ExcelToHtmlUtils.BuildMergedRangesMap(sheet);

            List<XmlElement> emptyRowElements = new List<XmlElement>(physicalNumberOfRows);
            int maxSheetColumns = 1;
            for (int r = 0; r < physicalNumberOfRows; r++)
            {
                HSSFRow row = (HSSFRow)sheet.GetRow(r);

                if (row == null)
                    continue;

                if (!OutputHiddenRows && row.ZeroHeight)
                    continue;

                XmlElement tableRowElement = htmlDocumentFacade.CreateTableRow();
                htmlDocumentFacade.AddStyleClass(tableRowElement, "r", "height:"
                        + (row.Height / 20f) + "pt;");

                int maxRowColumnNumber = ProcessRow(mergedRanges, row,
                        tableRowElement);

                if (maxRowColumnNumber == 0)
                {
                    emptyRowElements.Add(tableRowElement);
                }
                else
                {
                    if (emptyRowElements.Count > 0)
                    {
                        foreach (XmlElement emptyRowElement in emptyRowElements)
                        {
                            tableBody.AppendChild(emptyRowElement);
                        }
                        emptyRowElements.Clear();
                    }

                    tableBody.AppendChild(tableRowElement);
                }
                maxSheetColumns = Math.Max(maxSheetColumns, maxRowColumnNumber);
            }

            ProcessColumnWidths(sheet, maxSheetColumns, table);

            if (OutputColumnHeaders)
            {
                ProcessColumnHeaders(sheet, maxSheetColumns, table);
            }

            table.AppendChild(tableBody);

            htmlDocumentFacade.Body.AppendChild(table);
        }

        protected void ProcessSheetHeader(XmlElement htmlBody, HSSFSheet sheet)
        {
            XmlElement h2 = htmlDocumentFacade.CreateHeader2();
            h2.AppendChild(htmlDocumentFacade.CreateText(sheet.SheetName));
            htmlBody.AppendChild(h2);
        }

        protected void ProcessDocumentInformation(SummaryInformation summaryInformation)
        {
            if (!string.IsNullOrEmpty(summaryInformation.Title))
                htmlDocumentFacade.Title = summaryInformation.Title;

            if (!string.IsNullOrEmpty(summaryInformation.Author))
                htmlDocumentFacade.AddAuthor(summaryInformation.Author);

            if (!string.IsNullOrEmpty(summaryInformation.Keywords))
                htmlDocumentFacade.AddKeywords(summaryInformation.Keywords);

            if (!string.IsNullOrEmpty(summaryInformation.Comments))
                htmlDocumentFacade.AddDescription(summaryInformation.Comments);
        }
        /**
     * @return maximum 1-base index of column that were rendered, zero if none
     */
        protected int ProcessRow(CellRangeAddress[][] mergedRanges, HSSFRow row,
                XmlElement tableRowElement)
        {
            HSSFSheet sheet = (HSSFSheet)row.Sheet;
            int maxColIx = row.LastCellNum;
            if (maxColIx <= 0)
                return 0;

            List<XmlElement> emptyCells = new List<XmlElement>(maxColIx);

            if (OutputRowNumbers)
            {
                XmlElement tableRowNumberCellElement = htmlDocumentFacade.CreateTableHeaderCell();
                ProcessRowNumber(row, tableRowNumberCellElement);
                emptyCells.Add(tableRowNumberCellElement);
            }

            int maxRenderedColumn = 0;
            for (int colIx = 0; colIx < maxColIx; colIx++)
            {
                if (!OutputHiddenColumns && sheet.IsColumnHidden(colIx))
                    continue;

                CellRangeAddress range = ExcelToHtmlUtils.GetMergedRange(
                        mergedRanges, row.RowNum, colIx);

                if (range != null && (range.FirstColumn != colIx || range.FirstRow != row.RowNum))
                    continue;

                HSSFCell cell = (HSSFCell)row.GetCell(colIx);

                int divWidthPx = 0;
                if (UseDivsToSpan)
                {
                    divWidthPx = GetColumnWidth(sheet, colIx);

                    bool hasBreaks = false;
                    for (int nextColumnIndex = colIx + 1; nextColumnIndex < maxColIx; nextColumnIndex++)
                    {
                        if (!OutputHiddenColumns && sheet.IsColumnHidden(nextColumnIndex))
                            continue;

                        if (row.GetCell(nextColumnIndex) != null && !IsTextEmpty((HSSFCell)row.GetCell(nextColumnIndex)))
                        {
                            hasBreaks = true;
                            break;
                        }

                        divWidthPx += GetColumnWidth(sheet, nextColumnIndex);
                    }

                    if (!hasBreaks)
                        divWidthPx = int.MaxValue;
                }

                XmlElement tableCellElement = htmlDocumentFacade.CreateTableCell();

                if (range != null)
                {
                    if (range.FirstColumn != range.LastColumn)
                        tableCellElement.SetAttribute("colspan", (range.LastColumn - range.FirstColumn + 1).ToString());
                    if (range.FirstRow != range.LastRow)
                        tableCellElement.SetAttribute("rowspan", (range.LastRow - range.FirstRow + 1).ToString());
                }

                bool emptyCell;
                if (cell != null)
                {
                    emptyCell = ProcessCell(cell, tableCellElement, GetColumnWidth(sheet, colIx), divWidthPx, row.Height / 20f);
                }
                else
                {
                    emptyCell = true;
                }

                if (emptyCell)
                {
                    emptyCells.Add(tableCellElement);
                }
                else
                {
                    foreach (XmlElement emptyCellElement in emptyCells)
                    {
                        tableRowElement.AppendChild(emptyCellElement);
                    }
                    emptyCells.Clear();

                    tableRowElement.AppendChild(tableCellElement);
                    maxRenderedColumn = colIx;
                }
            }

            return maxRenderedColumn + 1;
        }
        private string GetRowName(HSSFRow row)
        {
            return (row.RowNum + 1).ToString();
        }
        protected void ProcessRowNumber(HSSFRow row, XmlElement tableRowNumberCellElement)
        {
            tableRowNumberCellElement.SetAttribute("class", "rownumber");
            XmlText text = htmlDocumentFacade.CreateText(GetRowName(row));
            tableRowNumberCellElement.AppendChild(text);
        }
        /**
     * Creates COLGROUP element with width specified for all columns. (Except
     * first if <tt>{@link #isOutputRowNumbers()}==true</tt>)
     */
        protected void ProcessColumnWidths(HSSFSheet sheet, int maxSheetColumns,
                XmlElement table)
        {
            // draw COLS after we know max column number
            XmlElement columnGroup = htmlDocumentFacade.CreateTableColumnGroup();
            if (OutputRowNumbers)
            {
                columnGroup.AppendChild(htmlDocumentFacade.CreateTableColumn());
            }
            for (int c = 0; c < maxSheetColumns; c++)
            {
                if (!OutputHiddenColumns && sheet.IsColumnHidden(c))
                    continue;

                XmlElement col = htmlDocumentFacade.CreateTableColumn();
                col.SetAttribute("width", GetColumnWidth(sheet, c).ToString());
                columnGroup.AppendChild(col);
            }
            table.AppendChild(columnGroup);
        }
        protected void ProcessColumnHeaders(HSSFSheet sheet, int maxSheetColumns,
            XmlElement table)
        {
            XmlElement tableHeader = htmlDocumentFacade.CreateTableHeader();
            table.AppendChild(tableHeader);

            XmlElement tr = htmlDocumentFacade.CreateTableRow();

            if (OutputRowNumbers)
            {
                // empty row at left-top corner
                tr.AppendChild(htmlDocumentFacade.CreateTableHeaderCell());
            }

            for (int c = 0; c < maxSheetColumns; c++)
            {
                if (!OutputHiddenColumns && sheet.IsColumnHidden(c))
                    continue;

                XmlElement th = htmlDocumentFacade.CreateTableHeaderCell();
                string text = GetColumnName(c);
                th.AppendChild(htmlDocumentFacade.CreateText(text));
                tr.AppendChild(th);
            }
            tableHeader.AppendChild(tr);
        }
        protected string GetColumnName(int columnIndex)
        {
            return (columnIndex + 1).ToString();
        }
        protected bool IsTextEmpty(HSSFCell cell)
        {
            string value;
            switch (cell.CellType)
            {
                case CellType.String:
                    // XXX: enrich
                    value = cell.RichStringCellValue.String;
                    break;
                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.String:
                            HSSFRichTextString str = cell.RichStringCellValue as HSSFRichTextString;
                            if (str == null || str.Length <= 0)
                                return false;

                            value = str.ToString();
                            break;
                        case CellType.Numeric:
                            HSSFCellStyle style = cell.CellStyle as HSSFCellStyle;
                            if (style == null)
                            {
                                return false;
                            }

                            value = (_formatter.FormatRawCellContents(cell.NumericCellValue, style.DataFormat, style.GetDataFormatString()));
                            break;
                        case CellType.Boolean:
                            value = cell.BooleanCellValue.ToString();
                            break;
                        case CellType.Error:
                            value = ErrorEval.GetText(cell.ErrorCellValue);
                            break;
                        default:
                            value = string.Empty;
                            break;
                    }
                    break;
                case CellType.Blank:
                    value = string.Empty;
                    break;
                case CellType.Numeric:
                    value = _formatter.FormatCellValue(cell);
                    break;
                case CellType.Boolean:
                    value = cell.BooleanCellValue.ToString();
                    break;
                case CellType.Error:
                    value = ErrorEval.GetText(cell.ErrorCellValue);
                    break;
                default:
                    return true;
            }

            return string.IsNullOrEmpty(value);
        }

        protected bool ProcessCell(HSSFCell cell, XmlElement tableCellElement,
                int normalWidthPx, int maxSpannedWidthPx, float normalHeightPt)
        {
            HSSFCellStyle cellStyle = cell.CellStyle as HSSFCellStyle;

            string value;
            switch (cell.CellType)
            {
                case CellType.String:
                    // XXX: enrich
                    value = cell.RichStringCellValue.String;
                    break;
                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.String:
                            HSSFRichTextString str = cell.RichStringCellValue as HSSFRichTextString;
                            if (str != null && str.Length > 0)
                            {
                                value = (str.String);
                            }
                            else
                            {
                                value = string.Empty;
                            }
                            break;
                        case CellType.Numeric:
                            HSSFCellStyle style = cellStyle;
                            if (style == null)
                            {
                                value = cell.NumericCellValue.ToString();
                            }
                            else
                            {
                                value = (_formatter.FormatRawCellContents(cell.NumericCellValue, style.DataFormat, style.GetDataFormatString()));
                            }
                            break;
                        case CellType.Boolean:
                            value = cell.BooleanCellValue.ToString();
                            break;
                        case CellType.Error:
                            value = ErrorEval.GetText(cell.ErrorCellValue);
                            break;
                        default:
                            logger.Log(POILogger.WARN, "Unexpected cell cachedFormulaResultType (" + cell.CachedFormulaResultType.ToString() + ")");
                            value = string.Empty;
                            break;
                    }
                    break;
                case CellType.Blank:
                    value = string.Empty;
                    break;
                case CellType.Numeric:
                    value = _formatter.FormatCellValue(cell);
                    break;
                case CellType.Boolean:
                    value = cell.BooleanCellValue.ToString();
                    break;
                case CellType.Error:
                    value = ErrorEval.GetText(cell.ErrorCellValue);
                    break;
                default:
                    logger.Log(POILogger.WARN, "Unexpected cell type (" + cell.CellType.ToString() + ")");
                    return true;
            }

            bool noText = string.IsNullOrEmpty(value);
            bool wrapInDivs = !noText && UseDivsToSpan && !cellStyle.WrapText;

            short cellStyleIndex = cellStyle.Index;
            if (cellStyleIndex != 0)
            {
                HSSFWorkbook workbook = cell.Row.Sheet.Workbook as HSSFWorkbook;
                string mainCssClass = GetStyleClassName(workbook, cellStyle);
                if (wrapInDivs)
                {
                    tableCellElement.SetAttribute("class", mainCssClass + " "
                            + cssClassContainerCell);
                }
                else
                {
                    tableCellElement.SetAttribute("class", mainCssClass);
                }

                if (noText)
                {
                    /*
                     * if cell style is defined (like borders, etc.) but cell text
                     * is empty, add "&nbsp;" to output, so browser won't collapse
                     * and ignore cell
                     */
                    value = "\u00A0"; //“ ”全角空格
                }
            }

            if (OutputLeadingSpacesAsNonBreaking && value.StartsWith(" "))
            {
                StringBuilder builder = new StringBuilder();
                for (int c = 0; c < value.Length; c++)
                {
                    if (value[c] != ' ')
                        break;
                    builder.Append('\u00a0');
                }

                if (value.Length != builder.Length)
                    builder.Append(value.Substring(builder.Length));

                value = builder.ToString();
            }

            XmlText text = htmlDocumentFacade.CreateText(value);

            if (wrapInDivs)
            {
                XmlElement outerDiv = htmlDocumentFacade.CreateBlock();
                outerDiv.SetAttribute("class", this.cssClassContainerDiv);

                XmlElement innerDiv = htmlDocumentFacade.CreateBlock();
                StringBuilder innerDivStyle = new StringBuilder();
                innerDivStyle.Append("position:absolute;min-width:");
                innerDivStyle.Append(normalWidthPx);
                innerDivStyle.Append("px;");
                if (maxSpannedWidthPx != int.MaxValue)
                {
                    innerDivStyle.Append("max-width:");
                    innerDivStyle.Append(maxSpannedWidthPx);
                    innerDivStyle.Append("px;");
                }
                innerDivStyle.Append("overflow:hidden;max-height:");
                innerDivStyle.Append(normalHeightPt);
                innerDivStyle.Append("pt;white-space:nowrap;");
                ExcelToHtmlUtils.AppendAlign(innerDivStyle, cellStyle.Alignment);
                htmlDocumentFacade.AddStyleClass(outerDiv, "d", innerDivStyle.ToString());

                innerDiv.AppendChild(text);
                outerDiv.AppendChild(innerDiv);
                tableCellElement.AppendChild(outerDiv);
            }
            else
            {
                tableCellElement.AppendChild(text);
            }

            return string.IsNullOrEmpty(value) && cellStyleIndex == 0;
        }

        protected string GetStyleClassName(HSSFWorkbook workbook, HSSFCellStyle cellStyle)
        {
            short cellStyleKey = cellStyle.Index;

            if(excelStyleToClass.ContainsKey(cellStyleKey))
                return excelStyleToClass[cellStyleKey];

            String cssStyle = BuildStyle(workbook, cellStyle);
            String cssClass = htmlDocumentFacade.GetOrCreateCssClass("td", "c",
                    cssStyle);
            excelStyleToClass.Add(cellStyleKey, cssClass);
            return cssClass;
        }

        protected String BuildStyle(HSSFWorkbook workbook, HSSFCellStyle cellStyle)
        {
            StringBuilder style = new StringBuilder();
            HSSFPalette palette = workbook.GetCustomPalette();
            style.Append("white-space: pre-wrap; ");
            ExcelToHtmlUtils.AppendAlign(style, cellStyle.Alignment);

            if (cellStyle.FillPattern == FillPattern.NoFill)
            {
                // no fill
            }
            else if (cellStyle.FillPattern == FillPattern.SolidForeground)
            {
                //cellStyle.
                //HSSFColor.
                HSSFColor foregroundColor = palette.GetColor(cellStyle.FillForegroundColor);
                if (foregroundColor != null)
                    style.Append("background-color: " + ExcelToHtmlUtils.GetColor(foregroundColor) + "; ");
            }
            else
            {
                HSSFColor backgroundColor = palette.GetColor(cellStyle.FillBackgroundColor);
                if (backgroundColor != null)
                    style.Append("background-color: " + ExcelToHtmlUtils.GetColor(backgroundColor) + "; ");
            }

            BuildStyle_Border(workbook, style, "top", cellStyle.BorderTop, cellStyle.TopBorderColor);
            BuildStyle_Border(workbook, style, "right", cellStyle.BorderRight, cellStyle.RightBorderColor);
            BuildStyle_Border(workbook, style, "bottom", cellStyle.BorderBottom, cellStyle.BottomBorderColor);
            BuildStyle_Border(workbook, style, "left", cellStyle.BorderLeft, cellStyle.LeftBorderColor);

            HSSFFont font = cellStyle.GetFont(workbook) as HSSFFont;
            BuildStyle_Font(workbook, style, font);

            return style.ToString();
        }

        private void BuildStyle_Border(HSSFWorkbook workbook, StringBuilder style,
                String type, BorderStyle xlsBorder, short borderColor)
        {
            if (xlsBorder == BorderStyle.None)
                return;

            StringBuilder borderStyle = new StringBuilder();
            borderStyle.Append(ExcelToHtmlUtils.GetBorderWidth(xlsBorder));
            borderStyle.Append(' ');
            borderStyle.Append(ExcelToHtmlUtils.GetBorderStyle(xlsBorder));

            HSSFColor color = workbook.GetCustomPalette().GetColor(borderColor);
            if (color != null)
            {
                borderStyle.Append(' ');
                borderStyle.Append(ExcelToHtmlUtils.GetColor(color));
            }

            style.Append("border-" + type + ": " + borderStyle + "; ");
        }

        void BuildStyle_Font(HSSFWorkbook workbook, StringBuilder style,
                HSSFFont font)
        {
            switch (font.Boldweight)
            {
                case (short)FontBoldWeight.Bold:
                    style.Append("font-weight: bold; ");
                    break;
                case (short)FontBoldWeight.Normal:
                    // by default, not not increase HTML size
                    // style.Append( "font-weight: normal; " );
                    break;
            }

            HSSFColor fontColor = workbook.GetCustomPalette().GetColor(font.Color);
            if (fontColor != null)
                style.Append("color: " + ExcelToHtmlUtils.GetColor(fontColor) + "; ");

            if (font.FontHeightInPoints != 0)
                style.Append("font-size: " + font.FontHeightInPoints + "pt; ");

            if (font.IsItalic)
            {
                style.Append("font-style: italic; ");
            }
        }
    }
}
