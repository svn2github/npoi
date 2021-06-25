/* ====================================================================
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



namespace NPOI.SS.Formula
{

    using System;
    using System.Text;
    using NPOI.SS.Formula.PTG;
    /**
     * @author Josh Micich
     */
    public class FormulaShifter
    {
        private enum ShiftMode
        {
            RowMove,
            RowCopy,
            SheetMove
        }
        /**
         * Extern sheet index of sheet where moving is occurring
         */
        private int _externSheetIndex;
        /**
         * Sheet name of the sheet where moving is occurring, 
         *  used for updating XSSF style 3D references on row shifts.
         */
        private String _sheetName;
        private int _firstMovedIndex;
        private int _lastMovedIndex;
        private int _amountToMove;
        private int _srcSheetIndex;
        private int _dstSheetIndex;
        private SpreadsheetVersion _version;

        private ShiftMode _mode;

        /**
         * Create an instance for Shifting row.
         *
         * For example, this will be called on {@link NPOI.HSSF.UserModel.HSSFSheet#ShiftRows(int, int, int)} }
         */
        private FormulaShifter(int externSheetIndex, String sheetName, int firstMovedIndex, int lastMovedIndex, int amountToMove,
            ShiftMode mode, SpreadsheetVersion version)
        {
            if (amountToMove == 0)
            {
                throw new ArgumentException("amountToMove must not be zero");
            }
            if (firstMovedIndex > lastMovedIndex)
            {
                throw new ArgumentException("firstMovedIndex, lastMovedIndex out of order");
            }
            _externSheetIndex = externSheetIndex;
            _sheetName = sheetName;
            _firstMovedIndex = firstMovedIndex;
            _lastMovedIndex = lastMovedIndex;
            _amountToMove = amountToMove;
            _mode = mode;
            _version = version;

            _srcSheetIndex = _dstSheetIndex = -1;
        }

        /**
        * Create an instance for shifting sheets.
        *
        * For example, this will be called on {@link org.apache.poi.hssf.usermodel.HSSFWorkbook#setSheetOrder(String, int)}  
        */
        private FormulaShifter(int srcSheetIndex, int dstSheetIndex)
        {
            _externSheetIndex = _firstMovedIndex = _lastMovedIndex = _amountToMove = -1;
            _sheetName = null;
            _version = null;

            _srcSheetIndex = srcSheetIndex;
            _dstSheetIndex = dstSheetIndex;
            _mode = ShiftMode.SheetMove;
        }

        [Obsolete("deprecated As of 3.14 beta 1 (November 2015), replaced by CreateForRowShift(int, String, int, int, int, SpreadsheetVersion)")]
        public static FormulaShifter CreateForRowShift(int externSheetIndex, String sheetName, int firstMovedRowIndex, int lastMovedRowIndex, int numberOfRowsToMove)
        {
            return CreateForRowShift(externSheetIndex, sheetName, firstMovedRowIndex, lastMovedRowIndex, numberOfRowsToMove, SpreadsheetVersion.EXCEL97);
        }

        public static FormulaShifter CreateForRowShift(int externSheetIndex, String sheetName, int firstMovedRowIndex, int lastMovedRowIndex, int numberOfRowsToMove,
            SpreadsheetVersion version)
        {
            return new FormulaShifter(externSheetIndex, sheetName, firstMovedRowIndex, lastMovedRowIndex, numberOfRowsToMove, ShiftMode.RowMove, version);
        }

        public static FormulaShifter CreateForRowCopy(int externSheetIndex, String sheetName, int firstMovedRowIndex, int lastMovedRowIndex, int numberOfRowsToMove,
                SpreadsheetVersion version)
        {
            return new FormulaShifter(externSheetIndex, sheetName, firstMovedRowIndex, lastMovedRowIndex, numberOfRowsToMove, ShiftMode.RowCopy, version);
        }

        public static FormulaShifter CreateForSheetShift(int srcSheetIndex, int dstSheetIndex)
        {
            return new FormulaShifter(srcSheetIndex, dstSheetIndex);
        }
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GetType().Name);
            sb.Append(" [");
            sb.Append(_firstMovedIndex);
            sb.Append(_lastMovedIndex);
            sb.Append(_amountToMove);
            return sb.ToString();
        }

        /**
         * @param ptgs - if necessary, will get modified by this method
         * @param currentExternSheetIx - the extern sheet index of the sheet that contains the formula being adjusted
         * @return <c>true</c> if a change was made to the formula tokens
         */
        public bool AdjustFormula(Ptg[] ptgs, int currentExternSheetIx)
        {
            bool refsWereChanged = false;
            for (int i = 0; i < ptgs.Length; i++)
            {
                Ptg newPtg = AdjustPtg(ptgs[i], currentExternSheetIx);
                if (newPtg != null)
                {
                    refsWereChanged = true;
                    ptgs[i] = newPtg;
                }
            }
            return refsWereChanged;
        }

        private Ptg AdjustPtg(Ptg ptg, int currentExternSheetIx)
        {
            //return AdjustPtgDueToRowMove(ptg, currentExternSheetIx);
            switch (_mode)
            {
                case ShiftMode.RowMove:
                    return AdjustPtgDueToRowMove(ptg, currentExternSheetIx);
                case ShiftMode.RowCopy:
                    // Covered Scenarios:
                    // * row copy on same sheet
                    // * row copy between different sheetsin the same workbook
                    return AdjustPtgDueToRowCopy(ptg);
                case ShiftMode.SheetMove:
                    return AdjustPtgDueToSheetMove(ptg);
                default:
                    throw new InvalidOperationException("Unsupported shift mode: " + _mode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptg"></param>
        /// <param name="currentExternSheetIx"></param>
        /// <returns>in-place modified ptg (if row move would cause Ptg to change),
        /// deleted ref ptg (if row move causes an error),
        /// or null (if no Ptg change is needed)</returns>
        private Ptg AdjustPtgDueToRowMove(Ptg ptg, int currentExternSheetIx)
        {
            if (ptg is RefPtg)
            {
                if (currentExternSheetIx != _externSheetIndex)
                {
                    // local refs on other sheets are unaffected
                    return null;
                }
                RefPtg rptg = (RefPtg)ptg;
                return RowMoveRefPtg(rptg);
            }
            if (ptg is Ref3DPtg)
            {
                Ref3DPtg rptg = (Ref3DPtg)ptg;
                if (_externSheetIndex != rptg.ExternSheetIndex)
                {
                    // only move 3D refs that refer to the sheet with cells being moved
                    // (currentExternSheetIx is irrelevant)
                    return null;
                }
                return RowMoveRefPtg(rptg);
            }

            if (ptg is Ref3DPxg)
            {
                Ref3DPxg rpxg = (Ref3DPxg)ptg;
                if (rpxg.ExternalWorkbookNumber > 0 ||
                       !_sheetName.Equals(rpxg.SheetName))
                {
                    // only move 3D refs that refer to the sheet with cells being moved
                    return null;
                }
                return RowMoveRefPtg(rpxg);
            }
            if (ptg is Area2DPtgBase)
            {
                if (currentExternSheetIx != _externSheetIndex)
                {
                    // local refs on other sheets are unaffected
                    return ptg;
                }
                return RowMoveAreaPtg((Area2DPtgBase)ptg);
            }
            if (ptg is Area3DPtg)
            {
                Area3DPtg aptg = (Area3DPtg)ptg;
                if (_externSheetIndex != aptg.ExternSheetIndex)
                {
                    // only move 3D refs that refer to the sheet with cells being moved
                    // (currentExternSheetIx is irrelevant)
                    return null;
                }
                return RowMoveAreaPtg(aptg);
            }

            if (ptg is Area3DPxg)
            {
                Area3DPxg apxg = (Area3DPxg)ptg;
                if (apxg.ExternalWorkbookNumber > 0 ||
                        !_sheetName.Equals(apxg.SheetName))
                {
                    // only move 3D refs that refer to the sheet with cells being moved
                    return null;
                }
                return RowMoveAreaPtg(apxg);
            }
            return null;
        }

        /**
        * Call this on any ptg reference contained in a row of cells that was copied.
        * If the ptg reference is relative, the references will be shifted by the distance
        * that the rows were copied.
        * In the future similar functions could be written due to column copying or
        * individual cell copying. Just make sure to only call adjustPtgDueToRowCopy on
        * formula cells that are copied (unless row shifting, where references outside
        * of the shifted region need to be updated to reflect the shift, a copy is self-contained).
        * 
        * @param ptg the ptg to shift
        * @return deleted ref ptg, in-place modified ptg, or null
        * If Ptg would be shifted off the first or last row of a sheet, return deleted ref
        * If Ptg needs to be changed, modifies Ptg in-place
        * If Ptg doesn't need to be changed, returns <code>null</code>
        */
        private Ptg AdjustPtgDueToRowCopy(Ptg ptg)
        {
            if (ptg is RefPtg)
            {
                RefPtg rptg = (RefPtg)ptg;
                return RowCopyRefPtg(rptg);
            }
            if (ptg is Ref3DPtg)
            {
                Ref3DPtg rptg = (Ref3DPtg)ptg;
                return RowCopyRefPtg(rptg);
            }
            if (ptg is Ref3DPxg)
            {
                Ref3DPxg rpxg = (Ref3DPxg)ptg;
                return RowCopyRefPtg(rpxg);
            }
            if (ptg is Area2DPtgBase)
            {
                return RowCopyAreaPtg((Area2DPtgBase)ptg);
            }
            if (ptg is Area3DPtg)
            {
                Area3DPtg aptg = (Area3DPtg)ptg;
                return RowCopyAreaPtg(aptg);
            }
            if (ptg is Area3DPxg)
            {
                Area3DPxg apxg = (Area3DPxg)ptg;
                return RowCopyAreaPtg(apxg);
            }
            return null;
        }

        private Ptg AdjustPtgDueToSheetMove(Ptg ptg)
        {
            if (ptg is Ref3DPtg)
            {
                Ref3DPtg ref1 = (Ref3DPtg)ptg;
                int oldSheetIndex = ref1.ExternSheetIndex;

                // we have to handle a few cases here

                // 1. sheet is outside moved sheets, no change necessary
                if (oldSheetIndex < _srcSheetIndex &&
                        oldSheetIndex < _dstSheetIndex)
                {
                    return null;
                }
                if (oldSheetIndex > _srcSheetIndex &&
                        oldSheetIndex > _dstSheetIndex)
                {
                    return null;
                }

                // 2. ptg refers to the moved sheet
                if (oldSheetIndex == _srcSheetIndex)
                {
                    ref1.ExternSheetIndex = (_dstSheetIndex);
                    return ref1;
                }

                // 3. new index is lower than old one => sheets get moved up
                if (_dstSheetIndex < _srcSheetIndex)
                {
                    ref1.ExternSheetIndex = (oldSheetIndex + 1);
                    return ref1;
                }

                // 4. new index is higher than old one => sheets get moved down
                if (_dstSheetIndex > _srcSheetIndex)
                {
                    ref1.ExternSheetIndex = (oldSheetIndex - 1);
                    return ref1;
                }
            }
            return null;
        }
        private Ptg RowMoveRefPtg(RefPtgBase rptg)
        {
            int refRow = rptg.Row;
            if (_firstMovedIndex <= refRow && refRow <= _lastMovedIndex)
            {
                // Rows being moved completely enclose the ref.
                // - move the area ref along with the rows regardless of destination
                rptg.Row = (refRow + _amountToMove);
                return rptg;
            }
            // else rules for adjusting area may also depend on the destination of the moved rows

            int destFirstRowIndex = _firstMovedIndex + _amountToMove;
            int destLastRowIndex = _lastMovedIndex + _amountToMove;

            // ref is outside source rows
            // check for clashes with destination

            if (destLastRowIndex < refRow || refRow < destFirstRowIndex)
            {
                // destination rows are completely outside ref
                return null;
            }

            if (destFirstRowIndex <= refRow && refRow <= destLastRowIndex)
            {
                // destination rows enclose the area (possibly exactly)
                return CreateDeletedRef(rptg);
            }
            throw new InvalidOperationException("Situation not covered: (" + _firstMovedIndex + ", " +
                        _lastMovedIndex + ", " + _amountToMove + ", " + refRow + ", " + refRow + ")");
        }

        private Ptg RowMoveAreaPtg(AreaPtgBase aptg)
        {
            int aFirstRow = aptg.FirstRow;
            int aLastRow = aptg.LastRow;
            if (_firstMovedIndex <= aFirstRow && aLastRow <= _lastMovedIndex)
            {
                // Rows being moved completely enclose the area ref.
                // - move the area ref along with the rows regardless of destination
                aptg.FirstRow = (aFirstRow + _amountToMove);
                aptg.LastRow = (aLastRow + _amountToMove);
                return aptg;
            }
            // else rules for adjusting area may also depend on the destination of the moved rows

            int destFirstRowIndex = _firstMovedIndex + _amountToMove;
            int destLastRowIndex = _lastMovedIndex + _amountToMove;

            if (aFirstRow < _firstMovedIndex && _lastMovedIndex < aLastRow)
            {
                // Rows moved were originally *completely* within the area ref

                // If the destination of the rows overlaps either the top
                // or bottom of the area ref there will be a change
                if (destFirstRowIndex < aFirstRow && aFirstRow <= destLastRowIndex)
                {
                    // truncate the top of the area by the moved rows
                    aptg.FirstRow = (destLastRowIndex + 1);
                    return aptg;
                }
                else if (destFirstRowIndex <= aLastRow && aLastRow < destLastRowIndex)
                {
                    // truncate the bottom of the area by the moved rows
                    aptg.LastRow = (destFirstRowIndex - 1);
                    return aptg;
                }
                // else - rows have moved completely outside the area ref,
                // or still remain completely within the area ref
                return null; // - no change to the area
            }
            if (_firstMovedIndex <= aFirstRow && aFirstRow <= _lastMovedIndex)
            {
                // Rows moved include the first row of the area ref, but not the last row
                // btw: (aLastRow > _lastMovedIndex)
                if (_amountToMove < 0)
                {
                    // simple case - expand area by shifting top upward
                    aptg.FirstRow = (aFirstRow + _amountToMove);
                    return aptg;
                }
                if (destFirstRowIndex > aLastRow)
                {
                    // in this case, excel ignores the row move
                    return null;
                }
                int newFirstRowIx = aFirstRow + _amountToMove;
                if (destLastRowIndex < aLastRow)
                {
                    // end of area is preserved (will remain exact same row)
                    // the top area row is moved simply
                    aptg.FirstRow = (newFirstRowIx);
                    return aptg;
                }
                // else - bottom area row has been replaced - both area top and bottom may move now
                int areaRemainingTopRowIx = _lastMovedIndex + 1;
                if (destFirstRowIndex > areaRemainingTopRowIx)
                {
                    // old top row of area has moved deep within the area, and exposed a new top row
                    newFirstRowIx = areaRemainingTopRowIx;
                }
                aptg.FirstRow = (newFirstRowIx);
                aptg.LastRow = (Math.Max(aLastRow, destLastRowIndex));
                return aptg;
            }
            if (_firstMovedIndex <= aLastRow && aLastRow <= _lastMovedIndex)
            {
                // Rows moved include the last row of the area ref, but not the first
                // btw: (aFirstRow < _firstMovedIndex)
                if (_amountToMove > 0)
                {
                    // simple case - expand area by shifting bottom downward
                    aptg.LastRow = (aLastRow + _amountToMove);
                    return aptg;
                }
                if (destLastRowIndex < aFirstRow)
                {
                    // in this case, excel ignores the row move
                    return null;
                }
                int newLastRowIx = aLastRow + _amountToMove;
                if (destFirstRowIndex > aFirstRow)
                {
                    // top of area is preserved (will remain exact same row)
                    // the bottom area row is moved simply
                    aptg.LastRow = (newLastRowIx);
                    return aptg;
                }
                // else - top area row has been replaced - both area top and bottom may move now
                int areaRemainingBottomRowIx = _firstMovedIndex - 1;
                if (destLastRowIndex < areaRemainingBottomRowIx)
                {
                    // old bottom row of area has moved up deep within the area, and exposed a new bottom row
                    newLastRowIx = areaRemainingBottomRowIx;
                }
                aptg.FirstRow = (Math.Min(aFirstRow, destFirstRowIndex));
                aptg.LastRow = (newLastRowIx);
                return aptg;
            }
            // else source rows include none of the rows of the area ref
            // check for clashes with destination

            if (destLastRowIndex < aFirstRow || aLastRow < destFirstRowIndex)
            {
                // destination rows are completely outside area ref
                return null;
            }

            if (destFirstRowIndex <= aFirstRow && aLastRow <= destLastRowIndex)
            {
                // destination rows enclose the area (possibly exactly)
                return CreateDeletedRef(aptg);
            }

            if (aFirstRow <= destFirstRowIndex && destLastRowIndex <= aLastRow)
            {
                // destination rows are within area ref (possibly exact on top or bottom, but not both)
                return null; // - no change to area
            }

            if (destFirstRowIndex < aFirstRow && aFirstRow <= destLastRowIndex)
            {
                // dest rows overlap top of area
                // - truncate the top
                aptg.FirstRow = (destLastRowIndex + 1);
                return aptg;
            }
            if (destFirstRowIndex <= aLastRow && aLastRow < destLastRowIndex)
            {
                // dest rows overlap bottom of area
                // - truncate the bottom
                aptg.LastRow = (destFirstRowIndex - 1);
                return aptg;
            }
            throw new InvalidOperationException("Situation not covered: (" + _firstMovedIndex + ", " +
                        _lastMovedIndex + ", " + _amountToMove + ", " + aFirstRow + ", " + aLastRow + ")");
        }


        /**
         * Modifies rptg in-place and return a reference to rptg if the cell reference
         * would move due to a row copy operation
         * Returns <code>null</code> or {@link #RefErrorPtg} if no change was made
         *
         * @param aptg
         * @return
         */
        private Ptg RowCopyRefPtg(RefPtgBase rptg)
        {
            int refRow = rptg.Row;
            if (rptg.IsRowRelative)
            {
                int destRowIndex = _firstMovedIndex + _amountToMove;
                if (destRowIndex < 0 || _version.LastRowIndex < destRowIndex)
                    return CreateDeletedRef(rptg);
                rptg.Row = (refRow + _amountToMove);
                return rptg;
            }
            return null;
        }

        /**
         * Modifies aptg in-place and return a reference to aptg if the first or last row of
         * of the Area reference would move due to a row copy operation
         * Returns <code>null</code> or {@link #AreaErrPtg} if no change was made
         *
         * @param aptg
         * @return null, AreaErrPtg, or modified aptg
         */
        private Ptg RowCopyAreaPtg(AreaPtgBase aptg)
        {
            bool changed = false;

            int aFirstRow = aptg.FirstRow;
            int aLastRow = aptg.LastRow;

            if (aptg.IsFirstRowRelative)
            {
                int destFirstRowIndex = aFirstRow + _amountToMove;
                if (destFirstRowIndex < 0 || _version.LastRowIndex < destFirstRowIndex)
                    return CreateDeletedRef(aptg);
                aptg.FirstRow = (destFirstRowIndex);
                changed = true;
            }
            if (aptg.IsLastRowRelative)
            {
                int destLastRowIndex = aLastRow + _amountToMove;
                if (destLastRowIndex < 0 || _version.LastRowIndex < destLastRowIndex)
                    return CreateDeletedRef(aptg);
                aptg.LastRow = (destLastRowIndex);
                changed = true;
            }
            if (changed)
            {
                aptg.SortTopLeftToBottomRight();
            }

            return changed ? aptg : null;
        }

        private static Ptg CreateDeletedRef(Ptg ptg)
        {
            if (ptg is RefPtg)
            {
                return new RefErrorPtg();
            }
            if (ptg is Ref3DPtg)
            {
                Ref3DPtg rptg = (Ref3DPtg)ptg;
                return new DeletedRef3DPtg(rptg.ExternSheetIndex);
            }
            if (ptg is AreaPtg)
            {
                return new AreaErrPtg();
            }
            if (ptg is Area3DPtg)
            {
                Area3DPtg area3DPtg = (Area3DPtg)ptg;
                return new DeletedArea3DPtg(area3DPtg.ExternSheetIndex);
            }
            if (ptg is Ref3DPxg)
            {
                Ref3DPxg pxg = (Ref3DPxg)ptg;
                return new Deleted3DPxg(pxg.ExternalWorkbookNumber, pxg.SheetName);
            }
            if (ptg is Area3DPxg)
            {
                Area3DPxg pxg = (Area3DPxg)ptg;
                return new Deleted3DPxg(pxg.ExternalWorkbookNumber, pxg.SheetName);
            }
            throw new ArgumentException("Unexpected ref ptg class (" + ptg.GetType().Name + ")");
        }
    }
}