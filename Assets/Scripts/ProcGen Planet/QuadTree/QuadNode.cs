using System.Collections;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a node in a quadtree.
    /// </summary>
    /// <author>Stuart Brown</author>
    class QuadNode
    {
        public BitArray ID { get; }
        public Vector3 Centre { get; }
        public Vector3 UAxis { get; }
        public Vector3 VAxis { get; }

        public int Depth { get; }
        public bool HasChildren { get; private set; }
        public QuadNode[] Children { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadNode"/> class.
        /// </summary>
        /// <param name="id">The ID of the node.</param>
        /// <param name="centre">The center point of the node.</param>
        /// <param name="uAxis">The vector representing the local U axis of the node.</param>
        /// <param name="vAxis">The vector representing the local V axis of the node.</param>
        /// <param name="depth">The depth of the node in the quadtree.</param>
        public QuadNode(BitArray id, Vector3 centre, Vector3 uAxis, Vector3 vAxis, int depth)
        {
            ID = id;
            Centre = centre;
            UAxis = uAxis;
            VAxis = vAxis;
            Depth = depth;
        }

        /// <summary>
        /// Adds four child nodes to this node.
        /// </summary>
        public void AddChildren()
        {
            Children = new QuadNode[4];
            Children[0] = new QuadNode(GenerateID(false, false), Centre - (UAxis / 2) - (VAxis / 2), UAxis / 2, VAxis / 2, Depth + 1);
            Children[1] = new QuadNode(GenerateID(false, true), Centre + (UAxis / 2) - (VAxis / 2), UAxis / 2, VAxis / 2, Depth + 1);
            Children[2] = new QuadNode(GenerateID(true, false), Centre - (UAxis / 2) + (VAxis / 2), UAxis / 2, VAxis / 2, Depth + 1);
            Children[3] = new QuadNode(GenerateID(true, true), Centre + (UAxis / 2) + (VAxis / 2), UAxis / 2, VAxis / 2, Depth + 1);
            HasChildren = true;
        }

        /// <summary>
        /// Removes the child nodes of this node.
        /// </summary>
        public void MergeChildren()
        {
            if (!HasChildren)
                return;
            Children = null;
            HasChildren = false;
        }

        /// <summary>
        /// Generates a unique identifier for a child node by copying the current node's ID 
        /// and adding two bits to represent the child's location.
        /// The most significant bit represents whether the child is in the "top" half of the node, 
        /// and the least significant bit represents whether the child is in the "right" half of the node.
        /// </summary>
        /// <param name="moreSigBit">The value of the most significant bit of the child's ID.</param>
        /// <param name="lessSigBit">The value of the least significant bit of the child's ID.</param>
        /// <returns>A new BitArray representing the child's unique identifier.</returns>
        BitArray GenerateID(bool moreSigBit, bool lessSigBit)
        {
            var childID = new BitArray(ID);
            childID.Length += 2;
            childID.Set(childID.Count - 2, moreSigBit);
            childID.Set(childID.Count - 1, lessSigBit);
            return childID;
        }
    }
}