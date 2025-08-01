using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace AmcacheParser
{
    class Program
    {
        private static string art = @"
               _____                               .__             __________                                   
              /  _  \   _____   ____ _____    ____ |  |__   ____   \______   \_____ _______  ______ ___________ 
             /  /_\  \ /     \_/ ___\\__  \ _/ ___\|  |  \_/ __ \   |     ___/\__  \\_  __ \/  ___// __ \_  __ \
            /    |    \  Y Y  \  \___ / __ \\  \___|   Y  \  ___/   |    |     / __ \|  | \/\___ \\  ___/|  | \/
            \____|__  /__|_|  /\___  >____  /\___  >___|  /\___  >  |____|    (____  /__|  /____  >\___  >__|   
                    \/      \/     \/     \/     \/     \/     \/                  \/           \/     \/       

                                                   [ Made by Fantakk ]
                                                [ discord.gg/9sWEa5DeP8 ]
        ";

        private static string filterFilePath = null;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(art);

            Console.WriteLine("             Do you have a parse list? (y/n)");
            string response = Console.ReadLine()?.Trim().ToLower();
            if (response == "y")
            {
                Console.WriteLine("             Enter the path to the parse list file:");
                string inputPath = Console.ReadLine()?.Trim();

                if (File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    filterFilePath = inputPath;
                    Console.WriteLine($"Using filter file: {filterFilePath}");
                }
                else
                {
                    Console.WriteLine("Invalid filter file path or file is not a .txt. The filter will not be applied.");
                }
            }

            DumpAmcache();
            Console.ReadKey();
        }

        private static void DumpAmcache()
        {
            string url = "https://download.ericzimmermanstools.com/net6/AmcacheParser.zip";
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\EricZimmermanAmcacheParser.zip";

            Thread.Sleep(750);
            Console.WriteLine("             Downloading Dependencies...");
            Console.WriteLine($"             Saving file to: {path}");

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        if (e.Error != null)
                        {
                            Console.WriteLine($"             Download Error: {e.Error.Message}");
                        }
                        else
                        {
                            Console.WriteLine("             Success");
                        }
                    };

                    client.DownloadFileAsync(new Uri(url), path);

                    while (client.IsBusy)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"             Error: {ex.Message}");
            }

            try
            {
                Console.WriteLine($"             Extracting Files..");
                string zipPath = AppDomain.CurrentDomain.BaseDirectory + "\\EricZimmermanAmcacheParser.zip";
                string extractPath = AppDomain.CurrentDomain.BaseDirectory + "\\EricZimmermanAmcacheParser";
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                Console.WriteLine("             Success");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"             Error: {ex.Message}");
            }

            try
            {
                string zipPath = AppDomain.CurrentDomain.BaseDirectory + "\\EricZimmermanAmcacheParser.zip";
                Console.WriteLine($"             Deleting Trash..");
                File.Delete(path);
                Console.WriteLine("             Success");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"             Error: {ex.Message}");
            }

            try
            {
                Console.WriteLine($"             Generating Amcache Dump..");
                gendump();
                Console.WriteLine($"             Success");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"             Error: {ex.Message}");
            }

            try
            {
                Console.WriteLine($"             Managing Table..");
                ManageTable();
            }

            catch (Exception ex)
            {
                Console.WriteLine($"             Error: {ex.Message}");
            }
        }

        private static void gendump()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string amcacheExePath = Path.Combine(baseDirectory, "EricZimmermanAmcacheParser", "AmcacheParser.exe");
            string hveFilePath = @"C:\Windows\AppCompat\Programs\Amcache.hve";
            string outputDir = Path.Combine(baseDirectory, "EricZimmermanAmcacheParser");
            string commandAmcache = $"\"{amcacheExePath}\" -f \"{hveFilePath}\" --csv \"{outputDir}\"";
            string commandCd = $"cd \"{outputDir}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {commandCd} && {commandAmcache}",
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"             Error: {ex.Message}");
            }
        }

        private static void ManageTable()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string extractPath = Path.Combine(baseDirectory, "EricZimmermanAmcacheParser");

            var file = Directory.GetFiles(extractPath, "*Amcache_Unassociated*.csv").FirstOrDefault();

            if (file == null)
            {
                Console.WriteLine("No file containing 'Amcache_Unassociated' found.");
                return;
            }

            Console.WriteLine($"File found: {file}");

            var lines = File.ReadAllLines(file);

            if (lines.Length < 2)
            {
                Console.WriteLine("CSV does not contain enough data.");
                return;
            }

            var headers = lines[0].Split(',').ToList();
            var rows = lines.Skip(1).Select(line => line.Split(',').ToList()).ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                rows[i] = rows[i].Take(headers.Count).ToList();
            }

            ShowTableInForm(rows, headers, LoadFilters(filterFilePath));
        }

        private static void ShowTableInForm(List<List<string>> rows, List<string> headers, Dictionary<string, List<string>> filters)
        {
            var table = ConvertToDataTable(rows, headers);

            List<DataRow> matchingRows = new List<DataRow>();
            List<DataRow> nonMatchingRows = new List<DataRow>();

            foreach (DataRow row in table.Rows)
            {
                bool isMatch = false;

                foreach (var filter in filters)
                {
                    if (table.Columns.Contains(filter.Key))
                    {
                        var cellValue = row[filter.Key]?.ToString();
                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            if (filter.Value.Any(fv => fv.Equals(cellValue, StringComparison.OrdinalIgnoreCase)))
                            {
                                isMatch = true;
                                break;
                            }
                        }
                    }
                }

                if (isMatch)
                    matchingRows.Add(row);
                else
                    nonMatchingRows.Add(row);
            }

            DataTable finalTable = table.Clone();

            foreach (var match in matchingRows)
                finalTable.ImportRow(match);
            foreach (var rest in nonMatchingRows)
                finalTable.ImportRow(rest);

            DataGridView dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = finalTable
            };

            dataGridView.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var currentRow = dataGridView.Rows[e.RowIndex];
                    var rowData = ((DataRowView)currentRow.DataBoundItem).Row;

                    foreach (var filter in filters)
                    {
                        if (finalTable.Columns.Contains(filter.Key))
                        {
                            var cellValue = rowData[filter.Key]?.ToString();
                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                if (filter.Value.Any(fv => fv.Equals(cellValue, StringComparison.OrdinalIgnoreCase)))
                                {
                                    currentRow.DefaultCellStyle.ForeColor = Color.Red;
                                    break;
                                }
                            }
                        }
                    }
                }
            };

            Form tableForm = new Form
            {
                Text = "CSV Data (Filtred)",
                Size = new Size(1000, 600)
            };

            tableForm.Controls.Add(dataGridView);
            Application.Run(tableForm);
        }

        public static DataTable ConvertToDataTable(List<List<string>> rows, List<string> columnNames)
        {
            if (rows == null || rows.Count == 0)
                throw new ArgumentException("The row list cannot be null or empty.", nameof(rows));

            if (columnNames == null || columnNames.Count == 0)
                throw new ArgumentException("The column name list cannot be null or empty.", nameof(columnNames));

            DataTable dataTable = new DataTable();

            HashSet<string> uniqueNames = new HashSet<string>();
            for (int i = 0; i < columnNames.Count; i++)
            {
                string originalName = columnNames[i] ?? $"Coluna{i + 1}";
                string name = originalName;
                int counter = 1;
                while (!uniqueNames.Add(name))
                {
                    name = originalName + "_" + counter;
                    counter++;
                }

                dataTable.Columns.Add(name);
            }

            foreach (var row in rows)
            {
                var rowData = new string[dataTable.Columns.Count];
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    rowData[j] = j < row.Count ? row[j] : null;
                }

                dataTable.Rows.Add(rowData);
            }

            return dataTable;
        }

        private static Dictionary<string, List<string>> LoadFilters(string filePath)
        {
            var filters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return filters;

            if (!Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                return filters;

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string valuePart = parts[1].Trim();

                    var values = valuePart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(v => v.Trim())
                                          .ToList();

                    if (filters.ContainsKey(key))
                    {
                        filters[key].AddRange(values);
                        filters[key] = filters[key].Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    }
                    else
                    {
                        filters[key] = values;
                    }
                }
            }

            return filters;
        }
    }
}
