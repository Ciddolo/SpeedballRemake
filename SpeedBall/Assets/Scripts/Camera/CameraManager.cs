using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float Speed;

    public GameObject Ball { get; set; }
    public GameObject CurrentPlayer { get; set; }

    private Vector2 target;
    private int index;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            IncreaseIndex();

        if (index == 0)
            BallMode();
        else if (index == 1)
            CurrentPlayerMode();
        else if (index == 2)
            BetweenBallAndCurrentPlayerMode();

        Vector3 newPosition;
        if (target.y < -21.0f)
            newPosition = new Vector3(transform.position.x, -21.0f, transform.position.z);
        else if (target.y > 21.0f)
            newPosition = new Vector3(transform.position.x, -21.0f, transform.position.z);
        else
            newPosition = new Vector3(transform.position.x, target.y, transform.position.z);

        gameObject.transform.position = Vector3.Lerp(transform.position, newPosition, Speed * Time.deltaTime);
    }

    private void IncreaseIndex()
    {
        if (++index > 2)
            index = 0;
    }

    private void BallMode()
    {
        target = Ball.transform.position;
    }

    private void CurrentPlayerMode()
    {
        target = CurrentPlayer.transform.position;
    }

    private void BetweenBallAndCurrentPlayerMode()
    {
        target = Ball.transform.position + (CurrentPlayer.transform.position - Ball.transform.position) * 0.5f;
    }
}
