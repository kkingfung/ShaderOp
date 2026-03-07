#nullable enable

using System;
using Cysharp.Threading.Tasks;

namespace ShaderOp.Core.Services.Online
{
    /// <summary>
    /// Firebase認証サービスインターフェース
    /// </summary>
    /// <remarks>
    /// Firebase Authenticationを使用してユーザー認証を管理します。
    /// JWT トークンを取得し、バックエンドAPIへのアクセスに使用します。
    /// </remarks>
    public interface IFirebaseAuthService
    {
        /// <summary>認証済みか</summary>
        bool IsAuthenticated { get; }

        /// <summary>現在のユーザーID</summary>
        string? CurrentUserId { get; }

        /// <summary>現在のユーザー表示名</summary>
        string? CurrentUserDisplayName { get; }

        /// <summary>現在のユーザーメールアドレス</summary>
        string? CurrentUserEmail { get; }

        /// <summary>最後に取得したIDトークン</summary>
        string? CurrentIdToken { get; }

        /// <summary>認証状態変更イベント</summary>
        event Action<bool>? OnAuthStateChanged;

        /// <summary>
        /// メールアドレスとパスワードでサインイン
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> SignInWithEmailAndPasswordAsync(string email, string password);

        /// <summary>
        /// メールアドレスとパスワードで新規登録
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <param name="displayName">表示名（オプション）</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> SignUpWithEmailAndPasswordAsync(string email, string password, string? displayName = null);

        /// <summary>
        /// 匿名ログイン
        /// </summary>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> SignInAnonymouslyAsync();

        /// <summary>
        /// サインアウト
        /// </summary>
        void SignOut();

        /// <summary>
        /// IDトークンを取得
        /// </summary>
        /// <param name="forceRefresh">強制的にトークンを更新するか</param>
        /// <returns>IDトークン文字列</returns>
        UniTask<string?> GetIdTokenAsync(bool forceRefresh = false);

        /// <summary>
        /// パスワードリセットメール送信
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> SendPasswordResetEmailAsync(string email);

        /// <summary>
        /// 表示名を更新
        /// </summary>
        /// <param name="displayName">新しい表示名</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> UpdateDisplayNameAsync(string displayName);

        /// <summary>
        /// メールアドレスを更新
        /// </summary>
        /// <param name="newEmail">新しいメールアドレス</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> UpdateEmailAsync(string newEmail);

        /// <summary>
        /// パスワードを更新
        /// </summary>
        /// <param name="newPassword">新しいパスワード</param>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> UpdatePasswordAsync(string newPassword);

        /// <summary>
        /// アカウントを削除
        /// </summary>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> DeleteAccountAsync();

        /// <summary>
        /// 現在のユーザー情報を再読み込み
        /// </summary>
        /// <returns>成功した場合はtrue</returns>
        UniTask<bool> ReloadUserAsync();
    }
}
