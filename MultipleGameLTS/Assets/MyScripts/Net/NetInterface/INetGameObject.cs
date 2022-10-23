using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetGameObject 
{
    public int NetID { get; set; }
    
    public SwitchNetMsg SwitchNetMsg { get; set; }
}
