using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KP
{
    public class VRFirearm_ProxyRound : MonoBehaviour
    {
        public bool IsFull;
        public bool IsSpent;
        public VRFirearm_Round Round;
        public Transform ProxyRound;
        public MeshFilter ProxyMesh;
        public MeshRenderer ProxyRenderer;

        public void Init(Transform t)
        {
            ProxyRound = transform;
            ProxyRound.SetParent(t);
            ProxyRound.localPosition = Vector3.zero;
            ProxyRound.localEulerAngles = Vector3.zero;
            ProxyMesh = gameObject.AddComponent<MeshFilter>();
            ProxyRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        public void UpdateProxyDisplay()
        {
            if (Round == null)
            {
                ProxyMesh.mesh = null;
                ProxyRenderer.material = null;
                ProxyRenderer.enabled = false;
            }
            else
            {
                if (IsSpent)
                {
                    ProxyMesh.mesh = Round.casingRender.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    ProxyRenderer.material = Round.casingRender.sharedMaterial;
                }
                else
                {
                    ProxyMesh.mesh = Round.defaultRender.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    ProxyRenderer.material = Round.defaultRender.sharedMaterial;
                }
                ProxyRenderer.enabled = true;
            }
        }

        public void ClearProxy()
        {
            Round = null;
            IsFull = false;
            IsSpent = true;
            UpdateProxyDisplay();
        }

        public void SetFromPrefabReference(GameObject go)
        {
            Round = go.GetComponent<VRFirearm_Round>();
            IsFull = true;
            IsSpent = false;
            UpdateProxyDisplay();
        }
    }
}