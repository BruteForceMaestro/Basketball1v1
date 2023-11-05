using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DribbleAnimations
{
    internal class CrossoverAnimation : DribbleAnimation
    {



        protected override void Anim(GameObject dribbler)
        {
            
            dribble.BallInLeftHand = !dribble.BallInLeftHand;

            
            animator.SetTrigger("Crossover");
            

            Vector3 direction = dribble.BallInLeftHand ? -dribbler.transform.right : dribbler.transform.right;
            rb.velocity += direction * crossPower;

        }
    }
}
