using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    public Transform Background;
    public float ParallaxEffect = -10;

    Vector3 NextPos;

    private void Start()
    {
        NextPos = transform.position;
    }

    private void Update()
    {
     //   var playerPos = transform.position;

        NextPos = new Vector3(
            CameraController.Instance.Cam.transform.position.x * ParallaxEffect, 
            CameraController.Instance.Cam.transform.position.y, 0);

        Background.position = NextPos;
    }
}
