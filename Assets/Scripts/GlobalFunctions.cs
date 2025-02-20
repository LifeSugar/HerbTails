using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HT
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
        
        public static Item ChangeItemType(Item item, GridTypes gridType, bool overwriteCount = true)
        {
            switch (gridType)
            {
                case GridTypes.HERBS:
                case GridTypes.HERBSINVENTORY:
                {
                    // 创建一个新的 Herb 对象
                    Herb newHerb = new Herb();
                    // 复制基类字段
                    newHerb.Name        = item.Name;
                    newHerb.Description = item.Description;
                    newHerb.Count       = overwriteCount? item.Count : 1;
                    newHerb.Icon        = item.Icon; 
                    return newHerb;
                }
                case GridTypes.GRINDEDHERBS:
                {
                    GrindedHerb newGrinded = new GrindedHerb();
                    newGrinded.Name        = item.Name;
                    newGrinded.Description = item.Description;
                    newGrinded.Count       = overwriteCount? item.Count : 1;
                    newGrinded.Icon        = item.Icon; 
                    // ...
                    return newGrinded;
                }
                case GridTypes.MEDICINES:
                {
                    Medicine newMedicine   = new Medicine();
                    newMedicine.Name       = item.Name;
                    newMedicine.Description = item.Description;
                    newMedicine.Count       = overwriteCount? item.Count : 1;
                    newMedicine.Icon        = item.Icon;
                    // ...
                    return newMedicine;
                }
                default:
                    return item; // 如果不需要转换就返回原对象
            }
            
           
        }
        /// <summary>
        /// 返回一个新的 UISlot 对象，其属性复制自 source
        /// </summary>
        public static UISlot DeepCopyUISlot(UISlot source, bool overwriteCount = true)
        {
            return new UISlot
            {
                Name = source.Name,
                Icon = source.Icon,
                Count =overwriteCount? source.Count : 1,
                GridType = source.GridType
            };
        }
    
        /// <summary>
        /// 将 source 的所有属性深拷贝到 target 中
        /// </summary>
        public static void DeepCopyUISlot(UISlot source, UISlot target, bool overwriteCount = true , bool overwriteType = false)
        {
            target.Name = source.Name;
            target.Icon = source.Icon;
            target.Count =overwriteCount? source.Count : 1;
            target.GridType =overwriteType? source.GridType : target.GridType;
        }
        
      
    }
    
    

}