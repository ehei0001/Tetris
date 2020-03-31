﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawnManager : SpawnManager
{
    public bool IsReady {
        set { this.isReady = value; }
    }

    private bool isReady = false;
    private bool isCreating = false;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        this.isCreating = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isReady)
        {
            if (this.isCreating)
            {
                this.BuildBlock(this.transform.position);

                this.isCreating = false;
            }
        }
    }
}
