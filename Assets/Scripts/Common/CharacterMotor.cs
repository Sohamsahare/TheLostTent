using UnityEngine;
namespace TheLostTent
{
    public class CharacterMotor : MonoBehaviour
    {
        public float movementSpeed = 1f;
        new CharacterRenderer renderer;
        private Rigidbody2D rb;
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            renderer = GetComponentInChildren<CharacterRenderer>();
        }

        // call move in fixed update
        public void Move(float horizontalInput, float verticalInput)
        {
            Vector2 currentPos = rb.position;
            Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
            inputVector = Vector2.ClampMagnitude(inputVector, 1);
            Vector2 movement = inputVector * movementSpeed;
            Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }

        public void Move(Vector2 direction)
        {
            Move(direction.x, direction.y);
        }
    }
}