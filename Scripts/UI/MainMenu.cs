using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject findOponnentPanel = null;
    [SerializeField] GameObject waitingStatusPanel = null;
    [SerializeField] TextMeshProUGUI waitingStatusText = null;

    private bool isConnecting = false;

    private const string GameVersion = "1.2";
    private const int maxPlayerPerRoom = 2;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void FindOpponnet()
    {
        isConnecting = true;

        findOponnentPanel.SetActive(false);
        waitingStatusPanel.SetActive(true);

        waitingStatusText.text = "Searching...";

        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");

        if(isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        waitingStatusPanel.SetActive(false);
        findOponnentPanel.SetActive(true);

        Debug.Log($"Disconnected due to {cause}.");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting for an opponents, creating a new room");

        // Add more room options if there are anymore.
        PhotonNetwork.CreateRoom(null, new RoomOptions {
            MaxPlayers = maxPlayerPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Client successfully joined room {PhotonNetwork.CurrentRoom}");

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if(playerCount != maxPlayerPerRoom)
        {
            waitingStatusText.text = "Waiting for Opponent";
            Debug.Log("Client is waiting for an opponent");
        }
        else
        {
            waitingStatusText.text = "Match is ready to begin";
            Debug.Log("Match is ready to begin");
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == maxPlayerPerRoom)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            waitingStatusText.text = "Opponent found!";
            Debug.Log("Match is ready");

            PhotonNetwork.LoadLevel("DustLevel");
        }
    }
}
