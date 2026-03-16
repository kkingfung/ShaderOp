#nullable enable

using UnityEngine;
using UnityEditor;
using System.IO;

namespace ShaderOp.Editor
{
    /// <summary>
    /// Photon Prefab作成を実行するEditorScript
    /// Unity Editor起動時に自動実行
    /// </summary>
    [InitializeOnLoad]
    public static class PhotonPrefabCreator
    {
        private const string PREFAB_FOLDER = "Assets/Prefabs/Services";
        private const string NETWORK_SERVICE_PATH = PREFAB_FOLDER + "/NetworkService.prefab";
        private const string GAME_SYNC_SERVICE_PATH = PREFAB_FOLDER + "/GameSyncService.prefab";
        private const string PREFS_KEY = "PhotonPrefabsCreated";

        /// <summary>
        /// エディタ起動時に実行
        /// </summary>
        static PhotonPrefabCreator()
        {
            // エディタがプレイモード中は実行しない
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // 既に作成済みかチェック
            if (EditorPrefs.GetBool(PREFS_KEY, false))
            {
                return;
            }

            // Prefabが既に存在するかチェック
            if (File.Exists(NETWORK_SERVICE_PATH) && File.Exists(GAME_SYNC_SERVICE_PATH))
            {
                Debug.Log("[PhotonPrefabCreator] Prefabは既に存在します。スキップします。");
                EditorPrefs.SetBool(PREFS_KEY, true);
                return;
            }

            // 次回のエディタ起動時に自動作成を試みる
            EditorApplication.delayCall += CreatePrefabsDelayed;
        }

        /// <summary>
        /// 遅延実行でPrefab作成
        /// </summary>
        private static void CreatePrefabsDelayed()
        {
            try
            {
                Debug.Log("[PhotonPrefabCreator] Photon Service Prefab自動作成を開始します...");

                // フォルダ作成
                EnsureFolderExists();

                // Prefab作成
                bool success = CreateBothPrefabs();

                if (success)
                {
                    EditorPrefs.SetBool(PREFS_KEY, true);
                    Debug.Log("[PhotonPrefabCreator] ✅ Prefab作成完了");
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhotonPrefabCreator] Prefab作成失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// フォルダ作成
        /// </summary>
        private static void EnsureFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Services");
            }
        }

        /// <summary>
        /// 両方のPrefabを作成
        /// </summary>
        private static bool CreateBothPrefabs()
        {
            bool networkSuccess = CreateNetworkServicePrefab();
            bool gameSyncSuccess = CreateGameSyncServicePrefab();

            return networkSuccess && gameSyncSuccess;
        }

        /// <summary>
        /// NetworkService.prefab作成
        /// </summary>
        private static bool CreateNetworkServicePrefab()
        {
            if (File.Exists(NETWORK_SERVICE_PATH))
            {
                Debug.Log($"[PhotonPrefabCreator] NetworkService.prefabは既に存在します");
                return true;
            }

            try
            {
                // GameObject作成
                GameObject obj = new GameObject("NetworkService");

                // コンポーネント追加
                var component = obj.AddComponent(System.Type.GetType("ShaderOp.Core.Services.Networking.PhotonNetworkService, ShaderOp.Runtime"));

                if (component == null)
                {
                    Debug.LogError("[PhotonPrefabCreator] PhotonNetworkServiceコンポーネントが見つかりません");
                    Object.DestroyImmediate(obj);
                    return false;
                }

                // Prefab保存
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, NETWORK_SERVICE_PATH);
                Object.DestroyImmediate(obj);

                if (prefab != null)
                {
                    Debug.Log($"[PhotonPrefabCreator] ✅ NetworkService.prefab作成成功: {NETWORK_SERVICE_PATH}");
                    return true;
                }

                Debug.LogError("[PhotonPrefabCreator] NetworkService.prefab作成失敗");
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhotonPrefabCreator] NetworkService作成エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// GameSyncService.prefab作成
        /// </summary>
        private static bool CreateGameSyncServicePrefab()
        {
            if (File.Exists(GAME_SYNC_SERVICE_PATH))
            {
                Debug.Log($"[PhotonPrefabCreator] GameSyncService.prefabは既に存在します");
                return true;
            }

            try
            {
                // GameObject作成
                GameObject obj = new GameObject("GameSyncService");

                // PhotonGameSyncService追加
                var gameSyncComponent = obj.AddComponent(System.Type.GetType("ShaderOp.Core.Services.Networking.PhotonGameSyncService, ShaderOp.Runtime"));

                if (gameSyncComponent == null)
                {
                    Debug.LogError("[PhotonPrefabCreator] PhotonGameSyncServiceコンポーネントが見つかりません");
                    Object.DestroyImmediate(obj);
                    return false;
                }

                // PhotonView追加（RPC用に必須）
                var photonViewType = System.Type.GetType("Photon.Pun.PhotonView, PhotonUnityNetworking");
                if (photonViewType != null)
                {
                    var photonView = obj.AddComponent(photonViewType);

                    // PhotonView設定
                    var ownershipProperty = photonViewType.GetProperty("OwnershipTransfer");
                    var synchronizationProperty = photonViewType.GetProperty("Synchronization");

                    if (ownershipProperty != null && synchronizationProperty != null)
                    {
                        // OwnershipTransfer = Fixed (0)
                        ownershipProperty.SetValue(photonView, 0);
                        // Synchronization = Off (0)
                        synchronizationProperty.SetValue(photonView, 0);

                        Debug.Log("[PhotonPrefabCreator] PhotonViewコンポーネント追加 (RPC用)");
                    }
                }
                else
                {
                    Debug.LogWarning("[PhotonPrefabCreator] PhotonView型が見つかりません（Photon PUN未インポート？）");
                }

                // Prefab保存
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, GAME_SYNC_SERVICE_PATH);
                Object.DestroyImmediate(obj);

                if (prefab != null)
                {
                    Debug.Log($"[PhotonPrefabCreator] ✅ GameSyncService.prefab作成成功: {GAME_SYNC_SERVICE_PATH}");
                    return true;
                }

                Debug.LogError("[PhotonPrefabCreator] GameSyncService.prefab作成失敗");
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhotonPrefabCreator] GameSyncService作成エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Prefab作成をリセット（再実行用）
        /// </summary>
        [MenuItem("Tools/ShaderOp/Phase 5/Reset Prefab Creation Flag")]
        public static void ResetPrefabCreationFlag()
        {
            EditorPrefs.DeleteKey(PREFS_KEY);
            Debug.Log("[PhotonPrefabCreator] Prefab作成フラグをリセットしました。エディタを再起動してください。");
        }
    }
}
