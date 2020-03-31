using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBlock : MonoBehaviour
{
    public int freezeSeconds = 2;

    public Vector3 CubeSize { set { this.cubeSize = value; } }

    private string lastCollisionTag;
    private Vector3 cubeSize;
    
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
        var angles = this.transform.eulerAngles;
        this.transform.eulerAngles = new Vector3(angles.x, angles.y, angles.z + 90);
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
            var position = this.transform.position;
            this.transform.position = new Vector3(position.x, position.y - offset, position.z);
        }
    }

    void MoveSide(bool isRight)
    {
        var direction = (isRight ? 1 : -1);
        var offset = this.cubeSize.x * direction;
        var position = this.transform.position;
        this.transform.position = new Vector3(position.x + offset, position.y, position.z);
    }
}
