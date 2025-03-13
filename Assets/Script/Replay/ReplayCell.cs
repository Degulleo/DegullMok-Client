using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayCell : MonoBehaviour
{
    [SerializeField] private Image winImage;
    [SerializeField] private Image loseImage;
    [SerializeField] private TMP_Text playerNicknameText;
    [SerializeField] private TMP_Text recordDateText;
    
    private ReplayRecord _storedReplayRecord;
    private string _myPlayerType;
    private string _opponentNickname;
    
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

    public void SetMyPlayerType(string myPlayerType)
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
        recordDateText.text = date;
    }

    public void SetReplayRecord(ReplayRecord record)
    {
        _storedReplayRecord = record;
    }

    public void OnClickReplayButton()
    {
        Debug.Log($"Replay Start with {_opponentNickname}\nDate: {_storedReplayRecord.gameDate}\n" +
                  $"Moves: {_storedReplayRecord.moves}");
    }
    
}
