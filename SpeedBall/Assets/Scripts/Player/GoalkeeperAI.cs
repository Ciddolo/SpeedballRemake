using UnityEngine;

public class GoalkeeperAI : MonoBehaviour
{
    private const float DEFAULT_SPEED = 300.0f;
    private const float MIN_X_RANGE = -5.0f;
    private const float MAX_X_RANGE = 5.0f;
    private const float RED_Y_VALUE = -23.0f;
    private const float BLUE_Y_VALUE = 23.0f;

    private Rigidbody2D rigidBody2D;
    private Vector2 velocity;
    private Vector2 shotDirection;
    private float speed;
    private float yValue;

    //void Start()
    //{
    //    rigidBody2D = GetComponentInParent<Rigidbody2D>();
    //    speed = DEFAULT_SPEED;
    //    if (gameObject.name == "RED_GK")
    //    {
    //        shotDirection = Vector2.up;
    //        yValue = RED_Y_VALUE;
    //    }
    //    else if (gameObject.name == "BLUE_GK")
    //    {
    //        shotDirection = Vector2.down;
    //        yValue = BLUE_Y_VALUE;
    //    }
    //}

    //void FixedUpdate()
    //{
    //    rigidBody2D.velocity = velocity;
    //}

    //void Update()
    //{
    //    Move();

    //    if (gameObject.GetComponent<PlayerManager>().IsSelected)
    //        Shot();
    //}

    //void OnTriggerStay2D(Collider2D other)
    //{
    //    if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<PlayerManager>().Ball != null)
    //    {
    //        GameObject ball = other.GetComponent<PlayerManager>().Ball;
    //        other.GetComponent<PlayerManager>().BallLost();
    //        ball.GetComponent<BallBehaviour>().AttachBall(gameObject);
    //        Shot();
    //    }
    //}

    //private void Move()
    //{
    //    Vector2 direction = GameManager.Ball.transform.position - transform.position;
    //    direction = new Vector2(direction.x, 0.0f);
    //    if (direction.magnitude > 0.5f)
    //        velocity = direction.normalized * speed * Time.deltaTime;
    //    else
    //        velocity = Vector2.zero;

    //    if (transform.position.x < MIN_X_RANGE)
    //        transform.position = new Vector2(MIN_X_RANGE, yValue);
    //    else if (transform.position.x > MAX_X_RANGE)
    //        transform.position = new Vector2(MAX_X_RANGE, yValue);

    //    if ((new Vector2(transform.position.x, yValue) - (Vector2)transform.position).magnitude > 0.5f)
    //        transform.position = new Vector2(transform.position.x, yValue);
    //}

    //private void Shot()
    //{
    //    gameObject.GetComponent<PlayerShot>().GKShot(shotDirection);
    //    transform.parent.GetComponent<TeamManager>().SelectPlayer((int)Random.Range(1.0f, 4.0f));
    //}
}
