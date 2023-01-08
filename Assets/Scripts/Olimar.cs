using UnityEngine;

public class Olimar : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int maxHealth;
    [SerializeField] private float maxSpeed;
    private int currentHealth;
    private Rigidbody2D rigidBody;

    private void Start()
    {
        currentHealth = maxHealth; 
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float verticalMovement = 0;
        float horizontalMovement = 0;
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            verticalMovement += 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            verticalMovement -= 1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalMovement -= 1;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalMovement += 1;
        }

        Vector3 movement = new Vector2(horizontalMovement, verticalMovement);
        //transform.position += movement * speed * Time.deltaTime;
        // Removed rotation because the art asset didnt need it.
        if (movement != Vector3.zero)
        {
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
            rigidBody.velocity = Vector3.zero;
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
