/**
 * カスタムエラークラス
 */

class AppError extends Error {
  constructor(message, statusCode, code) {
    super(message);
    this.statusCode = statusCode;
    this.code = code;
    this.isOperational = true;

    Error.captureStackTrace(this, this.constructor);
  }
}

class NotFoundError extends AppError {
  constructor(message = 'リソースが見つかりません') {
    super(message, 404, 'NOT_FOUND');
  }
}

class UnauthorizedError extends AppError {
  constructor(message = '認証が必要です') {
    super(message, 401, 'UNAUTHORIZED');
  }
}

class ForbiddenError extends AppError {
  constructor(message = 'アクセス権限がありません') {
    super(message, 403, 'FORBIDDEN');
  }
}

class ValidationError extends AppError {
  constructor(message = 'バリデーションエラー', details = null) {
    super(message, 400, 'VALIDATION_ERROR');
    this.details = details;
  }
}

class InsufficientFundsError extends AppError {
  constructor(message = '通貨が不足しています') {
    super(message, 400, 'INSUFFICIENT_FUNDS');
  }
}

class VipOnlyError extends AppError {
  constructor(message = 'VIP限定アイテムです') {
    super(message, 403, 'VIP_ONLY');
  }
}

module.exports = {
  AppError,
  NotFoundError,
  UnauthorizedError,
  ForbiddenError,
  ValidationError,
  InsufficientFundsError,
  VipOnlyError,
};
