using UnityEngine;

/* Disk.cs controls the disk movement. 
 * When the episode starts, it chooses a random direction
 * for the velocity vector of the disk and check for collisions to trigger events.
 * Handles the angle recomputation logic 
 * when the disk hits a wall (add random value in range [-5.0f, 5.0f]
 * or when the disk hits the paddle */

public class Disk : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField]
    private float speed = 15f;
    public PaddleAgent paddleAgent; 

    private float timePassed = 0.0f; // Used to check if the ball gets stuck for a time interval
    private Vector2 previousVelocity = Vector2.zero;
    private Vector2 lastVelocity;
    private Vector2 startingPos;
    private bool hasCollided = false; //Flag used to limit collisions overlapping

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPos = rb.transform.position;
        AddStartingForce();
    }

    private void AddStartingForce()
    {
        // Choose a random starting direction
        float x = Random.value < 0.5f ? -1.0f : 1.0f;
        float y = Random.value < 0.5f ? Random.Range(-1.0f, -0.5f) : Random.Range(0.5f, 1.0f);

        Vector2 direction = new Vector2(x, y).normalized;

        rb.velocity = direction * this.speed;
    }

    private void LateUpdate()
    {
        lastVelocity = rb.velocity;
        hasCollided = false;

        if (lastVelocity == previousVelocity)
        {
            timePassed += Time.deltaTime;
            if (timePassed >= 3.0f)
            {
                if (Mathf.Abs(lastVelocity.y) < 0.1f)
                    rb.velocity = new Vector2(lastVelocity.x, Random.Range(-0.5f, 0.5f)).normalized * speed;
                else if (Mathf.Abs(lastVelocity.x) < 0.1f)
                    rb.velocity = new Vector2(Random.Range(-0.5f, 0.5f), rb.velocity.y).normalized * speed;

                timePassed = 0.0f;
            } 
        }
        else
        {
            timePassed = 0.0f;
        }

        previousVelocity = lastVelocity;
    }

    /* Recompute the new angle, the new velocity and the event when the disk 
     * collides with a specific element of the field.
     Overlapping collisions are limited using the hasCollided flag.*/
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        float verticalDistance = 0.0f;

        if (collision.gameObject.tag == "Wall")
        {
            if (hasCollided == false) 
            {
                // Reflect the velocity vector
                Vector2 newVelocity = Vector2.Reflect(lastVelocity.normalized, contact.normal);

                float reflectionAngle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                // Add random variation (noise) to the reflection angle in a range of [-5, 5]
                float randAngle = reflectionAngle + Random.Range(-5.0f, 5.0f);

                newVelocity = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));
                //Debug.Log(randAngle +  " " + newVelocity);

                rb.velocity = newVelocity.normalized * speed;
            }
        }
        else if (collision.gameObject.tag == "Paddle")
        {
            hasCollided = true;
            // Reflect the velocity
            Vector2 newVelocity = Vector2.Reflect(lastVelocity.normalized, contact.normal);

            float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
            // rotate velocity vector accordinag to new angle
            newVelocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            //Debug.Log(angle +  " " + newVelocity);
            // Apply the new velocity with constant speed
            rb.velocity = newVelocity * speed;
            
            // Trigger event
            paddleAgent.ResolveEvent(Event.HitByPaddle, verticalDistance);
        }
        else if (collision.gameObject.tag == "OuterBounds")
        {
            // disk went out of bounds. Restart.
            verticalDistance = Mathf.Abs(contact.point.y - paddleAgent.transform.position.y);

            paddleAgent.ResolveEvent(Event.HitOutOfBounds, verticalDistance);
            rb.transform.position = startingPos;
            AddStartingForce();
        }

    }
}