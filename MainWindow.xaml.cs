using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace ANP_Helper
{
    public class TimetableColumn
    {
        public string definitionType { get; set; }
        public string trainNumbers { get; set; }
    }

    public partial class MainWindow : Window
    {
        readonly DataManager dm;
        readonly string chosenScenery = "Drzewko";
        private string chosenFilePath = "";

        List<AnpEntry> anpEntries;

        public MainWindow()
        {
            InitializeComponent();
            statusTextXAML.Text = $"{chosenScenery.ToUpper()} OFFLINE!";


            dm = new DataManager(chosenScenery);        
        }

        private async void refreshData()
        {
            await getSceneryData();

            if (anpEntries != null)
            {
                List<string> lines = new List<string>();

                foreach (string line in File.ReadLines(@chosenFilePath))
                {
                    lines.Add(line);

                    if (line.Contains("###"))
                        break;
                }


                foreach (AnpEntry anpEntry in anpEntries)
                {
                    lines.Add($"przebieg {anpEntry.trainNo} - - {anpEntry.delay} {anpEntry.definitionType}");
                }

                await File.WriteAllLinesAsync(@chosenFilePath, lines);

                renderXAML();
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            refreshData();
        }

        public void renderXAML()
        {
            statusIndicatorXAML.Fill = Brushes.LightGreen;

            statusTextXAML.Text = $"{chosenScenery.ToUpper()} ONLINE!";
            statusTextXAML.Foreground = Brushes.LightGreen;

            Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();

            foreach (AnpEntry entry in anpEntries)
            {
                if (!dict.ContainsKey(entry.definitionType))
                    dict[entry.definitionType] = new List<int>();

                dict[entry.definitionType].Add(entry.trainNo);
            }

            List<TimetableColumn> columns = new List<TimetableColumn>();

            foreach (string definitionTypeKey in dict.Keys)
            {
                dict[definitionTypeKey].Sort((a, b) => a > b ? 1 : -1);

                columns.Add(new TimetableColumn()
                {
                    definitionType = definitionTypeKey,
                    trainNumbers = string.Join(", ", dict[definitionTypeKey].Select(x => x.ToString()).ToArray())
                });

                Trace.WriteLine(definitionTypeKey);
            }

            columns.Sort((col1, col2) => col1.definitionType[0] > col2.definitionType[0] ? 1 : -1);

            DataGridXAML.ItemsSource = columns;
        }

        public async Task getSceneryData()
        {
            bool isSceneryOnline = await dm.isChosenSceneryOnline();

            if (!isSceneryOnline)
            {
                anpEntries = null;
                return;
            }

            anpEntries = await dm.fetchANPData();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void DataGridXAML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridXAML_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            chosenFilePath = FileManager.getANPFilePath();

            if (chosenFilePath == null)
                return;



            /*  DispatcherTimer timer = new DispatcherTimer
              {
                  Interval = TimeSpan.FromSeconds(10)
              };

              timer.Tick += timerTick;
              timer.Start();*/

            refreshData();
        }
    }
}
