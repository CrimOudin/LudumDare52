using UnityEngine;

public class Olimar : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int maxHealth;
    [SerializeField] private float maxSpeed;
    private int currentHealth;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth; 
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float verticalMovement = Input.GetAxisRaw("Vertical");
        float horizontalMovement = Input.GetAxisRaw("Horizontal");

        Vector3 movement = new Vector2(horizontalMovement, verticalMovement) * 1.4f;
        if (movement.magnitude > 1.4f)
            movement = movement.normalized * 1.4f;

        //transform.position += movement * speed * Time.deltaTime;
        // Removed rotation because the art asset didnt need it.
        if (movement != Vector3.zero)
        {
            if (!animator.GetBool("IsWalking"))
            {
                animator.SetBool("IsWalking", true);
            }
            if (!Mathf.Approximately(horizontalMovement, 0))
            {
                spriteRenderer.flipX = horizontalMovement < 0;
            }
            rigidBody.AddForce(movement * speed * Time.deltaTime, ForceMode2D.Impulse);
            if (rigidBody.velocity.magnitude > maxSpeed)
            {
                rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
            }
            //    var direction = -movement;
            //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            if (animator.GetBool("IsWalking"))
            {
                animator.SetBool("IsWalking", false);
            }
            rigidBody.velocity = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.SetBool("IsAction", true);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            animator.SetBool("IsAction", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Damage"))
        {
            TakeDamage(collision.gameObject.GetComponent<Damage>().Amount);
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if(currentHealth < 0)
        {
            currentHealth = 0;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // TODO: handle game reset/player respawn.
    }
}
