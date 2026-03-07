FROM node:18-alpine

# 作業ディレクトリ設定
WORKDIR /app

# package.jsonとpackage-lock.jsonをコピー
COPY package*.json ./

# 依存関係インストール
RUN npm ci --only=production

# アプリケーションファイルをコピー
COPY . .

# ログディレクトリ作成
RUN mkdir -p logs

# ポート公開
EXPOSE 3000

# ヘルスチェック
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s \
  CMD node -e "require('http').get('http://localhost:3000/health', (r) => {process.exit(r.statusCode === 200 ? 0 : 1)})"

# アプリケーション起動
CMD ["npm", "start"]
