using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFirearm_ClosedBolt : VRFirearm
{
    public VRFirearm_ClosedBoltChargeHandle chargeHandle;

    private void Update()
    {
        chargeHandle.UpdateChargingHandle();
    }
}
