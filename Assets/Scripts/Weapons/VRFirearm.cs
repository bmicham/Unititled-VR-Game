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

    private float m_ejectDelay;
    private VRFirearm_Magazine m_lastEjectedMag;


}
