using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OSPSuite.Utility;
using SmartXLS;

namespace OSPSuite.Infrastructure.Services
{
   public static class ExportToExcelTask
   {
      /// <summary>
      ///    Exports the given datatable to the file given as parameter.
      /// </summary>
      /// <param name="dataTable">Data Table to export</param>
      /// <param name="fileName">Target file</param>
      /// <param name="openExcel">If set to true, excel will be launched with the exported file</param>
      /// <param name="workbookConfiguration">Specifies some configuration for the workbook (e.g. layout)</param>
      public static void ExportDataTableToExcel(DataTable dataTable, string fileName, bool openExcel, Action<WorkBook> workbookConfiguration = null)
      {
         ExportDataTablesToExcel(new[] {dataTable}, fileName, openExcel, workbookConfiguration);
      }

      /// <summary>
      ///    Exports the given datatables to the file given as parameter. One sheet will be created per table
      /// </summary>
      /// <param name="dataTables">Data Tables to export</param>
      /// <param name="fileName">Target file</param>
      /// <param name="openExcel">If set to true, excel will be launched with the exported file</param>
      /// <param name="workbookConfiguration">Specifies some configuration for the workbook (e.g. layout)</param>
      public static void ExportDataTablesToExcel(IEnumerable<DataTable> dataTables, string fileName, bool openExcel, Action<WorkBook, DataTable> workbookConfiguration)
      {
         var tables = dataTables.ToList();
         using (var workBook = new WorkBook())
         {
            if (tables.Count > 1)
               workBook.insertSheets(0, tables.Count - 1); //-1 because one is always available by default

            for (var i = 0; i < tables.Count(); i++)
            {
               var dataTable = tables.ElementAt(i);
               exportDataTableToWorkBook(workBook, dataTable, i);

               if (workbookConfiguration != null)
                  workbookConfiguration(workBook, dataTable);
            }

            saveWorkbook(fileName, workBook);
         }

         if (openExcel)
            FileHelper.TryOpenFile(fileName);
      }

      private static void saveWorkbook(string fileName, WorkBook workBook)
      {
         FileHelper.TrySaveFile(fileName, () =>
         {
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Extension.Contains("xlsx"))
               workBook.writeXLSX(fileName);
            else
               workBook.write(fileName);
         });
      }

      /// <summary>
      ///    Exports the given datatables to the file given as parameter. One sheet will be created per table
      /// </summary>
      /// <param name="dataTables">Data Tables to export</param>
      /// <param name="fileName">Target file</param>
      /// <param name="openExcel">If set to true, excel will be launched with the exported file</param>
      /// <param name="workbookConfiguration">Specifies some configuration for the workbook (e.g. layout)</param>
      public static void ExportDataTablesToExcel(IEnumerable<DataTable> dataTables, string fileName, bool openExcel, Action<WorkBook> workbookConfiguration = null)
      {
         ExportDataTablesToExcel(dataTables, fileName, openExcel, (wb, dt) =>
         {
            if (workbookConfiguration != null)
               workbookConfiguration(wb);
         });
      }

      private static void exportDataTableToWorkBook(WorkBook workBook, DataTable dataTable, int sheetIndex)
      {
         workBook.Sheet = sheetIndex;
         workBook.ImportDataTable(dataTable, true, 0, 0, dataTable.Rows.Count, dataTable.Columns.Count);
         workBook.setSheetName(sheetIndex, dataTable.TableName);
      }
   }
}