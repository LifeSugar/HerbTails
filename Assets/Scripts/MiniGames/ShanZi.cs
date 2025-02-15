using System.Collections.Generic;
using UnityEngine;

namespace Herbs
{
    public class ShanZi : MonoBehaviour
    {
        [Header("合起来的扇子")]public Collider shanziCollider;
        [Header("打开的扇子")] public Transform shanziTransform;

        void Start()
        {
            shanziCollider.gameObject.SetActive(true);
            shanziTransform.gameObject.SetActive(false);
        }

        void SwitchShanzi()
        {
            shanziCollider.gameObject.SetActive(!shanziCollider.gameObject.activeInHierarchy);
            shanziTransform.gameObject.SetActive(shanziCollider.gameObject.activeInHierarchy);
        }
    }
}