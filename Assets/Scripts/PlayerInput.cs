using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
   

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

        }

        if (LevelManager.Instance.CurrentPlayerTurn == LevelManager.PlayerTurn.Action)
            return;

        PlayerMovement.OnPointer(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0) && !ButtonExternal.OnButton)
            PlayerMovement.OnPointerClick();
    }
}
