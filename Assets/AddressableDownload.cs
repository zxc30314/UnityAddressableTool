using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddressableDownload : MonoBehaviour
{
    private string key = "preload";
    CancellationTokenSource cts = new();
    [SerializeField] private TMP_Text process;
    [SerializeField] private TMP_Text info;
    [SerializeField] private Button download;
    [SerializeField] private Button stop;
    [SerializeField] private Button nextScene;
    [SerializeField] private AssetReference scene;

    private void Awake()
    {
        download.onClick.AddListener(() => DownloadResource(key, cts.Token));
        stop.onClick.AddListener(cts.Cancel);
        nextScene.onClick.AddListener(() => LoadScene(scene));
    }

    async UniTask LoadScene(AssetReference sceneName)
    {
        var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        await handle;
    }

    private async UniTask Start()
    {
        await Addressables.InitializeAsync();
        var checkUpdateCatalog = await CheckUpdateCatalog();
        var calculateKeyDownloadSizeSync = await CalculateKeyDownloadSizeSync(key);
        if (checkUpdateCatalog || calculateKeyDownloadSizeSync != 0)
        {
            Debug.Log($"has update");
            info.text = $"Download size :{calculateKeyDownloadSizeSync}";
            Debug.Log($"Download size :{calculateKeyDownloadSizeSync}");
        }
    }

    private async UniTask<bool> CheckUpdateCatalog()
    {
        var catalogsToUpdate = await Addressables.CheckForCatalogUpdates().ToUniTask();
        if (catalogsToUpdate.Count > 0)
        {
            var asyncOperationHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            await asyncOperationHandle;
            Addressables.Release(asyncOperationHandle);
        }

        return catalogsToUpdate.Count > 0;
    }

    private async UniTask DownloadResource(string key, CancellationToken cancellationToken = default)
    {
        Debug.Log($"[Patcher] Download key [{key}]");
        try
        {
            var handle = Addressables.DownloadDependenciesAsync(key, false);
            await handle.ToUniTask(Progress.Create<float>(UpdateProcess), cancellationToken: cancellationToken);
            Addressables.Release(handle);
            Debug.Log($"Download key [{key}] done");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Patcher] Download key [{key}] exception: {ex.Message}");
        }
    }

    private void UpdateProcess(float value)
    {
        process.text = $"{value * 100}%";
        Debug.Log($"process{value}");
    }

    private async Task<float> CalculateKeyDownloadSizeSync(object asset)
    {
        var asyncOperationHandle = Addressables.GetDownloadSizeAsync(asset);
        var getDownloadSize = await asyncOperationHandle;
        var downloadSize = getDownloadSize / (1024f * 1024f);
        Addressables.Release(asyncOperationHandle);
        return downloadSize;
    }

    private async UniTask ClearAsset(string label)
    {
        var resourceLocationsAsync = Addressables.LoadResourceLocationsAsync(label);
        var loadResourceLocationsAsync = await resourceLocationsAsync;
        loadResourceLocationsAsync.ToUniTaskAsyncEnumerable().ForEachAsync(x => { Addressables.ClearDependencyCacheAsync(x.PrimaryKey); });
        Addressables.Release(resourceLocationsAsync);
    }

    private async UniTask ClearAllAsset()
    {
        Addressables.ResourceLocators.ToUniTaskAsyncEnumerable().ForEachAsync(x =>
        {
            var async = Addressables.ClearDependencyCacheAsync(x.Keys, false);
            Addressables.Release(async);
        });
        Caching.ClearCache();
    }
}