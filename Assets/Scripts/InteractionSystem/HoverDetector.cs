using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverDetector : MonoBehaviour
{
    public GrabPoint m_grapPoint;
    public GameObject m_hoverUI;
    public Vector3 HoverUIScale = new Vector3(0.05f, 0.05f, 0f);

    private int uiScaleTime = 64;
    private int elapsedFrames = 0;

    private void OnTriggerStay(Collider other)
    {
        DoHoverUI(true);
    }

    private void OnTriggerExit(Collider other)
    {
        DoHoverUI(false);
    }

    public void DoHoverUI(bool lerp)
    {

        if (lerp)
        {
            if (m_grapPoint.IsGrabbed)
            {
                m_hoverUI.transform.localScale = Vector3.zero;
                elapsedFrames = 0;
            }

            if (m_hoverUI.transform.localScale == HoverUIScale)
                return;

            float interpolationRatio = (float)elapsedFrames / uiScaleTime;
            Vector3 interpolatedPosition = m_hoverUI.transform.localScale = Vector3.Lerp(m_hoverUI.transform.localScale, HoverUIScale, interpolationRatio); ;
            elapsedFrames = (elapsedFrames + 1) % (uiScaleTime + 1);
        }
        else
        {
            elapsedFrames = 0;
            m_hoverUI.transform.localScale = Vector3.zero;
        }
    }
}
