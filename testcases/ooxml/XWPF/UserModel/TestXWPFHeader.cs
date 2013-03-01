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

namespace NPOI.XWPF.UserModel
{
    using System;



    using NUnit.Framework;

    using NPOI.XWPF;
    using NPOI.XWPF.Model;
    using NPOI.OpenXmlFormats.Wordprocessing;

    [TestFixture]
    public class TestXWPFHeader
    {

        [Test]
        public void TestSimpleHeader()
        {
            XWPFDocument sampleDoc = XWPFTestDataSamples.OpenSampleDocument("headerFooter.docx");

            XWPFHeaderFooterPolicy policy = sampleDoc.GetHeaderFooterPolicy();

            XWPFHeader header = policy.GetDefaultHeader();
            XWPFFooter footer = policy.GetDefaultFooter();
            Assert.IsNotNull(header);
            Assert.IsNotNull(footer);
        }

        [Test]
        public void TestImageInHeader()
        {
            XWPFDocument sampleDoc = XWPFTestDataSamples.OpenSampleDocument("headerPic.docx");

            XWPFHeaderFooterPolicy policy = sampleDoc.GetHeaderFooterPolicy();

            XWPFHeader header = policy.GetDefaultHeader();

            Assert.IsNotNull(header.GetRelations());
            Assert.AreEqual(1, header.GetRelations().Count);
        }

        [Test]
        public void TestSetHeader()
        {
            XWPFDocument sampleDoc = XWPFTestDataSamples.OpenSampleDocument("SampleDoc.docx");
            // no header is Set (yet)
            XWPFHeaderFooterPolicy policy = sampleDoc.GetHeaderFooterPolicy();
            Assert.IsNull(policy.GetDefaultHeader());
            Assert.IsNull(policy.GetFirstPageHeader());
            Assert.IsNull(policy.GetDefaultFooter());

            CT_P ctP1 = new CT_P();
            CT_R ctR1 = ctP1.AddNewR();
            CT_Text t = ctR1.AddNewT();
            t.Value = ("Paragraph in header");

            // Commented MB 23 May 2010
            //CTP ctP2 = CTP.Factory.NewInstance();
            //CTR ctR2 = ctP2.AddNewR();
            //CTText t2 = ctR2.AddNewT();
            //t2.StringValue=("Second paragraph.. for footer");

            // Create two paragraphs for insertion into the footer.
            // Previously only one was inserted MB 23 May 2010
            CT_P ctP2 = new CT_P();
            CT_R ctR2 = ctP2.AddNewR();
            CT_Text t2 = ctR2.AddNewT();
            t2.Value = ("First paragraph for the footer");

            CT_P ctP3 = new CT_P();
            CT_R ctR3 = ctP3.AddNewR();
            CT_Text t3 = ctR3.AddNewT();
            t3.Value = ("Second paragraph for the footer");

            XWPFParagraph p1 = new XWPFParagraph(ctP1, sampleDoc);
            XWPFParagraph[] pars = new XWPFParagraph[1];
            pars[0] = p1;

            XWPFParagraph p2 = new XWPFParagraph(ctP2, sampleDoc);
            XWPFParagraph p3 = new XWPFParagraph(ctP3, sampleDoc);
            XWPFParagraph[] pars2 = new XWPFParagraph[2];
            pars2[0] = p2;
            pars2[1] = p3;

            // Set headers
            policy.CreateHeader(XWPFHeaderFooterPolicy.DEFAULT, pars);
            policy.CreateHeader(XWPFHeaderFooterPolicy.FIRST);
            // Set a default footer and capture the returned XWPFFooter object.
            XWPFFooter footer = policy.CreateFooter(XWPFHeaderFooterPolicy.DEFAULT, pars2);

            // Ensure the headers and footer were Set correctly....
            Assert.IsNotNull(policy.GetDefaultHeader());
            Assert.IsNotNull(policy.GetFirstPageHeader());
            Assert.IsNotNull(policy.GetDefaultFooter());
            // ....and that the footer object captured above Contains two
            // paragraphs of text.
            Assert.AreEqual(2, footer.Paragraphs.Count);

            // As an Additional Check, recover the defauls footer and
            // make sure that it Contains two paragraphs of text and that
            // both do hold what is expected.
            footer = policy.GetDefaultFooter();

            XWPFParagraph[] paras = new XWPFParagraph[footer.Paragraphs.Count];
            int i = 0;
            foreach (XWPFParagraph p in footer.Paragraphs)
            {
                paras[i++] = p;
            }

            Assert.AreEqual(2, paras.Length);
            Assert.AreEqual("First paragraph for the footer", paras[0].GetText());
            Assert.AreEqual("Second paragraph for the footer", paras[1].GetText());
        }

        [Test]
        public void TestSetWatermark()
        {
            XWPFDocument sampleDoc = XWPFTestDataSamples.OpenSampleDocument("SampleDoc.docx");
            // no header is Set (yet)
            XWPFHeaderFooterPolicy policy = sampleDoc.GetHeaderFooterPolicy();
            Assert.IsNull(policy.GetDefaultHeader());
            Assert.IsNull(policy.GetFirstPageHeader());
            Assert.IsNull(policy.GetDefaultFooter());

            policy.CreateWatermark("DRAFT");

            Assert.IsNotNull(policy.GetDefaultHeader());
            Assert.IsNotNull(policy.GetFirstPageHeader());
            Assert.IsNotNull(policy.GetEvenPageHeader());
        }

        [Test]
        public void TestAddPictureData()
        {

        }

        [Test]
        public void TestGetAllPictures()
        {

        }

        [Test]
        public void TestGetAllPackagePictures()
        {

        }

        [Test]
        public void TestGetPictureDataById()
        {

        }
    }

}