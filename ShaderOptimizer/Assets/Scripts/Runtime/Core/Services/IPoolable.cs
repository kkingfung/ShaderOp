#nullable enable

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// プール可能なオブジェクトのライフサイクルフック
    /// </summary>
    /// <remarks>
    /// ObjectPoolServiceで管理されるオブジェクトがこのインターフェースを実装すると、
    /// プールからの取得時・返却時にコールバックを受け取れます。
    /// 例: HexTileのリセット処理、GamePieceの状態初期化など
    /// </remarks>
    public interface IPoolable
    {
        /// <summary>
        /// プールから取得された時に呼ばれる
        /// </summary>
        /// <remarks>
        /// オブジェクトのリセット・初期化処理を実装します。
        /// このメソッドの後にgameObject.SetActive(true)が呼ばれます。
        /// </remarks>
        void OnGetFromPool();

        /// <summary>
        /// プールに返却される時に呼ばれる
        /// </summary>
        /// <remarks>
        /// オブジェクトのクリーンアップ処理を実装します。
        /// このメソッドの後にgameObject.SetActive(false)が呼ばれます。
        /// </remarks>
        void OnReturnToPool();
    }
}
