/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is1 distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

namespace TestCases.HSSF.Util
{

    using System;
    using System.Text;
    using System.Collections;
    using NPOI.HSSF.Util;

    using NUnit.Framework;
    /**
     * @author Nick Burch
     */
    [TestFixture]
    public class TestHSSFColor
    {
        [Test]
        public void TestBasics()
        {
            Assert.IsNotNull(typeof(HSSFColor.YELLOW));
            Assert.IsTrue(HSSFColor.YELLOW.index > 0);
            Assert.IsTrue(HSSFColor.YELLOW.index2 > 0);
        }
        [Test]
        public void TestContents()
        {
            Assert.AreEqual(3, HSSFColor.YELLOW.triplet.Length);
            Assert.AreEqual(255, HSSFColor.YELLOW.triplet[0]);
            Assert.AreEqual(255, HSSFColor.YELLOW.triplet[1]);
            Assert.AreEqual(0, HSSFColor.YELLOW.triplet[2]);

            Assert.AreEqual("FFFF:FFFF:0", HSSFColor.YELLOW.hexString);
        }
        [Test]
        public void TestTrippletHash()
        {
            Hashtable tripplets = HSSFColor.GetTripletHash();

            Assert.AreEqual(
                    typeof(HSSFColor.MAROON),
                    tripplets[HSSFColor.MAROON.hexString].GetType()
            );
            Assert.AreEqual(
                    typeof(HSSFColor.YELLOW),
                    tripplets[HSSFColor.YELLOW.hexString].GetType()
            );
        }
    }
}
