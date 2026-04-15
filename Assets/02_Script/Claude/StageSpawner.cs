using System.Collections.Generic;
using UnityEngine;

namespace LittleSword.Spawn
{
    // ============================================================
    // StageSpawner: 지정된 위치(GameObject)에 적을 스폰하는 스테이지 매니저
    // ============================================================
    // 사용법:
    //   1. 빈 GameObject 만들고 이 스크립트 붙이기
    //   2. spawnPoints 배열에 위치용 빈 GameObject들 드래그
    //   3. enemyPrefabs 배열에 적 프리팹 드래그
    //   4. totalEnemyCount 만큼 게임 시작 시 자동 스폰
    //
    // 옵션:
    //   - respawnOnDeath: 적이 죽을 때마다 totalEnemyCount 만큼 다시 채움 (무한 모드)
    //   - spawnInterval: 스폰 간격(초). 0이면 한꺼번에 스폰
    // ============================================================
    public class StageSpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        [SerializeField] private GameObject[] enemyPrefabs;   // 적 프리팹 후보들
        [SerializeField] private Transform[] spawnPoints;     // 스폰 위치 GameObject들
        [SerializeField] private int totalEnemyCount = 5;     // 동시에 유지할 적 수

        [Header("동작 옵션")]
        [SerializeField] private bool spawnOnStart = true;    // Start에서 자동 스폰
        [SerializeField] private bool respawnOnDeath = false; // 죽으면 다시 채울지
        [SerializeField] private float spawnInterval = 0f;    // 스폰 간격(0이면 즉시)

        // 현재 살아있는 적 목록
        private List<GameObject> aliveEnemies = new List<GameObject>();

        private void Start()
        {
            if (spawnOnStart) SpawnAll();
        }

        // 부족한 만큼 적 스폰
        public void SpawnAll()
        {
            // null 정리 (이미 파괴된 적 제거)
            aliveEnemies.RemoveAll(e => e == null);

            int needed = totalEnemyCount - aliveEnemies.Count;
            if (spawnInterval > 0f)
                StartCoroutine(SpawnSequence(needed));
            else
                for (int i = 0; i < needed; i++) SpawnOne();
        }

        // 일정 간격으로 순차 스폰
        private System.Collections.IEnumerator SpawnSequence(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnOne();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // 적 하나 스폰
        private void SpawnOne()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            aliveEnemies.Add(enemy);
        }

        // respawnOnDeath용: 일정 주기로 살아있는 수 체크 후 보충
        private void Update()
        {
            if (!respawnOnDeath) return;

            // 죽은 적(파괴된 오브젝트) 제거
            aliveEnemies.RemoveAll(e => e == null);

            // 부족하면 즉시 1마리만 추가 스폰 (한꺼번에 몰리는 거 방지)
            if (aliveEnemies.Count < totalEnemyCount) SpawnOne();
        }

        // Scene 뷰에서 스폰 위치 시각화 (초록 구체)
        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            foreach (var p in spawnPoints)
            {
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.4f);
            }
        }
    }
}
