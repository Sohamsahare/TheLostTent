using UnityEngine;

namespace TheLostTent
{
    public class Character : MonoBehaviour
    {

        public float maxHP { get; private set; }
        public float HP = 100f;
        protected Rigidbody2D rb;
        protected CharacterRenderer isoRenderer;
        protected void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            isoRenderer = GetComponentInChildren<CharacterRenderer>();
            maxHP = HP;
        }

        private void Start()
        {
            Debug.Log("Setting HP to " + maxHP + "for " + transform.name);
        }

        public Character()
        {

        }
    }
}