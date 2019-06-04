using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoothing : MonoBehaviour
{
    public ClientManager ClientMng;

    private bool isStarted;
    private float speed;
    private float gradient;
    private float timeBetweenTwoInfo;
    private float oldTimeInfo;
    private float timer;
    private uint infoPerSecond;
    private Vector2 currentServerPosition;
    private Vector2 previousServerPosition;
    private Vector2 predictedServerPosition;

    void Start()
    {
        isStarted = false;
        speed = 5.0f;
        infoPerSecond = 0;
        timeBetweenTwoInfo = 0;
        timer = 0.0f;
    }

    public void IncreaseInfo()
    {
        if (timer < 1.0f)
            infoPerSecond++;
    }

    public void GetNextUpdate(Vector2 newServerPosition, float speed)
    {
        gradient = 0;

        this.speed = speed;

        previousServerPosition = currentServerPosition;
        currentServerPosition = newServerPosition;

        predictedServerPosition = currentServerPosition + (currentServerPosition - previousServerPosition);

        IncreaseInfo();

        timeBetweenTwoInfo = oldTimeInfo;
        oldTimeInfo = 0.0f;

        isStarted = true;
    }

    void Update()
    {
        if (!isStarted)
            return;

        Debug.Log("------------------------------------------------------------");
        Debug.Log("GRADIENT:[" + gradient + "]");
        Debug.Log("INFO PER SECOND:[" + infoPerSecond + "]");
        Debug.Log("TIME BETWEEN INFO:[" + timeBetweenTwoInfo + "]");
        Debug.Log("------------------------------------------------------------");

        if (timer < 1.0f)
            timer += Time.deltaTime;

        oldTimeInfo += Time.deltaTime;

        transform.position = Vector2.Lerp(currentServerPosition, predictedServerPosition, gradient);
        gradient += Time.deltaTime * infoPerSecond;
    }
}
