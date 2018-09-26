using log4net;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FsnCreateServer
{
    public class FileOperate
    {
        private static ILog log = LogManager.GetLogger(typeof(FileOperate));

        /// <summary>
        /// Win32API DLL名（Kernel32.dll）
        /// </summary>
        private const string KERNELDLL_NAME = "Kernel32.dll";

        /// <summary>
        /// ニアフル解除フラグ
        /// </summary>
        private static bool isNotNearFull = false;

        /// <summary>
        /// ニアフルフラグ
        /// </summary>
        private static bool isNearFull = false;

        /// <summary>
        /// フルフラグ
        /// </summary>
        private static bool isFull = false;

        /// <summary>
        /// ファイル数ニアフルフラグ
        /// </summary>
        private static bool isFileCountNearFull = false;

        /// <summary>
        /// ファイル数フルフラグ
        /// </summary>
        private static bool isFileCountFull = false;

        /// <summary>
        /// ファイル数ニアフル近いフラグ
        /// </summary>
        private static bool isNotFileCountNearFull = false;

        /// <summary>
        /// 外部ＳＤカードパス
        /// </summary>
        private static string mediaPath = "";
        private static string MediaPath
        {
            get
            {
                return mediaPath;
            }
        }

        /// <summary>
        /// 外部ＳＤのパスを初期化する
        /// </summary>
        /// <param name="externalMediaPath">外部ＳＤパス</param>
        public static void Initialize(string externalMediaPath)
        {
            mediaPath = externalMediaPath;
        }

        /// <summary>
        /// ファイルを削除メソッド
        /// </summary>
        /// <param name="filePath">削除するファイルのパス</param>
        /// <returns>結果</returns>
        public static int DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return Const.FAILURE;
                }

                if (!Directory.Exists(mediaPath))
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                if (File.Exists(filePath))
                {
                    // ファイルを削除
                    FileInfo fi = new FileInfo(filePath);
                    fi.Delete();
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$DeleteFile()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// ファイルをコピーするメソッド
        /// </summary>
        /// <param name="sourceFilePath">ソースファイルのパス</param>
        /// <param name="destFilePath">ターゲットファイルのパス</param>
        /// <returns>結果</returns>
        public static int CopyFile(string sourceFilePath, string destFilePath, bool overwrite)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destFilePath))
                {
                    return Const.FAILURE;
                }

                if (!Directory.Exists(mediaPath))
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                if (File.Exists(sourceFilePath))
                {
                    // ファイルをコピー
                    FileInfo fi = new FileInfo(sourceFilePath);
                    fi.CopyTo(destFilePath, overwrite);
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CopyFile()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        //@@@ add UW-F4_V-Power向け強化_GlorySuzhou_201711_wu ↓
        /// <summary>
        /// ファイルをコピーしないメソッド
        /// </summary>
        /// <param name="sourceFilePath">ソースファイルのパス</param>
        /// <param name="destFilePath">ターゲットファイルのパス</param>
        /// <returns>結果</returns>
        public static int GetState(string sourceFilePath, string destFilePath, bool overwrite)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destFilePath))
                {
                    return Const.FAILURE;
                }

                if (!Directory.Exists(mediaPath))
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CopyFile()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }
        //@@@ add UW-F4_V-Power向け強化_GlorySuzhou_201711_wu ↑

        /// <summary>
        /// ファイル存在チェックメソッド
        /// </summary>
        /// <param name="dirPath">フォルダのパス</param>
        /// <returns>結果</returns>
        public static int CheckFileExist(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return Const.FAILURE;
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CheckFileExist()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// ファイルをリネームするメソッド
        /// </summary>
        /// <param name="oldFilePath">古いファイルパス</param>
        /// <param name="newFilePath">新いファイルパス</param>
        /// <returns>結果</returns>
        public static int RenameFile(string oldFilePath, string newFilePath)
        {
            try
            {
                if (!Directory.Exists(mediaPath))
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                if (string.IsNullOrEmpty(oldFilePath) || string.IsNullOrEmpty(newFilePath)
                    || FileOperate.CheckFileExist(oldFilePath) == Const.FAILURE
                    || FileOperate.CheckFileExist(newFilePath) == Const.SUCCESS)
                {
                    return Const.FAILURE;
                }

                FileInfo fi = new FileInfo(oldFilePath);
                fi.MoveTo(newFilePath);

                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$RenameFile()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        ///  メディア容量を取得するメソッド
        /// </summary>
        [DllImport(KERNELDLL_NAME, EntryPoint = "GetDiskFreeSpaceEx")]
        private static extern bool GetDiskFreeSpaceEx(string dirName,
                                                      out ulong freeBytesAvailableToCaller,
                                                      out ulong totalNumberOfBytes,
                                                      out ulong totalNumberOfFreeBytes);

        /// <summary>
        /// メディア容量をチェックするメソッド
        /// </summary>
        /// <param name="notNearFullSize">ニアフル近いチェックサイズ</param>
        /// <param name="nearFullSize">ニアフルチェックサイズ</param>
        /// <param name="fullSize">フルチェックサイズ</param>
        /// <returns>結果</returns>
        public static int CheckMediaCapacity(ulong notNearFullSize, ulong nearFullSize, ulong fullSize)
        {
            try
            {
                bool ret = true;

                // ディレクトリ名
                string directory = mediaPath;

                // 呼び出し側が利用できるバイト数
                ulong caller = 0;

                // ディスク全体のバイト数
                ulong total = 0;

                // ディスク全体の空きバイト数
                ulong freeSpace = 0;

                // SD Memoryの容量を取得
                ret = GetDiskFreeSpaceEx(directory, out caller, out total, out freeSpace);

                if (!ret)
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                if (freeSpace <= fullSize)
                {
                    if (!isFull)
                    {
                        // ログを書き込み
                        log.Warn(string.Format("$CheckMediaCapacity()$:  total={0}MB, freeSpace={1}MB , FreeAvailable={2}% ", ((total / 1024) / 1024), ((freeSpace / 1024) / 1024), Math.Round(((Math.Round((double)freeSpace, 2) / Math.Round((double)total, 2)) * 100), 2)));
                    }
                    isFull = true;
                    isNearFull = false;
                    isNotNearFull = false;
                    return Const.FAILURE_OUTPUT_SD_CARD_FULL;
                }
                else if ((fullSize < freeSpace) && (freeSpace <= nearFullSize))
                {
                    if (!isNearFull)
                    {
                        // ログを書き込み
                        log.Warn(string.Format("$CheckMediaCapacity()$:  total={0}MB, freeSpace={1}MB , FreeAvailable={2}% ", ((total / 1024) / 1024), ((freeSpace / 1024) / 1024), Math.Round(((Math.Round((double)freeSpace, 2) / Math.Round((double)total, 2)) * 100), 2)));
                    }
                    isNearFull = true;
                    isFull = false;
                    isNotNearFull = false;
                    return Const.FAILURE_OUTPUT_SD_CARD_NEAR_FULL;
                }
                else if ((nearFullSize < freeSpace) && (freeSpace <= notNearFullSize))
                {
                    if (!isNotNearFull)
                    {
                        // ログを書き込み
                        log.Warn(string.Format("$CheckMediaCapacity()$:  total={0}MB, freeSpace={1}MB , FreeAvailable={2}% ", ((total / 1024) / 1024), ((freeSpace / 1024) / 1024), Math.Round(((Math.Round((double)freeSpace, 2) / Math.Round((double)total, 2)) * 100), 2)));
                    }
                    isNotNearFull = true;
                    isNearFull = false;
                    isFull = false;
                    return Const.SUCCESS_OUTPUT_SD_CARD_NOT_NEAR_FULL;
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $CheckMediaCapacity()$:  Exception - {0}.", ex.ToString()));
                isFull = false;
                isNearFull = false;
                isNotNearFull = false;
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// メディア容量をチェックするメソッド
        /// </summary>
        /// <param name="checkSize">チェックサイズ</param>
        /// <returns>結果</returns>
        public static int CheckMediaCapacity(ulong checkSize)
        {
            try
            {
                bool ret = true;

                // ディレクトリ名
                string directory = mediaPath;

                // 呼び出し側が利用できるバイト数
                ulong caller = 0;

                // ディスク全体のバイト数
                ulong total = 0;

                // ディスク全体の空きバイト数
                ulong freeSpace = 0;

                // SD Memoryの容量を取得
                ret = GetDiskFreeSpaceEx(directory, out caller, out total, out freeSpace);

                if (!ret)
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                // 容量不足
                if (freeSpace < checkSize)
                {
                    // ログを書き込み
                    log.Warn(string.Format("$CheckMediaCapacity()$:  total={0}MB, freeSpace={1}MB , FreeAvailable={2}% ", ((total / 1024) / 1024), ((freeSpace / 1024) / 1024), Math.Round(((Math.Round((double)freeSpace, 2) / Math.Round((double)total, 2)) * 100), 2)));
                    return Const.FAILURE_OUTPUT_SD_CARD_FULL;
                }

                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CheckMediaCapacity()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// メディア容量をチェックするメソッド
        /// </summary>
        /// <param name="checkSize">チェックパーセント</param>
        /// <returns>結果</returns>
        public static int CheckMediaCapacity(double checkPercent)
        {
            try
            {
                bool ret = true;

                // ディレクトリ名
                string directory = mediaPath;

                // 呼び出し側が利用できるバイト数
                ulong caller = 0;

                // ディスク全体のバイト数
                ulong total = 0;

                // ディスク全体の空きバイト数
                ulong freeSpace = 0;

                // SD Memoryの容量を取得
                ret = GetDiskFreeSpaceEx(directory, out caller, out total, out freeSpace);

                double currentPercent = Math.Round(((Math.Round((double)freeSpace, 2) / Math.Round((double)total, 2)) * 100), 2);

                if (!ret)
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }

                // 容量不足
                if (Math.Abs(checkPercent) > Math.Abs(currentPercent))
                {
                    // ログを書き込み
                    log.Warn(string.Format("$CheckMediaCapacity()$:  total={0}MB, freeSpace={1}MB , FreeAvailable={2}% ", ((total / 1024) / 1024), ((freeSpace / 1024) / 1024), Math.Round(((Math.Round((double)freeSpace, 2) / Math.Round((double)total, 2)) * 100), 2)));

                    return Const.FAILURE_OUTPUT_SD_CARD_NEAR_FULL;
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CheckMediaCapacity()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// 指定ディレクトリのファイル数取得メソッド
        /// </summary>
        /// <param name="directory">チェックディレクトリ</param>
        /// <param name="fileCount">ファイル数</param>
        public static int GetFileCount(string directory, out int fileCount)
        {
            fileCount = 0;
            if (!Directory.Exists(mediaPath))
            {
                return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
            }

            try
            {
                if (Directory.Exists(directory))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    fileCount = fileInfos.Length;
                }
                return Const.SUCCESS;
            }
            catch (Exception e)
            {
                // ログを書き込み
                log.Error(string.Format("$GetFileCount()$:  Exception - {0}.", e.ToString()));
                if (!Directory.Exists(mediaPath))
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// ファイル数のニアフルチェックメソッド
        /// </summary>
        /// <param name="notNearFullCheckCount">外部SDファイル数ニアフル（近い）チェックのカウント</param>
        /// <param name="nearFullCheckCount">外部SDファイル数ニアフルチェックのカウント</param>
        /// <param name="fullCheckCount">外部SDファイル数フルチェックのカウント</param>
        /// <param name="fileCount">ファイル数</param>
        public static int CheckFileCountState(int notNearFullCheckCount, int nearFullCheckCount, int fullCheckCount, int fileCount)
        {
            try
            {
                if (nearFullCheckCount <= fileCount && fileCount < fullCheckCount)
                {
                    if (!isFileCountNearFull)
                    {
                        // ログを書き込み
                        log.Warn(string.Format("$CheckFileCountState()$: Today Directory's File is Near Full,File Count={0}", fileCount));
                    }
                    isFileCountNearFull = true;
                    isFileCountFull = false;
                    isNotFileCountNearFull = false;
                    return Const.FAILURE_OUTPUT_SD_CARD_FILE_COUNT_NEAR_FULL;
                }
                else if (fileCount >= fullCheckCount)
                {
                    if (!isFileCountFull)
                    {
                        // ログを書き込み
                        log.Warn(string.Format("$CheckFileCountState()$: Today Directory's File is Full,File Count={0}", fileCount));
                    }
                    isFileCountFull = true;
                    isFileCountNearFull = false;
                    isNotFileCountNearFull = false;
                    return Const.FAILURE_OUTPUT_SD_CARD_FILE_COUNT_FULL;
                }
                else if (notNearFullCheckCount <= fileCount && fileCount < nearFullCheckCount)
                {
                    if (!isNotFileCountNearFull)
                    {
                        // ログを書き込み
                        log.Warn(string.Format("$CheckFileCountState()$: Today Directory's File is Not Near Full,File Count={0}", fileCount));
                    }
                    isNotFileCountNearFull = true;
                    isFileCountNearFull = false;
                    isFileCountFull = false;
                    return Const.SUCCESS_OUTPUT_SD_CARD_FILE_COUNT_NOT_NEAR_FULL;
                }

                return Const.SUCCESS;
            }
            catch (Exception e)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $CheckFileCountState()$:  DirectoryNotFoundException - {0}.", e.ToString()));
                isFileCountFull = false;
                isFileCountNearFull = false;
                isNotFileCountNearFull = false;
                if (!Directory.Exists(mediaPath))
                {
                    return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
                }
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// 作成バスとファイル内容より、新規ファイルを作成
        /// </summary>
        /// <param name="fileData">ファイルデータ</param>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>結果</returns>
        public static int Create(byte[] fileData, string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);

                    // ストリーム内ファイルの最後の位置を設定
                    binaryWriter.Seek(0, SeekOrigin.End);

                    // 記番号データを追加
                    binaryWriter.Write(fileData, 0, fileData.Length);

                    binaryWriter.Close();
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $Create()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }
    }
}
