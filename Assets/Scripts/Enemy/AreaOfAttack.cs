using System;
using System.Collections;
using UnityEngine;

namespace TheLostTent
{
    public class AreaOfAttack : MonoBehaviour
    {

        public float damage = 30f;
        public float afterEffectTime = .25f;
        public float chargeTime = 1f;
        private CircleCollider2D trigger2D;
        private Rigidbody2D rb;
        private Animator animator;
        private Pooler pooler;

        private void Awake()
        {
            pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            trigger2D = GetComponent<CircleCollider2D>();
        }

        private void Start()
        {
            StartCoroutine(Attack());
        }

        IEnumerator Attack()
        {
            trigger2D.enabled = false;
            yield return new WaitForSeconds(chargeTime);
            trigger2D.enabled = true;
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(afterEffectTime);
            trigger2D.enabled = false;
            // TODO: POOL
            pooler.ReturnToPool(Constants.PoolTags.Spell, gameObject, 0);
        }

        public void Initialise(float damage, float chargeTime, float afterEffectTime)
        {
            this.damage = damage;
            this.chargeTime = chargeTime;
            this.afterEffectTime = afterEffectTime;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.transform.tag == "Player")
            {
                Debug.Log("Collide with " + col.transform.name);
                // Damage player
                col.transform.GetComponentInParent<Witch>().TakeDamage(damage);
                // run some anim if any
                // and pool it back
                pooler.ReturnToPool(Constants.PoolTags.Spell, gameObject, 0);
            }
        }
    }
}
