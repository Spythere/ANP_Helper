using System;
using System.Collections.Generic;
using System.Text;

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
        public string? arrivalLine { get; set; }
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
        public string? departureLine { get; set; }
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

    public static class DefinitionType
    {
        public const string
            DEF_STOP = "Postój",
            DEF_ENTRY_ASIDE = "Wjazd (bok)",
            DEF_ENTRY_END = "Wjazd (koniec)",

            DEF_EXIT = "Wyjazd",
            DEF_NOSTOP = "Przelot";
    }

    class AnpEntry
    {
        public int trainNo { get; set; }

        public string trainType { get; set; }

        public double delay { get; set; }

        public string definitionType { get; set; }

        public DateTime? activationTime { get; set; }

        public string? departureLine { get; set; }

        public string? arrivalLine { get; set; }
    }
}
