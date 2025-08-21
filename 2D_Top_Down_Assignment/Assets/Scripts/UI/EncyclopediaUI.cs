using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaUI : MonoBehaviour
{
    [Header("Hierarchy Refs")]
    [SerializeField] private Button buttonEncyclopedia;      // Canvas/Button - Encyclopedia
    [SerializeField] private GameObject popupEncyclopedia;   // Canvas/Popup - Encyclopedia (Panel 또는 Root GO)
    [SerializeField] private TextMeshProUGUI descriptionTMP; // Canvas/Popup - Encyclopedia/Image - Description/Text (TMP) - Description
    [SerializeField] private Transform content;               // Canvas/Popup - Encyclopedia/Scroll View/Viewport/Content
    [SerializeField] private Button monsterButtonPrefab;      // Button - Monster (프리팹)

    [Header("Optional")]
    [Tooltip("Escape 키로 도감 닫기")]
    [SerializeField] private bool closeOnEscape = true;

    [Tooltip("도감 열 때 첫 몬스터 자동 선택")]
    [SerializeField] private bool autoSelectFirst = true;

    private bool isOpen;
    private float prevTimeScale = 1f;

    private void Awake()
    {
        if (buttonEncyclopedia != null)
            buttonEncyclopedia.onClick.AddListener(TogglePopup);

        // 시작 시 팝업 닫혀있도록
        if (popupEncyclopedia != null)
            popupEncyclopedia.SetActive(false);
        isOpen = false;
    }

    private void OnDestroy()
    {
        if (buttonEncyclopedia != null)
            buttonEncyclopedia.onClick.RemoveListener(TogglePopup);
    }

    private void Update()
    {
        if (!isOpen) return;
        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
            TogglePopup();
    }

    /// <summary>
    /// 도감 토글 (열기/닫기)
    /// </summary>
    public void TogglePopup()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            // 일시정지
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // 팝업 열기
            if (popupEncyclopedia != null)
                popupEncyclopedia.SetActive(true);

            // 리스트 구성
            BuildMonsterList();

            // 기본 설명 초기화
            if (descriptionTMP != null) descriptionTMP.text = "몬스터를 선택하세요.";
        }
        else
        {
            // 팝업 닫기
            if (popupEncyclopedia != null)
                popupEncyclopedia.SetActive(false);

            // 일시정지 해제
            Time.timeScale = prevTimeScale;
        }
    }

    /// <summary>
    /// DataRepository 인스턴스를 안전하게 가져오기
    /// </summary>
    private DataRepository GetRepository()
    {
        var repo = DataRepository.Instance;
        if (repo != null) return repo;

        // 씬에서 수동 탐색 (Awake 순서/프리팹 배치 문제 대비)
#if UNITY_2023_1_OR_NEWER
        repo = FindFirstObjectByType<DataRepository>(FindObjectsInactive.Exclude);
#else
        repo = FindObjectOfType<DataRepository>();
#endif
        if (repo == null)
        {
            Debug.LogError("[EncyclopediaUI] DataRepository가 씬에 없습니다. 빈 오브젝트에 DataRepository 컴포넌트를 붙여 배치하세요.");
            return null;
        }

        // 필요 시 강제 로드
        repo.LoadAll();
        return repo;
    }

    /// <summary>
    /// DataRepository에서 몬스터 데이터를 가져와 버튼을 동적으로 생성
    /// </summary>
    private void BuildMonsterList()
    {
        Debug.Log($"[EncyclopediaUI] Monsters: {DataRepository.Instance?.Monsters.Count}");

        var repo = GetRepository();
        if (repo == null) return;

        // 필수 레퍼런스 누락 체크
        if (monsterButtonPrefab == null)
        {
            Debug.LogError("[EncyclopediaUI] monsterButtonPrefab 이(가) 비었습니다. Button - Monster 프리팹을 할당하세요.");
            return;
        }
        if (content == null)
        {
            Debug.LogError("[EncyclopediaUI] content 가 비었습니다. Scroll View/Viewport/Content 를 할당하세요.");
            return;
        }

        // 기존 버튼 정리
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        var monsters = repo.Monsters;
        if (monsters == null || monsters.Count == 0)
        {
            if (descriptionTMP != null)
                descriptionTMP.text = "몬스터 데이터가 없습니다.\n(DataRepository 경로/로드 설정을 확인하세요.)";
            return;
        }

        Button firstBtn = null;

        // 몬스터 수만큼 버튼 생성
        for (int i = 0; i < monsters.Count; i++)
        {
            var m = monsters[i];

            Button btn = Instantiate(monsterButtonPrefab, content);
            btn.name = $"Button - Monster - {m.MonsterID}";

            // 버튼 안의 Text (TMP) - Name 찾아서 이름 세팅
            var nameTMP = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (nameTMP != null)
                nameTMP.text = m.Name;

            // 클릭 시 해당 몬스터 설명 표시
            int capturedIndex = i; // 클로저 주의
            btn.onClick.AddListener(() => ShowMonster(monsters[capturedIndex]));

            if (firstBtn == null) firstBtn = btn;
        }

        // 첫 항목 자동 선택
        if (autoSelectFirst && monsters.Count > 0)
            ShowMonster(monsters[0]);
    }

    /// <summary>
    /// 선택한 몬스터의 상세정보를 좌측 설명창에 표시
    /// </summary>
    private void ShowMonster(MonsterRecord m)
    {
        if (descriptionTMP == null || m == null) return;

        var sb = new StringBuilder(256);
        sb.AppendLine($"[{m.MonsterID}] {m.Name}");
        if (!string.IsNullOrWhiteSpace(m.Description))
        {
            sb.AppendLine(m.Description);
            sb.AppendLine();
        }

        sb.AppendLine($"HP : {m.MaxHP} (x{m.MaxHPMul})");
        sb.AppendLine($"ATK: {m.Attack} (x{m.AttackMul})");
        sb.AppendLine($"RNG: {m.AttackRange} (x{m.AttackRangeMul})");
        sb.AppendLine($"AS : {m.AttackSpeed}");
        sb.AppendLine($"MoveSpeed: {m.MoveSpeed}");
        sb.AppendLine($"EXP: {m.MinExp} ~ {m.MaxExp}");

        if (m.DropItem != null && m.DropItem.Length > 0)
        {
            sb.Append("Drops: ");
            for (int i = 0; i < m.DropItem.Length; i++)
            {
                sb.Append(m.DropItem[i]);
                if (i < m.DropItem.Length - 1) sb.Append(", ");
            }
        }

        descriptionTMP.text = sb.ToString();
    }
}