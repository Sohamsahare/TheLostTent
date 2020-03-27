using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // TODO: add configurable shake variables
    public AnimationCurve shakeCurve;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float dampingSpeed = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.7f;
    private bool isShaking;
    private Vector3 initialPosition;

    public void ShakeOnce()
    {
        if (!isShaking)
        {
            StartCoroutine(Shake());
        }
    }


    IEnumerator Shake()
    {
        isShaking = true;
        initialPosition = transform.position;
        float journey = 0;
        while (journey <= shakeDuration)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            journey += Time.deltaTime * dampingSpeed;
            yield return null;
        }
        transform.localPosition = initialPosition;
        isShaking = false;
    }
}

// Vector2 start = transform.position;
// Vector2 end = start + Random.insideUnitCircle * shakeMagnitude;
// float journey = 0;
// while (journey <= shakeDuration)
// {
//     journey += Time.deltaTime;
//     float percent = Mathf.Clamp01(journey / shakeDuration);
//     if (shakeCurve != null)
//     {
//         percent = shakeCurve.Evaluate(percent);
//     }
//     // transform.position = Vector2(start, )
//     transform.position = Vector2.Lerp(start, end, percent);
// }
// transform.position = start;