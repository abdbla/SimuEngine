using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using SimuEngine;
using Core;
using NodeMonog;
using OfficeOpenXml.Drawing.Chart;

namespace Implementation {
    class ExcelExport {
        ExcelPackage excel;
        ExcelWorksheet statusSheet;
        ExcelWorksheet traitSheet;
        ExcelWorksheet districtSheet;
        TickStates history;
        int tick;

        public ExcelExport() {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            excel = new ExcelPackage();
            statusSheet = excel.Workbook.Worksheets.Add("Statuses");
            traitSheet = excel.Workbook.Worksheets.Add("Traits");
            districtSheet = excel.Workbook.Worksheets.Add("District stats");
            history = new TickStates();
            tick = 0;
        }

        public void Save(System.IO.FileInfo file) {
            excel.SaveAs(file);
        }

        public void OnTick(object sender, Engine engine) {
            Dictionary<string, int> statCount = new Dictionary<string, int>();
            Dictionary<string, (int sum, int count)> traitStuff = new Dictionary<string, (int, int)>();
            Dictionary<string, int> testCapacity = new Dictionary<string, int>();

            int districtCount = engine.system.graph.Nodes[0].SubGraph.Nodes.Cast<District>().Count();
            statCount["Population"] = 0;

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

                    statCount["Population"]++;
                }

                testCapacity.Add(district.Name, district.traits["Testing Capacity"]);
            }
            history.AddStats(statCount, tick);
            history.AddTraits(traitStuff, tick);
            history.AddDistricts(testCapacity, tick);

            tick++;

            UpdateSheet();
        }

        ExcelRange StatusValueRange(string name) {
            (string _, List<int?> series, int index) = history.statCount.OrderBy(kv => kv.Key)
                .Select((kv, i) => (kv.Key, kv.Value, i))
                .First(x => x.Item1 == name);

            return statusSheet.Cells[2, 2 + index,
                                     2 + series.Count - 1, 2 + index];
        }

        ExcelRange TraitValueRange(string name) {
            (string _, List<(int, int)?> series, int index) = history.traitStuff.OrderBy(kv => kv.Key)
                .Select((kv, i) => (kv.Key, kv.Value, i))
                .First(x => x.Item1 == name);

            const int FIRST_COL = 2;
            const int OFFSET = 0;

            var col = FIRST_COL + 2 * index + OFFSET;

            return traitSheet.Cells[2, col,
                                    2 + series.Count - 1, col];
        }

        ExcelRange TraitCountRange(string name) {
            (string _, List<(int, int)?> series, int index) = history.traitStuff.OrderBy(kv => kv.Key)
                .Select((kv, i) => (kv.Key, kv.Value, i))
                .First(x => x.Item1 == name);

            const int FIRST_COL = 2;
            const int OFFSET = 1;

            var col = FIRST_COL + 2 * index + OFFSET;

            return traitSheet.Cells[2, col,
                                    2 + series.Count - 1, col];
        }

        ExcelAreaChart CreateStatusBarChart() {
            ExcelAreaChart chart = statusSheet.Drawings.AddAreaChart("Healthy/Infected/Dead/Recovered", eAreaChartType.AreaStacked);

            var rangeLabel = statusSheet.Cells[2, 1,
                                               2 + tick - 1, 1];

            foreach (var status in new[] { "Healthy", "Dead", "Infected", "Recovered" }) {
                var range = StatusValueRange(status);
                var series = chart.Series.Add(range, rangeLabel);
                series.Header = status;
            }

            var lineChart = chart.PlotArea.ChartTypes.AddLineChart(eLineChartType.Line);
            var cumulativeSeries = lineChart.Series.Add(StatusValueRange("Cumulative Infection"), rangeLabel);
            cumulativeSeries.Header = "Cumulative Infection";

            return chart;
        }

        ExcelLineChart CreateStatusLineChart() {
            ExcelLineChart chart = statusSheet.Drawings.AddLineChart("Statuses over time", eLineChartType.Line);
            // all statuses that don't remain constant over their whole series
            // ordered from the start by key because we need .Select to return the right index
            var nonConsts = (from item in history.statCount.OrderBy(x => x.Key).Select((x, i) => (x, i))
                             let timeSeries = item.Item1.Value
                             let Index = item.Item2
                             let Name = item.Item1.Key
                             where timeSeries.Distinct().Count() > 1
                             select new { Name, timeSeries.Count, Index }).ToList();

            var rangeLabel = statusSheet.Cells[2, 1,
                                               2 + tick - 1, 1];
            foreach (var status in nonConsts) {
                const int FIRST_STATUS_COL = 2;
                const int DATA_ROW_START = 2;
                int col = FIRST_STATUS_COL + status.Index;
                int rowStart = DATA_ROW_START;
                int rowEnd = rowStart + status.Count - 1;
                var dataRange = statusSheet.Cells[rowStart, col,
                                                  rowEnd,   col];

                var series = chart.Series.Add(dataRange, rangeLabel);
                series.Header = status.Name;
            }

            return chart;
        }

        void UpdateStatusSheet() {
            statusSheet.Cells.Clear();

            statusSheet.Cells[1, 1].Style.Font.Bold = true;
            statusSheet.Cells[1, 1].Value = "Statuses";
            for (int i = 0; i < tick; i++) {
                statusSheet.Cells[i + 2, 1].Value = $"Day {i + 1}";
            }

            double width = 0;
            int colIndex = 2;
            foreach ((string status, List<int?> counts) in history.statCount.OrderBy(x => x.Key).Select(x => (x.Key, x.Value))) {
                statusSheet.Cells[1, colIndex].Value = status;
                for (int i = 0; i < counts.Count; i++) {
                    statusSheet.Cells[i + 2, colIndex].Value = counts[i];
                }
                statusSheet.Column(colIndex).AutoFit();
                width += statusSheet.Column(colIndex).Width;
                colIndex++;
            }
            statusSheet.Drawings.Clear();
            var lineChart = CreateStatusLineChart();
            lineChart.SetPosition(0, (int)(width / 0.1423) + 100);
            var barChart = CreateStatusBarChart();
            barChart.SetPosition(0, (int)(width / 0.1423) + 400);
        }

        void UpdateTraitSheet() {
            traitSheet.Cells[1, 1].Style.Font.Bold = true;
            traitSheet.Cells[1, 1].Value = "Traits";

            for (int i = 0; i < tick; i++) {
                traitSheet.Cells[i + 2, 1].Value = $"Day {i + 1}";
            }

            int colIndex = 2;
            foreach ((string trait, List<(int, int)?> vals) in history.traitStuff.OrderBy(x => x.Key).Select(x => (x.Key, x.Value))) {
                traitSheet.Cells[1, colIndex].Value = trait;
                traitSheet.Cells[1, colIndex + 1].Value = trait + " (count)";
                for (int i = 0; i < vals.Count; i++) {
                    if (vals[i] != null) {
                        traitSheet.Cells[i + 2, colIndex].Value = vals[i]?.Item1 / (double)vals[i]?.Item2;
                        traitSheet.Cells[i + 2, colIndex + 1].Value = vals[i]?.Item2;
                    } else {

                    }
                }
                traitSheet.Column(colIndex).AutoFit();
                traitSheet.Column(colIndex + 1).AutoFit();
                colIndex += 2;
            }

            traitSheet.Drawings.Clear();
            CreateAwarenessChart();
        }

        void UpdateDistrictSheet() {
            districtSheet.Cells.Clear();
            districtSheet.Cells[1, 1].Style.Font.Bold = true;
            districtSheet.Cells[1, 1].Value = "Districts";
            for (int i = 0; i < tick; i++) {
                districtSheet.Cells[i + 2, 1].Value = $"Day {i + 1}";
            }

            int colIndex = 2;
            foreach ((string district, List<int?> testingCapacities) in
                    history.districtStuff.OrderBy(x => x.Key).Select(x => (x.Key, x.Value))) {
                districtSheet.Cells[1, colIndex].Value = district;
                for (int i = 0; i < testingCapacities.Count; i++) {
                    districtSheet.Cells[2 + i, colIndex].Value = testingCapacities[i];
                }
                districtSheet.Column(colIndex).AutoFit();
                colIndex++;
            }
        }

        ExcelLineChart CreateAwarenessChart() {
            ExcelLineChart chart = traitSheet.Drawings.AddLineChart("Awareness", eLineChartType.Line);
            var labelRange = traitSheet.Cells[2, 1,
                                              2 + tick - 1, 1];

            var dataRange = TraitValueRange("Awareness");
            var series = chart.Series.Add(dataRange, labelRange);

            return chart;
        }

        void UpdateSheet() {
            lock (excel) {
                UpdateStatusSheet();
                UpdateTraitSheet();
                UpdateDistrictSheet();
            }
        }

        ~ExcelExport() {
            excel.Dispose();
        }
    }

    class TickStates {
        public Dictionary<string, List<int?>> statCount;
        public Dictionary<string, List<(int, int)?>> traitStuff;
        public Dictionary<string, List<int?>> districtStuff;

        public TickStates() {
            statCount = new Dictionary<string, List<int?>>();
            traitStuff = new Dictionary<string, List<(int, int)?>>();
            districtStuff = new Dictionary<string, List<int?>>();
        }

        public bool AddStats(Dictionary<string, int> stats, int tick) {
            return AddStuff(statCount, stats, tick);
        }

        public bool AddTraits(Dictionary<string, (int, int)> traits, int tick) {
            return AddStuff(traitStuff, traits, tick);
        }

        public bool AddDistricts(Dictionary<string, int> districts, int tick) {
            return AddStuff(districtStuff, districts, tick);
        }

        private static bool AddStuff<T>(Dictionary<string, List<T?>> dict, Dictionary<string, T> source, int tick)
            where T : struct
        {
            bool countChanged = false;
            foreach ((string key, List<T?> timeSeries) in dict.Select(x => (x.Key, x.Value))) {
                if (source.TryGetValue(key, out T newValue)) {
                    timeSeries.Add(newValue);
                    source.Remove(key);
                } else {
                    timeSeries.Add(null);
                }
            }
            foreach ((string newKey, T newVal) in source.Select(x => (x.Key, x.Value))) {
                countChanged = true;
                List<T?> newList = new List<T?>();
                for (int i = 0; i < tick; i++) {
                    newList.Add(null);
                }
                newList.Add(newVal);
                dict.Add(newKey, newList);
            }

            return countChanged;
        }
    }
}
