using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class LeaderBoardController : MonoBehaviour
{
    [SerializeField] private GameObject rankingPrefab;  // Ranking 프리팹을 참조 (Horizontal Layout)
    [SerializeField] private Transform content;  // Vertical Layout Group
    [SerializeField] private GameObject MainPanel; 
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Scrollbar verticalScrollbar;// LeaderboardPanel 참조

    private bool isLeaderboardLoaded = false;

    private void Start()
    {
        OnClickLeaderboardButton();
    }

    public void OnClickLeaderboardButton()
    {
        if (isLeaderboardLoaded) return;  // 이미 리더보드가 로드되었으면 중복 호출 방지

        leaderboardPanel.SetActive(true);
        NetworkManager.Instance.GetLeaderboardData((leaderboardItems) =>
        {
            Show(leaderboardItems);
        }, () => { });
        isLeaderboardLoaded = true;
    }
    
    public void Show(List<ScoreInfo> leaderboardItems)
    {
        // 기존 셀 삭제 (리스트가 갱신될 때마다)
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 받은 데이터로 셀 생성
        foreach (var item in leaderboardItems)
        {
            CreateCell(item);  // 셀 생성
        }
    }

    private void CreateCell(ScoreInfo item)
    {
        // Ranking 프리팹을 content의 자식으로 생성
        var scoreCellObj = Instantiate(rankingPrefab, content);

        // Ranking 프리팹에 포함된 ScoreCellController를 찾아서 설정
        var scoreCellController = scoreCellObj.GetComponent<ScoreCellController>();

        if (scoreCellController != null)
        {
            // 각 항목에 대한 UI 설정
            scoreCellController.SetCellInfo(item);  // ScoreInfo로 셀 정보 설정
        }
        else
        {
            Debug.LogError("ScoreCellController가 Ranking 프리팹에 없습니다.");
        }
    }

    // BackButton 클릭 시 호출되는 메소드
    public void OnBackButtonClicked()
    {
        leaderboardPanel.SetActive(false);  // LeaderboardPanel 숨기기
        MainPanel.SetActive(true);        // SignInPanel 보이게 하기
    }

    private List<ScoreInfo> LoadOfflineLeaderboard()
    {
        List<ScoreInfo> leaderboard = new List<ScoreInfo>();

        // 오프라인 데이터 로딩 (PlayerPrefs 사용 예시)
        string savedData = PlayerPrefs.GetString("OfflineLeaderboard", string.Empty);

        if (!string.IsNullOrEmpty(savedData))
        {
            // 저장된 JSON 데이터를 파싱하여 리더보드 리스트로 변환
            leaderboard = JsonUtility.FromJson<ScoreListWrapper>(savedData).leaderboardDatas;
        }

        return leaderboard;
    }

    private void OnDisable()
    {
        Destroy(gameObject);  // 자기 자신을 삭제
    }
}