using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour
{
    [Header("俯视小地图摄像机 (Camera)")]
    [Tooltip("拖入场景中用于小地图的俯视相机")]
    public Camera miniMapCamera;

    [Header("小地图 UI 背景 (RawImage) 的 RectTransform")]
    [Tooltip("拖入 Canvas → MiniMapUI（RawImage）的 RectTransform")]
    public RectTransform miniMapUIRect;

    [Header("小地图上的玩家红点 (UI Image)")]
    [Tooltip("拖入 Canvas → MiniMapUI → MiniMapPlayerDot（玩家红点）的 RectTransform")]
    public RectTransform playerDot;

    [Header("小地图上的 AI 黑点 (UI Image)")]
    [Tooltip("拖入 Canvas → MiniMapUI → MiniMapAIDot（AI 黑点）的 Image 组件")]
    public GameObject aiDotParent; // 父物体，包含所有 AI 车的黑点

    private GameManager gameManager;

    void Start()
    {
        // 1. 拿到 GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[MiniMap] 找不到 GameManager，请检查场景中是否挂载了 GameManager 脚本。");
        }

        // 2. 检查引用
        if (miniMapCamera == null)
            Debug.LogError("[MiniMap] miniMapCamera 未赋值，请拖入俯视摄像机。");
        if (miniMapUIRect == null)
            Debug.LogError("[MiniMap] miniMapUIRect 未赋值，请拖入 RawImage 的 RectTransform。");
        if (playerDot == null)
            Debug.LogError("[MiniMap] playerDot 未赋值，请拖入玩家红点的 RectTransform。");
        if (aiDotParent == null)
            Debug.LogError("[MiniMap] aiDotParent 未赋值，请拖入包含 AI 黑点的父物体。");
    }

    void LateUpdate()
    {
        if (gameManager == null || miniMapCamera == null || miniMapUIRect == null || playerDot == null)
            return;

        // 1. 更新玩家红点位置
        UpdatePlayerDot();

        // 2. 更新所有 AI 黑点位置
        UpdateAIDots();
    }

    // 更新玩家红点位置
    void UpdatePlayerDot()
    {
        GameObject carGO = gameManager.PlayerCar;  // 从 GameManager 拿到玩家车辆 GameObject
        if (carGO == null) return;               // 如果车辆还没生成，就不做任何事

        Transform playerTransform = carGO.transform;

        // 把玩家坐标投影到小地图摄像机的视口
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(playerTransform.position);

        if (viewportPos.z <= 0f)
        {
            // 玩家在摄像机后面时隐藏红点
            playerDot.gameObject.SetActive(false);
            return;
        }
        else
        {
            playerDot.gameObject.SetActive(true);
        }

        // 计算红点在 RawImage 上的像素位置
        float mapUIWidth = miniMapUIRect.rect.width;
        float mapUIHeight = miniMapUIRect.rect.height;

        float pixelX = (viewportPos.x * mapUIWidth) - (mapUIWidth * 0.5f);
        float pixelY = (viewportPos.y * mapUIHeight) - (mapUIHeight * 0.5f);

        playerDot.anchoredPosition = new Vector2(pixelX, pixelY);
    }

    // 更新所有 AI 黑点位置
    void UpdateAIDots()
    {
        if (gameManager == null) return;

        // 获取所有 AI 车辆
        var aiCars = GameObject.FindGameObjectsWithTag("AI");  // 获取所有 AI 车
        for (int i = 0; i < aiCars.Length; i++)
        {
            GameObject aiCar = aiCars[i];

            // 1. 获取该 AI 车的黑点
            Transform aiCarDot = aiDotParent.transform.GetChild(i); // 通过索引获取对应的 AI 黑点

            if (aiCarDot == null)
            {
                Debug.LogError("[MiniMap] 没有找到 AI 车辆对应的黑点。");
                continue;
            }

            // 2. 将 AI 车的世界坐标投影到小地图上
            Transform aiTransform = aiCar.transform;
            Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(aiTransform.position);

            if (viewportPos.z <= 0f)
            {
                // 如果 AI 车在摄像机后面，隐藏其黑点
                aiCarDot.gameObject.SetActive(false);
                continue;
            }
            else
            {
                aiCarDot.gameObject.SetActive(true);
            }

            // 3. 计算 AI 黑点在 RawImage 上的像素位置
            float mapUIWidth = miniMapUIRect.rect.width;
            float mapUIHeight = miniMapUIRect.rect.height;

            float pixelX = (viewportPos.x * mapUIWidth) - (mapUIWidth * 0.5f);
            float pixelY = (viewportPos.y * mapUIHeight) - (mapUIHeight * 0.5f);

            aiCarDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(pixelX, pixelY);
        }
    }
}
