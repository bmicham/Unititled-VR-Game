using System;
using System.Linq;
using UnityEngine;

namespace HexabodyVR.PlayerController
{
    public class HexaHands : HexaHandsBase
    {
        [Header("HexaHands")]

        [Header("Joint Settings")]
        public float Spring = 5000;
        public float Damper = 1000;
        public float MaxForce = 1500;

        public float SlerpSpring = 3000;
        public float SlerpDamper = 200;
        public float SlerpMaxForce = 75;

        public BasicGrabber Grabber;

        protected override void Awake()
        {
            base.Awake();

            SetStrength(StrengthState.Default);
        }

        private void OnReleased()
        {
            SetHandState(HandGrabState.None);
        }

        private void OnGrabbed()
        {
            UpdateHandState();
        }


        protected override void SetStrength(StrengthState state)
        {
            var drive = new JointDrive();
            drive.positionSpring = Spring;
            drive.positionDamper = Damper;
            drive.maximumForce = MaxForce;
            Joint.xDrive = Joint.yDrive = Joint.zDrive = drive;

            var slerpDrive = new JointDrive();
            slerpDrive.positionSpring = SlerpSpring;
            slerpDrive.positionDamper = SlerpDamper;
            slerpDrive.maximumForce = SlerpMaxForce;
            Joint.slerpDrive = slerpDrive;
        }

        protected override HandGrabState GetHandState()
        {
            if (!Grabber || !Grabber.Joint)
                return HandGrabState.None;

            if (Grabber.GrabbedBody)
            {
                return Grabber.GrabbedBody.isKinematic ? HandGrabState.KinematicGrab : HandGrabState.DynamicGrab;
            }

            return HandGrabState.KinematicGrab;
        }

        protected override bool CanUnstuck()
        {
            return !Grabber || !Grabber.Joint;
        }
    }
}
