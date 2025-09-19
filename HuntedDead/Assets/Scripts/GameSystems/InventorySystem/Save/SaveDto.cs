using System;
using UnityEngine;

[Serializable] public class StackDto 
{  
    public string itemGuid; 
    public int tier; 
    public int qty; 
    public bool rotated; 
    public int x, y, z; 
    public string cont; }
[Serializable] public class SaveDto { 
    public StackDto[] stacks; 
    public string playerState; }
