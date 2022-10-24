using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game
{

    public class ArenaManager : MonoBehaviour
    {
        private HashSet<Enemy> activeEnemies;
        private LiquidatedObjectPool pool;

        [SerializeField]
        private List<Transform> spawnPoints;

        public void Spawn(Enemy prefab)
        {
            var enemy = pool.Get(prefab) as Enemy;
            if (enemy != null)
            {
                enemy.Restart();
                var point = spawnPoints[Random.Range(0, spawnPoints.Count)];
                enemy.transform.position = point.position;
                activeEnemies.Add(enemy);
                enemy.gameObject.SetActive(true);
                SetNewParticipant(enemy);
            }
        }

        [Inject]
        public void Construct(LiquidatedObjectPool pool)
        {
            this.pool = pool;
            pool.OnLiquidatedObjectCreated += (obj) =>
            {
                if (obj is Enemy enemy)
                {
                    Append(enemy);
                }
            };
        }


        private void Awake()
        {
            activeEnemies = new HashSet<Enemy>(FindObjectsOfType<Enemy>());

            foreach (var participant in activeEnemies)
            {
                Append(participant);
                pool.RegistrateObject(participant);
                if (!participant.HasTaget)
                {
                    SetNewParticipant(participant);
                }
            }
        }

        private void Append(Enemy obj)
        {
            obj.OnWin += SetNewParticipant;
            obj.OnDead += RemoveParticipant;
        }

        private void RemoveParticipant(Damageable participant)
        {
            if (participant is Enemy enemy){
                activeEnemies.Remove(enemy);
            }
        }

        private void SetNewParticipant(Enemy participant)
        {
            Enemy newTarget = null;
            NavMeshPath savedPath = null;
            foreach (var target in activeEnemies)
            {
                if (target != participant && !target.IsDead && !target.HasTaget)
                {
                    NavMeshPath path = new NavMeshPath();

                    if (
                        NavMesh.CalculatePath(participant.transform.position, target.transform.position, 1, path)
                    )
                    {
                        if (newTarget == null)
                        {
                            newTarget = target;
                            savedPath = path;
                        } else if (
                            PathLength(path) < PathLength(savedPath)
                        )
                        {
                            newTarget = target;
                            savedPath = path;
                        }
                    }
                }
            }

            if (newTarget != null)
            {
                participant.TrySetTarget(newTarget);
                newTarget.TrySetTarget(participant);
            }
        }

        private float PathLength(NavMeshPath path)
        {
            if (path.corners.Length < 2)
                return 0;

            Vector3 previousCorner = path.corners[0];
            float lengthSoFar = 0.0F;
            int i = 1;
            while (i < path.corners.Length)
            {
                Vector3 currentCorner = path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
            return lengthSoFar;
        }

        public void CloseGame()
        {
            Application.Quit();
        }
    }
}
