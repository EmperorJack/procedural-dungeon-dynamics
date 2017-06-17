using UnityEngine;
using System.Collections;

namespace DungeonGeneration
{

    public abstract class PipelineComponent : MonoBehaviour
    {
        public abstract void ChangeValue(string targetField, float value);
    }
}