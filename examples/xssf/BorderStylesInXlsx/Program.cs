﻿using System;
using System.Collections.Generic;
using System.Text;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.Util;

namespace NPOI.Examples.XSSF.BorderStylesInXlsx
{
    class Program
    {
        static void Main(string[] args)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet A1");
            IRow row = sheet.CreateRow(1);
            // Create a cell and put a value in it.
            ICell cell = row.CreateCell(1);
            cell.SetCellValue(4);

            // Style the cell with borders all around.
            ICellStyle style = workbook.CreateCellStyle();
            style.BorderBottom = BorderStyle.Thin;
            style.BottomBorderColor = HSSFColor.Black.Index;
            style.BorderLeft = BorderStyle.DashDotDot;
            style.LeftBorderColor = HSSFColor.Green.Index;
            style.BorderRight = BorderStyle.Hair;
            style.RightBorderColor = HSSFColor.Blue.Index;
            style.BorderTop = BorderStyle.MediumDashed;
            style.TopBorderColor = HSSFColor.Orange.Index;

            //create border diagonal
            style.BorderDiagonalLineStyle = BorderStyle.Medium; //this property must be set before BorderDiagonal and BorderDiagonalColor
            style.BorderDiagonal = BorderDiagonal.Forward;
            style.BorderDiagonalColor = HSSFColor.Gold.Index;

            cell.CellStyle = style;
            // Create a cell and put a value in it.
            ICell cell2 = row.CreateCell(2);
            cell2.SetCellValue(5);
            ICellStyle style2 = workbook.CreateCellStyle();
            style2.BorderDiagonalLineStyle = BorderStyle.Medium;
            style2.BorderDiagonal = BorderDiagonal.Backward;
            style2.BorderDiagonalColor = HSSFColor.Red.Index;
            cell2.CellStyle = style2;

            FileStream sw = File.Create("test.xlsx");
            workbook.Write(sw);
            sw.Close();
        }
    }
}
