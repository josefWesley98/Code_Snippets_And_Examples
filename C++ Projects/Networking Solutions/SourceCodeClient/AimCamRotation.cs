using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCamRotation : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform aimCamPos;
    private void Update()
    {
        transform.rotation = mainCam.transform.rotation;
        transform.position = aimCamPos.position;
    }
}
