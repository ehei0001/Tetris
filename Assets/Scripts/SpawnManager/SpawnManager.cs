using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject blockPrefab;
    public Vector3 cubeSize = new Vector3(10, 10, 10);

    protected struct BlockOffset
    {
        public string name;
        public Vector3[] cellOffsets;
    }
    protected interface IBlockData
    {
        int Column { get; }
        int Row { get; }
        int[] Cells { get; }
    }

    class BlockData : IBlockData 
    { 
        public int Column { get { return (int)Mathf.Sqrt(this.Cells.Length); } }
        public int Row { get { return this.Column; } }
        public virtual int[] Cells { get; }
    }

    class ITypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[]{
            0,0,0,0,
            1,1,1,1,
            0,0,0,0,
            0,0,0,0,
        };
    }

    class JTypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[] {
            1,0,0,
            1,1,1,
            0,0,0,
        };
    }

    class LTypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[] {
            0,0,1,
            1,1,1,
            0,0,0,
        };
    }

    class OTypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[] {
            1,1,
            1,1,
        };
    }

    class STypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[] {
            0,1,1,
            1,1,0,
            0,0,0,
        };
    }

    class TTypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[] {
            0,1,0,
            1,1,1,
            0,0,0,
        };
    }

    class ZTypeBlockData : BlockData
    {
        public override int[] Cells { get { return cells; } }

        private readonly int[] cells = new int[] {
            1,1,0,
            0,1,1,
            0,0,0,
        };
    }

    private BlockData[] blockDatas = new BlockData[] {
        new ITypeBlockData(),
        new JTypeBlockData(),
        new LTypeBlockData(),
        new OTypeBlockData(),
        new STypeBlockData(),
        new TTypeBlockData(),
        new ZTypeBlockData(),
    };
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

    protected Vector3[] GetCubeOffsets(IBlockData blockData)
    {
        var rowCount = blockData.Row;
        var columnCount = blockData.Column;
        var rowOffset = rowCount / 2;
        var columnOffset = columnCount / 2;
        var offsets = new Vector3[blockData.Cells.Length];

        for (var row = 0; row < rowCount; ++row)
        {
            for (var column = 0; column < columnCount; ++column)
            {
                var index = (rowCount - row - 1) * columnCount + column;
                var offset = new Vector3(column - columnOffset, row - rowOffset);
                offsets[index] = new Vector3(offset.x * this.cubeSize.x, offset.y * this.cubeSize.y);
            }
        }

        var compactOffsets = new List<Vector3>();

        for (var i = 0; i < blockData.Cells.Length; ++i)
        {
            if (blockData.Cells[i] == 1)
            {
                var offset = offsets[i];
                compactOffsets.Add(offset);
            }
        }

        return compactOffsets.ToArray();
    }

    protected void PutCubes(Transform parentTransform, Vector3[] cubeOffsets)
    {   
        foreach(var cubeOffset in cubeOffsets)
        {
            var index = Random.Range(0, this.blockDatas.Length);
            var blockData = this.blockDatas[index];
            var materialName = "Materials/" + blockData.GetType().Name;
            var material = Resources.Load<Material>(materialName);
            Debug.Assert(material, materialName + " is not found");

            var cube = Instantiate(cubePrefab, Vector3.zero, cubePrefab.transform.rotation);
            cube.transform.parent = parentTransform;
            cube.transform.localPosition = cubeOffset;
            cube.GetComponent<Renderer>().sharedMaterial = material;
        }
    }


    Bounds GetCameraBounds()
    {
        // https://forum.unity.com/threads/calculating-world-position-of-screens-corners.9292/
        var camera = Camera.main;
        var canvas = GameObject.Find("Canvas");

        if (canvas)
        {
            var depth = (canvas.transform.position.z - camera.transform.position.z);

            // Screens coordinate corner location
            var upperLeftScreen = new Vector3(0, Screen.height, depth);
            var upperRightScreen = new Vector3(Screen.width, Screen.height, depth);
            var lowerLeftScreen = new Vector3(0, 0, depth);
            var lowerRightScreen = new Vector3(Screen.width, 0, depth);

            //Corner locations in world coordinates
            //var upperLeft = camera.ScreenToWorldPoint(upperLeftScreen);
            var upperRight = camera.ScreenToWorldPoint(upperRightScreen);
            var lowerLeft = camera.ScreenToWorldPoint(lowerLeftScreen);
            //var lowerRight = camera.ScreenToWorldPoint(lowerRightScreen);

            var size = upperRight - lowerLeft;
            var center = lowerLeft + size / 2;

            return new Bounds(center, size);
        }
        else
        {
            return new Bounds();
        }
    }

    BlockOffset[] UpdateBlockData(Vector3 cubeSize)
    {
        var blockOffsets = new BlockOffset[this.blockDatas.Length];

        for( var i = 0; i < this.blockDatas.Length; ++i)
        {
            var blockData = this.blockDatas[i];
            var blockOffset = new BlockOffset
            {
                name = blockData.GetType().Name,
                cellOffsets = this.GetCubeOffsets(blockData)
            };
            blockOffsets[i] = blockOffset;
        }

        return blockOffsets;
    }
}
