using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    private Coroutine waveRoutine;

    [SerializeField]
    private List<GameObject> enemyPrefabs; // 생성할 적 프리팹 리스트

    [SerializeField]
    private List<Rect> spawnAreas; // 적을 생성할 영역 리스트

    [SerializeField]
    private Color gizmoColor = new Color(1, 0, 0, 0.3f); // 기즈모 색상

    private List<EnemyController> activeEnemies = new List<EnemyController>(); // 현재 활성화된 적들

    private bool enemySpawnComplite;

    [SerializeField] private float timeBetweenSpawns = 0.2f;
    [SerializeField] private float timeBetweenWaves = 1f;
    [SerializeField] private int maxPerType = 10;

    private Dictionary<EnemyController, int> typeByEnemy = new Dictionary<EnemyController, int>();
    private Dictionary<int, int> createdCount = new Dictionary<int, int>();

    GameManager gameManager;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void StartWave(int waveCount)
    {
        if (waveCount <= 0)
        {
            gameManager.EndOfWave();
            return;
        }

        if (waveRoutine != null)
            StopCoroutine(waveRoutine);
        waveRoutine = StartCoroutine(SpawnWave(waveCount));
    }

    public void StopWave()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnWave(int waveCount)
    {
        enemySpawnComplite = false;

        // 웨이브 시작: 동시 존재 카운트 초기화
        createdCount.Clear();

        yield return new WaitForSeconds(timeBetweenWaves);

        // 종류별로 maxPerType(기본 10)까지 스폰
        for (int type = 0; type < enemyPrefabs.Count; type++)
        {
            int already = 0;
            createdCount.TryGetValue(type, out already);
            int toSpawn = Mathf.Max(0, maxPerType - already);

            for (int i = 0; i < toSpawn; i++)
            {
                bool ok = SpawnEnemyOfType(type);

                if (timeBetweenSpawns > 0f)
                    yield return new WaitForSeconds(timeBetweenSpawns);

                if (!ok) break;
            }
        }

        enemySpawnComplite = true;
    }

    private bool SpawnEnemyOfType(int prefabIndex)
    {
        int alive = 0;
        createdCount.TryGetValue(prefabIndex, out alive);
        if (alive >= maxPerType) return false; // 이 종류는 동시에 10마리까지

        // 랜덤 스폰 위치
        Rect area = spawnAreas[Random.Range(0, spawnAreas.Count)];
        Vector2 pos = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));

        // 생성 및 초기화
        GameObject origin = enemyPrefabs[prefabIndex];
        GameObject spawned = Instantiate(origin, (Vector3)pos, Quaternion.identity);

        EnemyController ec = spawned.GetComponent<EnemyController>();
        ec.Init(this, gameManager.player.transform);

        // 현재 생존 카운트/매핑 갱신
        if (!createdCount.ContainsKey(prefabIndex)) createdCount[prefabIndex] = 0;
        createdCount[prefabIndex]++;

        typeByEnemy[ec] = prefabIndex;   // ← 나중에 죽을 때 감소시키기 위한 매핑

        activeEnemies.Add(ec);
        return true;
    }

    // 기즈모를 그려 영역을 시각화 (선택된 경우에만 표시)
    private void OnDrawGizmosSelected()
    {
        if (spawnAreas == null) return;

        Gizmos.color = gizmoColor;
        foreach (var area in spawnAreas)
        {
            Vector3 center = new Vector3(area.x + area.width / 2, area.y + area.height / 2);
            Vector3 size = new Vector3(area.width, area.height);
            Gizmos.DrawCube(center, size);
        }
    }

    public void RemoveEnemyOnDeath(EnemyController enemy)
    {
        activeEnemies.Remove(enemy);

        // 이 적의 타입을 찾아 현재 생존 수 감소
        int typeIndex;
        if (typeByEnemy.TryGetValue(enemy, out typeIndex))
        {
            int alive;
            if (createdCount.TryGetValue(typeIndex, out alive) && alive > 0)
                createdCount[typeIndex] = alive - 1;

            typeByEnemy.Remove(enemy);
        }

        if (enemySpawnComplite && activeEnemies.Count == 0)
            gameManager.EndOfWave();
    }
}