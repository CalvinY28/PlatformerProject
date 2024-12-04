using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraControl : MonoBehaviour
{
    public Transform target;
    public Tilemap tilemap;
    public Vector2 offset;
    public Camera followCamera;
    public float smoothing;

    Vector2 viewportHalfSize;

    float leftBound;
    float rightBound;
    float bottomBound;
    private Vector3 shakeOffset;

    void Start()
    {
        tilemap.CompressBounds();
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        viewportHalfSize = new Vector2(followCamera.aspect * followCamera.orthographicSize, followCamera.orthographicSize);

        leftBound = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewportHalfSize.x;
        rightBound = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewportHalfSize.x;
        bottomBound = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewportHalfSize.y;
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z) + shakeOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));

        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, leftBound, rightBound);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, bottomBound, smoothedPosition.y);

        transform.position = smoothedPosition;

    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            shakeOffset = Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }


    void Update()
    {
        
    }
}
