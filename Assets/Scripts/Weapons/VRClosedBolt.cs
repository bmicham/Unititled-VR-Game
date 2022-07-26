using System.Collections;
using System.Collections.Generic;
using KP;
using UnityEngine;

public class VRClosedBolt : GrabPoint
{
    [Header("Bolt")]
    public VRFirearm_ClosedBolt Weapon;
    public float Speed_Forward;
    public float Speed_Rearward;
    public float Speed_Held;
    public float _BoltSpeed = 5f;
    public BoltPos CurPos;
    public BoltPos LastPos;
    public Transform Point_Bolt_Forward;
    public Transform Point_Bolt_Rear;
    public bool HasLastRoundBoltHoldOpen = true;
    //public bool UsesAKSafetyLock;
    private float m_curBoltSpeed;
    private float m_boltZ_current;
    private float m_boltZ_heldTarget;
    private float m_boltZ_forward;
    private float m_boltZ_rear;
    private float m_boltZ_safetylock;
    private bool m_isBoltLocked;
    private bool m_isHandleHeld;
    private float m_handleLerp;
    private float _BoltMaxDistance;
    public bool HasBoltCatchReleaseButton;
    private bool m_isBoltCatchHeldOnHandle;
    private bool m_isReleaseCatchHeldOnHandle;
    [Header("Reciprocating Barrel")]
    public bool HasReciprocatingBarrel;
    public Transform Barrel;
    public Vector3 BarrelForward;
    public Vector3 BarrelRearward;
    private bool m_isBarrelReciprocating;
    /*
    [Header("Hammer")]
    public bool HasHammer;
    public Transform Hammer;
    public Vector3 HammerForward;
    public Vector3 HammerRearward;
    [Header("Rotating Bit")]
    public bool HasRotatingPart;
    public Transform RotatingPart;
    public Vector3 RotatingPartNeutralEulers;
    public Vector3 RotatingPartLeftEulers;
    public Vector3 RotatingPartRightEulers;
    [Header("Z Rot Part")]
    public bool HasZRotPart;
    public Transform ZRotPiece;
    public AnimationCurve ZRotCurve;
    public Vector2 ZAngles;
    public bool ZRotPieceDips;
    public float DipMagnitude;
    public bool ZRotPieceLags;
    public float LagMagnitude;
    [Header("Z Scale Part")]
    public bool HasZScalePart;
    public Transform ZScalePiece;
    public AnimationCurve ZScaleCurve;
    */

    protected void Awake()
    {
        m_boltZ_current = transform.localPosition.x;
        m_boltZ_forward = Point_Bolt_Forward.localPosition.x;
        m_boltZ_rear = Point_Bolt_Rear.localPosition.x;
        _BoltMaxDistance = m_boltZ_forward - m_boltZ_rear;
        //if (!UsesAKSafetyLock)
        //return;
        //m_boltZ_safetylock = Point_Bolt_SafetyLock.localPosition.x;
    }

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
            //Weapon.PlayAudioEvent(FirearmAudioEventType.BoltRelease);
        m_isBoltLocked = false;
    }

    public bool IsBoltLocked() => m_isBoltLocked;

    public override void UpdateInteraction(VRHand hand)
    {
        base.UpdateInteraction(hand);
        //if (HasRotatingPart)
            //RotatingPart.localEulerAngles = Vector3.Dot((transform.position - m_grabHand.palmPosition.position).normalized, transform.right) <= 0.0 ? RotatingPartRightEulers : RotatingPartLeftEulers;
        if (!hand.input.TouchpadDown)
            return;
        if (Vector2.Angle(hand.input.TouchpadAxes, Vector2.down) < 45.0 && HasBoltCatchReleaseButton)
        {
            m_isBoltCatchHeldOnHandle = true;
            //ForceBreakInteraction();
        }
        else
        {
            if (Vector2.Angle(hand.input.TouchpadAxes, Vector2.up) >= 45.0 || !HasBoltCatchReleaseButton)
                return;
            m_isReleaseCatchHeldOnHandle = true;
            //ForceBreakInteraction();
        }
    }

    public override void EndGrab(VRHand hand)
    {
        if (!m_isBoltLocked)
            m_curBoltSpeed = Speed_Forward;
        if (m_isBoltCatchHeldOnHandle)
        {
            m_isBoltCatchHeldOnHandle = false;
        }
        if (m_isReleaseCatchHeldOnHandle)
        {
            m_isReleaseCatchHeldOnHandle = false;
            ReleaseBolt();
        }
        base.EndGrab(hand);
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

    public bool IsBoltForwardOfSafetyLock() => m_boltZ_current > m_boltZ_safetylock;

    public void UpdateBolt()
    {
        if (CurPos != VRClosedBolt.BoltPos.Forward && !IsGrabbed)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Point_Bolt_Forward.localPosition, _BoltSpeed * Time.deltaTime);
        }
        if (Weapon.chargeHandle.IsGrabbed)
        {
            transform.localPosition = Vector3.Lerp(Point_Bolt_Forward.localPosition, Point_Bolt_Rear.localPosition, Weapon.chargeHandle.GetBoltLerpBetweenRearAndFore());
        }
        m_boltZ_current = transform.localPosition.x;
        var num2 = Mathf.InverseLerp(m_boltZ_forward, m_boltZ_rear, m_boltZ_current);
        BoltPos curPos1 = CurPos;
        BoltPos BoltPos = num2 <= 0 ? BoltPos.Forward : (num2 >= 1.0 ? BoltPos.Rear : BoltPos.ForwardToMid);
        int curPos2 = (int)CurPos;
        CurPos = (BoltPos)Mathf.Clamp((int)BoltPos, curPos2 - 1, curPos2 + 1);

        if (CurPos == BoltPos.Forward && LastPos != BoltPos.Forward)
        {
            Debug.Log("<color=green> Event: </color> Bolt arrived at forward position!");
            BoltEvent_ArriveAtFore();
        }
        else if (CurPos == BoltPos.Rear && LastPos == BoltPos.ForwardToMid && (LastPos != BoltPos.Rear || LastPos != BoltPos.Forward))
        {
            Debug.Log("<color=green> Event: </color> Bolt smacked Rear!");
        }
        LastPos = CurPos;
    }

    private void BoltEvent_ArriveAtFore()
    {
        //Weapon.ChamberRound();
        if (HasReciprocatingBarrel && m_isBarrelReciprocating)
        {
            m_isBarrelReciprocating = false;
            Barrel.localPosition = BarrelForward;
        }
        //if (IsGrabbed)
        //    Weapon.PlayAudioEvent(FirearmAudioEventType.BoltSlideForwardHeld);
        //else
        //    Weapon.PlayAudioEvent(FirearmAudioEventType.BoltSlideForward);
    }

    private void BoltEvent_EjectRound()
    {
        //Weapon.EjectExtractedRound();
        //Weapon.CockHammer();
    }

    //private void BoltEvent_ExtractRoundFromMag() => Weapon.BeginChamberingRound();

    private void BoltEvent_BoltCaught()
    {
        if (!m_isBoltLocked)
            return;
        //Weapon.PlayAudioEvent(FirearmAudioEventType.BoltSlideBackLocked);
    }

    private void BoltEvent_SmackRear()
    {
        /*
        if ((IsGrabbed || m_isHandleHeld) && (!Weapon.BoltLocksWhenNoMagazineFound || Weapon.Magazine != null))
            ReleaseBolt();
        if (Weapon.EjectsMagazineOnEmpty && Weapon.Magazine != null && !Weapon.Magazine.HasARound())
            Weapon.EjectMag();
        if (Weapon.BoltLocksWhenNoMagazineFound && Weapon.Magazine == null)
            LockBolt();
        if (IsGrabbed)
            Weapon.PlayAudioEvent(FirearmAudioEventType.BoltSlideBackHeld);
        else
            Weapon.PlayAudioEvent(FirearmAudioEventType.BoltSlideBack);
        */
    }

    public enum BoltPos
    {
        Forward,
        ForwardToMid,
        Rear,
    }
}
