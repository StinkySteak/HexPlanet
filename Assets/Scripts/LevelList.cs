using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelList : MonoBehaviour
{
    public LevelData[] Levels;

    [System.Serializable]
    public class LevelData
    {
        public GameObject Tilemap;
        public Vector2 StartPlayerPosition;
        public Vector2 StartVirusPosition;
        [Space]
        public int InitialFuel;
        public int InitialFood;
        public int InitialWood;
    }
}
