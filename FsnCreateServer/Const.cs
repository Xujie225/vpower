using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace FsnCreateServer
{
    /// <summary>
    /// ファイル出力モジュール共通定数クラス
    /// </summary>
    public static class Const
    {
        /// <summary>
        /// 成功
        /// </summary>
        public const int SUCCESS = 0;

        /// <summary>
        /// 失敗
        /// </summary>
        public const int FAILURE = 1;

        /// <summary>
        /// 失敗（SDカード未セット）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_UNSET = 2;

        /// <summary>
        /// 失敗（SDカードフル）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_FULL = 3;

        /// <summary>
        /// 失敗（SDカード書き込み異常）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE = 4;

        /// <summary>
        /// 失敗（SDカードニアフル）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_NEAR_FULL = 5;

        /// <summary>
        /// 成功（SDカードニアフル近い）
        /// </summary>
        public const int SUCCESS_OUTPUT_SD_CARD_NOT_NEAR_FULL = 6;

        /// <summary>
        /// 失敗（SDカードファイル数ニアフル）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_FILE_COUNT_NEAR_FULL = 7;

        /// <summary>
        /// 失敗（SDカードファイル数フル）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_FILE_COUNT_FULL = 8;

        /// <summary>
        /// 成功（SDカードファイル数ニアフル近い）
        /// </summary>
        public const int SUCCESS_OUTPUT_SD_CARD_FILE_COUNT_NOT_NEAR_FULL = 9;

        /// <summary>
        /// アップロード失敗
        /// </summary>
        public const int UPLOAD_FAILURE = 10;

        /// <summary>
        /// 失敗（SDカードフォルダ数フル）
        /// </summary>
        public const int FAILURE_OUTPUT_SD_CARD_UPLOADED_DIR_COUNT_FULL = 11;

        /// <summary>
        /// デフォルトステータス
        /// </summary>
        public const int DEFAULT_OUTPUT_SD_CARD = -1;

        /// <summary>
        /// 画像なし
        /// </summary>
        public const int CAPTURE_NONE = 0x00000000;

        /// <summary>
        /// 全面キャプチャ(両面16階調 100x100dpi)
        /// </summary>
        public const int CAPTURE_FULL_GRAYSCALE_100DPI_100DPI = 0x00000001;

        /// <summary>
        /// 全面キャプチャ(両面16階調 200x100dpi)
        /// </summary>
        public const int CAPTURE_FULL_GRAYSCALE_200DPI_100DPI = 0x00000002;

        /// <summary>
        /// 記番号部分キャプチャ(片側16階調 100x100dpi)
        /// </summary>
        public const int CAPTURE_AREA_GRAYSCALE_100DPI_100DPI = 0x00000004;

        /// <summary>
        /// 記番号部分キャプチャ(両側256階調 200x100dpi)
        /// </summary>
        public const int CAPTURE_AREA_GRAYSCALE_200DPI_100DPI = 0x00000008;

        /// <summary>
        /// 記番号部分キャプチャ(片側2階調 セグメント分割)
        /// </summary>
        public const int CAPTURE_SEGMENT_MONO = 0x0000010;

        /// <summary>
        /// 記番号部分キャプチャ(片側2階調)
        /// </summary>
        public const int CAPTURE_AREA_MONO = 0x0000020;

        /// <summary>
        /// FSNファイルの拡張子
        /// </summary>
        public const string EXTENSION_FSN = ".FSN";

        /// <summary>
        /// GLYファイルの拡張子
        /// </summary>
        public const string EXTENSION_GLY = ".GLY";

        /// <summary>
        /// DATファイルの拡張子
        /// </summary>
        public const string EXTENSION_DAT = ".DAT";

        /// <summary>
        /// zipファイルの拡張子
        /// </summary>
        public const string EXTENSION_ZIP = ".zip";

        /// <summary>
        /// nowファイルの拡張子
        /// </summary>
        public const string EXTENSION_NOW = ".now";

        /// <summary>
        /// tmpファイルの拡張子
        /// </summary>
        public const string EXTENSION_TMP = ".tmp";

        /// <summary>
        /// tempファイルの拡張子
        /// </summary>
        public const string EXTENSION_TEMP = ".temp";

        /// <summary>
        /// 記号(*)
        /// </summary>
        public const string ASTERISK = "*";

        /// <summary>
        /// 通信プロトコルFTP
        /// </summary>
        public const int TRANSMIT_PROTOCOL_FTP = 0;

        /// <summary>
        /// 通信プロトコルTCP
        /// </summary>
        public const int TRANSMIT_PROTOCOL_TCP = 1;

        /// <summary>
        /// 通信ステータスデフォルト値
        /// </summary>
        public const int UPLOAD_STATE_DEFAULT = -1;

        /// <summary>
        /// 容量ニアフル解除チェックサイズ（300MB）
        /// </summary>
        public const ulong DISK_SPACE_NOT_NEAR_FULL = 314572800;

        /// <summary>
        /// 容量ニアフルチェックサイズ（200MB）
        /// </summary>
        public const ulong DISK_SPACE_NEAR_FULL = 209715200;

        /// <summary>
        /// 容量フルチェックサイズ（20MB）
        /// </summary>
        public const ulong DISK_SPACE_FULL = 20971520;

        /// <summary>
        /// ファイル数ニアフル解除チェックのカウント（800個）
        /// </summary>
        public const int FILE_COUNT_NOT_NEAR_FULL = 800;

        /// <summary>
        /// ファイル数ニアフルチェックのカウント（850個）
        /// </summary>
        public const int FILE_COUNT_NEAR_FULL = 850;

        /// <summary>
        /// ファイル数フルチェックのカウント（950個）
        /// </summary>
        public const int FILE_COUNT_FULL = 950;

        /// <summary>
        /// 外部SDフォルダ数ファイルフルチェックのカウント
        /// </summary>
        public const int UPLOADED_SUBDIRECTORY_FILE_COUNT_FULL = 999;

        /// <summary>
        /// 外部SDフォルダ数フルチェックのカウント
        /// </summary>
        public const int UPLOADED_SUBDIRECTORY_COUNT_FULL = 998;

        /// <summary>
        /// サブフォルダのスタート基準
        /// </summary>
        public const string SUB_DIRECTORY_START = "001";

        /// <summary>
        /// デフォルト取引タイプ
        /// </summary>
        public const string DEFAULT_SERVICE_TYPE = "HM";

        //@@@ add UW-F4_広電サーバ向け_FTP送信_GlorySuzhou_201712_wu ↓
        /// <summary>
        /// デフォルト業務タイプ(４桁)
        /// </summary>
        public const string DEFAULT_SERVICE_TYPE_GRG = "GZHM";
        //@@@ add UW-F4_広電サーバ向け_FTP送信_GlorySuzhou_201712_wu ↑

        /// <summary>
        /// ファイル名におけるファイル時刻位置を指す文字列
        /// </summary>
        public const string FAIL_MAKING_DATE = "FAILMAIKINGDATE";
    }
}
