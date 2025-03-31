using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class LeaderBoardController : MonoBehaviour
{
    [SerializeField] private GameObject rankingPrefab;  
    [SerializeField] private Transform content; 
    [SerializeField] private GameObject MainPanel; 
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Scrollbar verticalScrollbar;

    private bool isLeaderboardLoaded = false;

    private void Start()
    {
        OnClickLeaderboardButton();
    }

    public void OnClickLeaderboardButton()
    {
        GameManager.Instance.audioManager.PlayClickSound();

        if (isLeaderboardLoaded) return;  

        leaderboardPanel.SetActive(true);
        NetworkManager.Instance.GetLeaderboard((leaderboardItems) =>
        {
            Show(leaderboardItems);
        }, () => { });
        isLeaderboardLoaded = true;
    }
    
    public void Show(List<ScoreInfo> leaderboardItems)
    {
        
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        
        foreach (var item in leaderboardItems)
        {
            CreateCell(item);
        }
    }

    private void CreateCell(ScoreInfo item)
    {
        
        var scoreCellObj = Instantiate(rankingPrefab, content);

       
        var scoreCellController = scoreCellObj.GetComponent<ScoreCellController>();

        if (scoreCellController != null)
        {
            
            scoreCellController.SetCellInfo(item); 
        }
    }

    
    public void OnBackButtonClicked()
    {
        GameManager.Instance.audioManager.PlayCloseSound();

        leaderboardPanel.SetActive(false);  
        MainPanel.SetActive(true);        
    }

    private List<ScoreInfo> LoadOfflineLeaderboard()
    {
        List<ScoreInfo> leaderboard = new List<ScoreInfo>();

        
        string savedData = PlayerPrefs.GetString("OfflineLeaderboard", string.Empty);

        if (!string.IsNullOrEmpty(savedData))
        {
            leaderboard = JsonUtility.FromJson<ScoreListWrapper>(savedData).leaderboardDatas;
        }

        return leaderboard;
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }
}