using System;
using System.Collections.Generic;
using PixelPerfectURP;
using UnityEngine;
using DG.Tweening;

namespace Herbs
{
    public class ShanZi : MonoBehaviour
    {
        [Header("合起来的扇子")]public Collider shanziCollider;
        [Header("打开的扇子")] public Transform shanziTransform;
        [Header ("打开扇子的高度")] public float shanziHeight;
        [Header ("扇子指向")] public Transform shanziTarget;
        [Header ("确定扇子位置的collider")] public Collider posHelper;
        
        public Animator animator;
        
        private Vector3 originalPos;
        private Quaternion originalRot;

        void Start()
        {
            posHelper.gameObject.SetActive(false);
            shanziHeight = shanziTransform.position.y;
            originalPos = shanziTransform.position;
            originalRot = shanziTarget.rotation;
            shanziCollider.gameObject.SetActive(true);
            shanziTransform.gameObject.SetActive(false);
        }
        

        public void SwitchShanzi()
        {
            
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0) && !JianYaoHandler.instance.shanziInHand)
            {
                Ray ray = PixelCameraManager.Instance.viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 999f, Color.red);
                LayerMask mask = 1 << 9;

                if (Physics.Raycast(ray, out hit, mask))
                {
                    shanziTransform.gameObject.SetActive(true);
                    shanziCollider.gameObject.transform.parent.gameObject.SetActive(false);
                    posHelper.gameObject.SetActive(true);
                    JianYaoHandler.instance.shanziInHand = true;
                }
            }
            else if (JianYaoHandler.instance.shanziInHand)
            {
                Ray ray = PixelCameraManager.Instance.viewCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 999f, Color.red);
                LayerMask mask = 1 << 9;
                if (Physics.Raycast(ray, out hit, mask))
                {
                    shanziTransform.position = new Vector3(hit.point.x, shanziHeight, hit.point.z);
                    shanziTransform.LookAt(shanziTarget);
                    shanziTransform.Rotate(0, 180f, 90);
                }

                if (Input.GetMouseButtonDown(1) && JianYaoHandler.instance.shanziInHand)
                {
                    shanziTransform.gameObject.SetActive(false);
                    shanziTransform.position = originalPos;
                    shanziCollider.gameObject.transform.parent.gameObject.SetActive(true);
                    posHelper.gameObject.SetActive(false);
                    JianYaoHandler.instance.shanziInHand = false;
                }
            }
            
        }

        public void Wave(float currency)
        {
            if (currency > 40)
            {
                animator.SetFloat("speed", 3f);
            }
            else if (currency > 20)
            {
                animator.SetFloat("speed", 2f);
            }
            else if (currency > 10)
            {
                animator.SetFloat("speed", 1f);
            }
            else
            {
                animator.SetFloat("speed", 0f);
            }
            
            
        }
    }
}