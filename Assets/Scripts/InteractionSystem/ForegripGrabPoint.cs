using UnityEngine;
using KP;

public class ForegripGrabPoint : GrabPoint
{
    [Header("Foregrip Joint Settings")]
    public GrabPoint m_primaryGrabPoint;
    public bool IsForegripAttached = false;
    public float _SpringReduced = 5000f;
    public float _DamperReduced = 500f;
    public float _MaxForceReduced = 2500f;

    protected override void DoInitialSetup()
    {
        _RB = objectRoot.GetComponent<Rigidbody>();
    }

    public override void EndGrab(VRHand hand)
    {
        m_primaryGrabPoint.AdjustJointStrength(10000f, 1000f, 5000f);
        base.EndGrab(hand);
    }

    protected override void CreateJoint(VRHand hand)
    {
        m_startRot = transform.rotation;
        _initialHandRotation = m_grabHand.transform.rotation;


        _Jt = objectRoot.gameObject.AddComponent<ConfigurableJoint>();

        m_primaryGrabPoint.AdjustJointStrength(_SpringReduced, _DamperReduced, _MaxForceReduced);

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

}
