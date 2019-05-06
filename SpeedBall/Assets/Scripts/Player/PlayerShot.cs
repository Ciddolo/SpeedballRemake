using UnityEngine;

public class PlayerShot : MonoBehaviour
{
    private const float FORCE = 2000.0f;
    private const float MAX_SHOOT_FORCE = 1000.0f;

    public GameObject Ball { get; set; }
    public Vector2 LastDirection { get; set; }

    private string horizontalAxisName;
    private string verticalAxisName;
    private Vector2 direction;
    private float currentForce;

    void Start()
    {
        horizontalAxisName = "Horizontal Aim";
        verticalAxisName = "Vertical Aim";
    }

    void Update()
    {
        if (Ball == null)
            return;

        direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;

        Ball.transform.localPosition = LastDirection;

        if (direction.magnitude > 0.5f)
        {
            LastDirection = direction.normalized;

            float state = currentForce / MAX_SHOOT_FORCE;
            if (Input.GetKey(KeyCode.Space))
            {
                currentForce += FORCE * Time.deltaTime;
                if (state >= 1.0f)
                    Shot();
            }
            else if (Input.GetKeyUp(KeyCode.Space) && currentForce > 0.0f)
                Shot();
        }
        else
            currentForce = 0.0f;
    }

    private void Shot()
    {
        Ball.GetComponent<BallMove>().Direction = direction;
        Ball.GetComponent<BallMove>().Force = currentForce;
        Ball.GetComponent<BallBehaviour>().RemoveBall();
        currentForce = 0.0f;
    }
}
