using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BlockData
{
#if DEBUG
    public BlockData()
    {
        Debug.Assert(this.Column == this.Row);
        Debug.Assert(this.Column == Mathf.Sqrt(this.Cells.Length));
    }
#endif


    public int Column { get { return (int)Mathf.Sqrt(this.Cells.Length); } }
    public int Row { get { return this.Column; } }
    public virtual int[] Cells { get; }
}

class ITypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[]{
        0,0,0,0,
        1,1,1,1,
        0,0,0,0,
        0,0,0,0,
    };
}

class JTypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[] {
        1,0,0,
        1,1,1,
        0,0,0,
    };
}

class LTypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[] {
        0,0,1,
        1,1,1,
        0,0,0,
    };
}

class OTypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[] {
        1,1,
        1,1,
    };
}

class STypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[] {
        0,1,1,
        1,1,0,
        0,0,0,
    };
}

class TTypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[] { 
        0,1,0, 
        1,1,1,
        0,0,0,
    };
}

class ZTypeBlockData : BlockData
{
    public override int[] Cells { get { return cells; } }

    private int[] cells = new int[] {
        1,1,0,
        0,1,1,
        0,0,0,
    };
}

public class SpawnManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject blockPrefab;
    public Vector3 cubeSize = new Vector3(10, 10, 10);

    private BlockData[] blockDatas = new BlockData[] {
        //new ITypeBlockData(),
        //new JTypeBlockData(),
        //new LTypeBlockData(),
        new OTypeBlockData(),
        //new STypeBlockData(),
        //new TTypeBlockData(),
        //new ZTypeBlockData(),
    };
    struct BlockOffset
    {
        public string name;
        public Vector3[] cellOffsets;
    }
    private BlockOffset[] blockOffsets;
    private Bounds cameraBounds;

    // Start is called before the first frame update
    protected void Start()
    {
        Debug.Assert(cubePrefab);
        Debug.Assert(blockPrefab);
        Debug.Assert(this.blockDatas.Length > 0);
        Debug.Assert(this.cubeSize.magnitude > 0);

        this.blockOffsets = this.UpdateBlockData(this.cubeSize);

        cubePrefab.transform.localScale = this.cubeSize;
        this.cameraBounds = this.GetCameraBounds();
    }

    protected GameObject BuildBlock(Vector3 position)
    {
        int index = Random.Range(0, this.blockOffsets.Length);
        var blockOffset = this.blockOffsets[index];
        var blockGameObject = Instantiate(blockPrefab, position, Quaternion.identity);

        {
            var gameBlock = blockGameObject.GetComponent<GameBlock>();

            if (gameBlock)
            {
                gameBlock.CubeSize = this.cubeSize;
            }
        }
        
        var cubeWidth = this.cubeSize.x;
        var cubeHeight = this.cubeSize.y;
        Debug.Assert(cubeWidth > 0 && cubeHeight > 0);

        var material = Resources.Load<Material>("Materials/" + blockOffset.name);
        Debug.Assert(material, blockOffset.name + " is not found");

        foreach(var offset in blockOffset.cellOffsets)
        {
            var cell = Instantiate(cubePrefab, Vector3.zero, cubePrefab.transform.rotation);
            cell.transform.parent = blockGameObject.transform;
            cell.transform.localPosition = offset;
            cell.GetComponent<Renderer>().sharedMaterial = material;
        }

        return blockGameObject;
    }

    Bounds GetCameraBounds()
    {
        // https://forum.unity.com/threads/calculating-world-position-of-screens-corners.9292/
        var camera = Camera.main;
        var canvas = GameObject.Find("Canvas");
        var depth = (canvas.transform.position.z - camera.transform.position.z);

        // Screens coordinate corner location
        var upperLeftScreen = new Vector3(0, Screen.height, depth );
        var upperRightScreen = new Vector3(Screen.width, Screen.height, depth);
        var lowerLeftScreen = new Vector3(0, 0, depth);
        var lowerRightScreen = new Vector3(Screen.width, 0, depth);
    
        //Corner locations in world coordinates
        var upperLeft = camera.ScreenToWorldPoint(upperLeftScreen); 
        var upperRight = camera.ScreenToWorldPoint(upperRightScreen);
        var lowerLeft = camera.ScreenToWorldPoint(lowerLeftScreen);
        var lowerRight = camera.ScreenToWorldPoint(lowerRightScreen);

        var size = upperRight - lowerLeft;
        var center = lowerLeft + size / 2;

        return new Bounds(center, size);
    }

    BlockOffset[] UpdateBlockData(Vector3 cubeSize)
    {
        var twoByTwoOffsets = new Vector3[] {
            new Vector3(-1, +1), new Vector3(+0, +1),
            new Vector3(-1, +0), new Vector3(+0, -0),
        };
        var threeByThreeOffsets = new Vector3[] {
            new Vector3(-1, +1), new Vector3(+0, +1), new Vector3(+1, +1),
            new Vector3(-1, +0), new Vector3(+0, +0), new Vector3(+1, +0),
            new Vector3(-1, -1), new Vector3(+0, -1), new Vector3(+1, -1),
        };
        var fourByFourOffsets = new Vector3[] {
            new Vector3(-1, +1), new Vector3(+0, +1), new Vector3(+1, +1), new Vector3(+2, +1),
            new Vector3(-1, +0), new Vector3(+0, +0), new Vector3(+1, +0), new Vector3(+2, +0),
            new Vector3(-1, -1), new Vector3(+0, -1), new Vector3(+1, -1), new Vector3(+2, -1),
            new Vector3(-1, -2), new Vector3(+0, -2), new Vector3(+1, -2), new Vector3(+2, -2),
        };
        var blockOffsets = new List<BlockOffset>();

        foreach (var blockData in this.blockDatas)
        {
            Debug.Assert(blockData.Row == blockData.Column);
            Vector3[] offsets;

            if (blockData.Row == 2)
            {
                offsets = twoByTwoOffsets;
            }
            else if(blockData.Row == 3)
            {
                offsets = threeByThreeOffsets;
            }
            else
            {
                Debug.Assert(blockData.Row == 4);

                offsets = fourByFourOffsets;
            }

            Debug.Assert(offsets.Length == blockData.Cells.Length);
            var cellOffsets = new List<Vector3>();

            for(var i = 0; i < blockData.Cells.Length; ++i)
            {
                if (blockData.Cells[i] == 1)
                {
                    var offset = offsets[i];
                    offset = new Vector3(offset.x * this.cubeSize.x, offset.y * this.cubeSize.y);
                    cellOffsets.Add(offset);
                }
            }

            Debug.Assert(cellOffsets.Count > 0);

            var blockOffset = new BlockOffset();
            blockOffset.name = blockData.GetType().Name;
            blockOffset.cellOffsets = cellOffsets.ToArray();
            blockOffsets.Add(blockOffset);
        }

        return blockOffsets.ToArray();
    }
}
