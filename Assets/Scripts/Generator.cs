using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Generator : MonoBehaviour
{
    [Header("Tilemap settings")]
    [SerializeField] protected Tilemap tilemap;
    [SerializeField] protected TileBase tile_Floor;
    [SerializeField] protected TileBase tile_Wall;

    [Header("Dungeon settings")]
    [SerializeField] protected Vector2Int size = new(50, 50);
    [SerializeField, Min(1)] protected int maxItineration = 3;


    public abstract void Generate(Vector2 center);
}
