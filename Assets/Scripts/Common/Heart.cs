﻿using System.Collections;
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
        private bool dieActionDone;

        public delegate void Death();
        public event Death deathEvent;

        public RectTransform greenHPTransform;

        private CharacterRenderer characterRenderer;
        private Vector2 sizeDelta;
        private float maxHp;

        private void Awake()
        {
            characterRenderer = GetComponentInChildren<CharacterRenderer>();
            character = GetComponent<Character>();
        }

        private void Start()
        {
            sizeDelta = greenHPTransform.sizeDelta;
        }

        public void SetStats(float hp)
        {
            dieActionDone = false;
            maxHp = hp;
            this.hp = hp;
            if (sizeDelta == Vector2.zero)
            {
                sizeDelta = greenHPTransform.sizeDelta;
            }
            greenHPTransform.sizeDelta = sizeDelta;
        }

        public void Damage(float damage)
        {
            hp = Mathf.Clamp(hp - damage, 0, maxHp);
            float percent = hp / maxHp;
            greenHPTransform.sizeDelta = new Vector2(sizeDelta.x * percent, sizeDelta.y);
            if (hp <= 0 && !dieActionDone)
            {
                // fire death event
                if (deathEvent != null)
                {
                    deathEvent();
                    dieActionDone = true;
                }
            }
        }
    }
}