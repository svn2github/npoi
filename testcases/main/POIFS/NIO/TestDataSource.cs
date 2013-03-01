﻿/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
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

using System;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;
using NPOI.POIFS.NIO;
using System.IO;
using NPOI.Util;

namespace TestCases.POIFS.NIO
{
    /// <summary>
    /// Summary description for TestDataSource
    /// </summary>
    [TestFixture]
    public class TestDataSource
    {
        private static POIDataSamples data = POIDataSamples.GetPOIFSInstance();
        public TestDataSource()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [Test]
        public void TestFile()
        {
            FileStream f =  data.GetFile("Notes.ole2");

            FileBackedDataSource ds = new FileBackedDataSource(f);

            Assert.AreEqual(8192, ds.Size);

            ByteBuffer bs;
            bs = ds.Read(4, 0);
            Assert.AreEqual(4, bs.Length);
            //Assert.AreEqual(0, bs
            Assert.AreEqual(unchecked((byte)0xd0 - (byte)256), bs[0]);
            Assert.AreEqual(unchecked((byte)0xcf - (byte)256), bs[1]);
            Assert.AreEqual(unchecked((byte)0x11 - 0), bs[2]);
            Assert.AreEqual(unchecked((byte)0xe0 - (byte)256), bs[3]);

            bs = ds.Read(8, 0x400);
            Assert.AreEqual(8, bs.Length);
            //Assert.AreEqual(0, bs.position());
            Assert.AreEqual((byte)'R', bs[0]);
            Assert.AreEqual(0, bs[1]);
            Assert.AreEqual((byte)'o', bs[2]);
            Assert.AreEqual(0, bs[3]);
            Assert.AreEqual((byte)'o', bs[4]);
            Assert.AreEqual(0, bs[5]);
            Assert.AreEqual((byte)'t', bs[6]);
            Assert.AreEqual(0, bs[7]);

            bs = ds.Read(8, 8190);
            //Assert.AreEqual(0, 

            try
            {
                bs = ds.Read(4, 8192);
                Assert.Fail("Shouldn't be able to read off the end of the file");
            }
            catch (System.ArgumentException)
            {
            }

        }


        [Test]
        public void TestByteArray()
        {
            byte[] data = new byte[256];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }

            ByteArrayBackedDataSource ds = new ByteArrayBackedDataSource(data);

            ByteBuffer bs;
            bs = ds.Read(4, 0);
            //assertEquals(0, bs.position());
            Assert.AreEqual(0x00, bs[0]);
            Assert.AreEqual(0x01, bs[1]);
            Assert.AreEqual(0x02, bs[2]);
            Assert.AreEqual(0x03, bs[3]);


            bs = ds.Read(4, 100);
            Assert.AreEqual(100, bs.Position);
            Assert.AreEqual(100, bs.Read());
            Assert.AreEqual(101, bs.Read());
            Assert.AreEqual(102, bs.Read());
            Assert.AreEqual(103, bs.Read());

            bs = ds.Read(4, 252);
            Assert.AreEqual(unchecked((byte)-4), bs.Read());
            Assert.AreEqual(unchecked((byte)-3), bs.Read());
            Assert.AreEqual(unchecked((byte)-2), bs.Read());
            Assert.AreEqual(unchecked((byte)-1), bs.Read());

            // Off the end
            bs = ds.Read(4, 254);
            Assert.AreEqual(unchecked((byte)-2), bs.Read());
            Assert.AreEqual(unchecked((byte)-1), bs.Read());
            try
            {
                //bs.get();
                //fail("Shouldn't be able to read off the end");
            }
            catch (System.Exception) { }

            // Past the end
            try
            {
                bs = ds.Read(4, 256);
                Assert.Fail("Shouldn't be able to read off the end");
            }
            catch (System.IndexOutOfRangeException) { }


            // Overwrite
            bs = ByteBuffer.CreateBuffer(4);
            //bs.put(0, (byte)-55);
            //bs.put(1, (byte)-54);
            //bs.put(2, (byte)-53);
            //bs.put(3, (byte)-52);
            //bs.Write(0, unchecked((byte)-55));
            //bs.Write(1, unchecked((byte)-54));
            //bs.Write(2, unchecked((byte)-53));
            //bs.Write(3, unchecked((byte)-52));
            bs[0] = unchecked((byte)-55);
            bs[1] = unchecked((byte)-54);
            bs[2] = unchecked((byte)-53);
            bs[3] = unchecked((byte)-52);

            Assert.AreEqual(256, ds.Size);
            ds.Write(bs, 40);
            Assert.AreEqual(256, ds.Size);
            bs = ds.Read(4, 40);

            Assert.AreEqual(unchecked((byte)-55), bs.Read());
            Assert.AreEqual(unchecked((byte)-54), bs.Read());
            Assert.AreEqual(unchecked((byte)-53), bs.Read());
            Assert.AreEqual(unchecked((byte)-52), bs.Read());

            // Append
            //bs = ByteBuffer.allocate(4);
            bs = ByteBuffer.CreateBuffer(4);
            //bs.Write(0, unchecked((byte)-55));
            //bs.Write(1, unchecked((byte)-54));
            //bs.Write(2, unchecked((byte)-53));
            //bs.Write(3, unchecked((byte)-52));
            bs[0] = unchecked((byte)-55);
            bs[1] = unchecked((byte)-54);
            bs[2] = unchecked((byte)-53);
            bs[3] = unchecked((byte)-52);

            //bs[0] = unchecked((byte)-55);
            //bs[1] = unchecked((byte)-54);
            //bs[2] = unchecked((byte)-53);
            //bs[3] = unchecked((byte)-52);

            Assert.AreEqual(256, ds.Size);
            ds.Write(bs, 256);
            Assert.AreEqual(260, ds.Size);

            bs = ds.Read(4, 256);
            Assert.AreEqual(256, bs.Position);
            Assert.AreEqual(unchecked((byte)-55), bs.Read());
            Assert.AreEqual(unchecked((byte)-54), bs.Read());
            Assert.AreEqual(unchecked((byte)-53), bs.Read());
            Assert.AreEqual(unchecked((byte)-52), bs.Read());

        }
    }
}
