using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SpineController : MonoBehaviour
{

    public Dictionary<PlayerMovementState, Vector3> Offset;
    
    void UpdateOffset()
    {
        var movement = GetComponent<PlayerMovement>();
    }
}
