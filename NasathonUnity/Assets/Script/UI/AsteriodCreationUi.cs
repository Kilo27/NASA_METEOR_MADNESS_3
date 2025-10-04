using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class AsteroidCreationUI : MonoBehaviour
{

    public static AsteroidCreationUI instance { get; private set; }

    [Header("UI Input Fields (TextMeshPro)")]
    [SerializeField] private TMP_InputField velocityInput;
    [SerializeField] private TMP_InputField massInput;
    [SerializeField] private TMP_InputField diameterInput;
    
    [Header("Create Button")]
    [SerializeField] private Button createButton;

    public event EventHandler<AsteroidCreationData> OnCreateAsteroid;

    [System.Serializable]
    public class AsteroidCreationData
    {
        public float velocity;
        public float mass;
        public float diameter;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Add listener to the create button
        if (createButton != null)
        {
            createButton.onClick.AddListener(OnCreateButtonClicked);
        }
        else
        {
            Debug.LogWarning("Create button reference not set in AsteroidCreationUI");
        }
    }

    private void OnCreateButtonClicked()
    {
        // Parse input values with error handling
        float velocity = ParseFloatInput(velocityInput, "Velocity");
        float mass = ParseFloatInput(massInput, "Mass");
        float diameter = ParseFloatInput(diameterInput, "Diameter");

        // Validate inputs
        if (velocity < 0 || mass <= 0 || diameter <= 0)
        {
            Debug.LogWarning("Invalid asteroid parameters. Mass and diameter must be positive, velocity cannot be negative.");
            return;
        }

        // Create asteroid data and invoke event
        AsteroidCreationData asteroidData = new AsteroidCreationData
        {
            velocity = velocity,
            mass = mass,
            diameter = diameter
        };

        // Invoke the event
        OnCreateAsteroid?.Invoke(this, asteroidData);

        // Optional: Clear input fields after creation
        ClearInputFields();
    }

    private float ParseFloatInput(TMP_InputField inputField, string fieldName)
    {
        if (inputField == null)
        {
            Debug.LogWarning($"{fieldName} input field reference not set");
            return 0f;
        }

        if (float.TryParse(inputField.text, out float result))
        {
            return result;
        }
        else
        {
            Debug.LogWarning($"Invalid {fieldName} input: {inputField.text}");
            return 0f;
        }
    }

    private void ClearInputFields()
    {
        if (velocityInput != null) velocityInput.text = "";
        if (massInput != null) massInput.text = "";
        if (diameterInput != null) diameterInput.text = "";
    }

    // Public method to manually trigger asteroid creation (useful for testing)
    public void CreateAsteroidWithValues(float velocity, float mass, float diameter)
    {
        AsteroidCreationData asteroidData = new AsteroidCreationData
        {
            velocity = velocity,
            mass = mass,
            diameter = diameter
        };

        OnCreateAsteroid?.Invoke(this, asteroidData);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (createButton != null)
        {
            createButton.onClick.RemoveListener(OnCreateButtonClicked);
        }
    }
}