using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSpawnManager : SpawnManager
{
    public float torqueStrength = 5;
    public GameObject canvas;
    public int removingSeconds = 10;

    private float planeDistance;

    new void Start()
    {
        base.Start();

        this.planeDistance = canvas.GetComponent<Canvas>().planeDistance;

        InvokeRepeating("SpawnBlock", 0, 1);
    }

    void SpawnBlock()
    {
        var position = this.transform.position;
        var left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, -this.planeDistance));
        var right = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, -this.planeDistance));
        var x = Random.Range(left.x, right.x);

        var block = this.BuildBlock(new Vector3(x, position.y, position.z));
        Destroy(block, this.removingSeconds);

        var rigidBody = block.GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        rigidBody.AddTorque(Vector3.forward * this.torqueStrength, ForceMode.Impulse);
    }
}
