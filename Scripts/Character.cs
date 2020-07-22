using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Character on the boat.
/// </summary>
public class Character : MonoBehaviourPun
{
    Animator animator;
    GameManager gameManager;
    GameObject instantiatedExplosion;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        TurningAnimation();
    }

    private void TurningAnimation()
    {
        if (Input.GetKey(KeyCode.A)) animator.SetInteger("Turning", -1); // left
        else if (Input.GetKey(KeyCode.D)) animator.SetInteger("Turning", 1); // right
        else animator.SetInteger("Turning", 0); // default
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Flag")
        {
            // Need audio
            other.gameObject.SetActive(false);
            gameObject.GetComponentInParent<BoxCollider>().enabled = false;
            gameManager.playerScore += 1;
            instantiatedExplosion = Instantiate(gameManager.FlagExplosion, transform.position, transform.rotation);
            Detenation();
            StartCoroutine(DelayExplosion());
            PhotonNetwork.Destroy(other.gameObject);
            Destroy(instantiatedExplosion, gameManager.FlagExplosionTimer);
            gameObject.GetComponentInParent<BoxCollider>().enabled = true;
        }

        // Not finished
        if (other.gameObject.tag == "Enemy")
        {
            gameManager.playerHealth -= 1;
        }
    }
    IEnumerator DelayExplosion()
    {
        yield return new WaitForSeconds(.5f);
    }

    private void Detenation()
    {
        Vector3 explosionPos = instantiatedExplosion.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, gameManager.flagExplosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            EnemyBoat enemyPowerReduced = hit.GetComponent<EnemyBoat>();
            WaterBoat friendlyPowerReduced = hit.GetComponent<WaterBoat>();

            if (rb != null)
            {
                rb.AddExplosionForce(gameManager.flagExplosionPower, explosionPos, gameManager.flagExplosionRadius, gameManager.flagExplosionupForce, ForceMode.Impulse);
                if(enemyPowerReduced != null) enemyPowerReduced.Power -= 1f;
                else if(friendlyPowerReduced != null) friendlyPowerReduced.Power -= 1f;
            }
                
        }
    }
}
