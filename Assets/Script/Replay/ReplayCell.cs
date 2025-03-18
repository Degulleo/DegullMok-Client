using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayCell : MonoBehaviour
{
    [SerializeField] private Image winImage;
    [SerializeField] private Image loseImage;
    //TODO: TextMeshProUGI 수정하기
    [SerializeField] private TMP_Text playerNicknameText;
    [SerializeField] private TMP_Text recordDateText;
    
    private ReplayRecord _storedReplayRecord;
    private Enums.PlayerType _myPlayerType;
    private string _opponentNickname;
    
    
    //유저가 이겼을 경우 '승'(파랑)이미지 졌을 경우'패'(빨강)이미지
    public void SetWinImage(bool isWin)
    {
        if (isWin == true)
        {
            winImage.gameObject.SetActive(true);
            loseImage.gameObject.SetActive(false);
        }
        else
        {
            loseImage.gameObject.SetActive(true);
            winImage.gameObject.SetActive(false);
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
        string text = "";
        string[] dateSplit = date.Split(' ');
        if (dateSplit.Length == 2)
        {
             text += dateSplit[0].Replace("-", ".");
             text += "\n";
             text += dateSplit[1].Replace("_", ":");
        }

        recordDateText.text = text;
    }

    public void SetReplayRecord(ReplayRecord record)
    {
        _storedReplayRecord = record;
    }

    
    //TODO: storedReplayRecord를 가지고 게임 씬으로 전환
    public void OnClickReplayButton()
    {
        Debug.Log($"Replay Start with {_opponentNickname}\nDate: {_storedReplayRecord.gameDate}\n" +
                  $"Moves: {_storedReplayRecord.moves}");
    }
    
}
