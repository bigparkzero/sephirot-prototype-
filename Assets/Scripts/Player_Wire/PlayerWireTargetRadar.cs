using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWireTargetRadar : MonoBehaviour
{
    public Canvas canvas_WireMarkers;
    public Sprite sprite_WireMarker;

    public GameObject go_Player;

    public GameObject closest = null;

    Camera mainCam;

    float RADAR_FAR_RANGE = 45f;
    float RADAR_MIN_RANGE = 6f;

    List<Collider> detectedCols = new List<Collider>();
    List<Collider> visibleCols = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Time.timeScale = 0;
        }
        if (Input.GetKey(KeyCode.Return))
        {
            Time.timeScale = 1;
        }

        detectedCols.Clear();

        foreach (Transform images in canvas_WireMarkers.transform)
        {
            Destroy(images.gameObject);
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);

        Collider[] cols = Physics.OverlapSphere(mainCam.transform.position, RADAR_FAR_RANGE, LayerMask.GetMask("WireTarget"));

        foreach (Collider col in cols)
        {
            float distance = Vector3.Distance(go_Player.transform.position, col.transform.position);
            if (distance > RADAR_FAR_RANGE || distance < RADAR_MIN_RANGE) continue;

            if (GeometryUtility.TestPlanesAABB(planes, col.bounds))
            {
                detectedCols.Add(col);
            }
        }

        closest = null;
        float minDistance = float.MaxValue;
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        foreach (Collider col in detectedCols)
        {
            Vector3 pos = col.transform.position;
            Vector3 screenPos = mainCam.WorldToScreenPoint(pos);

            float distance = Vector2.Distance(screenCenter, screenPos);
            if (distance < minDistance)
            {
                closest = col.gameObject;
                minDistance = distance;
            }

            CreateMarker(pos, screenPos);
        }

        //closest marker has a special visual indicator.
        //while fight, only draw closest marker.
    }

    void CreateMarker(Vector3 pos, Vector3 screenPos)
    {
        GameObject marker = new GameObject("WireTargetMarker");
        marker.transform.SetParent(canvas_WireMarkers.transform);

        Image markerImage = marker.AddComponent<Image>();
        markerImage.sprite = sprite_WireMarker;

        RectTransform rect = marker.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.anchoredPosition = screenPos;
        rect.sizeDelta = new Vector2(45, 45);
    }
}
