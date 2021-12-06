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

namespace ANP_Helper
{
    public class TimetableColumn
    {
        public int timetableId { get; set; }
        public int trainNo { get; set; }
        public string driverName { get; set; }
    }

    public partial class MainWindow : Window
    {
        readonly DataManager dm;
        string chosenScenery = "Piaskowo";

        public MainWindow()
        {
            InitializeComponent();
            statusTextXAML.Text = $"{chosenScenery.ToUpper()} OFFLINE!";


            dm = new DataManager(chosenScenery);

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };

            timer.Tick += timerTick;
            timer.Start();

            getSceneryData();
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

            List<SceneryTimetable> timetables = await dm.fetchTimetableData();

            List<TimetableColumn> columns = new List<TimetableColumn>();

            foreach (SceneryTimetable timetable in timetables)
            {
                columns.Add(new TimetableColumn
                {
                    timetableId = timetable.trainInfo.timetableId,
                    trainNo = timetable.trainInfo.trainNo,
                    driverName = timetable.trainInfo.driverName
                });
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
    }
}
