using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    Vector2[] GetNeighbourDirection()
    {
        var x = PlayerMovement.Instance.Horizontal;
        var y = PlayerMovement.Instance.Vertical;

        Vector2[] NeighbourDirection = new Vector2[]
  {
        Vector2.zero,

       new Vector2(0,-y),
       new Vector2(0,y),

        new Vector2(x,y / 2),
        new Vector2(x,-y / 2),

        new Vector2(-x,y / 2),
        new Vector2(-x,-y / 2 ),
  };

        return NeighbourDirection;
    }

    public void OnPlayerEndTurn()
    {

    }
}
