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
using NPOI.SS.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
namespace NPOI.XSSF.UserModel
{

/**
 * @author Yegor Kozlov
 */
    public class XSSFBorderFormatting : IBorderFormatting
    {
        CT_Border _border;

        /*package*/
        internal XSSFBorderFormatting(CT_Border border)
        {
            _border = border;
        }

        #region IBorderFormatting Members

        public BorderStyle BorderBottom
        {
            get
            {
                if (!_border.IsSetBottom())
                {
                    return BorderStyle.None;
                }
                else
                {
                    return (BorderStyle)_border.bottom.style;
                }
            }
            set
            {
                CT_BorderPr pr = _border.IsSetBottom() ? _border.bottom : _border.AddNewBottom();
                if (value == BorderStyle.None) _border.UnsetBottom();
                else pr.style = (ST_BorderStyle)value;
            }
        }

        public BorderStyle BorderDiagonal
        {
            get
            {
                if (!_border.IsSetDiagonal())
                {
                    return BorderStyle.None;
                }
                else
                {
                    return (BorderStyle)_border.diagonal.style;
                }
            }
            set
            {
                CT_BorderPr pr = _border.IsSetDiagonal() ? _border.diagonal : _border.AddNewDiagonal();
                if (value == (short)BorderStyle.None) _border.unsetDiagonal();
                else pr.style = (ST_BorderStyle)value;
            }
        }

        public BorderStyle BorderLeft
        {
            get
            {
                if (!_border.IsSetLeft())
                {
                    return BorderStyle.None;
                }
                else
                {
                    return (BorderStyle)_border.left.style;
                }
            }
            set
            {
                CT_BorderPr pr = _border.IsSetLeft() ? _border.left : _border.AddNewLeft();
                if (value == (short)BorderStyle.None) _border.unsetLeft();
                else pr.style = (ST_BorderStyle)(value);
            }
        }

        public BorderStyle BorderRight
        {
            get
            {
                if (!_border.IsSetRight())
                {
                    return BorderStyle.None;
                }
                else
                {
                    return (BorderStyle)_border.right.style;
                }
            }
            set
            {
                CT_BorderPr pr = _border.IsSetRight() ? _border.right : _border.AddNewRight();
                if (value == (short)BorderStyle.None) _border.unsetRight();
                else pr.style = (ST_BorderStyle)(value );
            }
        }

        public BorderStyle BorderTop
        {
            get
            {
                if (!_border.IsSetTop())
                {
                    return BorderStyle.None;
                }
                else
                {
                    return (BorderStyle)_border.top.style;
                }
            }
            set
            {
                CT_BorderPr pr = _border.IsSetTop() ? _border.top : _border.AddNewTop();
                if (value == (short)BorderStyle.None) _border.unsetTop();
                else pr.style = (ST_BorderStyle)(value );
            }
        }

        public short BottomBorderColor
        {
            get
            {
                XSSFColor color = BottomBorderColorColor as XSSFColor;
                if (color == null) return 0;
                return color.Indexed;
            }
            set
            {
                CT_Color ctColor = new CT_Color();
                ctColor.indexed = (uint)(value);
                ctColor.indexedSpecified = true;
                setBottomBorderColor(ctColor);
            }
        }

        public short DiagonalBorderColor
        {
            get
            {
                XSSFColor color = DiagonalBorderColorColor as XSSFColor;
                if (color == null) return 0;
                return color.Indexed;
            }
            set
            {
                CT_Color ctColor = new CT_Color();
                ctColor.indexed = (uint)(value);
                ctColor.indexedSpecified = true;
                setDiagonalBorderColor(ctColor);
            }
        }

        public short LeftBorderColor
        {
            get
            {
                XSSFColor color = LeftBorderColorColor as XSSFColor;
                if (color == null) return 0;
                return color.Indexed;
            }
            set
            {
                CT_Color ctColor = new CT_Color();
                ctColor.indexed = (uint)(value);
                ctColor.indexedSpecified = true;
                setLeftBorderColor(ctColor);
            }
        }

        public short RightBorderColor
        {
            get
            {
                XSSFColor color = RightBorderColorColor as XSSFColor;
                if (color == null) return 0;
                return color.Indexed;
            }
            set
            {
                CT_Color ctColor = new CT_Color();
                ctColor.indexed = (uint)(value);
                ctColor.indexedSpecified = true;
                setRightBorderColor(ctColor);
            }
        }

        public short TopBorderColor
        {
            get
            {
                XSSFColor color = RightBorderColorColor as XSSFColor;
                if (color == null) return 0;
                return color.Indexed;
            }
            set
            {
                CT_Color ctColor = new CT_Color();
                ctColor.indexed = (uint)(value);
                ctColor.indexedSpecified = true;
                setTopBorderColor(ctColor);
            }
        }

        public IColor BottomBorderColorColor
        {
            get
            {
                if (!_border.IsSetBottom()) return null;

                CT_BorderPr pr = _border.bottom;
                return new XSSFColor(pr.color);
            }
            set
            {
                XSSFColor xcolor = XSSFColor.ToXSSFColor(value);
                if (xcolor == null) setBottomBorderColor((CT_Color)null);
                else setBottomBorderColor(xcolor.GetCTColor());
            }
        }
        public void setBottomBorderColor(CT_Color color)
        {
            CT_BorderPr pr = _border.IsSetBottom() ? _border.bottom : _border.AddNewBottom();
            if (color == null)
            {
                pr.UnsetColor();
            }
            else
            {
                pr.color = (color);
            }
        }
        public IColor DiagonalBorderColorColor
        {
            get
            {
                if (!_border.IsSetDiagonal()) return null;

                CT_BorderPr pr = _border.diagonal;
                return new XSSFColor(pr.color);
            }
            set
            {
                XSSFColor xcolor = XSSFColor.ToXSSFColor(value);
                if (xcolor == null) setDiagonalBorderColor((CT_Color)null);
                else setDiagonalBorderColor(xcolor.GetCTColor());
            }
        }
        public void setDiagonalBorderColor(CT_Color color)
        {
            CT_BorderPr pr = _border.IsSetDiagonal() ? _border.diagonal : _border.AddNewDiagonal();
            if (color == null)
            {
                pr.UnsetColor();
            }
            else
            {
                pr.color = (color);
            }
        }
        public IColor LeftBorderColorColor
        {
            get
            {
                if (!_border.IsSetLeft()) return null;

                CT_BorderPr pr = _border.left;
                return new XSSFColor(pr.color);
            }
            set
            {
                XSSFColor xcolor = XSSFColor.ToXSSFColor(value);
                if (xcolor == null) setLeftBorderColor((CT_Color)null);
                else setLeftBorderColor(xcolor.GetCTColor());
            }
        }

        public void setLeftBorderColor(CT_Color color)
        {
            CT_BorderPr pr = _border.IsSetLeft() ? _border.left : _border.AddNewLeft();
            if (color == null)
            {
                pr.UnsetColor();
            }
            else
            {
                pr.color = (color);
            }
        }
        public IColor RightBorderColorColor
        {
            get {
                if (!_border.IsSetRight()) return null;

                CT_BorderPr pr = _border.right;
                return new XSSFColor(pr.color);
            }
            set
            {
                XSSFColor xcolor = XSSFColor.ToXSSFColor(value);
                if (xcolor == null) setRightBorderColor((CT_Color)null);
                else setRightBorderColor(xcolor.GetCTColor());
            }
        }

        public void setRightBorderColor(CT_Color color)
        {
            CT_BorderPr pr = _border.IsSetRight() ? _border.right : _border.AddNewRight();
            if (color == null)
            {
                pr.UnsetColor();
            }
            else
            {
                pr.color = (color);
            }
        }

        public IColor TopBorderColorColor
        {
            get {
                if (!_border.IsSetTop()) return null;

                CT_BorderPr pr = _border.top;
                return new XSSFColor(pr.color);
            }
            set
            {
                XSSFColor xcolor = XSSFColor.ToXSSFColor(value);
                if (xcolor == null) setTopBorderColor((CT_Color)null);
                else setTopBorderColor(xcolor.GetCTColor());
            }
        }
        public void setTopBorderColor(CT_Color color)
        {
            CT_BorderPr pr = _border.IsSetTop() ? _border.top : _border.AddNewTop();
            if (color == null)
            {
                pr.UnsetColor();
            }
            else
            {
                pr.color = (color);
            }
        }
        #endregion
    }
}



