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
    public Vector3 EjectionSpeed = new Vector3(4f, 2.5f, -1.2f);
    public Vector3 EjectionSpin = new Vector3(20f, 180f, 30f);
    [Header("Component Connections")]
    public VRClosedBolt bolt;
    public VRFirearm_ClosedBoltChargeHandle chargeHandle;
    public VRFirearm_Chamber Chamber;
    public Transform Trigger;
    public Transform FireSelectorSwitch;
    [Header("Audio Parameters")]
    public AudioSource m_Aud;
    public AudioClip[] selectorSwitch;
    [Header("Round Positions")]
    public Transform RoundPos_Ejecting;
    public Transform RoundPos_Ejection;
    public Transform RoundPos_MagazinePos;
    private VRFirearm_ProxyRound m_proxy;
    [Header("Trigger Parameters")]
    public bool HasTrigger;
    public float TriggerFiringThreshold = 0.8f;
    public float TriggerResetThreshold = 0.4f;
    public float TriggerDualStageThreshold = 0.95f;
    public float Trigger_ForwardValue;
    public float Trigger_RearwardValue;
    public Axis TriggerAxis;
    public bool UsesDualStageFullAuto;
    public MoveType TriggerMoveType = MoveType.Rotation;
    private float m_triggerFloat;
    private bool m_hasTriggerReset;
    private int m_fireSelectorMode;
    private float m_timeSinceFiredShot = 1f;
    [Header("SpecialFeatures")]
    public bool BoltLocksWhenNoMagazineFound;
    [Header("Fire Selector Parameters")]
    public MoveType FireSelector_MoveType = MoveType.Rotation;
    public Axis FireSelector_Axis;
    public FireSelectorMode[] FireSelector_Modes;
    [HideInInspector]
    public bool IsBoltCatchButtonHeld;
    private int m_CamBurst;

    private bool m_isHammerCocked;

    public bool IsHammerCocked
    {
        get
        {
            return m_isHammerCocked;
        }
    }

    public int FireSelectorModeIndex
    {
        get
        {
            return m_fireSelectorMode;
        }
    }

    public bool HasExtractedRound()
    {
        return m_proxy.IsFull;
    }

    private void Awake()
    {
        m_CamBurst = 1;
        m_proxy = new GameObject("m_proxyRound").AddComponent<VRFirearm_ProxyRound>();
        m_proxy.Init(transform);
    }

    public void CockHammer()
    {
        if (m_isHammerCocked)
            return;
        Debug.Log("<color=green> Event: </color> HAMMER COCKED!");
        m_isHammerCocked = true;
        //audSource.PlayOneShot(preFire);
    }

    public void DropHammer()
    {
        if (!m_isHammerCocked)
            return;
        Debug.Log("<color=green> Event: </color> HAMMER DROPPED!");
        m_isHammerCocked = false;
        //audSource.PlayOneShot(hammerHit);
        Fire();
    }

    public bool IsWeaponOnSafe()
    {
        return FireSelector_Modes.Length != 0 && FireSelector_Modes[m_fireSelectorMode].ModeType == FireSelectorModeType.Safe;
    }

    protected virtual void ToggleFireSelector()
    {
        if (FireSelector_Modes.Length <= 1)
            return;
        int num1 = UnityEngine.Random.Range(1, 3);
        //m_Aud.PlayOneShot(selectorSwitch[num1]);
        /*
        if (bolt.UsesAKSafetyLock && !bolt.IsBoltForwardOfSafetyLock())
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
        if (m_triggerFloat < 0.100000001490116)
            m_CamBurst = fireSelectorMode.BurstAmount;
        if (FireSelectorSwitch != null)
            AnimatedComponent(FireSelectorSwitch, fireSelectorMode.SelectorPosition, FireSelector_MoveType, FireSelector_Axis);
    }

    public void EjectExtractedRound()
    {
        if (!Chamber.IsFull)
            return;
        Chamber.EjectRound(RoundPos_Ejection.position, transform.right * EjectionSpeed.x + transform.up * EjectionSpeed.y + transform.forward * EjectionSpeed.z, transform.right * EjectionSpin.x + transform.up * EjectionSpin.y + transform.forward * EjectionSpin.z);
    }

    public void BeginChamberingRound()
    {
        bool flag = false;
        GameObject go = null;
        if (!m_proxy.IsFull && currentLoadedMagazine != null && (currentLoadedMagazine.HasRound()))
        {
            flag = true;
            go = currentLoadedMagazine.RemoveRound(false);
        }
        if (!flag || !flag)
            return;
        m_proxy.SetFromPrefabReference(go);
    }

    public bool ChamberRound()
    {
        if (!m_proxy.IsFull || Chamber.IsFull)
            return false;
        Chamber.SetRound(m_proxy.Round);
        m_proxy.ClearProxy();
        return true;
    }

    public bool Fire() 
    {
        if (!Chamber.Fire())
            return false;
        m_timeSinceFiredShot = 0.0f;
        Fire(Chamber, muzzlePosition);
        Debug.Log("<color=green> Event: </color> WEAPON FIRED!");
        //FireMuzzleEffects();
        //Recoil(IsHedlWithTwoHands());
        //audSource.PlayOneShot(gunShot);
        bolt.ImpartFiringImpulse();
        return true;
    }

    private void Update()
    {
        if (m_grabHand != null)
        {
            UpdateInputAndAnimate(m_grabHand);
            if (HasTrigger)
                AnimatedComponent(Trigger, Mathf.Lerp(Trigger_ForwardValue, Trigger_RearwardValue, m_triggerFloat), TriggerMoveType, TriggerAxis);
        }
        if (HasChargingHandle)
        {
            chargeHandle.UpdateHandle();
            bolt.UpdateHandleHeldState(chargeHandle.ShouldControlBolt(), 1f - chargeHandle.GetBoltLerpBetweenLockAndFore());
        }
        bolt.UpdateBolt();
        UpdateDisplayRoundPositions();
        if (m_timeSinceFiredShot >= 1.0)
            return;
        m_timeSinceFiredShot += Time.deltaTime;
    }

    public override void LoadMagazine(VRFirearm_Magazine mag)
    {
        base.LoadMagazine(mag);
        if (!BoltLocksWhenNoMagazineFound || !(mag != null) || !bolt.IsBoltLocked())
            return;
        bolt.ReleaseBolt();
    }

    public override void UpdateInteraction(VRHand hand)
    {
        base.UpdateInteraction(hand);
        UpdateInputAndAnimate(hand);
    }

    private void UpdateInputAndAnimate(VRHand hand)
    {
        IsBoltCatchButtonHeld = false;
        //m_triggerFloat = !m_hasTriggeredUpSinceBegin ? 0.0f : hand.input.TriggerFloat;
        m_triggerFloat = hand.input.TriggerFloat;
        if (!m_hasTriggerReset && m_triggerFloat <= TriggerResetThreshold)
        {
            m_hasTriggerReset = true;
            if (FireSelector_Modes.Length > 0)
                m_CamBurst = FireSelector_Modes[m_fireSelectorMode].BurstAmount;
            //audSource.PlayOneShot(triggerReset);
        }
        Vector2 touchpadAxes = hand.input.TouchpadAxes;
        if (hand.input.TouchpadDown && touchpadAxes.magnitude > 0.200000002980232)
        {
            if (Vector2.Angle(touchpadAxes, Vector2.left) <= 45.0)
                ToggleFireSelector();
            else if (Vector2.Angle(touchpadAxes, Vector2.up) <= 45.0)
            {
                if (HasBoltReleaseButton)
                    bolt.ReleaseBolt();
            }
            else if (Vector2.Angle(touchpadAxes, Vector2.down) <= 45.0 && HasMagReleaseButton && bolt.CurPos >= VRClosedBolt.BoltPos.Locked && !m_proxy.IsFull)
                ReleaseMag();
        }
        if (hand.input.TouchpadPressed && touchpadAxes.magnitude > 0.200000002980232)
        {
            if (Vector2.Angle(touchpadAxes, Vector2.right) <= 45.0 && HasBoltCatchButton)
                IsBoltCatchButtonHeld = true;
        }
        FireSelectorModeType modeType = FireSelector_Modes[m_fireSelectorMode].ModeType;
        if (modeType == FireSelectorModeType.Safe || m_triggerFloat < TriggerFiringThreshold || bolt.CurPos != VRClosedBolt.BoltPos.Forward || !m_hasTriggerReset && (modeType != FireSelectorModeType.FullAuto || UsesDualStageFullAuto) && (modeType != FireSelectorModeType.FullAuto || !UsesDualStageFullAuto || m_triggerFloat <= TriggerDualStageThreshold) && (modeType != FireSelectorModeType.Burst || m_CamBurst <= 0))
            return;
        DropHammer();
        m_hasTriggerReset = false;
        if (m_CamBurst <= 0)
            return;
        --m_CamBurst;
    }

    private void UpdateDisplayRoundPositions()
    {
        float betweenLockAndFore = bolt.GetBoltLerpBetweenLockAndFore();
        if (Chamber.IsFull)
        {
            Chamber.ProxyRound.position = Vector3.Lerp(RoundPos_Ejecting.position, Chamber.transform.position, betweenLockAndFore);
            Chamber.ProxyRound.rotation = Quaternion.Slerp(RoundPos_Ejecting.rotation, Chamber.transform.rotation, betweenLockAndFore);
        }
        if (!m_proxy.IsFull)
            return;
        m_proxy.ProxyRound.position = Vector3.Lerp(RoundPos_MagazinePos.position, Chamber.transform.position, betweenLockAndFore);
        m_proxy.ProxyRound.rotation = Quaternion.Slerp(RoundPos_MagazinePos.rotation, Chamber.transform.rotation, betweenLockAndFore);
    }

    public void ReleaseMag()
    {
        if (currentLoadedMagazine == null)
            return;
        EjectMagazine();
    }

    public enum FireSelectorModeType
    {
        Safe,
        Single,
        Burst,
        FullAuto,
    }

    [Serializable]
    public class FireSelectorMode
    {
        public float SelectorPosition;
        public FireSelectorModeType ModeType;
        public int BurstAmount = 3;
    }
}
