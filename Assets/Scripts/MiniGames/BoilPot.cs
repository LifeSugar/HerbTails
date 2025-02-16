using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Herbs
{
    public class BoilPot : MonoBehaviour
    {
        private Animator animator;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void SetAnimationParameter(FirePower firePower)
        {
            switch (firePower)
            {
                case FirePower.NONE:
                    animator.SetFloat("Speed", 0f);
                    break;
                case FirePower.SMALL:
                    animator.SetFloat("Speed", 0.4f);
                    break;
                case FirePower.MIDDLE:
                    animator.SetFloat("Speed", 0.7f);
                    break;
                case FirePower.LARGE:
                    animator.SetFloat("Speed", 1.2f);
                    break;
                default:
                    break;
            }
        }
    }

}