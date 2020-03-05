using UnityEngine;
using MLAgents;
using KartGame.Track;
using KartGame.KartSystems;

namespace KartGame.ML
{

    /// <summary>
    /// this class implements the agent for the Kart.
    /// it also implements a IInput interface to control the movement of the kart
    /// </summary>
    public class KartAgent : Agent, IInput
    {
        Rigidbody rBody;
        TrackManager m_trackManager;
        [RequireInterface(typeof(IMovable))]
        public Object movable;
        public GameObject trackManager;
        IRacer racer;
        KartMovement m_movable;
        bool isFirst;
        float lastDistanceToTarget;
        void Start()
        {
            rBody = GetComponent<Rigidbody>();
            m_movable = (KartMovement)movable;
            m_trackManager = trackManager.GetComponent<TrackManager>();
            racer = GetComponent<IRacer>();

            // IInput relevant Params
            m_Acceleration = 0;
            m_Steering = 0;
            m_BoostPressed = false;
            m_FirePressed = false;
            isFirst = true;
        }

        public override void AgentReset()
        {
            if(!isFirst)
            {
                m_trackManager.SetRacerToFirstPos(racer);
                m_trackManager.ReplaceMovable(m_movable);
            }
            isFirst = false;
            
        }

        public void KartHit()
        {
            Done();
            AgentReset();
        }

        
        public override void CollectObservations()
        {
            AddVectorObs(rBody.velocity.x);
            AddVectorObs(rBody.velocity.z);
            var checkpoint = m_trackManager.GetCheckpoint(racer);
            lastDistanceToTarget = Vector3.Distance(transform.position,
                                                   checkpoint.transform.position);
            AddVectorObs(lastDistanceToTarget);
        }

        private float eps = 0.3f;
        public override void AgentAction(float[] vectorAction)
        {
            if (m_movable.Position.z > 192 || m_movable.Position.z < -48 || m_movable.Position.x < -169 || m_movable.Position.x > 21)
                KartHit();
            if (vectorAction[0] > 0 + eps)
                m_Acceleration = 1f;
            else if (vectorAction[0] < 0 - eps)
                m_Acceleration = -1f;
            else
                m_Acceleration = 0f;
            m_Steering = vectorAction[1];
            m_HopPressed = vectorAction[2] > 0.8;
            AddReward(m_movable.LocalSpeed * 0.001f);

        }

        public void OnCheckPointReached(IRacer racer, Checkpoint checkpoint)
        {
            if (this.racer.Equals(racer))
            {
                Debug.Log(string.Format("checkpointName={0}", racer.GetName()));
                AddReward(2);
            }
        }

        public void OnDrift()
        {
            if (this.racer.Equals(racer))
            {
                Debug.Log(string.Format("checkpointName={0}", racer.GetName()));
                AddReward(m_movable.LocalSpeed*0.05f);
            }
        }

        public override float[] Heuristic()
        {
            var action = new float[3];
            if (Input.GetKey(KeyCode.UpArrow))
                action[0] = 1f;
            else if (Input.GetKey(KeyCode.DownArrow))
                action[0] = -1f;
            else
                action[0] = 0f;

            if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                action[1] = -1f;
            else if (!Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                action[1] = 1f;
            else
                action[1] = 0f;

            if (Input.GetKey(KeyCode.Space))
                action[2] = 1f;
            else
                action[2] = 0f;

            return action;
        }


        #region IInput
        public float Acceleration
        {
            get { return m_Acceleration; }
        }
        public float Steering
        {
            get { return m_Steering; }
        }
        public bool BoostPressed
        {
            get { return m_BoostPressed; }
        }
        public bool FirePressed
        {
            get { return m_FirePressed; }
        }
        public bool HopPressed
        {
            get { return m_HopPressed; }
        }
        public bool HopHeld
        {
            get { return false; }
        }

        

        float m_Acceleration;
        float m_Steering;
        bool m_BoostPressed;
        bool m_FirePressed;
        bool m_HopPressed;

        #endregion

    }
}