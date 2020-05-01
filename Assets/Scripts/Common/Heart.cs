using System.Collections;
using UnityEngine;
namespace TheLostTent
{
    [RequireComponent(typeof(Character))]
    public class Heart : MonoBehaviour
    {
        private Character character;
        public float HP
        {
            get
            {
                if (character == null)
                {
                    character = GetComponent<Character>();
                }
                return character.HP;
            }
        }

        private float hp
        {
            get
            {
                return HP;
            }
            set
            {
                if (character == null)
                {
                    character = GetComponent<Character>();
                }
                character.HP = value;
            }
        }

        public bool IsDead
        {
            get
            {
                return (hp <= 0);
            }
        }
        private bool _dieAction;
        private bool dieActionDone
        {
            get
            {
                return _dieAction;
            }
            set
            {
                // if (transform.tag == "Player")
                // {
                //     Debug.Log($"Die action for {transform.name} is set to {value}");
                // }
                _dieAction = value;
            }
        }

        public delegate void Death();
        public event Death deathEvent;

        public RectTransform greenHPTransform;

        private CharacterRenderer characterRenderer;
        private Vector2 initValue;
        private float maxHp;

        private void Awake()
        {
            characterRenderer = GetComponentInChildren<CharacterRenderer>();
            character = GetComponent<Character>();
        }

        private void Start()
        {
            initValue = greenHPTransform.sizeDelta;
        }

        public void SetStats(float hp)
        {
            dieActionDone = false;
            maxHp = hp;
            this.hp = hp;
            if (initValue == Vector2.zero)
            {
                initValue = greenHPTransform.sizeDelta;
            }
            greenHPTransform.sizeDelta = initValue;
            // if (transform.tag == "Player")
            // {
            //     Debug.Log($"{transform.name}'s heart set to {hp}");
            // }
        }

        public void Damage(float damage)
        {
            hp = Mathf.Clamp(hp - damage, 0, maxHp);
            float percent = hp / maxHp;
            greenHPTransform.sizeDelta = new Vector2(initValue.x * percent, initValue.y);
            if (hp <= 0 && !dieActionDone)
            {
                // fire death event
                if (deathEvent != null)
                {
                    dieActionDone = true;
                    deathEvent();
                }
                else
                {
                    Debug.Log("Deathevent is null for " + transform.name);
                }
            }
        }
    }
}