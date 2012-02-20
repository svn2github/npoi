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

using System.Collections.Generic;
using System;
using NPOI.XSSF.Util;
using System.IO;
using NPOI.OpenXml4Net.OPC;
namespace NPOI.XSSF.UserModel
{


    /**
     * Represents a SpreadsheetML VML Drawing.
     *
     * <p>
     * In Excel 2007 VML Drawings are used to describe properties of cell comments,
     * although the spec says that VML is deprecated:
     * </p>
     * <p>
     * The VML format is a legacy format originally introduced with Office 2000 and is included and fully defined
     * in this Standard for backwards compatibility reasons. The DrawingML format is a newer and richer format
     * Created with the goal of eventually replacing any uses of VML in the Office Open XML formats. VML should be
     * considered a deprecated format included in Office Open XML for legacy reasons only and new applications that
     * need a file format for Drawings are strongly encouraged to use preferentially DrawingML
     * </p>
     * 
     * <p>
     * Warning - Excel is known to Put invalid XML into these files!
     *  For example, &gt;br&lt; without being closed or escaped crops up.
     * </p>
     *
     * See 6.4 VML - SpreadsheetML Drawing in Office Open XML Part 4 - Markup Language Reference.pdf
     *
     * @author Yegor Kozlov
     */
    public class XSSFVMLDrawing : POIXMLDocumentPart
    {
        private static QName QNAME_SHAPE_LAYOUT = new QName("urn:schemas-microsoft-com:office:office", "shapelayout");
        private static QName QNAME_SHAPE_TYPE = new QName("urn:schemas-microsoft-com:vml", "shapetype");
        private static QName QNAME_SHAPE = new QName("urn:schemas-microsoft-com:vml", "shape");

        /**
         * regexp to parse shape ids, in VML they have weird form of id="_x0000_s1026"
         */
        private static Pattern ptrn_shapeId = Pattern.compile("_x0000_s(\\d+)");

        private List<QName> _qnames = new List<QName>();
        private List<XmlObject> _items = new List<XmlObject>();
        private String _shapeTypeId;
        private int _shapeId = 1024;

        /**
         * Create a new SpreadsheetML Drawing
         *
         * @see XSSFSheet#CreateDrawingPatriarch()
         */
        protected XSSFVMLDrawing()
            : base()
        {

            newDrawing();
        }

        /**
         * Construct a SpreadsheetML Drawing from a namespace part
         *
         * @param part the namespace part holding the Drawing data,
         * the content type must be <code>application/vnd.Openxmlformats-officedocument.Drawing+xml</code>
         * @param rel  the namespace relationship holding this Drawing,
         * the relationship type must be http://schemas.Openxmlformats.org/officeDocument/2006/relationships/drawing
         */
        protected XSSFVMLDrawing(PackagePart part, PackageRelationship rel)
            : base(part, rel)
        {

            Read(getPackagePart().GetInputStream());
        }


        protected void Read(Stream is1)
        {
            XmlObject root = XmlObject.Factory.Parse(
                  new EvilUnclosedBRFixingInputStream(is1)
            );

            _qnames = new List<QName>();
            _items = new List<XmlObject>();
            foreach (XmlObject obj in root.selectPath("$this/xml/*"))
            {
                Node nd = obj.GetDomNode();
                QName qname = new QName(nd.GetNamespaceURI(), nd.GetLocalName());
                if (qname.Equals(QNAME_SHAPE_LAYOUT))
                {
                    _items.Add(CT_ShapeLayout.Factory.Parse(obj.xmlText()));
                }
                else if (qname.Equals(QNAME_SHAPE_TYPE))
                {
                    CT_Shapetype st = CT_Shapetype.Factory.Parse(obj.xmlText());
                    _items.Add(st);
                    _shapeTypeId = st.GetId();
                }
                else if (qname.Equals(QNAME_SHAPE))
                {
                    CT_Shape shape = CT_Shape.Factory.Parse(obj.xmlText());
                    String id = shape.GetId();
                    if (id != null)
                    {
                        Matcher m = ptrn_shapeId.matcher(id);
                        if (m.Find()) _shapeId = Math.Max(_shapeId, Int32.ParseInt(m.group(1)));
                    }
                    _items.Add(shape);
                }
                else
                {
                    _items.Add(XmlObject.Factory.Parse(obj.xmlText()));
                }
                _qnames.Add(qname);
            }
        }

        protected List<XmlObject> GetItems()
        {
            return _items;
        }

        protected void Write(OutputStream out1)
        {
            XmlObject rootObject = XmlObject.Factory.newInstance();
            XmlCursor rootCursor = rootObject.newCursor();
            rootCursor.ToNextToken();
            rootCursor.beginElement("xml");

            for (int i = 0; i < _items.Count; i++)
            {
                XmlCursor xc = _items.Get(i).newCursor();
                rootCursor.beginElement(_qnames.Get(i));
                while (xc.ToNextToken() == XmlCursor.TokenType.ATTR)
                {
                    Node anode = xc.GetDomNode();
                    rootCursor.insertAttributeWithValue(anode.GetLocalName(), anode.GetNamespaceURI(), anode.GetNodeValue());
                }
                xc.ToStartDoc();
                xc.copyXmlContents(rootCursor);
                rootCursor.ToNextToken();
                xc.dispose();
            }
            rootCursor.dispose();

            //XmlOptions xmlOptions = new XmlOptions(DEFAULT_XML_OPTIONS);
            //xmlOptions.SetSavePrettyPrint();
            Dictionary<String, String> map = new Dictionary<String, String>();
            map["urn:schemas-microsoft-com:vml"] = "v";
            map["urn:schemas-microsoft-com:office:office"] = "o";
            map["urn:schemas-microsoft-com:office:excel"] = "x";
            //xmlOptions.SetSaveSuggestedPrefixes(map);

            rootObject.save(out1);
        }


        protected void Commit()
        {
            PackagePart part = GetPackagePart();
            Stream out1 = part.GetOutputStream();
            Write(out1);
            out1.Close();
        }

        /**
         * Initialize a new Speadsheet VML Drawing
         */
        private void newDrawing()
        {
            CT_ShapeLayout layout = CT_ShapeLayout.Factory.newInstance();
            layout.SetExt(STExt.EDIT);
            CTIdMap idmap = layout.AddNewIdmap();
            idmap.SetExt(STExt.EDIT);
            idmap.SetData("1");
            _items.Add(layout);
            _qnames.Add(QNAME_SHAPE_LAYOUT);

            CT_Shapetype shapetype = CT_Shapetype.Factory.newInstance();
            _shapeTypeId = "_xssf_cell_comment";
            shapetype.SetId(_shapeTypeId);
            shapetype.SetCoordsize("21600,21600");
            shapetype.SetSpt(202);
            shapetype.SetPath2("m,l,21600r21600,l21600,xe");
            shapetype.AddNewStroke().SetJoinstyle(ST_StrokeJoinStyle.MITER);
            CTPath path = shapetype.AddNewPath();
            path.SetGradientshapeok(ST_TrueFalse.T);
            path.SetConnecttype(ST_ConnectType.RECT);
            _items.Add(shapetype);
            _qnames.Add(QNAME_SHAPE_TYPE);
        }

        protected CT_Shape newCommentShape()
        {
            CT_Shape shape = CT_Shape.Factory.newInstance();
            shape.SetId("_x0000_s" + (++_shapeId));
            shape.SetType("#" + _shapeTypeId);
            shape.SetStyle("position:absolute; visibility:hidden");
            shape.SetFillcolor("#ffffe1");
            shape.SetInsetmode(ST_InsetMode.AUTO);
            shape.AddNewFill().SetColor("#ffffe1");
            CT_Shadow shadow = shape.AddNewShadow();
            shadow.SetOn(ST_TrueFalse.T);
            shadow.SetColor("black");
            shadow.SetObscured(ST_TrueFalse.T);
            shape.AddNewPath().SetConnecttype(ST_ConnectType.NONE);
            shape.AddNewTextbox().SetStyle("mso-direction-alt:auto");
            CT_ClientData cldata = shape.AddNewClientData();
            cldata.SetObjectType(ST_ObjectType.NOTE);
            cldata.AddNewMoveWithCells();
            cldata.AddNewSizeWithCells();
            cldata.AddNewAnchor().SetStringValue("1, 15, 0, 2, 3, 15, 3, 16");
            cldata.AddNewAutoFill().SetStringValue("False");
            cldata.AddNewRow().SetBigintValue(new Bigint("0"));
            cldata.AddNewColumn().SetBigintValue(new Bigint("0"));
            _items.Add(shape);
            _qnames.Add(QNAME_SHAPE);
            return shape;
        }

        /**
         * Find a shape with ClientData of type "NOTE" and the specified row and column
         *
         * @return the comment shape or <code>null</code>
         */
        protected CT_Shape FindCommentShape(int row, int col)
        {
            foreach (XmlObject itm in _items)
            {
                if (itm is CT_Shape)
                {
                    CT_Shape sh = (CT_Shape)itm;
                    if (sh.sizeOfClientDataArray() > 0)
                    {
                        CT_ClientData cldata = sh.GetClientDataArray(0);
                        if (cldata.GetObjectType() == ST_ObjectType.NOTE)
                        {
                            int crow = cldata.GetRowArray(0).intValue();
                            int ccol = cldata.GetColumnArray(0).intValue();
                            if (crow == row && ccol == col)
                            {
                                return sh;
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected bool RemoveCommentShape(int row, int col)
        {
            CT_Shape shape = FindCommentShape(row, col);
            return shape != null && _items.Remove(shape);
        }
    }
}
