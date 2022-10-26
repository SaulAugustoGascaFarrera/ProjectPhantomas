using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] float playerSpeed = 10.0f;
    [SerializeField] float jumpForce = 5.0f;
    public float dashSpeed = 20.0f;
    Rigidbody2D rb;
    Vector2 direction;

    [Header("Collisions")]
    [SerializeField] Vector2 down;
    [SerializeField] float collisionRadius;
    [SerializeField] LayerMask layerGround;


    [Header("Booleans")]
    public bool canMove = true;
    public bool isInGround = true;

    [Header("Dash Booleans")]
    public bool canDash = false;
    public bool doingDash = false;
    public bool isTouchingGround = false;

    [Header("Animations")]
    Animator animator;

    [Header("Cinemachine Stats && Atts")]
    public bool doingShake = false;
     CinemachineVirtualCamera cm;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
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


    #region CinemachineCamera Functions

    IEnumerator ShakeCamera()
    {
        doingShake = true;

        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;

        yield return new WaitForSeconds(0.3f);

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0.0f;

        doingShake = false;
    }


    IEnumerator ShakeCamera(float time)
    {
        doingShake = true;

        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;

        yield return new WaitForSeconds(time);

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0.0f;

        doingShake = false;
    }

    #endregion

    #region Dash Functions


    void Dash(float x, float y)
    {
        animator.SetBool("Dash", true);

        Vector3 playerPosition = Camera.main.WorldToViewportPoint(transform.position);

        Camera.main.GetComponent<RippleEffect>().Emit(playerPosition);

        StartCoroutine(ShakeCamera());

        canDash = true;

        rb.velocity = Vector2.zero;

        rb.velocity += new Vector2(x, y).normalized * dashSpeed;

        StartCoroutine(DashCoroutine());

    }


    IEnumerator DashCoroutine()
    {

        StartCoroutine(DashGround());

        rb.gravityScale = 0;
        doingDash = true;

        yield return new WaitForSeconds(0.3f);

        rb.gravityScale = 3;
        doingDash = false;

        FinishDashAnimation();
    }

    IEnumerator DashGround()
    {
        yield return new WaitForSeconds(0.15f);

        if (isInGround)
        {
            canDash = false;
        }
    }


    void TouchingGround()
    {
        canDash = false;
        doingDash = false;
        animator.SetBool("Jump", true);
    }


    public void FinishDashAnimation()
    {
        animator.SetBool("Dash", false);
    }




    #endregion



    #region Movement Functions


    void Movement()
    {
        float x = Input.GetAxis("Horizontal");

        float y = Input.GetAxis("Vertical");


        //Dash Variables
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");


        direction = new Vector2(x, y);

        Walk();

        ExtensiveJump();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isInGround)
            {
                animator.SetBool("Jump",true);
                Jump();
            }
        }


        if(isInGround && !isTouchingGround)
        {
            TouchingGround();
            isTouchingGround = true;
        }

        if(!isInGround && isTouchingGround)
        {
            isTouchingGround = false;
        }

        if(Input.GetKeyDown(KeyCode.X) && !doingDash)
        {
            //Camera.main.GetComponent<RippleEffect>().Emit(transform.position);

            if(xRaw != 0.0f || yRaw != 0.0f)
            {
                Dash(xRaw, yRaw);
            }
        }


        float velocity;

        if (rb.velocity.y > 0)
        {
            velocity = 1;
        }
        else
        {
            velocity = -1;
        }

        

        if (!isInGround)
        {

            animator.SetFloat("VerticalVelocity", velocity);

        }
        else
        {
            if(velocity == -1)
                FinishJump();
        }
       

    }


    public void FinishJump()
    {
        
            animator.SetBool("Jump", false);

            //animator.SetBool("Fall", true);

    }

    void Walk()
    {
        if(canMove && !doingDash)
        {
            rb.velocity = new Vector2(direction.x * playerSpeed, rb.velocity.y);

            if(direction != Vector2.zero)
            {

                if(!isInGround)
                {
                    animator.SetBool("Jump",true);
                }
                else
                {
                    animator.SetBool("Walk",true);
                }

                if(direction.x < 0 && transform.localScale.x > 0)
                {
                    transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z); 

                }else if(direction.x > 0 && transform.localScale.x < 0)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                animator.SetBool("Walk", false);
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
