using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawnManager : SpawnManager
{
    public float FreezeTime
    {
        set
        {
            this.freezeTime = value;
        }
    }

    public GameObject nextBlockPoint;
    public bool IsReady
    {
        set { this.isReady = value; }
    }

    private bool isReady = false;
    private bool isCreating = false;
    private GameObject nextBlock;
    private float freezeTime = 1.0f;

    public GameObject PutBlock()
    {
        if (!this.isReady)
        {
            this.IsReady = true;
            return null;
        }

        var block = this.BuildBlock(this.transform.position);
        block.GetComponent<GameBlock>().FreezeTime = this.freezeTime;

        {
            if (this.nextBlock) {
                Destroy(this.nextBlock);
            }

            this.nextBlock = this.BuildNextBlock(this.nextBlockPoint.transform.position);
            this.nextBlock.GetComponent<GameBlock>().IsDummy = true;
        }

        return block;
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
