using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdatePosToNet
{
    public TransformNetMsg TransformNetMsg { get; set; }
    
    public void UpdatePos();
}
