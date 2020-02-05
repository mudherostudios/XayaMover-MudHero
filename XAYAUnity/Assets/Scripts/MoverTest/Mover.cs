using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public string owner;
    public float offset = 0.5f;

    public void UpdatePosition(int x, int z)
    {
        transform.position = new Vector3(offset + x, offset, offset + z);
    }
}
