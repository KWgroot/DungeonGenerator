using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class StructureHelper
{
    /// <summary>
    /// Goes through all nodes in the tree graph to return the leaf nodes.
    /// </summary>
    /// <param name="parentNode">Parent node of this node.</param>
    /// <returns></returns>
    public static List<Node> TraverseGraphToExtractLowestLeafes(Node parentNode)
    {
        Queue<Node> nodesToCheck = new Queue<Node>();
        List<Node> listToReturn = new List<Node>();

        if (parentNode.ChildrenNodeList.Count == 0)
        {
            return new List<Node>() { parentNode };
        }

        foreach (Node child in parentNode.ChildrenNodeList)
        {
            nodesToCheck.Enqueue(child);
        }

        while (nodesToCheck.Count > 0)
        {
            Node currentNode = nodesToCheck.Dequeue();
            if (currentNode.ChildrenNodeList.Count == 0)
            {
                listToReturn.Add(currentNode);
            }
            else
            {
                foreach (Node child in currentNode.ChildrenNodeList)
                {
                    nodesToCheck.Enqueue(child);
                }
            }
        }

        return listToReturn;
    }
    /// <summary>
    /// Generates a point in bottom left corner between two floors to generate a corridor.
    /// </summary>
    /// <param name="boundaryLeftPoint">Left point where we can start drawing.</param>
    /// <param name="boundaryRightPoint">Right point where we can start drawing.</param>
    /// <param name="pointModifier">Modifier to make the chosed point more random.</param>
    /// <param name="offset">Offset to the edge.</param>
    /// <returns></returns>
    public static Vector2Int GenerateBottomLeftCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset)
    {
        int minX = boundaryLeftPoint.x + offset;
        int maxX = boundaryRightPoint.x - offset;
        int minY = boundaryLeftPoint.y + offset;
        int maxY = boundaryRightPoint.y - offset;

        return new Vector2Int(
            Random.Range(minX, (int)(minX + (maxX - minX) * pointModifier)),
            Random.Range(minY, (int)(minY + (minY - minY) * pointModifier)));
    }
    /// <summary>
    /// Generates a point in top right corner between two floors to generate a corridor.
    /// </summary>
    /// <param name="boundaryLeftPoint">Left point where we can start drawing.</param>
    /// <param name="boundaryRightPoint">Right point where we can start drawing.</param>
    /// <param name="pointModifier">Modifier to make the chosed point more random.</param>
    /// <param name="offset">Offset to the edge.</param>
    /// <returns></returns>
    public static Vector2Int GenerateTopRightCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset)
    {
        int minX = boundaryLeftPoint.x + offset;
        int maxX = boundaryRightPoint.x - offset;
        int minY = boundaryLeftPoint.y + offset;
        int maxY = boundaryRightPoint.y - offset;

        return new Vector2Int(
            Random.Range((int)(minX + (maxX - minX) * pointModifier), maxX),
            Random.Range((int)(minY + (maxY - minY) * pointModifier), maxY));
    }
    /// <summary>
    /// Middle point between corridor points.
    /// </summary>
    /// <param name="v1">Point one.</param>
    /// <param name="v2">Point two.</param>
    /// <returns></returns>
    public static Vector2Int CalculateMiddlePoint(Vector2Int v1, Vector2Int v2)
    {
        Vector2 sum = v1 + v2;
        Vector2 tempVector = sum / 2;
        return new Vector2Int((int)tempVector.x, (int)tempVector.y);
    }
    /// <summary>
    /// Random X point between corridor points.
    /// </summary>
    /// <param name="v1">Point one.</param>
    /// <param name="v2">Point two.</param>
    /// <returns></returns>
    public static int CalculateXPoint(Vector2Int v1, Vector2Int v2)
    {
        return Random.Range((int)v1.x, (int)v2.x);
    }
    /// <summary>
    /// Random Y point between corridor points.
    /// </summary>
    /// <param name="v1">Point one.</param>
    /// <param name="v2">Point two.</param>
    /// <returns></returns>
    public static int CalculateYPoint(Vector2Int v1, Vector2 v2)
    {
        return Random.Range((int)v1.y, (int)v2.y);
    }
}

public enum RelativePosition
{
    Up,
    Down,
    Right,
    Left
}

public enum Direction
{
    Horizontal,
    Vertical
}