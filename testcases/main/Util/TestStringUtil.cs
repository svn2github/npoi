
/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional inFormation regarding copyright ownership.
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

namespace TestCases.Util
{
    using System;
    using System.Text;

    using NPOI.Util;
    using NUnit.Framework;
    /**
     * Unit Test for StringUtil
     *
     * @author  Marc Johnson (mjohnson at apache dot org
     * @author  Glen Stampoultzis (glens at apache.org)
     * @author  Sergei Kozello (sergeikozello at mail.ru)
     */
    [TestFixture]
    public class TestStringUtil
    {
        /**
         * Creates new TestStringUtil
         *
         * @param name
         */
        public TestStringUtil()
        {

        }

        /**
         * Test simple form of getFromUnicode
         */
        [Test]
        public void TestSimpleGetFromUnicode()
        {
            byte[] Test_data = new byte[32];
            int index = 0;

            for (int k = 0; k < 16; k++)
            {
                Test_data[index++] = (byte)0;
                Test_data[index++] = (byte)('a' + k);
            }

            Assert.AreEqual("abcdefghijklmnop",
                    StringUtil.GetFromUnicodeBE(Test_data));
        }

        /**
         * Test simple form of getFromUnicode with symbols with code below and more 127
         */
        [Test]
        public void TestGetFromUnicodeSymbolsWithCodesMoreThan127()
        {
            byte[] Test_data = new byte[]{0x04, 0x22,
                                      0x04, 0x35,
                                      0x04, 0x41,
                                      0x04, 0x42,
                                      0x00, 0x20,
                                      0x00, 0x74,
                                      0x00, 0x65,
                                      0x00, 0x73,
                                      0x00, 0x74,
        };

            Assert.AreEqual("\u0422\u0435\u0441\u0442 test",
                    StringUtil.GetFromUnicodeBE(Test_data));
        }

        /**
         * Test getFromUnicodeHigh for symbols with code below and more 127
         */
        [Test]
        public void TestGetFromUnicodeHighSymbolsWithCodesMoreThan127()
        {
            byte[] Test_data = new byte[]{0x22, 0x04,
                                      0x35, 0x04,
                                      0x41, 0x04,
                                      0x42, 0x04,
                                      0x20, 0x00,
                                      0x74, 0x00,
                                      0x65, 0x00,
                                      0x73, 0x00,
                                      0x74, 0x00,
        };


            Assert.AreEqual("\u0422\u0435\u0441\u0442 test",
                    StringUtil.GetFromUnicodeLE(Test_data));
        }

        /**
         * Test more complex form of getFromUnicode
         */
        [Test]
        public void TestComplexGetFromUnicode()
        {
            byte[] Test_data = new byte[32];
            int index = 0;
            for (int k = 0; k < 16; k++)
            {
                Test_data[index++] = (byte)0;
                Test_data[index++] = (byte)('a' + k);
            }
            Assert.AreEqual("abcdefghijklmno",
                    StringUtil.GetFromUnicodeBE(Test_data, 0, 15));
            Assert.AreEqual("bcdefghijklmnop",
                    StringUtil.GetFromUnicodeBE(Test_data, 2, 15));
            try
            {
                StringUtil.GetFromUnicodeBE(Test_data, -1, 16);
                Assert.Fail("Should have caught IndexOutOfRangeException");
            }
            catch (IndexOutOfRangeException)// ignored
            {
                // as expected
            }

            try
            {
                StringUtil.GetFromUnicodeBE(Test_data, 32, 16);
                Assert.Fail("Should have caught IndexOutOfRangeException");
            }
            catch (IndexOutOfRangeException)// ignored
            {
                // as expected
            }

            try
            {
                StringUtil.GetFromUnicodeBE(Test_data, 1, 16);
                Assert.Fail("Should have caught ArgumentException");
            }
            catch (ArgumentException)// ignored
            {
                // as expected
            }

            try
            {
                StringUtil.GetFromUnicodeBE(Test_data, 1, -1);
                Assert.Fail("Should have caught ArgumentException");
            }
            catch (ArgumentException)// ignored
            {
                // as expected
            }
        }

        /**
         * Test PutCompressedUnicode
         */
        [Test]
        public void TestPutCompressedUnicode()
        {
            byte[] outPut = new byte[100];
            byte[] expected_outPut =
                {
                    (byte) 'H', (byte) 'e', (byte) 'l', (byte) 'l',
                    (byte) 'o', (byte) ' ', (byte) 'W', (byte) 'o',
                    (byte) 'r', (byte) 'l', (byte) 'd', (byte) 0xAE
                };
            String inPut = Encoding.GetEncoding( StringUtil.GetPreferredEncoding()).GetString(expected_outPut);

            StringUtil.PutCompressedUnicode(inPut, outPut, 0);
            for (int j = 0; j < expected_outPut.Length; j++)
            {
                Assert.AreEqual(expected_outPut[j],
                        outPut[j], "Testing offset " + j);
            }
            StringUtil.PutCompressedUnicode(inPut, outPut,
                    100 - expected_outPut.Length);
            for (int j = 0; j < expected_outPut.Length; j++)
            {
                Assert.AreEqual(expected_outPut[j],
                        outPut[100 + j - expected_outPut.Length], "Testing offset " + j);
            }
            try
            {
                StringUtil.PutCompressedUnicode(inPut, outPut,
                        101 - expected_outPut.Length);
                Assert.Fail("Should have caught ArgumentException");
            }
            catch (ArgumentException)// ignored
            {
                // as expected
            }
        }

        /**
         * Test PutUncompressedUnicode
         */
        [Test]
        public void TestPutUncompressedUnicode()
        {
            byte[] outPut = new byte[100];
            String inPut = "Hello World";
            byte[] expected_outPut =
                {
                    (byte) 'H', (byte) 0, (byte) 'e', (byte) 0, (byte) 'l',
                    (byte) 0, (byte) 'l', (byte) 0, (byte) 'o', (byte) 0,
                    (byte) ' ', (byte) 0, (byte) 'W', (byte) 0, (byte) 'o',
                    (byte) 0, (byte) 'r', (byte) 0, (byte) 'l', (byte) 0,
                    (byte) 'd', (byte) 0
                };

            StringUtil.PutUnicodeLE(inPut, outPut, 0);
            for (int j = 0; j < expected_outPut.Length; j++)
            {
                Assert.AreEqual(expected_outPut[j],
                        outPut[j], "Testing offset " + j);
            }
            StringUtil.PutUnicodeLE(inPut, outPut,
                    100 - expected_outPut.Length);
            for (int j = 0; j < expected_outPut.Length; j++)
            {
                Assert.AreEqual(expected_outPut[j],
                        outPut[100 + j - expected_outPut.Length], "Testing offset " + j);
            }
            try
            {
                StringUtil.PutUnicodeLE(inPut, outPut,
                        101 - expected_outPut.Length);
                Assert.Fail("Should have caught ArgumentException");
            }
            catch (ArgumentException)// ignored
            {
                // as expected
            }
        }
    }
}

