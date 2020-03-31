using UnityEngine;
using Pathfinding;
using System.Collections;
namespace TheLostTent
{
    public class Enemy : Character
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

        public virtual void Damage(float damage)
        {
            Debug.LogWarning("Not implemented");
        }

        protected new void Awake()
        {
            base.Awake();
            pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
            seeker = GetComponent<Seeker>();
            motor = GetComponent<CharacterMotor>();
        }

        protected void Update()
        {

            if (!isAttacking && Vector2.Distance(transform.position, target.position) <= attackRange)
            {
                StartCoroutine(Attack());
            }
        }

        protected void UpdatePath()
        {
            if (seeker.IsDone())
                seeker.StartPath(rb.position, target.position, OnPathComplete);
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
            Debug.LogError("ProcessPath not implemented for " + transform.name);
        }

        protected virtual IEnumerator Attack()
        {
            Debug.LogError("Attack not implemented for " + transform.name);
            yield return null;
        }

    }
}