using System;
using UnityEngine;

namespace ProcGenPlanet
{
    /// <summary>
    /// Attribute used to conditionally hide fields in the Unity inspector based on the value of another field.
    /// Used in <see cref="PlanetEditor"/>.
    /// </summary>
    /// <remarks>
    /// Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com).
    /// Modified by: Sebastian Lague and used by Stuart Brown.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        public string conditionalSourceField;
        public int enumIndex;

        public ConditionalHideAttribute(string boolVariableName)
        {
            conditionalSourceField = boolVariableName;
        }

        public ConditionalHideAttribute(string enumVariableName, int enumIndex)
        {
            conditionalSourceField = enumVariableName;
            this.enumIndex = enumIndex;
        }
    }
}