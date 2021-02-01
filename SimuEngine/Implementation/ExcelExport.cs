using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using SimuEngine;
using Core;

namespace Implementation {
    class ExcelExport {
        ExcelPackage excel;
        ExcelWorksheet sheet;
        int tick;

        public ExcelExport() {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            excel = new ExcelPackage();
            sheet = excel.Workbook.Worksheets.Add("Sheet 1");
            tick = 0;
        }

        public void Save(System.IO.FileInfo file) {
            excel.SaveAs(file);
        }

        public void OnTick(object sender, Engine engine) {
            Dictionary<string, int> statCount = new Dictionary<string, int>();
            Dictionary<string, (int, int)> traitStuff = new Dictionary<string, (int, int)>();
            foreach (District district in engine.system.graph.Nodes[0].SubGraph.Nodes.Cast<District>()) {
                foreach (Person person in district.SubGraph.Nodes.Cast<Person>()) {
                    foreach (string status in person.statuses) {
                        if (statCount.TryGetValue(status, out int _)) {
                            statCount[status]++;
                        } else {
                            statCount[status] = 1;
                        }
                    }

                    foreach ((string trait, int val) in person.traits.Select(x => (x.Key, x.Value))) {
                        if (traitStuff.TryGetValue(trait, out (int, int) stuff)) {
                            int sum = stuff.Item1;
                            int count = stuff.Item2;

                            traitStuff[trait] = (sum + val, count + 1);
                        } else {
                            traitStuff[trait] = (val, 1);
                        }
                    }
                }
            }

            int rowIndex = 1;
            sheet.Cells[rowIndex, 1, rowIndex, 2].Style.Font.Bold = true;
            sheet.Cells[rowIndex, 2].Value = "Count";
            sheet.Cells[rowIndex++, 1].Value = "Statuses";
            foreach ((string status, int count) in from s in statCount orderby s.Key ascending select (s.Key, s.Value)) {
                sheet.Cells[rowIndex, 1].Value = status;
                sheet.Cells[rowIndex, 2].Value = count;
                rowIndex++;
            }

            sheet.Cells[rowIndex, 1].Value = "Traits";
            sheet.Cells[rowIndex, 2].Value = "Count";
            sheet.Cells[rowIndex, 3].Value = "Average";
            sheet.Cells[rowIndex, 1, rowIndex, 3].Style.Font.Bold = true;
            rowIndex++;
            foreach ((string trait, (int sum, int count)) in
                from t in traitStuff orderby t.Key ascending select (t.Key, t.Value)) {

                sheet.Cells[rowIndex, 1].Value = trait;
                sheet.Cells[rowIndex, 2].Value = count;
                sheet.Cells[rowIndex, 3].Value = (double)sum / count;
            }
        }

        ~ExcelExport() {
            excel.Dispose();
        }
    }
}
