using UnityEngine;
using KP;

public class SlideGrabPoint : GrabPoint
{
    [Header("Slide Joint Settings")]
    public float _SpringReduced = 5000f;
    public float _DamperReduced = 500f;
    public float _MaxForceReduced = 2500f;
    public Transform firearmSlide;
    public Transform _slideForward;
    public Transform _slideRear;
    public float _handPerc;
    public float _SlideSpeed;
    public bool DoTests;
    [Tooltip("Faux difficulty for pulling back the charging handle")]
    public float Difficulty = .05f;

    private float _handPercLimited;
    private float _maximumDistance;
    private SoftJointLimit _jointLimit = new SoftJointLimit();
    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = transform.localPosition;
        _maximumDistance = Vector3.Distance(_slideRear.localPosition, _slideForward.localPosition);
    }

    protected override void DoInitialSetup() 
    {
        _RB = objectRoot.GetComponent<Rigidbody>();
    }

    public override void BeginGrab(VRHand hand) 
    {
        base.BeginGrab(hand);
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

    protected void Update()
    {
        if (IsGrabbed && m_grabHand.input.BButtonPressed)
        {
            var pullDirection = (m_grabHand.palmPosition.position - m_grabPointAnchor.transform.position);
            var backDirection = (_slideRear.position - _slideForward.position).normalized;
            var amount = Vector3.Dot(pullDirection, backDirection);
            _handPercLimited = Mathf.Clamp(amount, 0, 1.0f);

            transform.position = _slideForward.position + backDirection.normalized * _handPercLimited;

            var distance = Vector3.Distance(transform.position, _slideForward.position);

            if (distance > _maximumDistance)
            {
                transform.position = _slideForward.position + backDirection.normalized * _maximumDistance;
            }
        }
    }
}
