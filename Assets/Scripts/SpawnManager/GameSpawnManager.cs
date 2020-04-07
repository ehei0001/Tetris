using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawnManager : SpawnManager
{
    public GameObject nextBlockPoint;
    public bool IsReady
    {
        set { this.isReady = value; }
    }

    private bool isReady = false;
    private bool isCreating = false;
    private GameObject nextBlock;

    public GameObject PutBlock()
    {
        {
            if (this.nextBlock) {
                Destroy(this.nextBlock);
            }

            this.nextBlock = this.BuildNextBlock(this.nextBlockPoint.transform.position);
            this.nextBlock.GetComponent<GameBlock>().IsDummy = true;
        }

        return this.BuildBlock(this.transform.position);
    }

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
                PutBlock();

                this.isCreating = false;
            }
        }
    }
}
