using System.Collections;
using System.Collections.Generic;

namespace ProcGenPlanet
{
    /// <summary>
    /// Compares two <see cref="BitArray"/> objects by their length, in descending order.
    /// </summary>
    /// <author>Stuart Brown</author>
    public class BitArrayLengthComparer : IComparer<BitArray>
    {
        /// <inheritdoc />
        public int Compare(BitArray x, BitArray y)
        {
            if (x.Length < y.Length)
            {
                return 1;
            }
            else if (x.Length > y.Length)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}