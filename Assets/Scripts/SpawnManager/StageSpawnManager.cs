using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSpawnManager : SpawnManager
{
    class StageBlockData : IBlockData
    {
        public int Column { get { return this.Cells.Length / this.Row; } }
        public int Row { get { return 5; } }

        public int[] Cells { get; } = new int[]{
            1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,
            1,0,0,0,0,1,0,0,1,0,1,0,1,0,0,0,1,0,0,
            1,1,1,0,0,1,0,0,1,1,1,0,1,0,1,0,1,1,1,
            0,0,1,0,0,1,0,0,1,0,1,0,1,0,1,0,1,0,0,
            1,1,1,0,0,1,0,0,1,0,1,0,1,1,1,0,1,1,1,
        };
    }


    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        var cubeOffsets = this.GetCubeOffsets(new StageBlockData());

        this.PutCubes(this.transform, cubeOffsets);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
