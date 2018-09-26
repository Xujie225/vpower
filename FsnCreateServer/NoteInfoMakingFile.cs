using log4net;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace FsnCreateServer
{
    /// <summary>
    /// 作成中ファイルクラス
    /// </summary>
    public class NoteInfoMakingFile : NoteInfoFile
    {
        private static ILog log = LogManager.GetLogger(typeof(NoteInfoMakingFile));

        /// <summary>
        /// 紙幣枚数
        /// </summary>
        private int noteCount;
        public int NoteCount
        {
            get
            {
                return this.noteCount;
            }
        }

        /// <summary>
        /// 記番号ファイルボディのサイズ
        /// </summary>
        protected readonly int noteSize;

        /// <summary>
        /// ファイル名管理クラス
        /// </summary>
        FileNameManage fileNameManage;

        /// <summary>
        /// 記番号ファイル記番号データの長さを取得メソッド
        /// </summary>
        /// <param name="noteCount">記番号の数</param>
        /// <returns>記番号データの長さ</returns>
        //public int GetNoteDataSize(int noteCount)
        //{
        //    return noteCount * this.noteSize;
        //}

        public NoteInfoMakingFile(string unuploadDirectory, int noteSize)
        {
            this.DirectoryPath = unuploadDirectory;
            this.noteSize = noteSize;
            this.fileNameManage = new FileNameManage();
        }

        /// <summary>
        /// 記番号ファイル作成と追加メソッド
        /// </summary>
        /// <param name="noteData">記番号データ</param>
        /// <param name="addCount">作成中ファイルの記番号の数</param>
        /// <returns>結果</returns>
        public int Add(byte[] noteData)
        {
            int ret = Const.FAILURE;

            try
            {
                // 新規作成
                ret = this.Create(noteData);
                if (ret != Const.SUCCESS)
                {
                    // ログを書き込み
                    log.Error("$Add()$: Create file failure.");
                    return ret;
                }
                return Const.SUCCESS;

            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$Add()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE;
            }
        }

        private int Create(byte[] noteData)
        {
            // ヌルチェック
            if (noteData == null || noteData.Length == 0)
            {
                log.Error(string.Format("<Err> at $Create()$: Note Data NULL"));
                return Const.FAILURE;
            }
            int ret = Const.FAILURE;
            // 作成中フォルダ存在チェック
            ret = DirectoryOperate.CreateNotExistDirectory(this.DirectoryPath);
            if (ret != Const.SUCCESS)
            {
                log.Error(string.Format("$Create()$: Failure Create Directory"));
                return ret;
            }

            try
            {
                // ファイルサイズを取得
                //int dataSize = this.GetNoteDataSize(noteCount);
                int dataSize = noteSize;
                
                // ファイルのバイナリ配列を宣言
                byte[] fileData = new byte[dataSize];
                // データコピー
                Array.Copy(noteData, 0, fileData, 0, fileData.Length);     // データ部

                // ファイル名フォーマットを取得
                string provisionalFileName;
                ret = this.fileNameManage.GetProvisionalFileName(out provisionalFileName);
                if (ret != Const.SUCCESS || provisionalFileName == null || provisionalFileName.Length == 0)
                {
                    log.Error(string.Format("$Create()$: Failure Create File Name"));
                    return ret;
                }

                lock (fileNameLock)
                {
                    //ファイル名を新規
                    string fileName = this.GetFileName(this.DirectoryPath, provisionalFileName);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return Const.FAILURE;
                    }

                    //新規作成したファイル名でファイルを作成
                    this.ProvisionalFileName = provisionalFileName;
                    this.FileName = fileName;
                    ret = FileOperate.Create(fileData, this.FilePath);
                    log.Info(string.Format("$Create()$:  Create File Name is {0}.", this.FileName));
                }
                return ret;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$Create()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE;
            }
        }       
    }
}
