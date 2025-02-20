using System;
using System.Collections.Generic;
using PixelPerfectURP;
using UnityEngine;
using DG.Tweening;

namespace HT
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
                    shanziCollider.gameObject.transform.parent.gameObject.SetActive(false);
                    posHelper.gameObject.SetActive(true);
                    shanziTransform.position = new Vector3(hit.point.x, shanziHeight, hit.point.z);
                    JianYaoHandler.instance.shanziInHand = true;
                    shanziTransform.gameObject.SetActive(true);
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
                    shanziTransform.Rotate(0f, 180f, 90);
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

        public void Wave(float clickFrequenccy)
        {
            if (clickFrequenccy > 75)
            {
                animator.SetFloat("speed", 3.2f);
            }
            else if (clickFrequenccy > 40)
            {
                animator.SetFloat("speed", 2.4f);
            }
            else if (clickFrequenccy > 20)
            {
                animator.SetFloat("speed", 1.5f);
            }
            else if (clickFrequenccy > 10)
            {
                animator.SetFloat("speed", 1f);
            }
            else if (clickFrequenccy < 10)
            {
                animator.SetFloat("speed", 0.3f);
            }
            
            
        }
    }
}