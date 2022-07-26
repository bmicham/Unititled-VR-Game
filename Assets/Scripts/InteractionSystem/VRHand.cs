using System.Collections;
using UnityEngine;
using Valve.VR;

namespace KP
{
    public class VRHand : MonoBehaviour
    {
        //public GameObject Display_Controller;
        public VRHand otherHand;
        public LayerMask GrabLayer;
        public GameObject model;
        public Rigidbody physicalHand;
        public Transform playerRig;
        public Transform palmPosition;
        public float interactorSphereRadius;
        public BoxCollider m_handCollider;
        public Vector3 m_handColliderSize;
        public float m_handColliderRadius = 0.5f;
        [Header("Input")]
        public bool IsRightHand;
        public SteamVR_Input_Sources HandSource = SteamVR_Input_Sources.LeftHand;
        public HandInputs input;
        public SteamVR_Action_Boolean Trigger_Button;
        public SteamVR_Action_Boolean Trigger_Touch;
        public SteamVR_Action_Boolean Primary2Axis_Button;
        public SteamVR_Action_Boolean Primary2Axis_Touch;
        public SteamVR_Action_Boolean Secondary2Axis_Button;
        public SteamVR_Action_Boolean Secondary2Axis_Touch;
        public SteamVR_Action_Boolean A_Button;
        public SteamVR_Action_Boolean A_Button_Touch;
        public SteamVR_Action_Boolean B_Button;
        public SteamVR_Action_Boolean B_Button_Touch;
        public SteamVR_Action_Boolean Grip_Button;
        public SteamVR_Action_Boolean Grip_Touch;
        public SteamVR_Action_Single Trigger_Axis;
        public SteamVR_Action_Single Grip_Squeeze;
        public SteamVR_Action_Single Thumb_Squeeze;
        public SteamVR_Action_Vector2 Primary2Axis_Axes;
        public SteamVR_Action_Vector2 Secondary2Axis_Axes;
        public SteamVR_Action_Pose Pose;
        public SteamVR_Action_Vibration Vibration;
        public SteamVR_Action_Skeleton Skeleton;

        private GrabPoint currentGrabPoint;
        private GrabPoint hoveredGrabPoint;

        private float m_timeSinceGripButtonDown;
        private readonly Collider[] _colliders = new Collider[100];
        public HandState currentHandState = HandState.Free;

        public void Awake()
        {
            input.hand = this;
            m_handColliderSize = m_handCollider.size;
        }

        private void FixedUpdate()
        {
            if (!Skeleton.deviceIsConnected)
                return;

            if (input.TriggerDown && currentHandState == HandState.Free)
            {
                var hits = Physics.OverlapSphereNonAlloc(palmPosition.position, interactorSphereRadius, _colliders, GrabLayer, QueryTriggerInteraction.Ignore);
                if (hits > 0)
                {
                    var hit = _colliders[0];

                    if (!hit.attachedRigidbody)
                    {
                        for (var index = 0; index < hits; index++)
                        {
                            var col = _colliders[index];
                            if (!col)
                                break;
                            if (col.attachedRigidbody)
                            {
                                hit = col;
                                break;
                            }
                        }
                    }


                    if (hit.GetComponent<GrabPoint>()) 
                    {
                        m_handCollider.size = new Vector3(0.01f, 0.01f, 0.01f);
                        currentGrabPoint = hit.GetComponent<GrabPoint>();
                        currentGrabPoint.BeginGrab(this);
                        currentHandState = HandState.Interacting;
                        model.SetActive(false);
                    }
                }
            }
            else if (input.TriggerDown && currentHandState == HandState.Interacting)
            {
                currentGrabPoint.EndGrab(this);
                currentHandState = HandState.Free;
                currentGrabPoint = null;
                m_handCollider.size = m_handColliderSize;
                model.SetActive(true);
            }

            SetInputs();
        }

        public void EndCurrentGrab(GrabPoint grab) 
        {
            if (grab != currentGrabPoint)
                return;
            currentGrabPoint.DestroyJoint(this);
            currentGrabPoint = null;
            currentHandState = HandState.Free;
            Debug.Log("<color=blue>Interaction:</color> Switching hands!");
        }

        private void SetInputs() 
        {
            input.TriggerUp = Trigger_Button.GetStateUp(HandSource);
            input.TriggerDown = Trigger_Button.GetStateDown(HandSource);
            input.TriggerPressed = Trigger_Button.GetState(HandSource);
            input.TriggerFloat = Trigger_Axis.GetAxis(HandSource);
            input.TriggerTouchUp = Trigger_Touch.GetStateUp(HandSource);
            input.TriggerTouchDown = Trigger_Touch.GetStateDown(HandSource);
            input.TriggerTouched = Trigger_Touch.GetState(HandSource);
            input.GripUp = Grip_Button.GetStateUp(HandSource);
            input.GripDown = Grip_Button.GetStateDown(HandSource);
            input.GripPressed = Grip_Button.GetState(HandSource);
            input.GripTouchUp = Grip_Touch.GetStateUp(HandSource);
            input.GripTouchDown = Grip_Touch.GetStateDown(HandSource);
            input.GripTouched = Grip_Touch.GetState(HandSource);
            input.TouchpadUp = Primary2Axis_Button.GetStateUp(HandSource);
            input.TouchpadDown = Primary2Axis_Button.GetStateDown(HandSource);
            input.TouchpadPressed = Primary2Axis_Button.GetState(HandSource);
            input.TouchpadTouchUp = Primary2Axis_Touch.GetStateUp(HandSource);
            input.TouchpadTouchDown = Primary2Axis_Touch.GetStateDown(HandSource);
            input.TouchpadTouched = Primary2Axis_Touch.GetState(HandSource);
            input.TouchpadAxes = Primary2Axis_Axes.GetAxis(HandSource);
            input.TouchpadNorthDown = false;
            input.TouchpadSouthDown = false;
            input.TouchpadWestDown = false;
            input.TouchpadEastDown = false;
            input.TouchpadNorthUp = false;
            input.TouchpadSouthUp = false;
            input.TouchpadWestUp = false;
            input.TouchpadEastUp = false;
            if (input.TouchpadAxes.magnitude < 0.5)
            {
                if (!input.TouchpadCenterPressed)
                    input.TouchpadCenterDown = true;
                if (input.TouchpadNorthPressed)
                    input.TouchpadNorthUp = true;
                if (input.TouchpadSouthPressed)
                    input.TouchpadSouthUp = true;
                if (input.TouchpadWestPressed)
                    input.TouchpadWestUp = true;
                if (input.TouchpadEastPressed)
                    input.TouchpadEastUp = true;
                input.TouchpadNorthPressed = false;
                input.TouchpadSouthPressed = false;
                input.TouchpadWestPressed = false;
                input.TouchpadEastPressed = false;
                input.TouchpadCenterPressed = true;
            }
            else
            {
                if (input.TouchpadCenterPressed)
                    input.TouchpadCenterUp = true;
                input.TouchpadCenterPressed = false;
                if (Vector2.Angle(input.TouchpadAxes, Vector2.up) <= 45.0)
                {
                    if (!input.TouchpadNorthPressed)
                        input.TouchpadNorthDown = true;
                    input.TouchpadNorthPressed = true;
                    input.TouchpadSouthPressed = false;
                    input.TouchpadWestPressed = false;
                    input.TouchpadEastPressed = false;
                }
                else if (Vector2.Angle(input.TouchpadAxes, Vector2.down) <= 45.0)
                {
                    if (!input.TouchpadSouthPressed)
                        input.TouchpadSouthDown = true;
                    input.TouchpadSouthPressed = true;
                    input.TouchpadNorthPressed = false;
                    input.TouchpadWestPressed = false;
                    input.TouchpadEastPressed = false;
                }
                else if (Vector2.Angle(input.TouchpadAxes, Vector2.left) <= 45.0)
                {
                    if (!input.TouchpadWestPressed)
                        input.TouchpadWestDown = true;
                    input.TouchpadWestPressed = true;
                    input.TouchpadNorthPressed = false;
                    input.TouchpadSouthPressed = false;
                    input.TouchpadEastPressed = false;
                }
                else if (Vector2.Angle(input.TouchpadAxes, Vector2.right) <= 45.0)
                {
                    if (!input.TouchpadEastPressed)
                        input.TouchpadEastDown = true;
                    input.TouchpadEastPressed = true;
                    input.TouchpadNorthPressed = false;
                    input.TouchpadSouthPressed = false;
                    input.TouchpadWestPressed = false;
                }
            }
            input.BButtonUp = B_Button.GetStateUp(HandSource);
            input.BButtonDown = B_Button.GetStateDown(HandSource);
            input.BButtonPressed = B_Button.GetState(HandSource);
            input.BButtonTouched = A_Button.GetState(HandSource);
            input.AButtonUp = A_Button.GetStateUp(HandSource);
            input.AButtonDown = A_Button.GetStateDown(HandSource);
            input.AButtonPressed = A_Button.GetState(HandSource);
            input.AButtonTouched = A_Button.GetState(HandSource);
            input.Secondary2AxisInputUp = Secondary2Axis_Button.GetStateUp(HandSource);
            input.Secondary2AxisInputDown = Secondary2Axis_Button.GetStateDown(HandSource);
            input.Secondary2AxisInputPressed = Secondary2Axis_Button.GetState(HandSource);
            input.Secondary2AxisInputTouchUp = Secondary2Axis_Touch.GetStateUp(HandSource);
            input.Secondary2AxisInputTouchDown = Secondary2Axis_Touch.GetStateDown(HandSource);
            input.Secondary2AxisInputTouched = Secondary2Axis_Touch.GetState(HandSource);
            input.Secondary2AxisInputAxes = Secondary2Axis_Axes.GetAxis(HandSource);
            input.Secondary2AxisNorthDown = false;
            input.Secondary2AxisSouthDown = false;
            input.Secondary2AxisWestDown = false;
            input.Secondary2AxisEastDown = false;
            input.Secondary2AxisNorthUp = false;
            input.Secondary2AxisSouthUp = false;
            input.Secondary2AxisWestUp = false;
            input.Secondary2AxisEastUp = false;
            if (input.Secondary2AxisInputAxes.magnitude < 0.5)
            {
                if (!input.Secondary2AxisCenterPressed)
                    input.Secondary2AxisCenterDown = true;
                if (input.Secondary2AxisNorthPressed)
                    input.Secondary2AxisNorthUp = true;
                if (input.Secondary2AxisSouthPressed)
                    input.Secondary2AxisSouthUp = true;
                if (input.Secondary2AxisWestPressed)
                    input.Secondary2AxisWestUp = true;
                if (input.Secondary2AxisEastPressed)
                    input.Secondary2AxisEastUp = true;
                input.Secondary2AxisNorthPressed = false;
                input.Secondary2AxisSouthPressed = false;
                input.Secondary2AxisWestPressed = false;
                input.Secondary2AxisEastPressed = false;
                input.Secondary2AxisCenterPressed = true;
            }
            else
            {
                if (input.Secondary2AxisCenterPressed)
                    input.Secondary2AxisCenterUp = true;
                input.Secondary2AxisCenterPressed = false;
                if (Vector2.Angle(input.Secondary2AxisInputAxes, Vector2.up) <= 45.0)
                {
                    if (!input.Secondary2AxisNorthPressed)
                        input.Secondary2AxisNorthDown = true;
                    input.Secondary2AxisNorthPressed = true;
                    input.Secondary2AxisSouthPressed = false;
                    input.Secondary2AxisWestPressed = false;
                    input.Secondary2AxisEastPressed = false;
                }
                else if (Vector2.Angle(input.Secondary2AxisInputAxes, Vector2.down) <= 45.0)
                {
                    if (!input.Secondary2AxisSouthPressed)
                        input.Secondary2AxisSouthDown = true;
                    input.Secondary2AxisSouthPressed = true;
                    input.Secondary2AxisNorthPressed = false;
                    input.Secondary2AxisWestPressed = false;
                    input.Secondary2AxisEastPressed = false;
                }
                else if (Vector2.Angle(input.Secondary2AxisInputAxes, Vector2.left) <= 45.0)
                {
                    if (!input.Secondary2AxisWestPressed)
                        input.Secondary2AxisWestDown = true;
                    input.Secondary2AxisWestPressed = true;
                    input.Secondary2AxisNorthPressed = false;
                    input.Secondary2AxisSouthPressed = false;
                    input.Secondary2AxisEastPressed = false;
                }
                else if (Vector2.Angle(input.Secondary2AxisInputAxes, Vector2.right) <= 45.0)
                {
                    if (!input.Secondary2AxisEastPressed)
                        input.Secondary2AxisEastDown = true;
                    input.Secondary2AxisEastPressed = true;
                    input.Secondary2AxisNorthPressed = false;
                    input.Secondary2AxisSouthPressed = false;
                    input.Secondary2AxisWestPressed = false;
                }
            }
            input.FingerCurl_Thumb = Skeleton.fingerCurls[0];
            input.FingerCurl_Index = Skeleton.fingerCurls[1];
            input.FingerCurl_Middle = Skeleton.fingerCurls[2];
            input.FingerCurl_Ring = Skeleton.fingerCurls[3];
            input.FingerCurl_Pinky = Skeleton.fingerCurls[4];
            input.VelLinearLocal = Pose.GetVelocity(HandSource);
            input.VelAngularLocal = Pose.GetAngularVelocity(HandSource);
            input.VelLinearWorld = playerRig.TransformDirection(input.VelLinearLocal);
            input.VelAngularWorld = playerRig.TransformDirection(input.VelAngularLocal);
            bool flag = false;
            float num12 = (float)((input.FingerCurl_Middle * 2.0 + input.FingerCurl_Ring * 1.0) / 3.0);
            float num13 = num12 - input.LastCurlAverage;
            if (input.TriggerPressed)
                flag = true;
            else if (input.IsGrabbing && num12 >= 0.5)
                flag = true;
            else if (input.IsGrabbing && input.TriggerTouched)
                flag = true;
            else if (input.IsGrabbing && input.GripTouched)
                flag = true;
            else if (input.GripPressed)
                flag = true;
            if (input.IsGrabbing && !input.TriggerPressed && !input.GripPressed && (num13 < -0.300000011920929 && num12 < 0.699999988079071 || num13 < -0.5))
                flag = false;
            input.IsGrabUp = input.IsGrabbing && !flag;
            input.IsGrabDown = !input.IsGrabbing && flag || input.TriggerDown;
            input.IsGrabbing = flag;
            input.LastCurlAverage = num12;
            if (m_timeSinceGripButtonDown < 5.0)
                m_timeSinceGripButtonDown += Time.deltaTime;
        }

        public enum HandState 
        {
            Interacting,
            Free,
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(palmPosition.position, interactorSphereRadius);
        }
    }
}