using System.Collections.Generic;
using UnityEngine;

public enum CameraMode
{
    Between,
    Ball,
    Player,
    MAX
}

public class CameraManager : MonoBehaviour
{
    private const float SPEED = 50.0f;
    private const float MIN_Y = -21.0f;
    private const float MAX_Y = 21.0f;

    public GameObject Ball { get; set; }
    public GameObject CurrentPlayer { get; set; }

    private delegate void Mode();
    private Dictionary<int, Mode> modeTable;
    private Vector2 target;
    private int index;

    void Start()
    {
        modeTable = new Dictionary<int, Mode>();
        modeTable[(int)CameraMode.Between] = BetweenBallAndCurrentPlayerMode;
        modeTable[(int)CameraMode.Ball] = BallMode;
        modeTable[(int)CameraMode.Player] = CurrentPlayerMode;
    }

    void Update()
    {
        if (Ball == null || CurrentPlayer == null)
            return;
        modeTable[index]();
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector3 newPosition;
        if (target.y < MIN_Y)
            newPosition = new Vector3(transform.position.x, MIN_Y, transform.position.z);
        else if (target.y > MAX_Y)
            newPosition = new Vector3(transform.position.x, MAX_Y, transform.position.z);
        else
            newPosition = new Vector3(transform.position.x, target.y, transform.position.z);

        gameObject.transform.position = Vector3.Lerp(transform.position, newPosition, SPEED * Time.deltaTime);
    }

    public void IncreaseIndex()
    {
        if (++index == (int)CameraMode.MAX)
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
