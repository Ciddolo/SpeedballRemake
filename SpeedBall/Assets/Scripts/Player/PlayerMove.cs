using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerManager))]
public class PlayerMove : MonoBehaviour
{
    private const float DEFAULT_SPEED = 400.0f;

    private PlayerManager playerManager;
    private Rigidbody2D rigidBody2D;
    private Vector2 direction;
    private Vector2 velocity;
    private float speed;
    private string horizontalAxisName;
    private string verticalAxisName;

    void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();
        rigidBody2D = GetComponentInParent<Rigidbody2D>();
        horizontalAxisName = "Horizontal";
        verticalAxisName = "Vertical";
        speed = DEFAULT_SPEED;
    }

    void FixedUpdate()
    {
        if (!playerManager.IsSelected)
            rigidBody2D.velocity = Vector2.zero;
        else
            rigidBody2D.velocity = velocity;
    }

    void Update()
    {
        if (!playerManager.IsSelected)
            return;

        direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;
        velocity = direction * speed * Time.deltaTime;
    }
}
