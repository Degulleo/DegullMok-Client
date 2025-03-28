using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReplayCell : MonoBehaviour
{
    [SerializeField] private Image winImage;
    [SerializeField] private Image loseImage;
    [SerializeField] private Image drawImage;
    [SerializeField] private TMP_Text playerNicknameText;
    [SerializeField] private TMP_Text recordDateText;
    
    private ReplayRecord _storedReplayRecord;
    private Enums.PlayerType _myPlayerType;
    private string _opponentNickname;

    public void SetWinImage(Enums.GameResult gameResult)
    {
        switch(gameResult)
        {
            case Enums.GameResult.Win:
                winImage.gameObject.SetActive(true);
                loseImage.gameObject.SetActive(false);
                drawImage.gameObject.SetActive(false);
                break;
            case Enums.GameResult.Lose:
                winImage.gameObject.SetActive(false);
                loseImage.gameObject.SetActive(true);
                drawImage.gameObject.SetActive(false);
                break;
            case Enums.GameResult.Draw:
                winImage.gameObject.SetActive(false);
                loseImage.gameObject.SetActive(false);
                drawImage.gameObject.SetActive(true);
                break;
        }
    }
    
    public void SetMyPlayerType(Enums.PlayerType myPlayerType)
    {
        _myPlayerType = myPlayerType;
    }

    public void SetOpponentPlayerNickname(string nickname)
    {
        _opponentNickname = nickname;
        playerNicknameText.text = nickname;
    }

    public void SetRecordDate(string date)
    {
        if (string.IsNullOrEmpty(date))
        {
            // 입력이 비어있거나 null인 경우 예외 처리
            recordDateText.text = "Invalid Date Format";
            return;
        }
        
        string[] dateSplit = date.Split(' ');
        if (dateSplit.Length == 2)
        {
            StringBuilder text = new StringBuilder();
        
            // 첫 번째 부분 (날짜) - "-"을 "."으로 교체
            text.Append(dateSplit[0].Replace("-", "."));
            text.Append("\n");
        
            // 두 번째 부분 (시간) - "_"을 ":"으로 교체
            text.Append(dateSplit[1].Replace("_", ":"));

            recordDateText.text = text.ToString();
        }
        else
        {
            // 잘못된 포맷 처리
            recordDateText.text = "Invalid Date Format";
        }
    }

    public void SetReplayRecord(ReplayRecord record)
    {
        _storedReplayRecord = record;
    }
    
    public void OnClickReplayButton()
    {
        GameManager.Instance.panelManager.OpenConfirmPanel($"{_opponentNickname}님 과의\n대결을 다시 보시겠습니까?", 
            () => {
                ReplayManager.Instance.SetReplayData(_storedReplayRecord);
                SceneManager.LoadScene("Replay"); }, true);
    }
}
