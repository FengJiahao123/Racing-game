using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour
{
    [Header("����С��ͼ����� (Camera)")]
    [Tooltip("���볡��������С��ͼ�ĸ������")]
    public Camera miniMapCamera;

    [Header("С��ͼ UI ���� (RawImage) �� RectTransform")]
    [Tooltip("���� Canvas �� MiniMapUI��RawImage���� RectTransform")]
    public RectTransform miniMapUIRect;

    [Header("С��ͼ�ϵ���Һ�� (UI Image)")]
    [Tooltip("���� Canvas �� MiniMapUI �� MiniMapPlayerDot����Һ�㣩�� RectTransform")]
    public RectTransform playerDot;

    [Header("С��ͼ�ϵ� AI �ڵ� (UI Image)")]
    [Tooltip("���� Canvas �� MiniMapUI �� MiniMapAIDot��AI �ڵ㣩�� Image ���")]
    public GameObject aiDotParent; // �����壬�������� AI ���ĺڵ�

    private GameManager gameManager;

    void Start()
    {
        // 1. �õ� GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[MiniMap] �Ҳ��� GameManager�����鳡�����Ƿ������ GameManager �ű���");
        }

        // 2. �������
        if (miniMapCamera == null)
            Debug.LogError("[MiniMap] miniMapCamera δ��ֵ�������븩���������");
        if (miniMapUIRect == null)
            Debug.LogError("[MiniMap] miniMapUIRect δ��ֵ�������� RawImage �� RectTransform��");
        if (playerDot == null)
            Debug.LogError("[MiniMap] playerDot δ��ֵ����������Һ��� RectTransform��");
        if (aiDotParent == null)
            Debug.LogError("[MiniMap] aiDotParent δ��ֵ����������� AI �ڵ�ĸ����塣");
    }

    void LateUpdate()
    {
        if (gameManager == null || miniMapCamera == null || miniMapUIRect == null || playerDot == null)
            return;

        // 1. ������Һ��λ��
        UpdatePlayerDot();

        // 2. �������� AI �ڵ�λ��
        UpdateAIDots();
    }

    // ������Һ��λ��
    void UpdatePlayerDot()
    {
        GameObject carGO = gameManager.PlayerCar;  // �� GameManager �õ���ҳ��� GameObject
        if (carGO == null) return;               // ���������û���ɣ��Ͳ����κ���

        Transform playerTransform = carGO.transform;

        // ���������ͶӰ��С��ͼ��������ӿ�
        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(playerTransform.position);

        if (viewportPos.z <= 0f)
        {
            // ��������������ʱ���غ��
            playerDot.gameObject.SetActive(false);
            return;
        }
        else
        {
            playerDot.gameObject.SetActive(true);
        }

        // �������� RawImage �ϵ�����λ��
        float mapUIWidth = miniMapUIRect.rect.width;
        float mapUIHeight = miniMapUIRect.rect.height;

        float pixelX = (viewportPos.x * mapUIWidth) - (mapUIWidth * 0.5f);
        float pixelY = (viewportPos.y * mapUIHeight) - (mapUIHeight * 0.5f);

        playerDot.anchoredPosition = new Vector2(pixelX, pixelY);
    }

    // �������� AI �ڵ�λ��
    void UpdateAIDots()
    {
        if (gameManager == null) return;

        // ��ȡ���� AI ����
        var aiCars = GameObject.FindGameObjectsWithTag("AI");  // ��ȡ���� AI ��
        for (int i = 0; i < aiCars.Length; i++)
        {
            GameObject aiCar = aiCars[i];

            // 1. ��ȡ�� AI ���ĺڵ�
            Transform aiCarDot = aiDotParent.transform.GetChild(i); // ͨ��������ȡ��Ӧ�� AI �ڵ�

            if (aiCarDot == null)
            {
                Debug.LogError("[MiniMap] û���ҵ� AI ������Ӧ�ĺڵ㡣");
                continue;
            }

            // 2. �� AI ������������ͶӰ��С��ͼ��
            Transform aiTransform = aiCar.transform;
            Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(aiTransform.position);

            if (viewportPos.z <= 0f)
            {
                // ��� AI ������������棬������ڵ�
                aiCarDot.gameObject.SetActive(false);
                continue;
            }
            else
            {
                aiCarDot.gameObject.SetActive(true);
            }

            // 3. ���� AI �ڵ��� RawImage �ϵ�����λ��
            float mapUIWidth = miniMapUIRect.rect.width;
            float mapUIHeight = miniMapUIRect.rect.height;

            float pixelX = (viewportPos.x * mapUIWidth) - (mapUIWidth * 0.5f);
            float pixelY = (viewportPos.y * mapUIHeight) - (mapUIHeight * 0.5f);

            aiCarDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(pixelX, pixelY);
        }
    }
}
