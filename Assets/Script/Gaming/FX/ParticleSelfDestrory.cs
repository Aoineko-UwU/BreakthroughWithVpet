using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSelfDestrory : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 3f);
    }
}
