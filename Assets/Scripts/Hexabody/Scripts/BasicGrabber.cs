using HexabodyVR.SampleScene;
using KP;
using UnityEngine;
using UnityEngine.Events;

namespace HexabodyVR.PlayerController
{
    [RequireComponent(typeof(Rigidbody))]
    public class BasicGrabber : MonoBehaviour
    {
        public float Radius = .1f;
        public LayerMask GrabLayer;
        public VRHand Controller;
        public bool Grabbing;
        public Transform Anchor;
        private readonly Collider[] _colliders = new Collider[100];

        public ConfigurableJoint Joint { get; private set; }
        public Rigidbody GrabbedBody { get; private set; }

        public UnityEvent Grabbed = new UnityEvent();
        public UnityEvent Released = new UnityEvent();


        void FixedUpdate()
        {

            if (Controller.input.TriggerDown && !Grabbing)
            {
                var hits = Physics.OverlapSphereNonAlloc(Anchor.position, Radius, _colliders, GrabLayer, QueryTriggerInteraction.Ignore);
                if (hits > 0)
                {
                    Joint = gameObject.AddComponent<ConfigurableJoint>();
                    Joint.xMotion = Joint.yMotion = Joint.zMotion = ConfigurableJointMotion.Locked;
                    Joint.angularXMotion = Joint.angularYMotion = Joint.angularZMotion = ConfigurableJointMotion.Locked;
                    Joint.anchor = transform.InverseTransformPoint(Anchor.position);
                    Joint.autoConfigureConnectedAnchor = false;
                    var hit = _colliders[0];

                    if (!hit.attachedRigidbody)
                    {
                        for (var index = 0; index < hits; index++)
                        {
                            var col = _colliders[index];
                            if (!col)
                                break;
                            if (col.attachedRigidbody)
                            {
                                hit = col;
                                break;
                            }
                        }
                    }

                    if (hit.attachedRigidbody)
                    {
                        Joint.connectedBody = hit.attachedRigidbody;
                        Joint.connectedAnchor = hit.attachedRigidbody.transform.InverseTransformPoint(Anchor.position);
                    }
                    else
                    {
                        Joint.connectedAnchor = Anchor.position;
                    }

                    GrabbedBody = hit.attachedRigidbody;

                    Grabbing = true;
                    Grabbed.Invoke();
                }
            }
            else if (Controller.input.TriggerDown && Grabbing)
            {
                Grabbing = false;
                if (Joint)
                {
                    Destroy(Joint);
                }

                GrabbedBody = null;
                Released.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Anchor.position, Radius);
        }
    }
}