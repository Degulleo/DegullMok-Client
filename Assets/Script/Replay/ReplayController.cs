using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text playerANicknameText;  
    [SerializeField] private TMP_Text playerBNicknameText;
    [SerializeField] private Image playerAImage;
    [SerializeField] private Image playerBImage;
    [SerializeField] private GameObject[] userAProfileImages;
    [SerializeField] private GameObject[] userBProfileImages;
    void Start()
    {
        InitReplayUI();
        //TODO: 프로필 이미지 불러오기
    }
    
    public void OnclickExitButton()
    {
        //TODO: 메인씬으로 다시 넘어갈 때 호출해야하는 함수 등등이 있을지....
        SceneManager.LoadScene("Main-Jay");
    }

    public void OnclickFirstButton()
    {
        ReplayManager.Instance.ReplayFirst();
    }

    public void OnclickUndoButton()
    {
        Move targetMove = ReplayManager.Instance.PopPlacedMove();
        if (targetMove != null)
        {
            ReplayManager.Instance.ReplayUndo(targetMove);
        }
    }

    public void OnclickNextButton()
    {
        Move nextMove = ReplayManager.Instance.GetNextMove();
        if (nextMove != null)
        {
            ReplayManager.Instance.ReplayNext(nextMove);
        }
    }

    public void OnClickFinishButton()
    {
        ReplayManager.Instance.ReplayFinish();
    }

    public void InitReplayUI()
    {
        playerANicknameText.text = ReplayManager.Instance.GetPlayerANickname();
        playerBNicknameText.text = ReplayManager.Instance.GetPlayerBNickname();
    }
}
