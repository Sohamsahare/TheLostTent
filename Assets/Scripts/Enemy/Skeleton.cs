using System.Collections;
using UnityEngine;

namespace TheLostTent
{
    public class Skeleton : Enemy
    {
        public float damage = 10f;
        public float dieAnimation = 1f;
        private bool isAttackAnimating = false;

        protected override IEnumerator Die()
        {
            // play gfx
            isoRenderer.animator.Play("Die");
            GetComponentInChildren<CircleCollider2D>().enabled = false;
            yield return new WaitForSeconds(dieAnimation);
            pooler.ReturnToPool(Constants.PoolTags.Skeleton, gameObject, 0);
        }

        protected override IEnumerator Attack()
        {
            isAttacking = true;
            isAttackAnimating = true;
            int split = (isoRenderer.possibleDirections == PossibleDirection.Four) ? 4 : 8;
            float startTime = Time.time;
            int dir = CharacterRenderer.DirectionToIndex(direction, split);

            isoRenderer.animator.Play(attackAnimations[dir]);

            float journey = 0;
            bool hasAttacked = false;
            while (journey <= attackDuration)
            {
                journey += Time.deltaTime;
                if (
                    !hasAttacked &&
                    // will allow the player to evade from the attack in tkhe 
                    // first half of attack duration
                    journey >= attackDuration / 2 &&
                    Vector2.Distance(transform.position, target.position) <= attackRange
                )
                {
                    target.GetComponent<Witch>().TakeDamage(damage);
                    hasAttacked = true;
                }
                yield return null;
            }
            isAttackAnimating = false;
            yield return new WaitForSeconds(attackCooldown);
            isAttacking = false;
        }
    }
}