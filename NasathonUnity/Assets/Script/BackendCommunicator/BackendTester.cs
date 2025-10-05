using TMPro;
using UnityEngine;

public class BackendTester : MonoBehaviour
{
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("asldhfsldg");
            GameBackend.Instance.BigBossSimulation(90000, 100, 20, (data) =>
            {
                Debug.Log(data.seismic_effects.richter_magnitude);
                Debug.Log(data.effect_radii.max_tsunami_range_km);
                Debug.Log(data.atmospheric_passage.airburst_altitude_km);
            });
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Example usage: Fetch asteroids starting from 2023-10-01 for 2 weeks
            GameBackend.Instance.GetAsteroids("2024-10-01", 2, (asteroids) =>
            {
                Debug.Log("Callback received for GetAsteroids");
                if (asteroids != null)
                {
                    Debug.Log($"Received {asteroids.Count} asteroids.");
                    foreach (var asteroid in asteroids)
                    {
                        Debug.Log($"Asteroid ID: {asteroid.id}, Name: {asteroid.name}");
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch asteroids.");
                }
            });
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // Example usage: Fetch trajectory for asteroid with ID 12345 until 2023-12-31
            GameBackend.Instance.GetTrajectory(2497232, "2026-12-31", (trajectory) =>
            {
                Debug.Log("Callback received for GetTrajectory");
                if (trajectory != null)
                {
                    Debug.Log($"Received trajectory with {trajectory.Count} points.");
                    foreach (var point in trajectory)
                    {
                        Debug.Log($"Point: ({point[0]}, {point[1]}, {point[2]})");
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch trajectory.");
                }
            });
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            GameBackend.Instance.CalculateCrater(2497232, 3434.5f);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            GameBackend.Instance.GetAsteroidInfoByDay(2497232, 3, (asteroid) =>
            {
                Debug.Log("Callback received for GetAsteroidInfoByDay");
                if (asteroid != null)
                {
                    Debug.Log($"Received {asteroid.velocity} asteroids for the day.");
                }
                else
                {
                    Debug.LogError("Failed to fetch asteroids for the day.");
                }
            });
        }

    }
}
