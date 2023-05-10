using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a quadtree data structure used for efficient traversal and querying of a 3D environment
    /// represented on a 2D surface. It stores a root node and maintains a maximum depth and threshold distance
    /// multiplier used for updating its structure based on player position. The quadtree can be reset to its
    /// root node, and it can be traversed to obtain all leaf nodes or a list of all QuadNodes in a fully expanded
    /// QuadTree. Additionally, it supports the culling of leaves whose normal vector faces away from the player.
    /// </summary>
    /// <author>Stuart Brown</author>
    class QuadTree
    {
        public QuadNode rootNode;
        int maxDepth;
        float thresholdMultiplier;
        QuadTreePlanet parentPlanet;

        public QuadTree(BitArray id, Vector3 centre, Vector3 uAxis, Vector3 vAxis, int maxDepth, float thresholdMultiplier, QuadTreePlanet parentPlanet)
        {
            rootNode = new QuadNode(id, centre, uAxis, vAxis, 0);
            this.maxDepth = maxDepth;
            this.thresholdMultiplier = thresholdMultiplier;
            this.parentPlanet = parentPlanet;
        }

        /// <summary>
        /// Resets quadtree to just its root node.
        /// </summary>
        public void Reset()
        {
            rootNode = new QuadNode(rootNode.ID, rootNode.Centre, rootNode.UAxis, rootNode.VAxis, 0);
        }

        /// <summary>
        /// Inserts a player position into the quadtree and updates its structure based on a threshold distance.
        /// </summary>
        /// <param name="node">The root node to insert the player position into.</param>
        /// <param name="playerPos">The player's position.</param>
        public void Insert(QuadNode node, Vector3 playerPos)
        {
            var stack = new Stack<QuadNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                QuadNode current = stack.Pop();

                var centreOnSphere = parentPlanet.centre + (current.Centre.normalized * parentPlanet.radius);
                float squareDistance = (centreOnSphere - playerPos).sqrMagnitude;
                float threshold = Mathf.Pow(2, -current.Depth + 1) * parentPlanet.radius;
                threshold *= thresholdMultiplier;

                if (squareDistance < threshold * threshold)
                {
                    if (!current.HasChildren && current.Depth < maxDepth)
                    {
                        current.AddChildren();
                    }
                }
                else if (squareDistance > threshold * threshold * 1.1)
                {
                    if (current.HasChildren)
                    {
                        current.MergeChildren();
                    }
                }

                if (current.HasChildren)
                {
                    stack.Push(current.Children[0]);
                    stack.Push(current.Children[1]);
                    stack.Push(current.Children[2]);
                    stack.Push(current.Children[3]);
                }
            }
        }

        /// <summary>
        /// Traverses the QuadTree from the provided root node and gets all leaf nodes.
        /// </summary>
        /// <param name="node">The root node of the QuadTree to traverse.</param>
        /// <param name="leafNodes">The Dictionary of leaf nodes with ID.</param>
        public void GetAllLeafNodes(QuadNode node, Dictionary<BitArray, QuadNode> leafNodes)
        {
            var stack = new Stack<QuadNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                QuadNode current = stack.Pop();

                if (!current.HasChildren)
                {
                    leafNodes.Add(current.ID, current);
                }
                else
                {
                    stack.Push(current.Children[0]);
                    stack.Push(current.Children[1]);
                    stack.Push(current.Children[2]);
                    stack.Push(current.Children[3]);
                }
            }
        }


        /// <summary>
        /// Culls leaves from the given leaves whose normal vector faces away from the player.
        /// </summary>
        /// <param name="leafNodes">Dictionary containing the leaf nodes to cull.</param>
        /// <param name="playerPosition">Position of the player object.</param>
        public void CullLeaves(Dictionary<BitArray, QuadNode> leafNodes, Vector3 playerPosition)
        {
            var keysToRemove = new List<BitArray>();
            var playerDirection = (playerPosition - parentPlanet.centre).normalized;

            foreach (KeyValuePair<BitArray, QuadNode> leaf in leafNodes)
            {
                var normal = (leaf.Value.Centre - parentPlanet.centre).normalized;

                // If the dot product is less than or equal to -0.707f,
                // the player is located more than 135 degrees away from the node's normal.
                if (Vector3.Dot(normal, playerDirection) <= -0.707f)
                {
                    keysToRemove.Add(leaf.Key);
                }
            }

            foreach (BitArray key in keysToRemove)
            {
                leafNodes.Remove(key);
            }
        }

        /// <summary>
        /// Returns a list of all QuadNodes in a fully expanded QuadTree.
        /// </summary>
        /// <param name="node">The root QuadNode of the QuadTree.</param>
        /// <returns>A list of all QuadNodes in the QuadTree.</returns>
        public List<QuadNode> GetFullQuadTree(QuadNode node)
        {
            var nodes = new List<QuadNode>();
            var stack = new Stack<QuadNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                QuadNode current = stack.Pop();
                nodes.Add(current);

                if (current.Depth < maxDepth)
                {
                    if (!current.HasChildren)
                    {
                        current.AddChildren();
                    }
                    stack.Push(current.Children[0]);
                    stack.Push(current.Children[1]);
                    stack.Push(current.Children[2]);
                    stack.Push(current.Children[3]);
                }
            }
            return nodes;
        }
    }
}