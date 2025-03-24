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
    [SerializeField] private GameObject[] userAProfileImages;
    [SerializeField] private GameObject[] userBProfileImages;
    void Start()
    {
        InitReplayUI();
    }
    
    public void OnclickExitButton()
    {
        ReplayManager.Instance.StopReplayFinish();
        SceneManager.LoadScene("Main");
    }

    public void OnclickFirstButton()
    {
        ReplayManager.Instance.StopReplayFinish();
        ReplayManager.Instance.ReplayFirst();
    }

    public void OnclickUndoButton()
    {
        ReplayManager.Instance.StopReplayFinish();
        Move targetMove = ReplayManager.Instance.PopPlacedMove();
        if (targetMove != null)
        {
            ReplayManager.Instance.ReplayUndo(targetMove);
        }
    }

    public void OnclickNextButton()
    {
        ReplayManager.Instance.StopReplayFinish();
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
        //유저 닉네임 설정
        //TODO: 유니티 에디터에서 폰트 설정바꾸기
        playerANicknameText.text = ReplayManager.Instance.GetPlayerANickname();
        playerBNicknameText.text = ReplayManager.Instance.GetPlayerBNickname();
        
        //프로필 이미지 설정
        int playerAProgileIndex = ReplayManager.Instance.GetPlayerAProfileIndex();
        int playerBProgileIndex = ReplayManager.Instance.GetPlayerBProfileIndex();
        SetUserProfileImages(playerAProgileIndex, userAProfileImages);
        SetUserProfileImages(playerBProgileIndex, userBProfileImages);
    }

    private void SetUserProfileImages(int imageIndex,GameObject[] profileImages)
    { 
        if (imageIndex < 0 || imageIndex >= profileImages.Length)
        {
            return;
        }

        // 모든 프로필 이미지 비활성화 후, 선택한 이미지만 활성화
        foreach (var img in profileImages)
        {
            img.SetActive(false);
        }
        profileImages[imageIndex].SetActive(true);
    }
}
