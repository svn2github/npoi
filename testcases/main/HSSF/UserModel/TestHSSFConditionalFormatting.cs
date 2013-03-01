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

    using NPOI.HSSF.UserModel;
    using NPOI.HSSF.Util;
    using NPOI.HSSF.Record;
    using NUnit.Framework;
    using NPOI.SS.Util;
    using NPOI.SS.UserModel;
    using NPOI.HSSF.Record.CF;
    /**
     * 
     * @author Dmitriy Kumshayev
     */
    [TestFixture]
    public class TestHSSFConditionalFormatting
    {
        [Test]
        public void TestCreateCF()
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)workbook.CreateSheet();
            String formula = "7";

            HSSFSheetConditionalFormatting sheetCF = (HSSFSheetConditionalFormatting)sheet.SheetConditionalFormatting;

            HSSFConditionalFormattingRule rule1 = (HSSFConditionalFormattingRule)sheetCF.CreateConditionalFormattingRule(formula);
            HSSFFontFormatting fontFmt = (HSSFFontFormatting)rule1.CreateFontFormatting();
            fontFmt.SetFontStyle(true, false);

            HSSFBorderFormatting bordFmt = (HSSFBorderFormatting)rule1.CreateBorderFormatting();
            bordFmt.BorderBottom = BorderFormatting.BORDER_THIN;
            bordFmt.BorderTop = BorderFormatting.BORDER_THICK;
            bordFmt.BorderLeft = BorderFormatting.BORDER_DASHED;
            bordFmt.BorderRight = BorderFormatting.BORDER_DOTTED;

            HSSFPatternFormatting patternFmt = (HSSFPatternFormatting)rule1.CreatePatternFormatting();
            patternFmt.FillBackgroundColor= (HSSFColor.YELLOW.index);


            HSSFConditionalFormattingRule rule2 = (HSSFConditionalFormattingRule)sheetCF.CreateConditionalFormattingRule(ComparisonOperator.BETWEEN, "1", "2");
            HSSFConditionalFormattingRule[] cfRules =
		    {
			    rule1, rule2
		    };

            short col = 1;
            CellRangeAddress[] regions = {
			    new CellRangeAddress(0, 65535, col, col)
		    };

            sheetCF.AddConditionalFormatting(regions, cfRules);
            sheetCF.AddConditionalFormatting(regions, cfRules);

            // Verification
            Assert.AreEqual(2, sheetCF.NumConditionalFormattings);
            sheetCF.RemoveConditionalFormatting(1);
            Assert.AreEqual(1, sheetCF.NumConditionalFormattings);
            HSSFConditionalFormatting cf = (HSSFConditionalFormatting)sheetCF.GetConditionalFormattingAt(0);
            Assert.IsNotNull(cf);

            regions = cf.GetFormattingRanges();
            Assert.IsNotNull(regions);
            Assert.AreEqual(1, regions.Length);
            CellRangeAddress r = regions[0];
            Assert.AreEqual(1, r.FirstColumn);
            Assert.AreEqual(1, r.LastColumn);
            Assert.AreEqual(0, r.FirstRow);
            Assert.AreEqual(65535, r.LastRow);

            Assert.AreEqual(2, cf.NumberOfRules);

            rule1 = (HSSFConditionalFormattingRule)cf.GetRule(0);
            Assert.AreEqual("7", rule1.Formula1);
            Assert.IsNull(rule1.Formula2);

            HSSFFontFormatting r1fp = (HSSFFontFormatting)rule1.GetFontFormatting();
            Assert.IsNotNull(r1fp);

            Assert.IsTrue(r1fp.IsItalic);
            Assert.IsFalse(r1fp.IsBold);

            HSSFBorderFormatting r1bf = (HSSFBorderFormatting)rule1.GetBorderFormatting();
            Assert.IsNotNull(r1bf);
            Assert.AreEqual(BorderFormatting.BORDER_THIN, r1bf.BorderBottom);
            Assert.AreEqual(BorderFormatting.BORDER_THICK, r1bf.BorderTop);
            Assert.AreEqual(BorderFormatting.BORDER_DASHED, r1bf.BorderLeft);
            Assert.AreEqual(BorderFormatting.BORDER_DOTTED, r1bf.BorderRight);

            HSSFPatternFormatting r1pf = (HSSFPatternFormatting)rule1.GetPatternFormatting();
            Assert.IsNotNull(r1pf);
            Assert.AreEqual(HSSFColor.YELLOW.index, r1pf.FillBackgroundColor);

            rule2 = (HSSFConditionalFormattingRule)cf.GetRule(1);
            Assert.AreEqual("2", rule2.Formula2);
            Assert.AreEqual("1", rule2.Formula1);
        }
        [Test]
        public void TestClone()
        {

            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)wb.CreateSheet();
            String formula = "7";

            HSSFSheetConditionalFormatting sheetCF = (HSSFSheetConditionalFormatting)sheet.SheetConditionalFormatting;

            HSSFConditionalFormattingRule rule1 = (HSSFConditionalFormattingRule)sheetCF.CreateConditionalFormattingRule(formula);
            HSSFFontFormatting fontFmt = (HSSFFontFormatting)rule1.CreateFontFormatting();
            fontFmt.SetFontStyle(true, false);

            HSSFPatternFormatting patternFmt = (HSSFPatternFormatting)rule1.CreatePatternFormatting();
            patternFmt.FillBackgroundColor = (HSSFColor.YELLOW.index);


            HSSFConditionalFormattingRule rule2 = (HSSFConditionalFormattingRule)sheetCF.CreateConditionalFormattingRule(ComparisonOperator.BETWEEN, "1", "2");
            HSSFConditionalFormattingRule[] cfRules =
		    {
			    rule1, rule2
		    };

            short col = 1;
            CellRangeAddress[] regions = {
			    new CellRangeAddress(0, 65535, col, col)
		    };

            sheetCF.AddConditionalFormatting(regions, cfRules);

            try
            {
                wb.CloneSheet(0);
            }
            catch (Exception e)
            {
                if (e.Message.IndexOf("needs to define a clone method") > 0)
                {
                    throw new AssertionException("Indentified bug 45682");
                }
                throw e;
            }
            Assert.AreEqual(2, wb.NumberOfSheets);
        }
        [Test]
        public void TestShiftRows()
        {

            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)wb.CreateSheet();

            HSSFSheetConditionalFormatting sheetCF = (HSSFSheetConditionalFormatting)sheet.SheetConditionalFormatting;

            HSSFConditionalFormattingRule rule1 = (HSSFConditionalFormattingRule)sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.BETWEEN, "sum(A10:A15)", "1+sum(B16:B30)");
            HSSFFontFormatting fontFmt = (HSSFFontFormatting)rule1.CreateFontFormatting();
            fontFmt.SetFontStyle(true, false);

            HSSFPatternFormatting patternFmt = (HSSFPatternFormatting)rule1.CreatePatternFormatting();
            patternFmt.FillBackgroundColor = (HSSFColor.YELLOW.index);
            HSSFConditionalFormattingRule[] cfRules = { rule1, };

            CellRangeAddress[] regions = {
			    new CellRangeAddress(2, 4, 0, 0), // A3:A5
		    };
            sheetCF.AddConditionalFormatting(regions, cfRules);

            // This row-shift should destroy the CF region
            sheet.ShiftRows(10, 20, -9);
            Assert.AreEqual(0, sheetCF.NumConditionalFormattings);

            // re-Add the CF
            sheetCF.AddConditionalFormatting(regions, cfRules);

            // This row shift should only affect the formulas
            sheet.ShiftRows(14, 17, 8);
            HSSFConditionalFormatting cf = (HSSFConditionalFormatting)sheetCF.GetConditionalFormattingAt(0);
            Assert.AreEqual("SUM(A10:A23)", cf.GetRule(0).Formula1);
            Assert.AreEqual("1+SUM(B24:B30)", cf.GetRule(0).Formula2);

            sheet.ShiftRows(0, 8, 21);
            cf = (HSSFConditionalFormatting)sheetCF.GetConditionalFormattingAt(0);
            Assert.AreEqual("SUM(A10:A21)", cf.GetRule(0).Formula1);
            Assert.AreEqual("1+SUM(#REF!)", cf.GetRule(0).Formula2);
        }
    }
}