using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : PanelController
{
    [SerializeField] private Button sfxSwitch;
    [SerializeField] private Button bgmSwitch;
    
    void Start()
    {
        //스위치 컨트롤러 상태 변경 이벤트 연결
        sfxSwitch.GetComponent<SwitchController>().OnSwitchChanged += (OnSFXToggleValueChanged);
        bgmSwitch.GetComponent<SwitchController>().OnSwitchChanged += (OnBGMToggleValueChanged);
        
        // 현재 저장된 설정 값을 UI에 반영
        sfxSwitch.GetComponent<SwitchController>().SetSwitch(UserManager.IsPlaySFX);
        bgmSwitch.GetComponent<SwitchController>().SetSwitch(UserManager.IsPlayBGM);
    }
    
    // SFX On/Off시 호출되는 함수
    public void OnSFXToggleValueChanged(bool value)
    {
        Debug.Log("SFX : "+ value);
        UserManager.IsPlaySFX = value; // UserManager에 값 저장
    }
    
    
    // BGM On/Off시 호출되는 함수
    public void OnBGMToggleValueChanged(bool value)
    {
        Debug.Log("BGM : "+ value);
        UserManager.IsPlayBGM = value; // UserManager에 값 저장
        
        // GameManager에서 BGM 상태를 반영
        if (value)
        {
            GameManager.Instance.audioManager.PlayMainBGM();  // BGM을 켜기
        }
        else
        {
            GameManager.Instance.audioManager.StopMainBGM();  // BGM을 끄기
        }
    }


    // X 버튼 클릭시 호출되는 함수
    public void OnClickCloseButton()
    {
        Hide();
    }
}