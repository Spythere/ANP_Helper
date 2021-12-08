using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Threading;
using Microsoft.Win32;

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
        readonly string chosenScenery = "Otwocko";
        private string chosenFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
            statusTextXAML.Text = $"{chosenScenery.ToUpper()} OFFLINE!";


            dm = new DataManager(chosenScenery);        
        }

        private void timerTick(object sender, EventArgs e)
        {
            getSceneryData();
        }

        public async void getSceneryData()
        {
            bool isSceneryOnline = await dm.isChosenSceneryOnline();

            if(!isSceneryOnline)
                return;

            statusIndicatorXAML.Fill = Brushes.LightGreen;

            statusTextXAML.Text = $"{chosenScenery.ToUpper()} ONLINE!";
            statusTextXAML.Foreground = Brushes.LightGreen;

            List<AnpEntry> anpEntries = await dm.fetchANPData();


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
                columns.Add(new TimetableColumn()
                {
                    definitionType = definitionTypeKey,
                    trainNumbers = string.Join(",", dict[definitionTypeKey].Select(x => x.ToString()).ToArray())
                });

                /* columns.Add(new TimetableColumn
                 {
                     timetableId = timetable.trainInfo.timetableId,
                     trainNo = timetable.trainInfo.trainNo,
                     driverName = timetable.trainInfo.driverName,
                     directivesString = directivesString
                 });*/

                Trace.WriteLine(definitionTypeKey);
            }

            DataGridXAML.ItemsSource = columns;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*chosenFilePath = FileManager.openANPFile();

            if (chosenFilePath == null)
                return;*/

           /* trajectoryDict = FileManager.readTrajectories(chosenFilePath);

            foreach (Trajectory t in trajectoryDict.Values)
            {
                Trace.WriteLine(t.name);
            }*/

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };

            timer.Tick += timerTick;
            timer.Start();

            getSceneryData();

        }
    }
}
