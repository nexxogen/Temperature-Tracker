using System;
using System.Collections.Generic;

using SerializerFree;
using UnityEngine;
using System.Linq;

public class DataManager 
{
    [Serializable]
    class TemperatureByDate
    {
        public DateTime DateTaken { get; set; }
        public float Temperature { get; set; }

        public bool Equals(TemperatureByDate toCompare)
        {
            return DateTaken.Date.Equals(toCompare.DateTaken.Date);
        }
    }

    [Serializable]
    class DataContainer
    {
        public List<string> Locations { get; set; }
        public Dictionary<string, List<TemperatureByDate>> Temperatures { get; set; }

        public DataContainer()
        {
            Locations = new List<string>();
            Temperatures = new Dictionary<string, List<TemperatureByDate>>();
        }
    }

    private const string JSON = "JSON";

    private static DataContainer Container { get; set; }
    private static string jsonString { get; set; }

    static DataManager()
    {
        if (Container == null)
        {
            Container = new DataContainer();
        }

        if (PlayerPrefs.HasKey(JSON))
        {
            jsonString = PlayerPrefs.GetString(JSON);
            Container = Serializer.Deserialize<DataContainer>(jsonString);
        }
    }

    public static List<string> Locations
    {
        get { return Container.Locations; }
    }

    public static string AddLocation(string location)
    {
        if (!Container.Locations.Contains(location))
        {
            Container.Locations.Add(location);
            UpdateJsonString();

            return "Lokacija uspjesno dodata";
        }

        return "Lokacija vec postoji";
    }

    public static string AddTemperature(string location, float temperature)
    {
        TemperatureByDate tempByDate = new TemperatureByDate()
        {
            DateTaken = DateTime.Now.Date,
            Temperature = temperature
        };

        if (Container.Temperatures.ContainsKey(location))
        {
            TemperatureByDate currentTempByDate = Container.Temperatures[location].First(t => t.Equals(tempByDate));
            if (currentTempByDate != null)
            {
                Container.Temperatures[location].Remove(currentTempByDate);
                Container.Temperatures[location].Add(tempByDate);
            }
        }
        else
        {
            Container.Temperatures.Add(location, new List<TemperatureByDate> { tempByDate });
        }

        UpdateJsonString();

        return "Temperatura uspjesno dodata";
    }

    public static List<string> GetReport(DateTime from, DateTime to)
    {
        Dictionary<string, float> averageByLocation = new Dictionary<string, float>();

        foreach (string location in Container.Temperatures.Keys)
        {
            List<TemperatureByDate> filteredByDate = Container.Temperatures[location].Where(t => t.DateTaken >= from && t.DateTaken <= to).ToList();

            if (filteredByDate.Count == 0)
            {
                return new List<string>();
            }

            float averageTemperature = filteredByDate.Average(t => t.Temperature);
            averageByLocation.Add(location, averageTemperature);
        }

        List<KeyValuePair<string, float>> ordered = averageByLocation.ToList();
        ordered.Sort((p1, p2) => p1.Value.CompareTo(p2.Value));

        List<string> finalList = new List<string>();

        for (int i = ordered.Count - 1; i >= 0; i--)
        {
            string formatted = ordered[i].Key + " : " + ordered[i].Value;
            finalList.Add(formatted);
        }

        return finalList;
    }

    private List<TemperatureByDate> GetTemperaturesForPeriod(string location, DateTime from, DateTime to)
    {
        return Container.Temperatures[location].Where(t => t.DateTaken >= from && t.DateTaken <= to).ToList();
    }

    private static void UpdateJsonString()
    {
        jsonString = Serializer.Serialize(Container);
        PlayerPrefs.SetString(JSON, jsonString);
    }

    public static void PrintCurrentData()
    {
        Debug.Log(jsonString);
    }
}
