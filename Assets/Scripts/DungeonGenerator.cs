using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using static System.Collections.Specialized.BitVector32;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon settings")]
    [SerializeField] private Vector2 size;
    [SerializeField, Min(1)] private int maxItineration;
    [SerializeField] private Vector2 minRatio;
    [SerializeField] private int minSectionSize = 1;

    [Header("Room")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase tile_Floor;
    [SerializeField] private TileBase tile_Wall;

    [SerializeField] private List<Rect> rooms = new();

    [Header("Debug")]
    [SerializeField] private bool generateAgain;
    private DungeonSection firstSection;
    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        rooms.Clear();
        tilemap.ClearAllTiles();

        Rect originalSection = new(transform.position.x, transform.position.y, size.x, size.y);
        firstSection = new(originalSection);

        CreateSubSections(firstSection, maxItineration);
        
        CreateRooms(firstSection);
        CreateHallways(firstSection);
    }

    private void CreateHallways(DungeonSection root_Section)
    {
        if(root_Section.Left_Child == null ||  root_Section.Right_Child == null) return;

        Vector2 left_Child_Center = root_Section.Left_Child.Rect.center;
        Vector2 right_Child_Center = root_Section.Right_Child.Rect.center;
        DrawHallway(left_Child_Center, right_Child_Center);

        CreateHallways(root_Section.Left_Child);
        CreateHallways(root_Section.Right_Child);
    }

    private void DrawHallway(Vector2 center_A, Vector2 center_B)
    {
        float distance = Vector2.Distance(center_A, center_B);

        bool isHorizontalLine = center_A.x != center_B.x;
        Vector3Int posToDraw;
        for (int offset = 0; offset <= distance; offset++)
        {
            if(isHorizontalLine)
            {

                //FIXME: Il cast usa floor mentre io nella generazione ho usato round, creerà problemi?
                posToDraw = new((int)center_A.x + offset, (int)center_A.y);

                CheckAndDrawTile(posToDraw, tile_Floor, tile_Floor);

                posToDraw.y++;
                CheckAndDrawTile(posToDraw, tile_Floor, tile_Wall);

                posToDraw.y -= 2;
                CheckAndDrawTile(posToDraw, tile_Floor, tile_Wall);
            }
            else
            {
                //FIXME: Il cast usa floor mentre io nella generazione ho usato round, creerà problemi?
                posToDraw = new((int)center_A.x, (int)center_A.y + offset);

                CheckAndDrawTile(posToDraw, tile_Floor, tile_Floor);

                posToDraw.x++;
                CheckAndDrawTile(posToDraw, tile_Floor, tile_Wall);

                posToDraw.x -= 2;
                CheckAndDrawTile(posToDraw, tile_Floor, tile_Wall);
            }
        }
    }

    private bool CheckAndDrawTile(Vector3Int pos, TileBase tileToAvoid, TileBase tileToDraw)
    {
        TileBase currentTile = tilemap.GetTile((Vector3Int)pos);
        
        if(currentTile == tileToAvoid) return false;

        tilemap.SetTile((Vector3Int)pos, tileToDraw);

        return true;
    }

    private void CreateSubSections(DungeonSection rectToSplit, int currentIteration)
    {
        if (currentIteration == 0) return;

        Rect r1, r2;
        bool isHorizontalSplit = UnityEngine.Random.value < 0.5f;

        if (isHorizontalSplit)
        {
            r1 = new(rectToSplit.Rect.x, rectToSplit.Rect.y, MathF.Floor(UnityEngine.Random.Range(minSectionSize, rectToSplit.Rect.width)), rectToSplit.Rect.height);
            r2 = new(rectToSplit.Rect.x + r1.width, rectToSplit.Rect.y, rectToSplit.Rect.width - r1.width, rectToSplit.Rect.height);
            
            var r1_w_ratio = r1.width / r1.height;
            var r2_w_ratio = r2.width / r2.height;
            if (r1_w_ratio < minRatio.x || r2_w_ratio < minRatio.x)
            {
                CreateSubSections(rectToSplit, currentIteration);
                return;
            }
        }
        else
        {
            r1 = new(rectToSplit.Rect.x, rectToSplit.Rect.y, rectToSplit.Rect.width, MathF.Floor(UnityEngine.Random.Range(minSectionSize, rectToSplit.Rect.height)));
            r2 = new(rectToSplit.Rect.x, rectToSplit.Rect.y +  r1.height, rectToSplit.Rect.width, rectToSplit.Rect.height - r1.height);

            var r1_h_ratio = r1.height / r1.width;
            var r2_h_ratio = r2.height / r2.width;
            if (r1_h_ratio < minRatio.y || r2_h_ratio < minRatio.y)
            {
                CreateSubSections(rectToSplit, currentIteration);
                return;
            }
        }

        DungeonSection left_Child = new(r1);
        DungeonSection right_Child = new(r2);
        rectToSplit.SetChildren(left_Child, right_Child);

        //FIXME: Do i need this?
        //sections.Remove(rectToSplit);
        //sections.Add(r1);
        //sections.Add(r2);

        CreateSubSections(left_Child, currentIteration - 1);
        CreateSubSections(right_Child, currentIteration - 1);
    }

    private void CreateRooms(DungeonSection firstSection)
    {
        List<DungeonSection> bottomChild = new List<DungeonSection>();
        List<DungeonSection> allSections = firstSection.GetSubSections();

        foreach (var subSection in allSections)
        {
            if(subSection.Left_Child == null || subSection.Right_Child == null)
                bottomChild.Add(subSection);
        }

        foreach (var section in bottomChild)
        {
            Rect roomToCreate = new();

            roomToCreate.x = section.Rect.x + Mathf.FloorToInt(UnityEngine.Random.Range(1, section.Rect.width / 3));
            roomToCreate.y = section.Rect.y + Mathf.FloorToInt(UnityEngine.Random.Range(1, section.Rect.height / 3));
            roomToCreate.width = section.Rect.width - (roomToCreate.x - section.Rect.x);
            roomToCreate.height = section.Rect.height - (roomToCreate.y - section.Rect.y);
            roomToCreate.width -= Mathf.FloorToInt(UnityEngine.Random.Range(1, roomToCreate.width / 3));
            roomToCreate.height -= Mathf.FloorToInt(UnityEngine.Random.Range(1, roomToCreate.height / 3));

            rooms.Add(roomToCreate);
            DrawRoom(roomToCreate);
        }
    }

    private void DrawRoom(Rect roomRect)
    {
        Vector2Int bottomLeftCorner = Vector2Int.FloorToInt(roomRect.position);

        // Loop per disegnare il rettangolo
        for (int i = 0; i < roomRect.width; i++)
        {
            for (int j = 0; j < roomRect.height; j++)
            {
                Vector2Int offset = new Vector2Int(i, j);
                if(i == 0 || i == roomRect.width - 1
                    || j == 0 || j == roomRect.height -1)
                    tilemap.SetTile((Vector3Int)(bottomLeftCorner + offset), tile_Wall);
                else    
                    tilemap.SetTile((Vector3Int)(bottomLeftCorner + offset), tile_Floor);
            }
        }

    }

    private void LateUpdate()
    {
        if (generateAgain)
            Generate();

        generateAgain = false;
    }

    private void OnDrawGizmoSelected()
    {
        if (!Application.isPlaying) return;

        foreach (var section in firstSection.GetSubSections())
        {
            Gizmos.color = Color.green;
            Vector3 sectionSize = new(section.Rect.width, section.Rect.height, 0);
            Gizmos.DrawWireCube(section.Rect.center, sectionSize);
        }

        foreach (var room in rooms)
        {
            Gizmos.color = Color.yellow;
            Vector3 sectionSize = new(room.width, room.height, 0);
            Vector3Int roomSize = Vector3Int.FloorToInt(sectionSize);
            Gizmos.DrawWireCube(room.center, roomSize);
        }

        Gizmos.color = Color.red;
        Rect originalSection = new(transform.position.x, transform.position.y, size.x, size.y);
        Vector3 cubeSize = new(originalSection.width, originalSection.height, 0);
        Gizmos.DrawWireCube(originalSection.center, cubeSize);
    }

    [Serializable]
    public class DungeonSection
    {
        private DungeonSection parent;
        private Rect rect;

        private DungeonSection left_Child;
        private DungeonSection right_Child;

        public DungeonSection(Rect rect)
        {
            this.rect = rect;
        }

        public DungeonSection Left_Child { get => left_Child; }
        public DungeonSection Right_Child { get => right_Child; }
        public Rect Rect { get => rect; }

        public void SetChildren(DungeonSection left_Child, DungeonSection right_Child)
        {
            this.left_Child = left_Child;
            this.right_Child = right_Child;

            left_Child.parent = this;
            right_Child.parent = this;
        }

        //FIXME:Serve?
        public List<DungeonSection> GetSubSections(List<DungeonSection> subSections)
        {
            if(this.left_Child == null ||  this.right_Child == null) return null;

            subSections.Add(this.left_Child);
            subSections.Add(this.right_Child);

            this.left_Child.GetSubSections(subSections);
            this.right_Child.GetSubSections(subSections);

            return subSections;
        }

        public List<DungeonSection> GetSubSections()
        {
            List<DungeonSection> subSections = new();
            return GetSubSections(subSections);
        }

    }
}
