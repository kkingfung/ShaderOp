#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Firebase Unity SDK ダウンロードスクリプト

Firebase Unity SDKの最新版をダウンロードし、
必要なパッケージを抽出します。
"""

import os
import sys
import urllib.request
import zipfile
import shutil
from pathlib import Path

# Windows コンソールのエンコーディング設定
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

# Firebase Unity SDK ダウンロードURL（最新版）
FIREBASE_SDK_URL = "https://dl.google.com/firebase/sdk/unity/firebase_unity_sdk_12.3.0.zip"
FIREBASE_VERSION = "12.3.0"

# 必要なFirebaseパッケージ
REQUIRED_PACKAGES = [
    "FirebaseAuth.unitypackage",
    "FirebaseDatabase.unitypackage",
    "FirebaseStorage.unitypackage",
]

def download_firebase_sdk(download_dir: Path) -> Path:
    """
    Firebase Unity SDKをダウンロードします

    Args:
        download_dir: ダウンロード先ディレクトリ

    Returns:
        ダウンロードしたZIPファイルのパス
    """
    download_dir.mkdir(parents=True, exist_ok=True)
    zip_path = download_dir / f"firebase_unity_sdk_{FIREBASE_VERSION}.zip"

    if zip_path.exists():
        print(f"✅ Firebase SDK already downloaded: {zip_path}")
        return zip_path

    print(f"📥 Downloading Firebase Unity SDK {FIREBASE_VERSION}...")
    print(f"   URL: {FIREBASE_SDK_URL}")

    try:
        urllib.request.urlretrieve(FIREBASE_SDK_URL, zip_path)
        print(f"✅ Download completed: {zip_path}")
        return zip_path
    except Exception as e:
        print(f"❌ Download failed: {e}")
        sys.exit(1)

def extract_firebase_packages(zip_path: Path, extract_dir: Path) -> list[Path]:
    """
    必要なFirebaseパッケージをZIPから抽出します

    Args:
        zip_path: Firebase SDK ZIPファイルのパス
        extract_dir: 抽出先ディレクトリ

    Returns:
        抽出したunitypackageファイルのパスリスト
    """
    extract_dir.mkdir(parents=True, exist_ok=True)
    extracted_packages = []

    print(f"\n📦 Extracting Firebase packages...")

    try:
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            # ZIPファイル内のすべてのファイルをリスト
            all_files = zip_ref.namelist()

            # 必要なパッケージのみを抽出
            for package_name in REQUIRED_PACKAGES:
                # パッケージファイルを検索
                matching_files = [f for f in all_files if f.endswith(package_name)]

                if not matching_files:
                    print(f"⚠️  Package not found in ZIP: {package_name}")
                    continue

                # 最初にマッチしたファイルを抽出
                source_file = matching_files[0]
                target_file = extract_dir / package_name

                # ファイルを抽出
                with zip_ref.open(source_file) as source:
                    with open(target_file, 'wb') as target:
                        shutil.copyfileobj(source, target)

                print(f"   ✅ Extracted: {package_name}")
                extracted_packages.append(target_file)

        return extracted_packages

    except Exception as e:
        print(f"❌ Extraction failed: {e}")
        sys.exit(1)

def generate_import_instructions(packages: list[Path]):
    """
    Unityへのインポート手順を表示します

    Args:
        packages: unitypackageファイルのパスリスト
    """
    print("\n" + "=" * 80)
    print("🎯 FIREBASE SDK IMPORT INSTRUCTIONS")
    print("=" * 80)
    print("\n📝 Follow these steps to import Firebase SDK into Unity:\n")

    print("1. Open Unity Editor (ShaderOp project)")
    print("\n2. Import Firebase packages in this order:\n")

    for i, package in enumerate(packages, 1):
        print(f"   {i}. Assets > Import Package > Custom Package...")
        print(f"      Select: {package.name}")
        print(f"      Click 'Import' to import all files")
        print()

    print("3. Wait for Unity to compile (this may take several minutes)")
    print("\n4. Verify installation:")
    print("   - Check for 'Firebase' folder in Project window")
    print("   - No compilation errors should appear")
    print("\n5. Configure Firebase:")
    print("   - Download google-services.json from Firebase Console")
    print("   - Place it in Assets/StreamingAssets/")

    print("\n" + "=" * 80)
    print(f"📂 Extracted packages location: {packages[0].parent}")
    print("=" * 80 + "\n")

def main():
    """メイン処理"""
    # プロジェクトルートディレクトリ
    project_root = Path(__file__).parent.parent

    # ダウンロード・抽出先ディレクトリ
    download_dir = project_root / "Temp" / "Firebase"

    print("=" * 80)
    print("🔥 Firebase Unity SDK Download Script")
    print("=" * 80)
    print(f"\nProject Root: {project_root}")
    print(f"Download Dir: {download_dir}")
    print()

    # Firebase SDKをダウンロード
    zip_path = download_firebase_sdk(download_dir)

    # 必要なパッケージを抽出
    extracted_packages = extract_firebase_packages(zip_path, download_dir)

    if not extracted_packages:
        print("❌ No packages extracted. Aborting.")
        sys.exit(1)

    # インポート手順を表示
    generate_import_instructions(extracted_packages)

    print("✅ Firebase SDK download completed successfully!")

if __name__ == "__main__":
    main()
