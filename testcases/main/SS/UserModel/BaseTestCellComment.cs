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
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;
    using NPOI.Util;
    using NUnit.Framework;

    /**
     * Common superclass for testing implementatiosn of
     * {@link Comment}
     */
    public class BaseTestCellComment
    {

        private ITestDataProvider _testDataProvider;

        public BaseTestCellComment()
            : this(TestCases.HSSF.HSSFITestDataProvider.Instance)
        {}
        protected BaseTestCellComment(ITestDataProvider testDataProvider) {
            _testDataProvider = testDataProvider;
        }
        [Test]
        public void Find()
        {
            IWorkbook book = _testDataProvider.CreateWorkbook();
            ISheet sheet = book.CreateSheet();
            Assert.IsNull(sheet.GetCellComment(new CellAddress(0, 0)));

            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            Assert.IsNull(sheet.GetCellComment(new CellAddress(0, 0)));
            Assert.IsNull(cell.CellComment);

            book.Close();
        }
        [Test]
        public void Create()
        {
            String cellText = "Hello, World";
            String commentText = "We can set comments in POI";
            String commentAuthor = "Apache Software Foundation";
            int cellRow = 3;
            int cellColumn = 1;

            IWorkbook wb1 = _testDataProvider.CreateWorkbook();
            ICreationHelper factory = wb1.GetCreationHelper();

            ISheet sheet = wb1.CreateSheet();
            Assert.IsNull(sheet.GetCellComment(new CellAddress(cellRow, cellColumn)));

            ICell cell = sheet.CreateRow(cellRow).CreateCell(cellColumn);
            cell.SetCellValue(factory.CreateRichTextString(cellText));
            Assert.IsNull(cell.CellComment);
            Assert.IsNull(sheet.GetCellComment(new CellAddress(cellRow, cellColumn)));

            IDrawing patr = sheet.CreateDrawingPatriarch();
            IClientAnchor anchor = factory.CreateClientAnchor();
            anchor.Col1=(2);
            anchor.Col2=(5);
            anchor.Row1=(1);
            anchor.Row2=(2);
            IComment comment = patr.CreateCellComment(anchor);
            Assert.IsFalse(comment.Visible);
            comment.Visible = (true);
            Assert.IsTrue(comment.Visible);
            IRichTextString string1 = factory.CreateRichTextString(commentText);
            comment.String=(string1);
            comment.Author=(commentAuthor);
            cell.CellComment=(comment);
            Assert.IsNotNull(cell.CellComment);
            Assert.IsNotNull(sheet.GetCellComment(new CellAddress(cellRow, cellColumn)));

            //verify our Settings
            Assert.AreEqual(commentAuthor, comment.Author);
            Assert.AreEqual(commentText, comment.String.String);
            Assert.AreEqual(cellRow, comment.Row);
            Assert.AreEqual(cellColumn, comment.Column);

            IWorkbook wb2 = _testDataProvider.WriteOutAndReadBack(wb1);
            wb1.Close();

            sheet = wb2.GetSheetAt(0);
            cell = sheet.GetRow(cellRow).GetCell(cellColumn);
            comment = cell.CellComment;

            Assert.IsNotNull(comment);
            Assert.AreEqual(commentAuthor, comment.Author);
            Assert.AreEqual(commentText, comment.String.String);
            Assert.AreEqual(cellRow, comment.Row);
            Assert.AreEqual(cellColumn, comment.Column);
            Assert.IsTrue(comment.Visible);

            // Change slightly, and re-test
            comment.String = (factory.CreateRichTextString("New Comment Text"));
            comment.Visible = (false);

            IWorkbook wb3 = _testDataProvider.WriteOutAndReadBack(wb2);
            wb2.Close();

            sheet = wb3.GetSheetAt(0);
            cell = sheet.GetRow(cellRow).GetCell(cellColumn);
            comment = cell.CellComment;

            Assert.IsNotNull(comment);
            Assert.AreEqual(commentAuthor, comment.Author);
            Assert.AreEqual("New Comment Text", comment.String.String);
            Assert.AreEqual(cellRow, comment.Row);
            Assert.AreEqual(cellColumn, comment.Column);
            Assert.IsFalse(comment.Visible);

            // Test Comment.equals and Comment.hashCode
            Assert.AreEqual(comment, cell.CellComment);
            Assert.AreEqual(comment.GetHashCode(), cell.CellComment.GetHashCode());

            wb3.Close();
        }

        /**
         * test that we can read cell comments from an existing workbook.
         */
        [Test]
        public void ReadComments()
        {
            IWorkbook wb = _testDataProvider.OpenSampleWorkbook("SimpleWithComments." + _testDataProvider.StandardFileNameExtension);

            ISheet sheet = wb.GetSheetAt(0);

            ICell cell;
            IRow row;
            IComment comment;

            for (int rownum = 0; rownum < 3; rownum++)
            {
                row = sheet.GetRow(rownum);
                cell = row.GetCell(0);
                comment = cell.CellComment;
                Assert.IsNull(comment, "Cells in the first column are not commented");
                Assert.IsNull(sheet.GetCellComment(new CellAddress(rownum, 0)));
            }

            for (int rownum = 0; rownum < 3; rownum++)
            {
                row = sheet.GetRow(rownum);
                cell = row.GetCell(1);
                comment = cell.CellComment;
                Assert.IsNotNull(comment, "Cells in the second column have comments");
                Assert.IsNotNull(sheet.GetCellComment(new CellAddress(rownum, 1)), "Cells in the second column have comments");

                Assert.AreEqual("Yegor Kozlov", comment.Author);
                Assert.IsFalse(comment.String.String == string.Empty, "cells in the second column have not empyy notes");
                Assert.AreEqual(rownum, comment.Row);
                Assert.AreEqual(cell.ColumnIndex, comment.Column);
            }

            wb.Close();
        }

        /**
         * test that we can modify existing cell comments
         */
        [Test]
        public void ModifyComments()
        {

            IWorkbook wb1 = _testDataProvider.OpenSampleWorkbook("SimpleWithComments." + _testDataProvider.StandardFileNameExtension);
            ICreationHelper factory = wb1.GetCreationHelper();

            ISheet sheet = wb1.GetSheetAt(0);

            ICell cell;
            IRow row;
            IComment comment;

            for (int rownum = 0; rownum < 3; rownum++)
            {
                row = sheet.GetRow(rownum);
                cell = row.GetCell(1);
                comment = cell.CellComment;
                comment.Author = ("Mofified[" + rownum + "] by Yegor");
                comment.String = (factory.CreateRichTextString("Modified comment at row " + rownum));
            }

            IWorkbook wb2 = _testDataProvider.WriteOutAndReadBack(wb1);
            wb1.Close();

            sheet = wb2.GetSheetAt(0);

            for (int rownum = 0; rownum < 3; rownum++)
            {
                row = sheet.GetRow(rownum);
                cell = row.GetCell(1);
                comment = cell.CellComment;

                Assert.AreEqual("Mofified[" + rownum + "] by Yegor", comment.Author);
                Assert.AreEqual("Modified comment at row " + rownum, comment.String.String);
            }

            wb2.Close();
        }
        [Test]
        public void DeleteComments()
        {
            IWorkbook wb1 = _testDataProvider.OpenSampleWorkbook("SimpleWithComments." + _testDataProvider.StandardFileNameExtension);
            ISheet sheet = wb1.GetSheetAt(0);

            // Zap from rows 1 and 3
            Assert.IsNotNull(sheet.GetRow(0).GetCell(1).CellComment);
            Assert.IsNotNull(sheet.GetRow(1).GetCell(1).CellComment);
            Assert.IsNotNull(sheet.GetRow(2).GetCell(1).CellComment);

            sheet.GetRow(0).GetCell(1).RemoveCellComment();
            sheet.GetRow(2).GetCell(1).CellComment = (null);

            // Check gone so far
            Assert.IsNull(sheet.GetRow(0).GetCell(1).CellComment);
            Assert.IsNotNull(sheet.GetRow(1).GetCell(1).CellComment);
            Assert.IsNull(sheet.GetRow(2).GetCell(1).CellComment);

            // Save and re-load
            IWorkbook wb2 = _testDataProvider.WriteOutAndReadBack(wb1);
            wb1.Close();

            sheet = wb2.GetSheetAt(0);
            // Check
            Assert.IsNull(sheet.GetRow(0).GetCell(1).CellComment);
            Assert.IsNotNull(sheet.GetRow(1).GetCell(1).CellComment);
            Assert.IsNull(sheet.GetRow(2).GetCell(1).CellComment);

            wb2.Close();
        }

        /**
         * code from the quick guide
         */
        [Test]
        public void QuickGuide()
        {
            IWorkbook wb1 = _testDataProvider.CreateWorkbook();

            ICreationHelper factory = wb1.GetCreationHelper();

            ISheet sheet = wb1.CreateSheet();

            ICell cell = sheet.CreateRow(3).CreateCell(5);
            cell.SetCellValue("F4");

            IDrawing drawing = sheet.CreateDrawingPatriarch();

            IClientAnchor anchor = factory.CreateClientAnchor();
            IComment comment = drawing.CreateCellComment(anchor);
            IRichTextString str = factory.CreateRichTextString("Hello, World!");
            comment.String = (str);
            comment.Author = ("Apache POI");
            //assign the comment to the cell
            cell.CellComment = (comment);

            IWorkbook wb2 = _testDataProvider.WriteOutAndReadBack(wb1);
            wb1.Close();

            sheet = wb2.GetSheetAt(0);
            cell = sheet.GetRow(3).GetCell(5);
            comment = cell.CellComment;
            Assert.IsNotNull(comment);
            Assert.AreEqual("Hello, World!", comment.String.String);
            Assert.AreEqual("Apache POI", comment.Author);
            Assert.AreEqual(3, comment.Row);
            Assert.AreEqual(5, comment.Column);

            wb2.Close();
        }

        [Test]
        public void GetClientAnchor()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();

            ISheet sheet = wb.CreateSheet();
            IRow row = sheet.CreateRow(10);
            ICell cell = row.CreateCell(5);
            ICreationHelper factory = wb.GetCreationHelper();

            IDrawing Drawing = sheet.CreateDrawingPatriarch();

            double r_mul, c_mul;
            if (sheet is HSSFSheet)
            {
                double rowheight = Units.ToEMU(row.HeightInPoints) / Units.EMU_PER_PIXEL;
                r_mul = 256.0 / rowheight;
                double colwidth = sheet.GetColumnWidthInPixels(2);
                c_mul = 1024.0 / colwidth;
            }
            else
            {
                r_mul = c_mul = Units.EMU_PER_PIXEL;
            }

            int dx1 = (int)Math.Round(10 * c_mul);
            int dy1 = (int)Math.Round(10 * r_mul);
            int dx2 = (int)Math.Round(3 * c_mul);
            int dy2 = (int)Math.Round(4 * r_mul);
            int col1 = cell.ColumnIndex + 1;
            int row1 = row.RowNum;
            int col2 = cell.ColumnIndex + 2;
            int row2 = row.RowNum + 1;

            IClientAnchor anchor = Drawing.CreateAnchor(dx1, dy1, dx2, dy2, col1, row1, col2, row2);
            IComment comment = Drawing.CreateCellComment(anchor);
            comment.Visible = (/*setter*/true);
            cell.CellComment = (/*setter*/comment);

            anchor = comment.ClientAnchor;
            Assert.AreEqual(dx1, anchor.Dx1);
            Assert.AreEqual(dy1, anchor.Dy1);
            Assert.AreEqual(dx2, anchor.Dx2);
            Assert.AreEqual(dy2, anchor.Dy2);
            Assert.AreEqual(col1, anchor.Col1);
            Assert.AreEqual(row1, anchor.Row1);
            Assert.AreEqual(col2, anchor.Col2);
            Assert.AreEqual(row2, anchor.Row2);

            anchor = factory.CreateClientAnchor();
            comment = Drawing.CreateCellComment(anchor);
            cell.CellComment = (/*setter*/comment);
            anchor = comment.ClientAnchor;

            if (sheet is HSSFSheet)
            {
                Assert.AreEqual(0, anchor.Col1);
                Assert.AreEqual(0, anchor.Dx1);
                Assert.AreEqual(0, anchor.Row1);
                Assert.AreEqual(0, anchor.Dy1);
                Assert.AreEqual(0, anchor.Col2);
                Assert.AreEqual(0, anchor.Dx2);
                Assert.AreEqual(0, anchor.Row2);
                Assert.AreEqual(0, anchor.Dy2);
            }
            else
            {
                // when anchor is Initialized without parameters, the comment anchor attributes default to
                // "1, 15, 0, 2, 3, 15, 3, 16" ... see XSSFVMLDrawing.NewCommentShape()
                Assert.AreEqual(1, anchor.Col1);
                Assert.AreEqual(15 * Units.EMU_PER_PIXEL, anchor.Dx1);
                Assert.AreEqual(0, anchor.Row1);
                Assert.AreEqual(2 * Units.EMU_PER_PIXEL, anchor.Dy1);
                Assert.AreEqual(3, anchor.Col2);
                Assert.AreEqual(15 * Units.EMU_PER_PIXEL, anchor.Dx2);
                Assert.AreEqual(3, anchor.Row2);
                Assert.AreEqual(16 * Units.EMU_PER_PIXEL, anchor.Dy2);
            }

            wb.Close();
        }
        [Test]
        public void AttemptToSave2CommentsWithSameCoordinates()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sh = wb.CreateSheet();
            ICreationHelper factory = wb.GetCreationHelper();
            IDrawing patriarch = sh.CreateDrawingPatriarch();
            patriarch.CreateCellComment(factory.CreateClientAnchor());

            try
            {
                patriarch.CreateCellComment(factory.CreateClientAnchor());
                _testDataProvider.WriteOutAndReadBack(wb);
                Assert.Fail("Expected InvalidOperationException(found multiple cell comments for cell $A$1");
            }
            catch (InvalidOperationException e)
            {
                // HSSFWorkbooks fail when writing out workbook
                Assert.AreEqual(e.Message, "found multiple cell comments for cell A1");
            }
            catch (ArgumentException e)
            {
                // XSSFWorkbooks fail when creating and setting the cell address of the comment
                Assert.AreEqual(e.Message, "Multiple cell comments in one cell are not allowed, cell: A1");
            }
            finally
            {
                wb.Close();
            }
        }

        [Test]
        public void GetAddress()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sh = wb.CreateSheet();
            ICreationHelper factory = wb.GetCreationHelper();
            IDrawing patriarch = sh.CreateDrawingPatriarch();
            IComment comment = patriarch.CreateCellComment(factory.CreateClientAnchor());

            Assert.AreEqual(CellAddress.A1, comment.Address);
            ICell C2 = sh.CreateRow(1).CreateCell(2);
            C2.CellComment = comment;
            Assert.AreEqual(new CellAddress("C2"), comment.Address);
        }

        [Test]
        public void SetAddress()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sh = wb.CreateSheet();
            ICreationHelper factory = wb.GetCreationHelper();
            IDrawing patriarch = sh.CreateDrawingPatriarch();
            IComment comment = patriarch.CreateCellComment(factory.CreateClientAnchor());

            Assert.AreEqual(CellAddress.A1, comment.Address);
            CellAddress C2 = new CellAddress("C2");
            Assert.AreEqual("C2", C2.FormatAsString());
            comment.Address = C2;
            Assert.AreEqual(C2, comment.Address);

            CellAddress E10 = new CellAddress(9, 4);
            Assert.AreEqual("E10", E10.FormatAsString());
            comment.SetAddress(9, 4);
            Assert.AreEqual(E10, comment.Address);
        }

    }
}



