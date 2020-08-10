using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLostTent
{
    public class Pickable : MonoBehaviour
    {
        public string pickerTag = Constants.Tags.Player;
        public AnimationCurve wobbleCurve;
        public float wobbleOffsetDuration = .5f;
        private Vector2 startPosition;
        private bool isWobbling = false;
        public float wobbleDuration = 2;

        protected virtual void Start()
        {
            if (wobbleCurve.keys.Length == 0)
            {
                wobbleCurve = new AnimationCurve(
                                new Keyframe(0, 0),
                                new Keyframe(1, 1)
                            );
                Debug.Log("wobble curve points -> " + wobbleCurve.keys.Length);
            }
            startPosition = transform.position;
        }

        protected virtual void OnPickup(GameObject pickerObj = null)
        {
            Debug.Log(transform.name + " picked up by " + pickerObj.transform.name);
            Debug.Log("OnPickup Not Implemeneted for item -> " + transform.name);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == pickerTag)
            {
                OnPickup(other.gameObject);
                // TODO: add to pool
                Destroy(gameObject);
            }
        }

        protected virtual void Update()
        {
            // wobble effect
            if (!isWobbling)
            {
                StartCoroutine(DoWobble());
            }
        }

        protected virtual IEnumerator DoWobble()
        {
            isWobbling = true;
            startPosition = transform.position;
            Vector2 start = startPosition;
            Vector2 end = start + Random.insideUnitCircle * wobbleOffset;
            float halfWobbleDuration = wobbleDuration / 2;
            float journey = 0;
            while (journey <= halfWobbleDuration)
            {
                journey += Time.deltaTime;
                float percent = Mathf.Clamp01(journey / halfWobbleDuration);
                if (wobbleCurve != null)
                {
                    percent = wobbleCurve.Evaluate(percent);
                }
                // transform.position = Vector2(start, )
                transform.position = Vector2.Lerp(start, end, percent);
                yield return null;
            }
            start = end;
            end = startPosition;
            journey = 0;
            while (journey <= halfWobbleDuration)
            {
                journey += Time.deltaTime;
                float percent = Mathf.Clamp01(journey / halfWobbleDuration);
                if (wobbleCurve != null)
                {
                    percent = wobbleCurve.Evaluate(percent);
                }
                // transform.position = Vector2(start, )
                transform.position = Vector2.Lerp(start, end, percent);
                yield return null;
            }
            isWobbling = false;
        }
    }
}