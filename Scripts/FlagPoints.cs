using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlagPoints : MonoBehaviourPun
{
    [SerializeField] Transform spawnParent;
    [SerializeField] float minFlagTimer = 1f;
    [SerializeField] float maxFlagTimer = 3f;
    [SerializeField] int maxFlagSpawn = 6;

    public GameObject flag;
    bool spawning;
    Vector3 randomSpawnPoint;

    void Start()
    {

            

    }

    void Update()
    {
        if (!spawning && spawnParent.childCount < maxFlagSpawn)
        {
            StartCoroutine(Spawn());
        }
    }

    // Random flag spawning
    IEnumerator Spawn()
    {
        spawning = true;
        randomSpawnPoint.x = Random.Range(20f, 140f);
        randomSpawnPoint.y = 15f;
        randomSpawnPoint.z = Random.Range(20f, 125f);
        PhotonNetwork.Instantiate(flag.name, randomSpawnPoint, transform.rotation).transform.SetParent(spawnParent);
        yield return new WaitForSeconds(Random.Range(minFlagTimer, maxFlagTimer));
        spawning = false;
    }

}
