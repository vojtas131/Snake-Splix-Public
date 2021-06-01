using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{
    public int x;
    public int y;
    public Vector3 worldPosition;
    public int land;
    /* 
        -1 - border
        0 - null
        1 - P1 concured
        2 - P1 concured + border
        3 - P2 concured
        4 - P2 concured + border
    */
    public int tail;
    /* 
        0 - null
        1 - P1
        2 - P2 
    */
}