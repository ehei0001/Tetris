using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawnManager : SpawnManager
{
    private bool isCreating = false;

    // Start is called before the first frame update
    void Start()
    {
        this.isCreating = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isCreating)
        {
            this.BuildBlock();

            this.isCreating = false;
        }
    }
}
