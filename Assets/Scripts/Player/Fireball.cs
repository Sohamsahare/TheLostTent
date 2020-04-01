using UnityEngine;
namespace TheLostTent
{
    public class Fireball : MonoBehaviour
    {
        public float damage = 10f;
        private Pooler pooler;
        private void Awake()
        {

            pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Debug.Log("Collided with " + other.tag);
            if (other.tag == "Enemy")
            {
                // Damage enemy
                other.GetComponentInParent<Heart>().Damage(damage);
                pooler.ReturnToPool(Constants.PoolTags.Fireball, gameObject, 0);
            }
        }
    }
}