using UnityEngine;
using System;

namespace Game
{
    public class LiquidatedObject : MonoBehaviour
    {
        public event Action<LiquidatedObject> OnLiquidation;
        public event Action<LiquidatedObject> OnRestart;

        [SerializeField]
        private LiquidatedObject originPrefab;

        [SerializeField]
        private int key;

        public int Key => key;

        private void Awake()
        {
            OnAwake();
        }

        public virtual void Liquidate()
        {
            OnLiquidation?.Invoke(this);
        }

        public virtual void Restart()
        {
            OnRestart?.Invoke(this);
        }

        protected virtual void OnAwake()
        {

        }
    }
}

