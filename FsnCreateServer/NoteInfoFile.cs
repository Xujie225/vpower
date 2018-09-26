using log4net;
using System;
using System.Globalization;
using System.IO;

namespace FsnCreateServer
{
    public class NoteInfoFile
    {
        //ファイル名を取得のロック
        protected static object fileNameLock = new object();

        private static ILog log = LogManager.GetLogger(typeof(NoteInfoFile));

        /// <summary>
        /// ファイル名におけるファイル時刻位置を指す文字列
        /// </summary>
        public string FAIL_MAKING_DATE = "FAILMAIKINGDATE";

        /// <summary>
        /// ファイルパス名
        /// </summary>
        private string filePath = string.Empty;

        public string FilePath
        {
            get
            {
                return this.filePath;
            }
            protected set
            {
                this.filePath = value;
                this.fileName = Path.GetFileName(this.filePath);
                this.directoryPath = Path.GetDirectoryName(this.filePath);
            }
        }


        /// <summary>
        /// ファイル名
        /// </summary>
        private string fileName = string.Empty;
        public string FileName
        {
            get
            {
                return this.fileName;
            }
            protected set
            {
                this.fileName = value;
                if (!string.IsNullOrEmpty(this.fileName))
                {
                    this.filePath = Path.Combine(this.DirectoryPath, this.fileName);
                }
            }
        }

        /// <summary>
        /// 仮ファイル名
        /// </summary>
        private string provisionalFileName = string.Empty;
        public string ProvisionalFileName
        {
            get
            {
                return this.provisionalFileName;
            }
            protected set
            {
                this.provisionalFileName = value;
            }
        }

        /// <summary>
        /// ディレクトリパス名
        /// </summary>
        private string directoryPath = string.Empty;
        public string DirectoryPath
        {
            get
            {
                return this.directoryPath;
            }
            protected set
            {
                this.directoryPath = value;
                if (!string.IsNullOrEmpty(this.fileName))
                {
                    this.filePath = Path.Combine(this.directoryPath, this.fileName);
                }
            }
        }

        /// <summary>
        /// ファイル作成日時
        /// </summary>
        private string makingFileDateTime = string.Empty;
        public string MakingFileDateTime
        {
            get
            {
                return this.makingFileDateTime;
            }
            set
            {
                this.makingFileDateTime = value;
            }
        }

        /// <summary>
        /// ファイル名作成
        /// </summary>
        /// <param name="checkDirectoryPath">新ファイル名存在チェック対象フォルダ</param>
        /// <param name="prefixFileName">仮ファイル</param>
        /// <param name="extensionName">ファイル拡張子</param>
        /// <returns>新規ファイル名</returns>
        protected string GetFileName(string checkDirectoryPath, string provisionalFileName)
        {
            string[] checkDirectoryPaths = { checkDirectoryPath };
            return GetFileName(checkDirectoryPaths, provisionalFileName);
        }

        /// <summary>
        /// ファイル名作成
        /// </summary>
        /// <param name="checkDirectoryPath">新ファイル名存在チェック対象フォルダ配列</param>
        /// <param name="prefixFileName">仮ファイル</param>
        /// <returns>新規ファイル名</returns>
        protected string GetFileName(string[] checkDirectoryPath, string provisionalFileName)
        {
            try
            {
                foreach (string str in checkDirectoryPath)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        return string.Empty;
                    }
                }

                string newFileName = null;

                // システム時間
                DateTime systemTime = System.DateTime.Now;

                int cnt = 0;
                while (cnt < Int32.MaxValue)
                {
                    // ファイル作成日時
                    this.makingFileDateTime = systemTime.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
                    // ファイルのパス、ファイル日時を含めたファイル名
                    newFileName = provisionalFileName.Replace(this.FAIL_MAKING_DATE, makingFileDateTime);

                    if (this.IsOverlapFileName(checkDirectoryPath, newFileName))
                    {
                        //配列にすべてのフォルダをチェックして、新ファイル名がある場合、
                        //1秒をプラスして、ファイルを直した後、再チェック
                        systemTime = systemTime.AddSeconds(1);
                    }
                    else
                    {
                        break;
                    }
                    cnt++;

                }
                return newFileName;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("$GetFileName()$: Exception - {0}.", ex.ToString()));
                return string.Empty;
            }
        }

        /// <summary>
        /// ファイル重複チェック
        /// </summary>
        /// <param name="checkDirectoryPath">重複チェック対象フォルダ配列</param>
        /// <param name="checkFileName">重複チェック対象ファイル名</param>
        /// <returns>チェック結果</returns>
        protected bool IsOverlapFileName(string[] checkDirectoryPaths, string checkFileName)
        {
            try
            {
                //配列すべてのフォルダをチェックして、ファイル名の重複があるかをチェック
                foreach (string checkDirectory in checkDirectoryPaths)
                {
                    if ((FileOperate.CheckFileExist(Path.Combine(checkDirectory, checkFileName)) == 0) ||
                        (FileOperate.CheckFileExist(Path.Combine(checkDirectory, checkFileName + ".zip")) == 0))
                    {
                        // 重複ファイル有
                        return true;
                    }
                }
                // 重複ファイル無
                return false;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("$CheckOverlapFileName()$:  Exception - {0}.", ex.ToString()));
                return false;
            }
        }
    }
}
