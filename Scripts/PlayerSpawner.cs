using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class PlayerSpawner : MonoBehaviourPun
{
    [SerializeField] GameObject wavesPrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] CinemachineVirtualCamera playerCamera;

    private void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject waves = PhotonNetwork.InstantiateSceneObject(wavesPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
            //waves.GetComponent<PhotonView>().RPC()
        }
        
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(100f, 10f, 55f), Quaternion.identity);
        playerCamera.Follow = player.transform;
        playerCamera.LookAt = player.transform;
    }
}
