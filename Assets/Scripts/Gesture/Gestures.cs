using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheLostTent
{
    public class Gestures : MonoBehaviour
    {
        public float movementGestureRatio = 10f;
        public float dashGestureRatio = 5f;
        public float shootThresholdMagnitude = 2f;
        public float maxGestureRecordTime = .5f;
        public TextMeshProUGUI tutorialTMP;
        public Transform joystickKnobTransform;
        public Transform dashTrailTransform;
        public float maxKnobRange = 1f;
        public string[] tutorialTexts;
        float minMovementThreshold;
        float minDashThreshold;
        bool hasShot;
        List<Touch> currentTouches;
        bool dashTriggered;
        TouchData movementData;
        TouchData dashData;
        Transform playerTransform;
        Camera mainCamera;
        Vector3 knobStartPosition;

        float startTime;
        int lastTouchCount = 0;


        private void Start()
        {
            lastTouchCount = 0;
            joystickKnobTransform.parent.gameObject.SetActive(false);
            UpdateTutorialMessage();
            mainCamera = Camera.main;
            currentTouches = new List<Touch>();
            minMovementThreshold = Mathf.Min(Screen.height, Screen.width) / movementGestureRatio;
            minDashThreshold = Mathf.Min(Screen.height, Screen.width) / dashGestureRatio;
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }


        private void Update()
        {
            var currentTouchCount = Input.touchCount;
            if (lastTouchCount != currentTouchCount)
            {
                UpdateTutorialMessage();
                lastTouchCount = currentTouchCount;
            }

            // do movement during the first touch
            if (currentTouchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                // do movement
                DoMovement(touch);
            }
            // do dash/shoot after first touch
            else if (currentTouchCount == 2)
            {
                Touch touch = Input.GetTouch(1);
                ListenForDashAndShoot(touch);
            }

#if UNITY_EDITOR
            // MOUSE SECTION
            if (Input.GetMouseButtonDown(0))
            {
                movementData = new TouchData();
                startTime = Time.time;
                dashTriggered = false;
                movementData.startPosition = Input.mousePosition;
                // dashTrailTransform.gameObject.SetActive(true);
                Vector3 trailPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                trailPosition.z = 0;
                dashTrailTransform.position = trailPosition;
            }
            else if (Input.GetMouseButton(0))
            {
                movementData.lastPosition = Input.mousePosition;
                Vector3 trailPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                trailPosition.z = 0;
                dashTrailTransform.position = trailPosition;
                // get direction relative to start point
                var direction = movementData.direction;
                // read input and do movement if above threshold
                if (!dashTriggered && Time.time - startTime <= maxGestureRecordTime && direction.magnitude >= minMovementThreshold)
                {
                    direction = direction.normalized;
                    Debug.DrawRay(playerTransform.position, direction, Color.magenta);
                    Input.SetAxisMobile("D_Horizontal", direction.x);
                    Input.SetAxisMobile("D_Vertical", direction.y);
                    dashTriggered = true;
                }
                else
                {
                    Input.SetAxisMobile("D_Horizontal", 0);
                    Input.SetAxisMobile("D_Vertical", 0);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                movementData.lastPosition = Input.mousePosition;
                dashTriggered = false;
                // dashTrailTransform.gameObject.SetActive(false);
                // Input.PressButtonUpMobile("Dash");
                Input.SetAxisMobile("D_Horizontal", 0);
                Input.SetAxisMobile("D_Vertical", 0);
            }

            // else no movement is happenning

#endif
        }

        private void UpdateTutorialMessage()
        {
            switch (Input.touchCount)
            {
                case 0:
                    tutorialTMP.text = tutorialTexts[0];
                    break;
                case 1:
                    tutorialTMP.text = tutorialTexts[1];
                    break;
                default:
                    break;
            }
        }

        private void ListenForDashAndShoot(Touch touch)
        {
            Vector3 trailPosition;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    dashData = new TouchData();
                    hasShot = false;
                    startTime = Time.time;
                    dashData.startPosition = touch.position;
                    dashTriggered = false;
                    trailPosition = mainCamera.ScreenToWorldPoint(touch.position);
                    trailPosition.z = 0;
                    dashTrailTransform.position = trailPosition;
                    break;
                case TouchPhase.Moved:
                    dashData.lastPosition = touch.position;
                    trailPosition = mainCamera.ScreenToWorldPoint(touch.position);
                    trailPosition.z = 0;
                    dashTrailTransform.position = trailPosition;
                    // get direction relative to start point
                    var direction = dashData.direction;
                    // read input and do movement if above threshold
                    if (!dashTriggered && Time.time - startTime <= maxGestureRecordTime && direction.magnitude >= minMovementThreshold)
                    {
                        direction = direction.normalized;
                        // Debug.DrawRay(playerTransform.position, direction, Color.cyan, 1.5f);
                        Input.PressButtonDownMobile("Dash");
                        Input.SetAxisMobile("D_Horizontal", direction.x);
                        Input.SetAxisMobile("D_Vertical", direction.y);
                        dashTriggered = true;
                    }
                    else
                    {
                        Input.SetAxisMobile("D_Horizontal", 0);
                        Input.SetAxisMobile("D_Vertical", 0);
                    }
                    break;
                case TouchPhase.Ended:
                    dashData.lastPosition = touch.position;
                    dashTriggered = false;
                    Input.PressButtonUpMobile("Dash");
                    Input.SetAxisMobile("D_Horizontal", 0);
                    Input.SetAxisMobile("D_Vertical", 0);
                    // dashTrailTransform.gameObject.SetActive(false);
                    break;
                case TouchPhase.Stationary:
                    dashData.lastPosition = touch.position;
                    // shoot now
                    Vector2 relativeMovement = dashData.startPosition - dashData.lastPosition;
                    if (!hasShot && relativeMovement.magnitude <= shootThresholdMagnitude)
                    {
                        Input.PressButtonDownMobile("Shoot");
                        hasShot = true;
                    }
                    else
                    {
                        Debug.Log("sHOOT WILL NOT HAPPEN AS MOVEMENT WAS TOO FAR!!");
                    }
                    break;
                default:
                    Debug.Log("Invalid Gesture type");
                    break;
            }
        }

        private void DoMovement(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    movementData = new TouchData();
                    movementData.startPosition = touch.position;
                    // spawn handle and knob
                    joystickKnobTransform.parent.position = touch.position;
                    joystickKnobTransform.parent.gameObject.SetActive(true);
                    knobStartPosition = joystickKnobTransform.localPosition;
                    break;
                case TouchPhase.Moved:
                    movementData.lastPosition = touch.position;
                    // get direction relative to start point
                    Vector2 direction = movementData.direction;
                    // read input and do movement if above threshold
                    if (direction.magnitude >= minMovementThreshold)
                    {
                        direction = direction.normalized;
                        // Debug.DrawRay(playerTransform.position, direction, Color.magenta);
                        Input.SetAxisMobile("Horizontal", direction.x);
                        Input.SetAxisMobile("Vertical", direction.y);
                        // move knob
                        joystickKnobTransform.localPosition = (Vector2)knobStartPosition + direction * maxKnobRange;
                    }
                    break;
                case TouchPhase.Ended:
                    movementData.lastPosition = touch.position;
                    Input.SetAxisMobile("Horizontal", 0);
                    Input.SetAxisMobile("Vertical", 0);
                    joystickKnobTransform.localPosition = knobStartPosition;
                    joystickKnobTransform.parent.gameObject.SetActive(false);
                    // hide handle and knob
                    break;
                default:
                    Debug.Log("Invalid Gesture type");
                    break;
            }
        }
    }
}