using System.Collections.Generic;
using KP;
using UnityEngine;

public class VRFirearm_Bullet : MonoBehaviour
{
    public bool IsTraveling;
    public float muzzleVelocity;
    public float timeTraveled;
    public float distanceTraveled;
    public float maxDistance;
    public float destroyTime;
    public float m_RicochetAmount;
    public LayerMask layerMask;
    //public GameObject impactVisusal;
    //public GameObject impactSound;
    //public Transform bulletPath;
    //public Vector2 variableTrailLength;
    //public GameObject hitDecal;
    public float m_PenetrationPower;
    public bool ShowDebugLines;

    private List<Vector3> PastPoses = new List<Vector3>();
    private RaycastHit rayHit;
    private RaycastHit exitHit;
    private Vector3 velocity;
    private Rigidbody rb;

    public void Fire()
    {
        IsTraveling = true;
        velocity = muzzleVelocity * transform.forward.normalized;
        UpdateBullet();
    }

    private void Update()
    {
        UpdateBullet();
        DebugPathDraw();
    }

    private void UpdateBullet()
    {
        timeTraveled += Time.deltaTime;
        if (IsTraveling)
        {
            Vector3 position = transform.position;
            Vector3 forwardDir = transform.forward.normalized;
            float m_maxDistance = muzzleVelocity * Time.deltaTime;
            if (Physics.Raycast(position, forwardDir, out rayHit, m_maxDistance, (int)layerMask, QueryTriggerInteraction.Collide))
            {
                Debug.DrawLine(position, rayHit.point, Color.green, 600, false);
                if (!float.IsNaN(rayHit.point.x))
                    transform.position = rayHit.point;
                IsTraveling = false;

                /*
                if (rayHit.transform.CompareTag("Penetrable"))
                {
                    Vector3 secondPosition = rayHit.point - rayHit.normal * (1f / 1000);
                    forwardDir += new Vector3(Random.Range(-m_RicochetAmount, m_RicochetAmount), Random.Range(-m_RicochetAmount, m_RicochetAmount), 0);

                    if (Physics.Raycast(secondPosition, forwardDir, out exitHit, 10f, (int)layerMask, QueryTriggerInteraction.Collide))
                    {
                        Debug.DrawLine(secondPosition, exitHit.point, Color.red, 600, false);
                        if (!float.IsNaN(exitHit.point.x))
                            transform.position = exitHit.point;
                        IsTraveling = false;
                        if (hitDecal != null)
                            Instantiate(hitDecal, exitHit.point, Quaternion.identity);
                        if (impactVisusal != null)
                            Instantiate(impactVisusal, exitHit.point, Quaternion.LookRotation(exitHit.normal));
                        if (impactSound != null)
                            Instantiate(impactSound, exitHit.point, Quaternion.identity);
                        if (exitHit.collider.attachedRigidbody != null)
                            exitHit.collider.attachedRigidbody.AddForceAtPosition(transform.forward * muzzleVelocity * 1f, exitHit.point);
                    }
                }
                
                if (impactVisusal != null)
                    Instantiate(impactVisusal, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                if (impactSound != null)
                    Instantiate(impactSound, rayHit.point, Quaternion.identity);
                */

                if (rayHit.collider.gameObject.GetComponent<RPMTest>()) 
                {
                    var comp = rayHit.collider.gameObject.GetComponent<RPMTest>();
                    comp.Damage();
                }

                else if (rayHit.collider.attachedRigidbody != null)
                    rayHit.collider.attachedRigidbody.AddForceAtPosition(transform.forward * muzzleVelocity * 1f, rayHit.point);
            }
            else
            {
                transform.position += transform.forward * muzzleVelocity * Time.deltaTime;
                distanceTraveled += muzzleVelocity * Time.deltaTime;
            }
            //if (bulletPath != null)
                //bulletPath.localScale = new Vector3(bulletPath.localScale.x, bulletPath.localScale.y, Mathf.Lerp(variableTrailLength.x, variableTrailLength.y, velocity.magnitude / muzzleVelocity));
        }
        else
        {
            Destroy(gameObject);
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.mass = 0.01f;
                rb.isKinematic = false;
                rb.drag = 0.3f;
            }
        }

        if (distanceTraveled >= maxDistance)
            Destroy(gameObject);
        destroyTime -= Time.deltaTime;
        if (destroyTime > 0.0)
            return;
        Destroy(gameObject);

    }

    private void DebugPathDraw()
    {
        if (IsTraveling)
            return;
        for (int index = 1; index < PastPoses.Count; ++index)
            Debug.DrawLine(PastPoses[index], PastPoses[index - 1], Color.green);
    }
}
