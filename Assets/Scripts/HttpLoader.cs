using System.Collections;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;

public class HttpLoader : MonoBehaviour
{
    public string glbUrl = "https://storage.googleapis.com/furniture-models/armchair/poltrona-biza_a3bc4a40-53e9-4be0-89f1-a7573d600971.glb";
    public string glbUrl2 = "https://storage.googleapis.com/furniture-models/dining-chair/victoria-ghost_8d4ea5cc-78a8-4edc-961d-17a3f6e83d4f.glb";

    void Start()
    {
        StartCoroutine(DownloadAndLoadGLB(glbUrl, "armchair", new Vector3(1.7f, 0.4f, -1.8f)));
        StartCoroutine(DownloadAndLoadGLB(glbUrl2, "dining-chair", new Vector3(1f, 0.4f, .75f)));
    }

    private IEnumerator DownloadAndLoadGLB(string url, string itemName, Vector3 position)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download GLB: " + request.error);
            yield break;
        }

        LoadModel(request.downloadHandler.data, itemName, position);
    }

    private async void LoadModel(byte[] glbData, string itemName, Vector3 position)
    {
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(glbData);

        if (!success)
        {
            Debug.LogError("Failed to load GLB model.");
            return;
        }

        var sceneInstance = new GameObject(itemName)
        {
            transform =
            {
                position = position
            }
        };
        success = await gltf.InstantiateMainSceneAsync(sceneInstance.transform);
        if (!success)
        {
            Debug.LogError("Failed to instantiate GLB model.");
        }
    }
}
