using System.Collections;
using System.Collections.Generic;
using KP;
using UnityEngine;

public class VRFirearm_Magazine : GrabPoint
{
    [Header("Magazine Settings")]
    public VRFirearm_MagazineType magazineType;
    public VR_RoundCaliber magRoundCalibre;
    public VRFirearm_Round roundPrefab;
    public int currentAmount = 30;
    public int maxCapacity = 30;
    public bool IsExtractable = true;
    public float _ejectSpeed = 1f;
    public MagazineState currentState = MagazineState.Free;
    public VRFirearm currentFireArm;
    public bool UsesVizInterp;
    public Transform Viz;
    [Header("Rounds In Mag Settings")]
    [Tooltip("Needs to be the round prefab")]
    public GameObject bulletPrefab;
    public GameObject[] StaticBullets;

    private Vector3 m_vizLerpStartPos;
    private Vector3[] StaticBulletPositions;
    private float timeSinceRoundAdded;
    private float m_vizLerpSpeedMultiplier_Eject = 18f;
    private float m_vizLerpSpeedMultiplier_Insert = 18f;
    private Quaternion m_vizLerpStartRot;
    private float m_vizLerp;
    private bool m_isVizLerping;
    private bool m_isVizLerpInward;
    private Transform m_vizLerpReferenceTransform;

    public float TimeSinceRoundAdded
    {
        get
        {
            return timeSinceRoundAdded;
        }
    }

    public override bool IsInteractable()
    {
        return currentState == MagazineState.Free;
    }

    public bool HasRound()
    {
        return currentAmount > 0 && IsExtractable;
    }

    public bool IsFull()
    {
        return currentAmount >= maxCapacity;
    }

    protected void Awake()
    {
        StaticBulletPositions = new Vector3[StaticBullets.Length];
        for (int index = 0; index < StaticBullets.Length; ++index)
        {
            if (StaticBullets[index] != null)
                StaticBulletPositions[index] = StaticBullets[index].transform.localPosition;
        }
        UpdateStaticBullets();
    }

    public void RemoveRound()
    {
        if (currentAmount > 0)
        {
            --currentAmount;
        }
        UpdateStaticBullets();
    }

    public GameObject RemoveRound(bool b)
    {
        GameObject prefab = bulletPrefab;
        --currentAmount;
        UpdateStaticBullets();
        return prefab;
    }

    public override void BeginGrab(VRHand hand)
    {
        if (currentState == MagazineState.Loaded)
            currentFireArm.EjectMagazine();
        base.BeginGrab(hand);
    }

    public void Release()
    {
        currentState = MagazineState.Free;
        SetParent(null);
        if (UsesVizInterp)
        {
            m_vizLerpStartPos = Viz.transform.position;
            m_vizLerp = 0.0f;
            m_isVizLerpInward = false;
            m_isVizLerping = true;
        }
        if (currentFireArm.magazineEjectPosition != null)
            transform.position = currentFireArm.magazineEjectPosition.position;
        else
            transform.position = currentFireArm.magazineEjectPosition.position;
        if (UsesVizInterp)
        {
            Viz.position = m_vizLerpStartPos;
            m_vizLerpReferenceTransform = currentFireArm.MagazineLoadedPosition;
        }
        AddSavedRigidBody();
        _RB.maxAngularVelocity = 100f;
        _RB.isKinematic = false;
        _RB.velocity = currentFireArm._RB.velocity - transform.up * _ejectSpeed;
        _RB.angularVelocity = currentFireArm._RB.angularVelocity;
        if (currentFireArm.m_grabHand != null)
        {
            bool flag = false;
            VRHand otherHand = currentFireArm.m_grabHand.otherHand;
            if (otherHand.currentGrabPoint == null)
            {
                if (otherHand.input.TriggerPressed)
                    flag = true;
            }
            if (flag)
            {
                Vector3 vector1 = otherHand.transform.position - currentFireArm.MagazineLoadedPosition.position;
                if (Vector3.Distance(transform.position, otherHand.transform.position) < 0.200000002980232 && Vector3.Angle(transform.up, vector1) > 90.0)
                {
                    //otherHand.ForceSetInteractable(this);
                    BeginGrab(otherHand);
                }
            }
        }
        currentFireArm = null;
        if (GetComponent<CapsuleCollider>() != null)
            GetComponent<CapsuleCollider>().enabled = true;
    }

    public void load(VRFirearm loadedfireArm)
    {
        Debug.Log("<color=green> Event: </color> Loaded Magazine: " + gameObject.name);
        currentState = MagazineState.Loaded;
        currentFireArm = loadedfireArm;
        currentFireArm.LoadMagazine(this);
        IsGrabbed = false;
        EndGrab(m_grabHand);
        if (UsesVizInterp)
        {
            m_vizLerpStartPos = Viz.transform.position;
            m_vizLerpStartRot = Viz.transform.rotation;
            m_vizLerp = 0.0f;
            m_isVizLerpInward = true;
            m_isVizLerping = true;
        }
        SetParent(currentFireArm.transform);
        transform.rotation = currentFireArm.GetMagMountPos().rotation;
        transform.position = currentFireArm.GetMagMountPos().position;
        if (UsesVizInterp)
        {
            Viz.position = m_vizLerpStartPos;
            Viz.rotation = m_vizLerpStartRot;
        }
        RemoveRigidBody();
        if (GetComponent<CapsuleCollider>() != null)
            GetComponent<CapsuleCollider>().enabled = false;
    }

    public void UpdateStaticBullets()
    {
        int index1 = currentAmount - 1;
        for (int index2 = 0; index2 < StaticBullets.Length; ++index2)
        {
            if (StaticBullets[index2] != null)
            {
                if (index2 >= currentAmount || index1 < 0)
                {
                    StaticBullets[index2].SetActive(false);
                }
                else
                {
                    StaticBullets[index2].SetActive(true);
                }
            }
            --index1;
        }
        if (currentAmount % 2 == 1)
        {
            for (int index2 = 0; index2 < StaticBullets.Length; ++index2)
            {
                if (StaticBullets[index2] != null)
                    StaticBullets[index2].transform.localPosition = new Vector3(StaticBulletPositions[index2].x * -1f, StaticBulletPositions[index2].y, StaticBulletPositions[index2].z);
            }
        }
        else
        {
            for (int index2 = 0; index2 < StaticBullets.Length; ++index2)
            {
                if (StaticBullets[index2] != null)
                    StaticBullets[index2].transform.localPosition = StaticBulletPositions[index2];
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!UsesVizInterp || !m_isVizLerping)
            return;
        if (!m_isVizLerpInward)
        {
            m_vizLerp += Time.deltaTime * m_vizLerpSpeedMultiplier_Eject;
            if (m_vizLerp >= 1.0)
            {
                m_vizLerp = 1f;
                m_isVizLerping = false;
            }
            Viz.position = Vector3.Lerp(m_vizLerpStartPos, transform.position, m_vizLerp);
            if (m_vizLerpReferenceTransform != null)
                Viz.rotation = Quaternion.Slerp(m_vizLerpReferenceTransform.rotation, transform.rotation, m_vizLerp * m_vizLerp);
            else
                Viz.rotation = Quaternion.Slerp(Viz.rotation, transform.rotation, m_vizLerp);
        }
        else
        {
            m_vizLerp += Time.deltaTime * m_vizLerpSpeedMultiplier_Insert;
            if (m_vizLerp >= 1.0)
            {
                m_vizLerp = 1f;
                m_isVizLerping = false;
            }
            Viz.position = Vector3.Lerp(m_vizLerpStartPos, transform.position, m_vizLerp);
            Viz.rotation = Quaternion.Slerp(m_vizLerpStartRot, transform.rotation, Mathf.Sqrt(m_vizLerp));
        }
    }

    public enum MagazineState
    {
        Loaded,
        Free,
    }
}
