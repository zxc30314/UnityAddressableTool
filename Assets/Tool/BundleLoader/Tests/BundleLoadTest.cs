using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;

public class BundleLoadTest
{
    [UnityTest]
    public IEnumerator BundleInstantiateAsync_HasComponent_Success()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        yield return assetReference.BundleInstantiateAsync<Cube>().ToUniTask().ToCoroutine();
    }

    [UnityTest]
    public IEnumerator BundleLoadAsync_HasComponent_Success()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        yield return assetReference.BundleLoadAsync<Cube>().ToUniTask().ToCoroutine();
    }

    [UnityTest]
    public IEnumerator BundleLoadAsync_NotHasComponent_Fail()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleLoadAsync<BoxCollider2D>().Subscribe(x => { isDone = true; }, e => isError = e);
        while (!isDone && isError == default)
        {
            yield return null;
        }

        Assert.AreEqual(isError != default, true);
    }
    [UnityTest]
    public IEnumerator BundleInstantiateAsync_NotHasComponent_Fail()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleInstantiateAsync<BoxCollider2D>().Subscribe(x => { isDone = true; }, e => isError = e);
        while (!isDone && isError == default)
        {
            yield return null;
        }

        Assert.AreEqual(isError != default, true);
    }
    [UnityTest]
    [Timeout(1000)]
    public IEnumerator BundleLoadAsync_Null_Fail()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        assetReference = default;
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleLoadAsync<Cube>().Subscribe(x => { isDone = true; }, e => isError = e);
        while (!isDone && isError == default)
        {
            yield return null;
        }

        Assert.AreEqual(isError != default, true);
    }

    [UnityTest]
    [Timeout(1000)]
    public IEnumerator BundleInstantiateAsync_Null_Fail()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        assetReference = default;
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleInstantiateAsync<Cube>().Subscribe(x => { isDone = true; }, e => isError = e);
        while (!isDone && isError == default)
        {
            yield return null;
        }

        Assert.AreEqual(isError != default, true);
    }
}