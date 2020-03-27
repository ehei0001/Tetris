using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private float removingSecond = 10;
    private float elapsedTime;

    // Update is called once per frame
    void Update()
    {
        if(this.elapsedTime > this.removingSecond)
        {
            Destroy(this.gameObject);
        }
        else
        {
            elapsedTime += Time.deltaTime;
        }
    }
}
