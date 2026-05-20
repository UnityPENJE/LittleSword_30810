using Unity.Netcode;
using UnityEngine;

namespace LittleSword.Network
{
    public class NetworkGameManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += SpawnEnemies;
        }

        private void SpawnEnemies()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            foreach(var point in spawnPoints)
            {
                var enemy = Instantiate(enemyPrefab, point.position, point.rotation);

                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}

