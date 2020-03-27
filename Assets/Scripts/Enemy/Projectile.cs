using UnityEngine;
namespace TheLostTent
{
    public class Projectile : MonoBehaviour
    {
        public float damage;
        public float speed;
        private Vector2 direction;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            rb.AddForce(direction * speed);
        }

        public void Initialise(float damage, float speed, Vector2 direction)
        {
            this.damage = damage;
            this.speed = speed;
            this.direction = direction;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.transform.tag == "Player")
            {
                Debug.Log("Collide with " + col.transform.name);
                // Damage player
                col.transform.GetComponentInParent<Witch>().TakeDamage(damage);
                // run some anim if any
                // and destroy afterwards
                // TODO: Pool later on
                Destroy(gameObject);
            }
        }
    }
}