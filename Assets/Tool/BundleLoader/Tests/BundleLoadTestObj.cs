using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

public class BundleLoadTestObj : MonoBehaviour
{

    [SerializeField] private AssetReference _assetReference;

    public AssetReference GetAssetReference()
    {
        return _assetReference;
    }
}