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

using NPOI.XSSF.UserModel;
using NUnit.Framework;
using NPOI.SS.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
namespace NPOI.XSSF.Model
{

    [TestFixture]
    public class TestCalculationChain
    {
        [Test]
        public void Test46535()
        {
            XSSFWorkbook wb = XSSFTestDataSamples.OpenSampleWorkbook("46535.xlsx");

            CalculationChain chain = wb.GetCalculationChain();
            //the bean holding the reference to the formula to be deleted
            CT_CalcCell c = chain.GetCTCalcChain().GetCArray(0);
            int cnt = chain.GetCTCalcChain().c.Count;
            Assert.AreEqual(10, c.i);
            Assert.AreEqual("E1", c.r);

            ISheet sheet = wb.GetSheet("Test");
            ICell cell = sheet.GetRow(0).GetCell(4);

            Assert.AreEqual(CellType.FORMULA, cell.CellType);
            cell.SetCellFormula(null);

            //the count of items is less by one
            c = chain.GetCTCalcChain().GetCArray(0);
            int cnt2 = chain.GetCTCalcChain().c.Count;
            Assert.AreEqual(cnt - 1, cnt2);
            //the first item in the calculation chain is the former second one
            Assert.AreEqual(10, c.i);
            Assert.AreEqual("C1", c.r);

            Assert.AreEqual(CellType.STRING, cell.CellType);
            cell.SetCellValue("ABC");
            Assert.AreEqual(CellType.STRING, cell.CellType);
        }


    }
}

