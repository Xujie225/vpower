using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FsnCreateServer
{
    public class FsnCreater
    {
        /// <summary>
        /// ファイル作成用スレッド
        /// </summary>
        public Thread fileMakingProcessThread = null;

        /// <summary>
        /// 作成待ち処理用リスト
        /// </summary>
        public List<byte[]> makingFileList;


        public FsnCreater()
        {
            this.makingFileList = new List<byte[]>();

            if(this.fileMakingProcessThread == null)
            {
                this.fileMakingProcessThread = new Thread(MainMakingProcess);
                this.fileMakingProcessThread.Name = "MAKING";
                this.fileMakingProcessThread.IsBackground = false;
                this.fileMakingProcessThread.Start();
            }
        }

        ~FsnCreater()
        {
        }

        private void MainMakingProcess()
        {
            int cnt;

            while(true)
            {
                lock(this.makingFileList)
                {
                    cnt = this.makingFileList.Count;
                }

                if(cnt > 0)
                {
                    byte[] makingFileStructTmp;

                    lock(this.makingFileList)
                    {
                        makingFileStructTmp = this.makingFileList[0];
                    }

                    string mkDir = Path.Combine(".\\Fsn", formatDateBySpecified("yyyy-MM-dd", DateTime.Now));

                    NoteInfoMakingFile makingFileObject = new NoteInfoMakingFile(mkDir, makingFileStructTmp.Length);

                    if(makingFileObject.Add(makingFileStructTmp) == Const.SUCCESS)
                    {
                        lock (this.makingFileList)
                        {
                            this.makingFileList.Remove(makingFileStructTmp);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public string formatDateBySpecified(string dateFormat, DateTime dt)
        {
            // 空指定の場合、デフォルト値をセット
            if (string.IsNullOrEmpty(dateFormat))
            {
                dateFormat = "yyyyMMddHHmmss";
            }

            string ret = string.Empty;
            try
            {
                string stringFormat = string.Format("{0}0:{1}{2}", "{", dateFormat, "}");
                ret = String.Format(stringFormat, dt);
            }
            catch
            {
            }

            return ret;
        }
    }
}
