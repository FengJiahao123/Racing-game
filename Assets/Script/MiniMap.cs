using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour
{
    [Header("Top-Down Mini Map Camera")]
    [Tooltip("Drag the top-down camera used for the mini-map in the scene")]
    public Camera miniMapCamera;

    [Header("Mini Map UI Background (RawImage) RectTransform")]
    [Tooltip("Drag the RectTransform of the RawImage in Canvas ¡ú MiniMapUI")]
    public RectTransform miniMapUIRect;

    [Header("Player Dot on the Mini Map (UI Image)")]
    [Tooltip("Drag the RectTransform of the MiniMapPlayerDot (player dot) in Canvas ¡ú MiniMapUI")]
    public RectTransform playerDot;

    [Header("AI Dots on the Mini Map (UI Image)")]
    [Tooltip("Drag the Image component of MiniMapAIDot (AI dot) in Canvas ¡ú MiniMapUI")]
    public GameObject aiDotParent; // Parent object containing all AI car dots

    private GameManager gameManager;

    void Start()
    {
        // 1. Get the GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[MiniMap] GameManager not found. Please check if the GameManager script is attached in the scene.");
        }

        // 2. Check references
        if (miniMapCamera == null)
            Debug.LogError("[MiniMap] miniMapCamera is not assigned. Please drag the top-down camera.");
        if (miniMapUIRect == null)
            Debug.LogError("[MiniMap] miniMapUIRect is not assigned. Please drag the RectTransform of RawImage.");
        if (playerDot == null)
            Debug.LogError("[MiniMap] playerDot is not assigned. Please drag the RectTransform of the player dot.");
        if (aiDotParent == null)
            Debug.LogError("[MiniMap] aiDotParent is not assigned. Please drag the parent object containing AI car dots.");
    }

    void LateUpdate()
    {
        if (gameManager == null || miniMapCamera == null || miniMapUIRect == null || playerDot == null)
            return;

        // 1. Update player dot position
        UpdatePlayerDot();

        // 2. Update all AI dots positions
        UpdateAIDots();
    }

    // Update the player dot position
    void UpdatePlayerDot()
    {
        GameObject carGO = gameManager.PlayerCar;  // Get the player vehicle GameObject from GameManager
        if (carGO == null) return;               // If the vehicle hasn't spawned, do nothing

        Transform playerTransform = carGO.transform;

        // Project the player's coordinates to the mini-map camera's viewport
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(playerTransform.position);

        if (viewportPos.z <= 0f)
        {
            // Hide the player dot if the player is behind the camera
            playerDot.gameObject.SetActive(false);
            return;
        }
        else
        {
            playerDot.gameObject.SetActive(true);
        }

        // Calculate the pixel position of the dot on the RawImage
        float mapUIWidth = miniMapUIRect.rect.width;
        float mapUIHeight = miniMapUIRect.rect.height;

        float pixelX = (viewportPos.x * mapUIWidth) - (mapUIWidth * 0.5f);
        float pixelY = (viewportPos.y * mapUIHeight) - (mapUIHeight * 0.5f);

        playerDot.anchoredPosition = new Vector2(pixelX, pixelY);
    }

    // Update all AI dots positions
    void UpdateAIDots()
    {
        if (gameManager == null) return;

        // Get all AI vehicles
        var aiCars = GameObject.FindGameObjectsWithTag("AI");  // Get all AI cars
        for (int i = 0; i < aiCars.Length; i++)
        {
            GameObject aiCar = aiCars[i];

            // 1. Get the AI car's dot
            Transform aiCarDot = aiDotParent.transform.GetChild(i); // Get the corresponding AI dot by index

            if (aiCarDot == null)
            {
                Debug.LogError("[MiniMap] Could not find the corresponding dot for the AI vehicle.");
                continue;
            }

            // 2. Project the AI car's world position to the mini-map
            Transform aiTransform = aiCar.transform;
            Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(aiTransform.position);

            if (viewportPos.z <= 0f)
            {
                // If the AI car is behind the camera, hide its dot
                aiCarDot.gameObject.SetActive(false);
                continue;
            }
            else
            {
                aiCarDot.gameObject.SetActive(true);
            }

            // 3. Calculate the AI dot's pixel position on the RawImage
            float mapUIWidth = miniMapUIRect.rect.width;
            float mapUIHeight = miniMapUIRect.rect.height;

            float pixelX = (viewportPos.x * mapUIWidth) - (mapUIWidth * 0.5f);
            float pixelY = (viewportPos.y * mapUIHeight) - (mapUIHeight * 0.5f);

            aiCarDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(pixelX, pixelY);
        }
    }
}
