using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _registerPanel;
    [SerializeField] private VisualizeUserData _statsPanel;

    public void OpenLoginPanel()
    {
        _loginPanel.SetActive(true);
        _registerPanel.SetActive(false);
        _statsPanel.SetPanelActive(false);
    }

    public void OpenRegisterPanel()
    {
        _registerPanel.SetActive(true);
        _loginPanel.SetActive(false);
        _statsPanel.SetPanelActive(false);
    }

    public void OpenStatsPanel(UserData data)
    {
        _loginPanel.SetActive(false);
        _registerPanel.SetActive(false);

        _statsPanel.SetPanelActive(true);
        if(data != null)
        {
            _statsPanel.SetUserData(data);
        }
        else
        {
            Debug.Log("Data is null!");
        }

    }
}
