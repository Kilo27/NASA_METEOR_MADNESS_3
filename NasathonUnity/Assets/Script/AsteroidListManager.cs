using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class AsteroidListManager : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private AsteroidListButton asteroidButtonPrefab;


    private List<AsteroidDataClass> allAsteroids = new List<AsteroidDataClass>();
    private Queue<GameObject> buttonPool = new Queue<GameObject>();
    private List<GameObject> activeButtons = new List<GameObject>();

    private void Start() {
        LoadAsteroidData();
        InititializeButtons();
    }

    private void InititializeButtons() {

        int i = 0;
        foreach (var data in allAsteroids)
        {
            AsteroidListButton prefab = Instantiate(asteroidButtonPrefab, contentParent);
            prefab.Setup(data);
            i++;
            if (i >= 40) {
                break;
            }
        }
    }
    
    private void LoadAsteroidData()
    {
        // Load your JSON data here
        TextAsset jsonFile = Resources.Load<TextAsset>("neos");
        if (jsonFile != null)
        {
            string[] lines = jsonFile.text.Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    Debug.Log(line);
                    AsteroidDataClass asteroid = JsonUtility.FromJson<AsteroidDataClass>(line);
                    Debug.Log(asteroid);
                    allAsteroids.Add(asteroid);
                }
            }
        }
        
        Debug.Log($"Loaded {allAsteroids.Count} asteroids");
    }
}