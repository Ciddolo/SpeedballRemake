using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private Vector2 direction;
    private Vector2 velocity;
    private float speed;
    private string horizontalAxisName;
    private string verticalAxisName;

    void Start()
    {
        rigidBody2D = GetComponentInParent<Rigidbody2D>();
        horizontalAxisName = "Horizontal";
        verticalAxisName = "Vertical";
        speed = 400.0f;
    }

    void FixedUpdate()
    {
        rigidBody2D.velocity = velocity;
    }

    void Update()
    {
        direction = new Vector2(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName)).normalized;
        velocity = direction * speed * Time.deltaTime;
    }
}
