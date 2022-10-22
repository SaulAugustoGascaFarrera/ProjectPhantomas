using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] float playerSpeed = 10.0f;
    [SerializeField] float jumpForce = 5.0f;
    Rigidbody2D rb;
    Vector2 direction;

    [Header("Collisions")]
    [SerializeField] Vector2 down;
    [SerializeField] float collisionRadius;
    [SerializeField] LayerMask layerGround;


    [Header("Booleans")]
    public bool canMove = true;
    public bool isInGround = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Grips();
    }

    #region Movement Functions


    void Movement()
    {
        float x = Input.GetAxis("Horizontal");

        float y = Input.GetAxis("Vertical");

        direction = new Vector2(x, y);

        Walk();

        ExtensiveJump();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isInGround) Jump();
        }


    }


    void Walk()
    {
        if(canMove)
        {
            rb.velocity = new Vector2(direction.x * playerSpeed, rb.velocity.y);

            if(direction != Vector2.zero)
            {
                if(direction.x < 0 && transform.localScale.x > 0)
                {
                    transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z); 

                }else if(direction.x > 0 && transform.localScale.x < 0)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
        }
        
    }


    #endregion


    #region Jump Functions

    void Grips()
    {
        isInGround = Physics2D.OverlapCircle((Vector2)transform.position + down,collisionRadius,layerGround);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += Vector2.up * jumpForce;
    }

    void ExtensiveJump()
    {
        if (rb.velocity.y < 0)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.0f - 1) * Time.deltaTime;
    }


    #endregion

}
