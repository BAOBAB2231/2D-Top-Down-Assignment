using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerController player { get; private set; } // 플레이어 컨트롤러 (읽기 전용 프로퍼티)
    private ResourceController _playerResourceController;

    [SerializeField] private int currentWaveIndex = 0; // 현재 웨이브 번호

    private EnemyManager enemyManager; // 적 생성 및 관리하는 매니저

    private void Awake()
    {
        // 싱글톤 할당
        instance = this;

        // 플레이어 찾고 초기화
        player = FindFirstObjectByType<PlayerController>();
        player.Init(this);

        // 적 매니저 초기화
        enemyManager = GetComponentInChildren<EnemyManager>();
        enemyManager.Init(this);
    }

    public void StartGame()
    {
        StartNextWave(); // 첫 웨이브 시작
    }

    void StartNextWave()
    {
        currentWaveIndex += 1; // 웨이브 인덱스 증가
        // 5웨이브마다 난이도 증가 (예: 1~4 → 레벨 1, 5~9 → 레벨 2 ...)
        enemyManager.StartWave(1 + currentWaveIndex / 5);
    }

    // 웨이브 종료 후 다음 웨이브 시작
    public void EndOfWave()
    {
        StartNextWave();
    }

    // 플레이어가 죽었을 때 게임 오버 처리
    public void GameOver()
    {
        enemyManager.StopWave(); // 적 스폰 중지
    }

    // 개발용 테스트: Space 키로 게임 시작
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }
}
