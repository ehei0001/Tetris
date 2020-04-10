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
    private int nextBlockIndex;

    public GameObject BuildBlock(Vector3 position)
    {
        var index = this.nextBlockIndex;
        this.nextBlockIndex = Random.Range(0, this.blockDatas.Length);

        return this.CreateBlock(position, index);
    }

    public GameObject BuildNextBlock(Vector3 position)
    {
        return this.CreateBlock(position, this.nextBlockIndex);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        Debug.Assert(cubePrefab);
        Debug.Assert(blockPrefab);
        Debug.Assert(this.blockDatas.Length > 0);
        Debug.Assert(this.cubeSize.magnitude > 0);

        this.blockOffsets = this.UpdateBlockData(this.cubeSize);
        this.cubePrefab.transform.localScale = this.cubeSize;
        this.nextBlockIndex = Random.Range(0, this.blockOffsets.Length);
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

    GameObject CreateBlock(Vector3 position, int index)
    {
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

        foreach (var offset in blockOffset.cellOffsets)
        {
            var cell = Instantiate(cubePrefab, Vector3.zero, cubePrefab.transform.rotation);
            cell.transform.parent = blockGameObject.transform;
            cell.transform.localPosition = offset;
            cell.GetComponent<Renderer>().sharedMaterial = material;
        }

        return blockGameObject;
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
