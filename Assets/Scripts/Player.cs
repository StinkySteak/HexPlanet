using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public PlayerMovement PlayerMovement;
    public PlayerInput PlayerInput;

    private void Awake()
    {
        Instance = this;
    }
}
