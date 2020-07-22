using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("Flag being captured points")]
    public int playerScore;
    [Tooltip("Boat hp before you die")]
    public int playerHealth = 10;


    // Look up the code to put this in one section by itself
    public GameObject FlagExplosion;
    public float FlagExplosionTimer = 1f;
    public float flagExplosionRadius = 5.0f;
    public float flagExplosionPower = 10.0f;
    public float flagExplosionupForce = 3.0f;

    private void Update()
    {
       
    }
}
