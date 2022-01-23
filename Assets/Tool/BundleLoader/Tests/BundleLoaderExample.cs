using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BundleLoaderExample : MonoBehaviour
{
    [SerializeField] private AssetReference assetReference;

    private void Start()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        BundleLoader.Release(assetReference);
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleLoadAsync<BoxCollider2D>().Subscribe(x => { Instantiate(x); },Debug.LogError);
    }
}