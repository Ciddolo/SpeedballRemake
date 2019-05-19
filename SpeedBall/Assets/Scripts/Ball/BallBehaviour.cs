using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BallMove))]
public class BallBehaviour : MonoBehaviour
{
    private const float MAGNIFY_VALUE = 10.0f;
    private const float DEMAGNIFY_VALUE = 5.0f;
    private const float MAGNIFY_FORCE = 800.0f;
    private const float MAX_SIZE = 2.5f;
    private const float MAX_SIZE_CATCH = 1.5f;

    public GameObject Owner { get; set; }

    private BallMove ballMove;
    private bool isCatchable;
    private Vector3 defaulSize = Vector3.one;
    private Vector3 maxSize = new Vector3(MAX_SIZE, MAX_SIZE, MAX_SIZE);

    void Start()
    {
        ballMove = GetComponent<BallMove>();
        isCatchable = true;
    }

    void Update()
    {
        if (Owner != null)
            return;

        if (ballMove.Force >= MAGNIFY_FORCE)
            transform.localScale = Vector3.Lerp(transform.localScale, maxSize, MAGNIFY_VALUE * Time.deltaTime);
        else
            transform.localScale = Vector3.Lerp(transform.localScale, defaulSize, DEMAGNIFY_VALUE * Time.deltaTime);

        isCatchable = transform.localScale.x <= MAX_SIZE_CATCH;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Goal")
        {
            if (other.gameObject.name == "BlueGoal")
                GameManager.TeamScore(1);
            else
                GameManager.TeamScore(2);

            if (Owner != null)
                RemoveBall();

            ResetPosition();
        }

        if (Owner != null)
            return;

        if (other.gameObject.tag == "Player")
        {
            if ((other.gameObject.name == "RED_GK" || other.gameObject.name == "BLUE_GK") || isCatchable)
                AttachBall(other.gameObject);
        }
        else if (other.gameObject.tag == "Wall")
        {
            float deltaX = Mathf.Abs((transform.position - other.bounds.center).x) - (transform.localScale.x * 0.5f + other.bounds.extents.x);
            float deltaY = Mathf.Abs((transform.position - other.bounds.center).y) - (transform.localScale.x * 0.5f + other.bounds.extents.y);

            if (deltaX > deltaY)
                ballMove.Direction = new Vector2(-ballMove.Direction.x, ballMove.Direction.y);
            else
                ballMove.Direction = new Vector2(ballMove.Direction.x, -ballMove.Direction.y);
        }
    }

    public void AttachBall(GameObject target)
    {
        Owner = target;
        Owner.GetComponent<PlayerManager>().BallReceived();
        transform.localScale = defaulSize;
        transform.parent = target.transform;
        Vector2 lastDirection = transform.position - Owner.transform.position;
        Owner.GetComponent<PlayerShot>().LastDirection = lastDirection.normalized;
        target.GetComponent<PlayerManager>().Ball = gameObject;
    }

    public void RemoveBall()
    {
        Owner.GetComponent<PlayerManager>().Ball = null;
        Owner = null;
        transform.parent = null;
    }

    public void ResetPosition()
    {
        ballMove.Direction = Vector2.zero;
        ballMove.Velocity = Vector2.zero;
        ballMove.Force = 0.0f;
        transform.localPosition = Vector3.zero;
    }
}
