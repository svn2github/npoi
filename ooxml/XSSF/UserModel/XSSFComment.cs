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

using System;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.Model;
using NPOI.Util;
using NPOI.OpenXmlFormats.Vml.Spreadsheet;

namespace NPOI.XSSF.UserModel
{

    public class XSSFComment : IComment
    {

        private CT_Comment _comment;
        private CommentsTable _comments;
        private CT_Shape _vmlShape;

        /**
         * cached reference to the string with the comment text
         */
        private XSSFRichTextString _str;

        /**
         * Creates a new XSSFComment, associated with a given
         *  low level comment object.
         */
        public XSSFComment(CommentsTable comments, CT_Comment comment, CT_Shape vmlShape)
        {
            _comment = comment;
            _comments = comments;
            _vmlShape = vmlShape;

            // we potentially need to adjust the column/row information in the shape
            // the same way as we do in setRow()/setColumn()
            if (vmlShape != null && vmlShape.SizeOfClientDataArray() > 0)
            {
                CellReference ref1 = new CellReference(comment.@ref);
                CT_ClientData clientData = vmlShape.GetClientDataArray(0);
                clientData.SetRowArray(0, ref1.Row);

                clientData.SetColumnArray(0, ref1.Col);

                // There is a very odd xmlbeans bug when changing the row
                //  arrays which can lead to corrupt pointer
                // This call seems to fix them again... See bug #50795
                //vmlShape.GetClientDataList().ToString();
            }
        }

        /**
         *
         * @return Name of the original comment author. Default value is blank.
         */
        public String Author
        {
            get
            {
                return _comments.GetAuthor((int)_comment.authorId);
            }
            set 
            {
                _comment.authorId = (
                    (uint)_comments.FindAuthor(value)
                );
            }
        }

        public CellAddress Address
        {
            get
            {
                return new CellAddress(_comment.@ref);
            }
            set
            {
                CellAddress oldRef = new CellAddress(_comment.@ref);
                if (value.Equals(oldRef))
                {
                    // nothing to do
                    return;
                }
                _comment.@ref = value.FormatAsString();
                _comments.ReferenceUpdated(oldRef, _comment);

                if (_vmlShape != null)
                {
                    CT_ClientData clientData = _vmlShape.GetClientDataArray(0);
                    clientData.SetRowArray(0, value.Row);
                    clientData.SetColumnArray(0, value.Column);

                    // There is a very odd xmlbeans bug when changing the column
                    //  arrays which can lead to corrupt pointer
                    // This call seems to fix them again... See bug #50795
                    //_vmlShape.GetClientDataList().toString();
                }
            }
        }
        public void SetAddress(int row, int col)
        {
            Address = new CellAddress(row, col);
        }
        /**
         * @return the 0-based column of the cell that the comment is associated with.
         */
        public int Column
        {
            get
            {
                return Address.Column;
            }
            set
            {
                SetAddress(Row, value);
            }
        }

        /**
         * @return the 0-based row index of the cell that the comment is associated with.
         */
        public int Row
        {
            get
            {
                return Address.Row;
            }
            set 
            {
                SetAddress(value, Column);
            }
        }

        /**
         * @return whether the comment is visible
         */
        public bool Visible
        {
            get
            {
                bool visible = false;
                if (_vmlShape != null)
                {
                    String style = _vmlShape.style;
                    if (style != null)
                        visible = style.IndexOf("visibility:visible") != -1;
                    else
                    {
                        if (_vmlShape.GetClientDataArray(0) == null)
                            return false;
                        else
                            visible = _vmlShape.GetClientDataArray(0).visibleSpecified;
                    }
                }
                return visible;
            }
            set 
            {
                if (_vmlShape != null)
                {
                    String style;
                    if (value)
                    {
                        style = "position:absolute;visibility:visible";
                        _vmlShape.GetClientDataArray(0).visible = OpenXmlFormats.Vml.Spreadsheet.ST_TrueFalseBlank.@true;
                        _vmlShape.GetClientDataArray(0).visibleSpecified = true;
                    }
                    else
                    {
                        style = "position:absolute;visibility:hidden";
                        _vmlShape.GetClientDataArray(0).visible = OpenXmlFormats.Vml.Spreadsheet.ST_TrueFalseBlank.@false;
                        _vmlShape.GetClientDataArray(0).visibleSpecified = false;

                    }
                    _vmlShape.style = (style);
                }   
            }
        }

        /**
         * @return the rich text string of the comment
         */
        public IRichTextString String
        {
            get
            {
                if (_str == null)
                {
                    if (_comment.text != null) _str = new XSSFRichTextString(_comment.text);
                }
                return _str;
            }
            set 
            {
                if (!(value is XSSFRichTextString))
                {
                    throw new ArgumentException("Only XSSFRichTextString argument is supported");
                }
                _str = (XSSFRichTextString)value;
                _comment.text = (_str.GetCTRst());
            }
        }

        /**
         * Sets the rich text string used by this comment.
         *
         * @param string  the XSSFRichTextString used by this object.
         */
        public void SetString(string str)
        {
            this.String = (new XSSFRichTextString(str));
        }

        public IClientAnchor ClientAnchor
        {
            get
            {
                String position = _vmlShape.GetClientDataArray(0).GetAnchorArray(0);
                int[] pos = new int[8];
                int i = 0;
                foreach (String s in position.Split(",".ToCharArray()))
                {
                    pos[i++] = int.Parse(s.Trim());
                }
                XSSFClientAnchor ca = new XSSFClientAnchor(pos[1] * Units.EMU_PER_PIXEL, pos[3] * Units.EMU_PER_PIXEL,
                    pos[5] * Units.EMU_PER_PIXEL, pos[7] * Units.EMU_PER_PIXEL, pos[0], pos[2], pos[4], pos[6]);
                return ca;
            }
        }

        /**
         * @return the xml bean holding this comment's properties
         */
        internal CT_Comment GetCTComment()
        {
            return _comment;
        }

        internal CT_Shape GetCTShape()
        {
            return _vmlShape;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is XSSFComment)) {
                return false;
            }
            XSSFComment other = (XSSFComment)obj;
            return ((GetCTComment() == other.GetCTComment()) &&
                    (GetCTShape() == other.GetCTShape()));
        }

        public override int GetHashCode()
        {
            return ((Row * 17) + Column) * 31;
        }
    }
}

