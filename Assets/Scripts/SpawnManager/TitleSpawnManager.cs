using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSpawnManager : SpawnManager
{
    [SerializeField]
    private float torqueStrength = 5;

    private new void Start()
    {
        base.Start();

        InvokeRepeating("SpawnBlock", 0, 2);
    }

    void SpawnBlock()
    {
        var block = this.BuildBlock();
        block.GetComponent<Rigidbody>().AddTorque(Vector3.forward * this.torqueStrength);
    }
}
