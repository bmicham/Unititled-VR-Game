using KP;
using UnityEngine;

public class VRFirearm_Chamber : MonoBehaviour
{
    [Tooltip("x = multiplier, y = add/substract")]
    public Vector2 SpreadRangeModifier = new Vector2(1f, 0.0f);
    [Header("Chamber Params")]
    public VR_RoundCaliber calibre;
    public VRFirearm FireArm;
    private VRFirearm_Round m_Round;
    [Header("Chamber State")]
    public bool IsAccessible;
    public bool IsFull;
    public bool IsSpent;
    [Header("Proxy Stuff")]
    public GameObject LoadedPhys;
    public Transform ProxyRound;
    public MeshFilter ProxyMesh;
    public MeshRenderer ProxyRenderer;

    public VRFirearm_Round GetRound()
    {
        return m_Round;
    }

    protected void Awake()
    {
        GameObject gameObject = new GameObject("Proxy");
        ProxyRound = gameObject.transform;
        ProxyRound.SetParent(transform);
        ProxyRound.localPosition = Vector3.zero;
        ProxyRound.localEulerAngles = Vector3.zero;
        ProxyMesh = gameObject.AddComponent<MeshFilter>();
        ProxyRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    public void UpdateProxyDisplay()
    {
        if (m_Round == null)
        {
            ProxyMesh.mesh = null;
            ProxyRenderer.material = null;
            ProxyRenderer.enabled = false;
        }
        else
        {
            if (IsSpent)
            {
                if (m_Round.casingRender != null)
                {
                    ProxyMesh.mesh = m_Round.casingRender.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    ProxyRenderer.material = m_Round.casingRender.sharedMaterial;
                }
                else
                    ProxyMesh.mesh = null;
            }
            else
            {
                ProxyMesh.mesh = m_Round.defaultRender.gameObject.GetComponent<MeshFilter>().sharedMesh;
                ProxyRenderer.material = m_Round.defaultRender.sharedMaterial;
            }
            ProxyRenderer.enabled = true;
        }
    }

    public void SetRound(VRFirearm_Round round)
    {
        if (round != null)
        {
            IsFull = true;
            IsSpent = round.IsCasing;
            m_Round = FireArm.currentLoadedMagazine.roundPrefab;
            if (LoadedPhys != null)
                LoadedPhys.SetActive(true);
        }
        else
        {
            IsFull = false;
            m_Round = null;
            if (LoadedPhys != null)
                LoadedPhys.SetActive(false);
        }
        UpdateProxyDisplay();
    }

    public VRFirearm_Round EjectRound(Vector3 EjectionPosition, Vector3 EjectionVelocity, Vector3 EjectionAngularVelocity)
    {
        if (!(m_Round != null))
            return null;
        VRFirearm_Round component = Instantiate(m_Round.gameObject, EjectionPosition, transform.rotation).GetComponent<VRFirearm_Round>();
        component.rb.velocity = Vector3.Lerp(EjectionVelocity * 0.7f, EjectionVelocity, Random.value);
        component.rb.maxAngularVelocity = 200f;
        component.rb.angularVelocity = Vector3.Lerp(EjectionAngularVelocity * 0.3f, EjectionAngularVelocity, Random.value);
        if (IsSpent)
        {
            component.Fire();
        }
        SetRound(null);
        return component;
    }

    public bool Fire()
    {
        if (!IsFull || !(m_Round != null) || IsSpent)
            return false;
        IsSpent = true;
        UpdateProxyDisplay();
        return true;
    }
}
