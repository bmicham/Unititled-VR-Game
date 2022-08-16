using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KP;

public class VRFirearm : GrabPoint
{
    [Header("Firearm Parameters")]
    public VRFirearm_MagazineType _loadableMag;
    public VRFirearm_Magazine currentLoadedMagazine;
    public Transform MagazineLoadedPosition;
    public Transform magazineEjectPosition;
    public Transform muzzlePosition;
    [Header("Recoil Settings")]
    public float _recoilForceX;
    public float _recoilForceY;
    public float _recoilForceZ;

    private float m_ejectDelay;
    private VRFirearm_Magazine m_lastEjectedMag;

    public float EjectDelay
    {
        get
        {
            return m_ejectDelay;
        }
    }

    public Transform GetMagMountPos()
    {
        return MagazineLoadedPosition;
    }

    protected virtual void Recoil(bool isHeldwithTwoHands)
    {
        _RB.AddForceAtPosition(muzzlePosition.up * _recoilForceY, muzzlePosition.position, ForceMode.Impulse);
        _RB.AddForceAtPosition(muzzlePosition.forward * -_recoilForceZ, muzzlePosition.position, ForceMode.Impulse);
        //_RB.AddForceAtPosition(muzzlePosition.right * _recoilForceX, muzzlePosition.position, ForceMode.Impulse);
    }

    public virtual void Fire(VRFirearm_Chamber chamber, Transform muzzle)
    {
        for (int index = 0; index < chamber.GetRound().projectileAmount; ++index)
        {
            float max = chamber.GetRound().ProjectileSpread * chamber.SpreadRangeModifier.x + chamber.SpreadRangeModifier.y;
            if (chamber.GetRound().BulletPrefab != null)
            {
                GameObject gameObject = Instantiate(chamber.GetRound().BulletPrefab, muzzle.position, muzzle.rotation);
                gameObject.transform.Rotate(new Vector3(Random.Range(-max, max), Random.Range(-max, max), 0.0f));
                VRFirearm_Bullet component = gameObject.GetComponent<VRFirearm_Bullet>();
                component.Fire();
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (m_ejectDelay > 0.0)
            m_ejectDelay -= Time.deltaTime;
        else
            m_ejectDelay = 0.0f;
        if (currentLoadedMagazine)
        {
            if (currentLoadedMagazine.IsGrabbed)
                EjectMagazine();
        }
    }

    public virtual void LoadMagazine(VRFirearm_Magazine mag)
    {
        if (currentLoadedMagazine != null || mag == null)
            return;
        m_lastEjectedMag = null;
        currentLoadedMagazine = mag;
    }

    public virtual void EjectMagazine()
    {
        if (currentLoadedMagazine == null)
            return;
        m_lastEjectedMag = currentLoadedMagazine;
        m_ejectDelay = 0.4f;
        currentLoadedMagazine.Release();
        if (currentLoadedMagazine.m_grabHand != null)
            currentLoadedMagazine = null;
        currentLoadedMagazine = null;
    }

    public void AnimatedComponent(Transform t, float val, MoveType interp, Axis axis)
    {
        switch (interp)
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

    public enum Axis
    {
        X,
        Y,
        Z,
    }
}
