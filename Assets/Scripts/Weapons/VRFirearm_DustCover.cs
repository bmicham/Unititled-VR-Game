using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFirearm_DustCover : MonoBehaviour
{
    public VRClosedBolt Bolt;
    public Transform DustCoverGeo;
    private bool m_isOpen;
    public float OpenRot;
    public float ClosedRot;
    public float RotSpeed = 360f;
    private float m_curRot;
    private float m_tarRot;

    protected void Awake()
    {
        m_curRot = ClosedRot;
        m_tarRot = ClosedRot;
        Close();
    }

    protected void Update()
    {
        if (!m_isOpen && Bolt.CurPos != VRClosedBolt.BoltPos.Forward)
            Open();
        if (Mathf.Abs(m_tarRot - m_curRot) <= 0.00999999977648258)
            return;
        m_curRot = Mathf.MoveTowards(m_curRot, m_tarRot, Time.deltaTime * RotSpeed);
        DustCoverGeo.localEulerAngles = new Vector3(m_curRot, 0.0f, 0.0f);
    }

    private void Open()
    {
        m_isOpen = true;
        m_tarRot = OpenRot;
        RotSpeed = 1900f;
    }

    private void Close()
    {
        m_isOpen = false;
        m_tarRot = ClosedRot;
        RotSpeed = 500f;
    }
}
