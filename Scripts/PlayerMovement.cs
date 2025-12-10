using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float jumpForce = 300f;
    [SerializeField] private Transform leftFoot, rightFoot;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private AudioClip jumpSound, pickupSound;
    [SerializeField] private GameObject cherryParticles, dustParticles;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillColor;
    [SerializeField] private Color greenHealth, redHealth;
    [SerializeField] private TMP_Text cherryText;
  

    private float horizontalValue;
    private float rayDistance = 0.25f;
    private bool isGrounded;
    private bool canMove;
    private int startingHealth = 5;
    private int currentHealth = 0;
    public int cherriesCollected = 0;

    private Rigidbody2D rgbd;
    private SpriteRenderer rend;
    private Animator anim;
    private AudioSource audioSource;


    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
        currentHealth = startingHealth;
        cherryText.text = "" + cherriesCollected;
        rgbd = GetComponent<Rigidbody2D>();
        rend = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalValue = Input.GetAxis("Horizontal");

        if(horizontalValue < 0)
        {
            FlipSprite(true);
        }

        if(horizontalValue > 0)
        {
            FlipSprite(false); 
        }


        if (Input.GetButtonDown("Jump") && CheckIfGrounded() == true)
        {
            Jump();
        }

        anim.SetFloat("MoveSpeed", Mathf.Abs(rgbd.velocity.x) );
        anim.SetFloat("VerticalSpeed", rgbd.velocity.y);
        anim.SetBool("IsGrounded", CheckIfGrounded());

        

    }

    private void FixedUpdate()
    {
        if(!canMove)
        {
            return;
        }

        rgbd.velocity = new Vector2(horizontalValue * moveSpeed * Time.deltaTime, rgbd.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cherry"))
        {
            Destroy(other.gameObject);
            cherriesCollected++;
            cherryText.text = "" + cherriesCollected;
            audioSource.PlayOneShot(pickupSound, 0.5f);
            Instantiate(cherryParticles, other.transform.position, Quaternion.identity);
        }

        if (other.CompareTag("Health"))
        {
            RestoreHealth(other.gameObject);
        }

    }

    private void FlipSprite(bool direction)
    {
        rend.flipX = direction; 
    }

    private void Jump()
    {
        rgbd.AddForce(new Vector2(0, jumpForce));
        audioSource.PlayOneShot(jumpSound, 0.5f);
        Instantiate(dustParticles, transform.position, dustParticles.transform.localRotation);

    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Respawn();
        }
    }

    public void TakeKnockback(float knockbackForce, float upwards)
    {
        canMove = false;
        rgbd.AddForce(new Vector2(knockbackForce, upwards));
        Invoke("CanMoveAgain", 0.25f);
    }

    private void CanMoveAgain()
    {
        canMove = true;
    }

    private void Respawn()
    {
        currentHealth = startingHealth;
        UpdateHealthBar();
        transform.position = spawnPosition.position;
        rgbd.velocity = Vector2.zero;
    }

    private void RestoreHealth(GameObject healthPickup)
    {
        if(currentHealth >= startingHealth)
        {
            return;
        }
        else
        {
            int healthToRestore = healthPickup.GetComponent<HealthPickup>().healthAmount;
            currentHealth += healthToRestore;
            UpdateHealthBar();
            Destroy(healthPickup);

            if(currentHealth >= startingHealth)
            {
                currentHealth = startingHealth;
            }
        }
    }

    private void UpdateHealthBar()
    {
        healthSlider.value = currentHealth;
        if(currentHealth >= 2)
        {
            fillColor.color = greenHealth;
        }
        else
        {
            fillColor.color = redHealth;
        }
    }

    private bool CheckIfGrounded()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(leftFoot.position, Vector2.down, rayDistance, whatIsGround);
        RaycastHit2D rightHit = Physics2D.Raycast(rightFoot.position, Vector2.down, rayDistance, whatIsGround);


        //Debug.DrawRay(leftFoot.position, Vector2.down * rayDistance, Color.blue, 0.25f);
        // Debug.DrawRay(rightFoot.position, Vector2.down * rayDistance, Color.red, 0.25f);


        if (leftHit.collider != null && leftHit.collider.CompareTag("Ground") || rightHit.collider != null && rightHit.collider.CompareTag("Ground") )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
