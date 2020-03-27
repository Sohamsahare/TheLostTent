using UnityEngine;

namespace TheLostTent
{
    public class Character : MonoBehaviour
    {
        public float HP = 100f;
        protected Rigidbody2D rb;
        protected CharacterRenderer isoRenderer;
        protected void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            isoRenderer = GetComponentInChildren<CharacterRenderer>();
        }
        public Character()
        {
        }
    }
}