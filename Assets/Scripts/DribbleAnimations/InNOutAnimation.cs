using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DribbleAnimations
{
    internal class InNOutAnimation : DribbleAnimation
    {
        const float DELAY_BETWEEN_MOVES = 0.1f;
        const float DEGREES_SECOND_MOVE = 60f;
        protected override void Anim(GameObject dribbler)
        {
            animator.SetTrigger("InNOut");

            dribble.StartCoroutine(Movement(dribbler));
        }

        IEnumerator Movement(GameObject dribbler)
        {
            int sign = dribble.BallInLeftHand ? 1 : -1;
            Vector3 direction_firstMove = dribbler.transform.right * sign;

            rb.velocity += direction_firstMove * crossPower / 3;

            yield return new WaitForSeconds(DELAY_BETWEEN_MOVES);

            float radians = (float)(DEGREES_SECOND_MOVE * Math.PI / 180);
            Vector3 direction_secondMove = Vector3.RotateTowards(dribbler.transform.forward, dribbler.transform.right * -sign, radians, 0.0f);

            rb.velocity += direction_secondMove * crossPower;
        }
    }
}
