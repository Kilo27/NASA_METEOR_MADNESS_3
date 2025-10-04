using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


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

    public void BigBossSimulation()
    {
        
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

