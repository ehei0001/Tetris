using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBlock : MonoBehaviour
{
    public Vector3[] CellOffsets  
    { 
        set 
        {
            this.cellOffsets = new Vector3[value.Length];
            value.CopyTo(this.cellOffsets, 0); 
        } 
    }

    public Vector3 CubeSize { set { this.cubeSize = value; } }

    private string lastCollisionTag;
    private Side side = Side.East;
    private Vector3[] cellOffsets;
    private Vector3 cubeSize;

    enum Side 
    {
        North,
        East,
        South,
        West,
        Max,
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // it position is unchanged during n seconds, it'll freeze

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

    private void OnCollisionEnter(Collision collision)
    {
        this.lastCollisionTag = collision.gameObject.tag;
        Debug.Log("OnCollisionEnter: " + this.lastCollisionTag);

        if(collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Cell")
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        this.lastCollisionTag = "";
    }

    void Rotate()
    {
        // side update for next direction
        {
            this.side += 1;

            if (this.side == Side.Max)
            {
                this.side = Side.North;
            }
        }
    }

    void MoveLeft()
    {
        if(this.lastCollisionTag != "Left Wall")
        {
            this.MoveSide(false);
        }
    }

    void MoveRight()
    {
        if(this.lastCollisionTag != "Right Wall")
        {
            this.MoveSide(true);
        }
    }

    void MoveDown()
    {
        if(!this.GetComponent<Rigidbody>().isKinematic)
        {
            var offset = this.cubeSize.y;

            for (var i = 0; i < this.transform.childCount; ++i)
            {
                var transform = this.transform.GetChild(i);
                transform.position += new Vector3(0, -offset);
            }
        }
    }

    void MoveSide(bool isRight)
    {
        var direction = (isRight ? 1 : -1);
        var offset = this.cubeSize.x * direction;

        for (var i = 0; i < this.transform.childCount; ++i)
        {
            var transform = this.transform.GetChild(i);
            transform.position += new Vector3(offset, 0);
        }
    }
}
