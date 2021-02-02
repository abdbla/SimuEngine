using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using SimuEngine;
using Core;
using NodeMonog;

namespace Implementation {
    class ExcelExport {
        ExcelPackage excel;
        ExcelWorksheet sheet;
        TickStates history;
        int tick;

        public ExcelExport() {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            excel = new ExcelPackage();
            sheet = excel.Workbook.Worksheets.Add("Sheet 1");
            history = new TickStates();
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
            bool updateAll = history.AddStats(statCount, tick)
                          || history.AddTraits(traitStuff, tick);
            tick++;

#if false
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
#endif
            UpdateSheet();
        }

        void UpdateSheet() {
            lock (excel) {
                sheet.Cells.Clear();

                int rowIndex = 1;
                sheet.Cells[rowIndex, 1].Style.Font.Bold = true;
                sheet.Cells[rowIndex, 1].Value = "Statuses";
                rowIndex++;
                foreach ((string status, List<int?> counts) in history.statCount.Select(x => (x.Key, x.Value))) {
                    sheet.Cells[rowIndex, 1].Value = status;
                    for (int i = 0; i < counts.Count; i++) {
                        sheet.Cells[rowIndex, i + 2].Value = counts[i];
                    }
                    rowIndex++;
                }
                sheet.Cells[rowIndex, 1].Style.Font.Bold = true;
                sheet.Cells[rowIndex, 1].Value = "Traits";
                rowIndex++;
                foreach ((string trait, List<(int, int)?> vals) in history.traitStuff.Select(x => (x.Key, x.Value))) {
                    sheet.Cells[rowIndex, 1].Value = trait;
                    sheet.Cells[rowIndex + 1, 1].Value = trait + " (count)";
                    for (int i = 0; i < vals.Count; i++) {
                        if (vals[i] != null) {
                            sheet.Cells[rowIndex, i + 2].Value = vals[i]?.Item1 / (double)vals[i]?.Item2;
                            sheet.Cells[rowIndex + 1, i + 2].Value = vals[i]?.Item2;
                        } else {

                        }
                    }
                    rowIndex += 2;
                }
            }
        }

        ~ExcelExport() {
            excel.Dispose();
        }
    }

    class TickStates {
        public Dictionary<string, List<int?>> statCount;
        public Dictionary<string, List<(int, int)?>> traitStuff;

        public TickStates() {
            statCount = new Dictionary<string, List<int?>>();
            traitStuff = new Dictionary<string, List<(int, int)?>>();
        }

        public bool AddStats(Dictionary<string, int> stats, int tick) {
            bool statCountChanged = false;
            foreach ((string stat, int count) in stats.Select(kv => (kv.Key, kv.Value))) {
                if (statCount.TryGetValue(stat, out List<int?> timeSeries)) {
                    timeSeries.Add(count);
                } else {
                    statCountChanged = true;
                    List<int?> newStatList = new List<int?>();
                    for (int i = 0; i < tick; i++) {
                        newStatList.Add(null);
                    }
                    newStatList.Add(count);
                    statCount.Add(stat, newStatList);
                }
            }
            return statCountChanged;
        }

        public bool AddTraits(Dictionary<string, (int, int)> traits, int tick) {
            bool traitCountChanged = false;
            foreach ((string trait, (int sum, int count)) in traits.Select(kv => (kv.Key, kv.Value))) {
                if (traitStuff.TryGetValue(trait, out List<(int, int)?> timeSeries)) {
                    timeSeries.Add((sum, count));
                } else {
                    traitCountChanged = true;
                    List<(int, int)?> newTraitList = new List<(int, int)?>();
                    for (int i = 0; i < tick; i++) {
                        newTraitList.Add(null);
                    }
                    newTraitList.Add((sum, count));
                    traitStuff.Add(trait, newTraitList);
                }
            }
            return traitCountChanged;
        }
    }
}
