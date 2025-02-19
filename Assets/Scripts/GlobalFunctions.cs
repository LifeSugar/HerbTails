using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Herbs
{
    public static class GlobalFunctions
    {
        public static void DeepCopyItem(Item inItem, Item outItem , bool overwriteCount = true)
        {
            outItem.Name = inItem.Name;
            outItem.Description = inItem.Description;
            outItem.Icon = inItem.Icon;
            outItem.Count = overwriteCount? inItem.Count : 1;
        }
    }

}