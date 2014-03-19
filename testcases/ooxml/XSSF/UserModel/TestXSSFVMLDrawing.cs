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
using NUnit.Framework;
using System.Collections.Generic;
using NPOI.OpenXmlFormats.Vml;
using NPOI.OpenXmlFormats.Vml.Office;
using System.IO;
using TestCases;
using System.Collections;
using NPOI.OpenXmlFormats.Vml.Spreadsheet;
namespace NPOI.XSSF.UserModel
{

    /**
     * @author Yegor Kozlov
     */
    [TestFixture]
    public class TestXSSFVMLDrawing
    {
        [Test]
        public void TestNew()
        {
            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            ArrayList items = vml.GetItems();
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items[0] is CT_ShapeLayout);
            CT_ShapeLayout layout = (CT_ShapeLayout)items[0];
            Assert.AreEqual(ST_Ext.edit, layout.ext);
            Assert.AreEqual(ST_Ext.edit, layout.idmap.ext);
            Assert.AreEqual("1", layout.idmap.data);

            Assert.IsTrue(items[1] is CT_Shapetype);
            CT_Shapetype type = (CT_Shapetype)items[1];
            Assert.AreEqual("21600,21600", type.coordsize);
            Assert.AreEqual(202.0f, type.spt);
            Assert.AreEqual("m,l,21600r21600,l21600,xe", type.path2);
            Assert.AreEqual("_xssf_cell_comment", type.id);
            Assert.AreEqual(ST_TrueFalse.t, type.path.gradientshapeok);
            Assert.AreEqual(ST_ConnectType.rect, type.path.connecttype);

            CT_Shape shape = vml.newCommentShape();
            Assert.AreEqual(3, items.Count);
            Assert.AreSame(items[2], shape);
            Assert.AreEqual("#_xssf_cell_comment", shape.type);
            Assert.AreEqual("position:absolute; visibility:hidden", shape.style);
            Assert.AreEqual("#ffffe1", shape.fillcolor);
            Assert.AreEqual(ST_InsetMode.auto, shape.insetmode);
            Assert.AreEqual("#ffffe1", shape.fill.color);
            CT_Shadow shadow = shape.shadow;
            Assert.AreEqual(ST_TrueFalse.t, shadow.on);
            Assert.AreEqual("black", shadow.color);
            Assert.AreEqual(ST_TrueFalse.t, shadow.obscured);
            Assert.AreEqual(ST_ConnectType.none, shape.path.connecttype);
            Assert.AreEqual("mso-direction-alt:auto", shape.textbox.style);
            CT_ClientData cldata = shape.GetClientDataArray(0);
            Assert.AreEqual(ST_ObjectType.Note, cldata.ObjectType);
            Assert.AreEqual(1, cldata.SizeOfMoveWithCellsArray());
            Assert.AreEqual(1, cldata.SizeOfSizeWithCellsArray());
            Assert.AreEqual("1, 15, 0, 2, 3, 15, 3, 16", cldata.anchor);
            Assert.AreEqual(ST_TrueFalseBlank.@false, cldata.autoFill);
            Assert.AreEqual(0, cldata.GetRowArray(0));
            Assert.AreEqual(0, cldata.GetColumnArray(0));

            //serialize and read again
            MemoryStream out1 = new MemoryStream();
            vml.Write(out1);

            XSSFVMLDrawing vml2 = new XSSFVMLDrawing();
            vml2.Read(new MemoryStream(out1.ToArray()));
            ArrayList items2 = vml2.GetItems();
            Assert.AreEqual(3, items2.Count);
            Assert.IsTrue(items2[0] is CT_ShapeLayout);
            Assert.IsTrue(items2[1] is CT_Shapetype);
            Assert.IsTrue(items2[2] is CT_Shape);
        }
        [Test]
        public void TestFindCommentShape()
        {

            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            vml.Read(POIDataSamples.GetSpreadSheetInstance().OpenResourceAsStream("vmlDrawing1.vml"));

            CT_Shape sh_a1 = vml.FindCommentShape(0, 0);
            Assert.IsNotNull(sh_a1);
            Assert.AreEqual("_x0000_s1025", sh_a1.id);

            CT_Shape sh_b1 = vml.FindCommentShape(0, 1);
            Assert.IsNotNull(sh_b1);
            Assert.AreEqual("_x0000_s1026", sh_b1.id);

            CT_Shape sh_c1 = vml.FindCommentShape(0, 2);
            Assert.IsNull(sh_c1);

            CT_Shape sh_d1 = vml.newCommentShape();
            Assert.AreEqual("_x0000_s1027", sh_d1.id);
            sh_d1.GetClientDataArray(0).SetRowArray(0, 0);
            sh_d1.GetClientDataArray(0).SetColumnArray(0, 3);
            Assert.AreSame(sh_d1, vml.FindCommentShape(0, 3));

            //newly created drawing
            XSSFVMLDrawing newVml = new XSSFVMLDrawing();
            Assert.IsNull(newVml.FindCommentShape(0, 0));

            sh_a1 = newVml.newCommentShape();
            Assert.AreEqual("_x0000_s1025", sh_a1.id);
            sh_a1.GetClientDataArray(0).SetRowArray(0, 0);
            sh_a1.GetClientDataArray(0).SetColumnArray(0, 1);
            Assert.AreSame(sh_a1, newVml.FindCommentShape(0, 1));
        }
        [Test]
        public void TestRemoveCommentShape()
        {
            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            vml.Read(POIDataSamples.GetSpreadSheetInstance().OpenResourceAsStream("vmlDrawing1.vml"));

            CT_Shape sh_a1 = vml.FindCommentShape(0, 0);
            Assert.IsNotNull(sh_a1);

            Assert.IsTrue(vml.RemoveCommentShape(0, 0));
            Assert.IsNull(vml.FindCommentShape(0, 0));

        }
    }
}
