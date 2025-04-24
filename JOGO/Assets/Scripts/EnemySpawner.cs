using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;           // O prefab do inimigo a ser instanciado
    public Transform[] spawnPoints;          // Locais onde os inimigos podem nascer
    public float spawnInterval = 1f;         // Tempo entre cada spawn
    public float duration = 120f;            // Duração total do spawn (2 minutos)

    private float timer = 0f;
    private float spawnTimer = 0f;

    void Update()
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnEnemy();
                spawnTimer = 0f;
            }
        }
    }

    void SpawnEnemy()
    {
        int index = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity);
    }
}