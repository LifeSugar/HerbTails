using System.Collections;
using System.Collections.Generic;
using NPOI.SS.Formula.Functions;
using UnityEngine;
using PixelPerfectURP;

namespace HT
{
    public static class Utility
    {
        public static void DeepCopyItem(Item inItem, Item outItem , bool overwriteCount = true)
        {
            outItem.Name = inItem.Name;
            outItem.Description = inItem.Description;
            outItem.Icon = inItem.Icon;
            outItem.Count = overwriteCount? inItem.Count : 1;
        }

        public static void DeepCopyHerb(Herb inItem, Herb outItem, bool overwriteCount = true)
        {
            outItem.Name = inItem.Name;
            outItem.Description = inItem.Description;
            outItem.Icon = inItem.Icon;
            outItem.Count = overwriteCount? inItem.Count : 1;
            outItem.Weight = inItem.Weight;
            // outItem.CoarseGrinded = inItem.CoarseGrinded;
            // outItem.FineGrinded = inItem.FineGrinded;
            outItem.Prefab = inItem.Prefab;
            
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
        
        
        /// <summary>
        /// 返回与当前屏幕点击（Input.mousePosition）相对应的、左侧透视相机下的正确射线。
        /// 若与 UpScaledCanvas 不相交则返回默认 Ray(0,0,0->0,0,1)。
        /// </summary>
        public static Ray GetCorrectedRay()
        {
            // 1) 取得左右相机、以及 UpScaledCanvas Transform
            Camera leftCam = PixelCameraManager.Instance.GetComponent<Camera>();
            Camera rightCam = PixelCameraManager.Instance.viewCamera.GetComponent<Camera>();
            Transform upScaledCanvas = PixelCameraManager.Instance.upscaledCanvas.transform;
            
            // 2) 从右侧正交相机获取鼠标屏幕坐标对应的射线
            Ray screenRay = rightCam.ScreenPointToRay(Input.mousePosition);
            
            // 3) 与 UpScaledCanvas 所在平面做求交
            //    这里假设 UpScaledCanvas.forward 指向平面法线
            Plane plane = new Plane(upScaledCanvas.forward, upScaledCanvas.position);
            
            float distance;
            if (plane.Raycast(screenRay, out distance))
            {
                // 4) 交点的世界坐标
                Vector3 worldPos = screenRay.GetPoint(distance);
            
                // 5) 生成正确的射线：从 leftCam 的位置，指向刚才的世界坐标
                return new Ray(leftCam.transform.position, worldPos - leftCam.transform.position);
            }
            else
            {
                // 若没与平面相交，返回一个默认 Ray
                return new Ray(Vector3.zero, Vector3.forward);
            
            }
        }
        
        /// <summary>
        /// 根据屏幕坐标，返回「左侧相机」指向场景中相同像素的射线。
        /// 注意：假设左侧相机的渲染结果铺满整个 Screen。
        /// 若有 UI/Canvas 缩放，需要自行做坐标偏移与缩放修正。
        /// </summary>
        public static Ray GetRayFromRealCamScreenPos(Vector2 screenPos)
        {
            // 1) 取得左侧相机
            Camera RealCam = PixelCameraManager.Instance.GetComponent<Camera>();
        
            // 2) 把屏幕坐标转成 [0..1,0..1] 视口坐标
            float u = screenPos.x / Screen.width;
            float v = screenPos.y / Screen.height;
        
            // 3) 用左侧相机的视口坐标转换，得到射线
            return RealCam.ViewportPointToRay(new Vector3(u, v, 0));
        }

        public static UISlot CreateUISlotFromItem(Item item, int count, GridTypes gridType)
        {
            var uiSlot = new UISlot();
            uiSlot.Name = item.Name;
            uiSlot.Icon = item.Icon;
            uiSlot.Count = count;
            uiSlot.GridType = gridType;
            
            return uiSlot;
        }
        
        
        
      
    }
    
    

}