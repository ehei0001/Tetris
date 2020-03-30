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
        1, 1,
        1, 1,
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
    [SerializeField]
    private GameObject cubePrefab;
    [SerializeField]
    private GameObject blockPrefab;

    private BlockData[] blockDatas = new BlockData[] {
        new ITypeBlockData(),
        new JTypeBlockData(),
        new LTypeBlockData(),
        new OTypeBlockData(),
        new STypeBlockData(),
        new TTypeBlockData(),
        new ZTypeBlockData(),
    };
    private Vector3 cubeSize;
    private Bounds cameraBounds;

    // Start is called before the first frame update
    protected void Start()
    {
        Debug.Assert(cubePrefab);
        Debug.Assert(blockPrefab);
        Debug.Assert(this.blockDatas.Length > 0);

        cubeSize = cubePrefab.GetComponent<Renderer>().bounds.size;
        cameraBounds = this.GetCameraBounds();
    }

    protected GameObject BuildBlock()
    {
        int index = Random.Range(0, this.blockDatas.Length);
        var blockData = this.blockDatas[index];
        Debug.Assert(blockData.Column * blockData.Row == blockData.Cells.Length);
        Debug.Assert(blockData.Cells.Length > 0);

        var block = Instantiate(blockPrefab, this.transform);

        var cubeWidth = this.cubeSize.x;
        var cubeHeight = this.cubeSize.y;
        Debug.Assert(cubeWidth > 0 && cubeHeight > 0);

        var left = Random.Range(this.cameraBounds.min.x, this.cameraBounds.max.x);
        var x = (cubeWidth * blockData.Column) / -2;
        var y = (cubeHeight * blockData.Row) / -2;
        var materialName = blockData.GetType().Name;
        var material = Resources.Load<Material>("Materials/" + materialName);
        Debug.Assert(material, materialName + " is not found");

        for(int row = 0;row < blockData.Row; ++row)
        {
            for(int column = 0; column < blockData.Column; ++column)
            {
                var cellIndex = row * blockData.Column + column;
                Debug.Assert(blockData.Cells.Length > cellIndex, blockData.GetType().Name + " has invalid data");
                var isCreating = blockData.Cells[cellIndex];

                if(isCreating > 0)
                {
                    var cellX = x + cubeWidth * column;
                    var cellY = y + cubeHeight * row;
                    var cell = Instantiate(cubePrefab, Vector3.zero, cubePrefab.transform.rotation);
                    cell.transform.parent = block.transform;
                    cell.transform.localPosition = new Vector3(cellX, cellY);
                    cell.GetComponent<Renderer>().sharedMaterial = material;
                }
            }
        }

        return block;
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
}
