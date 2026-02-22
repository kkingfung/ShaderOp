#nullable enable

using UnityEngine;

namespace ShaderOp.Core
{
    /// <summary>
    /// オービットカメラコントローラー
    /// </summary>
    /// <remarks>
    /// キャラクターカスタマイズ用の軌道カメラ
    /// マウスドラッグで回転、スクロールでズーム
    /// </remarks>
    public class OrbitCameraController : MonoBehaviour
    {
        /// <summary>注視対象</summary>
        [Header("Target")]
        [SerializeField] private Transform? _target;

        /// <summary>自動回転有効</summary>
        [Header("Rotation")]
        [SerializeField] private bool _autoRotate = false;

        /// <summary>自動回転速度</summary>
        [SerializeField] private float _autoRotateSpeed = 10f;

        /// <summary>回転速度</summary>
        [SerializeField] private float _rotationSpeed = 5f;

        /// <summary>回転スムージング</summary>
        [SerializeField] private float _rotationSmoothing = 0.1f;

        /// <summary>垂直角度制限（最小）</summary>
        [SerializeField] private float _minVerticalAngle = -30f;

        /// <summary>垂直角度制限（最大）</summary>
        [SerializeField] private float _maxVerticalAngle = 80f;

        /// <summary>ズーム設定</summary>
        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 2f;

        /// <summary>最小距離</summary>
        [SerializeField] private float _minDistance = 1f;

        /// <summary>最大距離</summary>
        [SerializeField] private float _maxDistance = 10f;

        /// <summary>初期距離</summary>
        [SerializeField] private float _initialDistance = 3f;

        /// <summary>ズームスムージング</summary>
        [SerializeField] private float _zoomSmoothing = 0.1f;

        /// <summary>ターゲットオフセット</summary>
        [Header("Offset")]
        [SerializeField] private Vector3 _targetOffset = new Vector3(0, 1, 0);

        /// <summary>現在の水平角度</summary>
        private float _currentHorizontalAngle = 0f;

        /// <summary>現在の垂直角度</summary>
        private float _currentVerticalAngle = 20f;

        /// <summary>現在の距離</summary>
        private float _currentDistance = 3f;

        /// <summary>目標水平角度</summary>
        private float _targetHorizontalAngle = 0f;

        /// <summary>目標垂直角度</summary>
        private float _targetVerticalAngle = 20f;

        /// <summary>目標距離</summary>
        private float _targetDistance = 3f;

        /// <summary>ドラッグ中フラグ</summary>
        private bool _isDragging = false;

        /// <summary>前フレームのマウス位置</summary>
        private Vector2 _lastMousePosition;

        private void Start()
        {
            _currentDistance = _initialDistance;
            _targetDistance = _initialDistance;

            // InputManagerのイベントを購読
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMouseDrag += OnMouseDrag;
                InputManager.Instance.OnMouseScroll += OnMouseScroll;
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMouseDrag -= OnMouseDrag;
                InputManager.Instance.OnMouseScroll -= OnMouseScroll;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            UpdateRotation();
            UpdatePosition();
        }

        /// <summary>
        /// 回転を更新
        /// </summary>
        private void UpdateRotation()
        {
            // 自動回転
            if (_autoRotate && !_isDragging)
            {
                _targetHorizontalAngle += _autoRotateSpeed * Time.deltaTime;
            }

            // スムージング
            _currentHorizontalAngle = Mathf.Lerp(_currentHorizontalAngle, _targetHorizontalAngle, _rotationSmoothing);
            _currentVerticalAngle = Mathf.Lerp(_currentVerticalAngle, _targetVerticalAngle, _rotationSmoothing);
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, _zoomSmoothing);
        }

        /// <summary>
        /// 位置を更新
        /// </summary>
        private void UpdatePosition()
        {
            if (_target == null)
                return;

            // 回転を計算
            Quaternion rotation = Quaternion.Euler(_currentVerticalAngle, _currentHorizontalAngle, 0);

            // カメラ位置を計算
            Vector3 targetPosition = _target.position + _targetOffset;
            Vector3 offset = rotation * (Vector3.back * _currentDistance);
            transform.position = targetPosition + offset;

            // ターゲットを注視
            transform.LookAt(targetPosition);
        }

        /// <summary>
        /// マウスドラッグ時の処理
        /// </summary>
        private void OnMouseDrag(Vector2 delta)
        {
            if (Input.GetMouseButton(1)) // 右クリックドラッグ
            {
                _isDragging = true;

                // 水平回転
                _targetHorizontalAngle += delta.x * _rotationSpeed * 0.1f;

                // 垂直回転（制限付き）
                _targetVerticalAngle -= delta.y * _rotationSpeed * 0.1f;
                _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
            }
            else
            {
                _isDragging = false;
            }
        }

        /// <summary>
        /// マウススクロール時の処理
        /// </summary>
        private void OnMouseScroll(float delta)
        {
            // ズームイン/アウト
            _targetDistance -= delta * _zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        }

        /// <summary>
        /// ターゲットを設定
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// カメラをリセット
        /// </summary>
        public void ResetCamera()
        {
            _targetHorizontalAngle = 0f;
            _targetVerticalAngle = 20f;
            _targetDistance = _initialDistance;

            _currentHorizontalAngle = _targetHorizontalAngle;
            _currentVerticalAngle = _targetVerticalAngle;
            _currentDistance = _targetDistance;
        }

        /// <summary>
        /// 自動回転を切り替え
        /// </summary>
        public void ToggleAutoRotate()
        {
            _autoRotate = !_autoRotate;
        }

        /// <summary>
        /// 自動回転を設定
        /// </summary>
        public void SetAutoRotate(bool enabled)
        {
            _autoRotate = enabled;
        }

        /// <summary>
        /// 特定の角度にカメラを設定
        /// </summary>
        public void SetCameraAngle(float horizontal, float vertical)
        {
            _targetHorizontalAngle = horizontal;
            _targetVerticalAngle = Mathf.Clamp(vertical, _minVerticalAngle, _maxVerticalAngle);
        }

        /// <summary>
        /// ズーム距離を設定
        /// </summary>
        public void SetZoomDistance(float distance)
        {
            _targetDistance = Mathf.Clamp(distance, _minDistance, _maxDistance);
        }
    }
}
