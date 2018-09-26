using log4net;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FsnCreateServer
{
    public class DirectoryOperate
    {

        /// <summary>
        /// Win32API DLL名（Kernel32.dll）
        /// </summary>
        private const string KERNELDLL_NAME = "Kernel32.dll";

        private static ILog log = LogManager.GetLogger(typeof(DirectoryOperate));

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
        /// 存在しないフォルダを作成するメソッド
        /// </summary>
        /// <param name="dirPath">フォルダのパス</param>
        /// <returns>結果 0:正常、１:異常、2:SDカード未セット、4:SDカードアクセス異常</returns>
        public static int CreateNotExistDirectory(string dirPath)
        {
            try
            {
                if (string.IsNullOrEmpty(dirPath))
                {
                    return Const.FAILURE;
                }

                string[] dirPaths = dirPath.Split("\\".ToCharArray());
                string tmpDirPath = dirPaths[0];

                int ret = Const.FAILURE;

                for (int i = 1; i < dirPaths.Length; i++)
                {
                    tmpDirPath = Path.Combine(tmpDirPath, dirPaths[i]);

                    // フォルダ新規
                    ret = CreateDirectory(tmpDirPath);
                    if (ret != Const.SUCCESS)
                    {
                        return ret;
                    }
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CreateNotExistDirectory()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE;
            }
        }

        /// <summary>
        /// ファイル存在チェックメソッド
        /// </summary>
        /// <param name="dirPath">フォルダのパス</param>
        /// <returns>結果 0:存在する、１:存在しない、2:SDカード未セット、4:SDカードアクセス異常（発生しない）</returns>
        public static int CheckDirectoryExist(string dirPath)
        {
            try
            {

                if (!Directory.Exists(dirPath))
                {
                    return Const.FAILURE;
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $CheckDirectoryExist()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }

        /// <summary>
        /// フォルダを新規メソッド
        /// </summary>
        /// <param name="dirPath">フォルダのパス</param>
        /// <returns>結果 0:成功、１:異常、2:SDカード未セット、4:SDカードアクセス異常</returns>
        public static int CreateDirectory(string dirPath)
        {
            try
            {
                if (string.IsNullOrEmpty(dirPath))
                {
                    return Const.FAILURE;
                }                

                // フォルダがない場合
                if (CheckDirectoryExist(dirPath) == Const.FAILURE)
                {
                    // フォルダ新規
                    bool result = CreateDirectory(dirPath, null);
                    if (!result)
                    {
                        return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
                    }
                }

                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $CreateDirectory()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE;
            }
        }

        /// <summary>
        /// ファイル保存用日付フォルダをチェックして、空フォルダの場合削除
        /// </summary>
        /// <param name="dirPath">フォルダのパス</param>
        /// <param name="recursive">サブディレクトリ、およびファイルを削除する場合は true。それ以外の場合は false。</param>
        /// <returns>結果 0:削除成功、2:SDカード未セット、4:SDカードアクセス異常</returns>
        public static int DeleteDirectory(string dirPath, bool recursive)
        {
            if (!Directory.Exists(mediaPath))
            {
                return Const.FAILURE_OUTPUT_SD_CARD_UNSET;
            }
            try
            {
                if (recursive)
                {
                    if (Directory.Exists(dirPath))
                    {
                        Directory.Delete(dirPath, true);
                    }
                }
                else
                {
                    DirectoryInfo dirInfos = new DirectoryInfo(dirPath);
                    foreach (DirectoryInfo dirInfo in dirInfos.GetDirectories())
                    {
                        //フォルダにファイルのない場合
                        if (dirInfo.GetFiles().Length == 0)
                        {
                            Directory.Delete(dirInfo.FullName, true);
                        }
                    }
                    if (dirInfos.GetFiles().Length == 0 && dirInfos.GetDirectories().Length == 0)
                    {
                        Directory.Delete(dirPath, false);
                    }
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $DeleteDirectory()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE_OUTPUT_SD_CARD_ACCESS_FAILURE;
            }
        }
        
        /// <summary>
        ///  ディレクトリを作るメソッド
        /// </summary>
        [DllImport(KERNELDLL_NAME, EntryPoint = "CreateDirectory")]
        private static extern bool CreateDirectory(string lpPathName, string lpSecurityAttributes);
    }
}
