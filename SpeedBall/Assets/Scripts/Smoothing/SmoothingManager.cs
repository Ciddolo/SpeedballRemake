using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothingManager : MonoBehaviour
{
    public ClientManager ClientMng;

    private float speed;
    private float gradient;
    private float InfoPerSecond;
    private Vector2 currentServerPosition;
    private Vector2 previousServerPosition;
    private Vector2 predictedServerPosition;
    private bool isStarted;

    void Start()
    {
        isStarted = false;
        speed = 5.0f;
        InfoPerSecond = 1;
    }

    public void GetNextUpdate(Vector2 newServerPosition)
    {
        gradient = 0;

        previousServerPosition = currentServerPosition;
        currentServerPosition = newServerPosition;

        predictedServerPosition = currentServerPosition + (currentServerPosition - previousServerPosition);

        isStarted = true;
    }

    void Update()
    {
        if (!isStarted)
            return;

        Debug.Log("GRADIENT:[" + gradient + "]");
        Debug.Log("PING:[" + ClientMng.GetPing() + "]");
        transform.position = Vector2.Lerp(currentServerPosition, predictedServerPosition, gradient);
        gradient += ClientMng.GetPing() * InfoPerSecond;
    }
}
