using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    private void Awake()
    {
        Instance = this;
    }

    public float Horizontal = 1;
    public float Vertical = 1;

    [Tooltip("Min Value for transform.x & pointer.x to move X on player")]
    public float MinHorizontalDifference = 0.2f;

    public float MaxPointerPosMagnitude = 3f;

    Vector2 PointerPos;
    [HideInInspector] public Vector3Int PredictedNextPosition;

    public void OnPointer(Vector2 pointerPos)
    {
        PointerPos = CameraController.Instance.Cam.ScreenToWorldPoint(pointerPos);

        var currentPos = transform.position;

        float horizontal = PointerPos.x < currentPos.x ? -1 : 1;
        float vertical = PointerPos.y < currentPos.y ? -1 : 1;

        var diff = Mathf.Abs(PointerPos.x - currentPos.x);

        if (diff >= MinHorizontalDifference)
        {
            Predict(new Vector2(Horizontal * horizontal, vertical * (Vertical / 2)));
            return;
        }

        Predict(new Vector2(0, Vertical * vertical));
    }

    public void OnPointerClick()
    {
        var currentPos = transform.position;

        float horizontal = PointerPos.x < currentPos.x ? -1 : 1;
        float vertical = PointerPos.y < currentPos.y ? -1 : 1;

        var diff = Mathf.Abs(PointerPos.x - currentPos.x);

        if (diff >= MinHorizontalDifference)
        {
            MoveHorizontal(horizontal, vertical);
            return;
        }

        MoveVertical(vertical);
    }

    void MoveVertical(float _verticalAxis)
    {
        MoveBy(new Vector2(0, Vertical * _verticalAxis));
    }
    void MoveHorizontal(float _horizontalAxis, float _verticalAxis)
    {
        MoveBy(new Vector2(Horizontal * _horizontalAxis, _verticalAxis * (Vertical / 2)));
    }
    void MoveBy(Vector2 _axis)
    {
        if (LevelManager.Instance.HasLost || LevelManager.Instance.HasWon)
            return;

        if (LevelManager.Instance.IsMoveCooldown)
            return;

        if (LevelManager.Instance.Fuel <= 0)
        {
            LevelManager.Instance.OnNoFuel("Not enough fuel");
            return;
        }

        var nextPos = (Vector2)transform.position + _axis;
        var tilePos = LevelManager.Instance.Tilemap.WorldToCell(nextPos);
        var tile = LevelManager.Instance.Tilemap.GetTile(tilePos);

        if (tile == null)
            return;

        var spr = LevelManager.Instance.Tilemap.GetSprite(tilePos);
        var data = LevelManager.Instance.GetTileData(spr.name);

        if (data.TileType == LevelManager.TileType.Untravelable)
            return;

        if (data.TileType == LevelManager.TileType.Obstacle)
            if (!LevelManager.Instance.TryMoveToObstacle())
                return;

        transform.position = nextPos;

        LevelManager.Instance.OnMove();
    }
    void Predict(Vector2 _axis)
    {
        var nextPos = (Vector2)transform.position + _axis;
        var tilePos = LevelManager.Instance.Tilemap.WorldToCell(nextPos);
        var tile = LevelManager.Instance.Tilemap.GetTile(tilePos);

        if (tile == null)
            return;

        PredictedNextPosition = tilePos;
        LevelManager.Instance.Highlight((Vector2)transform.position, tilePos);
    }
}
