using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class ObjectPool
{
    [Header("Identificable")]
    [SerializeField] private string poolName;

    [Header("Pool Settings")]
    [SerializeField] private GameObject poolObject;
    [SerializeField] private int initialPoolSize;
    [SerializeField] private int maxPoolSize;
    [SerializeField] private bool canPoolGrow = false;
    [SerializeField] private bool canPoolGrowOverMaxSize = false;
    private List<GameObject> objectList;

    public ObjectPool()
    {
        poolObject = null;
        initialPoolSize = 0;
        maxPoolSize = 0;
        canPoolGrow = false;
        canPoolGrowOverMaxSize = false;
        objectList = new List<GameObject>();
    }

    public ObjectPool(string poolName, GameObject poolObject, int initialPoolSize, int maxPoolSize, bool canPoolGrow, bool canPoolGrowOverMaxSize)
    {
        this.poolName = poolName;
        this.poolObject = poolObject;
        this.initialPoolSize = initialPoolSize;
        this.maxPoolSize = maxPoolSize;
        this.canPoolGrow = canPoolGrow;
        this.canPoolGrowOverMaxSize = canPoolGrowOverMaxSize;
        objectList = new List<GameObject>();
    }
    protected abstract void HandleInitializedObjectOnPool(GameObject poolObject);

    public void AddObjectToPool(GameObject poolObject)
    {
        if(objectList.Count + 1 < initialPoolSize)
        {
            InitializeObjectOnPool(poolObject);
        } else if(canPoolGrow && objectList.Count + 1 < maxPoolSize)
        {
            InitializeObjectOnPool(poolObject);
        } else if(canPoolGrowOverMaxSize)
        {
            InitializeObjectOnPool(poolObject);
        }
    }
    public void InitializeObjectOnPool(GameObject poolObject)
    {
        objectList.Add(poolObject);
        HandleInitializedObjectOnPool(poolObject);
    }

   
    
}
