using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class ItemScriptableObject : ScriptableObject
    {
        public List<Item> items = new List<Item>();
    }

    public class SlicedHerbScriptableObject : ScriptableObject
    {
        public List<SlicedHerb> slicedHerbs = new List<SlicedHerb>();
    }
}