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

namespace TestCases.HSSF.UserModel
{
    using System;
    using NPOI.HSSF.Model;
    using NPOI.HSSF.Util;
    using NPOI.HSSF.UserModel;

    using NUnit.Framework;
    using TestCases.HSSF;
    using NPOI.SS.UserModel;
    using NPOI.HSSF.Record;
    using System.Text;
    using TestCases.SS.UserModel;

    /**
     * Tests various functionity having to do with Cell.  For instance support for
     * paticular datatypes, etc.
     * @author Andrew C. Oliver (andy at superlinksoftware dot com)
     * @author  Dan Sherman (dsherman at isisph.com)
     * @author Alex Jacoby (ajacoby at gmail.com)
     */
    [TestFixture]
    public class TestHSSFCell : BaseTestCell
    {
        public TestHSSFCell()
            : base(HSSFITestDataProvider.Instance)
        {
            // TestSetTypeStringOnFormulaCell and TestToString are depending on the american culture.
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
        }

        private static HSSFWorkbook OpenSample(String sampleFileName)
        {
            return HSSFTestDataSamples.OpenSampleWorkbook(sampleFileName);
        }
        private static HSSFWorkbook WriteOutAndReadBack(HSSFWorkbook original)
        {
            return HSSFTestDataSamples.WriteOutAndReadBack(original);
        }

        /**
         * Test that Boolean and Error types (BoolErrRecord) are supported properly.
         */
        //[Test]
        //public void TestBoolErr()
        //{

        //    HSSFWorkbook wb = new HSSFWorkbook();
        //    NPOI.SS.UserModel.Sheet s = wb.CreateSheet("TestSheet1");
        //    Row r = null;
        //    Cell c = null;
        //    r = s.CreateRow(0);
        //    c = r.CreateCell(1);
        //    //c.SetCellType(NPOI.SS.UserModel.CellType.BOOLEAN);
        //    c.SetCellValue(true);

        //    c = r.CreateCell(2);
        //    //c.SetCellType(NPOI.SS.UserModel.CellType.BOOLEAN);
        //    c.SetCellValue(false);

        //    r = s.CreateRow(1);
        //    c = r.CreateCell(1);
        //    //c.SetCellType(NPOI.SS.UserModel.CellType.ERROR);
        //    c.SetCellErrorValue((byte)0);

        //    c = r.CreateCell(2);
        //    //c.SetCellType(NPOI.SS.UserModel.CellType.ERROR);
        //    c.SetCellErrorValue((byte)7);

        //    wb = WriteOutAndReadBack(wb);
        //    s = wb.GetSheetAt(0);
        //    r = s.GetRow(0);
        //    c = r.GetCell(1);
        //    Assert.IsTrue(c.BooleanCellValue, "boolean value 0,1 = true");
        //    c = r.GetCell(2);
        //    Assert.IsTrue(c.BooleanCellValue == false, "boolean value 0,2 = false");
        //    r = s.GetRow(1);
        //    c = r.GetCell(1);
        //    Assert.IsTrue(c.ErrorCellValue == 0, "boolean value 0,1 = 0");
        //    c = r.GetCell(2);
        //    Assert.IsTrue(c.ErrorCellValue == 7, "boolean value 0,2 = 7");
        //}

        /**
         * Checks that the recognition of files using 1904 date windowing
         *  is working properly. Conversion of the date is also an issue,
         *  but there's a separate unit Test for that.
         */
        [Test]
        public void TestDateWindowingRead()
        {
            DateTime date = new DateTime(2000, 1, 1);

            // first Check a file with 1900 Date Windowing
            HSSFWorkbook workbook = OpenSample("1900DateWindowing.xls");
            NPOI.SS.UserModel.ISheet sheet = workbook.GetSheetAt(0);

            Assert.AreEqual(date, sheet.GetRow(0).GetCell(0).DateCellValue,
                               "Date from file using 1900 Date Windowing");

            // now Check a file with 1904 Date Windowing
            workbook = OpenSample("1904DateWindowing.xls");
            sheet = workbook.GetSheetAt(0);

            Assert.AreEqual(date, sheet.GetRow(0).GetCell(0).DateCellValue,
                             "Date from file using 1904 Date Windowing");
        }

        /**
         * Checks that dates are properly written to both types of files:
         * those with 1900 and 1904 date windowing.  Note that if the
         * previous Test ({@link #TestDateWindowingRead}) Assert.Fails, the
         * results of this Test are meaningless.
         */
        [Test]
        public void TestDateWindowingWrite()
        {
            DateTime date = new DateTime(2000, 1, 1);

            // first Check a file with 1900 Date Windowing
            HSSFWorkbook wb;
            wb = OpenSample("1900DateWindowing.xls");

            SetCell(wb, 0, 1, date);
            wb = WriteOutAndReadBack(wb);

            Assert.AreEqual(date,
                            ReadCell(wb, 0, 1), "Date from file using 1900 Date Windowing");

            // now Check a file with 1904 Date Windowing
            wb = OpenSample("1904DateWindowing.xls");
            SetCell(wb, 0, 1, date);
            wb = WriteOutAndReadBack(wb);
            Assert.AreEqual(date,
                            ReadCell(wb, 0, 1), "Date from file using 1900 Date Windowing");
        }

        /**
 * Test for small bug observable around r736460 (prior to version 3.5).  POI fails to remove
 * the {@link StringRecord} following the {@link FormulaRecord} after the result type had been 
 * changed to number/boolean/error.  Excel silently ignores the extra record, but some POI
 * versions (prior to bug 46213 / r717883) crash instead.
 */
        [Test]
        public void TestCachedTypeChange()
        {
            HSSFSheet sheet = (HSSFSheet)new HSSFWorkbook().CreateSheet("Sheet1");
            HSSFCell cell = (HSSFCell)sheet.CreateRow(0).CreateCell(0);
            cell.CellFormula = ("A1");
            cell.SetCellValue("abc");
            ConfirmStringRecord(sheet, true);
            cell.SetCellValue(123);
            NPOI.HSSF.Record.Record[] recs = RecordInspector.GetRecords(sheet, 0);
            if (recs.Length == 28 && recs[23] is StringRecord)
            {
                throw new AssertionException("Identified bug - leftover StringRecord");
            }
            ConfirmStringRecord(sheet, false);

            // string to error code
            cell.SetCellValue("abc");
            ConfirmStringRecord(sheet, true);
            cell.SetCellErrorValue((byte)ErrorConstants.ERROR_REF);
            ConfirmStringRecord(sheet, false);

            // string to boolean
            cell.SetCellValue("abc");
            ConfirmStringRecord(sheet, true);
            cell.SetCellValue(false);
            ConfirmStringRecord(sheet, false);
        }

        private static void ConfirmStringRecord(HSSFSheet sheet, bool isPresent)
        {
            Record[] recs = RecordInspector.GetRecords(sheet, 0);
            Assert.AreEqual(isPresent ? 29 : 28, recs.Length); //for SheetExtRecord
            //Assert.AreEqual(isPresent ? 28 : 27, recs.Length);
            int index = 22;
            Record fr = recs[index++];
            Assert.AreEqual(typeof(FormulaRecord), fr.GetType());
            if (isPresent)
            {
                Assert.AreEqual(typeof(StringRecord), recs[index++].GetType());
            }
            else
            {
                Assert.IsFalse(typeof(StringRecord) == recs[index].GetType());
            }
            Record dbcr = recs[index++];
            Assert.AreEqual(typeof(DBCellRecord), dbcr.GetType());
        }

        /**
         *  The maximum length of cell contents (text) is 32,767 characters.
         */
        [Test]
        public void TestMaxTextLength()
        {
            HSSFSheet sheet = (HSSFSheet)new HSSFWorkbook().CreateSheet();
            HSSFCell cell = (HSSFCell)sheet.CreateRow(0).CreateCell(0);

            int maxlen = NPOI.SS.SpreadsheetVersion.EXCEL97.MaxTextLength;
            Assert.AreEqual(32767, maxlen);

            StringBuilder b = new StringBuilder();

            // 32767 is okay
            for (int i = 0; i < maxlen; i++)
            {
                b.Append("X");
            }
            cell.SetCellValue(b.ToString());

            b.Append("X");
            // 32768 produces an invalid XLS file
            try
            {
                cell.SetCellValue(b.ToString());
                Assert.Fail("Expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("The maximum length of cell contents (text) is 32,767 characters", e.Message);
            }
        }

        private static void SetCell(HSSFWorkbook workbook, int rowIdx, int colIdx, DateTime date)
        {
            NPOI.SS.UserModel.ISheet sheet = workbook.GetSheetAt(0);
            IRow row = sheet.GetRow(rowIdx);
            ICell cell = row.GetCell(colIdx);

            if (cell == null)
            {
                cell = row.CreateCell(colIdx);
            }
            cell.SetCellValue(date);
        }

        private static DateTime ReadCell(HSSFWorkbook workbook, int rowIdx, int colIdx)
        {
            NPOI.SS.UserModel.ISheet sheet = workbook.GetSheetAt(0);
            IRow row = sheet.GetRow(rowIdx);
            ICell cell = row.GetCell(colIdx);
            return cell.DateCellValue;
        }

        /**
         * Tests that the active cell can be correctly read and set
         */
        [Test]
        public void TestActiveCell()
        {
            //read in sample
            HSSFWorkbook book = OpenSample("Simple.xls");

            //Check initial position
            HSSFSheet umSheet = (HSSFSheet)book.GetSheetAt(0);
            InternalSheet s = umSheet.Sheet;
            Assert.AreEqual(0, s.ActiveCellCol, "Initial active cell should be in col 0");
            Assert.AreEqual(1, s.ActiveCellRow, "Initial active cell should be on row 1");

            //modify position through Cell
            ICell cell = umSheet.CreateRow(3).CreateCell(2);
            cell.SetAsActiveCell();
            Assert.AreEqual(2, s.ActiveCellCol, "After modify, active cell should be in col 2");
            Assert.AreEqual(3, s.ActiveCellRow, "After modify, active cell should be on row 3");

            //Write book to temp file; read and Verify that position is serialized
            book = WriteOutAndReadBack(book);

            umSheet = (HSSFSheet)book.GetSheetAt(0);
            s = umSheet.Sheet;

            Assert.AreEqual(2, s.ActiveCellCol, "After serialize, active cell should be in col 2");
            Assert.AreEqual(3, s.ActiveCellRow, "After serialize, active cell should be on row 3");
        }


        /**
         * Test reading hyperlinks
         */
        [Test]
        public void TestWithHyperlink()
        {

            HSSFWorkbook wb = OpenSample("WithHyperlink.xls");

            NPOI.SS.UserModel.ISheet sheet = wb.GetSheetAt(0);
            ICell cell = sheet.GetRow(4).GetCell(0);
            IHyperlink link = cell.Hyperlink;
            Assert.IsNotNull(link);

            Assert.AreEqual("Foo", link.Label);
            Assert.AreEqual(link.Address, "http://poi.apache.org/");
            Assert.AreEqual(4, link.FirstRow);
            Assert.AreEqual(0, link.FirstColumn);
        }

        /**
         * Test reading hyperlinks
         */
        [Test]
        public void TestWithTwoHyperlinks()
        {

            HSSFWorkbook wb = OpenSample("WithTwoHyperLinks.xls");

            NPOI.SS.UserModel.ISheet sheet = wb.GetSheetAt(0);

            ICell cell1 = sheet.GetRow(4).GetCell(0);
            IHyperlink link1 = cell1.Hyperlink;
            Assert.IsNotNull(link1);
            Assert.AreEqual("Foo", link1.Label);
            Assert.AreEqual("http://poi.apache.org/", link1.Address);
            Assert.AreEqual(4, link1.FirstRow);
            Assert.AreEqual(0, link1.FirstColumn);

            ICell cell2 = sheet.GetRow(8).GetCell(1);
            IHyperlink link2 = cell2.Hyperlink;
            Assert.IsNotNull(link2);
            Assert.AreEqual("Bar", link2.Label);
            Assert.AreEqual("http://poi.apache.org/hssf/", link2.Address);
            Assert.AreEqual(8, link2.FirstRow);
            Assert.AreEqual(1, link2.FirstColumn);
        }


        [Test]
        public void TestHSSFCellToStringWithDataFormat()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            ICell cell = wb.CreateSheet("Sheet1").CreateRow(0).CreateCell(0);
            cell.SetCellValue(new DateTime(2009, 8, 20));
            NPOI.SS.UserModel.ICellStyle cellStyle = wb.CreateCellStyle();
            cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");
            cell.CellStyle = cellStyle;
            Assert.AreEqual("8/20/09", cell.ToString());

            NPOI.SS.UserModel.ICellStyle cellStyle2 = wb.CreateCellStyle();
            IDataFormat format = wb.CreateDataFormat();
            cellStyle2.DataFormat = format.GetFormat("YYYY-mm/dd");
            cell.CellStyle = cellStyle2;
            Assert.AreEqual("2009-08/20", cell.ToString());
        }
        [Test]
        public void TestGetDataFormatUniqueIndex()
        {
            HSSFWorkbook wb = new HSSFWorkbook();

            IDataFormat format = wb.CreateDataFormat();
            short formatidx1 = format.GetFormat("YYYY-mm/dd");
            short formatidx2 = format.GetFormat("YYYY-mm/dd");
            Assert.AreEqual(formatidx1, formatidx2);
            short formatidx3 = format.GetFormat("000000.000");
            Assert.AreNotEqual(formatidx1, formatidx3);
        }
        /**
         * Test to ensure we can only assign cell styles that belong
         *  to our workbook, and not those from other workbooks.
         */
        [Test]
        public void TestCellStyleWorkbookMatch()
        {
            HSSFWorkbook wbA = new HSSFWorkbook();
            HSSFWorkbook wbB = new HSSFWorkbook();

            HSSFCellStyle styA = (HSSFCellStyle)wbA.CreateCellStyle();
            HSSFCellStyle styB = (HSSFCellStyle)wbB.CreateCellStyle();

            styA.VerifyBelongsToWorkbook(wbA);
            styB.VerifyBelongsToWorkbook(wbB);
            try
            {
                styA.VerifyBelongsToWorkbook(wbB);
                Assert.Fail();
            }
            catch (ArgumentException) { }
            try
            {
                styB.VerifyBelongsToWorkbook(wbA);
                Assert.Fail();
            }
            catch (ArgumentException) { }

            ICell cellA = wbA.CreateSheet().CreateRow(0).CreateCell(0);
            ICell cellB = wbB.CreateSheet().CreateRow(0).CreateCell(0);

            cellA.CellStyle = (styA);
            cellB.CellStyle = (styB);
            try
            {
                cellA.CellStyle = (styB);
                Assert.Fail();
            }
            catch (ArgumentException) { }
            try
            {
                cellB.CellStyle = (styA);
                Assert.Fail();
            }
            catch (ArgumentException) { }
        }
        /**
  * HSSF prior to version 3.7 had a bug: it could write a NaN but could not read such a file back.
  */
        [Test]
        public void TestReadNaN()
        {
            HSSFWorkbook wb = HSSFTestDataSamples.OpenSampleWorkbook("49761.xls");
        }

    }

}