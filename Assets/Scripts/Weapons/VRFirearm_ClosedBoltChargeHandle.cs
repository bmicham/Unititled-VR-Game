using System.Collections;
using System.Collections.Generic;
using KP;
using UnityEngine;

public class VRFirearm_ClosedBoltChargeHandle : GrabPoint
{
    public float SpringStiffness = 100f;
    [Header("Safety Catch Config")]
    public bool UsesRotation = true;
    public float SlapDistance = 0.1f;
    /*
    [Header("Audio Settings")]
    public AudioSource audSource;
    public AudioClip handleGrab;
    public AudioClip boltRelease;
    public AudioClip handleForward;
    public AudioClip handleReverse;
    */
    [Header("Bolt Handle")]
    public VRFirearm_ClosedBolt Weapon;
    public float Speed_Forward;
    public float Speed_Held;
    public HandlePos CurPos;
    public HandlePos LastPos;
    public Transform Point_Forward;
    public Transform Point_LockPoint;
    public Transform Point_Rear;
    public Transform Point_SafetyRotLimit;
    private float m_curSpeed;
    private float m_posZ_current;
    private float m_posZ_heldTarget;
    private float m_posZ_forward;
    private float m_posZ_lock;
    private float m_posZ_rear;
    private float m_posZ_safetyrotLimit;
    public float Rot_Standard;
    public float Rot_Safe;
    public float Rot_SlipDistance;
    public bool IsSlappable;
    public Transform SlapPoint;
    private bool m_hasRotCatch;
    private float m_currentRot;
    [Header("Rotating Bit")]
    public bool HasRotatingPart;
    public Transform RotatingPart;
    public Vector3 RotatingPartNeutralEulers;
    public Vector3 RotatingPartLeftEulers;
    public Vector3 RotatingPartRightEulers;
    public bool StaysRotatedWhenBack;
    public bool UsesSoundOnGrab;
    private bool m_isHandleHeld;
    private float m_HandleLerp;
    private bool m_isAtLockAngle;
    private float _ChargeHandleMaxDistance;
    private SoftJointLimit _jointLimit = new SoftJointLimit();

    protected void Awake()
    {
        //audSource = GetComponent<AudioSource>();
        m_posZ_current = transform.localPosition.z;
        m_posZ_forward = Point_Forward.localPosition.z;
        m_posZ_lock = Point_LockPoint.localPosition.z;
        m_posZ_rear = Point_Rear.localPosition.z;
        _ChargeHandleMaxDistance = Point_Rear.localPosition.z - Point_Forward.localPosition.z;
    }

    public float GetBoltLerpBetweenLockAndFore()
    {
        return Mathf.InverseLerp(m_posZ_lock, m_posZ_forward, m_posZ_current);
    }

    public float GetBoltLerpBetweenRearAndFore()
    {
        return Mathf.InverseLerp(m_posZ_rear, m_posZ_forward, m_posZ_current);
    }

    public bool ShouldControlBolt()
    {
        if (!UsesRotation)
            return IsGrabbed;
        return IsGrabbed || m_isAtLockAngle;
    }

    public void UpdateHandle()
    {
        bool flag = false;
        if (IsGrabbed)
            flag = true;
        if (flag)
            m_posZ_heldTarget = Weapon.transform.InverseTransformPoint(GetClosestValidPoint(Point_Forward.position, Point_Rear.position, m_grabHand.transform.position)).z;
        Vector2 vector2 = new Vector2(m_posZ_rear, m_posZ_forward);
        if (flag)
            m_curSpeed = 0.0f;
        else if (m_curSpeed >= 0.0 || CurPos > HandlePos.Forward)
            m_curSpeed = Mathf.MoveTowards(m_curSpeed, Speed_Forward, Time.deltaTime * SpringStiffness);
        float posZCurrent1 = m_posZ_current;
        float posZCurrent2 = m_posZ_current;
        float num3 = Mathf.Clamp(!flag ? m_posZ_current + m_curSpeed * Time.deltaTime : Mathf.MoveTowards(m_posZ_current, m_posZ_heldTarget, Speed_Held * Time.deltaTime), vector2.x, vector2.y);
        if (Mathf.Abs(num3 - m_posZ_current) > Mathf.Epsilon)
        {
            m_posZ_current = num3;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, m_posZ_current);
        }
        else
            m_curSpeed = 0.0f;
        HandlePos curPos1 = CurPos;
        HandlePos handlePos = Mathf.Abs(m_posZ_current - m_posZ_forward) >= 1.0 / 1000.0 ? (Mathf.Abs(m_posZ_current - m_posZ_lock) >= 1.0 / 1000.0 ? (Mathf.Abs(m_posZ_current - m_posZ_rear) >= 1.0 / 1000.0 ? (m_posZ_current <= m_posZ_lock ? HandlePos.LockedToRear : HandlePos.ForwardToMid) : HandlePos.Rear) : HandlePos.Locked) : HandlePos.Forward;
        int curPos2 = (int)CurPos;
        CurPos = (HandlePos)Mathf.Clamp((int)handlePos, curPos2 - 1, curPos2 + 1);
        if (CurPos == HandlePos.Forward && LastPos != HandlePos.Forward)
            Event_ArriveAtFore();
        else if ((CurPos != HandlePos.ForwardToMid || LastPos != HandlePos.Forward) && (CurPos != HandlePos.Locked || LastPos != HandlePos.ForwardToMid) && (CurPos != HandlePos.ForwardToMid || LastPos != HandlePos.Locked))
        {
            if (CurPos == HandlePos.Locked && LastPos == HandlePos.LockedToRear && m_isAtLockAngle)
                Event_HitLockPosition();
            else if (CurPos == HandlePos.Rear && LastPos != HandlePos.Rear)
                Event_SmackRear();
        }
        LastPos = CurPos;
    }

    private void Event_ArriveAtFore()
    {
        //audSource.PlayOneShot(handleForward);
        Debug.Log("<color=green> Event: </color> Arrived at Forward Position!");
        if (!HasRotatingPart)
            return;
        RotatingPart.localEulerAngles = RotatingPartNeutralEulers;
    }

    private void Event_HitLockPosition()
    {
        Debug.Log("<color=green> Event: </color> Hit Lock Position!");
        //audSource.PlayOneShot(handleForward);
    }

    private void Event_SmackRear()
    {
        Debug.Log("<color=green> Event: </color> Smacked Rear!");
        //audSource.PlayOneShot(handleReverse);
    }

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
        Locked,
        LockedToRear,
        Rear,
    }
}
