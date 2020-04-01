using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBlock : MonoBehaviour
{
    public float freezeTime = 2;
    public float autoDownTime = 1;

    public Vector3 CubeSize { set { this.cubeSize = value; } }

    private Vector3 cubeSize;
    private Vector3 lastPosition = new Vector3();
    private float freezingElaspedTime;
    private float autoDownElaspedTime;
    private Stage stage;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(this.freezeTime > this.autoDownTime);

        {
            var gameObject = GameObject.Find("Stage");
            this.stage = gameObject.GetComponent<Stage>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // it position is unchanged during n seconds, it'll freeze
        //if(this.lastPosition == this.transform.position)
        //{
        //    this.freezingElaspedTime += Time.deltaTime;

        //    if(this.freezeTime < freezingElaspedTime)
        //    { 
        //        this.stage.FreezeBlock(this.transform);
                
        //        // next block ready
        //        {
        //            var gameObject = GameObject.Find("Spawn Manager");
        //            Debug.Assert(gameObject);
        //            gameObject.GetComponent<GameSpawnManager>().PutBlock();
        //        }

        //        Destroy(this.gameObject, 0.1f);
        //    }
        //} else
        //{
        //    this.freezingElaspedTime = 0;
        //    this.lastPosition = this.transform.position;
        //}

        //if(this.autoDownElaspedTime > this.autoDownTime)
        //{
        //    this.MoveDown();
        //    this.autoDownElaspedTime = 0;
        //} else
        //{
        //    this.autoDownElaspedTime += Time.deltaTime;
        //}

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.Rotate();
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.MoveDown();
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.MoveLeft();
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.MoveRight();
        }
    }

    void Rotate()
    {
        var angles = this.transform.eulerAngles;
        this.transform.eulerAngles += new Vector3(0, 0, 90);

        if (this.stage.IsCollideBlock(this.transform))
        {
            this.transform.eulerAngles -= new Vector3(0, 0, -90);
        }
    }

    void MoveLeft()
    {
        this.MoveSide(false);
    }

    void MoveRight()
    {
        this.MoveSide(true);
    }

    void MoveDown()
    {
        var offset = this.cubeSize.y;
        this.transform.position -= new Vector3(0, offset);

        if (this.stage.IsCollideBlock(this.transform))
        {
            this.transform.position += new Vector3(0, offset);
        }
    }

    void MoveSide(bool isRight)
    {
        var direction = (isRight ? 1 : -1);
        var offset = this.cubeSize.x * direction;
        this.transform.position += new Vector3(offset, 0);

        if (this.stage.IsCollideBlock(this.transform))
        {
            this.transform.position -= new Vector3(offset, 0);
        }
    }
}
