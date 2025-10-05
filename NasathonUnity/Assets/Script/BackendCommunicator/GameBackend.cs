using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System;


public class GameBackend : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiBaseURL = "http://localhost:5000"; // Change to your API URL
    

    public static GameBackend Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void GetAsteroids(string startDate, int weeks, System.Action<List<AsteroidData>> onComplete)
    {
        StartCoroutine(GetAsteroidsCoroutine(startDate, weeks, onComplete));
    }

    private IEnumerator GetAsteroidsCoroutine(string startDate, int weeks, System.Action<List<AsteroidData>> onComplete)
    {
        string url = $"{apiBaseURL}/getAsteroids?start_date={startDate}&weeks={weeks}";
        Debug.Log("Requesting URL: " + url);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            Debug.Log("SEND REQUEST");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string text = webRequest.downloadHandler.text;
                // Wrap the raw array into an object so JsonUtility can parse it
                text = "{ \"asteroids\": " + text + "}";

                Debug.Log("Asteroid data received: " + text);

                AsteroidDataResponse response = JsonUtility.FromJson<AsteroidDataResponse>(text);
                onComplete?.Invoke(response.asteroids);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
                onComplete?.Invoke(null);
            }
        }
    }
    
    public void GetTrajectory(int asteroidID, string endDate, System.Action<List<Vector3>> onComplete)
    {
        StartCoroutine(GetTrajectoryCoroutine(asteroidID, endDate, onComplete));
    }

    private IEnumerator GetTrajectoryCoroutine(int asteroidID, string endDate, System.Action<List<Vector3>> onComplete)
    {
        string url = $"{apiBaseURL}/calculateAsteroidTrajectory?asteroid_id={asteroidID}&end_date={endDate}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string text = webRequest.downloadHandler.text;
                Debug.Log("Trajectory data received: " + text);

                var parsed = ProcessTrajectoryData(text);
                onComplete?.Invoke(parsed);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
                onComplete?.Invoke(null);
            }
        }
    }

    public void GetAsteroidInfo(int asteroidID, System.Action<AsteroidData> onComplete)
    {
        StartCoroutine(GetAsteroidInfoCoroutine(asteroidID, onComplete));
    }

    private IEnumerator GetAsteroidInfoCoroutine(int asteroidID, System.Action<AsteroidData> onComplete)
    {
        string url = $"{apiBaseURL}/getAsteroidInfo?asteroid_id={asteroidID}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string text = webRequest.downloadHandler.text;
                Debug.Log("Asteroid info received: " + text);

                // Parse JSON into AsteroidInfo
                AsteroidData info = JsonUtility.FromJson<AsteroidData>(text);

                // Return via callback
                onComplete?.Invoke(info);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
                onComplete?.Invoke(null);
            }
        }
    }

    public void GetAsteroidInfoByDay(int asteroidID, int day, System.Action<AsteroidData> onComplete)
    {
        StartCoroutine(GetAsteroidInfoByDayCoroutine(asteroidID, day, onComplete));
    }

    private IEnumerator GetAsteroidInfoByDayCoroutine(int asteroidId, int day, System.Action<AsteroidData> onComplete)
    {
        string url = $"{apiBaseURL}/getAsteroidInfoByDay?asteroid_id={asteroidId}&day={day}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string text = webRequest.downloadHandler.text;
                Debug.Log("Asteroid info by day received: " + text);

                // Parse JSON into AsteroidData
                AsteroidData info = JsonUtility.FromJson<AsteroidData>(text);

                // Return via callback
                onComplete?.Invoke(info);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
                onComplete?.Invoke(null);
            }
        }
    }

    public void CalculateCrater(float mass, float velocity)
    {
        StartCoroutine(CalculateCraterCoroutine(mass, velocity));
    }

    private IEnumerator CalculateCraterCoroutine(float mass, float velocity)
    {
        // Build the URL with query parameters
        string url = $"{apiBaseURL}/calculateCrater?mass={mass}&velocity={velocity}";
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request
            yield return webRequest.SendWebRequest();
            
            // Handle the response
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("Crater data received: " + webRequest.downloadHandler.text);
                    // Process crater data here
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
            }
        }
    }

    public void BigBossSimulation(float velocity, float diameter, int impactAngle, Action<AsteroidImpactData> onComplete)
    {
        StartCoroutine(BigBossSimulationCoroutine(velocity, diameter, impactAngle, onComplete));
    }

    private IEnumerator BigBossSimulationCoroutine(float velocity, float diameter, int impactAngle, Action<AsteroidImpactData> onComplete)
    {
        // Build the URL with query parameters
        string url = $"{apiBaseURL}/simulate?velocity={velocity}&diameter={diameter}&impact_angle={impactAngle}";


        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request
            yield return webRequest.SendWebRequest();
            
            // Handle the response
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    string text = webRequest.downloadHandler.text;
                    onComplete?.Invoke(ParseBigBoss(text));
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
            }
        }
    }

    [System.Serializable]
    public class BigBossResponse
    {
        public Dictionary<string, string> asteroid_properties;
        public Dictionary<string, string> atmospheric_passage;
        public Dictionary<string, string> effect_radii;
        public Dictionary<string, string> effects_at_crater_rim;
        public Dictionary<string, string> impact_crater;
        public Dictionary<string, string> impact_severity_classification;
        public Dictionary<string, string> input_parameters;
        public Dictionary<string, string> seismic_effects;
        public Dictionary<string, string> tsunami_effects;
    }

    public AsteroidImpactData ParseBigBoss(string jsonResponse) 
    {
        return JsonUtility.FromJson<AsteroidImpactData>(jsonResponse);
    }

    private Dictionary<string, string> ConvertToStringDictionary(Dictionary<string, string> source)
    {
        // This method ensures all values are properly converted to strings
        Dictionary<string, string> result = new Dictionary<string, string>();
        
        foreach (var kvp in source)
        {
            result[kvp.Key] = kvp.Value?.ToString() ?? "null";
        }
        
        return result;
    }

    private void ProcessAsteroidData(string jsonData)
    {
        try
        {
            // Parse the JSON response
            AsteroidDataResponse response = JsonUtility.FromJson<AsteroidDataResponse>(jsonData);

            // Handle your asteroid data here
            Debug.Log($"Received {response.asteroids.Count} asteroids");

            // Trigger event or update UI
            OnAsteroidDataReceived?.Invoke(response.asteroids);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse asteroid data: " + e.Message);
        }
    }

    [System.Serializable]
    public class TrajectoryWrapper
    {
        public List<Vector3> trajectory;
    }

    private List<Vector3> ProcessTrajectoryData(string jsonData)
    {
        string wrapped = "{ \"trajectory\": " + jsonData + "}";
        TrajectoryWrapper response = JsonUtility.FromJson<TrajectoryWrapper>(wrapped);
        Debug.Log($"Received trajectory with {response.trajectory.Count} points");
        return response.trajectory;
    }


    
    // Event for when data is received
    public System.Action<List<AsteroidData>> OnAsteroidDataReceived;
}

// Data classes to match your API response
[System.Serializable]
public class AsteroidDataResponse
{
    public List<AsteroidData> asteroids;
}

[System.Serializable]
public class AsteroidData
{
    public string id;
    public string name;
    public float diameter; // in km
    public float velocity; // in km/s
    public string close_approach_date;
    public float miss_distance; // in km
    // Add other fields as needed based on your API response
}

[System.Serializable]
public class AsteroidProperties
{
    public string equivalent_nuclear_yield;
    public double kinetic_energy_joules;
    public double kinetic_energy_kt_tnt;
    public double mass_kg;
}

[System.Serializable]
public class AtmosphericPassage
{
    public double airburst_altitude_km;
    public double breakup_altitude_km;
    public string impact_scenario;
    public string scenario_description;
}

[System.Serializable]
public class EffectRadii
{
    public double fireball_radius_km;
    public double max_tsunami_range_km;
    public double overpressure_radius_20kpa_km;
    public double overpressure_radius_5kpa_km;
    public double shockwave_radius_50ms_km;
    public double thermal_damage_radius_km;
}

[System.Serializable]
public class EffectsAtCraterRim
{
    public double overpressure_kpa;
    public double thermal_radiation_mj_m2;
    public double wind_speed_m_s;
}

[System.Serializable]
public class ImpactCrater
{
    public double crater_radius_km;
    public double final_crater_diameter_km;
    public double transient_crater_diameter_km;
}

[System.Serializable]
public class ImpactSeverityClassification
{
    public string crater_class;
    public string energy_class;
    public string tsunami_class;
}

[System.Serializable]
public class InputParameters
{
    public double impact_angle_deg;
    public double mean_diameter_m;
    public double velocity_km_s;
}

[System.Serializable]
public class SeismicEffects
{
    public double richter_magnitude;
}

[System.Serializable]
public class TsunamiEffects
{
    public double runup_height_at_100km_m;
    public string tsunami_category;
    public double wave_amplitude_at_100km_m;
}

[System.Serializable]
public class AsteroidImpactData
{
    public AsteroidProperties asteroid_properties;
    public AtmosphericPassage atmospheric_passage;
    public EffectRadii effect_radii;
    public EffectsAtCraterRim effects_at_crater_rim;
    public ImpactCrater impact_crater;
    public ImpactSeverityClassification impact_severity_classification;
    public InputParameters input_parameters;
    public SeismicEffects seismic_effects;
    public TsunamiEffects tsunami_effects;
}
