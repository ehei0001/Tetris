using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBlock : MonoBehaviour
{
    public float freezeTime = 2;
    public float autoDownTime = 1;

    public Vector3 CubeSize { set { this.cubeSize = value; } }
    public bool IsDummy
    {
        set { this.isDummy = value; }
    }

    private Vector3 cubeSize;
    private Vector3 lastPosition = new Vector3();
    private float freezingElaspedTime;
    private float autoDownElaspedTime;
    private GameStage stage;
    private bool isForcedDropping;
    private bool isDummy;

    enum Direction 
    { 
        Up,
        Down,
        Left,
        Right,
    };

    public void MoveUp()
    {
        this.Move(Direction.Up);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(this.freezeTime > this.autoDownTime);

        {
            var gameObject = GameObject.Find("Stage");
            this.stage = gameObject.GetComponent<GameStage>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isDummy)
        {
            return;
        }

        Debug.Assert(this.enabled);

        // it position is unchanged during n seconds, it'll freeze
        if (this.lastPosition == this.transform.position)
        {
            this.freezingElaspedTime += Time.deltaTime;

            if (this.freezeTime < freezingElaspedTime)
            {
                this.stage.FreezeBlock(this.transform);

                Destroy(this.gameObject);
            }
        }
        else
        {
            this.freezingElaspedTime = 0;
            this.lastPosition = this.transform.position;
        }

        if (this.isForcedDropping)
        {
            this.Move(Direction.Down);
        }
        else
        {
            if (this.autoDownElaspedTime > this.autoDownTime)
            {
                this.Move(Direction.Down);
                this.autoDownElaspedTime = 0;
            }
            else
            {
                this.autoDownElaspedTime += Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //var offset = -this.cubeSize.y;
            //this.transform.position -= new Vector3(0, offset);
            this.Rotate();
        } 
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.Move(Direction.Down);
        } 
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.Move(Direction.Left);
        } 
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.Move(Direction.Right);
        } 
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            this.isForcedDropping = true;
        }
    }

    void Rotate()
    {
        var angles = this.transform.eulerAngles;
        this.transform.eulerAngles += new Vector3(0, 0, 90);

        if (this.stage.IsCollideBlock(this.transform))
        {
            this.transform.eulerAngles += new Vector3(0, 0, -90);
        }
    }

    void Move(Direction direction)
    {
        switch (direction) 
        {
            case Direction.Up:
            case Direction.Down:
                {
                    var bias = (direction == Direction.Down ? 1 : -1);
                    var offset = this.cubeSize.y * bias;
                    this.transform.position -= new Vector3(0, offset);

                    if (this.stage.IsCollideBlock(this.transform))
                    {
                        this.transform.position += new Vector3(0, offset);
                    }

                    break;
                }
            case Direction.Left:
            case Direction.Right:
                {
                    var bias = (Direction.Right == direction ? 1 : -1);
                    var offset = this.cubeSize.x * bias;
                    this.transform.position += new Vector3(offset, 0);

                    if (this.stage.IsCollideBlock(this.transform))
                    {
                        this.transform.position -= new Vector3(offset, 0);
                    }

                    break;
                }
            default:
                Debug.Assert(false);
                break;
        }
    }
}
