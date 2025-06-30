using UnityEngine;

public class ReactorCalculations : MonoBehaviour
{
    public struct CellRod
    {
        public int id;
        public Vector2Int position;
        public bool isWalkable;

        public CellRod(int id, Vector2Int position, bool isWalkable)
        {
            this.id = id;
            this.position = position;
            this.isWalkable = isWalkable;
        }
    }
}
