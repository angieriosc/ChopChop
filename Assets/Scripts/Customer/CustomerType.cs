using UnityEngine;

public enum CustomerType
{
    Type1,
    Type2,
    Type3
}

[System.Serializable]
public class CustomerPrefab
{
    public CustomerType type;
    public GameObject prefab;
}