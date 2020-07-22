using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBoat : MonoBehaviour
{
    #region Inspector

    [SerializeField] float playerSpeed;
    [SerializeField] float SteerPower = 100f;
    [SerializeField] float MaxSpeed = 15f;
    [SerializeField] float Drag = 0.1f;
    [SerializeField] GameObject motorEffect;
    public Transform Motor;
    public float Power = 10f;
    #endregion

    private GameManager gameManager;
    private GameObject flagChase;
    private GameObject playerChase;
    
    protected Vector3 pointTarget;
    protected Quaternion playerRotation;
    protected Vector3 CamVel;
    protected Rigidbody playerBody;

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        playerRotation = Motor.localRotation;
        playerChase = FindClosestTag("Player");
        flagChase = FindClosestTag("Flag");
    }

    private void FixedUpdate()
    {
        Vector3 forceDirection = transform.forward;
        playerSpeed = playerBody.velocity.magnitude;

        Vector3 forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);
        Vector3 targetVel = Vector3.zero;


        //  StartCoroutine(SlowEffectStop());

        // Chases the player if it gets close enough to an enemy and goes back to chasing flag when not.
        if (playerChase.transform.position.sqrMagnitude <= flagChase.transform.position.sqrMagnitude)
        {
            pointTarget = transform.position - playerChase.transform.position;
            //if (!gameObject.GetComponent<ParticleSystem>().isPlaying) gameObject.GetComponent<ParticleSystem>().Play();
        }
        else
        {
            pointTarget = transform.position - flagChase.transform.position;
            //gameObject.GetComponent<ParticleSystem>().Stop();
        }

        playerChase = FindClosestTag("Player");
        flagChase = FindClosestTag("Flag");
        pointTarget.Normalize();
        float value = Vector3.Cross(pointTarget, transform.forward).y;

        playerBody.angularVelocity = SteerPower * value * new Vector3(0, 1, 0);
        ApplyForceToReachVelocity(playerBody, forward * MaxSpeed, Power);
        motorEffect.SetActive(true);


    }


    public GameObject FindClosestTag(string tag)
    {
        GameObject[] gameObjects;
        gameObjects = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gameObjects)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }


    IEnumerator SlowEffectStop()
    {
        yield return new WaitForSeconds(.3f);
        motorEffect.SetActive(false);
    }

    public void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity, float force = 1, ForceMode mode = ForceMode.Force)
    {
        if (force == 0 || velocity.magnitude == 0)
            return;

        velocity = velocity + velocity.normalized * 0.2f * rigidbody.drag;

        //force = 1 => need 1 s to reach velocity (if mass is 1) => force can be max 1 / Time.fixedDeltaTime
        force = Mathf.Clamp(force, -rigidbody.mass / Time.fixedDeltaTime, rigidbody.mass / Time.fixedDeltaTime);

        //dot product is a projection from rhs to lhs with a length of result / lhs.magnitude https://www.youtube.com/watch?v=h0NJK4mEIJU
        if (rigidbody.velocity.magnitude == 0)
        {
            rigidbody.AddForce(velocity * force, mode);
        }
        else
        {
            Vector3 velocityProjectedToTarget = (velocity.normalized * Vector3.Dot(velocity, rigidbody.velocity) / velocity.magnitude);
            rigidbody.AddForce((velocity - velocityProjectedToTarget) * force, mode);
        }
    }

    public void ApplyTorqueToReachRPS(Rigidbody rigidbody, Quaternion rotation, float rps, float force = 1)
    {
        float radPerSecond = rps * 2 * Mathf.PI + rigidbody.angularDrag * 20;

        float angleInDegrees;
        Vector3 rotationAxis;
        rotation.ToAngleAxis(out angleInDegrees, out rotationAxis);

        if (force == 0 || rotationAxis == Vector3.zero)
            return;

        rigidbody.maxAngularVelocity = Mathf.Max(rigidbody.maxAngularVelocity, radPerSecond);

        force = Mathf.Clamp(force, -rigidbody.mass * 2 * Mathf.PI / Time.fixedDeltaTime, rigidbody.mass * 2 * Mathf.PI / Time.fixedDeltaTime);

        float currentSpeed = Vector3.Project(rigidbody.angularVelocity, rotationAxis).magnitude;

        rigidbody.AddTorque(rotationAxis * (radPerSecond - currentSpeed) * force);
    }

    public Vector3 QuaternionToAngularVelocity(Quaternion rotation)
    {
        float angleInDegrees;
        Vector3 rotationAxis;
        rotation.ToAngleAxis(out angleInDegrees, out rotationAxis);

        return rotationAxis * angleInDegrees * Mathf.Deg2Rad;
    }
}
