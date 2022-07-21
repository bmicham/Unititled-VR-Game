using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JointConfig", menuName = "Killhouse/JointConfig", order = 1)]
public class JointConfig : ScriptableObject
{
    public JointType jointType = JointType.Primary;
    public Vector3 m_jointPrimaryAxis = new Vector3(1, 0, 0);
    public Vector3 m_jointSecondaryAxis = new Vector3(0, 1, 0);
    public float _LinearLimit = 0.15f;
    public bool SetXMotion = false;
    public bool SetYMotion = false;
    public bool SetZMotion = false;
    public ConfigurableJointMotion xMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion yMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion zMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion AngularXMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion AngularYMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion AngularZMotion = ConfigurableJointMotion.Locked;
    public SoftJointLimit linearLimit;
    public float m_PositionSpring = 10000f;
    public float m_PositionDamper = 1000f;
    public float m_PositionMaximumForce = 5000f;
    public RotationDriveMode rotationDriveMode;
    public float m_AngularSpring = 10000f;
    public float m_AngularDamper = 1000f;
    public float m_AngularMaximumForce = 5000f;
    public bool autoConfigureConnectedAnchor = false;
    public bool enableCollision = false;
    public bool enablePreprocessing = false;
}

public enum JointType 
{
    Primary, 
    Secondary,
    Slide,
}
