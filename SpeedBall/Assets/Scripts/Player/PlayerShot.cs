using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
public class PlayerShot : MonoBehaviour
{
    private const float FORCE = 2000.0f;
    private const float MAX_SHOOT_FORCE = 1000.0f;

    public Vector2 LastDirection { get; set; }
    public Vector2 AimDirection { get; set; }
    public bool InputKey { get; set; }
    public bool InputKeyUp { get; set; }

    private float currentForce;

    void Update()
    {
        if (gameObject.GetComponent<PlayerManager>().Ball == null)
            return;

        gameObject.GetComponent<PlayerManager>().Ball.transform.localPosition = LastDirection;
        if (AimDirection.magnitude > 0.5f)
        {
            LastDirection = AimDirection.normalized;
            float state = currentForce / MAX_SHOOT_FORCE;
            if (InputKey)
            {
                currentForce += FORCE * Time.deltaTime;
                if (state >= 1.0f)
                    Shot();
            }
            else if (InputKeyUp && currentForce > 0.0f)
                Shot();
        }
        else
            currentForce = 0.0f;
    }

    private void Shot()
    {
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallMove>().Direction = AimDirection;
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallMove>().Force = currentForce;
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallBehaviour>().RemoveBall();
        GetComponent<PlayerManager>().BallThrown();
        currentForce = 0.0f;
        InputKey = false;
        InputKeyUp = false;
    }

    public void GKShot(Vector2 direction)
    {
        AimDirection = direction;
        currentForce = MAX_SHOOT_FORCE;
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallBehaviour>().SetMaxSize();
        Shot();
    }
}
