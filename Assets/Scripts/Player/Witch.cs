﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLostTent
{
    public class Witch : Character
    {
        public float attackPower = 33f;
        public float attackSpeed = 100f;
        public float dashDistance = 5f;
        public float dashCooldown = 1f;
        public float dashDuration = .5f;
        public GameObject fireballPrefab;
        public CameraShake cameraShake;
        private new CircleCollider2D collider;
        private bool isDashing;
        private Heart heart;
        CharacterMotor motor;

        private new void Awake()
        {
            base.Awake();
            collider = GetComponentInChildren<CircleCollider2D>();
            heart = GetComponent<Heart>();
            motor = GetComponent<CharacterMotor>();
        }

        private void Start()
        {
            heart.SetStats(HP);
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
            var obj = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            obj.GetComponent<Fireball>().damage = attackPower;
            var rb = obj.GetComponent<Rigidbody2D>();
            // var direction = DirectionFromString(isoRenderer.CurrentDirection);
            var direction = GetNonMovementTouch();
            // Debug.Log(direction);
            rb.AddForce(
                (direction * attackSpeed)
            );
            Destroy(obj, 3f);
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
            Vector3 start = transform.position;
            Vector3 end = transform.position + dashDirection * dashDistance;
            isDashing = true;
            collider.enabled = false;
            float journey = 0;
            while (journey <= dashDuration)
            {
                journey += Time.deltaTime;
                float percent = journey / dashDuration;
                transform.position = Vector3.Lerp(start, end, percent);
                yield return null;
            }
            collider.enabled = true;
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
            cameraShake.ShakeOnce();
            if (heart.IsDead)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public void FaceOnClick()
        {

        }

        public void DetermineDirection()
        {

        }
    }
}