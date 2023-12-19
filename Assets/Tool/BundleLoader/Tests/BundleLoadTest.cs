using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public class BundleLoadTest
{
    [UnityTest]
    public IEnumerator BundleLoadAsync_NotHasComponent_ShouldBe_Throw_MissingComponentException()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleLoadAsync<BoxCollider2D>().Subscribe(x => { isDone = true; }, e => isError = e);
        while (!isDone && isError == default)
        {
            yield return null;
        }

        Assert.Throws<MissingComponentException>(() => throw isError);
    }

    [UnityTest]
    public IEnumerator BundleInstantiateAsync_NotHasComponent_ShouldBe_Throw_MissingComponentException()
    {
        var assetReference = Resources.Load<BundleLoadTestObj>("BundleLoadTestObj").GetAssetReference();
        bool isDone = false;
        Exception isError = default;
        assetReference.BundleInstantiateAsync<BoxCollider2D>().Subscribe(x => { isDone = true; }, e => isError = e);
        while (!isDone && isError == default)
        {
            yield return null;
        }

        Assert.Throws<MissingComponentException>(() => throw isError);
    }

    [UnityTest]
    [Timeout(1000)]
    public IEnumerator BundleLoadAsync_Null_ShouldBe_Throws_NullReferenceException()
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

        Assert.Throws<ArgumentNullException>(() => throw isError);
    }

    [UnityTest]
    [Timeout(1000)]
    public IEnumerator BundleInstantiateAsync_ShouldBe_Throws_NullReferenceException()
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

        Assert.Throws<ArgumentNullException>(() => throw isError);
    }
}