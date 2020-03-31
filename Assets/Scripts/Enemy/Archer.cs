using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TheLostTent
{
    public class Archer : Enemy
    {
        public float maxAttackDelayRange = 1f;
        public float damage = 10f;
        public float dieAnimation = 1f;
        public float projectileSpeed = 150f;
        public GameObject arrowPrefab;
        private Heart heart;

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
            pooler.DisableObj(Constants.PoolTags.Archer, gameObject, 0);
        }

        protected override void ProcessPath()
        {
            if (path == null || heart.IsDead || isAttacking)
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
            int split = (isoRenderer.possibleDirections == PossibleDirection.Four) ? 4 : 8;
            float startTime = Time.time;
            // fire projectile
            Vector2 attackDirection = (target.position - transform.position).normalized;
            float angle = DetermineArrowAngle(attackDirection);
            int dir = CharacterRenderer.DirectionToIndex(direction, split);
            isoRenderer.animator.Play(attackAnimations[dir]);
            yield return new WaitForSeconds(attackDuration / 4);

            GameObject projectile = pooler.GetObject(Constants.PoolTags.Arrow, transform.position, new Vector3(45, 0, angle), transform);
            projectile.GetComponent<Projectile>().Initialise(damage, projectileSpeed, attackDirection);
            pooler.DisableObj(Constants.PoolTags.Arrow, projectile, 3f);

            float randomDelay = UnityEngine.Random.Range(0, maxAttackDelayRange);

            // render animations
            yield return new WaitForSeconds(attackCooldown + randomDelay);
            isAttacking = false;
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