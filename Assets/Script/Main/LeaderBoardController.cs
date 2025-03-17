using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderBoardController : MonoBehaviour
{
   [SerializeField] private GameObject scoreCell;
   [SerializeField] private Transform content;
   [SerializeField] private GameObject signInPanel; // SignInPanel 참조 (같은 씬에서 사용할 경우)
   [SerializeField] private GameObject leaderboardPanel; // LeaderboardPanel 참조

   private List<ScoreCellController> scoreCC = new List<ScoreCellController>();  // 점수 셀을 관리할 리스트

   private void Start()
   {
      leaderboardPanel.SetActive(false); // 초기에는 리더보드 패널을 숨깁니다.
      StartCoroutine(OnConnectedToServer()); // 서버에서 리더보드 데이터를 받아옵니다.
   }

   // 서버와 연결하여 데이터를 받아오는 함수
   private IEnumerator OnConnectedToServer()
   {
      string url = Constants.ServerURL + "/users/Leaderboard"; // 서버의 리더보드 데이터 URL

      UnityWebRequest www = UnityWebRequest.Get(url); // GET 요청으로 데이터 받기
      yield return www.SendWebRequest(); // 요청 전송 대기

      // 요청이 실패했을 때
      if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
      {
         Debug.LogError("Error: " + www.error);
      }
      else
      {
         // 성공적으로 데이터를 받아온 경우
         string jsonResponse = www.downloadHandler.text; // 응답으로 받은 JSON 데이터

         // JSON을 ScoreInfo 리스트로 파싱
         List<ScoreInfo> scoreInfos = JsonUtility.FromJson<ScoreListWrapper>(jsonResponse).scoreInfos;

         // 받아온 데이터를 기반으로 점수 셀을 생성하고 리스트를 갱신
         CreateCellsFromScoreInfos(scoreInfos);
      }
   }

   // 받아온 ScoreInfo 리스트를 기반으로 셀을 생성하는 함수
   private void CreateCellsFromScoreInfos(List<ScoreInfo> scoreInfos)
   {
      // 기존의 셀들을 초기화 (필요시)
      foreach (Transform child in content)
      {
         Destroy(child.gameObject);  // 기존 셀들을 삭제
      }

      // 받은 점수 데이터 수에 맞춰 셀을 생성
      for (int i = 0; i < scoreInfos.Count; i++)
      {
         CreateCell(scoreInfos[i]); // 각 점수 정보를 기반으로 셀 생성
      }
   }

   // 점수 정보를 기반으로 셀을 생성하는 함수
   public void CreateCell(ScoreInfo scoreInfo)
   {
      var scoreCellObj = Instantiate(scoreCell, content);  // 셀 객체 생성
      var scoreCellController = scoreCellObj.GetComponent<ScoreCellController>();

      // 점수 셀 정보 설정
      if (scoreCellController != null)
      {
         scoreCellController.SetCellInfo(scoreInfo);  // 점수 셀에 점수 정보 설정
      }
      else
      {
         Debug.LogError("ScoreCellController 컴포넌트가 점수 셀 프리팹에 없습니다.");
      }
   }
   
   // BackButton 클릭 시 호출되는 메소드
   public void OnBackButtonClicked()
   {
      // LeaderboardPanel을 숨기고 SignInPanel을 보이게 함
      leaderboardPanel.SetActive(false);  // LeaderboardPanel 숨기기
      signInPanel.SetActive(true);        // SignInPanel 보이기
   }
}

// ScoreListWrapper 클래스를 정의 (리스트를 감싸는 클래스)
[System.Serializable]
public class ScoreListWrapper
{
   public List<ScoreInfo> scoreInfos;  // 점수 정보 리스트
}