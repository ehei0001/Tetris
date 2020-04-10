using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStage : MonoBehaviour
{
    public int fillingLineCount = 0;
    public int floorCubeCount = 12;
    public int obstacleRowCount = 0;
    public float autoFillLineCreateTime = 3;
    public float freezeTime = 1;
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
    private bool isGameOver;
    private BannerSpawnManager bannerManager;
    private float autoFillLineElapsedTime;
    private bool isGameClear;
    private int score = 0;

    enum Banner {
        Start,
        GameOver,
        StageClear,
        Remove,
    }
    
    public void FreezeBlock(Transform blockTransform)
    {
        if (this.isGameOver)
        {
            return;
        }
        

        var updatedRows = new HashSet<int>();

        // block can collide by moving block to upside
        if(this.IsCollideBlock(blockTransform))
        {
            var y = this.GetTopCellY(blockTransform);

            blockTransform.GetComponent<GameBlock>().MoveUp(y);

            Debug.Log("Move Up");
        }

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
                Debug.Assert(!this.cellTransforms[cellIndex.y][cellIndex.x], "It'll cause lone block");
                this.cellTransforms[cellIndex.y][cellIndex.x] = childTransform;

                updatedRows.Add(cellIndex.y);
            }
        }

        var filledRows = this.GetFilledRows(updatedRows);

        this.ClearRows(filledRows);
        var isRemainLine = true;

        if (filledRows.Count > 0)
        {
            var score = (int)Mathf.Pow(filledRows.Count, 2);
            this.AddScore(score);

            var remainedLine = int.Parse(this.remaingLineText.text);
            remainedLine -= filledRows.Count;

            if(remainedLine > 0)
            {
                this.remaingLineText.text = remainedLine.ToString();
            }
            else
            {
                this.remaingLineText.text = "";
                isRemainLine = false;
            }
        }

        if (isRemainLine)
        {
            var gameObject = this.spawnManager.GetComponent<GameSpawnManager>().PutBlock();

            if(this.IsCollideBlock(gameObject.transform))
            {
                this.isGameOver = true;

                var y = this.GetTopCellY(gameObject.transform);
                var gameBlock = gameObject.GetComponent<GameBlock>();
                gameBlock.IsDummy = true;
                gameBlock.MoveUp(y);

                StartCoroutine(GameOver());
            }
        }
        else
        {
            this.isGameClear = true;

            StartCoroutine(this.PutBanner(Banner.StageClear));
            StartCoroutine(this.ClearStage());
        }

        {
            var gameObject = GameObject.Find("Particle System");
            gameObject.transform.position = blockTransform.position;
            gameObject.GetComponent<ParticleSystem>().Play();
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
            else if(cellIndex.y < 0)
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

    // get enable top point of block vertically
    float GetTopCellY(Transform blockTransform)
    {
        var columns = new HashSet<int>();

        foreach(Transform childTransform in blockTransform)
        {
            var index = this.GetCellIndex(childTransform.position);
            columns.Add(index.x);
        }

        for(var i = this.cellTransforms.Count - 1; i >= 0; --i)
        {
            var lineTransform = this.cellTransforms[i];

            foreach (var column in columns)
            {
                Debug.Assert(lineTransform.Length > column);

                var transform = lineTransform[column];

                if (transform)
                {
                    return transform.position.y;
                }
            }
        }

        return anchorPoint.y;
    }

    Vector2Int GetCellIndex(Vector3 position)
    {
        var cubeWidth = this.cubeSize.x;
        var cubeHeight = this.cubeSize.y;
        var localPosition = position - this.anchorPoint;
        var cellIndex = new Vector2(localPosition.x / cubeWidth, localPosition.y / cubeHeight);
        var x = (int)cellIndex.x;
        var y = (int)cellIndex.y - 1;

        return new Vector2Int(x, y);
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Banner", LoadSceneMode.Additive);

        this.cubeSize = this.cubePrefab.GetComponent<Renderer>().bounds.size;

        this.BuildFloor();
        StartCoroutine(this.Restart());
    }

    private void Update()
    {
        if (!this.isGameOver && !this.isGameClear)
        {
            if (this.autoFillLineElapsedTime > this.autoFillLineCreateTime)
            {
                this.autoFillLineElapsedTime = 0;

                this.FillBottom();
            }
            else
            {
                this.autoFillLineElapsedTime += Time.deltaTime;
            }
        }
    }

    void BuildFloor()
    {
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

        // Spawn Manager relocate
        {
            var transform = this.spawnManager.transform;
            var bias = 3;
            var x = left + (right - left) / 2 - cubeWidth / 2 + bias;

            transform.position = new Vector3(x, transform.position.y, this.rightWall.transform.position.z);
        }

        // text relocate
        {
            var names = new string[] { "Remain Lines", "Score" };

            foreach (var name in names)
            {
                var gameObject = GameObject.Find(name);
                var width = gameObject.GetComponent<RectTransform>().rect.width;
                var position = gameObject.transform.position;
                gameObject.transform.position = new Vector3(left - width / 2, position.y, position.z);
            }
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

    IEnumerator PutBanner(Banner banner)
    {
        var manager = this.bannerManager;

        while (!manager)
        {
            var gameObject = GameObject.Find("Banner Manager");

            if (gameObject)
            {
                this.bannerManager = manager = gameObject.GetComponent<BannerSpawnManager>();
                break;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        switch (banner) {
            case Banner.Start:
                {
                    bannerManager.PutStart(1);
                    break;
                }
            case Banner.GameOver:
                {
                    bannerManager.PutGameOver(0);
                    break;
                }
            case Banner.StageClear:
                {
                    bannerManager.PutStageClear(1);
                    break;
                }
            case Banner.Remove:
                {
                    bannerManager.ClearBanner(0);
                    break;
                }
            default:
                Debug.Assert(false);
                break;
        }

        yield return null;
    }

    IEnumerator ClearStage()
    {
        var spawnManagerPosition = this.spawnManager.transform.position;
        var height = spawnManagerPosition.y - this.floor.transform.position.y;
        var rowCount = (int)(height / this.cubeSize.y) - this.cellTransforms.Count - 1;

        var lineStock = new GameObject("Line Stock");

        for (var row = 0; row < rowCount; ++row)
        {
            var gameObject = new GameObject("Line");
            gameObject.transform.parent = lineStock.transform;
            var y = spawnManagerPosition.y - this.cubeSize.y * row;
            gameObject.transform.position = new Vector3(spawnManagerPosition.x - this.cubeSize.x / 2, y, spawnManagerPosition.z);

            var materialIndex = row % this.materialNames.Length;
            var materialName = this.materialNames[materialIndex];
            var material = Resources.Load<Material>("Materials/" + materialName);
            Debug.Assert(material, materialName + " is not found");

            var cube = Instantiate(this.cubePrefab, gameObject.transform);
            cube.transform.localScale = new Vector3(this.cubeSize.x * this.floorCubeCount, this.cubeSize.y, this.cubeSize.z);
            cube.GetComponent<Renderer>().sharedMaterial = material;

            this.AddScore(10 * row);

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1);

        yield return this.Restart();
    }

    IEnumerator Restart()
    {
        this.fillingLineCount += 1;
        this.obstacleRowCount += 1;
        this.autoFillLineCreateTime *= 0.8f;
        this.autoFillLineElapsedTime = 0;
        this.freezeTime *= 0.9f;
        this.isGameClear = false;

        // line clear
        {
            var gameObject = GameObject.Find("Line Stock");

            if (gameObject)
            {
                Destroy(gameObject);
            }
        }

        // remain cube clear
        if(this.cellTransforms.Count > 0)
        {
            foreach (var lineTransforms in this.cellTransforms)
            {
                foreach(var transform in lineTransforms)
                {
                    if (transform)
                    {
                        Destroy(transform.gameObject);
                    }
                }
            }

            this.cellTransforms.Clear();
        }

        // obstacles create at random location
        if (this.obstacleRowCount > 0)
        {
            var cellTransforms = new Dictionary<Vector2Int, Transform>();

            for (int row = 0; row < this.obstacleRowCount; ++row)
            {
                for (int column = 0; column < this.floorCubeCount; ++column)
                {
                    var isEnable = ( 5 < UnityEngine.Random.Range(0, 10));

                    if (isEnable)
                    {
                        var materialIndex = UnityEngine.Random.Range(0, this.materialNames.Length);
                        var materialName = this.materialNames[materialIndex];
                        var material = Resources.Load<Material>("Materials/" + materialName);

                        var x = anchorPoint.x + this.cubeSize.x * column;
                        var y = anchorPoint.y + this.cubeSize.x * (row + 1);
                        var cell = Instantiate(this.cubePrefab, new Vector3(x, y, anchorPoint.z), Quaternion.identity);
                        cell.GetComponent<Renderer>().sharedMaterial = material;

                        var cellIndex = this.GetCellIndex(cell.transform.position);
                        cellTransforms.Add(cellIndex, cell.transform);
                    }
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

        {
            var spawnManager = this.spawnManager.GetComponent<GameSpawnManager>();
            spawnManager.FreezeTime = this.freezeTime;
            
            var block = spawnManager.PutBlock();

            if (block)
            {
                if (this.IsCollideBlock(block.transform))
                {
                    block.GetComponent<GameBlock>().MoveUp(0);
                }
            }

            
        }

        yield return this.PutBanner(Banner.Start);
    }

    IEnumerator GameOver()
    {
        yield return this.PutBanner(Banner.GameOver);
        yield return new WaitForSeconds(3);
        yield return SceneManager.LoadSceneAsync("Title");
    }

    void AddScore(int score)
    {
        score += this.score;

        this.scoreText.text = Convert.ToString(score, 16).ToLower();
    }

    void FillBottom()
    {
        var offset = this.cubeSize.y;
        var material = Resources.Load<Material>("Materials/Wall/Wall");
        var cellIndex = this.GetCellIndex(anchorPoint);
        ReserveCellMap(cellIndex.y);
        var transforms = new Transform[this.floorCubeCount];
        this.cellTransforms.Insert(0, transforms);

        for (int i = 0; i < this.floorCubeCount; ++i)
        {
            var x = anchorPoint.x + this.cubeSize.x * i;
            var y = anchorPoint.y;
            var cell = Instantiate(this.cubePrefab, new Vector3(x, y, anchorPoint.z), Quaternion.identity);
            cell.GetComponent<Renderer>().sharedMaterial = material;
            transforms[i] = cell.transform;
        }

        foreach (var line in this.cellTransforms)
        {
            foreach (var transform in line)
            {
                if (transform)
                {
                    transform.position += new Vector3(0, offset);
                }
            }
        }

        foreach (var block in GameObject.FindGameObjectsWithTag("Block"))
        {
            var gameBlock = block.GetComponent<GameBlock>();

            if (!gameBlock.IsDummy)
            {
                while (this.IsCollideBlock(block.transform))
                {
                    gameBlock.MoveUp(0);
                }
            }
        }
    }
}
