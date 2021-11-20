using UnityEngine;
using System.Collections;

public interface ObjectPoolInterface{
    void Start();
    GameObject GetPooledObject();
}
