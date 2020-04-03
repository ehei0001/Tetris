using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameStage : MonoBehaviour
{
    public int fillingLineCount = 5;
    public int floorCubeCount = 12;
    public int obstacleRowCount = 0;
    public GameObject leftWall;
    public GameObject rightWall;
    public GameObject floor;
    public GameObject cubePrefab;
    public GameObject spawnManager;
    public TextMeshProUGUI remaingLineText;
    public TextMeshProUGUI scoreText;

    private readonly string[] materialNames = {
        "ITypeBlockData",
        "JTypeBlockData",
        "LTypeBlockData",
        "OTypeBlockData",
        "STypeBlockData",
        "TTypeBlockData",
        "ZTypeBlockData",
    };
    private List<Transform[]> cellTransforms = new List<Transform[]>();
    private Vector3 anchorPoint;
    private Vector3 cubeSize;
    private int stateCount = 0;

    public void FreezeBlock(Transform blockTransform)
    {
        var updatedRows = new HashSet<int>();

        // detach all children and atttach to stage
        for (var i = blockTransform.childCount - 1; i >= 0; --i)
        {
            var childTransform = blockTransform.GetChild(i);
            var position = childTransform.position;
            childTransform.parent = this.transform;
            childTransform.position = position;

            // update cell to map
            {
                var cellIndex = this.GetCellIndex(position);

                this.ReserveCellMap(cellIndex.y);
                this.cellTransforms[cellIndex.y][cellIndex.x] = childTransform;

                updatedRows.Add(cellIndex.y);
            }
        }

        var filledRows = this.GetFilledRows(updatedRows);

        this.ClearRows(filledRows);

        if (filledRows.Count > 0)
        {
            var score = (int)Mathf.Pow(filledRows.Count, 2);
            
            this.scoreText.text = Convert.ToString(score, 16).ToLower();

            var remainedLine = int.Parse(this.remaingLineText.text);
            remainedLine -= filledRows.Count;

            if(remainedLine > 0)
            {
                this.remaingLineText.text = remainedLine.ToString();
            }
            else
            {
                // TODO:stage clear guide
            }
        }
    }

    public bool IsCollideBlock(Transform transform)
    {
        for(var i = 0; i < transform.childCount; ++i)
        {
            var childTransform = transform.GetChild(i);
            var cellIndex = this.GetCellIndex(childTransform.position);

            if(cellIndex.x < 0 || cellIndex.x >= this.floorCubeCount)
            {
                return true;
            } 
            else if(cellIndex.y <= 0)
            {
                return true;
            } 
            else if(cellIndex.y < this.cellTransforms.Count)
            {
                if(this.cellTransforms[cellIndex.y][cellIndex.x] != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    Vector2Int GetCellIndex(Vector3 position)
    {
        var cubeWidth = this.cubeSize.x;
        var cubeHeight = this.cubeSize.y;
        var localPosition = position - this.anchorPoint;
        var cellIndex = new Vector2(localPosition.x / cubeWidth, localPosition.y / cubeHeight);
        var x = (int)cellIndex.x;
        var y = (int)cellIndex.y;

        return new Vector2Int(x, y);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.cubeSize = this.cubePrefab.GetComponent<Renderer>().bounds.size;

        this.BuildFloor();
    }

    void BuildFloor()
    {
        this.cellTransforms.Clear();

        var cubeWidth = this.cubeSize.x;
        var cubeHeight = this.cubeSize.y;
        var right = this.rightWall.transform.position.x - this.rightWall.GetComponent<Renderer>().bounds.size.x / 2;
        var leftWallSize = this.leftWall.GetComponent<Renderer>().bounds.size;
        var left = this.leftWall.transform.position.x + leftWallSize.x / 2;
        var floorHalfHeight = this.floor.GetComponent<Renderer>().bounds.size.y / 2;

        // left wall position relocate
        {
            var transform = this.leftWall.transform;
            var floorWidth = cubeWidth * this.floorCubeCount;

            var rightWallPosition = this.rightWall.transform.position;
            var x = transform.position.x + right - left - floorWidth;
            var y = rightWallPosition.y;
            var z = rightWallPosition.z;
            transform.position = new Vector3(x, y, z);
        }

        // floor position relocate
        {
            var bottom = this.leftWall.transform.position.y - leftWallSize.y / 2;
            var y = bottom - floorHalfHeight;

            var position = this.floor.transform.position;
            this.floor.transform.position = new Vector3(position.x, y, position.z);
        }

        // anchor point update
        {
            var position = this.rightWall.transform.position;
            var x = this.leftWall.transform.position.x + leftWallSize.x / 2 + cubeWidth / 2;
            var y = this.floor.transform.position.y + floorHalfHeight - cubeHeight / 2;
            var z = position.z;

            this.anchorPoint = new Vector3(x, y, z);

            Debug.Log("anchor point:" + this.anchorPoint);
        }

        // Spawn Manager reloate
        {
            var transform = this.spawnManager.transform;
            var bias = 3;
            var x = left + (right - left) / 2 - cubeWidth / 2 + bias;

            transform.position = new Vector3(x, transform.position.y, this.rightWall.transform.position.z);
            this.spawnManager.GetComponent<GameSpawnManager>().IsReady = true;
        }

        // obstacles create at random location
        if ( this.obstacleRowCount > 0 )
        {
            var cellTransforms = new Dictionary<Vector2Int, Transform>();

            for (int row = 0; row < this.obstacleRowCount; ++row)
            {
                for (int column = 0; column < this.floorCubeCount; ++column)
                {
                    var materialIndex = UnityEngine.Random.Range(0, this.materialNames.Length);
                    var materialName = this.materialNames[materialIndex];
                    var material = Resources.Load<Material>("Materials/" + materialName);

                    var x = anchorPoint.x + cubeWidth * column;
                    var y = anchorPoint.y + cubeWidth * row;
                    var cell = Instantiate(this.cubePrefab, new Vector3(x, y, anchorPoint.z), Quaternion.identity);
                    cell.GetComponent<Renderer>().sharedMaterial = material;

                    var cellIndex = this.GetCellIndex(cell.transform.position);
                    cellTransforms.Add(cellIndex, cell.transform);
                }
            }

            foreach (var item in cellTransforms)
            {
                var cellIndex = item.Key;

                this.ReserveCellMap(cellIndex.y);

                this.cellTransforms[cellIndex.y][cellIndex.x] = item.Value;
            }
        }

        // remaining line update
        {
            var remaingLine = this.stateCount + this.fillingLineCount;
            this.remaingLineText.text = remaingLine.ToString();
        }
    }

    void ReserveCellMap(int maxRow)
    {
        for (var row = this.cellTransforms.Count; row < maxRow + 1; ++row)
        {
            this.cellTransforms.Add(new Transform[this.floorCubeCount]);
        }
    }

    List<int> GetFilledRows(HashSet<int> updatedRows)
    {
        var filledLines = new List<int>();

        foreach (var row in updatedRows)
        {
            Debug.Assert(this.cellTransforms.Count > row);

            var filled = true;

            foreach (var transform in this.cellTransforms[row])
            {
                if(transform == null)
                {
                    filled = false;
                    break;
                }
            }

            if (filled)
            {
                filledLines.Add(row);
            }
        }

        return filledLines;
    }

    void ClearRows(List<int> clearingRows)
    {
        var clearingUnorderdRows = new HashSet<int>(clearingRows);

        if (clearingRows.Count > 0)
        {
            var firstRow = clearingRows[clearingRows.Count - 1];
            Debug.Assert(firstRow >= clearingRows[0]);
            var offset = 0.0f;

            for (var row = firstRow; row < this.cellTransforms.Count; ++row)
            {
                var cellTransformsAtRow = this.cellTransforms[row];

                // cell destroy
                if (clearingUnorderdRows.Contains(row))
                {
                    foreach (var transform in cellTransformsAtRow)
                    {
                        //transform.GetComponent<BoxCollider>().enabled = true;
                        //transform.GetComponent<Rigidbody>().AddForce(Vector3.right * 100, ForceMode.Impulse);

                        Destroy(transform.gameObject);
                    }

                    offset -= this.cubeSize.y;
                }
                // cell relocate
                else
                {
                    foreach (var transform in cellTransformsAtRow)
                    {
                        if (transform)
                        {
                            transform.position += new Vector3(0, offset, 0);
                        }
                    }
                }
            }

            // data remove
            {
                clearingRows.Reverse();
                Debug.Assert(clearingRows[0] >= clearingRows[clearingRows.Count - 1]);

                foreach (var line in clearingRows)
                {
                    this.cellTransforms.RemoveAt(line);
                }
            }
        }
    }
}
