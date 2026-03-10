using System;
using UnityEngine;

namespace Modules.Utils
{
    public sealed class TransformMover : MonoBehaviour
    {
        private float startPositionY;
        private float endPositionY;
        private float movingSpeedY;
        private float positionX;
        private float positionZ;

        private Transform myTransform;

        [SerializeField]
        private Params m_params;

        private void Awake()
        {
            this.startPositionY = this.m_params.m_startPositionY;
            this.endPositionY = this.m_params.m_endPositionY;
            this.movingSpeedY = this.m_params.m_movingSpeedY;
            this.myTransform = this.transform;
            var position = this.myTransform.position;
            this.positionX = position.x;
            this.positionZ = position.z;
        }

        private void FixedUpdate()
        {
            if (this.myTransform.position.y <= this.endPositionY)
            {
                this.myTransform.position = new Vector3(
                    this.positionX,
                    this.startPositionY,
                    this.positionZ
                );
            }

            this.myTransform.position -= Vector3.up * this.movingSpeedY * Time.fixedDeltaTime;
        }

        [Serializable]
        public sealed class Params
        {
            [SerializeField]
            public float m_startPositionY;

            [SerializeField]
            public float m_endPositionY;

            [SerializeField]
            public float m_movingSpeedY;
        }
    }
}