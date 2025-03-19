using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
    void Awake()
    {
        //TODO: 리플레이매니저 데이터로 화면 초기화
    }
    
    public void OnclickExitButton()
    {
        
    }

    public void OnclickFirstButton()
    {
        
    }

    public void OnclickUndoButton()
    {
        Move targetMove = ReplayManager.Instance.PopMove();
        if (targetMove != null)
        {
            GameManager.Instance.ReplayUndo(targetMove);
        }
    }

    public void OnclickNextButton()
    {
        Move nextMove = ReplayManager.Instance.GetNextMove();
        if (nextMove != null)
        {
            GameManager.Instance.ReplayNext(nextMove);
        }
    }

    public void OnClickFinishButton()
    {
        
    }
    
    
}
