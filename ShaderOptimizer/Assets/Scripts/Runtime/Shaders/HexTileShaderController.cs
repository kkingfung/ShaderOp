#nullable enable

using UnityEngine;

namespace ShaderOp.Shaders
{
    /// <summary>
    /// ヘックスタイルのシェーダー制御クラス
    /// SG_HexTile_Interactive.shadergraph と連携
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class HexTileShaderController : MonoBehaviour
    {
        private Material? _tileMaterial;
        private Renderer? _renderer;

        /// <summary>
        /// タイル状態の列挙型
        /// </summary>
        public enum TileState
        {
            /// <summary>通常状態</summary>
            Normal = 0,
            /// <summary>ホバー状態</summary>
            Hover = 1,
            /// <summary>選択状態</summary>
            Selected = 2,
            /// <summary>無効状態</summary>
            Disabled = 3
        }

        void Awake()
        {
            _renderer = GetComponent<Renderer>();

            // マテリアルをインスタンス化（各タイルが独立したマテリアルを持つ）
            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                _tileMaterial = _renderer.material;
            }
            else
            {
                Debug.LogError($"[HexTileShaderController] Renderer または Material が見つかりません: {gameObject.name}");
            }
        }

        /// <summary>
        /// タイル状態を設定
        /// </summary>
        /// <param name="state">設定する状態</param>
        public void SetState(TileState state)
        {
            if (_tileMaterial == null)
            {
                Debug.LogWarning($"[HexTileShaderController] マテリアルが初期化されていません: {gameObject.name}");
                return;
            }

            _tileMaterial.SetFloat("_State", (float)state);
        }

        /// <summary>
        /// チームカラーを設定
        /// </summary>
        /// <param name="color">チームの色</param>
        /// <param name="strength">ティント強度（0〜1）</param>
        public void SetTeamColor(Color color, float strength = 0.5f)
        {
            if (_tileMaterial == null) return;

            _tileMaterial.SetColor("_TeamColor", color);
            _tileMaterial.SetFloat("_TeamTintStrength", Mathf.Clamp01(strength));
        }

        /// <summary>
        /// 有効手グロー表示
        /// </summary>
        /// <param name="show">グロー表示のON/OFF</param>
        /// <param name="intensity">グロー強度（0〜2）</param>
        public void ShowValidMoveGlow(bool show, float intensity = 1.0f)
        {
            if (_tileMaterial == null) return;

            _tileMaterial.SetFloat("_GlowIntensity", show ? Mathf.Clamp(intensity, 0f, 2f) : 0f);
        }

        /// <summary>
        /// グローの点滅速度を設定
        /// </summary>
        /// <param name="speed">点滅速度（0〜5）</param>
        public void SetGlowSpeed(float speed)
        {
            if (_tileMaterial == null) return;

            _tileMaterial.SetFloat("_GlowSpeed", Mathf.Clamp(speed, 0f, 5f));
        }

        /// <summary>
        /// ホバー時の明るさを設定
        /// </summary>
        /// <param name="brightness">明るさ（1〜2）</param>
        public void SetHoverBrightness(float brightness)
        {
            if (_tileMaterial == null) return;

            _tileMaterial.SetFloat("_HoverBrightness", Mathf.Clamp(brightness, 1f, 2f));
        }

        void OnDestroy()
        {
            // マテリアルインスタンスを破棄（メモリリーク防止）
            if (_tileMaterial != null)
            {
                Destroy(_tileMaterial);
                _tileMaterial = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でのテスト用メソッド
        /// </summary>
        [ContextMenu("Test: Set Hover State")]
        private void TestHoverState()
        {
            SetState(TileState.Hover);
        }

        [ContextMenu("Test: Set Selected State")]
        private void TestSelectedState()
        {
            SetState(TileState.Selected);
        }

        [ContextMenu("Test: Show Glow")]
        private void TestShowGlow()
        {
            ShowValidMoveGlow(true, 1.5f);
        }

        [ContextMenu("Test: Set Red Team")]
        private void TestRedTeam()
        {
            SetTeamColor(Color.red, 0.6f);
        }

        [ContextMenu("Test: Set Blue Team")]
        private void TestBlueTeam()
        {
            SetTeamColor(Color.blue, 0.6f);
        }
#endif
    }
}
