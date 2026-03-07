#nullable enable

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShaderOp.UI.Navigation
{
    /// <summary>
    /// トースト通知タイプ
    /// </summary>
    public enum ToastType
    {
        Success,
        Error,
        Info,
        Warning
    }

    /// <summary>
    /// UI コンポーネントファクトリー
    /// 動的にUIコンポーネントを生成するユーティリティクラス
    /// </summary>
    public static class UIComponentFactory
    {
        /// <summary>
        /// ショップアイテムカードを生成
        /// </summary>
        public static VisualElement CreateShopItemCard(string itemName, string itemPrice, Sprite? thumbnail = null, Action? onBuyClicked = null)
        {
            var card = new VisualElement();
            card.AddToClassList("shop-item-card");

            // サムネイル
            var thumbnailContainer = new VisualElement { name = "ItemThumbnail" };
            thumbnailContainer.AddToClassList("item-thumbnail");
            if (thumbnail != null)
            {
                thumbnailContainer.style.backgroundImage = new StyleBackground(thumbnail);
            }
            card.Add(thumbnailContainer);

            // アイテム名
            var nameLabel = new Label(itemName);
            nameLabel.AddToClassList("label-body");
            card.Add(nameLabel);

            // 価格
            var priceLabel = new Label(itemPrice);
            priceLabel.AddToClassList("label-caption");
            priceLabel.AddToClassList("text-accent");
            card.Add(priceLabel);

            // 購入ボタン
            var buyButton = new Button(() => onBuyClicked?.Invoke()) { text = "Buy" };
            buyButton.AddToClassList("button-sm");
            buyButton.AddToClassList("touch-target-lg");
            buyButton.AddToClassList("w-full");
            card.Add(buyButton);

            return card;
        }

        /// <summary>
        /// フレンドカードを生成
        /// </summary>
        public static VisualElement CreateFriendCard(string friendName, bool isOnline, Sprite? avatar = null, Action? onChatClicked = null, Action? onInviteClicked = null)
        {
            var card = new VisualElement();
            card.AddToClassList("friend-item");
            card.AddToClassList("touch-target-lg");

            // アバター
            var avatarContainer = new VisualElement { name = "FriendAvatar" };
            avatarContainer.AddToClassList("avatar-md");
            if (avatar != null)
            {
                avatarContainer.style.backgroundImage = new StyleBackground(avatar);
            }

            // オンラインステータス
            var status = new VisualElement { name = "OnlineStatus" };
            status.AddToClassList(isOnline ? "status-online" : "status-offline");
            avatarContainer.Add(status);
            card.Add(avatarContainer);

            // 情報コンテナ
            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("flex-column");
            infoContainer.AddToClassList("flex-grow");

            var nameLabel = new Label(friendName);
            nameLabel.AddToClassList("label-body");
            infoContainer.Add(nameLabel);

            var statusLabel = new Label(isOnline ? "Online" : "Offline");
            statusLabel.AddToClassList("label-caption");
            statusLabel.AddToClassList(isOnline ? "text-success" : "text-disabled");
            infoContainer.Add(statusLabel);
            card.Add(infoContainer);

            // ボタンコンテナ
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("flex-row");

            var chatButton = new Button(() => onChatClicked?.Invoke()) { text = "💬" };
            chatButton.AddToClassList("button-icon");
            buttonContainer.Add(chatButton);

            var inviteButton = new Button(() => onInviteClicked?.Invoke()) { text = "🎮" };
            inviteButton.AddToClassList("button-icon");
            buttonContainer.Add(inviteButton);

            card.Add(buttonContainer);

            return card;
        }

        /// <summary>
        /// チャットメッセージを生成
        /// </summary>
        public static VisualElement CreateChatMessage(string senderName, string messageText, bool isOwnMessage = false, Sprite? senderAvatar = null)
        {
            var message = new VisualElement();
            message.AddToClassList("chat-message");
            if (isOwnMessage)
            {
                message.AddToClassList("chat-message-own");
            }

            if (!isOwnMessage)
            {
                // 相手のメッセージの場合はアバター表示
                var avatar = new VisualElement { name = "SenderAvatar" };
                avatar.AddToClassList("avatar-sm");
                if (senderAvatar != null)
                {
                    avatar.style.backgroundImage = new StyleBackground(senderAvatar);
                }
                message.Add(avatar);
            }

            var contentContainer = new VisualElement();
            contentContainer.AddToClassList("flex-column");
            contentContainer.AddToClassList("flex-grow");

            if (!isOwnMessage)
            {
                var nameLabel = new Label(senderName);
                nameLabel.AddToClassList("label-caption");
                contentContainer.Add(nameLabel);
            }

            var textLabel = new Label(messageText);
            textLabel.AddToClassList("label-body");
            contentContainer.Add(textLabel);

            message.Add(contentContainer);

            return message;
        }

        /// <summary>
        /// 通知トーストを生成
        /// </summary>
        public static VisualElement CreateToast(string message, ToastType type = ToastType.Success)
        {
            var toast = new VisualElement();
            toast.AddToClassList("toast");

            string icon = "";
            switch (type)
            {
                case ToastType.Success:
                    toast.AddToClassList("toast-success");
                    icon = "✓ ";
                    break;
                case ToastType.Error:
                    toast.AddToClassList("toast-error");
                    icon = "✕ ";
                    break;
                case ToastType.Info:
                    toast.AddToClassList("toast-info");
                    icon = "ℹ ";
                    break;
                case ToastType.Warning:
                    toast.AddToClassList("toast-warning");
                    icon = "⚠ ";
                    break;
            }

            var label = new Label(icon + message);
            label.AddToClassList("label-body");
            toast.Add(label);

            return toast;
        }

        /// <summary>
        /// プレイヤースロットを生成
        /// </summary>
        public static VisualElement CreatePlayerSlot(string playerName, int level, bool isOnline = true, Sprite? avatar = null)
        {
            var slot = new VisualElement();
            slot.AddToClassList("player-slot");
            slot.AddToClassList("flex-row");

            // アバター
            var avatarContainer = new VisualElement { name = "PlayerAvatar" };
            avatarContainer.AddToClassList("avatar-md");
            if (avatar != null)
            {
                avatarContainer.style.backgroundImage = new StyleBackground(avatar);
            }

            // オンラインステータス
            var status = new VisualElement { name = "OnlineStatus" };
            status.AddToClassList(isOnline ? "status-online" : "status-offline");
            avatarContainer.Add(status);
            slot.Add(avatarContainer);

            // 情報
            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("flex-column");

            var nameLabel = new Label(playerName);
            nameLabel.AddToClassList("label-body");
            infoContainer.Add(nameLabel);

            var levelLabel = new Label($"Lv {level}");
            levelLabel.AddToClassList("label-caption");
            infoContainer.Add(levelLabel);

            slot.Add(infoContainer);

            return slot;
        }

        /// <summary>
        /// ゲーム選択カードを生成
        /// </summary>
        public static VisualElement CreateGameCard(string gameName, string description, Sprite? thumbnail = null, Action? onPlayClicked = null)
        {
            var card = new VisualElement();
            card.AddToClassList("game-card");
            card.AddToClassList("card-portrait");

            // サムネイル
            var thumbnailContainer = new VisualElement { name = "GameThumbnail" };
            thumbnailContainer.AddToClassList("game-thumbnail");
            if (thumbnail != null)
            {
                thumbnailContainer.style.backgroundImage = new StyleBackground(thumbnail);
            }
            card.Add(thumbnailContainer);

            // ゲーム名
            var nameLabel = new Label(gameName);
            nameLabel.AddToClassList("label-heading");
            card.Add(nameLabel);

            // 説明
            var descLabel = new Label(description);
            descLabel.AddToClassList("label-caption");
            card.Add(descLabel);

            // プレイボタン
            var playButton = new Button(() => onPlayClicked?.Invoke()) { text = "Play" };
            playButton.AddToClassList("button-lg");
            playButton.AddToClassList("touch-target-lg");
            playButton.AddToClassList("w-full");
            card.Add(playButton);

            return card;
        }

        /// <summary>
        /// ローディングオーバーレイを生成
        /// </summary>
        public static VisualElement CreateLoadingOverlay(string message = "Loading...")
        {
            var overlay = new VisualElement();
            overlay.AddToClassList("modal-overlay");
            overlay.AddToClassList("loading-overlay");

            var container = new VisualElement();
            container.AddToClassList("flex-column");
            container.AddToClassList("flex-center");

            var spinner = new VisualElement { name = "LoadingSpinner" };
            spinner.AddToClassList("spinner");
            container.Add(spinner);

            var label = new Label(message);
            label.AddToClassList("label-body");
            label.AddToClassList("mt-md");
            container.Add(label);

            overlay.Add(container);

            return overlay;
        }

        /// <summary>
        /// 確認ダイアログを生成
        /// </summary>
        public static VisualElement CreateConfirmDialog(string title, string message, Action? onConfirm = null, Action? onCancel = null)
        {
            var overlay = new VisualElement();
            overlay.AddToClassList("modal-overlay");

            var content = new VisualElement();
            content.AddToClassList("modal-content");

            // ヘッダー
            var header = new VisualElement();
            header.AddToClassList("flex-row");
            header.AddToClassList("justify-between");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("label-heading");
            header.Add(titleLabel);

            var closeButton = new Button(() => onCancel?.Invoke()) { text = "✕" };
            closeButton.AddToClassList("button-icon");
            header.Add(closeButton);

            content.Add(header);

            // ボディ
            var body = new VisualElement();
            body.AddToClassList("flex-grow");
            body.AddToClassList("p-md");

            var messageLabel = new Label(message);
            messageLabel.AddToClassList("label-body");
            body.Add(messageLabel);

            content.Add(body);

            // フッター
            var footer = new VisualElement();
            footer.AddToClassList("flex-row");
            footer.AddToClassList("justify-end");

            var cancelButton = new Button(() => onCancel?.Invoke()) { text = "Cancel" };
            cancelButton.AddToClassList("button-outline");
            footer.Add(cancelButton);

            var confirmButton = new Button(() => onConfirm?.Invoke()) { text = "Confirm" };
            confirmButton.AddToClassList("button-success");
            footer.Add(confirmButton);

            content.Add(footer);
            overlay.Add(content);

            return overlay;
        }
    }
}
