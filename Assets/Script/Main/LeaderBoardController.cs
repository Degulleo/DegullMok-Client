using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderBoardController : MonoBehaviour
{
   [SerializeField] private GameObject scoreCell;
   [SerializeField] private Transform content;
   private List<ScoreCellController> scoreCC = new List<ScoreCellController>();

   private void Start()
   {
      StartCoroutine(OnConnectedToServer());
   }

   private IEnumerator OnConnectedToServer()
   {
      string url = Constants.ServerURL + "/users/Leaderboard"; // 서버의 리더보드 데이터 URL

      UnityWebRequest www = UnityWebRequest.Get(url); // GET 요청으로 데이터 받기
      yield return www.SendWebRequest(); // 요청 전송 대기

      // 요청이 실패했을 때
      if (www.isNetworkError || www.isHttpError)
      {
         Debug.LogError("Error: " + www.error);
      }
      else
      {
         // 성공적으로 데이터를 받아온 경우
         string jsonResponse = www.downloadHandler.text; // 응답으로 받은 JSON 데이터

         // JSON을 ScoreInfo 리스트로 파싱
         List<ScoreInfo> scoreInfos = JsonUtility.FromJson<ScoreListWrapper>(jsonResponse).scoreInfos;

         // 받아온 데이터를 기반으로 점수 셀 생성
         foreach (var scoreInfo in scoreInfos)
         {
            CreateCell(scoreInfo); // 각 점수 정보를 기반으로 셀 생성
         }
      }
   }

   public void CreateCell(ScoreInfo scoreInfo)
   {
      var scoreCellObj = Instantiate(scoreCell, content);
      var scoreCellController = scoreCellObj.GetComponent<ScoreCellController>();
      // 점수 셀 정보 설정
      if (scoreCellController != null)
      {
         scoreCellController.SetCellInfo(scoreInfo); // 점수 셀에 점수 정보 설정
      }
      else
      {
         Debug.LogError("ScoreCellController 컴포넌트가 점수 셀 프리팹에 없습니다.");
      }
   }
}

[System.Serializable]
public class ScoreListWrapper
{
   public List<ScoreInfo> scoreInfos;
}