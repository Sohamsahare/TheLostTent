using UnityEngine;
using Pathfinding;
using System.Collections;
namespace TheLostTent
{
    public class Enemy : Character, IPoolable
    {
        public Transform target;
        public float attackRange = 0.5f;
        public float attackCooldown = 1f;
        public float nextWaypointDistance = .5f;
        public float attackDuration = .5f;
        protected bool isAttacking;
        protected Seeker seeker;
        protected Path path;
        protected int currentWaypoint;
        protected bool hasReachedEndOfPath = false;
        protected Pooler pooler;
        protected readonly string[] attackAnimations =
        {
            "Attack N",
            "Attack W",
            "Attack S",
            "Attack E"
        };

        protected Vector2 direction;

        protected CharacterMotor motor;
        protected Heart heart;

        public virtual void Damage(float damage)
        {
            Debug.LogWarning("Not implemented");
        }

        protected new void Awake()
        {
            base.Awake();
            heart = GetComponent<Heart>();
            pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
            seeker = GetComponent<Seeker>();
            motor = GetComponent<CharacterMotor>();
        }

        private void Start()
        {
            ResetBehaviour();
        }

        private void OnEnable()
        {
            heart.deathEvent += () => StartCoroutine(Die());
        }

        private void OnDisable()
        {
            CancelInvoke("UpdatePath");
            heart.deathEvent -= () => StartCoroutine(Die());
        }

        protected void Update()
        {
            var distance = (target.position - transform.position).sqrMagnitude;
            bool isInAttackRange = distance <= attackRange * attackRange;
            if (target != null && !isAttacking && isInAttackRange)
            {
                StartCoroutine(Attack());
            }
            // else
            // {
            // Debug.LogWarning("Target-> " + target.name + " IsAttacking: " + isAttacking + " IsInAttackRange: " + isInAttackRange);
            // }
        }

        protected void UpdatePath()
        {
            if (seeker.IsDone())
            {
                seeker.StartPath(rb.position, target.position, OnPathComplete);
            }
        }

        protected void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                path = p;
                currentWaypoint = 0;
            }
        }


        // Update is called once per frame
        protected void FixedUpdate()
        {
            ProcessPath();
        }

        protected virtual void ProcessPath()
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

        protected virtual IEnumerator Attack()
        {
            Debug.LogError("Attack not implemented for " + transform.name);
            yield return null;
        }

        protected virtual IEnumerator Die()
        {
            Debug.LogError("Die not implemented for " + transform.name);
            yield return null;
        }

        public void ResetBehaviour()
        {
            if (target != null)
            {
                InvokeRepeating("UpdatePath", 0f, .5f);
            }
            heart.SetStats(maxHP);
            GetComponentInChildren<CircleCollider2D>().enabled = true;
        }
    }
}