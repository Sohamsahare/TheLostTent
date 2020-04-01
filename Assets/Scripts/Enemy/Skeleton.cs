using System.Collections;
using UnityEngine;

namespace TheLostTent
{
    public class Skeleton : Enemy
    {
        public float damage = 10f;
        public float dieAnimation = 1f;
        private Heart heart;
        private bool isAttackAnimating = false;

        private new void Awake()
        {
            base.Awake();
            heart = GetComponent<Heart>();
        }

        private void Start()
        {
            InvokeRepeating("UpdatePath", 0f, .5f);
            heart.SetStats(HP);
            heart.deathEvent += () => StartCoroutine(Die());
        }

        IEnumerator Die()
        {
            // play gfx
            isoRenderer.animator.Play("Die");
            GetComponentInChildren<CircleCollider2D>().enabled = false;
            yield return new WaitForSeconds(dieAnimation);
            pooler.ReturnToPool(Constants.PoolTags.Skeleton, gameObject, 0);
        }

        protected override void ProcessPath()
        {
            if (path == null || heart.IsDead || isAttackAnimating)
            {
                return;
            }

            if (currentWaypoint >= path.vectorPath.Count)
            {
                hasReachedEndOfPath = true;
                return;
            }
            else
            {
                hasReachedEndOfPath = false;
            }

            direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            motor.Move(direction);
            isoRenderer.SetDirection(direction);

            float distance = Vector2.Distance(path.vectorPath[currentWaypoint], rb.position);
            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
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