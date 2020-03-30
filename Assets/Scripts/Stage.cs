using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    [SerializeField]
    private int floorCubeCount = 12;
    [SerializeField]
    private int obstacleLineCount = 0;
    [SerializeField]
    private GameObject leftWall;
    [SerializeField]
    private GameObject rightWall;
    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject cubePrefab;
    private readonly string[] materialNames = {
        "ITypeBlockData",
        "JTypeBlockData",
        "LTypeBlockData",
        "OTypeBlockData",
        "STypeBlockData",
        "TTypeBlockData",
        "ZTypeBlockData",
    };

    // Start is called before the first frame update
    void Start()
    {
        this.BuildFloor();
    }

    void BuildFloor()
    {
        var cubeSize = this.cubePrefab.GetComponent<Renderer>().bounds.size;
        var right = this.rightWall.transform.position.x - this.rightWall.GetComponent<Renderer>().bounds.size.x / 2;

        // obstacles create at random location
        {
            var position = this.rightWall.transform.position;
            var y = position.y;
            var z = position.z;

            for (int i = 0; i < this.obstacleLineCount; ++i)
            {
                var index = Random.Range(0, this.materialNames.Length);
                var materialName = this.materialNames[index];
                var material = Resources.Load<Material>("Materials/" + materialName);

                var cell = Instantiate(this.cubePrefab, new Vector3(right - cubeSize.x * i, y, z), Quaternion.identity);
                cell.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        // left wall position relocate
        {
            var transform = this.leftWall.transform;
            var left = transform.position.x + this.leftWall.GetComponent<Renderer>().bounds.size.x / 2;
            var floorWidth = cubeSize.x * this.floorCubeCount;

            var rightWallPosition = this.rightWall.transform.position;
            var x = transform.position.x + right - left - floorWidth;
            var y = rightWallPosition.y;
            var z = rightWallPosition.z;
            transform.position = new Vector3(x, y, z);
        }

        // floor position relocate
        {
            var bottom = this.leftWall.transform.position.y - this.leftWall.GetComponent<Renderer>().bounds.size.y / 2;
            var floorHalfHeight = this.floor.GetComponent<Renderer>().bounds.size.y / 2;
            var y = bottom - floorHalfHeight;

            var position = this.floor.transform.position;
            this.floor.transform.position = new Vector3(position.x, y, position.z);
        }
    }
}
