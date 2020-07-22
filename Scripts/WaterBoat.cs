using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(WaterFloat))]
public class WaterBoat : MonoBehaviourPun
{
    #region Inspector
    [SerializeField] float playerSpeed;
    public Transform Motor;
    [SerializeField] float SteerPower = 500f;
    public float Power = 10f;
    [SerializeField] float MaxSpeed = 15f;
    [SerializeField] float Drag = 1f;
    [SerializeField] GameObject motorEffect;
    #endregion

    protected Rigidbody playerBody;
    protected Quaternion playerRotation;


    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        playerRotation = Motor.localRotation;
    }

    private void FixedUpdate()
    {
        if(photonView.IsMine)
        {
            TakeInput();
        }


        //Motor Animation // Particle system
        //Motor.SetPositionAndRotation(Motor.position, transform.rotation * playerRotation * Quaternion.Euler(0, 30f * steer, 0));

        //moving forward
        //bool movingForward = Vector3.Cross(transform.forward, playerBody.velocity).y < 0;

        //move in direction
        //playerBody.velocity = Quaternion.AngleAxis(Vector3.SignedAngle(playerBody.velocity, (movingForward ? 1f : 0f) * transform.forward, Vector3.up) * Drag, Vector3.up) * playerBody.velocity;

    }

    private void TakeInput()
    {
        Vector3 forceDirection = transform.forward;
        int steer = 0;
        playerSpeed = playerBody.velocity.magnitude;

        //steer direction [-1,0,1]
        if (Input.GetAxis("Horizontal") < 0) steer = 1;
        else if (Input.GetAxis("Horizontal") > 0) steer = -1;



        //Rotational Force
        playerBody.AddForceAtPosition(steer * transform.right * SteerPower / 100f, Motor.position);

        //compute vectors
        Vector3 forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);
        Vector3 targetVel = Vector3.zero;

        //forward/backward poewr
        if (Input.GetAxis("Vertical") > 0)
        {
            ApplyForceToReachVelocity(playerBody, forward * MaxSpeed, Power);
            motorEffect.SetActive(true);

        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            ApplyForceToReachVelocity(playerBody, forward * -MaxSpeed, Power);
        }
        else
        {
            StartCoroutine(SlowEffectStop());
        }
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

        rigidbody.AddTorque(rotationAxis * (radPerSecond - currentSpeed) * force );
    }

    public Vector3 QuaternionToAngularVelocity(Quaternion rotation)
    {
        float angleInDegrees;
        Vector3 rotationAxis;
        rotation.ToAngleAxis(out angleInDegrees, out rotationAxis);

        return rotationAxis * angleInDegrees * Mathf.Deg2Rad;
    }

}

