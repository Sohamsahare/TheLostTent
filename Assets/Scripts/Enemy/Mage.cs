using System;
using System.Collections;
using UnityEngine;
namespace TheLostTent
{
    public class Mage : Enemy
    {
        public float attackDelayRange = 2f;
        public float damage = 30f;
        public float dieAnimation = 1f;
        public float chargeTime = 1f;
        public float randomRadius = 4f;
        public float afterEffectTime = .5f;
        public float attackRadius = 2f;
        public GameObject attackPrefab;

        protected override IEnumerator Die()
        {
            // play gfx
            isoRenderer.animator.Play("Die");
            GetComponentInChildren<CircleCollider2D>().enabled = false;
            yield return new WaitForSeconds(dieAnimation);
            pooler.ReturnToPool(Constants.PoolTags.Mage, gameObject, 0);
        }

        protected override IEnumerator Attack()
        {

            isAttacking = true;
            float randomDelay = UnityEngine.Random.Range(0, attackDelayRange);
            int split = (isoRenderer.possibleDirections == PossibleDirection.Four) ? 4 : 8;
            Vector2 attackDirection = (target.position - transform.position).normalized;
            int dir = CharacterRenderer.DirectionToIndex(direction, split);
            yield return new WaitForSeconds(randomDelay);
            GameObject spellObj = pooler.RetrieveFromPool(Constants.PoolTags.Spell, GetAttackPosition(), Vector3.zero, transform);
            spellObj.transform.localScale = Vector3.one * attackRadius;
            spellObj.GetComponent<AreaOfAttack>().Initialise(damage, chargeTime, afterEffectTime);
            pooler.ReturnToPool(Constants.PoolTags.Spell, spellObj, 3f);
            // render animations
            isoRenderer.animator.Play(attackAnimations[dir]);
            yield return new WaitForSeconds(attackCooldown);
            isAttacking = false;
        }

        private Vector3 GetAttackPosition()
        {
            Vector3 position = target.position + new Vector3(UnityEngine.Random.Range(-randomRadius, randomRadius), UnityEngine.Random.Range(-randomRadius, randomRadius), 0);
            return position;
        }

        private float DetermineArrowAngle(Vector2 attackDirection)
        {
            float angle = 0;
            // angle = Vector2.SignedAngle(transform.position, target.position);
            angle = Vector2.SignedAngle(Vector2.up, attackDirection);
            Debug.DrawRay(transform.position, attackDirection, Color.white, 2);
            return angle;
        }
    }
}