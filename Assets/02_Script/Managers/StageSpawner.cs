using System.Collections.Generic;
using UnityEngine;

namespace LittleSword.Spawn
{
    public class StageSpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int totalEnemyCount = 5;

        [Header("동작 옵션")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool respawnOnDeath = false;
        [SerializeField] private float spawnInterval = 0f;

        [Header("보스 설정")]
        [SerializeField] private GameObject bossPrefab;        // 보스 프리팹
        [SerializeField] private Transform bossSpawnPoint;    // 보스 스폰 위치 (없으면 랜덤)

        private List<GameObject> aliveEnemies = new List<GameObject>();
        private bool bossSpawned = false;

        private void Start()
        {
            if (spawnOnStart) SpawnAll();
        }

        public void SpawnAll()
        {
            aliveEnemies.RemoveAll(e => e == null);
            int needed = totalEnemyCount - aliveEnemies.Count;
            if (spawnInterval > 0f)
                StartCoroutine(SpawnSequence(needed));
            else
                for (int i = 0; i < needed; i++) SpawnOne();
        }

        private System.Collections.IEnumerator SpawnSequence(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnOne();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnOne()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            aliveEnemies.Add(enemy);
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null) return;
            Transform point = bossSpawnPoint != null
                ? bossSpawnPoint
                : spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(bossPrefab, point.position, Quaternion.identity);
            bossSpawned = true;
        }

        private void Update()
        {
            aliveEnemies.RemoveAll(e => e == null);

            // 보스 스폰 체크
            if (!bossSpawned && aliveEnemies.Count == 0 && !respawnOnDeath)
            {
                SpawnBoss();
                return;
            }

            if (!respawnOnDeath) return;
            if (aliveEnemies.Count < totalEnemyCount) SpawnOne();
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            foreach (var p in spawnPoints)
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.4f);

            // 보스 스폰 위치 빨간 구체
            if (bossSpawnPoint != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 0.6f);
            }
        }
    }
}