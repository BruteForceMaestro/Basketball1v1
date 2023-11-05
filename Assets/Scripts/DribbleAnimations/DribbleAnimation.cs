
using UnityEngine;

namespace Assets.Scripts.DribbleAnimations
{
    abstract class DribbleAnimation
    {
        protected float crossPower;
        protected Animator animator;
        protected Rigidbody rb;
        protected CharacterDribble dribble;

        public void Play(CharacterDribble dribbler)
        {
            Ball.Instance.transform.localPosition = Vector3.zero;
            Ball.Instance.transform.localRotation = Quaternion.identity;


            animator = dribbler.GetComponent<Animator>();
            rb = dribbler.GetComponent<Rigidbody>();
            dribble = dribbler.GetComponent<CharacterDribble>();

            dribbler.InADribbleMove = true;
            crossPower = dribbler.CrossPower;

            Anim(dribbler.gameObject);

            
            dribbler.StartWait();
        }

        protected abstract void Anim(GameObject dribbler);
    }
}
