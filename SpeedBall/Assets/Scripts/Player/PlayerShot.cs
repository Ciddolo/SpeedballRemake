using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
public class PlayerShot : MonoBehaviour
{
    private const float FORCE = 10.0f;
    private const float MAX_SHOOT_FORCE = 20.0f;

    public Vector2 LastDirection { get; set; }
    public Vector2 AimDirection { get; set; }
    public bool InputKey { get; set; }
    public bool InputKeyUp { get; set; }

    private float currentForce;

    void Start()
    {
        currentForce = FORCE;
    }

    void Update()
    {
        if (gameObject.GetComponent<PlayerManager>().Ball == null)
            return;

        //gameObject.GetComponent<PlayerManager>().Ball.transform.localPosition = LastDirection;
        if (AimDirection.magnitude > 0.5f)
        {
            LastDirection = AimDirection.normalized;
            if (InputKey)
            {
                currentForce += FORCE * Time.deltaTime;
                if (currentForce >= MAX_SHOOT_FORCE)
                    Shot();
            }
            else if (InputKeyUp)
                Shot();
        }
        else
            currentForce = FORCE;
    }

    private void Shot()
    {
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallMove>().Direction = AimDirection;
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallMove>().Force = currentForce;
        gameObject.GetComponent<PlayerManager>().Ball.GetComponent<BallBehaviour>().RemoveBall();
        GetComponent<PlayerManager>().BallThrown();
        GetComponent<PlayerManager>().Team.ClientOwner.GetComponent<ClientManager>().SendShot(AimDirection.x, AimDirection.y, currentForce);
        currentForce = FORCE;
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
