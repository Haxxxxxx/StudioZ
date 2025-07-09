using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG6_Bundle : MonoBehaviour
{
    [SerializeField] private bool viable = false;

    public bool IsViable()
    {
        return viable;
    }
}
