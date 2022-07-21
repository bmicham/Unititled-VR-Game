using UnityEngine;

namespace KP
{
    public class HandInputs: MonoBehaviour
    {
        public float CrouchRate;

        public VRHand hand;
        public bool TriggerUp;
        public bool TriggerDown;
        public bool TriggerPressed;
        public float TriggerFloat;
        public bool TriggerTouchUp;
        public bool TriggerTouchDown;
        public bool TriggerTouched;
        public bool GripUp;
        public bool GripDown;
        public bool GripPressed;
        public bool GripTouchUp;
        public bool GripTouchDown;
        public bool GripTouched;
        public bool TouchpadUp;
        public bool TouchpadDown;
        public bool TouchpadPressed;
        public bool TouchpadTouchUp;
        public bool TouchpadTouchDown;
        public bool TouchpadTouched;
        public Vector2 TouchpadAxes;
        public bool TouchpadNorthUp;
        public bool TouchpadNorthDown;
        public bool TouchpadNorthPressed;
        public bool TouchpadSouthUp;
        public bool TouchpadSouthDown;
        public bool TouchpadSouthPressed;
        public bool TouchpadWestUp;
        public bool TouchpadWestDown;
        public bool TouchpadWestPressed;
        public bool TouchpadEastUp;
        public bool TouchpadEastDown;
        public bool TouchpadEastPressed;
        public bool TouchpadCenterUp;
        public bool TouchpadCenterDown;
        public bool TouchpadCenterPressed;
        public bool BButtonUp;
        public bool BButtonDown;
        public bool BButtonPressed;
        public bool BButtonTouched;
        public bool AButtonUp;
        public bool AButtonDown;
        public bool AButtonPressed;
        public bool AButtonTouched;
        public bool Secondary2AxisInputUp;
        public bool Secondary2AxisInputDown;
        public bool Secondary2AxisInputPressed;
        public bool Secondary2AxisInputTouchUp;
        public bool Secondary2AxisInputTouchDown;
        public bool Secondary2AxisInputTouched;
        public Vector2 Secondary2AxisInputAxes;
        public bool Secondary2AxisNorthUp;
        public bool Secondary2AxisNorthDown;
        public bool Secondary2AxisNorthPressed;
        public bool Secondary2AxisSouthUp;
        public bool Secondary2AxisSouthDown;
        public bool Secondary2AxisSouthPressed;
        public bool Secondary2AxisWestUp;
        public bool Secondary2AxisWestDown;
        public bool Secondary2AxisWestPressed;
        public bool Secondary2AxisEastUp;
        public bool Secondary2AxisEastDown;
        public bool Secondary2AxisEastPressed;
        public bool Secondary2AxisCenterUp;
        public bool Secondary2AxisCenterDown;
        public bool Secondary2AxisCenterPressed;
        public float FingerCurl_Thumb;
        public float FingerCurl_Index;
        public float FingerCurl_Middle;
        public float FingerCurl_Ring;
        public float FingerCurl_Pinky;
        public float LastCurlAverage;
        private Vector3 m_pos;
        private Quaternion m_rot;
        private Vector3 m_up;
        private Vector3 m_right;
        private Vector3 m_forward;
        public Vector3 FilteredUp;
        public Vector3 FilteredRight;
        public Vector3 VelLinearLocal;
        public Vector3 VelAngularLocal;
        public Vector3 VelLinearWorld;
        public Vector3 VelAngularWorld;
        public bool IsGrabUp;
        public bool IsGrabDown;
        public bool IsGrabbing;

        internal PlayerInputStateMachine JumpState;
        internal PlayerInputStateMachine CrouchState;

        public Vector3 Pos
        {
            get
            {
                m_pos = this.hand.transform.position;
                return m_pos;
            }
            set => m_pos = value;
        }

        public Quaternion Rot
        {
            get
            {
                m_rot = this.hand.transform.rotation;
                return m_rot;
            }
            set => m_rot = value;
        }

        public Vector3 Up
        {
            get
            {
                m_up = this.hand.transform.up;
                return m_up;
            }
        }

        public Vector3 Right
        {
            get
            {
                m_right = this.hand.transform.right;
                return m_right;
            }
        }

        public Vector3 Forward
        {
            get
            {
                m_forward = this.hand.transform.forward;
                return m_forward;
            }
        }

        protected virtual void Update()
        {
            SetStates();
        }

        protected virtual void SetStates()
        {
            ResetState(ref CrouchState);
            ResetState(ref JumpState);

            SetState(ref JumpState, AButtonDown);
            SetState(ref CrouchState, Mathf.Abs(CrouchRate) > .01f);
        }

        public void ResetState(ref PlayerInputStateMachine buttonState)
        {
            buttonState.JustDeactivated = false;
            buttonState.JustActivated = false;
            buttonState.Value = 0f;
        }

        public void SetState(ref PlayerInputStateMachine buttonState, bool pressed)
        {
            if (pressed)
            {
                if (!buttonState.Active)
                {
                    buttonState.JustActivated = true;
                    buttonState.Active = true;
                }
            }
            else
            {
                if (buttonState.Active)
                {
                    buttonState.Active = false;
                    buttonState.JustDeactivated = true;
                }
            }
        }
    }

    public struct PlayerInputStateMachine
    {
        public bool Active;
        public bool JustActivated;
        public bool JustDeactivated;
        public float Value;
    }
}