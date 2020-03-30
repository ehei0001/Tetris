using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleBlock : MonoBehaviour
{
    [SerializeField]
    private float removingSecond = 10;

    private void Start()
    {
        Destroy(this.gameObject, this.removingSecond);
    }
}
