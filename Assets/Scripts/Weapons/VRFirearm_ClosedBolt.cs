using System;
using KP;
using UnityEngine;

public class VRFirearm_ClosedBolt : VRFirearm
{
    [Header("Closed Bolt Bools")]
    public bool HasFireSelectorButton = true;
    public bool HasMagReleaseButton = true;
    public bool HasBoltReleaseButton = true;
    public bool HasBoltCatchButton = true;
    public bool HasChargingHandle = true;
    [Header("Component Connections")]
    public VRClosedBolt bolt;
    public VRFirearm_ClosedBoltChargeHandle chargeHandle;
    public Transform Trigger;
    public Transform FireSelectorSwitch;
    [Header("Audio Parameters")]
    public AudioSource m_Aud;
    public AudioClip[] selectorSwitch;
    [Header("Trigger Parameters")]
    public bool HasTrigger;
    public float TriggerFiringThreshold = 0.8f;
    public float TriggerResetThreshold = 0.4f;
    public float Trigger_ForwardValue;
    public float Trigger_RearwardValue;
    public Axis TriggerAxis;
    public MoveType TriggerMoveType = MoveType.Rotation;
    private float m_triggerFloat;
    private bool m_hasTriggerReset;
    private int m_fireSelectorMode;
    [Header("Fire Selector Parameters")]
    public MoveType FireSelector_MoveType = MoveType.Rotation;
    public Axis FireSelector_Axis;
    public FireSelectorMode[] FireSelector_Modes;

    private void Update()
    {
        chargeHandle.UpdateChargingHandle();
        bolt.UpdateHandleHeldState(chargeHandle.ShouldControlBolt(), 1f - chargeHandle.GetBoltLerpBetweenRearAndFore());
        bolt.UpdateBolt();
        m_Aud = GetComponent<AudioSource>();
        if (IsGrabbed)
        {
            UpdateInputAndAnimate(m_grabHand);
            UpdateTrigger();
        }
    }

    private void UpdateInputAndAnimate(VRHand hand)
    {
        m_triggerFloat = !m_hasTriggeredUpSinceBegin ? 0.0f : hand.input.TriggerFloat;
        if (hand.input.TouchpadDown)
        {
            if (hand.input.TouchpadAxes.magnitude > 0.200000002980232)
            {
                if (Vector2.Angle(hand.input.TouchpadAxes, Vector2.left) <= 45.0)
                    ToggleFireSelector();
                /*
                else if ((double)Vector2.Angle(hand.input.TouchpadAxes, Vector2.up) <= 45.0)
                {
                    if (HasBoltReleaseButton)
                        Bolt.ReleaseBolt();
                }
                else if ((double)Vector2.Angle(hand.input.TouchpadAxes, Vector2.down) <= 45.0 && HasMagReleaseButton && (!EjectsMagazineOnEmpty || Bolt.CurPos >= ClosedBolt.BoltPos.Locked && Bolt.IsHeld && !m_proxy.IsFull))
                    ReleaseMag();
                */
            }
        }
    }


    private void UpdateTrigger()
    {
        if (HasTrigger)
            AnimatedComponent(Trigger, Mathf.Lerp(Trigger_ForwardValue, Trigger_RearwardValue, m_triggerFloat), TriggerMoveType, TriggerAxis);
    }

    protected virtual void ToggleFireSelector()
    {
        if (FireSelector_Modes.Length <= 1)
            return;
        int num1 = UnityEngine.Random.Range(1, 3);
        m_Aud.PlayOneShot(selectorSwitch[num1]);
        /*
        if (Bolt.UsesAKSafetyLock && !Bolt.IsBoltForwardOfSafetyLock())
        {
            int index = m_fireSelectorMode + 1;
            if (index >= FireSelector_Modes.Length)
                index -= FireSelector_Modes.Length;
            if (FireSelector_Modes[index].ModeType == ClosedBoltWeapon.FireSelectorModeType.Safe)
                return;
        }
        */
        ++m_fireSelectorMode;
        if (m_fireSelectorMode >= FireSelector_Modes.Length)
            m_fireSelectorMode -= FireSelector_Modes.Length;
        FireSelectorMode fireSelectorMode = FireSelector_Modes[m_fireSelectorMode];
        if (FireSelectorSwitch != null)
            AnimatedComponent(FireSelectorSwitch, fireSelectorMode.SelectorPosition, FireSelector_MoveType, FireSelector_Axis);
    }

    public void AnimatedComponent(Transform t, float val, MoveType move, Axis axis)
    {
        switch (move)
        {
            case MoveType.Translate:
                Vector3 localPosition = t.localPosition;
                switch (axis)
                {
                    case Axis.X:
                        localPosition.x = val;
                        break;
                    case Axis.Y:
                        localPosition.y = val;
                        break;
                    case Axis.Z:
                        localPosition.z = val;
                        break;
                }
                t.localPosition = localPosition;
                break;
            case MoveType.Rotation:
                Vector3 zero = Vector3.zero;
                switch (axis)
                {
                    case Axis.X:
                        zero.x = val;
                        break;
                    case Axis.Y:
                        zero.y = val;
                        break;
                    case Axis.Z:
                        zero.z = val;
                        break;
                }
                t.localEulerAngles = zero;
                break;
        }
    }

    public enum MoveType
    {
        Translate,
        Rotation,
    }

    public enum FireSelectorModeType
    {
        Safe,
        Single,
        Burst,
        FullAuto,
    }

    public enum Axis
    {
        X,
        Y,
        Z,
    }

    [Serializable]
    public class FireSelectorMode
    {
        public float SelectorPosition;
        public FireSelectorModeType ModeType;
        public int BurstAmount = 3;
    }
}
