using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFirearm_Round : MonoBehaviour
{
    public VR_RoundCaliber calibre;
    public Rigidbody rb;
    public float ProjectileSpread;
    public GameObject BulletPrefab;
    public int projectileAmount = 1;
    public bool isCasing;
    public bool IsMagazineLoadable;
    public bool IsCaseless;
    private bool isChambered;
    public bool IsManuallyChamberable;
    public Renderer defaultRender;
    public Renderer casingRender;

    [HideInInspector]
    public VRFirearm_Chamber currentChamber;
    [HideInInspector]
    public VRFirearm_Chamber chamberBeingHoveredOver;
    [HideInInspector]
    public VRFirearm_Round currentRound;
    protected SavedRigidBody savedRigidBodyMass = new SavedRigidBody();

    public bool IsChambered
    {
        get
        {
            return isChambered;
        }
    }

    public bool IsCasing
    {
        get
        {
            return isCasing;
        }
    }

    public void Fire()
    {
        if (defaultRender != null)
            defaultRender.enabled = false;
        if (casingRender != null)
            casingRender.enabled = true;
        isCasing = true;
    }

    public void Chamber(VRFirearm_Chamber chamber)
    {
        if (currentChamber != null)
            currentChamber.SetRound(null);
        currentChamber = chamber;
        currentChamber.SetRound(this);
        SetParent(currentChamber.transform);
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.transform.localPosition = Vector3.zero;
            rb.transform.localEulerAngles = Vector3.zero;
            DestroyImmediate(rb);
        }
        GetComponent<Collider>().enabled = false;
        isChambered = true;
        chamberBeingHoveredOver = null;
    }

    public void Eject(Vector3 ejectPosition, Vector3 ejectVelocity, Vector3 ejectAngVelocity)
    {
        GetComponent<Collider>().enabled = true;
        AddSavedRigidBody();
        rb.maxAngularVelocity = 200f;
        rb.isKinematic = false;
        rb.useGravity = true;
        SetParent(null);
        currentChamber = null;
        chamberBeingHoveredOver = null;
        isChambered = false;
        rb.transform.position = ejectPosition;
        rb.velocity = Vector3.Lerp(ejectVelocity * 0.7f, ejectVelocity, Random.value);
        rb.maxAngularVelocity = 200f;
        rb.angularVelocity = Vector3.Lerp(ejectAngVelocity * 0.3f, ejectAngVelocity, Random.value);
        Destroy(gameObject, 10f);
    }

    public void SetParent(Transform t)
    {
        transform.SetParent(t);
    }

    public void AddSavedRigidBody()
    {
        if (rb != null)
            return;
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = savedRigidBodyMass.mass;
        rb.maxAngularVelocity = 100f;
    }

    public struct SavedRigidBody
    {
        public float mass;
    }
}
