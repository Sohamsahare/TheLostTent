using UnityEngine;
namespace TheLostTent
{
    public class Fireball : MonoBehaviour
    {
        public float damage = 10f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Debug.Log("Collided with " + other.tag);
            if (other.tag == "Enemy")
            {
                // Damage enemy
                other.GetComponentInParent<Heart>().Damage(damage);
                Destroy(gameObject);
            }
        }
    }
}