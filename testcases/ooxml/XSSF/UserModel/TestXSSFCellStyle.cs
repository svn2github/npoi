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

using NPOI.XSSF.Model;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.XSSF.UserModel.Extensions;
using NUnit.Framework;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Drawing;
using NPOI.HSSF.Util;
namespace NPOI.XSSF.UserModel
{

    [TestFixture]
    public class TestXSSFCellStyle
    {

        private StylesTable stylesTable;
        private CT_Border ctBorderA;
        private CT_Fill ctFill;
        private CT_Font ctFont;
        private CT_Xf cellStyleXf;
        private CT_Xf cellXf;
        private CT_CellXfs cellXfs;
        private XSSFCellStyle cellStyle;
        private CT_Stylesheet ctStylesheet;

        [SetUp]
        public void SetUp()
        {
            stylesTable = new StylesTable();

            ctStylesheet = stylesTable.GetCTStylesheet();

            ctBorderA = new CT_Border();
            XSSFCellBorder borderA = new XSSFCellBorder(ctBorderA);
            long borderId = stylesTable.PutBorder(borderA);
            Assert.AreEqual(1, borderId);

            XSSFCellBorder borderB = new XSSFCellBorder();
            Assert.AreEqual(1, stylesTable.PutBorder(borderB));

            ctFill = new CT_Fill();
            XSSFCellFill fill = new XSSFCellFill(ctFill);
            long fillId = stylesTable.PutFill(fill);
            Assert.AreEqual(2, fillId);

            ctFont = new CT_Font();
            XSSFFont font = new XSSFFont(ctFont);
            long fontId = stylesTable.PutFont(font);
            Assert.AreEqual(1, fontId);

            cellStyleXf = ctStylesheet.AddNewCellStyleXfs().AddNewXf();
            cellStyleXf.borderId = 1;
            cellStyleXf.fillId = 1;
            cellStyleXf.fontId = 1;

            cellXfs = ctStylesheet.AddNewCellXfs();
            cellXf = cellXfs.AddNewXf();
            cellXf.xfId = (1);
            cellXf.borderId = (1);
            cellXf.fillId = (1);
            cellXf.fontId = (1);
            stylesTable.PutCellStyleXf(cellStyleXf);
            stylesTable.PutCellXf(cellXf);
            cellStyle = new XSSFCellStyle(1, 1, stylesTable, null);
        }
        [Test]
        public void TestGetSetBorderBottom()
        {
            //default values
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderBottom);

            int num = stylesTable.GetBorders().Count;
            cellStyle.BorderBottom = (BorderStyle.Medium);
            Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderBottom);
            //a new border has been Added
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);
            //id of the Created border
            int borderId = (int)cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt(borderId).GetCTBorder();
            Assert.AreEqual(ST_BorderStyle.medium, ctBorder.bottom.style);

            num = stylesTable.GetBorders().Count;
            //setting the same border multiple times should not change borderId
            for (int i = 0; i < 3; i++)
            {
                cellStyle.BorderBottom = (BorderStyle.Medium);
                Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderBottom);
            }
            Assert.AreEqual((uint)borderId, cellStyle.GetCoreXf().borderId);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            Assert.AreSame(ctBorder, stylesTable.GetBorderAt(borderId).GetCTBorder());

            //setting border to none Removes the <bottom> element
            cellStyle.BorderBottom = (BorderStyle.None);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            borderId = (int)cellStyle.GetCoreXf().borderId;
            ctBorder = stylesTable.GetBorderAt(borderId).GetCTBorder();
            Assert.IsFalse(ctBorder.IsSetBottom());
        }
        [Test]
        public void TestSetServeralBordersOnSameCell()
        {
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderRight);
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderLeft);
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderTop);
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderBottom);
            Assert.AreEqual(2, stylesTable.GetBorders().Count);

            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BottomBorderColor = HSSFColor.Black.Index;
            cellStyle.BorderLeft = BorderStyle.DashDotDot;
            cellStyle.LeftBorderColor = HSSFColor.Green.Index;
            cellStyle.BorderRight = BorderStyle.Hair;
            cellStyle.RightBorderColor = HSSFColor.Blue.Index;
            cellStyle.BorderTop = BorderStyle.MediumDashed;
            cellStyle.TopBorderColor = HSSFColor.Orange.Index;
            //only one border style should be generated
            Assert.AreEqual(3, stylesTable.GetBorders().Count);

        }
        [Test]
        public void TestGetSetBorderDiagonal()
        {
            Assert.AreEqual(BorderDiagonal.None, cellStyle.BorderDiagonal);

            int num = stylesTable.GetBorders().Count;
            cellStyle.BorderDiagonalLineStyle = BorderStyle.Medium;
            cellStyle.BorderDiagonalColor = HSSFColor.Red.Index;
            cellStyle.BorderDiagonal = BorderDiagonal.Backward;

            Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderDiagonalLineStyle);
            //a new border has been added
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);
            //id of the created border
            uint borderId = cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);

            CT_Border ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.AreEqual(ST_BorderStyle.medium, ctBorder.diagonal.style);

            num = stylesTable.GetBorders().Count;
            //setting the same border multiple times should not change borderId
            for (int i = 0; i < 3; i++)
            {
                cellStyle.BorderDiagonal = BorderDiagonal.Backward;
                Assert.AreEqual(BorderDiagonal.Backward, cellStyle.BorderDiagonal);
            }
            Assert.AreEqual(borderId, cellStyle.GetCoreXf().borderId);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            Assert.AreSame(ctBorder, stylesTable.GetBorderAt((int)borderId).GetCTBorder());

            cellStyle.BorderDiagonal = (BorderDiagonal.None);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            borderId = cellStyle.GetCoreXf().borderId;
            ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.IsFalse(ctBorder.IsSetDiagonal());
        }
        [Test]
        public void TestGetSetBorderRight()
        {
            //default values
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderRight);

            int num = stylesTable.GetBorders().Count;
            cellStyle.BorderRight = (BorderStyle.Medium);
            Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderRight);
            //a new border has been Added
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);
            //id of the Created border
            uint borderId = cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.AreEqual(ST_BorderStyle.medium, ctBorder.right.style);

            num = stylesTable.GetBorders().Count;
            //setting the same border multiple times should not change borderId
            for (int i = 0; i < 3; i++)
            {
                cellStyle.BorderRight = (BorderStyle.Medium);
                Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderRight);
            }
            Assert.AreEqual(borderId, cellStyle.GetCoreXf().borderId);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            Assert.AreSame(ctBorder, stylesTable.GetBorderAt((int)borderId).GetCTBorder());

            //setting border to none Removes the <right> element
            cellStyle.BorderRight = (BorderStyle.None);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            borderId = cellStyle.GetCoreXf().borderId;
            ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.IsFalse(ctBorder.IsSetRight());
        }
        [Test]
        public void TestGetSetBorderLeft()
        {
            //default values
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderLeft);

            int num = stylesTable.GetBorders().Count;
            cellStyle.BorderLeft = (BorderStyle.Medium);
            Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderLeft);
            //a new border has been Added
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);
            //id of the Created border
            uint borderId = cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.AreEqual(ST_BorderStyle.medium, ctBorder.left.style);

            num = stylesTable.GetBorders().Count;
            //setting the same border multiple times should not change borderId
            for (int i = 0; i < 3; i++)
            {
                cellStyle.BorderLeft = (BorderStyle.Medium);
                Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderLeft);
            }
            Assert.AreEqual(borderId, cellStyle.GetCoreXf().borderId);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            Assert.AreSame(ctBorder, stylesTable.GetBorderAt((int)borderId).GetCTBorder());

            //setting border to none Removes the <left> element
            cellStyle.BorderLeft = (BorderStyle.None);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            borderId = cellStyle.GetCoreXf().borderId;
            ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.IsFalse(ctBorder.IsSetLeft());
        }
        [Test]
        public void TestGetSetBorderTop()
        {
            //default values
            Assert.AreEqual(BorderStyle.None, cellStyle.BorderTop);

            int num = stylesTable.GetBorders().Count;
            cellStyle.BorderTop = BorderStyle.Medium;
            Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderTop);
            //a new border has been Added
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);
            //id of the Created border
            uint borderId = cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.AreEqual(ST_BorderStyle.medium, ctBorder.top.style);

            num = stylesTable.GetBorders().Count;
            //setting the same border multiple times should not change borderId
            for (int i = 0; i < 3; i++)
            {
                cellStyle.BorderTop = BorderStyle.Medium;
                Assert.AreEqual(BorderStyle.Medium, cellStyle.BorderTop);
            }
            Assert.AreEqual((uint)borderId, cellStyle.GetCoreXf().borderId);
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            Assert.AreSame(ctBorder, stylesTable.GetBorderAt((int)borderId).GetCTBorder());

            //setting border to none Removes the <top> element
            cellStyle.BorderTop = BorderStyle.None;
            Assert.AreEqual(num, stylesTable.GetBorders().Count);
            borderId = cellStyle.GetCoreXf().borderId;
            ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.IsFalse(ctBorder.IsSetTop());
        }
        [Test]
        public void TestGetSetBottomBorderColor()
        {
            //defaults
            Assert.AreEqual(IndexedColors.Black.Index, cellStyle.BottomBorderColor);
            Assert.IsNull(cellStyle.BottomBorderXSSFColor);

            int num = stylesTable.GetBorders().Count;

            XSSFColor clr;

            //setting indexed color
            cellStyle.BottomBorderColor = (IndexedColors.BlueGrey.Index);
            Assert.AreEqual(IndexedColors.BlueGrey.Index, cellStyle.BottomBorderColor);
            clr = cellStyle.BottomBorderXSSFColor;
            Assert.IsTrue(clr.GetCTColor().IsSetIndexed());
            Assert.AreEqual(IndexedColors.BlueGrey.Index, clr.Indexed);
            //a new border was Added to the styles table
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);

            //id of the Created border
            uint borderId = cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt((int)borderId).GetCTBorder();
            Assert.AreEqual((uint)IndexedColors.BlueGrey.Index, ctBorder.bottom.color.indexed);

            //setting XSSFColor
            num = stylesTable.GetBorders().Count;
            clr = new XSSFColor(Color.Cyan);
            cellStyle.SetBottomBorderColor(clr);
            Assert.AreEqual(clr.GetCTColor().ToString(), cellStyle.BottomBorderXSSFColor.GetCTColor().ToString());
            byte[] rgb = cellStyle.BottomBorderXSSFColor.GetRgb();
            Assert.AreEqual(Color.Cyan.ToArgb(), Color.FromArgb(rgb[0] & 0xFF, rgb[1] & 0xFF, rgb[2] & 0xFF).ToArgb());
            //another border was Added to the styles table
            Assert.AreEqual(num, stylesTable.GetBorders().Count);

            //passing null unsets the color
            cellStyle.SetBottomBorderColor(null);
            Assert.IsNull(cellStyle.BottomBorderXSSFColor);
        }
        [Test]
        public void TestGetSetTopBorderColor()
        {
            //defaults
            Assert.AreEqual(IndexedColors.Black.Index, cellStyle.TopBorderColor);
            Assert.IsNull(cellStyle.TopBorderXSSFColor);

            int num = stylesTable.GetBorders().Count;

            XSSFColor clr;

            //setting indexed color
            cellStyle.TopBorderColor = (IndexedColors.BlueGrey.Index);
            Assert.AreEqual(IndexedColors.BlueGrey.Index, cellStyle.TopBorderColor);
            clr = cellStyle.TopBorderXSSFColor;
            Assert.IsTrue(clr.GetCTColor().IsSetIndexed());
            Assert.AreEqual(IndexedColors.BlueGrey.Index, clr.Indexed);
            //a new border was added to the styles table
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);

            //id of the Created border
            int borderId = (int)cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt(borderId).GetCTBorder();
            Assert.AreEqual((uint)IndexedColors.BlueGrey.Index, ctBorder.top.color.indexed);

            //setting XSSFColor
            num = stylesTable.GetBorders().Count;
            clr = new XSSFColor(Color.Cyan);
            cellStyle.SetTopBorderColor(clr);
            Assert.AreEqual(clr.GetCTColor().ToString(), cellStyle.TopBorderXSSFColor.GetCTColor().ToString());
            byte[] rgb = cellStyle.TopBorderXSSFColor.GetRgb();
            Assert.AreEqual(Color.Cyan.ToArgb(), Color.FromArgb(rgb[0], rgb[1], rgb[2]).ToArgb());
            //another border was added to the styles table
            Assert.AreEqual(num, stylesTable.GetBorders().Count);

            //passing null unsets the color
            cellStyle.SetTopBorderColor(null);
            Assert.IsNull(cellStyle.TopBorderXSSFColor);
        }
        [Test]
        public void TestGetSetLeftBorderColor()
        {
            //defaults
            Assert.AreEqual(IndexedColors.Black.Index, cellStyle.LeftBorderColor);
            Assert.IsNull(cellStyle.LeftBorderXSSFColor);

            int num = stylesTable.GetBorders().Count;

            XSSFColor clr;

            //setting indexed color
            cellStyle.LeftBorderColor = (IndexedColors.BlueGrey.Index);
            Assert.AreEqual(IndexedColors.BlueGrey.Index, cellStyle.LeftBorderColor);
            clr = cellStyle.LeftBorderXSSFColor;
            Assert.IsTrue(clr.GetCTColor().IsSetIndexed());
            Assert.AreEqual(IndexedColors.BlueGrey.Index, clr.Indexed);
            //a new border was Added to the styles table
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);

            //id of the Created border
            int borderId = (int)cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt(borderId).GetCTBorder();
            Assert.AreEqual((uint)IndexedColors.BlueGrey.Index, ctBorder.left.color.indexed);

            //setting XSSFColor
            num = stylesTable.GetBorders().Count;
            clr = new XSSFColor(Color.Cyan);
            cellStyle.SetLeftBorderColor(clr);
            Assert.AreEqual(clr.GetCTColor().ToString(), cellStyle.LeftBorderXSSFColor.GetCTColor().ToString());
            byte[] rgb = cellStyle.LeftBorderXSSFColor.GetRgb();
            Assert.AreEqual(Color.Cyan.ToArgb(), Color.FromArgb(rgb[0] & 0xFF, rgb[1] & 0xFF, rgb[2] & 0xFF).ToArgb());
            //another border was Added to the styles table
            Assert.AreEqual(num, stylesTable.GetBorders().Count);

            //passing null unsets the color
            cellStyle.SetLeftBorderColor(null);
            Assert.IsNull(cellStyle.LeftBorderXSSFColor);
        }
        [Test]
        public void TestGetSetRightBorderColor()
        {
            //defaults
            Assert.AreEqual(IndexedColors.Black.Index, cellStyle.RightBorderColor);
            Assert.IsNull(cellStyle.RightBorderXSSFColor);

            int num = stylesTable.GetBorders().Count;

            XSSFColor clr;

            //setting indexed color
            cellStyle.RightBorderColor = (IndexedColors.BlueGrey.Index);
            Assert.AreEqual(IndexedColors.BlueGrey.Index, cellStyle.RightBorderColor);
            clr = cellStyle.RightBorderXSSFColor;
            Assert.IsTrue(clr.GetCTColor().IsSetIndexed());
            Assert.AreEqual(IndexedColors.BlueGrey.Index, clr.Indexed);
            //a new border was Added to the styles table
            Assert.AreEqual(num + 1, stylesTable.GetBorders().Count);

            //id of the Created border
            int borderId = (int)cellStyle.GetCoreXf().borderId;
            Assert.IsTrue(borderId > 0);
            //check Changes in the underlying xml bean
            CT_Border ctBorder = stylesTable.GetBorderAt(borderId).GetCTBorder();
            Assert.AreEqual((uint)IndexedColors.BlueGrey.Index, ctBorder.right.color.indexed);

            //setting XSSFColor
            num = stylesTable.GetBorders().Count;
            clr = new XSSFColor(Color.Cyan);
            cellStyle.SetRightBorderColor(clr);
            Assert.AreEqual(clr.GetCTColor().ToString(), cellStyle.RightBorderXSSFColor.GetCTColor().ToString());
            byte[] rgb = cellStyle.RightBorderXSSFColor.GetRgb();
            Assert.AreEqual(Color.Cyan.ToArgb(), Color.FromArgb(rgb[0] & 0xFF, rgb[1] & 0xFF, rgb[2] & 0xFF).ToArgb());
            //another border was Added to the styles table
            Assert.AreEqual(num, stylesTable.GetBorders().Count);

            //passing null unsets the color
            cellStyle.SetRightBorderColor(null);
            Assert.IsNull(cellStyle.RightBorderXSSFColor);
        }


        [Test]
        public void TestGetSetFillBackgroundColor()
        {
            Assert.AreEqual(IndexedColors.Automatic.Index, cellStyle.FillBackgroundColor);
            Assert.IsNull(cellStyle.FillBackgroundColorColor);

            XSSFColor clr;

            int num = stylesTable.GetFills().Count;

            //setting indexed color
            cellStyle.FillBackgroundColor = (IndexedColors.Red.Index);
            Assert.AreEqual(IndexedColors.Red.Index, cellStyle.FillBackgroundColor);
            clr = (XSSFColor)cellStyle.FillBackgroundColorColor;
            Assert.IsTrue(clr.GetCTColor().IsSetIndexed());
            Assert.AreEqual(IndexedColors.Red.Index, clr.Indexed);
            //a new fill was Added to the styles table
            Assert.AreEqual(num + 1, stylesTable.GetFills().Count);

            //id of the Created border
            int FillId = (int)cellStyle.GetCoreXf().fillId;
            Assert.IsTrue(FillId > 0);
            //check changes in the underlying xml bean
            CT_Fill ctFill = stylesTable.GetFillAt(FillId).GetCTFill();
            Assert.AreEqual((uint)IndexedColors.Red.Index, ctFill.GetPatternFill().bgColor.indexed);

            //setting XSSFColor
            num = stylesTable.GetFills().Count;
            clr = new XSSFColor(Color.Cyan);
            cellStyle.SetFillBackgroundColor(clr); // TODO this testcase assumes that cellStyle creates a new CT_Fill, but the implementation changes the existing style. - do not know whats right 8-(
            Assert.AreEqual(clr.GetCTColor().ToString(), ((XSSFColor)cellStyle.FillBackgroundColorColor).GetCTColor().ToString());
            byte[] rgb = ((XSSFColor)cellStyle.FillBackgroundColorColor).GetRgb();
            Assert.AreEqual(Color.Cyan.ToArgb(), Color.FromArgb(rgb[0] & 0xFF, rgb[1] & 0xFF, rgb[2] & 0xFF).ToArgb());
            //another border was added to the styles table
            Assert.AreEqual(num, stylesTable.GetFills().Count);

            //passing null unsets the color
            cellStyle.SetFillBackgroundColor(null);
            Assert.IsNull(cellStyle.FillBackgroundColorColor);
            Assert.AreEqual(IndexedColors.Automatic.Index, cellStyle.FillBackgroundColor);
        }
        [Test]
        public void TestDefaultStyles()
        {

            XSSFWorkbook wb1 = new XSSFWorkbook();

            XSSFCellStyle style1 = (XSSFCellStyle)wb1.CreateCellStyle();
            Assert.AreEqual(IndexedColors.Automatic.Index, style1.FillBackgroundColor);
            Assert.IsNull(style1.FillBackgroundColorColor);

            //compatibility with HSSF
            HSSFWorkbook wb2 = new HSSFWorkbook();
            HSSFCellStyle style2 = (HSSFCellStyle)wb2.CreateCellStyle();
            Assert.AreEqual(style2.FillBackgroundColor, style1.FillBackgroundColor);
            Assert.AreEqual(style2.FillForegroundColor, style1.FillForegroundColor);
            Assert.AreEqual(style2.FillPattern, style1.FillPattern);

            Assert.AreEqual(style2.LeftBorderColor, style1.LeftBorderColor);
            Assert.AreEqual(style2.TopBorderColor, style1.TopBorderColor);
            Assert.AreEqual(style2.RightBorderColor, style1.RightBorderColor);
            Assert.AreEqual(style2.BottomBorderColor, style1.BottomBorderColor);

            Assert.AreEqual(style2.BorderBottom, style1.BorderBottom);
            Assert.AreEqual(style2.BorderLeft, style1.BorderLeft);
            Assert.AreEqual(style2.BorderRight, style1.BorderRight);
            Assert.AreEqual(style2.BorderTop, style1.BorderTop);
        }

        [Ignore]
        public void TestGetFillForegroundColor()
        {

            XSSFWorkbook wb = new XSSFWorkbook();
            StylesTable styles = wb.GetStylesSource();
            Assert.AreEqual(1, wb.NumCellStyles);
            Assert.AreEqual(2, styles.GetFills().Count);

            XSSFCellStyle defaultStyle = (XSSFCellStyle)wb.GetCellStyleAt((short)0);
            Assert.AreEqual(IndexedColors.Automatic.Index, defaultStyle.FillForegroundColor);
            Assert.AreEqual(null, defaultStyle.FillForegroundColorColor);
            Assert.AreEqual(FillPattern.NoFill, defaultStyle.FillPattern);

            XSSFCellStyle customStyle = (XSSFCellStyle)wb.CreateCellStyle();

            customStyle.FillPattern = (FillPattern.SolidForeground);
            Assert.AreEqual(FillPattern.SolidForeground, customStyle.FillPattern);
            Assert.AreEqual(3, styles.GetFills().Count);

            customStyle.FillForegroundColor = (IndexedColors.BrightGreen.Index);
            Assert.AreEqual(IndexedColors.BrightGreen.Index, customStyle.FillForegroundColor);
            Assert.AreEqual(4, styles.GetFills().Count);

            for (int i = 0; i < 3; i++)
            {
                XSSFCellStyle style = (XSSFCellStyle)wb.CreateCellStyle();

                style.FillPattern = (FillPattern.SolidForeground);
                Assert.AreEqual(FillPattern.SolidForeground, style.FillPattern);
                Assert.AreEqual(4, styles.GetFills().Count);

                style.FillForegroundColor = (IndexedColors.BrightGreen.Index);
                Assert.AreEqual(IndexedColors.BrightGreen.Index, style.FillForegroundColor);
                Assert.AreEqual(4, styles.GetFills().Count);
            }
        }
        [Test]
        public void TestGetFillPattern()
        {

            Assert.AreEqual(FillPattern.NoFill, cellStyle.FillPattern);

            int num = stylesTable.GetFills().Count;
            cellStyle.FillPattern = (FillPattern.SolidForeground);
            Assert.AreEqual(FillPattern.SolidForeground, cellStyle.FillPattern);
            Assert.AreEqual(num + 1, stylesTable.GetFills().Count);
            int FillId = (int)cellStyle.GetCoreXf().fillId;
            Assert.IsTrue(FillId > 0);
            //check Changes in the underlying xml bean
            CT_Fill ctFill = stylesTable.GetFillAt(FillId).GetCTFill();
            Assert.AreEqual(ST_PatternType.solid, ctFill.GetPatternFill().patternType);

            //setting the same fill multiple time does not update the styles table
            for (int i = 0; i < 3; i++)
            {
                cellStyle.FillPattern = (FillPattern.SolidForeground);
            }
            Assert.AreEqual(num + 1, stylesTable.GetFills().Count);

            cellStyle.FillPattern = (FillPattern.NoFill);
            Assert.AreEqual(FillPattern.NoFill, cellStyle.FillPattern);
            FillId = (int)cellStyle.GetCoreXf().fillId;
            ctFill = stylesTable.GetFillAt(FillId).GetCTFill();
            Assert.IsFalse(ctFill.GetPatternFill().IsSetPatternType());

        }
        [Test]
        public void TestGetFont()
        {
            Assert.IsNotNull(cellStyle.GetFont());
        }
        [Test]
        public void TestGetSetHidden()
        {
            Assert.IsFalse(cellStyle.IsHidden);
            cellStyle.IsHidden = (true);
            Assert.IsTrue(cellStyle.IsHidden);
            cellStyle.IsHidden = (false);
            Assert.IsFalse(cellStyle.IsHidden);
        }
        [Test]
        public void TestGetSetLocked()
        {
            Assert.IsTrue(cellStyle.IsLocked);
            cellStyle.IsLocked = (true);
            Assert.IsTrue(cellStyle.IsLocked);
            cellStyle.IsLocked = (false);
            Assert.IsFalse(cellStyle.IsLocked);
        }
        [Test]
        public void TestGetSetIndent()
        {
            Assert.AreEqual((short)0, cellStyle.Indention);
            cellStyle.Indention = ((short)3);
            Assert.AreEqual((short)3, cellStyle.Indention);
            cellStyle.Indention = ((short)13);
            Assert.AreEqual((short)13, cellStyle.Indention);
        }
        [Test]
        public void TestGetSetAlignement()
        {
            Assert.IsTrue(!cellStyle.GetCellAlignment().GetCTCellAlignment().horizontalSpecified);
            Assert.AreEqual(HorizontalAlignment.General, cellStyle.Alignment);

            cellStyle.Alignment = HorizontalAlignment.Left;
            Assert.AreEqual(HorizontalAlignment.Left, cellStyle.Alignment);
            Assert.AreEqual(ST_HorizontalAlignment.left, cellStyle.GetCellAlignment().GetCTCellAlignment().horizontal);

            cellStyle.Alignment = (HorizontalAlignment.Justify);
            Assert.AreEqual(HorizontalAlignment.Justify, cellStyle.Alignment);
            Assert.AreEqual(ST_HorizontalAlignment.justify, cellStyle.GetCellAlignment().GetCTCellAlignment().horizontal);

            cellStyle.Alignment = (HorizontalAlignment.Center);
            Assert.AreEqual(HorizontalAlignment.Center, cellStyle.Alignment);
            Assert.AreEqual(ST_HorizontalAlignment.center, cellStyle.GetCellAlignment().GetCTCellAlignment().horizontal);
        }
        [Test]
        public void TestGetSetVerticalAlignment()
        {
            Assert.AreEqual(VerticalAlignment.Bottom, cellStyle.VerticalAlignment);
            Assert.IsTrue(!cellStyle.GetCellAlignment().GetCTCellAlignment().verticalSpecified);

            cellStyle.VerticalAlignment = (VerticalAlignment.Top);
            Assert.AreEqual(VerticalAlignment.Top, cellStyle.VerticalAlignment);
            Assert.AreEqual(ST_VerticalAlignment.top, cellStyle.GetCellAlignment().GetCTCellAlignment().vertical);

            cellStyle.VerticalAlignment = (VerticalAlignment.Center);
            Assert.AreEqual(VerticalAlignment.Center, cellStyle.VerticalAlignment);
            Assert.AreEqual(ST_VerticalAlignment.center, cellStyle.GetCellAlignment().GetCTCellAlignment().vertical);

            cellStyle.VerticalAlignment = VerticalAlignment.Justify;
            Assert.AreEqual(VerticalAlignment.Justify, cellStyle.VerticalAlignment);
            Assert.AreEqual(ST_VerticalAlignment.justify, cellStyle.GetCellAlignment().GetCTCellAlignment().vertical);

            cellStyle.VerticalAlignment = (VerticalAlignment.Bottom);
            Assert.AreEqual(VerticalAlignment.Bottom, cellStyle.VerticalAlignment);
            Assert.AreEqual(ST_VerticalAlignment.bottom, cellStyle.GetCellAlignment().GetCTCellAlignment().vertical);
        }
        [Test]
        public void TestGetSetWrapText()
        {
            Assert.IsFalse(cellStyle.WrapText);
            cellStyle.WrapText = (true);
            Assert.IsTrue(cellStyle.WrapText);
            cellStyle.WrapText = (false);
            Assert.IsFalse(cellStyle.WrapText);
        }

        /**
         * Cloning one XSSFCellStyle onto Another, same XSSFWorkbook
         */
        [Test]
        public void TestCloneStyleSameWB()
        {
            XSSFWorkbook wb = new XSSFWorkbook();
            Assert.AreEqual(1, wb.NumberOfFonts);

            XSSFFont fnt = (XSSFFont)wb.CreateFont();
            fnt.FontName = ("TestingFont");
            Assert.AreEqual(2, wb.NumberOfFonts);

            XSSFCellStyle orig = (XSSFCellStyle)wb.CreateCellStyle();
            orig.Alignment = (HorizontalAlignment.Right);
            orig.SetFont(fnt);
            orig.DataFormat = (short)18;

            Assert.AreEqual(HorizontalAlignment.Right, orig.Alignment);
            Assert.AreEqual(fnt, orig.GetFont());
            Assert.AreEqual(18, orig.DataFormat);

            XSSFCellStyle clone = (XSSFCellStyle)wb.CreateCellStyle();
            Assert.AreNotEqual(HorizontalAlignment.Right, clone.Alignment);
            Assert.AreNotEqual(fnt, clone.GetFont());
            Assert.AreNotEqual(18, clone.DataFormat);

            clone.CloneStyleFrom(orig);
            Assert.AreEqual(HorizontalAlignment.Right, clone.Alignment);
            Assert.AreEqual(fnt, clone.GetFont());
            Assert.AreEqual(18, clone.DataFormat);
            Assert.AreEqual(2, wb.NumberOfFonts);
        }
        /**
         * Cloning one XSSFCellStyle onto Another, different XSSFWorkbooks
         */
        [Test]
        public void TestCloneStyleDiffWB()
        {
            XSSFWorkbook wbOrig = new XSSFWorkbook();
            Assert.AreEqual(1, wbOrig.NumberOfFonts);
            Assert.AreEqual(0, wbOrig.GetStylesSource().GetNumberFormats().Count);

            XSSFFont fnt = (XSSFFont)wbOrig.CreateFont();
            fnt.FontName = ("TestingFont");
            Assert.AreEqual(2, wbOrig.NumberOfFonts);
            Assert.AreEqual(0, wbOrig.GetStylesSource().GetNumberFormats().Count);

            XSSFDataFormat fmt = (XSSFDataFormat)wbOrig.CreateDataFormat();
            fmt.GetFormat("MadeUpOne");
            fmt.GetFormat("MadeUpTwo");

            XSSFCellStyle orig = (XSSFCellStyle)wbOrig.CreateCellStyle();
            orig.Alignment = (HorizontalAlignment.Right);
            orig.SetFont(fnt);
            orig.DataFormat = (fmt.GetFormat("Test##"));

            Assert.IsTrue(HorizontalAlignment.Right == orig.Alignment);
            Assert.IsTrue(fnt == orig.GetFont());
            Assert.IsTrue(fmt.GetFormat("Test##") == orig.DataFormat);

            Assert.AreEqual(2, wbOrig.NumberOfFonts);
            Assert.AreEqual(3, wbOrig.GetStylesSource().GetNumberFormats().Count);


            // Now a style on another workbook
            XSSFWorkbook wbClone = new XSSFWorkbook();
            Assert.AreEqual(1, wbClone.NumberOfFonts);
            Assert.AreEqual(0, wbClone.GetStylesSource().GetNumberFormats().Count);
            Assert.AreEqual(1, wbClone.NumCellStyles);

            XSSFDataFormat fmtClone = (XSSFDataFormat)wbClone.CreateDataFormat();
            XSSFCellStyle clone = (XSSFCellStyle)wbClone.CreateCellStyle();

            Assert.AreEqual(1, wbClone.NumberOfFonts);
            Assert.AreEqual(0, wbClone.GetStylesSource().GetNumberFormats().Count);

            Assert.IsFalse(HorizontalAlignment.Right == clone.Alignment);
            Assert.IsFalse("TestingFont" == clone.GetFont().FontName);

            clone.CloneStyleFrom(orig);

            Assert.AreEqual(2, wbClone.NumberOfFonts);
            Assert.AreEqual(2, wbClone.NumCellStyles);
            Assert.AreEqual(1, wbClone.GetStylesSource().GetNumberFormats().Count);

            Assert.AreEqual(HorizontalAlignment.Right, clone.Alignment);
            Assert.AreEqual("TestingFont", clone.GetFont().FontName);
            Assert.AreEqual(fmtClone.GetFormat("Test##"), clone.DataFormat);
            Assert.IsFalse(fmtClone.GetFormat("Test##") == fmt.GetFormat("Test##"));

            // Save it and re-check
            XSSFWorkbook wbReload = (XSSFWorkbook)XSSFTestDataSamples.WriteOutAndReadBack(wbClone);
            Assert.AreEqual(2, wbReload.NumberOfFonts);
            Assert.AreEqual(2, wbReload.NumCellStyles);
            Assert.AreEqual(1, wbReload.GetStylesSource().GetNumberFormats().Count);

            XSSFCellStyle reload = (XSSFCellStyle)wbReload.GetCellStyleAt((short)1);
            Assert.AreEqual(HorizontalAlignment.Right, reload.Alignment);
            Assert.AreEqual("TestingFont", reload.GetFont().FontName);
            Assert.AreEqual(fmtClone.GetFormat("Test##"), reload.DataFormat);
            Assert.IsFalse(fmtClone.GetFormat("Test##") == fmt.GetFormat("Test##"));
        }

        /**
          * Avoid ArrayIndexOutOfBoundsException  when creating cell style
          * in a workbook that has an empty xf table.
          */
        [Test]
        public void TestBug52348()
        {
            XSSFWorkbook workbook = XSSFTestDataSamples.OpenSampleWorkbook("52348.xlsx");
            StylesTable st = workbook.GetStylesSource();
            Assert.AreEqual(0, st.GetStyleXfsSize());


            XSSFCellStyle style = workbook.CreateCellStyle() as XSSFCellStyle; // no exception at this point
            Assert.IsNull(style.GetStyleXf());
        }

    }
}