using System;
using System.Linq;
using SLZ.Props.Weapons;
using UnityEngine;
using UnityEngine.Serialization;

[Elixir]
public class CrossBow : MonoBehaviour
{
    public Gun gun;
    [Space] 
    public Transform fireOrigin;
    public Transform fireDirection;
    public float fireDistance;
    public LayerMask mask;
    [Space]
    
    //private bool _wasFiredLastFrame;

    public bool firing;

    private Action _fireAction;
    
    private void Start()
    {
        _fireAction = () => OnFire();
        gun.onFireDelegate += _fireAction;
    }

    private void OnDestroy()
    {
        gun.onFireDelegate -= _fireAction;
    }

    private void OnFire()
    {
        firing = true;
    }


    private void FixedUpdate()
    {
        if (firing)
        {
            Shoot();
            firing = false;
        }
    }

    void Shoot()
    {
        var position = fireOrigin.position;
        var hits = Physics.RaycastAll(position, fireDirection.position - position, 
            fireDistance, mask);

        var sortedHits = hits.OrderBy(o => o.distance).ToList();

        //Only pin rigidbodies to another object!
        if (sortedHits.Count < 2 || !sortedHits.FirstOrDefault().rigidbody)
        {
            return;
        }

        Rigidbody pinnedRb = sortedHits[0].rigidbody;

        //Set up connected rb
        GameObject wallObj = new GameObject
        {
            transform =
            {
                parent = sortedHits[1].transform,
                position = sortedHits[1].point
            },
            name = $"[{pinnedRb.name}] pin"
        };

        Rigidbody wallRb = wallObj.AddComponent<Rigidbody>();
        wallRb.isKinematic = true;


        SetUpJoint(pinnedRb, wallRb);
    }

    void SetUpJoint(Rigidbody targetRb, Rigidbody wallRb)
    {
        var joint = wallRb.gameObject.AddComponent<ConfigurableJoint>();

        //Joint setup :)
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;


        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        joint.connectedBody = targetRb;
    }
}