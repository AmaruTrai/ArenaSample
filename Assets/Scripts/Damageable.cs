using System;
using UnityEngine;

namespace Game
{
    public class Damageable : LiquidatedObject
    {
        public event Action<Damageable> OnDead;

        [SerializeField]
        private int maxHP;

        [SerializeField]
        private int currentHP;
        private bool isDead;

        public bool IsDead => isDead;

        public override void Restart()
        {
            isDead = false;
            currentHP = maxHP;
        }

        protected override void OnAwake()
        {
            isDead = false;
            currentHP = maxHP;
            base.OnAwake();
        }

        protected virtual void Die()
        {
            isDead = true;
            OnDead?.Invoke(this);
            Liquidate();
        }

        public virtual void Damage(int damage)
        {
            if (!isDead) {
                currentHP -= damage;
                if (currentHP <= 0) {
                    Die();
                }
            }
        }
    }

}