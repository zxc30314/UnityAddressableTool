using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

public class TestMemoryProfiler : MonoBehaviour
{
    [SerializeField] private AssetReference assetReference;

    async Task Start()
    {
        await InstantiateAsync();
        await Task.Delay(1000);
        await BundleLoadAsync();
    }

    async Task InstantiateAsync()
    {
        try
        {
            var instantiateAsyncTask = await assetReference.BundleInstantiateAsync<Transform>();
            await Task.Delay(1000);
            Destroy(instantiateAsyncTask.gameObject);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }
    
    async Task BundleLoadAsync()
    {
        var instantiateAsyncTask = await assetReference.BundleLoadAsync<Transform>();
        var instantiate = Instantiate(instantiateAsyncTask);
        await Task.Delay(1000);
        Destroy(instantiate.gameObject);
        assetReference.Release();
    }
  
}