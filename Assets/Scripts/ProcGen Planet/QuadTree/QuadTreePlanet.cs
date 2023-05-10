using System.Collections;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Represents a planet that is divided into six equal sides each using a quadtree structure.
    /// </summary>
    class QuadTreePlanet
    {
        public Vector3 centre;
        public float radius;
        public QuadTree[] quadTrees;

        public QuadTreePlanet(Vector3 centre, float radius, int maxDepth, float thresholdMultiplier)
        {
            this.centre = centre;
            this.radius = radius;

            // Generates each of the 6 sides for each root node of each quadtree.
            quadTrees = new QuadTree[6];
            for (int i = 0; i < quadTrees.Length; i++)
            {
                RootNodeData side = GetRoodNodeData(i, centre, radius);
                quadTrees[i] = new QuadTree(side.id, side.Centre, side.IAxis, side.JAxis, maxDepth, thresholdMultiplier, this);
            }
        }

        /// <summary>
        /// Represents id, position and orientation for the root node of one of the six QuadTrees.
        /// </summary>
        struct RootNodeData
        {
            public BitArray id;
            public Vector3 Centre;
            public Vector3 IAxis;
            public Vector3 JAxis;
        }

        /// <summary>
        /// Gets the data for the root node of one of the six QuadTrees.
        /// </summary>
        /// <param name="id">Index for generating ID of a quad. Converts it into 3 bits.</param>
        /// <param name="c">Planet centre.</param>
        /// <param name="r">Planet radius.</param>
        /// <returns></returns>
        static RootNodeData GetRoodNodeData(int id, Vector3 c, float r)
        {
            var side = new RootNodeData();
            switch (id)
            {
                case 0:
                    side.id = new BitArray(new bool[] { false, false, false });
                    side.Centre = c + new Vector3(0, 0, -r);
                    side.IAxis = new Vector3(r, 0, 0);
                    side.JAxis = new Vector3(0, r, 0);
                    break;
                case 1:
                    side.id = new BitArray(new bool[] { false, false, true });
                    side.Centre = c + new Vector3(r, 0, 0);
                    side.IAxis = new Vector3(0, 0, r);
                    side.JAxis = new Vector3(0, r, 0);
                    break;
                case 2:
                    side.id = new BitArray(new bool[] { false, true, false });
                    side.Centre = c + new Vector3(0, -r, 0);
                    side.IAxis = new Vector3(0, 0, r);
                    side.JAxis = new Vector3(r, 0, 0);
                    break;
                case 3:
                    side.id = new BitArray(new bool[] { false, true, true });
                    side.Centre = c + new Vector3(0, 0, r);
                    side.IAxis = new Vector3(0, r, 0);
                    side.JAxis = new Vector3(r, 0, 0);
                    break;
                case 4:
                    side.id = new BitArray(new bool[] { true, false, false });
                    side.Centre = c + new Vector3(-r, 0, 0);
                    side.IAxis = new Vector3(0, r, 0);
                    side.JAxis = new Vector3(0, 0, r);
                    break;
                default:
                    side.id = new BitArray(new bool[] { true, false, true });
                    side.Centre = c + new Vector3(0, r, 0);
                    side.IAxis = new Vector3(r, 0, 0);
                    side.JAxis = new Vector3(0, 0, r);
                    break;
            }
            return side;
        }
    }
}