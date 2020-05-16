using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLostTent
{
    public class Witch : Character
    {
        [Tooltip("damage potential of an attack projectile")]
        public float attackPower = 33f;
        [Tooltip("speed at which an attack projectile is fired")]
        public float projectileSpeed = 100f;
        [Tooltip("number of attacks per second")]
        public float attackSpeed = 4;
        [Tooltip("min joystick movement required to enable shooting")]
        public float minShootThreshold = .2f;
        public float dashDistance = 5f;
        public float dashDamageMultiplier = 2f;
        public float dashCooldown = 1f;
        public float dashDuration = .5f;
        public GameObject fireballPrefab;
        public CameraShake cameraShake;
        private new CircleCollider2D collider;
        private BoxCollider2D trigger;
        private bool isAttacking;
        private bool isDashing;
        private Heart heart;
        CharacterMotor motor;
        private TrailRenderer dashTrail;
        private Pooler pooler;
        private LevelManager levelManager;
        private CinemachineImpulseSource impulseSource;
        public float impulseMagnitude = 1;
        private Vector3 lastMovement;

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
            // dash in the direction of movement
            Vector3 dashDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

            bool dashCondition = Input.GetButtonDownMobile("Dash");
            // if aim stick has been moved above the threshold value, 
            // then enable shooting
            bool shootCondition = Mathf.Max(
                Mathf.Abs(Input.GetAxis("Aim_Horizontal")),
                Mathf.Abs(Input.GetAxis("Aim_Vertical"))
                ) > minShootThreshold;

            if (dashCondition)
            {
                Dash(dashDirection);
            }

            if (shootCondition)
            {
                Attack();
            }
        }

        private void FixedUpdate()
        {
            Vector2 movement;
            movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            motor.Move(movement);
            isoRenderer.SetDirection(movement);
            if (movement != Vector2.zero)
            {
                lastMovement = movement;
            }
        }

        // todo: add modifiers and limit to attack potential
        public void Attack()
        {
            if (!isAttacking)
            {
                StartCoroutine(DoAttack());
            }
        }

        private IEnumerator DoAttack()
        {
            isAttacking = true;
            // for dual stick input
            Vector2 direction = new Vector2(Input.GetAxis("Aim_Horizontal"), Input.GetAxis("Aim_Vertical"));
            Debug.DrawRay(transform.position, direction, Color.red, 1);
            GameObject obj = pooler.RetrieveFromPool(Constants.PoolTags.Fireball, transform.position, Vector3Int.zero, transform);
            obj.GetComponent<Fireball>().damage = attackPower;
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            // rb.AddForce(
            //     (direction * projectileSpeed)
            // );
            rb.velocity = (direction * projectileSpeed);
            pooler.ReturnToPool(Constants.PoolTags.Fireball, obj, 3f);
            yield return new WaitForSeconds(1 / attackSpeed);
            isAttacking = false;
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
            if (dashDirection == Vector3.zero)
            {
                dashDirection = lastMovement.normalized;
            }
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
                // strong vibrate when player dies
                Vibration.VibratePop();
                velocity *= 3;
            }
            else
            {
                // vibrate when player recieves damage
                Vibration.VibratePeek();
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