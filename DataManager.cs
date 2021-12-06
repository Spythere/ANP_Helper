using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ANP_Helper

{
     public class StationOnline
    {
        public int dispatcherId { get; set; }
        public string dispatcherName { get; set; }
        public bool dispatcherIsSupporter { get; set; }
        public string stationName { get; set; }
        public string stationHash { get; set; }
        public string region { get; set; }
        public int maxUsers { get; set; }
        public int currentUsers { get; set; }
        public int spawn { get; set; }
        public object lastSeen { get; set; }
        public int dispatcherExp { get; set; }
        public string nameFromHeader { get; set; }
        public string spawnString { get; set; }
        public string networkConnectionString { get; set; }
        public int isOnline { get; set; }
        public int dispatcherRate { get; set; }
    }

    public class StationAPIResponse
    {
        public bool success { get; set; }
        public int respCode { get; set; }
        public List<StationOnline> message { get; set; }
    }
    class TrainAPIResponse
    {
        public bool success { get; set; }
        public int respCode { get; set; }
        public List<TrainOnline> message { get; set; }
    }

    class TrainInfo
    {
        public int timetableId { get; set; }
        public int trainNo { get; set; }
        public string trainCategoryCode { get; set; }
        public int driverId { get; set; }
        public string driverName { get; set; }
        public string route { get; set; }
        public int twr { get; set; }
        public int skr { get; set; }
        public List<string> sceneries { get; set; }
    }

    class StopPoint
    {
        public string arrivalLine { get; set; }
        public DateTime? arrivalTime { get; set; }
        public int arrivalDelay { get; set; }
        public DateTime? arrivalRealTime { get; set; }
        public double pointDistance { get; set; }
        public string pointName { get; set; }
        public string pointNameRAW { get; set; }
        public int entryId { get; set; }
        public string pointId { get; set; }
        public object comments { get; set; }
        public int confirmed { get; set; }
        public int isStopped { get; set; }
        public int? pointStopTime { get; set; }
        public string pointStopType { get; set; }
        public string departureLine { get; set; }
        public DateTime? departureTime { get; set; }
        public int departureDelay { get; set; }
        public DateTime? departureRealTime { get; set; }
    }

    class Message
    {
        public TrainInfo? trainInfo { get; set; }
        public List<StopPoint>? stopPoints { get; set; }
    }

    class TimetableAPIResponse
    {
        public bool success { get; set; }
        public int respCode { get; set; }
        public Message? message { get; set; }
    }

    class TrainOnline
    {
        public int trainNo { get; set; }
        public int driverId { get; set; }
        public string driverName { get; set; }

        public string region { get; set; }
    }

    class Timetable
    {
        public TrainInfo trainInfo { get; set; }
        public List<StopPoint> stopPoints { get; set; }
    }

    class SceneryTimetable
    {
        public TrainInfo trainInfo { get; set; }
    
        public StopPoint sceneryStopPoint { get; set; }
    }

    class DataManager
    {
        readonly HttpClient client = new HttpClient();
        readonly string chosenScenery = "drzewko";


        public DataManager(string chosenScenery)
        {
            this.chosenScenery = chosenScenery;
        }

        public async Task<bool> isChosenSceneryOnline()
        {
            string responseString = await client.GetStringAsync("https://api.td2.info.pl:9640/?method=getStationsOnline");
            StationAPIResponse response = JsonConvert.DeserializeObject<StationAPIResponse>(responseString);
            List<StationOnline> onlineStations = response.message;

            return onlineStations.Find(station => station.nameFromHeader.ToLower().Equals(chosenScenery.ToLower())) != null;
        }

        public async Task<List<SceneryTimetable>> fetchTimetableData()
        {
            Trace.WriteLine("Ładowanie danych...");
            string responseString = await client.GetStringAsync("https://api.td2.info.pl:9640/?method=getTrainsOnline");

            TrainAPIResponse response = JsonConvert.DeserializeObject<TrainAPIResponse>(responseString);

            List<TrainOnline> onlineTrains = response.message;

            List<Task<TimetableAPIResponse>> tasks = new List<Task<TimetableAPIResponse>>();

            Parallel.ForEach(onlineTrains, train =>
            {
                if (train.region != "eu")
                    return;

                Task<TimetableAPIResponse> task = getTimetableData(client, $"https://api.td2.info.pl:9640/?method=readFromSWDR&value=getTimetable%3B{train.trainNo}%3Beu");

                tasks.Add(task);
            });

            TimetableAPIResponse[] ttResponse = await Task.WhenAll(tasks);

            Trace.WriteLine("Załadowano!");

            List<SceneryTimetable> sceneryTimetables = new List<SceneryTimetable>();

            foreach (TimetableAPIResponse res in ttResponse)
            {
                if (res == null)
                    continue;

                if (res.message.trainInfo == null)
                    continue;


                StopPoint sceneryStopPoint = res.message.stopPoints.Find(stop => stop.pointNameRAW.ToLower().Equals(chosenScenery.ToLower()));


                if (sceneryStopPoint == null)
                    continue;

                sceneryTimetables.Add(new SceneryTimetable
                {
                    trainInfo = res.message.trainInfo,
                    sceneryStopPoint = sceneryStopPoint
                });
            }

            sceneryTimetables.Sort((a, b) => a.trainInfo.timetableId > b.trainInfo.timetableId ? 1 : -1);

            return sceneryTimetables;
        }


        private async Task<TimetableAPIResponse> getTimetableData(HttpClient client, string url)
        {
            string res = await client.GetStringAsync(url);

            TimetableAPIResponse resp;
            try
            {
                resp = JsonConvert.DeserializeObject<TimetableAPIResponse>(res);
            } catch (JsonSerializationException)
            {
                Trace.WriteLine("Ups! Błąd przy " + url);
                return null;
            }

            return resp;
        } 
    }
}
