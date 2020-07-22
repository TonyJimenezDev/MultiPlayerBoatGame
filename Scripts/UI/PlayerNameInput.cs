using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField = null;
    [SerializeField] Button continueButton = null;

    private const string playerPrefsNameKey = "PlayerName";

    private void Awake() => continueButton.interactable = false;

    private void Start()
    {
        SetUpInputField();
    }

    private void SetUpInputField()
    {
        
        if (!PlayerPrefs.HasKey(playerPrefsNameKey)) return;

        string defaultName = PlayerPrefs.GetString(playerPrefsNameKey);
        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name)
    {
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        string playerName = nameInputField.text;

        PhotonNetwork.NickName = playerName;

        PlayerPrefs.SetString(playerPrefsNameKey, playerName);
    }
}
