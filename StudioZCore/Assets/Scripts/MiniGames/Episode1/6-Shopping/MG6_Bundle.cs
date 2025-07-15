using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG6_Bundle : MonoBehaviour
{
    [SerializeField] private bool viable = false;
    [SerializeField] private int price = 6;

    
    public bool IsViable()
    {
        return viable;
    }

    public int GetPrice()
    {
        return price;
    }
}
