using UnityEngine;
using KP;

public class GrabPoint : MonoBehaviour
{
    /* Soft Grab Testing
    public bool DoesSoftGrab;
    public float m_MinSoftGrabDistance = 0.1f;
    */
    public bool SetupInteraction;
    public bool IsSimpleInteract;
    public bool IsGrabbed;
    public InteractionStyle interactionStyle = InteractionStyle.Toggle;
    public bool HasPhysicalColliders = true;
    public GameObject _PhysicalCOLRoot;
    public GameObject objectRoot;

    [Header("Rigidbody Settings")]
    public Rigidbody _RB;
    public bool OverrideCenterOfMass = false;
    public Vector3 m_centerOfMass;
    [Header("Grab Offsets")]
    public Transform m_grabPointAnchor;
    public Vector3 m_rotOffset;
    [Header("Joint Settings")]
    public JointConfig _jointConfig;

    [HideInInspector]
    public VRHand m_grabHand;
    [HideInInspector]
    public ConfigurableJoint _Jt;
    [HideInInspector]
    public Quaternion m_startRot;
    [HideInInspector]
    public Quaternion _initialHandRotation;
    [HideInInspector]
    public JointDrive slerpDrive;
    [HideInInspector]
    public bool m_hasTriggeredUpSinceBegin;

    protected float triggerCooldown = 0.5f;
    private Collider[] _PhysicalColliders;
    protected SavedRigidBody savedRigidBodyMass = new SavedRigidBody();


    #region Hand Fields

    public virtual bool IsInteractable()
    {
        return true;
    }

    public virtual void SimpleInteraction(VRHand hand)
    {
    }

    #endregion

    public virtual void Start()
    {
        if (HasPhysicalColliders)
            _PhysicalColliders = _PhysicalCOLRoot.GetComponentsInChildren<Collider>();
        DoInitialSetup();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsGrabbed)
            return;
        UpdateInteraction(m_grabHand);
        if (SetupInteraction && _Jt)
        {
            _Jt.anchor = m_grabPointAnchor.localPosition;
            ConfigurableJointExtensions.SetTargetRotationLocal(_Jt, _initialHandRotation * Quaternion.Euler(m_rotOffset.x, m_rotOffset.y, m_rotOffset.z), m_startRot);
        }
    }

    public virtual void BeginGrab(VRHand hand) 
    {
        if (IsGrabbed && m_grabHand != hand && m_grabHand != null)
        {
            m_grabHand.EndCurrentGrab(this);
        }

        IsGrabbed = true;
        m_grabHand = hand;
        m_hasTriggeredUpSinceBegin = false;
        if (HasPhysicalColliders)
            MoveCollidersToLayer(false, "ObjectInHand");
        CreateJoint(hand);
    }


    public virtual void UpdateInteraction(VRHand hand)
    {
        if (!m_hasTriggeredUpSinceBegin && m_grabHand.input.TriggerFloat < 0.150000005960464)
            m_hasTriggeredUpSinceBegin = true;
        if (triggerCooldown <= 0.0)
            return;
        triggerCooldown -= Time.deltaTime;
    }

    public virtual void EndGrab(VRHand hand)
    {
        if (HasPhysicalColliders)
            MoveCollidersToLayer(false, "Default");
        DestroyJoint(hand);
        IsGrabbed = false;
        m_grabHand = null;
    }

    protected virtual void DoInitialSetup() 
    {
        if (OverrideCenterOfMass)
            _RB.centerOfMass = m_centerOfMass;
    }

    public void DestroyJoint(VRHand hand) 
    {
        Destroy(_Jt);
    }

    protected virtual void CreateJoint(VRHand hand)
    {
        m_startRot = transform.rotation;

        _initialHandRotation = hand.transform.rotation;
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

    public void AdjustJointStrength(float Spring, float Damper, float MaximumForce) 
    {
        if (_Jt == null)
            return;
        slerpDrive.positionSpring = Spring;
        slerpDrive.positionDamper = Damper;
        slerpDrive.maximumForce = MaximumForce;
        _Jt.slerpDrive = slerpDrive;
    }

    /*
    public void RemoveRigidBody()
    {
        if (_RB == null)
            return;
        savedRigidBodyMass.mass = _RB.mass;
        Destroy(_RB);
    }

    public void AddSavedRigidBody()
    {
        if (_RB != null)
            return;
        _RB = gameObject.AddComponent<Rigidbody>();
        _RB.mass = savedRigidBodyMass.mass;
        _RB.maxAngularVelocity = 100f;
        _RB.centerOfMass = m_centerOfMass;
    }
    */

    public Vector3 GetClosestValidPoint(Vector3 vA, Vector3 vB, Vector3 vPoint)
    {
        Vector3 rhs = vPoint - vA;
        Vector3 normalized = (vB - vA).normalized;
        float num1 = Vector3.Distance(vA, vB);
        float num2 = Vector3.Dot(normalized, rhs);
        if ((double)num2 <= 0.0)
            return vA;
        if ((double)num2 >= (double)num1)
            return vB;
        Vector3 vector3 = normalized * num2;
        return vA + vector3;
    }

    public void MoveCollidersToLayer(bool triggersToo, string layerName)
    {
        if (triggersToo)
        {
            foreach (Collider collider in _PhysicalColliders)
            {
                if (collider != null)
                    collider.gameObject.layer = LayerMask.NameToLayer(layerName);
            }
        }
        else
        {
            foreach (Collider collider in _PhysicalColliders)
            {
                if (collider != null && !collider.isTrigger)
                    collider.gameObject.layer = LayerMask.NameToLayer(layerName);
            }
        }
    }

    public enum InteractionStyle 
    {
        Toggle,
        Hold,
    }

    public void SetParent(Transform t)
    {
        transform.SetParent(t);
    }

    public struct SavedRigidBody
    {
        public float mass;
    }

    private void OnDrawGizmos()
    {
        if (!_RB || !OverrideCenterOfMass)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * m_centerOfMass, 0.01f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(m_grabPointAnchor.position, 0.015f);
    }
}
