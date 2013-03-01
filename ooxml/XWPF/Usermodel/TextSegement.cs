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


    /**
     * saves the begin and end position  of a text in a Paragraph
    */
    public class TextSegement
    {
        private PositionInParagraph beginPos;
        private PositionInParagraph endPos;

        public TextSegement()
        {
            this.beginPos = new PositionInParagraph();
            this.endPos = new PositionInParagraph();
        }

        public TextSegement(int beginRun, int endRun, int beginText, int endText, int beginChar, int endChar)
        {
            PositionInParagraph beginPos = new PositionInParagraph(beginRun, beginText, beginChar);
            PositionInParagraph endPos = new PositionInParagraph(endRun, endText, endChar);
            this.beginPos = beginPos;
            this.endPos = endPos;
        }

        public TextSegement(PositionInParagraph beginPos, PositionInParagraph endPos)
        {
            this.beginPos = beginPos;
            this.endPos = endPos;
        }

        public PositionInParagraph GetBeginPos()
        {
            return beginPos;
        }

        public PositionInParagraph GetEndPos()
        {
            return endPos;
        }

        public int GetBeginRun()
        {
            return beginPos.Run;
        }

        public void SetBeginRun(int beginRun)
        {
            beginPos.Run = (beginRun);
        }

        public int GetBeginText()
        {
            return beginPos.Text;
        }

        public void SetBeginText(int beginText)
        {
            beginPos.Text = (beginText);
        }

        public int GetBeginChar()
        {
            return beginPos.Char;
        }

        public void SetBeginChar(int beginChar)
        {
            beginPos.Char = (beginChar);
        }
        public int GetEndRun()
        {
            return endPos.Run;
        }

        public void SetEndRun(int endRun)
        {
            endPos.Run = (endRun);
        }

        public int GetEndText()
        {
            return endPos.Text;
        }

        public void SetEndText(int endText)
        {
            endPos.Text = (endText);
        }

        public int GetEndChar()
        {
            return endPos.Char;
        }

        public void SetEndChar(int endChar)
        {
            endPos.Char = (endChar);
        }
    }

}