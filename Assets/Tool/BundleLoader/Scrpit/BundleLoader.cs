using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public static class BundleLoaderExpansion
{
    public static IObservable<T> BundleLoadAsync<T>(this AssetReference reference) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            return Observable.Throw<T>(new ArgumentNullException());
        }
        
        return BundleLoader.BundleLoadAsync<T>(reference);
    }

    public static IObservable<T> BundleInstantiateAsync<T>(this AssetReference reference, Vector3 position, Quaternion rotation, Transform parent) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            return Observable.Throw<T>(new ArgumentNullException());
        }
        
        return InstantiateAsyncTask<T>(reference, position, rotation, parent).ToObservable();
    }

    public static IObservable<T> BundleInstantiateAsync<T>(this AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            return Observable.Throw<T>(new ArgumentNullException());
        }
        return InstantiateAsyncTask<T>(reference, parent, instantiateInWorldSpace).ToObservable();
    }

    private static async UniTask<T> InstantiateAsyncTask<T>(this AssetReference reference, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            throw new ArgumentNullException();
        }

        var asyncOperationHandle = reference.InstantiateAsync(position, rotation, parent);
        return await BundleLoader.InstantiateAsyncTask<T>(asyncOperationHandle);
    }

    private static async UniTask<T> InstantiateAsyncTask<T>(this AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            throw new ArgumentNullException();
        }

        var asyncOperationHandle = reference.InstantiateAsync(parent, instantiateInWorldSpace);
        return await BundleLoader.InstantiateAsyncTask<T>(asyncOperationHandle);
    }

    public static void Release(this AssetReference reference)
    {
        BundleLoader.Release(reference);
    }
}

public static class BundleLoader
{
    private static readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> Container = new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();
    private static readonly Dictionary<AssetReference, Subject<AsyncOperationHandle<GameObject>>> OnLoadingContainer = new Dictionary<AssetReference, Subject<AsyncOperationHandle<GameObject>>>();

    public static IObservable<T> BundleLoadAsync<T>(AssetReference reference) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            return Observable.Throw<T>(new ArgumentNullException());
        }

        return BundleLoadAsyncAndAddToContainerTask(reference).ToObservable().Select(GetComponent<T>);
    }

    public static async UniTask<T> InstantiateAsyncTask<T>(AsyncOperationHandle<GameObject> handle) where T : Component
    {
        await handle.Task;
        if (!handle.Result.TryGetComponent<T>(out var result))
        {
            Object.Destroy(handle.Result);
            Addressables.ReleaseInstance(handle);
            throw new MissingComponentException();
        }

        Disposable.Create(() => { Addressables.ReleaseInstance(handle); }).AddTo(result);
        return result;
    }

    public static UniTask<AsyncOperationHandle<GameObject>> BundleLoadAsyncTask<T>(AssetReference reference) where T : Component
    {
        if (string.IsNullOrEmpty(reference?.AssetGUID))
        {
            throw new ArgumentNullException();
        }

        return BundleLoadAsyncAndAddToContainerTask(reference);
    }

    private static T GetComponent<T>(AsyncOperationHandle<GameObject> reference) where T : Component
    {
        return reference.Result.TryGetComponent<T>(out var result) ? result : throw new MissingComponentException();
    }

    private static UniTask<AsyncOperationHandle<GameObject>> BundleLoadAsyncAndAddToContainerTask(AssetReference reference)
    {
        if (Container.ContainsKey(reference))
        {
            return UniTask.Run(() => Container[reference]);
        }

        if (OnLoadingContainer.ContainsKey(reference))
        {
            return OnLoadingContainer[reference].ToUniTask();
        }

        OnLoadingContainer.Add(reference, new Subject<AsyncOperationHandle<GameObject>>());
        var assetAsync = LoadAssetAsync(reference);
        var continueWith = assetAsync.ContinueWith(x =>
        {
            if (x.Status == AsyncOperationStatus.Succeeded)
            {
                Container.Add(reference, x);
                OnLoadingContainer[reference].OnNext(x);
                OnLoadingContainer[reference].OnCompleted();
            }
            else
            {
                OnLoadingContainer[reference].OnError(x.OperationException);
                throw x.OperationException;
            }

            OnLoadingContainer.Remove(reference);
            return x;
        });
        return continueWith;
    }

    private static async UniTask<AsyncOperationHandle<GameObject>> LoadAssetAsync(AssetReference reference)
    {
        var asyncOperationHandle = reference.LoadAssetAsync<GameObject>();

        await asyncOperationHandle.Task;
        if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(asyncOperationHandle);
        }

        return asyncOperationHandle;
    }

    public static void Release(AssetReference reference)
    {
        if (Container.ContainsKey(reference))
        {
            Addressables.Release(Container[reference]);
            Container.Remove(reference);
            OnLoadingContainer.Remove(reference);
        }
    }
}