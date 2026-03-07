#nullable enable

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ShaderOp.Shaders
{
    /// <summary>
    /// ゲームピースのシェーダーアニメーション制御
    /// SG_GamePiece_2D.shadergraph と連携
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class GamePieceShaderAnimator : MonoBehaviour
    {
        private Material? _pieceMaterial;
        private Renderer? _renderer;

        [Header("アニメーション設定")]
        [SerializeField, Range(0.1f, 2f)] private float _fadeInDuration = 0.5f;
        [SerializeField, Range(0.1f, 1f)] private float _blinkInterval = 0.3f;

        void Awake()
        {
            _renderer = GetComponent<Renderer>();

            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                _pieceMaterial = _renderer.material;
            }
            else
            {
                Debug.LogError($"[GamePieceShaderAnimator] Renderer または Material が見つかりません: {gameObject.name}");
            }
        }

        /// <summary>
        /// プレイヤーカラーを設定
        /// </summary>
        /// <param name="color">プレイヤーの色</param>
        /// <param name="tintStrength">ティント強度（0〜1）</param>
        public void SetPlayerColor(Color color, float tintStrength = 0.5f)
        {
            if (_pieceMaterial == null) return;

            _pieceMaterial.SetColor("_PlayerColor", color);
            _pieceMaterial.SetFloat("_TintStrength", Mathf.Clamp01(tintStrength));
        }

        /// <summary>
        /// 選択ハイライトを設定
        /// </summary>
        /// <param name="highlighted">ハイライトのON/OFF</param>
        /// <param name="intensity">ハイライト強度（0〜1）</param>
        public void SetHighlight(bool highlighted, float intensity = 1.0f)
        {
            if (_pieceMaterial == null) return;

            _pieceMaterial.SetFloat("_HighlightIntensity", highlighted ? Mathf.Clamp01(intensity) : 0f);
        }

        /// <summary>
        /// フェードインアニメーション（配置時）
        /// </summary>
        /// <param name="duration">アニメーション時間（秒）</param>
        public async UniTask FadeIn(float? duration = null)
        {
            if (_pieceMaterial == null)
            {
                Debug.LogWarning($"[GamePieceShaderAnimator] マテリアルが初期化されていません: {gameObject.name}");
                return;
            }

            float animDuration = duration ?? _fadeInDuration;
            float elapsed = 0f;

            // 初期状態: 完全透明
            _pieceMaterial.SetFloat("_Fade", 0f);

            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / animDuration);
                _pieceMaterial.SetFloat("_Fade", t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 最終状態: 完全不透明
            _pieceMaterial.SetFloat("_Fade", 1f);
        }

        /// <summary>
        /// フェードアウトアニメーション（削除時）
        /// </summary>
        /// <param name="duration">アニメーション時間（秒）</param>
        public async UniTask FadeOut(float? duration = null)
        {
            if (_pieceMaterial == null) return;

            float animDuration = duration ?? _fadeInDuration;
            float elapsed = 0f;

            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(1, 0, elapsed / animDuration);
                _pieceMaterial.SetFloat("_Fade", t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            _pieceMaterial.SetFloat("_Fade", 0f);
        }

        /// <summary>
        /// ハイライト点滅アニメーション
        /// </summary>
        /// <param name="blinkCount">点滅回数</param>
        /// <param name="interval">点滅間隔（秒）</param>
        public async UniTask BlinkHighlight(int blinkCount = 3, float? interval = null)
        {
            if (_pieceMaterial == null) return;

            float blinkInterval = interval ?? _blinkInterval;
            int halfInterval = (int)(blinkInterval * 500); // ミリ秒に変換

            for (int i = 0; i < blinkCount; i++)
            {
                // ON
                _pieceMaterial.SetFloat("_HighlightIntensity", 1f);
                await UniTask.Delay(halfInterval);

                // OFF
                _pieceMaterial.SetFloat("_HighlightIntensity", 0f);
                await UniTask.Delay(halfInterval);
            }
        }

        /// <summary>
        /// ドロップシャドウ設定
        /// </summary>
        /// <param name="offset">シャドウオフセット</param>
        /// <param name="opacity">シャドウ不透明度（0〜1）</param>
        public void SetShadow(Vector2 offset, float opacity = 0.3f)
        {
            if (_pieceMaterial == null) return;

            _pieceMaterial.SetVector("_ShadowOffset", offset);
            _pieceMaterial.SetFloat("_ShadowOpacity", Mathf.Clamp01(opacity));
        }

        void OnDestroy()
        {
            // マテリアルインスタンスを破棄
            if (_pieceMaterial != null)
            {
                Destroy(_pieceMaterial);
                _pieceMaterial = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上でのテスト用メソッド
        /// </summary>
        [ContextMenu("Test: Fade In")]
        private async void TestFadeIn()
        {
            await FadeIn();
        }

        [ContextMenu("Test: Fade Out")]
        private async void TestFadeOut()
        {
            await FadeOut();
        }

        [ContextMenu("Test: Blink Highlight")]
        private async void TestBlinkHighlight()
        {
            await BlinkHighlight(3);
        }

        [ContextMenu("Test: Set Player Color (Red)")]
        private void TestRedPlayer()
        {
            SetPlayerColor(Color.red, 0.6f);
        }

        [ContextMenu("Test: Set Player Color (Blue)")]
        private void TestBluePlayer()
        {
            SetPlayerColor(Color.blue, 0.6f);
        }

        [ContextMenu("Test: Toggle Highlight")]
        private void TestToggleHighlight()
        {
            bool isHighlighted = _pieceMaterial?.GetFloat("_HighlightIntensity") > 0.5f;
            SetHighlight(!isHighlighted);
        }
#endif
    }
}
