using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace NPOI.OpenXmlFormats.Dml.Picture
{
    [Serializable]
    [System.Diagnostics.DebuggerStepThrough]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(Namespace = "http://schemas.openxmlformats.org/drawingml/2006/picture")]
    [XmlRoot("pic", Namespace = "http://schemas.openxmlformats.org/drawingml/2006/picture", IsNullable = false)]
    // draw-pic:pic
    public class CT_Picture
    {
        private CT_PictureNonVisual nvPicPrField = new CT_PictureNonVisual();        //  draw-pic 1..1 

        private CT_BlipFillProperties blipFillField = new CT_BlipFillProperties();   //  draw-pic: 1..1 

        private CT_ShapeProperties spPrField = new CT_ShapeProperties();             //  draw-pic: 1..1 

        [XmlElement(Order = 0)]
        public CT_PictureNonVisual nvPicPr
        {
            get { return this.nvPicPrField; }
            set { this.nvPicPrField = value; }
        }

        [XmlElement(Order = 1)]
        public CT_BlipFillProperties blipFill
        {
            get { return this.blipFillField; }
            set { this.blipFillField = value; }
        }

        [XmlElement(Order = 2)]
        public CT_ShapeProperties spPr
        {
            get { return this.spPrField; }
            set { this.spPrField = value; }
        }

        public CT_PictureNonVisual AddNewNvPicPr()
        {
            nvPicPrField = new CT_PictureNonVisual();
            return this.nvPicPrField;
        }

        public CT_BlipFillProperties AddNewBlipFill()
        {
            blipFillField = new CT_BlipFillProperties();
            return this.blipFillField;
        }

        public CT_ShapeProperties AddNewSpPr()
        {
            spPrField = new CT_ShapeProperties();
            return this.spPrField;
        }

        //public void Set(CT_Picture pict)
        //{
        //    throw new NotImplementedException();
        //}

    }

    // see same class in different name space in SpeedsheetDrawing.cs
    [Serializable]
    [System.Diagnostics.DebuggerStepThrough]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(Namespace = "http://schemas.openxmlformats.org/drawingml/2006/picture")]
    public class CT_PictureNonVisual
    {

        private CT_NonVisualDrawingProps cNvPrField = new CT_NonVisualDrawingProps(); // 1..1
        private CT_NonVisualPictureProperties cNvPicPrField = new CT_NonVisualPictureProperties(); // 1..1

        public CT_NonVisualDrawingProps AddNewCNvPr()
        {
            this.cNvPrField = new CT_NonVisualDrawingProps();
            return this.cNvPrField;
        }
        public CT_NonVisualPictureProperties AddNewCNvPicPr()
        {
            this.cNvPicPrField = new CT_NonVisualPictureProperties();
            return this.cNvPicPrField;
        }

        [XmlElement(Order = 0)]
        public CT_NonVisualDrawingProps cNvPr
        {
            get { return this.cNvPrField; }
            set { this.cNvPrField = value; }
        }


        [XmlElement(Order = 1)]
        public CT_NonVisualPictureProperties cNvPicPr
        {
            get { return this.cNvPicPrField; }
            set { this.cNvPicPrField = value; }
        }
    }

}
