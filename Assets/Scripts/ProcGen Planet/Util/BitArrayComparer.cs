using System.Collections;
using System.Collections.Generic;

namespace ProcGenPlanet
{
    /// <summary>
    /// Provides a mechanism for comparing two <see cref="BitArray"/> objects for equality.
    /// </summary>
    /// <author>Stuart Brown</author>
    public class BitArrayComparer : IEqualityComparer<BitArray>
    {
        /// <inheritdoc />
        public bool Equals(BitArray x, BitArray y)
        {
            if (x == null || y == null) return x == y;

            if (x.Length != y.Length) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        /// <inheritdoc />
        public int GetHashCode(BitArray obj)
        {
            if (obj == null) return 0;

            int hash = 17;

            hash = hash * 31 + obj.Length;

            foreach (bool bit in obj)
            {
                hash = hash * 31 + bit.GetHashCode();
            }

            return hash;
        }
    }
}