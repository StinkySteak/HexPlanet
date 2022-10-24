using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    public Player Player;

    public Transform Target;

    public float LerpSpeed = 5;

    private void Update()
    {
        transform.position = Vector2.Lerp(transform.position, Target.transform.position, Time.deltaTime * LerpSpeed);
    }
}
