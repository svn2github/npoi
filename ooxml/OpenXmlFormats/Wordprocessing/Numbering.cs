﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NPOI.OpenXmlFormats.Wordprocessing
{
    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_Num
    {

        private CT_DecimalNumber abstractNumIdField;

        private List<CT_NumLvl> lvlOverrideField;

        private string numIdField;

        public CT_Num()
        {
            //this.lvlOverrideField = new List<CT_NumLvl>();
            this.abstractNumIdField = new CT_DecimalNumber();
        }

        [XmlElement(Order = 0)]
        public CT_DecimalNumber abstractNumId
        {
            get
            {
                return this.abstractNumIdField;
            }
            set
            {
                this.abstractNumIdField = value;
            }
        }

        [XmlElement("lvlOverride", Order = 1)]
        public List<CT_NumLvl> lvlOverride
        {
            get
            {
                return this.lvlOverrideField;
            }
            set
            {
                this.lvlOverrideField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string numId
        {
            get
            {
                return this.numIdField;
            }
            set
            {
                this.numIdField = value;
            }
        }

        public CT_DecimalNumber AddNewAbstractNumId()
        {
            if (this.abstractNumIdField == null)
                abstractNumIdField = new CT_DecimalNumber();
            return abstractNumIdField;
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_NumLvl
    {

        private CT_DecimalNumber startOverrideField;

        private CT_Lvl lvlField;

        private string ilvlField;

        public CT_NumLvl()
        {
            this.lvlField = new CT_Lvl();
            this.startOverrideField = new CT_DecimalNumber();
        }

        [XmlElement(Order = 0)]
        public CT_DecimalNumber startOverride
        {
            get
            {
                return this.startOverrideField;
            }
            set
            {
                this.startOverrideField = value;
            }
        }

        [XmlElement(Order = 1)]
        public CT_Lvl lvl
        {
            get
            {
                return this.lvlField;
            }
            set
            {
                this.lvlField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string ilvl
        {
            get
            {
                return this.ilvlField;
            }
            set
            {
                this.ilvlField = value;
            }
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot("numbering", Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = false)]
    public class CT_Numbering
    {

        private List<CT_NumPicBullet> numPicBulletField;

        private List<CT_AbstractNum> abstractNumField;

        private List<CT_Num> numField;

        private CT_DecimalNumber numIdMacAtCleanupField;

        public CT_Numbering()
        {
            //this.numIdMacAtCleanupField = new CT_DecimalNumber();
            this.numField = new List<CT_Num>();
            this.abstractNumField = new List<CT_AbstractNum>();
            //this.numPicBulletField = new List<CT_NumPicBullet>();
        }

        [XmlElement("numPicBullet", Order = 0)]
        public List<CT_NumPicBullet> numPicBullet
        {
            get
            {
                return this.numPicBulletField;
            }
            set
            {
                this.numPicBulletField = value;
            }
        }

        [XmlElement("abstractNum", Order = 1)]
        public List<CT_AbstractNum> abstractNum
        {
            get
            {
                return this.abstractNumField;
            }
            set
            {
                this.abstractNumField = value;
            }
        }

        [XmlElement("num", Order = 2)]
        public List<CT_Num> num
        {
            get
            {
                return this.numField;
            }
            set
            {
                this.numField = value;
            }
        }

        [XmlElement(Order = 3)]
        public CT_DecimalNumber numIdMacAtCleanup
        {
            get
            {
                return this.numIdMacAtCleanupField;
            }
            set
            {
                this.numIdMacAtCleanupField = value;
            }
        }

        public IList<CT_Num> GetNumList()
        {
            return numField;
        }

        public IList<CT_AbstractNum> GetAbstractNumList()
        {
            return abstractNumField;
        }

        public CT_Num AddNewNum()
        {
            CT_Num num = new CT_Num();
            numField.Add(num);
            return num;
        }

        public void SetNumArray(int pos, CT_Num cT_Num)
        {
            if (pos < 0 || pos >= numField.Count)
                numField.Add(cT_Num);
            numField[pos] = cT_Num;
        }

        public CT_AbstractNum AddNewAbstractNum()
        {
            CT_AbstractNum num = new CT_AbstractNum();
            this.abstractNumField.Add(num);
            return num;
        }

        public void SetAbstractNumArray(int pos, CT_AbstractNum cT_AbstractNum)
        {
            if (pos < 0 || pos >= abstractNumField.Count)
                abstractNumField.Add(cT_AbstractNum);
            abstractNumField[pos] = cT_AbstractNum;
        }

        public void RemoveAbstractNum(int p)
        {
            abstractNumField.RemoveAt(p);
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_NumPicBullet
    {

        private CT_Picture pictField;

        private string numPicBulletIdField;

        public CT_NumPicBullet()
        {
            this.pictField = new CT_Picture();
        }

        [XmlElement(Order = 0)]
        public CT_Picture pict
        {
            get
            {
                return this.pictField;
            }
            set
            {
                this.pictField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string numPicBulletId
        {
            get
            {
                return this.numPicBulletIdField;
            }
            set
            {
                this.numPicBulletIdField = value;
            }
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_NumFmt
    {

        private ST_NumberFormat valField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_NumberFormat val
        {
            get
            {
                return this.valField;
            }
            set
            {
                this.valField = value;
            }
        }
    }


    [Serializable]
    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    public enum ST_NumberFormat
    {

    
        [XmlEnum("decimal")]
        @decimal,

    
        upperRoman,

    
        lowerRoman,

    
        upperLetter,

    
        lowerLetter,

    
        ordinal,

    
        cardinalText,

    
        ordinalText,

    
        hex,

    
        chicago,

    
        ideographDigital,

    
        japaneseCounting,

    
        aiueo,

    
        iroha,

    
        decimalFullWidth,

    
        decimalHalfWidth,

    
        japaneseLegal,

    
        japaneseDigitalTenThousand,

    
        decimalEnclosedCircle,

    
        decimalFullWidth2,

    
        aiueoFullWidth,

    
        irohaFullWidth,

    
        decimalZero,

    
        bullet,

    
        ganada,

    
        chosung,

    
        decimalEnclosedFullstop,

    
        decimalEnclosedParen,

    
        decimalEnclosedCircleChinese,

    
        ideographEnclosedCircle,

    
        ideographTraditional,

    
        ideographZodiac,

    
        ideographZodiacTraditional,

    
        taiwaneseCounting,

    
        ideographLegalTraditional,

    
        taiwaneseCountingThousand,

    
        taiwaneseDigital,

    
        chineseCounting,

    
        chineseLegalSimplified,

    
        chineseCountingThousand,

    
        koreanDigital,

    
        koreanCounting,

    
        koreanLegal,

    
        koreanDigital2,

    
        vietnameseCounting,

    
        russianLower,

    
        russianUpper,

    
        none,

    
        numberInDash,

    
        hebrew1,

    
        hebrew2,

    
        arabicAlpha,

    
        arabicAbjad,

    
        hindiVowels,

    
        hindiConsonants,

    
        hindiNumbers,

    
        hindiCounting,

    
        thaiLetters,

    
        thaiNumbers,

    
        thaiCounting,
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_NumRestart
    {

        private ST_RestartNumber valField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_RestartNumber val
        {
            get
            {
                return this.valField;
            }
            set
            {
                this.valField = value;
            }
        }
    }


    [Serializable]
    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    public enum ST_RestartNumber
    {

    
        continuous,

    
        eachSect,

    
        eachPage,
    }
    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_NumPr
    {

        private CT_DecimalNumber ilvlField;

        private CT_DecimalNumber numIdField;

        private CT_TrackChangeNumbering numberingChangeField;

        private CT_TrackChange insField;

        public CT_NumPr()
        {
            //this.insField = new CT_TrackChange();
            //this.numberingChangeField = new CT_TrackChangeNumbering();
            this.numIdField = new CT_DecimalNumber();
            this.ilvlField = new CT_DecimalNumber();
        }

        [XmlElement(Order = 0)]
        public CT_DecimalNumber ilvl
        {
            get
            {
                return this.ilvlField;
            }
            set
            {
                this.ilvlField = value;
            }
        }

        [XmlElement(Order = 1)]
        public CT_DecimalNumber numId
        {
            get
            {
                return this.numIdField;
            }
            set
            {
                this.numIdField = value;
            }
        }

        [XmlElement(Order = 2)]
        public CT_TrackChangeNumbering numberingChange
        {
            get
            {
                return this.numberingChangeField;
            }
            set
            {
                this.numberingChangeField = value;
            }
        }

        [XmlElement(Order = 3)]
        public CT_TrackChange ins
        {
            get
            {
                return this.insField;
            }
            set
            {
                this.insField = value;
            }
        }

        public void AddNewNumId()
        {
            throw new NotImplementedException();
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_Sym
    {

        private string fontField;

        private byte[] charField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "hexBinary")]
        public byte[] @char
        {
            get
            {
                return this.charField;
            }
            set
            {
                this.charField = value;
            }
        }
    }

    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_AbstractNum
    {

        private CT_LongHexNumber nsidField;

        private CT_MultiLevelType multiLevelTypeField;

        private CT_LongHexNumber tmplField;

        private CT_String nameField;

        private CT_String styleLinkField;

        private CT_String numStyleLinkField;

        private List<CT_Lvl> lvlField;

        private string abstractNumIdField;

        public CT_AbstractNum()
        {
            this.lvlField = new List<CT_Lvl>();
            //this.numStyleLinkField = new CT_String();
            //this.styleLinkField = new CT_String();
            //this.nameField = new CT_String();
            //this.tmplField = new CT_LongHexNumber();
            this.multiLevelTypeField = new CT_MultiLevelType();
            this.nsidField = new CT_LongHexNumber();
            this.nsidField.val = new byte[4];
            Array.Copy(BitConverter.GetBytes(DateTime.Now.Ticks), 4, this.nsidField.val, 0, 4);
        }

        [XmlElement(Order = 0)]
        public CT_LongHexNumber nsid
        {
            get
            {
                return this.nsidField;
            }
            set
            {
                this.nsidField = value;
            }
        }

        [XmlElement(Order = 1)]
        public CT_MultiLevelType multiLevelType
        {
            get
            {
                return this.multiLevelTypeField;
            }
            set
            {
                this.multiLevelTypeField = value;
            }
        }

        [XmlElement(Order = 2)]
        public CT_LongHexNumber tmpl
        {
            get
            {
                return this.tmplField;
            }
            set
            {
                this.tmplField = value;
            }
        }

        [XmlElement(Order = 3)]
        public CT_String name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [XmlElement(Order = 4)]
        public CT_String styleLink
        {
            get
            {
                return this.styleLinkField;
            }
            set
            {
                this.styleLinkField = value;
            }
        }

        [XmlElement(Order = 5)]
        public CT_String numStyleLink
        {
            get
            {
                return this.numStyleLinkField;
            }
            set
            {
                this.numStyleLinkField = value;
            }
        }

        [XmlElement("lvl", Order = 6)]
        public CT_Lvl[] lvl
        {
            get
            {
                return this.lvlField.ToArray();
            }
            set
            {
                if (value == null || value.Length == 0)
                    this.lvlField = new List<CT_Lvl>();
                else
                    this.lvlField = new List<CT_Lvl>(value);
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string abstractNumId
        {
            get
            {
                return this.abstractNumIdField;
            }
            set
            {
                this.abstractNumIdField = value;
            }
        }

        public CT_AbstractNum Copy()
        {
            CT_AbstractNum anum = new CT_AbstractNum();
            anum.abstractNumIdField = this.abstractNumIdField;
            anum.lvlField = new List<CT_Lvl>(this.lvlField);
            anum.multiLevelTypeField = this.multiLevelTypeField;
            anum.nameField = this.nameField;
            anum.nsidField = this.nsidField;
            anum.numStyleLinkField = this.numStyleLinkField;
            anum.styleLinkField = this.styleLinkField;
            anum.tmplField = this.tmplField;
            return anum;
        }

        public bool ValueEquals(CT_AbstractNum cT_AbstractNum)
        {
            return this.abstractNumIdField == cT_AbstractNum.abstractNumIdField;
        }

        public void Set(CT_AbstractNum cT_AbstractNum)
        {
            this.abstractNumIdField = cT_AbstractNum.abstractNumIdField;
            this.lvlField = new List<CT_Lvl>(cT_AbstractNum.lvlField);
            this.multiLevelTypeField = cT_AbstractNum.multiLevelTypeField;
            this.nameField = cT_AbstractNum.nameField;
            this.nsidField = cT_AbstractNum.nsidField;
            this.numStyleLinkField = cT_AbstractNum.numStyleLinkField;
            this.styleLinkField = cT_AbstractNum.styleLinkField;
            this.tmplField = cT_AbstractNum.tmplField;
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_MultiLevelType
    {

        private ST_MultiLevelType valField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_MultiLevelType val
        {
            get
            {
                return this.valField;
            }
            set
            {
                this.valField = value;
            }
        }
    }


    [Serializable]
    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    public enum ST_MultiLevelType
    {

    
        singleLevel,

    
        multilevel,

    
        hybridMultilevel,
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_Lvl
    {

        private CT_DecimalNumber startField;

        private CT_NumFmt numFmtField;

        private CT_DecimalNumber lvlRestartField;

        private CT_String pStyleField;

        private CT_OnOff isLglField;

        private CT_LevelSuffix suffField;

        private CT_LevelText lvlTextField;

        private CT_DecimalNumber lvlPicBulletIdField;

        private CT_LvlLegacy legacyField;

        private CT_Jc lvlJcField;

        private CT_PPr pPrField;

        private CT_RPr rPrField;

        private string ilvlField;

        private byte[] tplcField;

        private ST_OnOff tentativeField;

        private bool tentativeFieldSpecified;

        public CT_Lvl()
        {
            this.rPrField = new CT_RPr();
            this.pPrField = new CT_PPr();
            this.lvlJcField = new CT_Jc();
            //this.legacyField = new CT_LvlLegacy();
            //this.lvlPicBulletIdField = new CT_DecimalNumber();
            this.lvlTextField = new CT_LevelText();
            //this.suffField = new CT_LevelSuffix();
            //this.isLglField = new CT_OnOff();
            //this.pStyleField = new CT_String();
            //this.lvlRestartField = new CT_DecimalNumber();
            this.numFmtField = new CT_NumFmt();
            this.startField = new CT_DecimalNumber();
        }

        [XmlElement(Order = 0)]
        public CT_DecimalNumber start
        {
            get
            {
                return this.startField;
            }
            set
            {
                this.startField = value;
            }
        }

        [XmlElement(Order = 1)]
        public CT_NumFmt numFmt
        {
            get
            {
                return this.numFmtField;
            }
            set
            {
                this.numFmtField = value;
            }
        }

        [XmlElement(Order = 2)]
        public CT_DecimalNumber lvlRestart
        {
            get
            {
                return this.lvlRestartField;
            }
            set
            {
                this.lvlRestartField = value;
            }
        }

        [XmlElement(Order = 3)]
        public CT_String pStyle
        {
            get
            {
                return this.pStyleField;
            }
            set
            {
                this.pStyleField = value;
            }
        }

        [XmlElement(Order = 4)]
        public CT_OnOff isLgl
        {
            get
            {
                return this.isLglField;
            }
            set
            {
                this.isLglField = value;
            }
        }

        [XmlElement(Order = 5)]
        public CT_LevelSuffix suff
        {
            get
            {
                return this.suffField;
            }
            set
            {
                this.suffField = value;
            }
        }

        [XmlElement(Order = 6)]
        public CT_LevelText lvlText
        {
            get
            {
                return this.lvlTextField;
            }
            set
            {
                this.lvlTextField = value;
            }
        }

        [XmlElement(Order = 7)]
        public CT_DecimalNumber lvlPicBulletId
        {
            get
            {
                return this.lvlPicBulletIdField;
            }
            set
            {
                this.lvlPicBulletIdField = value;
            }
        }

        [XmlElement(Order = 8)]
        public CT_LvlLegacy legacy
        {
            get
            {
                return this.legacyField;
            }
            set
            {
                this.legacyField = value;
            }
        }

        [XmlElement(Order = 9)]
        public CT_Jc lvlJc
        {
            get
            {
                return this.lvlJcField;
            }
            set
            {
                this.lvlJcField = value;
            }
        }

        [XmlElement(Order = 10)]
        public CT_PPr pPr
        {
            get
            {
                return this.pPrField;
            }
            set
            {
                this.pPrField = value;
            }
        }

        [XmlElement(Order = 11)]
        public CT_RPr rPr
        {
            get
            {
                return this.rPrField;
            }
            set
            {
                this.rPrField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string ilvl
        {
            get
            {
                return this.ilvlField;
            }
            set
            {
                this.ilvlField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "hexBinary")]
        public byte[] tplc
        {
            get
            {
                return this.tplcField;
            }
            set
            {
                this.tplcField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff tentative
        {
            get
            {
                return this.tentativeField;
            }
            set
            {
                this.tentativeField = value;
                this.tentativeFieldSpecified = true;
            }
        }

        [XmlIgnore]
        public bool tentativeSpecified
        {
            get
            {
                return this.tentativeFieldSpecified;
            }
            set
            {
                this.tentativeFieldSpecified = value;
            }
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_LevelSuffix
    {

        private ST_LevelSuffix valField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_LevelSuffix val
        {
            get
            {
                return this.valField;
            }
            set
            {
                this.valField = value;
            }
        }
    }


    [Serializable]
    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    public enum ST_LevelSuffix
    {

    
        tab,

    
        space,

    
        nothing,
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_LevelText
    {

        private string valField;

        private ST_OnOff nullField;

        private bool nullFieldSpecified;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string val
        {
            get
            {
                return this.valField;
            }
            set
            {
                this.valField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff @null
        {
            get
            {
                return this.nullField;
            }
            set
            {
                this.nullField = value;
            }
        }

        [XmlIgnore]
        public bool nullSpecified
        {
            get
            {
                return this.nullFieldSpecified;
            }
            set
            {
                this.nullFieldSpecified = value;
            }
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_LvlLegacy
    {

        private ST_OnOff legacyField;

        private bool legacyFieldSpecified;

        private ulong legacySpaceField;

        private bool legacySpaceFieldSpecified;

        private string legacyIndentField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff legacy
        {
            get
            {
                return this.legacyField;
            }
            set
            {
                this.legacyField = value;
            }
        }

        [XmlIgnore]
        public bool legacySpecified
        {
            get
            {
                return this.legacyFieldSpecified;
            }
            set
            {
                this.legacyFieldSpecified = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ulong legacySpace
        {
            get
            {
                return this.legacySpaceField;
            }
            set
            {
                this.legacySpaceField = value;
            }
        }

        [XmlIgnore]
        public bool legacySpaceSpecified
        {
            get
            {
                return this.legacySpaceFieldSpecified;
            }
            set
            {
                this.legacySpaceFieldSpecified = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string legacyIndent
        {
            get
            {
                return this.legacyIndentField;
            }
            set
            {
                this.legacyIndentField = value;
            }
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_LsdException
    {

        private string nameField;

        private ST_OnOff lockedField;

        private bool lockedFieldSpecified;

        private string uiPriorityField;

        private ST_OnOff semiHiddenField;

        private bool semiHiddenFieldSpecified;

        private ST_OnOff unhideWhenUsedField;

        private bool unhideWhenUsedFieldSpecified;

        private ST_OnOff qFormatField;

        private bool qFormatFieldSpecified;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff locked
        {
            get
            {
                return this.lockedField;
            }
            set
            {
                this.lockedField = value;
            }
        }

        [XmlIgnore]
        public bool lockedSpecified
        {
            get
            {
                return this.lockedFieldSpecified;
            }
            set
            {
                this.lockedFieldSpecified = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "integer")]
        public string uiPriority
        {
            get
            {
                return this.uiPriorityField;
            }
            set
            {
                this.uiPriorityField = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff semiHidden
        {
            get
            {
                return this.semiHiddenField;
            }
            set
            {
                this.semiHiddenField = value;
            }
        }

        [XmlIgnore]
        public bool semiHiddenSpecified
        {
            get
            {
                return this.semiHiddenFieldSpecified;
            }
            set
            {
                this.semiHiddenFieldSpecified = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff unhideWhenUsed
        {
            get
            {
                return this.unhideWhenUsedField;
            }
            set
            {
                this.unhideWhenUsedField = value;
            }
        }

        [XmlIgnore]
        public bool unhideWhenUsedSpecified
        {
            get
            {
                return this.unhideWhenUsedFieldSpecified;
            }
            set
            {
                this.unhideWhenUsedFieldSpecified = value;
            }
        }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public ST_OnOff qFormat
        {
            get
            {
                return this.qFormatField;
            }
            set
            {
                this.qFormatField = value;
            }
        }

        [XmlIgnore]
        public bool qFormatSpecified
        {
            get
            {
                return this.qFormatFieldSpecified;
            }
            set
            {
                this.qFormatFieldSpecified = value;
            }
        }
    }


    [Serializable]

    [XmlType(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main")]
    [XmlRoot(Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main", IsNullable = true)]
    public class CT_TrackChangeNumbering : CT_TrackChange
    {

        private string originalField;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string original
        {
            get
            {
                return this.originalField;
            }
            set
            {
                this.originalField = value;
            }
        }
    }

}
