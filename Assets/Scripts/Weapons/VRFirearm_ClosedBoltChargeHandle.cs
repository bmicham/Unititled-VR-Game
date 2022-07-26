using System.Collections;
using System.Collections.Generic;
using KP;
using UnityEngine;

public class VRFirearm_ClosedBoltChargeHandle : GrabPoint
{
    [Header("Charge Handle Parameters")]
    public VRFirearm_ClosedBolt parentFirearm;
    public float _SlideSpeed = 2.5f;
    public float _boltForce = 5f;
    [Header("Bolt Handle")]
    public VRFirearm_ClosedBolt Weapon;
    public HandlePos CurPos;
    public HandlePos LastPos;
    public Transform _slideForward;
    public Transform _slideRear;
    public Transform _slideLock;

    private float pos_forward;
    private float pos_rear;
    private float pos_lock;
    private float pos_current;
    private float _ChargeHandleMaxDistance;
    private SoftJointLimit _jointLimit = new SoftJointLimit();

    public float GetBoltLerpBetweenRearAndFore() => Mathf.InverseLerp(pos_forward, pos_rear, pos_current);

    public bool ShouldControlBolt()
    {
        return IsGrabbed;
    }

    private void Awake()
    {
        pos_forward = _slideForward.localPosition.x;
        pos_rear = _slideRear.localPosition.x;
        pos_lock = _slideLock.localPosition.x;
        pos_current = transform.localPosition.x;
        _ChargeHandleMaxDistance = _slideRear.localPosition.x - _slideForward.localPosition.x;
    }

    public void UpdateChargingHandle() 
    {
        if (CurPos != HandlePos.Forward && !IsGrabbed)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _slideForward.localPosition, _SlideSpeed * Time.deltaTime);
        }
        if (IsGrabbed)
        {
            var pullDirection = (m_grabHand.palmPosition.position - m_grabPointAnchor.transform.position);
            var backDirection = (_slideRear.position - _slideForward.position).normalized;
            var amount = Vector3.Dot(pullDirection, backDirection);
            var num1 = Mathf.Clamp(amount, 0, _ChargeHandleMaxDistance);
            var vector1 = _slideForward.position + backDirection.normalized * num1;
            transform.position = vector1;
        }
        pos_current = transform.localPosition.x;
        var num2 = Mathf.InverseLerp(pos_forward, pos_rear, pos_current);
        HandlePos curPos1 = CurPos;
        HandlePos handlePos = num2 <= 0 ? HandlePos.Forward : (num2 >= 1.0 ? HandlePos.Rear : HandlePos.ForwardToMid);
        int curPos2 = (int)CurPos;
        CurPos = (HandlePos)Mathf.Clamp((int)handlePos, curPos2 - 1, curPos2 + 1);

        if (CurPos == HandlePos.Forward && LastPos != HandlePos.Forward)
        {
            Debug.Log("<color=green> Event: </color> Arrived at forward position!");
            Event_ArriveAtFore();
        }
        else if (CurPos == HandlePos.Rear && LastPos == HandlePos.ForwardToMid && (LastPos != HandlePos.Rear || LastPos != HandlePos.Forward))
        {
            Debug.Log("<color=green> Event: </color> Smacked Rear!");
        }
        LastPos = CurPos;
    }

    private void Event_ArriveAtFore()
    {
        Vector3 v1 = parentFirearm.transform.position - transform.position;
        _RB.AddForceAtPosition(v1.normalized * _boltForce, transform.position, ForceMode.Impulse);
        //Weapon.PlayAudioEvent(FirearmAudioEventType.HandleForward);
    }

    //private void Event_SmackRear() => Weapon.PlayAudioEvent(FirearmAudioEventType.HandleBack);

    protected override void CreateJoint(VRHand hand)
    {
        m_startRot = transform.rotation;

        _Jt = objectRoot.AddComponent<ConfigurableJoint>();
        _Jt.autoConfigureConnectedAnchor = _jointConfig.autoConfigureConnectedAnchor;
        if (_jointConfig.SetXMotion)
            _Jt.xMotion = _jointConfig.xMotion;
        if (_jointConfig.SetYMotion)
            _Jt.yMotion = _jointConfig.yMotion;
        if (_jointConfig.SetZMotion)
            _Jt.zMotion = _jointConfig.zMotion;
        _Jt.angularXMotion = _jointConfig.AngularXMotion;
        _Jt.angularYMotion = _jointConfig.AngularYMotion;
        _Jt.angularZMotion = _jointConfig.AngularZMotion;
        _jointLimit.limit = _jointConfig._LinearLimit;
        _Jt.linearLimit = _jointLimit;
        _Jt.anchor = m_grabPointAnchor.localPosition;
        _Jt.connectedAnchor = hand.palmPosition.localPosition;
        _Jt.axis = _jointConfig.m_jointPrimaryAxis;
        _Jt.secondaryAxis = _jointConfig.m_jointSecondaryAxis;
        _Jt.connectedBody = hand.physicalHand;
        _Jt.enablePreprocessing = _jointConfig.enablePreprocessing;
        _Jt.rotationDriveMode = _jointConfig.rotationDriveMode;
        slerpDrive.positionSpring = _jointConfig.m_AngularSpring;
        slerpDrive.positionDamper = _jointConfig.m_AngularDamper;
        slerpDrive.maximumForce = _jointConfig.m_AngularMaximumForce;
        _Jt.slerpDrive = slerpDrive;
        ConfigurableJointExtensions.SetTargetRotationLocal(_Jt, hand.transform.rotation * Quaternion.Euler(m_rotOffset.x, m_rotOffset.y, m_rotOffset.z), m_startRot);
    }

    public enum HandlePos
    {
        Forward,
        ForwardToMid,
        Rear,
    }
}
