using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public Camera Cam;

    public float LerpSpeed = 5;

    public Vector3 Offset;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        var targetPos = Player.Instance.transform.position;

        Cam.transform.position = Vector2.Lerp(Cam.transform.position, targetPos, Time.deltaTime * LerpSpeed);
        Cam.transform.position += Offset;
    }


}
