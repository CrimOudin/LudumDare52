using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Olimar : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int maxHealth;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
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
        transform.position += movement * speed * Time.deltaTime;

        // Removed rotation because the art asset didnt need it.
        //if (movement != Vector3.zero)
        //{
        //    var direction = -movement;
        //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //}
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
