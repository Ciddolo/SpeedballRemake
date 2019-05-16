using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BallMove))]
public class BallBehaviour : MonoBehaviour
{
    private const float MAGNIFY_VALUE = 10.0f;
    private const float DEMAGNIFY_VALUE = 5.0f;
    private const float MAGNIFY_FORCE = 800.0f;

    public GameObject Owner { get; set; }

    private BallMove ballMove;
    private Vector3 defaulSize = Vector3.one;
    private Vector3 maxSize = new Vector3(2.0f, 2.0f, 2.0f);

    void Start()
    {
        ballMove = GetComponent<BallMove>();
    }

    void Update()
    {
        if (Owner != null)
            return;

        if (ballMove.Force >= MAGNIFY_FORCE)
            transform.localScale = Vector3.Lerp(transform.localScale, maxSize, MAGNIFY_VALUE * Time.deltaTime);
        else
            transform.localScale = Vector3.Lerp(transform.localScale, defaulSize, DEMAGNIFY_VALUE * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Owner != null)
            return;

        if (other.gameObject.tag == "Player")
        {
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
}
