using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ReplayPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private GameObject replayCellPrefab;
    [SerializeField] private Transform contentTransform;
    
    private CanvasGroup _backgroundCanvasGroup;    
    
    public delegate void PanelControllerHideDelegate();
    
    //TODO:Test용 닉네임 나중에 삭제하고 PlayerInfo에서 가져올 것
    private string _myNickname;
    private void Awake()
    {
        _backgroundCanvasGroup = GetComponent<CanvasGroup>();
        _myNickname = "Gildong";
    }

    private void Start()
    {
        List<ReplayRecord> records = new List<ReplayRecord>();
        records = ReplayManager.Instance.LoadReplayDatas();
        foreach (var replayRecord in records)
        {
            var replayCellButtonObject = Instantiate(replayCellPrefab, contentTransform);
            ReplayCell replayCell = replayCellButtonObject.GetComponent<ReplayCell>();
            string myPlayerType = _myNickname.Equals(replayRecord.playerA)?"PlayerA":"PlayerB";
            string opponentNickname = _myNickname.Equals(replayRecord.playerA)?replayRecord.playerB:replayRecord.playerA;
            
            replayCell.SetMyPlayerType(myPlayerType);
            replayCell.SetWinImage(myPlayerType.Equals(replayRecord.winnerPlayerType));
            replayCell.SetOpponentPlayerNickname(opponentNickname);
            replayCell.SetRecordDate(replayRecord.gameDate);
            replayCell.SetReplayRecord(replayRecord);
            
            
            
        }
    }
    
    
    
    public void OnClickCloseButton()
    {
        Hide(() =>
        {
            Destroy(gameObject);
        });
    }
    public void Hide(PanelControllerHideDelegate hideDelegate = null)
    {
        _backgroundCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;
        hideDelegate?.Invoke();
    }
}
