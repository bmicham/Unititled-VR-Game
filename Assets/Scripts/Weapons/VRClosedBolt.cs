using System.Collections;
using System.Collections.Generic;
using KP;
using UnityEngine;

public class VRClosedBolt : MonoBehaviour
{
    public bool DoTest = false;
    public float SpringStiffness = 5f;
    [Header("Bolt")]
    public VRFirearm_ClosedBolt parentFirearm;
    public float Speed_Forward;
    public float Speed_Rearward;
    public float SpeedHeld = 1f;
    public BoltPos CurPos;
    public BoltPos LastPos;
    public Transform Point_Bolt_Forward;
    public Transform Point_Bolt_Lock;
    public Transform Point_Bolt_Rear;
    public bool HasLastRoundBoltHoldOpen = true;
    //public bool UsesAKSafetyLock;
    private float m_curBoltSpeed;
    private float m_boltZ_current;
    private float m_boltZ_heldTarget;
    private float m_boltZ_forward;
    private float m_boltZ_lock;
    private float m_boltZ_rear;
    private float m_boltZ_safetylock;
    private bool m_isBoltLocked;
    private bool m_isHandleHeld;
    private float m_handleLerp;
    public bool HasBoltCatchReleaseButton;
    private bool m_isBoltCatchHeldOnHandle;
    private bool m_isReleaseCatchHeldOnHandle;
    [Header("Reciprocating Barrel")]
    public bool HasReciprocatingBarrel;
    public Transform Barrel;
    public Vector3 BarrelForward;
    public Vector3 BarrelRearward;
    private bool m_isBarrelReciprocating;
    [Header("Hammer")]
    public bool HasHammer;
    public Transform Hammer;
    public Vector3 HammerForward;
    public Vector3 HammerRearward;

    protected void Awake()
    {
        m_boltZ_current = transform.localPosition.z;
        m_boltZ_forward = Point_Bolt_Forward.localPosition.z;
        m_boltZ_lock = Point_Bolt_Lock.localPosition.z;
        m_boltZ_rear = Point_Bolt_Rear.localPosition.z;
    }

    public float GetBoltLerpBetweenLockAndFore() => Mathf.InverseLerp(m_boltZ_lock, m_boltZ_forward, m_boltZ_current);

    public float GetBoltLerpBetweenRearAndFore() => Mathf.InverseLerp(m_boltZ_rear, m_boltZ_forward, m_boltZ_current);

    public void LockBolt()
    {
        if (m_isBoltLocked)
            return;
        m_isBoltLocked = true;
    }

    public void ReleaseBolt()
    {
        if (!m_isBoltLocked)
            return;
        //if (!IsGrabbed)
            //parentFirearm.PlayAudioEvent(FirearmAudioEventType.BoltRelease);
        m_isBoltLocked = false;
    }

    public bool IsBoltLocked()
    {
        return m_isBoltLocked;
    }

    public void UpdateHandleHeldState(bool state, float lerp)
    {
        m_isHandleHeld = state;
        m_handleLerp = lerp;
    }

    public void ImpartFiringImpulse()
    {
        m_curBoltSpeed = Speed_Rearward;
        if (CurPos != BoltPos.Forward)
            return;
        m_isBarrelReciprocating = true;
    }

    public bool IsBoltForwardOfSafetyLock()
    {
        return m_boltZ_current > m_boltZ_safetylock;
    }

    public void UpdateBolt()
    {
        bool flag = false;
        if (m_isHandleHeld)
            flag = true;
        if (m_isHandleHeld)
            m_boltZ_heldTarget = Mathf.Lerp(m_boltZ_forward, m_boltZ_rear, m_handleLerp);
        Vector2 vector2 = new Vector2(m_boltZ_rear, m_boltZ_forward);
        if (m_boltZ_current <= m_boltZ_lock && m_isBoltLocked)
            vector2 = new Vector2(m_boltZ_rear, m_boltZ_lock);
        if (flag)
            m_curBoltSpeed = 0.0f;
        else if (CurPos < BoltPos.LockedToRear && m_curBoltSpeed >= 0.0 || LastPos >= BoltPos.Rear)
            m_curBoltSpeed = Mathf.MoveTowards(m_curBoltSpeed, Speed_Forward, Time.deltaTime * SpringStiffness);
        float boltZCurrent1 = m_boltZ_current;
        float boltZCurrent2 = m_boltZ_current;
        float num1 = Mathf.Clamp(!flag ? m_boltZ_current + m_curBoltSpeed * Time.deltaTime : Mathf.MoveTowards(m_boltZ_current, m_boltZ_heldTarget, SpeedHeld * Time.deltaTime), vector2.x, vector2.y);
        if (Mathf.Abs(num1 - m_boltZ_current) > Mathf.Epsilon)
        {
            m_boltZ_current = num1;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, m_boltZ_current);
            if (HasReciprocatingBarrel && m_isBarrelReciprocating)
                Barrel.localPosition = Vector3.Lerp(BarrelForward, BarrelRearward, 1f - GetBoltLerpBetweenLockAndFore());
        }
        else
            m_curBoltSpeed = 0.0f;
        if (HasHammer)
            Hammer.localEulerAngles = !parentFirearm.IsHammerCocked ? Vector3.Lerp(HammerRearward, HammerForward, GetBoltLerpBetweenLockAndFore()) : HammerRearward;
        BoltPos curPos1 = CurPos;
        BoltPos boltPos = Mathf.Abs(m_boltZ_current - m_boltZ_forward) >= 1.0 / 1000.0 ? (Mathf.Abs(m_boltZ_current - m_boltZ_lock) >= 1.0 / 1000.0 ? (Mathf.Abs(m_boltZ_current - m_boltZ_rear) >= 1.0 / 1000.0 ? (m_boltZ_current <= m_boltZ_lock ? BoltPos.LockedToRear : BoltPos.ForwardToMid) : BoltPos.Rear) : BoltPos.Locked) : BoltPos.Forward;
        int curPos2 = (int)CurPos;
        CurPos = boltPos;
        if (CurPos == BoltPos.Rear && LastPos != BoltPos.Rear)
            BoltEvent_SmackRear();
        if (CurPos == BoltPos.Locked && LastPos != BoltPos.Locked)
            BoltEvent_BoltCaught();
        if (CurPos >= BoltPos.Locked && LastPos < BoltPos.Locked)
            BoltEvent_EjectRound();
        if (CurPos < BoltPos.Locked && LastPos > BoltPos.ForwardToMid)
            BoltEvent_ExtractRoundFromMag();
        if (CurPos == BoltPos.Forward && LastPos != BoltPos.Forward)
            BoltEvent_ArriveAtFore();
        if (CurPos >= BoltPos.Locked && (HasLastRoundBoltHoldOpen && parentFirearm.currentLoadedMagazine != null && (!parentFirearm.currentLoadedMagazine.HasRound())))
            LockBolt();
        LastPos = CurPos;
    }

    private void BoltEvent_ArriveAtFore()
    {
        parentFirearm.ChamberRound();
        if (HasReciprocatingBarrel && m_isBarrelReciprocating)
        {
            m_isBarrelReciprocating = false;
            Barrel.localPosition = BarrelForward;
        }
        //parentFirearm.PlayAudioEvent(FirearmAudioEventType.BoltSlideForward);
    }

    private void BoltEvent_EjectRound()
    {
        parentFirearm.EjectExtractedRound();
        parentFirearm.CockHammer();
    }

    private void BoltEvent_ExtractRoundFromMag()
    {
        parentFirearm.BeginChamberingRound();
    }

    private void BoltEvent_BoltCaught()
    {
        if (!m_isBoltLocked)
            return;
        //parentFirearm.PlayAudioEvent(FirearmAudioEventType.BoltSlideBackLocked);
    }

    private void BoltEvent_SmackRear()
    {
        if (m_isHandleHeld && (!parentFirearm.BoltLocksWhenNoMagazineFound || parentFirearm.currentLoadedMagazine != null))
            ReleaseBolt();
        if (parentFirearm.BoltLocksWhenNoMagazineFound && parentFirearm.currentLoadedMagazine == null)
            LockBolt();
        //parentFirearm.PlayAudioEvent(FirearmAudioEventType.BoltSlideBack);
    }

    public enum BoltPos
    {
        Forward,
        ForwardToMid,
        Locked,
        LockedToRear,
        Rear,
    }
}
