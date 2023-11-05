using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DribbleAnimations
{
    internal class PullbackAnimation : DribbleAnimation
    {

        protected override void Anim(GameObject dribbler)
        {
            animator.SetTrigger("Pullback");
            dribble.BallInLeftHand = !dribble.BallInLeftHand;

            dribbler.transform.LookAt(new Vector3(SceneObjectsReferences.RimPosition.position.x, dribbler.transform.position.y, SceneObjectsReferences.RimPosition.position.z));

            Vector3 direction = -dribbler.transform.forward;
            rb.velocity = direction * crossPower;

        }
    }
}
