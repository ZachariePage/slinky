using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    void ReturnToPoolEvent()
    {
        ObjectPool.ReturnToPool(transform.gameObject);
    }
}
