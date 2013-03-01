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

using NPOI.SS.Util;
using NPOI.SS.UserModel;
using System;
using NUnit.Framework;
using NPOI.SS.UserModel.Charts;
namespace NPOI.XSSF.UserModel.Charts
{
    /**
     * Tests for XSSFScatterChartData.
     * @author Roman Kashitsyn
     */
    [TestFixture]
    public class TestXSSFScatterChartData
    {

        private static Object[][] plotData = new Object[][] {
	        new object[] {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J"},
	        new object[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
            };
        [Test]
        public void TestOneSeriePlot()
        {
            IWorkbook wb = new XSSFWorkbook();
            ISheet sheet = new SheetBuilder(wb, plotData).Build();
            IDrawing Drawing = sheet.CreateDrawingPatriarch();
            IClientAnchor anchor = Drawing.CreateAnchor(0, 0, 0, 0, 1, 1, 10, 30);
            IChart chart = Drawing.CreateChart(anchor);

            IChartAxis bottomAxis = chart.GetChartAxisFactory().CreateValueAxis(AxisPosition.BOTTOM);
            IChartAxis leftAxis = chart.GetChartAxisFactory().CreateValueAxis(AxisPosition.LEFT);

            IScatterChartData<string, double> scatterChartData =
                chart.GetChartDataFactory().CreateScatterChartData<string, double>();

            IChartDataSource<String> xs = DataSources.FromStringCellRange(sheet, CellRangeAddress.ValueOf("A1:J1"));
            IChartDataSource<double> ys = DataSources.FromNumericCellRange(sheet, CellRangeAddress.ValueOf("A2:J2"));
            IScatterChartSerie<string, double> serie = scatterChartData.AddSerie(xs, ys);

            Assert.IsNotNull(serie);

            Assert.AreEqual(1, scatterChartData.GetSeries().Count);
            Assert.IsTrue(scatterChartData.GetSeries().Contains(serie));

            chart.Plot(scatterChartData, bottomAxis, leftAxis);
        }

    }
}

