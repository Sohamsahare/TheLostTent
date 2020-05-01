using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLostTent
{
    public class Witch : Character
    {
        public float attackPower = 33f;
        public float attackSpeed = 100f;
        public float dashDistance = 5f;
        public float dashDamageMultiplier = 2f;
        public float dashCooldown = 1f;
        public float dashDuration = .5f;
        public GameObject fireballPrefab;
        public CameraShake cameraShake;
        private new CircleCollider2D collider;
        private BoxCollider2D trigger;
        private bool isDashing;
        private Heart heart;
        CharacterMotor motor;
        private TrailRenderer dashTrail;
        private Pooler pooler;
        private LevelManager levelManager;
        private CinemachineImpulseSource impulseSource;
        public float impulseMagnitude = 1;

        private new void Awake()
        {
            base.Awake();
            levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
            pooler = GameObject.FindGameObjectWithTag("Pooler").GetComponent<Pooler>();
            collider = GetComponent<CircleCollider2D>();
            trigger = GetComponent<BoxCollider2D>();
            heart = GetComponent<Heart>();
            motor = GetComponent<CharacterMotor>();
            dashTrail = GetComponentInChildren<TrailRenderer>();
            impulseSource = GetComponent<CinemachineImpulseSource>();
            dashTrail.enabled = false;
        }

        private void Start()
        {
            heart.SetStats(maxHP);
            isDashing = false;
        }

        private void Update()
        {
            Vector3 dashDirection = new Vector3(Input.GetAxisMobile("D_Horizontal"), Input.GetAxisMobile("D_Vertical"), 0f);
            bool dashCondition = false;
            bool shootCondition = false;
#if UNITY_EDITOR
            dashCondition = !!(dashDirection != Vector3.zero);
#elif UNITY_ANDROID
            dashCondition = Input.GetButtonDownMobile("Dash");
#endif
#if UNITY_EDITOR
            shootCondition = Input.GetButtonDown("Fire1");
#elif UNITY_ANDROID
            shootCondition = Input.GetButtonDownMobile("Shoot");
#endif
            if (dashCondition)
            {
                Dash(dashDirection);
            }
            // pressing the fire button
            else if (shootCondition)
            {
                Attack();
            }
        }

        private void FixedUpdate()
        {
            Vector2 movement;
#if UNITY_EDITOR
            movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#elif UNITY_ANDROID
            movement = new Vector2(Input.GetAxisMobile("Horizontal"), Input.GetAxisMobile("Vertical"));
#endif
            // Debug.Log("movement " + movement);1
            motor.Move(movement);
            isoRenderer.SetDirection(movement);
        }

        public void Attack()
        {
            var obj = pooler.RetrieveFromPool(Constants.PoolTags.Fireball, transform.position, Vector3Int.zero, transform);
            obj.GetComponent<Fireball>().damage = attackPower;
            var rb = obj.GetComponent<Rigidbody2D>();
            // var direction = DirectionFromString(isoRenderer.CurrentDirection);
            var direction = GetNonMovementTouch();
            // Debug.Log(direction);
            rb.AddForce(
                (direction * attackSpeed)
            );
            pooler.ReturnToPool(Constants.PoolTags.Fireball, obj, 3f);
        }

        public void Dash(Vector3 dashDirection)
        {
            if (!isDashing)
            {
                StartCoroutine(DashNow(dashDirection));
            }
        }

        IEnumerator DashNow(Vector3 dashDirection)
        {
            dashTrail.enabled = true;
            Vector3 start = transform.position;
            Vector3 end = transform.position + dashDirection * dashDistance;
            isDashing = true;
            collider.enabled = false;
            trigger.enabled = true;
            float journey = 0;
            while (journey <= dashDuration)
            {
                journey += Time.deltaTime;
                float percent = journey / dashDuration;
                transform.position = Vector3.Lerp(start, end, percent);
                yield return null;
            }
            trigger.enabled = false;
            collider.enabled = true;
            dashTrail.enabled = false;
            yield return new WaitForSeconds(dashCooldown);
            isDashing = false;
        }

        Vector2 GetNonMovementTouch()
        {
            Vector2 dir = Vector2.zero;
            if (Input.touchCount == 1)
            {
                dir = Input.GetTouch(0).position - (Vector2)Camera.main.WorldToScreenPoint(transform.position);
            }
            else if (Input.touchCount > 1)
            {
                dir = Input.GetTouch(Input.touchCount - 1).position - (Vector2)Camera.main.WorldToScreenPoint(transform.position);
            }
            else
            {
                // means input from pc
                dir = Input.mousePosition - (Vector2)Camera.main.WorldToScreenPoint(transform.position);
                // dir = DirectionFromString(isoRenderer.CurrentDirection);

            }
            dir = dir.normalized;
            return dir;
        }

        Vector2 DirectionFromString(string directionString)
        {
            Vector2 direction = new Vector2();
            var trimmedString = directionString.Split(' ');
            switch (trimmedString[trimmedString.Length - 1])
            {
                case "N":
                    direction = Vector2.up;
                    break;
                case "S":
                    direction = Vector2.down;
                    break;
                case "E":
                    direction = Vector2.right;
                    break;
                case "W":
                    direction = Vector2.left;
                    break;
                case "NE":
                    direction = (Vector2.up + Vector2.right).normalized;
                    break;
                case "NW":
                    direction = (Vector2.up + Vector2.left).normalized;
                    break;
                case "SE":
                    direction = (Vector2.down + Vector2.right).normalized;
                    break;
                case "SW":
                    direction = (Vector2.down + Vector2.left).normalized;
                    break;
                default:
                    Debug.LogWarning("Invalid direction string");
                    break;

            }
            return direction;
        }

        public void TakeDamage(float damage)
        {
            heart.Damage(damage);
            Vector3 velocity = Random.insideUnitSphere * impulseMagnitude;
            // kill the z-channel to avoid going below the 2d plane 
            // where characters are spawned
            velocity.z = 0;
            // higher intensity if the player dies
            if (heart.IsDead)
            {
                velocity *= 3;
            }
            // create camera shake effect 
            impulseSource.GenerateImpulse(velocity);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Enemy")
            {
                other.GetComponentInParent<Heart>().Damage(attackPower * dashDamageMultiplier);
            }
        }

        public void ResetAt(Vector2 position)
        {
            Debug.Log($"Player reset hp back to {maxHP}");
            heart.SetStats(maxHP);
            isDashing = false;
            transform.position = position;
        }
    }
}