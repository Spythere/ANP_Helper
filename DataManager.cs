using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ANP_Helper

{
    class DataManager
    {
        readonly Dictionary<string, string> definitions;
        
        readonly HttpClient client = new HttpClient();
        readonly string chosenScenery = "drzewko";
        readonly string region = "eu";

        public DataManager(string chosenScenery)
        {
            this.chosenScenery = chosenScenery;

            definitions = new Dictionary<string, string>
            {
                { "Wjazd_PnP_Pas_postoj", "Wjazd z PnP: tory 1/3" },
                { "Wjazd_KB_Pas_postoj", "Wjazd z KB: tory 2/3" },
                
                { "Wjazd_PnP_Tow_postoj", "Wjazd z PnP: tory 4/6" },
                { "Wjazd_KB_Tow_postoj", "Wjazd z KB: tory 4/6" },

                { "Wjazd_PnP_Pas_koniec", "Wjazd z PnP: tor 3" },
                { "Wjazd_PnP_Tow_koniec", "Wjazd z PnP: tory 4/6" },

                { "Wjazd_PnP_bok", "Wjazd z PnP: tor 4/6/2/3" },
                { "Wjazd_KB_bok", "Wjazd z KB: tory 4/6/1/3" },

                { "Przelot_KB", "Przelot na KB: tor 1" },
                { "Przelot_PnP", "Przelot na PnP: tor 2" },

                { "Wyjazd_KB", "Wyjazd na KB: wszystkie tory" },
                { "Wyjazd_PnP", "Wyjazd na PnP: wszystkie tory" },
            };
        }

        public async Task<bool> isChosenSceneryOnline()
        {
            string responseString = await client.GetStringAsync("https://api.td2.info.pl:9640/?method=getStationsOnline");
            StationAPIResponse response = JsonConvert.DeserializeObject<StationAPIResponse>(responseString);
            List<StationOnline> onlineStations = response.message;

            return onlineStations.Find(station => station.nameFromHeader.ToLower().Equals(chosenScenery.ToLower())) != null;
        }

        public async Task<List<AnpEntry>> fetchANPData()
        {
            List<AnpEntry> anpEntries = new List<AnpEntry>();

            Trace.WriteLine("Ładowanie danych...");
            string responseString = await client.GetStringAsync("https://api.td2.info.pl:9640/?method=getTrainsOnline");

            TrainAPIResponse response = JsonConvert.DeserializeObject<TrainAPIResponse>(responseString);

            List<TrainOnline> onlineTrains = response.message;

            List<Task<TimetableAPIResponse>> tasks = new List<Task<TimetableAPIResponse>>();

            Parallel.ForEach(onlineTrains, train =>
            {
                if (train.region != region)
                    return;

                Task<TimetableAPIResponse> task = getTimetableData(client, $"https://api.td2.info.pl:9640/?method=readFromSWDR&value=getTimetable%3B{train.trainNo}%3B{region}");

                tasks.Add(task);
            });

            TimetableAPIResponse[] ttResponse = await Task.WhenAll(tasks);

            Trace.WriteLine("Załadowano!");


            foreach (TimetableAPIResponse res in ttResponse)
            {
                if (res == null)
                    continue;

                if (res.message.trainInfo == null)
                    continue;


                TrainInfo trainInfo = res.message.trainInfo;
                StopPoint sceneryStopPoint = res.message.stopPoints.Find(stop => stop.pointNameRAW.ToLower().Equals(chosenScenery.ToLower()));

                if (sceneryStopPoint == null)
                    continue;

                if (sceneryStopPoint.confirmed == 1)
                    continue;

                anpEntries.Add(updateAnpEntry(trainInfo, sceneryStopPoint));
            }


            return anpEntries;
        }

        private AnpEntry updateAnpEntry(TrainInfo trainInfo, StopPoint stopPoint)
        {
            string arrivalLine = stopPoint.arrivalLine;
            string departureLine = stopPoint.departureLine;

            string trainType = trainInfo.trainNo.ToString().Length == 5 || trainInfo.trainNo.ToString().Length == 4 ? "Pas" : "Tow";

            AnpEntry anpEntry = new AnpEntry()
            {
                trainNo = trainInfo.trainNo,
                trainType = trainType,
                delay = 0d,
                arrivalLine = arrivalLine,
                departureLine = departureLine

            };
            // Początek
            if (stopPoint.pointStopTime == null && arrivalLine == null)
            {
                // Wyjazd_{departureLine}
                anpEntry.definitionType = DefinitionType.DEF_EXIT;    
            } 

            // Koniec
            if (stopPoint.pointStopTime == null && departureLine == null)
            {
                // Wjazd_{arrivalLine}_{trainType}_koniec
                anpEntry.definitionType = DefinitionType.DEF_ENTRY_END;
            }

            // Przelot
            if (stopPoint.pointStopTime != null && stopPoint.pointStopTime == 0)
            {
                // Przelot_{departureLine} Wjazd_bok_{arrivalLine} Wyjazd_{departureLine} Wjazd_postoj_{arrivalLine}_{trainType}
                anpEntry.definitionType = DefinitionType.DEF_NOSTOP;
                anpEntry.activationTime = stopPoint.departureTime;
            }

            // Postój
            if (stopPoint.pointStopTime != null && stopPoint.pointStopTime != 0)
            {
                // Wjazd_koniec_{arrivalLine}_{trainType} Wyjazd_{departureLine}
                anpEntry.definitionType = DefinitionType.DEF_STOP;
                anpEntry.activationTime = stopPoint.departureTime;
            }

            return anpEntry;
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
