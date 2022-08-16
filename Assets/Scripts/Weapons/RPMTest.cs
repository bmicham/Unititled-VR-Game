using System.Collections.Generic;
using UnityEngine;

public class RPMTest : MonoBehaviour
{
    public List<float> ShotTimes = new List<float>();
    public float RPM;

    public void Update()
    {
        if (Input.GetKey(KeyCode.R))
            ShotTimes.Clear();
    }

    public void Damage()
    {
        ShotTimes.Add(Time.time);
        UpdateROF();
    }

    public void UpdateROF() => RPM = ShotTimes.Count / (ShotTimes[ShotTimes.Count - 1] - ShotTimes[0]) * 60f;
}
