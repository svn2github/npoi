/*
 *  ====================================================================
 *    Licensed to the Apache Software Foundation (ASF) under one or more
 *    contributor license agreements.  See the NOTICE file distributed with
 *    this work for Additional information regarding copyright ownership.
 *    The ASF licenses this file to You under the Apache License, Version 2.0
 *    (the "License"); you may not use this file except in compliance with
 *    the License.  You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 * ====================================================================
 */

namespace TestCases.SS.UserModel
{
    using System;

    using NUnit.Framework;
    using NPOI.SS;
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;
    using TestCases.SS;
    using NPOI.HSSF.Record.CF;

    /**
     * @author Dmitriy Kumshayev
     * @author Yegor Kozlov
     */
    [TestFixture]
    public class BaseTestConditionalFormatting
    {
        private ITestDataProvider _testDataProvider;
        public BaseTestConditionalFormatting()
        {
            _testDataProvider = TestCases.HSSF.HSSFITestDataProvider.Instance;
        }
        public BaseTestConditionalFormatting(ITestDataProvider TestDataProvider)
        {
            _testDataProvider = TestDataProvider;
        }
        [Test]
        public void TestBasic()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sh = wb.CreateSheet();
            ISheetConditionalFormatting sheetCF = sh.SheetConditionalFormatting;

            Assert.AreEqual(0, sheetCF.NumConditionalFormattings);
            try
            {
                Assert.IsNull(sheetCF.GetConditionalFormattingAt(0));
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Specified CF index 0 is outside the allowable range"));
            }

            try
            {
                sheetCF.RemoveConditionalFormatting(0);
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Specified CF index 0 is outside the allowable range"));
            }

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule("1");
            IConditionalFormattingRule rule2 = sheetCF.CreateConditionalFormattingRule("2");
            IConditionalFormattingRule rule3 = sheetCF.CreateConditionalFormattingRule("3");
            IConditionalFormattingRule rule4 = sheetCF.CreateConditionalFormattingRule("4");
            try
            {
                sheetCF.AddConditionalFormatting(null, rule1);
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("regions must not be null"));
            }
            try
            {
                sheetCF.AddConditionalFormatting(
                        new CellRangeAddress[] { CellRangeAddress.ValueOf("A1:A3") },
                        (IConditionalFormattingRule)null);
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("cfRules must not be null"));
            }

            try
            {
                sheetCF.AddConditionalFormatting(
                        new CellRangeAddress[] { CellRangeAddress.ValueOf("A1:A3") },
                        new IConditionalFormattingRule[0]);
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("cfRules must not be empty"));
            }

            try
            {
                sheetCF.AddConditionalFormatting(
                        new CellRangeAddress[] { CellRangeAddress.ValueOf("A1:A3") },
                        new IConditionalFormattingRule[] { rule1, rule2, rule3, rule4 });
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Number of rules must not exceed 3"));
            }
        }

        /**
         * Test format conditions based on a bool formula
         */
        [Test]
        public void TestBooleanFormulaConditions()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sh = wb.CreateSheet();
            ISheetConditionalFormatting sheetCF = sh.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule("SUM(A1:A5)>10");
            Assert.AreEqual(ConditionType.FORMULA, rule1.ConditionType);
            Assert.AreEqual("SUM(A1:A5)>10", rule1.Formula1);
            int formatIndex1 = sheetCF.AddConditionalFormatting(
                    new CellRangeAddress[]{
                        CellRangeAddress.ValueOf("B1"),
                        CellRangeAddress.ValueOf("C3"),
                }, rule1);
            Assert.AreEqual(0, formatIndex1);
            Assert.AreEqual(1, sheetCF.NumConditionalFormattings);
            CellRangeAddress[] ranges1 = sheetCF.GetConditionalFormattingAt(formatIndex1).GetFormattingRanges();
            Assert.AreEqual(2, ranges1.Length);
            Assert.AreEqual("B1", ranges1[0].FormatAsString());
            Assert.AreEqual("C3", ranges1[1].FormatAsString());

            // adjacent Address are merged
            int formatIndex2 = sheetCF.AddConditionalFormatting(
                    new CellRangeAddress[]{
                        CellRangeAddress.ValueOf("B1"),
                        CellRangeAddress.ValueOf("B2"),
                        CellRangeAddress.ValueOf("B3"),
                }, rule1);
            Assert.AreEqual(1, formatIndex2);
            Assert.AreEqual(2, sheetCF.NumConditionalFormattings);
            CellRangeAddress[] ranges2 = sheetCF.GetConditionalFormattingAt(formatIndex2).GetFormattingRanges();
            Assert.AreEqual(1, ranges2.Length);
            Assert.AreEqual("B1:B3", ranges2[0].FormatAsString());
        }
        [Test]
        public void TestSingleFormulaConditions()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sh = wb.CreateSheet();
            ISheetConditionalFormatting sheetCF = sh.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.EQUAL, "SUM(A1:A5)+10");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule1.ConditionType);
            Assert.AreEqual("SUM(A1:A5)+10", rule1.Formula1);
            Assert.AreEqual(ComparisonOperator.EQUAL, rule1.ComparisonOperation);

            IConditionalFormattingRule rule2 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.NOT_EQUAL, "15");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule2.ConditionType);
            Assert.AreEqual("15", rule2.Formula1);
            Assert.AreEqual(ComparisonOperator.NOT_EQUAL, rule2.ComparisonOperation);

            IConditionalFormattingRule rule3 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.NOT_EQUAL, "15");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule3.ConditionType);
            Assert.AreEqual("15", rule3.Formula1);
            Assert.AreEqual(ComparisonOperator.NOT_EQUAL, rule3.ComparisonOperation);

            IConditionalFormattingRule rule4 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.GT, "0");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule4.ConditionType);
            Assert.AreEqual("0", rule4.Formula1);
            Assert.AreEqual(ComparisonOperator.GT, rule4.ComparisonOperation);

            IConditionalFormattingRule rule5 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.LT, "0");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule5.ConditionType);
            Assert.AreEqual("0", rule5.Formula1);
            Assert.AreEqual(ComparisonOperator.LT, rule5.ComparisonOperation);

            IConditionalFormattingRule rule6 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.GE, "0");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule6.ConditionType);
            Assert.AreEqual("0", rule6.Formula1);
            Assert.AreEqual(ComparisonOperator.GE, rule6.ComparisonOperation);

            IConditionalFormattingRule rule7 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.LE, "0");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule7.ConditionType);
            Assert.AreEqual("0", rule7.Formula1);
            Assert.AreEqual(ComparisonOperator.LE, rule7.ComparisonOperation);

            IConditionalFormattingRule rule8 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.BETWEEN, "0", "5");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule8.ConditionType);
            Assert.AreEqual("0", rule8.Formula1);
            Assert.AreEqual("5", rule8.Formula2);
            Assert.AreEqual(ComparisonOperator.BETWEEN, rule8.ComparisonOperation);

            IConditionalFormattingRule rule9 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.NOT_BETWEEN, "0", "5");
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule9.ConditionType);
            Assert.AreEqual("0", rule9.Formula1);
            Assert.AreEqual("5", rule9.Formula2);
            Assert.AreEqual(ComparisonOperator.NOT_BETWEEN, rule9.ComparisonOperation);
        }
        [Test]
        public void TestCopy()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sheet1 = wb.CreateSheet();
            ISheet sheet2 = wb.CreateSheet();
            ISheetConditionalFormatting sheet1CF = sheet1.SheetConditionalFormatting;
            ISheetConditionalFormatting sheet2CF = sheet2.SheetConditionalFormatting;
            Assert.AreEqual(0, sheet1CF.NumConditionalFormattings);
            Assert.AreEqual(0, sheet2CF.NumConditionalFormattings);

            IConditionalFormattingRule rule1 = sheet1CF.CreateConditionalFormattingRule(
                    ComparisonOperator.EQUAL, "SUM(A1:A5)+10");

            IConditionalFormattingRule rule2 = sheet1CF.CreateConditionalFormattingRule(
                    ComparisonOperator.NOT_EQUAL, "15");

            // adjacent Address are merged
            int formatIndex = sheet1CF.AddConditionalFormatting(
                    new CellRangeAddress[]{
                        CellRangeAddress.ValueOf("A1:A5"),
                        CellRangeAddress.ValueOf("C1:C5")
                }, rule1, rule2);
            Assert.AreEqual(0, formatIndex);
            Assert.AreEqual(1, sheet1CF.NumConditionalFormattings);

            Assert.AreEqual(0, sheet2CF.NumConditionalFormattings);
            sheet2CF.AddConditionalFormatting(sheet1CF.GetConditionalFormattingAt(formatIndex));
            Assert.AreEqual(1, sheet2CF.NumConditionalFormattings);

            IConditionalFormatting sheet2cf = sheet2CF.GetConditionalFormattingAt(0);
            Assert.AreEqual(2, sheet2cf.NumberOfRules);
            Assert.AreEqual("SUM(A1:A5)+10", sheet2cf.GetRule(0).Formula1);
            Assert.AreEqual(ComparisonOperator.EQUAL, sheet2cf.GetRule(0).ComparisonOperation);
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, sheet2cf.GetRule(0).ConditionType);
            Assert.AreEqual("15", sheet2cf.GetRule(1).Formula1);
            Assert.AreEqual(ComparisonOperator.NOT_EQUAL, sheet2cf.GetRule(1).ComparisonOperation);
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, sheet2cf.GetRule(1).ConditionType);
        }
        [Test]
        public void TestRemove()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sheet1 = wb.CreateSheet();
            ISheetConditionalFormatting sheetCF = sheet1.SheetConditionalFormatting;
            Assert.AreEqual(0, sheetCF.NumConditionalFormattings);

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.EQUAL, "SUM(A1:A5)");

            // adjacent Address are merged
            int formatIndex = sheetCF.AddConditionalFormatting(
                    new CellRangeAddress[]{
                        CellRangeAddress.ValueOf("A1:A5")
                }, rule1);
            Assert.AreEqual(0, formatIndex);
            Assert.AreEqual(1, sheetCF.NumConditionalFormattings);
            sheetCF.RemoveConditionalFormatting(0);
            Assert.AreEqual(0, sheetCF.NumConditionalFormattings);
            try
            {
                Assert.IsNull(sheetCF.GetConditionalFormattingAt(0));
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Specified CF index 0 is outside the allowable range"));
            }

            formatIndex = sheetCF.AddConditionalFormatting(
                    new CellRangeAddress[]{
                        CellRangeAddress.ValueOf("A1:A5")
                }, rule1);
            Assert.AreEqual(0, formatIndex);
            Assert.AreEqual(1, sheetCF.NumConditionalFormattings);
            sheetCF.RemoveConditionalFormatting(0);
            Assert.AreEqual(0, sheetCF.NumConditionalFormattings);
            try
            {
                Assert.IsNull(sheetCF.GetConditionalFormattingAt(0));
                Assert.Fail("expected exception");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Specified CF index 0 is outside the allowable range"));
            }
        }
        [Test]
        public void TestCreateCF()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();
            String formula = "7";

            ISheetConditionalFormatting sheetCF = sheet.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(formula);
            IFontFormatting fontFmt = rule1.CreateFontFormatting();
            fontFmt.SetFontStyle(true, false);

            IBorderFormatting bordFmt = rule1.CreateBorderFormatting();
            bordFmt.BorderBottom = (/*setter*/BorderFormatting.BORDER_THIN);
            bordFmt.BorderTop = (/*setter*/BorderFormatting.BORDER_THICK);
            bordFmt.BorderLeft = (/*setter*/BorderFormatting.BORDER_DASHED);
            bordFmt.BorderRight = (/*setter*/BorderFormatting.BORDER_DOTTED);

            IPatternFormatting patternFmt = rule1.CreatePatternFormatting();
            patternFmt.FillBackgroundColor = (/*setter*/IndexedColors.YELLOW.Index);


            IConditionalFormattingRule rule2 = sheetCF.CreateConditionalFormattingRule(ComparisonOperator.BETWEEN, "1", "2");
            IConditionalFormattingRule[] cfRules =
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
            IConditionalFormatting cf = sheetCF.GetConditionalFormattingAt(0);
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

            rule1 = cf.GetRule(0);
            Assert.AreEqual("7", rule1.Formula1);
            Assert.IsNull(rule1.Formula2);

            IFontFormatting r1fp = rule1.GetFontFormatting();
            Assert.IsNotNull(r1fp);

            Assert.IsTrue(r1fp.IsItalic);
            Assert.IsFalse(r1fp.IsBold);

            IBorderFormatting r1bf = rule1.GetBorderFormatting();
            Assert.IsNotNull(r1bf);
            Assert.AreEqual(BorderFormatting.BORDER_THIN, r1bf.BorderBottom);
            Assert.AreEqual(BorderFormatting.BORDER_THICK, r1bf.BorderTop);
            Assert.AreEqual(BorderFormatting.BORDER_DASHED, r1bf.BorderLeft);
            Assert.AreEqual(BorderFormatting.BORDER_DOTTED, r1bf.BorderRight);

            IPatternFormatting r1pf = rule1.GetPatternFormatting();
            Assert.IsNotNull(r1pf);
            //        Assert.AreEqual(IndexedColors.YELLOW.index,r1pf.FillBackgroundColor);

            rule2 = cf.GetRule(1);
            Assert.AreEqual("2", rule2.Formula2);
            Assert.AreEqual("1", rule2.Formula1);
        }
        [Test]
        public void TestClone()
        {

            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sheet = wb.CreateSheet();
            String formula = "7";

            ISheetConditionalFormatting sheetCF = sheet.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(formula);
            IFontFormatting fontFmt = rule1.CreateFontFormatting();
            fontFmt.SetFontStyle(true, false);

            IPatternFormatting patternFmt = rule1.CreatePatternFormatting();
            patternFmt.FillBackgroundColor = (/*setter*/IndexedColors.YELLOW.Index);


            IConditionalFormattingRule rule2 = sheetCF.CreateConditionalFormattingRule(ComparisonOperator.BETWEEN, "1", "2");
            IConditionalFormattingRule[] cfRules =
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
                    Assert.Fail("Indentified bug 45682");
                }
                throw e;
            }
            Assert.AreEqual(2, wb.NumberOfSheets);
        }
        [Test]
        public void TestShiftRows()
        {

            IWorkbook wb = _testDataProvider.CreateWorkbook();
            ISheet sheet = wb.CreateSheet();

            ISheetConditionalFormatting sheetCF = sheet.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(
                    ComparisonOperator.BETWEEN, "SUM(A10:A15)", "1+SUM(B16:B30)");
            IFontFormatting fontFmt = rule1.CreateFontFormatting();
            fontFmt.SetFontStyle(true, false);

            IPatternFormatting patternFmt = rule1.CreatePatternFormatting();
            patternFmt.FillBackgroundColor = (/*setter*/IndexedColors.YELLOW.Index);
            IConditionalFormattingRule[] cfRules = { rule1, };

            CellRangeAddress[] regions = {
            new CellRangeAddress(2, 4, 0, 0), // A3:A5
        };
            sheetCF.AddConditionalFormatting(regions, cfRules);

            // This row-shift should destroy the CF region
            sheet.ShiftRows(10, 20, -9);
            Assert.AreEqual(0, sheetCF.NumConditionalFormattings);

            // re-add the CF
            sheetCF.AddConditionalFormatting(regions, cfRules);

            // This row shift should only affect the formulas
            sheet.ShiftRows(14, 17, 8);
            IConditionalFormatting cf = sheetCF.GetConditionalFormattingAt(0);
            Assert.AreEqual("SUM(A10:A23)", cf.GetRule(0).Formula1);
            Assert.AreEqual("1+SUM(B24:B30)", cf.GetRule(0).Formula2);

            sheet.ShiftRows(0, 8, 21);
            cf = sheetCF.GetConditionalFormattingAt(0);
            Assert.AreEqual("SUM(A10:A21)", cf.GetRule(0).Formula1);
            Assert.AreEqual("1+SUM(#REF!)", cf.GetRule(0).Formula2);
        }
        //
        [Test]
        public void TestRead()
        {

            IWorkbook wb = _testDataProvider.OpenSampleWorkbook("WithConditionalFormatting.xls");
            ISheet sh = wb.GetSheet("CF");
            ISheetConditionalFormatting sheetCF = sh.SheetConditionalFormatting;
            Assert.AreEqual(3, sheetCF.NumConditionalFormattings);

            IConditionalFormatting cf1 = sheetCF.GetConditionalFormattingAt(0);
            Assert.AreEqual(2, cf1.NumberOfRules);

            CellRangeAddress[] regions1 = cf1.GetFormattingRanges();
            Assert.AreEqual(1, regions1.Length);
            Assert.AreEqual("A1:A8", regions1[0].FormatAsString());

            // CF1 has two rules: values less than -3 are bold-italic red, values greater than 3 are green
            IConditionalFormattingRule rule1 = cf1.GetRule(0);
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule1.ConditionType);
            Assert.AreEqual(ComparisonOperator.GT, rule1.ComparisonOperation);
            Assert.AreEqual("3", rule1.Formula1);
            Assert.IsNull(rule1.Formula2);
            // Fills and borders are not Set
            Assert.IsNull(rule1.GetPatternFormatting());
            Assert.IsNull(rule1.GetBorderFormatting());

            IFontFormatting fmt1 = rule1.GetFontFormatting();
            //        Assert.AreEqual(IndexedColors.GREEN.index, fmt1.FontColorIndex);
            Assert.IsTrue(fmt1.IsBold);
            Assert.IsFalse(fmt1.IsItalic);

            IConditionalFormattingRule rule2 = cf1.GetRule(1);
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule2.ConditionType);
            Assert.AreEqual(ComparisonOperator.LT, rule2.ComparisonOperation);
            Assert.AreEqual("-3", rule2.Formula1);
            Assert.IsNull(rule2.Formula2);
            Assert.IsNull(rule2.GetPatternFormatting());
            Assert.IsNull(rule2.GetBorderFormatting());

            IFontFormatting fmt2 = rule2.GetFontFormatting();
            //        Assert.AreEqual(IndexedColors.RED.index, fmt2.FontColorIndex);
            Assert.IsTrue(fmt2.IsBold);
            Assert.IsTrue(fmt2.IsItalic);


            IConditionalFormatting cf2 = sheetCF.GetConditionalFormattingAt(1);
            Assert.AreEqual(1, cf2.NumberOfRules);
            CellRangeAddress[] regions2 = cf2.GetFormattingRanges();
            Assert.AreEqual(1, regions2.Length);
            Assert.AreEqual("B9", regions2[0].FormatAsString());

            IConditionalFormattingRule rule3 = cf2.GetRule(0);
            Assert.AreEqual(ConditionType.FORMULA, rule3.ConditionType);
            Assert.AreEqual(ComparisonOperator.NO_COMPARISON, rule3.ComparisonOperation);
            Assert.AreEqual("$A$8>5", rule3.Formula1);
            Assert.IsNull(rule3.Formula2);

            IFontFormatting fmt3 = rule3.GetFontFormatting();
            //        Assert.AreEqual(IndexedColors.RED.index, fmt3.FontColorIndex);
            Assert.IsTrue(fmt3.IsBold);
            Assert.IsTrue(fmt3.IsItalic);

            IPatternFormatting fmt4 = rule3.GetPatternFormatting();
            //        Assert.AreEqual(IndexedColors.LIGHT_CORNFLOWER_BLUE.index, fmt4.FillBackgroundColor);
            //        Assert.AreEqual(IndexedColors.AUTOMATIC.index, fmt4.FillForegroundColor);
            Assert.AreEqual(PatternFormatting.NO_Fill, fmt4.FillPattern);
            // borders are not Set
            Assert.IsNull(rule3.GetBorderFormatting());

            IConditionalFormatting cf3 = sheetCF.GetConditionalFormattingAt(2);
            CellRangeAddress[] regions3 = cf3.GetFormattingRanges();
            Assert.AreEqual(1, regions3.Length);
            Assert.AreEqual("B1:B7", regions3[0].FormatAsString());
            Assert.AreEqual(2, cf3.NumberOfRules);

            IConditionalFormattingRule rule4 = cf3.GetRule(0);
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule4.ConditionType);
            Assert.AreEqual(ComparisonOperator.LE, rule4.ComparisonOperation);
            Assert.AreEqual("\"AAA\"", rule4.Formula1);
            Assert.IsNull(rule4.Formula2);

            IConditionalFormattingRule rule5 = cf3.GetRule(1);
            Assert.AreEqual(ConditionType.CELL_VALUE_IS, rule5.ConditionType);
            Assert.AreEqual(ComparisonOperator.BETWEEN, rule5.ComparisonOperation);
            Assert.AreEqual("\"A\"", rule5.Formula1);
            Assert.AreEqual("\"AAA\"", rule5.Formula2);
        }

        [Test]
        public void TestCreateFontFormatting()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();

            ISheetConditionalFormatting sheetCF = sheet.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(ComparisonOperator.EQUAL, "7");
            IFontFormatting fontFmt = rule1.CreateFontFormatting();
            Assert.IsFalse(fontFmt.IsItalic);
            Assert.IsFalse(fontFmt.IsBold);
            fontFmt.SetFontStyle(true, true);
            Assert.IsTrue(fontFmt.IsItalic);
            Assert.IsTrue(fontFmt.IsBold);

            Assert.AreEqual(-1, fontFmt.FontHeight); // not modified
            fontFmt.FontHeight = (/*setter*/200);
            Assert.AreEqual(200, fontFmt.FontHeight);
            fontFmt.FontHeight = (/*setter*/100);
            Assert.AreEqual(100, fontFmt.FontHeight);

            Assert.AreEqual(FontFormatting.SS_NONE, (short)fontFmt.EscapementType);
            fontFmt.EscapementType = (/*setter*/FontSuperScript.SUB);
            Assert.AreEqual(FontFormatting.SS_SUB, (short)fontFmt.EscapementType);
            fontFmt.EscapementType = (/*setter*/FontSuperScript.NONE);
            Assert.AreEqual(FontFormatting.SS_NONE, (short)fontFmt.EscapementType);
            fontFmt.EscapementType = (/*setter*/FontSuperScript.SUPER);
            Assert.AreEqual(FontFormatting.SS_SUPER, (short)fontFmt.EscapementType);

            Assert.AreEqual(FontFormatting.U_NONE, (byte)fontFmt.UnderlineType);
            fontFmt.UnderlineType = (/*setter*/FontUnderlineType.SINGLE);
            Assert.AreEqual(FontFormatting.U_SINGLE, (byte)fontFmt.UnderlineType);
            fontFmt.UnderlineType = (/*setter*/FontUnderlineType.NONE);
            Assert.AreEqual(FontFormatting.U_NONE, (byte)fontFmt.UnderlineType);
            fontFmt.UnderlineType = (/*setter*/FontUnderlineType.DOUBLE);
            Assert.AreEqual(FontFormatting.U_DOUBLE, (byte)fontFmt.UnderlineType);

            Assert.AreEqual(-1, fontFmt.FontColorIndex);
            fontFmt.FontColorIndex = (/*setter*/IndexedColors.RED.Index);
            Assert.AreEqual(IndexedColors.RED.Index, fontFmt.FontColorIndex);
            fontFmt.FontColorIndex = (/*setter*/IndexedColors.AUTOMATIC.Index);
            Assert.AreEqual(IndexedColors.AUTOMATIC.Index, fontFmt.FontColorIndex);
            fontFmt.FontColorIndex = (/*setter*/IndexedColors.BLUE.Index);
            Assert.AreEqual(IndexedColors.BLUE.Index, fontFmt.FontColorIndex);

            IConditionalFormattingRule[] cfRules = { rule1 };

            CellRangeAddress[] regions = { CellRangeAddress.ValueOf("A1:A5") };

            sheetCF.AddConditionalFormatting(regions, cfRules);

            // Verification
            IConditionalFormatting cf = sheetCF.GetConditionalFormattingAt(0);
            Assert.IsNotNull(cf);

            Assert.AreEqual(1, cf.NumberOfRules);

            IFontFormatting r1fp = cf.GetRule(0).GetFontFormatting();
            Assert.IsNotNull(r1fp);

            Assert.IsTrue(r1fp.IsItalic);
            Assert.IsTrue(r1fp.IsBold);
            Assert.AreEqual(FontFormatting.SS_SUPER, (short)r1fp.EscapementType);
            Assert.AreEqual(FontFormatting.U_DOUBLE, (short)r1fp.UnderlineType);
            Assert.AreEqual(IndexedColors.BLUE.Index, r1fp.FontColorIndex);

        }
        [Test]
        public void TestCreatePatternFormatting()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();

            ISheetConditionalFormatting sheetCF = sheet.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(ComparisonOperator.EQUAL, "7");
            IPatternFormatting patternFmt = rule1.CreatePatternFormatting();

            Assert.AreEqual(0, patternFmt.FillBackgroundColor);
            patternFmt.FillBackgroundColor = (/*setter*/IndexedColors.RED.Index);
            Assert.AreEqual(IndexedColors.RED.Index, patternFmt.FillBackgroundColor);

            Assert.AreEqual(0, patternFmt.FillForegroundColor);
            patternFmt.FillForegroundColor = (/*setter*/IndexedColors.BLUE.Index);
            Assert.AreEqual(IndexedColors.BLUE.Index, patternFmt.FillForegroundColor);

            Assert.AreEqual(PatternFormatting.NO_Fill, patternFmt.FillPattern);
            patternFmt.FillPattern = (/*setter*/PatternFormatting.SOLID_FOREGROUND);
            Assert.AreEqual(PatternFormatting.SOLID_FOREGROUND, patternFmt.FillPattern);
            patternFmt.FillPattern = (/*setter*/PatternFormatting.NO_Fill);
            Assert.AreEqual(PatternFormatting.NO_Fill, patternFmt.FillPattern);
            patternFmt.FillPattern = (/*setter*/PatternFormatting.BRICKS);
            Assert.AreEqual(PatternFormatting.BRICKS, patternFmt.FillPattern);

            IConditionalFormattingRule[] cfRules = { rule1 };

            CellRangeAddress[] regions = { CellRangeAddress.ValueOf("A1:A5") };

            sheetCF.AddConditionalFormatting(regions, cfRules);

            // Verification
            IConditionalFormatting cf = sheetCF.GetConditionalFormattingAt(0);
            Assert.IsNotNull(cf);

            Assert.AreEqual(1, cf.NumberOfRules);

            IPatternFormatting r1fp = cf.GetRule(0).GetPatternFormatting();
            Assert.IsNotNull(r1fp);

            Assert.AreEqual(IndexedColors.RED.Index, r1fp.FillBackgroundColor);
            Assert.AreEqual(IndexedColors.BLUE.Index, r1fp.FillForegroundColor);
            Assert.AreEqual(PatternFormatting.BRICKS, r1fp.FillPattern);
        }
        [Test]
        public void TestCreateBorderFormatting()
        {
            IWorkbook workbook = _testDataProvider.CreateWorkbook();
            ISheet sheet = workbook.CreateSheet();

            ISheetConditionalFormatting sheetCF = sheet.SheetConditionalFormatting;

            IConditionalFormattingRule rule1 = sheetCF.CreateConditionalFormattingRule(ComparisonOperator.EQUAL, "7");
            IBorderFormatting borderFmt = rule1.CreateBorderFormatting();

            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderBottom);
            borderFmt.BorderBottom = (/*setter*/BorderFormatting.BORDER_DOTTED);
            Assert.AreEqual(BorderFormatting.BORDER_DOTTED, borderFmt.BorderBottom);
            borderFmt.BorderBottom = (/*setter*/BorderFormatting.BORDER_NONE);
            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderBottom);
            borderFmt.BorderBottom = (/*setter*/BorderFormatting.BORDER_THICK);
            Assert.AreEqual(BorderFormatting.BORDER_THICK, borderFmt.BorderBottom);

            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderTop);
            borderFmt.BorderTop = (/*setter*/BorderFormatting.BORDER_DOTTED);
            Assert.AreEqual(BorderFormatting.BORDER_DOTTED, borderFmt.BorderTop);
            borderFmt.BorderTop = (/*setter*/BorderFormatting.BORDER_NONE);
            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderTop);
            borderFmt.BorderTop = (/*setter*/BorderFormatting.BORDER_THICK);
            Assert.AreEqual(BorderFormatting.BORDER_THICK, borderFmt.BorderTop);

            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderLeft);
            borderFmt.BorderLeft = (/*setter*/BorderFormatting.BORDER_DOTTED);
            Assert.AreEqual(BorderFormatting.BORDER_DOTTED, borderFmt.BorderLeft);
            borderFmt.BorderLeft = (/*setter*/BorderFormatting.BORDER_NONE);
            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderLeft);
            borderFmt.BorderLeft = (/*setter*/BorderFormatting.BORDER_THIN);
            Assert.AreEqual(BorderFormatting.BORDER_THIN, borderFmt.BorderLeft);

            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderRight);
            borderFmt.BorderRight = (/*setter*/BorderFormatting.BORDER_DOTTED);
            Assert.AreEqual(BorderFormatting.BORDER_DOTTED, borderFmt.BorderRight);
            borderFmt.BorderRight = (/*setter*/BorderFormatting.BORDER_NONE);
            Assert.AreEqual(BorderFormatting.BORDER_NONE, borderFmt.BorderRight);
            borderFmt.BorderRight = (/*setter*/BorderFormatting.BORDER_HAIR);
            Assert.AreEqual(BorderFormatting.BORDER_HAIR, borderFmt.BorderRight);

            IConditionalFormattingRule[] cfRules = { rule1 };

            CellRangeAddress[] regions = { CellRangeAddress.ValueOf("A1:A5") };

            sheetCF.AddConditionalFormatting(regions, cfRules);

            // Verification
            IConditionalFormatting cf = sheetCF.GetConditionalFormattingAt(0);
            Assert.IsNotNull(cf);

            Assert.AreEqual(1, cf.NumberOfRules);

            IBorderFormatting r1fp = cf.GetRule(0).GetBorderFormatting();
            Assert.IsNotNull(r1fp);
            Assert.AreEqual(BorderFormatting.BORDER_THICK, r1fp.BorderBottom);
            Assert.AreEqual(BorderFormatting.BORDER_THICK, r1fp.BorderTop);
            Assert.AreEqual(BorderFormatting.BORDER_THIN, r1fp.BorderLeft);
            Assert.AreEqual(BorderFormatting.BORDER_HAIR, r1fp.BorderRight);

        }
    }

}