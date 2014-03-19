﻿/* ====================================================================
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Model;
using TestCases.HSSF.Model;
using NPOI.Util;
using NPOI.HSSF.Record;

namespace TestCases.HSSF.UserModel
{
    /**
 * @author Evgeniy Berlog
 * @date 25.06.12
 */
    [TestFixture]
    public class TestText
    {
        [Test]
        public void TestResultEqualsToAbstractShape()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sh = wb.CreateSheet() as HSSFSheet;
            HSSFPatriarch patriarch = sh.CreateDrawingPatriarch() as HSSFPatriarch;
            HSSFTextbox textbox = patriarch.CreateTextbox(new HSSFClientAnchor()) as HSSFTextbox;
            TextboxShape textboxShape = HSSFTestModelHelper.CreateTextboxShape(1025, textbox);

            Assert.AreEqual(textbox.GetEscherContainer().ChildRecords.Count, 5);
            Assert.AreEqual(textboxShape.SpContainer.ChildRecords.Count, 5);

            //sp record
            byte[] expected = textboxShape.SpContainer.GetChild(0).Serialize();
            byte[] actual = textbox.GetEscherContainer().GetChild(0).Serialize();

            Assert.AreEqual(expected.Length, actual.Length);
            Assert.IsTrue(Arrays.Equals(expected, actual));

            expected = textboxShape.SpContainer.GetChild(2).Serialize();
            actual = textbox.GetEscherContainer().GetChild(2).Serialize();

            Assert.AreEqual(expected.Length, actual.Length);
            Assert.IsTrue(Arrays.Equals(expected, actual));

            expected = textboxShape.SpContainer.GetChild(3).Serialize();
            actual = textbox.GetEscherContainer().GetChild(3).Serialize();

            Assert.AreEqual(expected.Length, actual.Length);
            Assert.IsTrue(Arrays.Equals(expected, actual));

            expected = textboxShape.SpContainer.GetChild(4).Serialize();
            actual = textbox.GetEscherContainer().GetChild(4).Serialize();

            Assert.AreEqual(expected.Length, actual.Length);
            Assert.IsTrue(Arrays.Equals(expected, actual));

            ObjRecord obj = textbox.GetObjRecord();
            ObjRecord objShape = textboxShape.ObjRecord;

            expected = obj.Serialize();
            actual = objShape.Serialize();

            TextObjectRecord tor = textbox.GetTextObjectRecord();
            TextObjectRecord torShape = textboxShape.TextObjectRecord;

            expected = tor.Serialize();
            actual = torShape.Serialize();

            Assert.AreEqual(expected.Length, actual.Length);
            Assert.IsTrue(Arrays.Equals(expected, actual));
        }
        [Test]
        public void TestAddTextToExistingFile()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sh = wb.CreateSheet() as HSSFSheet;
            HSSFPatriarch patriarch = sh.CreateDrawingPatriarch() as HSSFPatriarch;
            HSSFTextbox textbox = patriarch.CreateTextbox(new HSSFClientAnchor()) as HSSFTextbox;
            textbox.String=(new HSSFRichTextString("just for Test"));
            HSSFTextbox textbox2 = patriarch.CreateTextbox(new HSSFClientAnchor()) as HSSFTextbox;
            textbox2.String=(new HSSFRichTextString("just for Test2"));

            Assert.AreEqual(patriarch.Children.Count, 2);

            wb = HSSFTestDataSamples.WriteOutAndReadBack(wb);
            sh = wb.GetSheetAt(0) as HSSFSheet;
            patriarch = sh.DrawingPatriarch as HSSFPatriarch;

            Assert.AreEqual(patriarch.Children.Count, 2);
            HSSFTextbox text3 = patriarch.CreateTextbox(new HSSFClientAnchor()) as HSSFTextbox;
            text3.String=(new HSSFRichTextString("text3"));
            Assert.AreEqual(patriarch.Children.Count, 3);

            wb = HSSFTestDataSamples.WriteOutAndReadBack(wb);
            sh = wb.GetSheetAt(0) as HSSFSheet;
            patriarch = sh.DrawingPatriarch as HSSFPatriarch;

            Assert.AreEqual(patriarch.Children.Count, 3);
            Assert.AreEqual(((HSSFTextbox)patriarch.Children[0]).String.String, "just for Test");
            Assert.AreEqual(((HSSFTextbox)patriarch.Children[1]).String.String, "just for Test2");
            Assert.AreEqual(((HSSFTextbox)patriarch.Children[2]).String.String, "text3");
        }
        [Test]
        public void TestSetGetProperties()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sh = wb.CreateSheet() as HSSFSheet;
            HSSFPatriarch patriarch = sh.CreateDrawingPatriarch() as HSSFPatriarch;
            HSSFTextbox textbox = patriarch.CreateTextbox(new HSSFClientAnchor()) as HSSFTextbox;
            textbox.String = (new HSSFRichTextString("test"));
            Assert.AreEqual(textbox.String.String, "test");

            textbox.HorizontalAlignment=((HorizontalAlignment)5);
            Assert.AreEqual((HorizontalAlignment)5, textbox.HorizontalAlignment);

            textbox.VerticalAlignment=((VerticalAlignment)6);
            Assert.AreEqual( (VerticalAlignment)6,textbox.VerticalAlignment);

            textbox.MarginBottom=(7);
            Assert.AreEqual(textbox.MarginBottom, 7);

            textbox.MarginLeft=(8);
            Assert.AreEqual(textbox.MarginLeft, 8);

            textbox.MarginRight=(9);
            Assert.AreEqual(textbox.MarginRight, 9);

            textbox.MarginTop=(10);
            Assert.AreEqual(textbox.MarginTop, 10);

            wb = HSSFTestDataSamples.WriteOutAndReadBack(wb);
            sh = wb.GetSheetAt(0) as HSSFSheet;
            patriarch = sh.DrawingPatriarch as HSSFPatriarch;
            textbox = (HSSFTextbox)patriarch.Children[0];
            Assert.AreEqual(textbox.String.String, "test");
            Assert.AreEqual(textbox.HorizontalAlignment, (HorizontalAlignment)5);
            Assert.AreEqual(textbox.VerticalAlignment, (VerticalAlignment)6);
            Assert.AreEqual(textbox.MarginBottom, 7);
            Assert.AreEqual(textbox.MarginLeft, 8);
            Assert.AreEqual(textbox.MarginRight, 9);
            Assert.AreEqual(textbox.MarginTop, 10);

            textbox.String = (new HSSFRichTextString("test1"));
            textbox.HorizontalAlignment = HorizontalAlignment.Center;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            textbox.MarginBottom = (71);
            textbox.MarginLeft = (81);
            textbox.MarginRight = (91);
            textbox.MarginTop = (101);

            Assert.AreEqual(textbox.String.String, "test1");
            Assert.AreEqual(textbox.HorizontalAlignment, HorizontalAlignment.Center);
            Assert.AreEqual(textbox.VerticalAlignment, VerticalAlignment.Top);
            Assert.AreEqual(textbox.MarginBottom, 71);
            Assert.AreEqual(textbox.MarginLeft, 81);
            Assert.AreEqual(textbox.MarginRight, 91);
            Assert.AreEqual(textbox.MarginTop, 101);

            wb = HSSFTestDataSamples.WriteOutAndReadBack(wb);
            sh = wb.GetSheetAt(0) as HSSFSheet;
            patriarch = sh.DrawingPatriarch as HSSFPatriarch;
            textbox = (HSSFTextbox)patriarch.Children[0];

            Assert.AreEqual(textbox.String.String, "test1");
            Assert.AreEqual(textbox.HorizontalAlignment, HorizontalAlignment.Center);
            Assert.AreEqual(textbox.VerticalAlignment, VerticalAlignment.Top);
            Assert.AreEqual(textbox.MarginBottom, 71);
            Assert.AreEqual(textbox.MarginLeft, 81);
            Assert.AreEqual(textbox.MarginRight, 91);
            Assert.AreEqual(textbox.MarginTop, 101);
        }
        [Test]
        public void TestExistingFileWithText()
        {
            HSSFWorkbook wb = HSSFTestDataSamples.OpenSampleWorkbook("drawings.xls");
            HSSFSheet sheet = wb.GetSheet("text") as HSSFSheet;
            HSSFPatriarch Drawing = sheet.DrawingPatriarch as HSSFPatriarch;
            Assert.AreEqual(1, Drawing.Children.Count);
            HSSFTextbox textbox = (HSSFTextbox)Drawing.Children[0];
            Assert.AreEqual(textbox.HorizontalAlignment, HorizontalAlignment.Left);
            Assert.AreEqual(textbox.VerticalAlignment, VerticalAlignment.Top);
            Assert.AreEqual(textbox.MarginTop, 0);
            Assert.AreEqual(textbox.MarginBottom, 3600000);
            Assert.AreEqual(textbox.MarginLeft, 3600000);
            Assert.AreEqual(textbox.MarginRight, 0);
            Assert.AreEqual(textbox.String.String, "teeeeesssstttt");
        }
    }

}
