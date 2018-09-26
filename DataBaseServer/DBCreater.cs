using System.Collections.Generic;
using System.Threading;

namespace DataBaseServer
{
    public class DBCreater
    {
        private static DBCreater me = null;

        public List<int> sequenceList; 

        /// <summary>
        /// DB作成用スレッド
        /// </summary>
        public Thread dbMakingProcessThread = null;

        public static DBCreater GetInstance()
        {
            if (null == me) { me = new DBCreater(); }
            return me;
        }

        public struct DbCreaterWk
        {
            public List<TransactionsTbl> tblTwk;
            public List<SerialNumberTbl> tblSwk;

            public DbCreaterWk (List<TransactionsTbl> tblTwk,List<SerialNumberTbl> tblSwk)
            {
                this.tblTwk = tblTwk;
                this.tblSwk = tblSwk;
            }
        }

        /// <summary>
        /// 作成待ち処理用リスト
        /// </summary>
        public List<DbCreaterWk> makingTblList;

        public DBCreater()
        {
            this.makingTblList = new List<DbCreaterWk>();
            this.sequenceList = new List<int>();

            if (this.dbMakingProcessThread == null)
            {
                this.dbMakingProcessThread = new Thread(MainMakingProcess);
                this.dbMakingProcessThread.Name = "DBMAKING";
                this.dbMakingProcessThread.IsBackground = false;
                this.dbMakingProcessThread.Start();
            }
        }

        ~DBCreater()
        {
        }
        
        private void MainMakingProcess()
        {
            int cnt;
                        
            while (true)
            {
                lock (this.makingTblList)
                {
                    cnt = this.makingTblList.Count;
                }

                if (cnt > 0)
                {
                    DbCreaterWk makingtTblStructTmp;

                    lock (this.makingTblList)
                    {
                        makingtTblStructTmp = this.makingTblList[0];
                    }

                    //データベース更新
                    SNDBClient.GetInstance().BulkInsert(makingtTblStructTmp.tblTwk, makingtTblStructTmp.tblSwk);
                    {
                        lock (this.makingTblList)
                        {
                            this.makingTblList.Remove(makingtTblStructTmp);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
        }
    }
}
