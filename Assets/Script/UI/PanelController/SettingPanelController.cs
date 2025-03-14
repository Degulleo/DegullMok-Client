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
    }
    
    // SFX On/Off시 호출되는 함수
    public void OnSFXToggleValueChanged(bool value)
    {
        Debug.Log("SFX : "+ value);
    }
    
    
    // BGM On/Off시 호출되는 함수
    public void OnBGMToggleValueChanged(bool value)
    {
        Debug.Log("BGM : "+ value);
    }


    // X 버튼 클릭시 호출되는 함수
    public void OnClickCloseButton()
    {
        Hide();
    }
}