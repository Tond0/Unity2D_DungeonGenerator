using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Generator_CellarAutomata : Generator
{
    [SerializeField, Range(0,8)] private int birth_threshold;
    [SerializeField, Range(0, 8)] private int survival_threshold;

    [Header("Debug")]
    [SerializeField] private bool wantToEvaluate = false;
    Cell[,] cells;
 
    public override void Generate(Vector2 initialPosition)
    { 
        //Create a new multidimensional array
        cells = new Cell[size.x, size.y];
        Vector2Int initialSnappedPosition = Vector2Int.FloorToInt(initialPosition);

        ApplyRandomNoise(ref cells);
        EvaluateCells(ref cells, maxItineration);

        DrawCells(cells, initialSnappedPosition);
    }

    private void DrawCells(Cell[,] cellsToDraw, Vector2Int startPos)
    {
        //White board!
        tilemap.ClearAllTiles();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector2Int currentPos = startPos + new Vector2Int(i, j);
                TileBase tileToPlace = cellsToDraw[i,j].Status == Cell.CellStatus.Alive ? tile_Wall : tile_Floor;

                tilemap.SetTile((Vector3Int)currentPos, tileToPlace);
            }
        }
    }

    private void ApplyRandomNoise(ref Cell[,] allCells)
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector2Int currentPosition = new(i, j);
                if (currentPosition.x < 0 || currentPosition.y < 0
                        || currentPosition.x >= size.x || currentPosition.y >= size.y)
                {
                    Cell borderCell = new(Cell.CellStatus.Alive);
                    allCells[currentPosition.x, currentPosition.y] = borderCell;
                }

                Cell cellToPlace = Random.value < 0.45f ? new Cell(Cell.CellStatus.Alive) : new Cell(Cell.CellStatus.Dead);
                allCells[i, j] = cellToPlace;
            }
        }
    }

    private void EvaluateCells(ref Cell[,] cellsToEvaluate, int itinerationToLoop)
    {
        for (int itineration = 0; itineration <= itinerationToLoop; itineration++)
        {
            Cell[,] evaluatedCells = new Cell[size.x, size.y];

            for (int i = 1; i < size.x - 1; i++)
            {
                for (int j = 1; j < size.y - 1; j++)
                {
                    Vector2Int posToEvaluate = new(i, j);
                    
                    int aliveNeighbors = GetAliveNeighborCells(cellsToEvaluate, posToEvaluate);

                    Cell cellToEvaluate = cellsToEvaluate[posToEvaluate.x, posToEvaluate.y];

                    Cell evaluatedCell = cellToEvaluate.Evaluate(aliveNeighbors, birth_threshold, survival_threshold);

                    evaluatedCells[i, j] = evaluatedCell;
                }
            }
            cellsToEvaluate = evaluatedCells;
        }
    }

    private int GetAliveNeighborCells(Cell[,] cellsToEvaluate, Vector2Int cellPos)
    {
        int aliveCells = 0;

        for (int i = - 1; i <= 1; i++)
        {
            for (int j = - 1; j <= 1; j++)
            {
                Vector2Int posToCheck = cellPos + new Vector2Int(i, j);

                if (posToCheck.x < 0 || posToCheck.y < 0 || posToCheck.x >= size.x || posToCheck.y >= size.y)
                {
                    Debug.LogError("Should not be here!");
                    aliveCells++;
                    continue;
                }

                Cell currentCell = cellsToEvaluate[posToCheck.x, posToCheck.y];

                if ((currentCell.Status == Cell.CellStatus.Dead) || (i == 0 && j == 0)) continue;

                aliveCells++;
            }
        }

        return aliveCells;
    }

    private void LateUpdate()
    {
        if(wantToEvaluate)
        {
            wantToEvaluate = false;
            EvaluateCells(ref cells, 1);
            Vector2Int initialSnappedPosition = Vector2Int.FloorToInt(transform.position);
            DrawCells(cells, initialSnappedPosition);
        }    
    }

    private struct Cell
    {
        //Alive = wall
        //Dead = floor
        public enum CellStatus { Alive, Dead }
        private CellStatus status;

        public CellStatus Status { get => status; }

        public Cell(CellStatus status)
        {
            this.status = status;
        }

        public Cell Evaluate(int aliveNeighbors, int birth_threshold, int survival_threshold)
        {
            switch (status)
            {
                case CellStatus.Alive:
                    
                    if(aliveNeighbors < survival_threshold)
                        return new(CellStatus.Dead);

                    break;

                case CellStatus.Dead:

                    if(aliveNeighbors > birth_threshold)
                        return new(CellStatus.Alive);

                    break;
            }

            return this;
        }
    }

}
