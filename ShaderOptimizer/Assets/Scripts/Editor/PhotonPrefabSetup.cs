#nullable enable

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using ShaderOp.Core.Services.Networking;

namespace ShaderOp.Editor
{
    /// <summary>
    /// Photon Service Prefab自動生成ツール
    /// Phase 5 Week 1 Day 3 - Prefab作成を自動化
    /// </summary>
    public static class PhotonPrefabSetup
    {
        private const string PREFAB_FOLDER = "Assets/Prefabs/Services";
        private const string NETWORK_SERVICE_PREFAB_PATH = PREFAB_FOLDER + "/NetworkService.prefab";
        private const string GAME_SYNC_SERVICE_PREFAB_PATH = PREFAB_FOLDER + "/GameSyncService.prefab";

        /// <summary>
        /// メニュー: Tools > ShaderOp > Create Photon Service Prefabs
        /// </summary>
        [MenuItem("Tools/ShaderOp/Phase 5/Create Photon Service Prefabs")]
        public static void CreatePhotonServicePrefabs()
        {
            // フォルダ作成
            EnsureFolderExists();

            // NetworkService.prefab作成
            bool networkSuccess = CreateNetworkServicePrefab();

            // GameSyncService.prefab作成
            bool gameSyncSuccess = CreateGameSyncServicePrefab();

            // 結果レポート
            if (networkSuccess && gameSyncSuccess)
            {
                Debug.Log("[PhotonPrefabSetup] ✅ 2つのPrefabを作成しました:");
                Debug.Log($"  - {NETWORK_SERVICE_PREFAB_PATH}");
                Debug.Log($"  - {GAME_SYNC_SERVICE_PREFAB_PATH}");
                EditorUtility.DisplayDialog(
                    "Prefab作成完了",
                    "NetworkService.prefab と GameSyncService.prefab を作成しました。\n\n" +
                    "次のステップ:\n" +
                    "1. Startup.unityシーンを開く\n" +
                    "2. GameBootstrapを選択\n" +
                    "3. InspectorでPrefabを設定",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[PhotonPrefabSetup] ❌ Prefab作成に失敗しました");
                EditorUtility.DisplayDialog(
                    "エラー",
                    "Prefab作成に失敗しました。Consoleを確認してください。",
                    "OK"
                );
            }
        }

        /// <summary>
        /// Prefabフォルダを作成
        /// </summary>
        private static void EnsureFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
                Debug.Log("[PhotonPrefabSetup] フォルダ作成: Assets/Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Services");
                Debug.Log($"[PhotonPrefabSetup] フォルダ作成: {PREFAB_FOLDER}");
            }
        }

        /// <summary>
        /// NetworkService.prefab作成
        /// </summary>
        private static bool CreateNetworkServicePrefab()
        {
            try
            {
                // 既存のPrefabを削除
                if (File.Exists(NETWORK_SERVICE_PREFAB_PATH))
                {
                    AssetDatabase.DeleteAsset(NETWORK_SERVICE_PREFAB_PATH);
                    Debug.Log($"[PhotonPrefabSetup] 既存のPrefabを削除: {NETWORK_SERVICE_PREFAB_PATH}");
                }

                // GameObject作成
                GameObject networkServiceObj = new GameObject("NetworkService");

                // PhotonNetworkServiceコンポーネント追加
                networkServiceObj.AddComponent<PhotonNetworkService>();

                // Prefab保存
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(networkServiceObj, NETWORK_SERVICE_PREFAB_PATH);

                // Hierarchyから削除
                Object.DestroyImmediate(networkServiceObj);

                if (prefab != null)
                {
                    Debug.Log($"[PhotonPrefabSetup] ✅ NetworkService.prefab作成成功: {NETWORK_SERVICE_PREFAB_PATH}");
                    return true;
                }
                else
                {
                    Debug.LogError("[PhotonPrefabSetup] ❌ NetworkService.prefab作成失敗");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhotonPrefabSetup] NetworkService.prefab作成中にエラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// GameSyncService.prefab作成
        /// </summary>
        private static bool CreateGameSyncServicePrefab()
        {
            try
            {
                // 既存のPrefabを削除
                if (File.Exists(GAME_SYNC_SERVICE_PREFAB_PATH))
                {
                    AssetDatabase.DeleteAsset(GAME_SYNC_SERVICE_PREFAB_PATH);
                    Debug.Log($"[PhotonPrefabSetup] 既存のPrefabを削除: {GAME_SYNC_SERVICE_PREFAB_PATH}");
                }

                // GameObject作成
                GameObject gameSyncServiceObj = new GameObject("GameSyncService");

                // PhotonGameSyncServiceコンポーネント追加
                gameSyncServiceObj.AddComponent<PhotonGameSyncService>();

                // ⚠️ PhotonViewコンポーネント追加（RPC用に必須）
                var photonView = gameSyncServiceObj.AddComponent<Photon.Pun.PhotonView>();

                // PhotonView設定
                photonView.OwnershipTransfer = Photon.Pun.OwnershipOption.Fixed;
                photonView.Synchronization = Photon.Pun.ViewSynchronization.Off;
                // ObservedComponentsは空のまま（RPCのみ使用）

                Debug.Log("[PhotonPrefabSetup] PhotonViewコンポーネントを追加（View ID: 自動割り当て）");

                // Prefab保存
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameSyncServiceObj, GAME_SYNC_SERVICE_PREFAB_PATH);

                // Hierarchyから削除
                Object.DestroyImmediate(gameSyncServiceObj);

                if (prefab != null)
                {
                    Debug.Log($"[PhotonPrefabSetup] ✅ GameSyncService.prefab作成成功: {GAME_SYNC_SERVICE_PREFAB_PATH}");
                    Debug.Log("  - PhotonGameSyncService コンポーネントあり");
                    Debug.Log("  - PhotonView コンポーネントあり（RPC用）");
                    return true;
                }
                else
                {
                    Debug.LogError("[PhotonPrefabSetup] ❌ GameSyncService.prefab作成失敗");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhotonPrefabSetup] GameSyncService.prefab作成中にエラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// メニュー検証: Photon PUNがインポートされているかチェック
        /// </summary>
        [MenuItem("Tools/ShaderOp/Phase 5/Create Photon Service Prefabs", true)]
        public static bool ValidateCreatePhotonServicePrefabs()
        {
            // Photon.Pun名前空間が存在するかチェック
            var photonPunAssembly = System.Reflection.Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .FirstOrDefault(a => a.Name.Contains("PhotonUnityNetworking"));

            if (photonPunAssembly == null)
            {
                // 警告は出さない（メニューを無効化するのみ）
                return false;
            }

            return true;
        }
    }
}
