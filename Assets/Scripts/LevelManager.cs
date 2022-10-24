using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Rendering.DebugUI;
using GUI = ProjectHexa.GUI;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Tilemap Tilemap;
    public double DehighlightDuration = 1;
    public Color PredictColor = Color.yellow;
    [Space]

    public float FogMove = 0.73f;
    public Transform StartFog, EndFog;
    public float ShowTileChance = 80;

    [Space]
    public float InfectedChance = 60;
    public Color InfectedTileColor = Color.red;
    public Vector2 StartVirusTile;
    readonly List<Vector3Int> InfectedTile = new();

    [Space]

    public int Turn = 0;

    public int Fuel = 10;
    public int Food = 10;
    public int Wood = 10;

    [Space]
    public int FuelPerTurn = 1;
    public int FoodPerTurn = 1;
    [Header("Resource Gain")]
    public int WoodPerFuel = 2;
    public int BerryBushFood = 10; // gain
    public int ForestWood = 10;
    [Space]
    public int WaterWoodCost = 3;
    [Space]
    public float ColorLerpSpeed = 5;
    public float ColorRevertLerpSpeed = 5;
    [Space]

    public float PlayerMoveCooldown = 1;

    float PlayerMoveTimer;

    public TileData[] TileDatas;
    public ObstacleData[] ObstacleDatas;
    public ActionData[] ActionDatas;

    public bool HasLost = false;
    public bool HasWon = false;

    public ActionData GetRandomAction => ActionDatas[Random.Range(0, ActionDatas.Length - 1)];

    private List<UnityTileData> UnityTileDatas = new();
    public bool IsMoveCooldown => PlayerMoveTimer > 0;

    public Player Player => Player.Instance;

    public void OnWoodToFuel()
    {
        OnMove(false);
    }

    public void AddFuel(int value)
    {
        Fuel += value;

        if (Fuel < 0)
            Fuel = 0;

        GUI.Instance.OnResourceChanged();
        WorldSpaceUI.Instance.OnResourceChanged(Player.transform.position, "Fuel", value, Turn);
    }
    public void AddFood(int value)
    {
        Food += value;

        GUI.Instance.OnResourceChanged();
        WorldSpaceUI.Instance.OnResourceChanged(Player.transform.position, "Food", value, Turn);
    }
    public void AddWood(int value)
    {
        Wood += value;

        GUI.Instance.OnResourceChanged();
        WorldSpaceUI.Instance.OnResourceChanged(Player.transform.position, "Wood", value, Turn);
    }

    public TileData GetTileData(string sprName)
    {
        foreach (var data in TileDatas)
        {
            if (data.SpriteName == sprName || data.SpriteName.Contains(sprName))
                return data;
        }

        return null;
    }
    public TileData GetTileData(TileType type)
    {
        foreach (var data in TileDatas)
        {
            if (data.TileType == type)
                return data;
        }

        return null;
    }

    [System.Serializable]
    public class TileData
    {
        public string SpriteName;
        public TileType TileType;
        public TileBase Tile;
    }

    [System.Serializable]
    public class ActionData
    {
        public string Title;
        public string Description;

        public Choice[] Choices;


    }
    [System.Serializable]
    public class Choice
    {
        public string Name;
        [TextArea(2, 5)]
        public string Description;

        public int Fuel, Food, Wood;
    }
    public PlayerTurn CurrentPlayerTurn;

    public enum PlayerTurn
    {
        Player, // player is able to move
        Action // 
    }

    public enum TileType
    {
        Basic, // travellable ( plain )
        Resource, // ( berry, forest )
        Untravelable, // not travellable ( mountain )
        Obstacle, // not travellable, until destroyed using resource ( water )
        Action, // ( outpost )
        ResourceDepleted, // ( depleted berry/forest/action )
        TamedObstacle, // bridged-water
        Destination // objective
    }



    [System.Serializable]
    public class ObstacleData
    {
        public string Name;

        public int FuelCost;
        public int FoodCost;
        public int WoodCost;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeTiles();
        SetFog();
    }

    void InitializeTiles()
    {
        UnityTileDatas = Tilemap.GetAllTiles();

        foreach (var data in UnityTileDatas)
            Tilemap.SetTileFlags(data.CellPosition, TileFlags.None);


    }

    /// <summary>
    /// Check if player has the material to cross water
    /// </summary>
    /// <returns></returns>
    public bool TryMoveToObstacle()
    {
        if (Wood >= WaterWoodCost)
        {
            Wood -= WaterWoodCost;
            return true;
        }

        OnNoFuel("Not enough wood");

        return false;
    }

    public void OnChoice(Choice choice)
    {
        GUI.Instance.EnableChoice(false);

        AddFood(choice.Food);
        AddFuel(choice.Fuel);
        AddWood(choice.Wood);

        AudioManager.Instance.PlayAudio("on_choice");

        CurrentPlayerTurn = PlayerTurn.Player;
    }

    void OnAction(Vector3Int tilePos)
    {
        var action = GetRandomAction;

        GUI.Instance.EnableChoice(true);
        GUI.Instance.OnAction(action);

        CurrentPlayerTurn = PlayerTurn.Action;

        Tilemap.SetTile(tilePos, GetTileData(TileType.ResourceDepleted).Tile);

        AudioManager.Instance.PlayAudio("action");
    }
    void OnResource(Vector3Int tilePos)
    {
        if (Tilemap.GetSprite(tilePos).name == "tile_tree")
        {
            AddWood(ForestWood);
        }
        else
        {
            AddFood(BerryBushFood);
        }

        Tilemap.SetTile(tilePos, GetTileData(TileType.ResourceDepleted).Tile);
        Tilemap.SetTileFlags(tilePos, TileFlags.None);
    }
    public void OnMove(bool _consumeResource = true)
    {
        Turn++;

        AudioManager.Instance.PlayAudio("move");

        if (_consumeResource)
        {
            AddFuel(-FuelPerTurn);
            AddFood(-FoodPerTurn);
        }

        if(Food <= 0)
        {
            OnLost("No Food Available");
            return;
        }

        PlayerMoveTimer = PlayerMoveCooldown;

        StartFog.transform.position += Vector3.right * FogMove;

        SetFog();
        SpreadVirus();
        CheckPlayerLost();

        var tilePos = Tilemap.WorldToCell(Player.Instance.transform.position);
        var tile = Tilemap.GetSprite(tilePos);
        var tileData = GetTileData(tile.name);


        if (tileData.TileType == TileType.Resource)
        {
            OnResource(tilePos);
            return;
        }

        if (tileData.TileType == TileType.Obstacle)
        {
            OnObstacle(tilePos, tileData);
            return;
        }

        if (tileData.TileType == TileType.Action)
        {
            OnAction(tilePos);
            return;
        }
        if (tileData.TileType == TileType.Destination)
        {
            OnWin();
            return;
        }
    }

    void OnObstacle(Vector3Int tilePos, TileData data)
    {
        Tilemap.SetTile(tilePos, GetTileData(TileType.TamedObstacle).Tile);
        Tilemap.SetTileFlags(tilePos, TileFlags.None);

        AudioManager.Instance.PlayAudio("wood");
    }
    public void Highlight(Vector2 initialPos, Vector3Int tilePos)
    {
        AddDehighlight(initialPos, tilePos);

        if (Tilemap.GetColor(tilePos) == InfectedTileColor)
            return;

        Tilemap.SetColor(tilePos, Color.Lerp(Tilemap.GetColor(tilePos), PredictColor, Time.deltaTime * ColorLerpSpeed));
    }
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

    List<DehighlightData> DehighlightDatas = new();

    public class DehighlightData
    {
        public Vector2 InitialPos;
        public Vector3Int HighlightedPos;
        public DateTime ActivatedTime;
    }

    private void Update()
    {
        PlayerMoveTimer -= Time.deltaTime;

        foreach (var data in DehighlightDatas)
            Dehighlight(data.InitialPos, data.HighlightedPos);
    }
    public void AddDehighlight(Vector2 initialPos, Vector3Int highlightedPos)
    {
        List<DehighlightData> expiredDataList = new();

        foreach (var dehighlightData in DehighlightDatas)
            if (DateTime.Now >= dehighlightData.ActivatedTime.AddSeconds(DehighlightDuration))
                expiredDataList.Add(dehighlightData);

        foreach (var expiredData in expiredDataList)
            DehighlightDatas.Remove(expiredData);

        if (TryGet(initialPos, out var data))
        {
            data.HighlightedPos = highlightedPos;
            return;
        }

        DehighlightDatas.Add(new DehighlightData() { HighlightedPos = highlightedPos, InitialPos = initialPos, ActivatedTime = DateTime.Now });

        bool TryGet(Vector2 initialPos, out DehighlightData expectedData)
        {
            foreach (var data in DehighlightDatas)
            {
                if (data.InitialPos == initialPos)
                {
                    expectedData = data;
                    return true;
                }
            }
            expectedData = null;
            return false;
        }
    }

    void Dehighlight(Vector2 initialPos, Vector3Int highlightedTilePos)
    {
        foreach (var dir in GetNeighbourDirection())
        {
            var tilePos = Tilemap.WorldToCell(initialPos + dir);

            if (tilePos == highlightedTilePos)
                continue;

            var color = Tilemap.GetColor(tilePos);

            if (color == InfectedTileColor)
                continue;

            Tilemap.SetColor(tilePos, Color.Lerp(color, Color.white, Time.deltaTime * ColorRevertLerpSpeed));
        }
    }

    public void OnNoFuel(string _text)
    {
        WorldSpaceUI.Instance.OnNoFuel(_text);
        OnMove(false);
    }

    void OnLost(string reasonText)
    {
        AudioManager.Instance.PlayAudio("lost");
        GUI.Instance.OnLost(reasonText);
        HasLost = true;
    }
    void OnWin()
    {
        AudioManager.Instance.PlayAudio("win");
        GUI.Instance.OnWin();
        HasWon = true;
    }

    void SpreadVirus()
    {
        if (InfectedTile.Count <= 0)
        {
            var cellPos = Tilemap.WorldToCell(StartVirusTile);
            Tilemap.SetColor(cellPos, InfectedTileColor);


            InfectedTile.Add(cellPos);
            //   print("Virus start at: " + cellPos);
        }

        List<Vector3Int> candidateTile = new();

        foreach (var dir in GetNeighbourDirection())
        {
            foreach (var tile in InfectedTile)
            {
                var pos = Tilemap.CellToWorld(tile);

                if (Random.Range(0, 101) < InfectedChance)
                    continue;

                var candidatePos = Tilemap.WorldToCell((Vector2)pos + dir);



                if (InfectedTile.Contains(candidatePos)) // is infected already
                    continue;

                if (Tilemap.GetSprite(candidatePos) == null) // is tile exist?
                    continue;

                Tilemap.SetColor(candidatePos, InfectedTileColor);

                candidateTile.Add(candidatePos);
            }
        }

        var accepted = 0;

        foreach (var tile in candidateTile)
        {
            if (InfectedTile.Contains(tile))
                continue;

            accepted++;

            InfectedTile.Add(tile);
        }

        //   print($"Candidate: {candidateTile.Count} Accepted: {accepted} Total: {InfectedTile.Count}");

        CheckPlayerLost();
    }
    void CheckPlayerLost()
    {
        var playerTileColor = Tilemap.GetColor(Tilemap.WorldToCell(Player.Instance.transform.position));

        if (playerTileColor == InfectedTileColor)
            OnLost("You are infected!");
    }
    void SetFog()
    {
        var startFogPos = Tilemap.WorldToCell(StartFog.position);
        var endFogPos = Tilemap.WorldToCell(EndFog.position);

        foreach (var data in UnityTileDatas)
            if (data.CellPosition.x > startFogPos.x && data.CellPosition.x < endFogPos.x
                && data.CellPosition.y > startFogPos.y && data.CellPosition.y < endFogPos.y)
                Tilemap.SetColor(data.CellPosition, new Color32(0, 0, 0, 0));
            else
            {
                if (Tilemap.GetColor(data.CellPosition) == new Color32(0, 0, 0, 0))
                {
                    LeanTween.value(Tilemap.GetColor(data.CellPosition).a, 255, 1)
                        .setEaseInQuad()
                        .setOnUpdate((float alpha) => TweenAlpha(data.CellPosition, alpha));
                }
            }

        void TweenAlpha(Vector3Int cellPos, float alpha)
        {
            Tilemap.SetColor(cellPos, new Color32(255, 255, 255, (byte)alpha));
        }
    }

    public void TurnWoodToFuel(int _fuel)
    {

    }
}

public class UnityTileData
{
    public Vector3Int CellPosition;
    public Sprite Sprite;


}
public static class TilemapExtension
{
    public static List<UnityTileData> GetAllTiles(this Tilemap tilemap)
    {
        var bounds = tilemap.cellBounds;

        List<UnityTileData> datas = new();

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var cellPos = new Vector3Int(x, y);
                var sprite = tilemap.GetSprite(cellPos);

                if (sprite == null)
                    continue;

                var tileData = new UnityTileData()
                {
                    CellPosition = cellPos,
                    Sprite = sprite
                };

                datas.Add(tileData);
            }
        }

        return datas;
    }
}