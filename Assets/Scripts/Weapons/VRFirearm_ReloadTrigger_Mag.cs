using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFirearm_ReloadTrigger_Mag : MonoBehaviour
{
    public VRFirearm_Magazine currentMagazine;

    private void OnTriggerEnter(Collider collider)
    {
        if (!(currentMagazine != null) || !(currentMagazine.currentFireArm == null) || !(collider.gameObject.tag == "FirearmReloadTrigger"))
            return;
        VRFirearm_ReloadTrigger component = collider.gameObject.GetComponent<VRFirearm_ReloadTrigger>();
        if (component == null || component.parentFirearm == null || (component.parentFirearm._loadableMag != currentMagazine.magazineType || component.parentFirearm.EjectDelay > 0.0) || !(component.parentFirearm.currentLoadedMagazine == null))
            return;
        currentMagazine.load(component.parentFirearm);
    }
}
