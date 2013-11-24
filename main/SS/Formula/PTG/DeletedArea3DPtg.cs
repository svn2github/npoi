/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) Under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You Under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed Under the License is distributed on an "AS Is" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations Under the License.
==================================================================== */

namespace NPOI.SS.Formula.PTG
{
    using System;
    using NPOI.Util;
    
    using NPOI.SS.Formula;

    using NPOI.HSSF.UserModel;

    /**
     * Title:        Deleted Area 3D Ptg - 3D referecnce (Sheet + Area)
     * Description:  Defined a area in Extern Sheet. 
     * REFERENCE:  
     * @author Patrick Luby
     * @version 1.0-pre
     */
    public class DeletedArea3DPtg : OperandPtg, WorkbookDependentFormula
    {
        public const byte sid = 0x3d;
        private int field_1_index_extern_sheet;
        private int unused1;
        private int unused2;

        public DeletedArea3DPtg(int externSheetIndex)
        {
            field_1_index_extern_sheet = externSheetIndex;
            unused1 = 0;
            unused2 = 0;
        }

        public DeletedArea3DPtg(ILittleEndianInput in1)
        {
            field_1_index_extern_sheet = in1.ReadUShort();
            unused1 = in1.ReadInt();
            unused2 = in1.ReadInt();
        }
        public String ToFormulaString(IFormulaRenderingWorkbook book)
        {
            return ExternSheetNameResolver.PrependSheetName(book, field_1_index_extern_sheet,
                    HSSFErrorConstants.GetText(HSSFErrorConstants.ERROR_REF));
        }
        public override String ToFormulaString()
        {
            throw new Exception("3D references need a workbook to determine formula text");
        }
        public override byte DefaultOperandClass
        {
            get { return Ptg.CLASS_REF; }
        }
        public override int Size
        {
            get { return 11; }
        }
        public override void Write(ILittleEndianOutput out1)
        {
            out1.WriteByte(sid + PtgClass);
            out1.WriteShort(field_1_index_extern_sheet);
            out1.WriteInt(unused1);
            out1.WriteInt(unused2);
        }
    }
}