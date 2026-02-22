#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Core
{
    /// <summary>
    /// 入力アクション種別
    /// </summary>
    public enum InputAction
    {
        Confirm,
        Cancel,
        Menu,
        RotateLeft,
        RotateRight,
        ZoomIn,
        ZoomOut,
        Reset,
        Screenshot
    }

    /// <summary>
    /// 入力マネージャー
    /// </summary>
    /// <remarks>
    /// キーボード、マウス、タッチ入力を統合管理
    /// アクションベースの入力システムを提供
    /// </remarks>
    public class InputManager : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static InputManager? _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("InputManager");
                    _instance = go.AddComponent<InputManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>入力アクションイベント</summary>
        public event Action<InputAction>? OnInputAction;

        /// <summary>マウス移動イベント</summary>
        public event Action<Vector2>? OnMouseMove;

        /// <summary>マウスドラッグイベント</summary>
        public event Action<Vector2>? OnMouseDrag;

        /// <summary>マウススクロールイベント</summary>
        public event Action<float>? OnMouseScroll;

        /// <summary>入力有効フラグ</summary>
        public bool IsInputEnabled { get; set; } = true;

        /// <summary>前フレームのマウス位置</summary>
        private Vector2 _lastMousePosition;

        /// <summary>マウスドラッグ中フラグ</summary>
        private bool _isDragging = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!IsInputEnabled)
                return;

            UpdateKeyboardInput();
            UpdateMouseInput();
        }

        /// <summary>
        /// キーボード入力を更新
        /// </summary>
        private void UpdateKeyboardInput()
        {
            // Confirm (Enter, Space)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnInputAction?.Invoke(InputAction.Confirm);
            }

            // Cancel (Escape)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnInputAction?.Invoke(InputAction.Cancel);
            }

            // Menu (Tab)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                OnInputAction?.Invoke(InputAction.Menu);
            }

            // RotateLeft (Q)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnInputAction?.Invoke(InputAction.RotateLeft);
            }

            // RotateRight (E)
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInputAction?.Invoke(InputAction.RotateRight);
            }

            // ZoomIn (+, PageUp)
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.PageUp))
            {
                OnInputAction?.Invoke(InputAction.ZoomIn);
            }

            // ZoomOut (-, PageDown)
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.PageDown))
            {
                OnInputAction?.Invoke(InputAction.ZoomOut);
            }

            // Reset (R)
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnInputAction?.Invoke(InputAction.Reset);
            }

            // Screenshot (F12)
            if (Input.GetKeyDown(KeyCode.F12))
            {
                OnInputAction?.Invoke(InputAction.Screenshot);
                TakeScreenshot();
            }
        }

        /// <summary>
        /// マウス入力を更新
        /// </summary>
        private void UpdateMouseInput()
        {
            // マウス移動
            Vector2 currentMousePosition = Input.mousePosition;
            if (currentMousePosition != _lastMousePosition)
            {
                OnMouseMove?.Invoke(currentMousePosition);
            }

            // マウスドラッグ
            if (Input.GetMouseButton(0)) // 左クリックホールド
            {
                if (!_isDragging)
                {
                    _isDragging = true;
                }

                Vector2 delta = currentMousePosition - _lastMousePosition;
                if (delta.magnitude > 0.1f)
                {
                    OnMouseDrag?.Invoke(delta);
                }
            }
            else if (_isDragging)
            {
                _isDragging = false;
            }

            _lastMousePosition = currentMousePosition;

            // マウススクロール
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                OnMouseScroll?.Invoke(scrollDelta);

                // スクロールでズーム
                if (scrollDelta > 0)
                {
                    OnInputAction?.Invoke(InputAction.ZoomIn);
                }
                else if (scrollDelta < 0)
                {
                    OnInputAction?.Invoke(InputAction.ZoomOut);
                }
            }
        }

        /// <summary>
        /// 特定のアクションが入力されたか確認
        /// </summary>
        public bool GetInputDown(InputAction action)
        {
            switch (action)
            {
                case InputAction.Confirm:
                    return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

                case InputAction.Cancel:
                    return Input.GetKeyDown(KeyCode.Escape);

                case InputAction.Menu:
                    return Input.GetKeyDown(KeyCode.Tab);

                case InputAction.RotateLeft:
                    return Input.GetKeyDown(KeyCode.Q);

                case InputAction.RotateRight:
                    return Input.GetKeyDown(KeyCode.E);

                case InputAction.ZoomIn:
                    return Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.PageUp);

                case InputAction.ZoomOut:
                    return Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.PageDown);

                case InputAction.Reset:
                    return Input.GetKeyDown(KeyCode.R);

                case InputAction.Screenshot:
                    return Input.GetKeyDown(KeyCode.F12);

                default:
                    return false;
            }
        }

        /// <summary>
        /// マウスのワールド座標を取得
        /// </summary>
        public Vector3 GetMouseWorldPosition(Camera? camera = null)
        {
            Camera cam = camera ?? Camera.main;
            if (cam == null)
                return Vector3.zero;

            return cam.ScreenToWorldPoint(Input.mousePosition);
        }

        /// <summary>
        /// マウスでRaycastを実行
        /// </summary>
        public bool RaycastMouse<T>(out T? component, Camera? camera = null) where T : Component
        {
            component = null;
            Camera cam = camera ?? Camera.main;
            if (cam == null)
                return false;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                component = hit.collider.GetComponent<T>();
                return component != null;
            }

            return false;
        }

        /// <summary>
        /// 2D Raycastを実行
        /// </summary>
        public bool Raycast2DMouse<T>(out T? component, Camera? camera = null) where T : Component
        {
            component = null;
            Camera cam = camera ?? Camera.main;
            if (cam == null)
                return false;

            Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                component = hit.collider.GetComponent<T>();
                return component != null;
            }

            return false;
        }

        /// <summary>
        /// スクリーンショットを撮影
        /// </summary>
        private void TakeScreenshot()
        {
            string filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($"[InputManager] Screenshot saved to {path}");
        }

        /// <summary>
        /// 入力を有効化
        /// </summary>
        public void EnableInput()
        {
            IsInputEnabled = true;
            Debug.Log("[InputManager] Input enabled");
        }

        /// <summary>
        /// 入力を無効化
        /// </summary>
        public void DisableInput()
        {
            IsInputEnabled = false;
            Debug.Log("[InputManager] Input disabled");
        }

        /// <summary>
        /// タッチ入力をサポートしているか
        /// </summary>
        public bool IsTouchSupported()
        {
            return Input.touchSupported;
        }

        /// <summary>
        /// タッチ数を取得
        /// </summary>
        public int GetTouchCount()
        {
            return Input.touchCount;
        }

        /// <summary>
        /// タッチ情報を取得
        /// </summary>
        public Touch? GetTouch(int index)
        {
            if (index >= 0 && index < Input.touchCount)
            {
                return Input.GetTouch(index);
            }
            return null;
        }
    }
}
