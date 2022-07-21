using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameralook : MonoBehaviour
{
    public Transform Camera;

    private void Update()
    {
        transform.LookAt(Camera);
    }
}
