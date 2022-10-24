using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : Damageable
    {
        public event Action<Enemy> OnWin;

        [SerializeField]
        private Enemy target;

        [SerializeField]
        [Tooltip("Damage of this instance.")]
        [Min(1)]
        private int damage;

        [SerializeField]
        [Tooltip("Attack count per second.")]
        [Min(0.001f)]
        private float attackRate;

        private Animation damageAnim;

        private NavMeshAgent agent;
        private Animator animator;
        private CancellationTokenSource source;
        private UniTask attackTask;

        public bool HasTaget => target != null;

        public bool IsNearTarget
        {
            get
            {
                if (target == null)
                {
                    return false;
                }

                var distance = (transform.position - target.transform.position).magnitude;
                return distance <= agent.stoppingDistance;
            }
        }

        protected override void OnAwake()
        {
            damageAnim = GetComponent<Animation>();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            base.OnAwake();
        }

        private void Update()
        {
            if (target == null || IsDead) {
                return;
            }


            if (
                IsNearTarget &&
                (
                    attackTask.Status == UniTaskStatus.Succeeded ||
                    attackTask.Status == UniTaskStatus.Canceled
                )
            ) {
                source = new CancellationTokenSource();
                attackTask = AttackCoroutine(source.Token);
            }
            else
            {
                agent.SetDestination(target.transform.position);
            }
        }

        private async UniTask AttackCoroutine(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1 / attackRate), cancellationToken: token);
            if (!token.IsCancellationRequested && !IsDead && IsNearTarget)
            {
                target.Damage(damage);
            }
        }

        public override void Damage(int damage)
        {
            damageAnim?.Play();
            base.Damage(damage);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject == target?.gameObject)
            {
                animator.SetBool("Run", false);
            }
        }

        private void OnTargetDead(Damageable target)
        {
            RemoveTarget();
            OnWin?.Invoke(this);
        }

        protected override void Die()
        {
            RemoveTarget();
            base.Die();
        }

        public override void Restart()
        {
            target = null;
            base.Restart();
        }

        public bool TrySetTarget(Enemy enemy)
        {
            if (enemy == null) {
                return false;
            }

            RemoveTarget();

            target = enemy;
            target.OnDead += OnTargetDead;
            animator?.SetBool("Run", true);
            return true;
        }

        private void RemoveTarget()
        {
            if (HasTaget)
            {
                source?.Cancel();
                target.OnDead -= OnTargetDead;
                target = null;
            }
        }
    }

    

}