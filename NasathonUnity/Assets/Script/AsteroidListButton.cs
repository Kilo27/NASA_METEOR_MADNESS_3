using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AsteroidListButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color hazardousColor = Color.red;
    [SerializeField] private Color safeColor = Color.green;

    public void Setup(AsteroidDataClass asteroid)
    {
        if (nameText != null)
            nameText.text = asteroid.name;

        if (detailsText != null)
        {
            string date = string.IsNullOrEmpty(asteroid.next_close_approach_date) ? "Unknown" : asteroid.next_close_approach_date;
            string distance = string.IsNullOrEmpty(asteroid.miss_distance_km) ? "Unknown" : FormatDistance(asteroid.miss_distance_km);
            detailsText.text = $"Next Approach: {date}\nMiss Distance: {distance}";
        }

        if (backgroundImage != null)
            backgroundImage.color = asteroid.is_hazardous ? hazardousColor : safeColor;
    }

    private string FormatDistance(string distanceKm)
    {
        if (float.TryParse(distanceKm, out float distance))
        {
            if (distance > 1000000)
                return $"{(distance / 1000000):F2}M km";
            else if (distance > 1000)
                return $"{(distance / 1000):F2}K km";
            else
                return $"{distance:F2} km";
        }
        return distanceKm + " km";
    }
}