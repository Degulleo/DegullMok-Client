using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : PanelController
{
    [SerializeField] private Button sfxSwitch;
    [SerializeField] private Button bgmSwitch;
    [SerializeField] private GameObject closeButton;
    
    void Start()
    {
        
        sfxSwitch.GetComponent<SwitchController>().OnSwitchChanged += OnSFXToggleValueChanged;
        bgmSwitch.GetComponent<SwitchController>().OnSwitchChanged += OnBGMToggleValueChanged;
        
        
        sfxSwitch.GetComponent<SwitchController>().SetSwitch(UserManager.IsPlaySFX);
        bgmSwitch.GetComponent<SwitchController>().SetSwitch(UserManager.IsPlayBGM);
    }

    public void Show()
    {
        closeButton.GetComponent<SingleInteractableButtonHandler>().ResetButton();
        base.Show();
    }

    
    public void OnSFXToggleValueChanged(bool value)
    {
        UserManager.IsPlaySFX = value; 
    }

   
    public void OnBGMToggleValueChanged(bool value)
    {
        UserManager.IsPlayBGM = value;

        
        if (!value)
        {
           AudioManager.Instance.StopBGM(); 
        }
        else
        {
            if (!AudioManager.Instance.bgmAudioSource.isPlaying)
            {
                AudioManager.Instance.PlayBGM(); 
            }
        }
    }

    
    public void OnClickCloseButton()
    {
        Hide();
    }
}