using System;
using Constants;
using Module.Enemy.Hose;
using UnityEngine;

public class WaterPusher : MonoBehaviour
{
    [SerializeField] private HoseBehaviour hoseBehaviour;
    private Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(Tag.Handle.Player))
        {
            ApplyWaterForce();
        }
    }

    private void ApplyWaterForce()
    {
        HoseWater hoseWater = hoseBehaviour.HoseWater;
        Vector3 verticalForce = hoseWater.CalculateVerticalForce(true);
        Vector3 horizontalForce = hoseWater.CalculateHorizontalForce(true, rigidBody.position);
        Vector3 force = verticalForce + horizontalForce;

        rigidBody.AddForce(force);
    }
}