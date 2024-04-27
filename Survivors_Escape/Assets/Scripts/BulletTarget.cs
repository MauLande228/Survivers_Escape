using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTarget : MonoBehaviour
{
    private int count = 10;
    
    public void LoseCount()
    {
        count -= 1;
        if (count < 1)
        {
            Destroy(gameObject);
        }
    }
}
