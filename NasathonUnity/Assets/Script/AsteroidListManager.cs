using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// public class AsteroidListManager : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private Transform contentParent;
//     [SerializeField] private ScrollRect scrollRect;
//     [SerializeField] private GameObject asteroidButtonPrefab;
    
//     // [Header("Grid Settings")]
//     // [SerializeField] private int poolSize = 20; // Number of buttons to keep in pool
//     // [SerializeField] private int gridColumns = 2;
//     // [SerializeField] private float spacing = 10f;
    
//     private List<AsteroidDataClass> allAsteroids = new List<AsteroidDataClass>();
//     private Queue<GameObject> buttonPool = new Queue<GameObject>();
//     private List<GameObject> activeButtons = new List<GameObject>();
    
//     private int currentStartIndex = 0;
//     private float buttonHeight;
//     private float buttonWidth;
//     private GridLayoutGroup gridLayout;

//     void Awake()
//     {
//         Debug.Log("WOPWOW");
//         InitializeGrid();
//         LoadAsteroidData();
//         InitializePool();
//         UpdateDisplay();
//     }

//     // void InitializeGrid()
//     // {
//     //     // Add GridLayoutGroup to content parent
//     //     gridLayout = contentParent.GetComponent<GridLayoutGroup>();
//     //     if (gridLayout == null)
//     //         gridLayout = contentParent.gameObject.AddComponent<GridLayoutGroup>();
            
//     //     // Calculate button size based on available width
//     //     RectTransform contentRT = contentParent.GetComponent<RectTransform>();
//     //     float availableWidth = contentRT.rect.width - (spacing * (gridColumns - 1));
//     //     buttonWidth = availableWidth / gridColumns;
        
//     //     // Get button height from prefab
//     //     RectTransform prefabRT = asteroidButtonPrefab.GetComponent<RectTransform>();
//     //     buttonHeight = prefabRT.rect.height;
        
//     //     // Configure grid
//     //     gridLayout.cellSize = new Vector2(buttonWidth, buttonHeight);
//     //     gridLayout.spacing = new Vector2(spacing, spacing);
//     //     gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//     //     gridLayout.constraintCount = gridColumns;
//     // }



//     // void InitializePool()
//     // {
//     //     for (int i = 0; i < poolSize; i++)
//     //     {
//     //         GameObject button = Instantiate(asteroidButtonPrefab, contentParent);
//     //         button.SetActive(false);
//     //         buttonPool.Enqueue(button);
//     //     }
//     // }

//     // void UpdateDisplay()

//     // {
//     //     // Return all active buttons to pool
//     //     foreach (GameObject button in activeButtons)
//     //     {
//     //         button.SetActive(false);
//     //         buttonPool.Enqueue(button);
//     //     }
//     //     activeButtons.Clear();

//     //     // Calculate which asteroids to display
//     //     int endIndex = Mathf.Min(currentStartIndex + poolSize, allAsteroids.Count);
        
//     //     for (int i = currentStartIndex; i < endIndex; i++)
//     //     {
//     //         if (buttonPool.Count > 0)
//     //         {
//     //             GameObject button = buttonPool.Dequeue();
//     //             SetupButton(button, allAsteroids[i], i);
//     //             button.SetActive(true);
//     //             activeButtons.Add(button);
//     //         }
//     //     }

//     //     UpdateContentSize();
//     // }

//     void SetupButton(GameObject button, AsteroidDataClass asteroid, int index)
//     {
//         // Set button position in grid
//         int row = index / gridColumns;
//         int col = index % gridColumns;
        
//         // Update button content
//         AsteroidListButton asteroidButton = button.GetComponent<AsteroidListButton>();
//         if (asteroidButton != null)
//         {
//             asteroidButton.Setup(asteroid);
//         }
        
//         // Add click listener
//         Button btnComponent = button.GetComponent<Button>();
//         if (btnComponent != null)
//         {
//             btnComponent.onClick.RemoveAllListeners();
//             btnComponent.onClick.AddListener(() => OnAsteroidClicked(asteroid));
//         }
//     }

//     // void UpdateContentSize()
//     // {
//     //     RectTransform contentRT = contentParent.GetComponent<RectTransform>();
//     //     int totalRows = Mathf.CeilToInt((float)allAsteroids.Count / gridColumns);
//     //     float totalHeight = (buttonHeight + spacing) * totalRows;
//     //     contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, totalHeight);
//     // }

//     public void OnAsteroidClicked(AsteroidDataClass asteroid)
//     {
//         Debug.Log($"Clicked: {asteroid.name} | Hazardous: {asteroid.is_hazardous}");
//         // Add your click handling logic here
//     }

//     // Call this when scroll value changes
//     public void OnScrollValueChanged(Vector2 scrollPos)
//     {
//         int newStartIndex = CalculateCurrentStartIndex();
//         if (newStartIndex != currentStartIndex)
//         {
//             currentStartIndex = newStartIndex;
//             UpdateDisplay();
//         }
//     }

//     int CalculateCurrentStartIndex()
//     {
//         RectTransform contentRT = contentParent.GetComponent<RectTransform>();
//         float scrollPercentage = 1 - scrollRect.verticalNormalizedPosition;
//         float totalScrollableHeight = contentRT.rect.height - scrollRect.viewport.rect.height;
//         float currentScrollPosition = scrollPercentage * totalScrollableHeight;
        
//         int row = Mathf.FloorToInt(currentScrollPosition / (buttonHeight + spacing));
//         return Mathf.Max(0, row * gridColumns);
//     }
// }

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
            if (i >= 8) {
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