/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
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

namespace TestCases.SS.UserModel
{
    using System;

    using NUnit.Framework;
    using NPOI.SS;
    using NPOI.SS.Util;
    using NPOI.SS.UserModel;
    using System.Collections;
    using System.IO;
    using NPOI.HSSF.UserModel;



    /**
     * Common superclass for Testing automatic sizing of sheet columns
     *
     * @author Yegor Kozlov
     */
    [TestFixture]
    public class BaseTestSheetAutosizeColumn
    {

        private ITestDataProvider _testDataProvider;
        public BaseTestSheetAutosizeColumn()
        {
            _testDataProvider = TestCases.HSSF.HSSFITestDataProvider.Instance;
        }
        protected BaseTestSheetAutosizeColumn(ITestDataProvider TestDataProvider)
        {
            _testDataProvider = TestDataProvider;
        }

        // TODO should we have this stuff in the FormulaEvaluator?
        private void EvaluateWorkbook(IWorkbook workbook){
        IFormulaEvaluator eval = workbook.GetCreationHelper().CreateFormulaEvaluator();
        for(int i=0; i < workbook.NumberOfSheets; i++) {
            ISheet sheet = workbook.GetSheetAt(i);
            IEnumerator rows = sheet.GetRowEnumerator();
            for (IRow r = null; rows.MoveNext(); )
            {
                r = (IRow)rows.Current;
                foreach (ICell c in r)
                {
                    if (c.CellType == CellType.Formula)
                    {
                        eval.EvaluateFormulaCell(c);
                    }
                }
            }
        }
    }
        [Test]
        public void TestNumericCells()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            IDataFormat df = workbook.GetCreationHelper().CreateDataFormat();
            ISheet sheet = workbook.CreateSheet();

            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(0); // GetCachedFormulaResult() returns 0 for not Evaluated formula cells
            row.CreateCell(1).SetCellValue(10);
            row.CreateCell(2).SetCellValue("10");
            row.CreateCell(3).CellFormula = (/*setter*/"(A1+B1)*1.0"); // a formula that returns '10'

            ICell cell4 = row.CreateCell(4);       // numeric cell with a custom style
            ICellStyle style4 = workbook.CreateCellStyle();
            style4.DataFormat = (/*setter*/df.GetFormat("0.0000"));
            cell4.CellStyle = (/*setter*/style4);
            cell4.SetCellValue(10); // formatted as '10.0000'

            row.CreateCell(5).SetCellValue("10.0000");

            // autosize not-Evaluated cells, formula cells are sized as if the result is 0
            for (int i = 0; i < 6; i++) 
                sheet.AutoSizeColumn(i);

            Assert.IsTrue(sheet.GetColumnWidth(0) < sheet.GetColumnWidth(1));  // width of '0' is less then width of '10'
            Assert.AreEqual(sheet.GetColumnWidth(1), sheet.GetColumnWidth(2)); // 10 and '10' should be sized Equally
            Assert.AreEqual(sheet.GetColumnWidth(3), sheet.GetColumnWidth(0)); // formula result is unknown, the width is calculated  for '0'
            Assert.AreEqual(sheet.GetColumnWidth(4), sheet.GetColumnWidth(5)); // 10.0000 and '10.0000'

            // Evaluate formulas and re-autosize
            EvaluateWorkbook(workbook);

            for (int i = 0; i < 6; i++) sheet.AutoSizeColumn(i);

            Assert.IsTrue(sheet.GetColumnWidth(0) < sheet.GetColumnWidth(1));  // width of '0' is less then width of '10'
            Assert.AreEqual(sheet.GetColumnWidth(1), sheet.GetColumnWidth(2)); // columns 1, 2 and 3 should have the same width
            Assert.AreEqual(sheet.GetColumnWidth(2), sheet.GetColumnWidth(3)); // columns 1, 2 and 3 should have the same width
            Assert.AreEqual(sheet.GetColumnWidth(4), sheet.GetColumnWidth(5)); // 10.0000 and '10.0000'
        }
        [Test]
        public void TestBooleanCells()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();

            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(0); // GetCachedFormulaResult() returns 0 for not Evaluated formula cells
            row.CreateCell(1).SetCellValue(true);
            row.CreateCell(2).SetCellValue("TRUE");
            row.CreateCell(3).CellFormula = (/*setter*/"1 > 0"); // a formula that returns true

            // autosize not-Evaluated cells, formula cells are sized as if the result is 0
            for (int i = 0; i < 4; i++) sheet.AutoSizeColumn(i);

            Assert.IsTrue(sheet.GetColumnWidth(1) > sheet.GetColumnWidth(0));  // 'true' is wider than '0'
            Assert.AreEqual(sheet.GetColumnWidth(1), sheet.GetColumnWidth(2));  // 10 and '10' should be sized Equally
            Assert.AreEqual(sheet.GetColumnWidth(3), sheet.GetColumnWidth(0));  // formula result is unknown, the width is calculated  for '0'

            // Evaluate formulas and re-autosize
            EvaluateWorkbook(workbook);

            for (int i = 0; i < 4; i++) sheet.AutoSizeColumn(i);

            Assert.IsTrue(sheet.GetColumnWidth(1) > sheet.GetColumnWidth(0));  // 'true' is wider than '0'
            Assert.AreEqual(sheet.GetColumnWidth(1), sheet.GetColumnWidth(2));  // columns 1, 2 and 3 should have the same width
            Assert.AreEqual(sheet.GetColumnWidth(2), sheet.GetColumnWidth(3));  // columns 1, 2 and 3 should have the same width
        }
        [Test]
        public void TestDateCells()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();
            IDataFormat df = workbook.GetCreationHelper().CreateDataFormat();

            ICellStyle style1 = workbook.CreateCellStyle();
            style1.DataFormat = (/*setter*/df.GetFormat("m"));

            ICellStyle style3 = workbook.CreateCellStyle();
            style3.DataFormat = (/*setter*/df.GetFormat("mmm"));

            ICellStyle style5 = workbook.CreateCellStyle(); //rotated text
            style5.DataFormat = (/*setter*/df.GetFormat("mmm/dd/yyyy"));

            //Calendar calendar = Calendar.Instance;
            //calendar.Set(2010, 0, 1); // Jan 1 2010
            DateTime calendar = new DateTime(2010, 1, 1);
            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(DateUtil.GetJavaDate(0));   //default date

            ICell cell1 = row.CreateCell(1);
            cell1.SetCellValue(calendar);
            cell1.CellStyle = (/*setter*/style1);
            row.CreateCell(2).SetCellValue("1"); // column 1 should be sized as '1'

            ICell cell3 = row.CreateCell(3);
            cell3.SetCellValue(calendar);
            cell3.CellStyle = (/*setter*/style3);
            row.CreateCell(4).SetCellValue("Jan");

            ICell cell5 = row.CreateCell(5);
            cell5.SetCellValue(calendar);
            cell5.CellStyle = (/*setter*/style5);
            row.CreateCell(6).SetCellValue("Jan/01/2010");

            ICell cell7 = row.CreateCell(7);
            cell7.CellFormula = (/*setter*/"DATE(2010,1,1)");
            cell7.CellStyle = (/*setter*/style3); // should be sized as 'Jan'

            // autosize not-Evaluated cells, formula cells are sized as if the result is 0
            for (int i = 0; i < 8; i++) 
                sheet.AutoSizeColumn(i);
            Assert.AreEqual(sheet.GetColumnWidth(2), sheet.GetColumnWidth(1)); // date formatted as 'm'
            Assert.IsTrue(sheet.GetColumnWidth(3) > sheet.GetColumnWidth(1));  // 'mmm' is wider than 'm'
            Assert.AreEqual(sheet.GetColumnWidth(4), sheet.GetColumnWidth(3)); // date formatted as 'mmm'
            Assert.IsTrue(sheet.GetColumnWidth(5) > sheet.GetColumnWidth(3));  // 'mmm/dd/yyyy' is wider than 'mmm'
            Assert.AreEqual(sheet.GetColumnWidth(6), sheet.GetColumnWidth(5)); // date formatted as 'mmm/dd/yyyy'

            // YK: width of not-Evaluated formulas that return data is not determined
            // POI seems to conevert '0' to Excel date which is the beginng of the Excel's date system

            // Evaluate formulas and re-autosize
            EvaluateWorkbook(workbook);

            for (int i = 0; i < 8; i++) sheet.AutoSizeColumn(i);

            Assert.AreEqual(sheet.GetColumnWidth(2), sheet.GetColumnWidth(1)); // date formatted as 'm'
            Assert.IsTrue(sheet.GetColumnWidth(3) > sheet.GetColumnWidth(1));  // 'mmm' is wider than 'm'
            Assert.AreEqual(sheet.GetColumnWidth(4), sheet.GetColumnWidth(3)); // date formatted as 'mmm'
            Assert.IsTrue(sheet.GetColumnWidth(5) > sheet.GetColumnWidth(3));  // 'mmm/dd/yyyy' is wider than 'mmm'
            Assert.AreEqual(sheet.GetColumnWidth(6), sheet.GetColumnWidth(5)); // date formatted as 'mmm/dd/yyyy'
            Assert.AreEqual(sheet.GetColumnWidth(4), sheet.GetColumnWidth(7)); // date formula formatted as 'mmm'
        }
        [Test]
        public void TestStringCells()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();
            IRow row = sheet.CreateRow(0);

            IFont defaultFont = workbook.GetFontAt((short)0);

            ICellStyle style1 = workbook.CreateCellStyle();
            IFont font1 = workbook.CreateFont();
            font1.FontHeight = (/*setter*/(short)(2 * defaultFont.FontHeight));
            style1.SetFont(font1);

            row.CreateCell(0).SetCellValue("x");
            row.CreateCell(1).SetCellValue("xxxx");
            row.CreateCell(2).SetCellValue("xxxxxxxxxxxx");
            row.CreateCell(3).SetCellValue("Apache\nSoftware Foundation"); // the text is splitted into two lines
            row.CreateCell(4).SetCellValue("Software Foundation");

            ICell cell5 = row.CreateCell(5);
            cell5.SetCellValue("Software Foundation");
            cell5.CellStyle = (/*setter*/style1); // same as in column 4 but the font is twice larger than the default font

            for (int i = 0; i < 10; i++) sheet.AutoSizeColumn(i);

            Assert.IsTrue(2 * sheet.GetColumnWidth(0) < sheet.GetColumnWidth(1)); // width is roughly proportional to the number of characters
            Assert.IsTrue(2 * sheet.GetColumnWidth(1) < sheet.GetColumnWidth(2));
            Assert.AreEqual(sheet.GetColumnWidth(4), sheet.GetColumnWidth(3));
            Assert.IsTrue(sheet.GetColumnWidth(5) > sheet.GetColumnWidth(4)); //larger font results in a wider column width
        }
        [Test]
        public void TestRotatedText()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();
            IRow row = sheet.CreateRow(0);

            ICellStyle style1 = workbook.CreateCellStyle();
            style1.Rotation = (/*setter*/(short)90);

            ICell cell0 = row.CreateCell(0);
            cell0.SetCellValue("Apache Software Foundation");
            cell0.CellStyle = (/*setter*/style1);

            ICell cell1 = row.CreateCell(1);
            cell1.SetCellValue("Apache Software Foundation");

            for (int i = 0; i < 2; i++) sheet.AutoSizeColumn(i);

            int w0 = sheet.GetColumnWidth(0);
            int w1 = sheet.GetColumnWidth(1);

            Assert.IsTrue(w0 * 5 < w1); // rotated text occupies at least five times less horizontal space than normal text
        }
        [Test]
        public void TestMergedCells()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();

            IRow row = sheet.CreateRow(0);
            sheet.AddMergedRegion(CellRangeAddress.ValueOf("A1:B1"));

            ICell cell0 = row.CreateCell(0);
            cell0.SetCellValue("Apache Software Foundation");

            int defaulWidth = sheet.GetColumnWidth(0);
            sheet.AutoSizeColumn(0);
            // column is unChanged if merged regions are ignored (Excel like behavior)
            Assert.AreEqual(defaulWidth, sheet.GetColumnWidth(0));

            sheet.AutoSizeColumn(0, true);
            Assert.IsTrue(sheet.GetColumnWidth(0) > defaulWidth);
        }


        /**
         * Auto-Sizing a column needs to work when we have rows
         *  passed the 32767 boundary. See bug #48079
         */
        [Test]
        public void TestLargeRowNumbers()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();

            IRow r0 = sheet.CreateRow(0);
            r0.CreateCell(0).SetCellValue("I am ROW 0");
            IRow r200 = sheet.CreateRow(200);
            r200.CreateCell(0).SetCellValue("I am ROW 200");

            // This should work fine
            sheet.AutoSizeColumn(0);

            // Get close to 32767
            IRow r32765 = sheet.CreateRow(32765);
            r32765.CreateCell(0).SetCellValue("Nearly there...");
            sheet.AutoSizeColumn(0);

            // To it
            IRow r32767 = sheet.CreateRow(32767);
            r32767.CreateCell(0).SetCellValue("At the boundary");
            sheet.AutoSizeColumn(0);

            // And passed it
            IRow r32768 = sheet.CreateRow(32768);
            r32768.CreateCell(0).SetCellValue("Passed");
            IRow r32769 = sheet.CreateRow(32769);
            r32769.CreateCell(0).SetCellValue("More Passed");
            sheet.AutoSizeColumn(0);

            // Long way passed
            IRow r60708 = sheet.CreateRow(60708);
            r60708.CreateCell(0).SetCellValue("Near the end");
            sheet.AutoSizeColumn(0);
        }
    }
}