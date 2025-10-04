using UnityEngine;

public class BackendTester : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Example usage: Fetch asteroids starting from 2023-10-01 for 2 weeks
            GameBackend.Instance.GetAsteroids("2024-10-01", 2);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // Example usage: Fetch trajectory for asteroid with ID 12345 until 2023-12-31
            GameBackend.Instance.GetTrajectory(2497232, "2026-12-31");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            GameBackend.Instance.CalculateCrater(2497232, 3434.5f);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            GameBackend.Instance.GetAsteroidInfoByDay(2497232, 2);
        }

    }
}
