using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VisualizeUserData : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _scoreText;
    [SerializeField] TextMeshProUGUI _healthText;
    [SerializeField] GameObject _holder;
    public void SetUserData(UserData data)
    {
        _nameText.text = data.Name;
        _scoreText.text = data.Score.ToString();
        _healthText.text = data.Health.ToString();
    }
    public void SetPanelActive(bool isActive)
    {
        _holder.SetActive(isActive);
    }
}
