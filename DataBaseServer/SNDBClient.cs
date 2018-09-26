using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataBaseServer
{
    public class SNDBClient
    {
        private static SNDBClient me = null;

        private static ILog log = LogManager.GetLogger(typeof(SNDBClient));

        /// <summary>
        /// 接続文
        /// </summary>
        private string sqlCon;
        public string SqlCon
        {
            get
            {
                return this.sqlCon;
            }
            set
            {
                this.sqlCon = value;
            }
        }
        
        private int sequence;
        public int Sequence
        {
            get { return this.sequence; }
            set { this.sequence = value; }
        }
        
        //Tbl_SerialNumber
        DataTable serialTbl = new DataTable();
        //Tbl_Transactions
        DataTable transactionTbl = new DataTable();

        public static SNDBClient GetInstance()
        {
            if (null == me) { me = new SNDBClient(); }
            return me;
        }

        public SNDBClient()
        {
            //記番号テーブル
            serialTbl.Columns.Add("TraceNumber", typeof(string));
            serialTbl.Columns.Add("TranDate", typeof(int));
            serialTbl.Columns.Add("TranTime", typeof(int));
            serialTbl.Columns.Add("Sequence", typeof(int));
            serialTbl.Columns.Add("Country", typeof(string));
            serialTbl.Columns.Add("Media", typeof(string));
            serialTbl.Columns.Add("UnitValue", typeof(int));
            serialTbl.Columns.Add("Version", typeof(int));
            serialTbl.Columns.Add("SerialNo1", typeof(string));
            serialTbl.Columns.Add("SerialNo2", typeof(string));
            serialTbl.Columns.Add("CounterFeit", typeof(int));
            serialTbl.Columns.Add("OCRRate1", typeof(int));
            serialTbl.Columns.Add("OCRRate2", typeof(int));
            serialTbl.Columns.Add("BVResult", typeof(int));
            serialTbl.Columns.Add("ErrorCode", typeof(int));
            serialTbl.Columns.Add("BundleDate", typeof(int));
            serialTbl.Columns.Add("BundleTime", typeof(int));
            serialTbl.Columns.Add("UpdateUser", typeof(int));
            serialTbl.Columns.Add("OperatorID", typeof(int));
            serialTbl.Columns.Add("Updatedatetime", typeof(string));

            //取引テーブル
            transactionTbl.Columns.Add("TraceNumber", typeof(string));
            transactionTbl.Columns.Add("TranDate", typeof(int));
            transactionTbl.Columns.Add("TranTime", typeof(int));
            transactionTbl.Columns.Add("ClientNumber", typeof(int));
            transactionTbl.Columns.Add("ClientName", typeof(string));
            transactionTbl.Columns.Add("TerminalNumber", typeof(int));
            transactionTbl.Columns.Add("MachineNumber", typeof(string));
            transactionTbl.Columns.Add("Piece", typeof(int));
            transactionTbl.Columns.Add("Amount", typeof(int));
            transactionTbl.Columns.Add("DeclareAmount", typeof(int));
            transactionTbl.Columns.Add("Result", typeof(int));
            transactionTbl.Columns.Add("ErrorCode", typeof(string));
            transactionTbl.Columns.Add("ErrorDetail", typeof(string));
            transactionTbl.Columns.Add("UpdateDateTime", typeof(string));
        }

        public bool BulkInsert(List<TransactionsTbl> tranTbl,List<SerialNumberTbl> snoTbl)
        {
            try
            {
                // DB接続
                SqlConnection sc = new SqlConnection(this.sqlCon);

                int seqTmp = DBCreater.GetInstance().sequenceList[0];
                DBCreater.GetInstance().sequenceList.RemoveAt(0);

                foreach (SerialNumberTbl snoTmpTbl in snoTbl)
                {
                    DataRow r = serialTbl.NewRow();

                    r["TraceNumber"] = snoTmpTbl.TraceNumber.Replace("NOSEQUENCE",seqTmp.ToString().PadLeft(5, '0'));
                    r["TranDate"] = snoTmpTbl.TranDate;
                    r["TranTime"] = snoTmpTbl.TranTime;
                    r["Sequence"] = snoTmpTbl.Sequence;
                    r["Country"] = snoTmpTbl.Country;
                    r["Media"] = snoTmpTbl.Media;
                    r["UnitValue"] = snoTmpTbl.UnitValue;
                    r["Version"] = snoTmpTbl.Version;
                    r["SerialNo1"] = snoTmpTbl.SerialNo1;
                    r["SerialNo2"] = snoTmpTbl.SerialNo2;
                    r["CounterFeit"] = snoTmpTbl.CounterFeit;
                    r["OCRRate1"] = snoTmpTbl.OCRRate1;
                    r["OCRRate2"] = snoTmpTbl.OCRRate2;
                    r["BVResult"] = snoTmpTbl.BVResult;
                    r["ErrorCode"] = snoTmpTbl.ErrorCode;
                    r["BundleDate"] = snoTmpTbl.BundleDate;
                    r["BundleTime"] = snoTmpTbl.BundleTime;
                    r["UpdateUser"] = snoTmpTbl.UpdateUser;
                    r["OperatorID"] = snoTmpTbl.OperatorID;
                    r["Updatedatetime"] = snoTmpTbl.Updatedatetime;

                    serialTbl.Rows.Add(r);
                }

                foreach (TransactionsTbl tranTmpTbl in tranTbl)
                {
                    DataRow r = transactionTbl.NewRow();

                    r["TraceNumber"] = tranTmpTbl.TraceNumber.Replace("NOSEQUENCE", seqTmp.ToString().PadLeft(5, '0'));
                    r["TranDate"] = tranTmpTbl.TranDate;
                    r["TranTime"] = tranTmpTbl.TranTime;
                    r["ClientNumber"] = tranTmpTbl.ClientNumber;
                    r["ClientName"] = tranTmpTbl.ClientName;
                    r["TerminalNumber"] = tranTmpTbl.TerminalNumber;
                    r["MachineNumber"] = tranTmpTbl.MachineNumber;
                    r["Piece"] = tranTmpTbl.Piece;
                    r["Amount"] = tranTmpTbl.Amount;
                    r["DeclareAmount"] = tranTmpTbl.DeclareAmount;
                    r["Result"] = tranTmpTbl.Result;
                    r["ErrorCode"] = tranTmpTbl.ErrorCode;
                    r["ErrorDetail"] = tranTmpTbl.ErrorDetail;
                    r["UpdateDateTime"] = tranTmpTbl.UpdateDateTime;

                    transactionTbl.Rows.Add(r);
                }

                //DB接続
                sc.Open();

                log.Info(@"TBL_SERIALNUMBER input data start...");
                log.Info(@"Sno count ：" + snoTbl.Count);
                
                using(SqlBulkCopy bulk = new SqlBulkCopy(this.sqlCon))
                {
                    bulk.BatchSize = 2000;
                    bulk.DestinationTableName = "dbo.Tbl_SerialNumber_bak";
                    bulk.ColumnMappings.Add("TraceNumber","TraceNumber");
                    bulk.ColumnMappings.Add("TranDate","TranDate");
                    bulk.ColumnMappings.Add("TranTime","TranTime");
                    bulk.ColumnMappings.Add("Sequence","Sequence");
                    bulk.ColumnMappings.Add("Country","Country");
                    bulk.ColumnMappings.Add("Media","Media");
                    bulk.ColumnMappings.Add("UnitValue","UnitValue");
                    bulk.ColumnMappings.Add("Version","Version");
                    bulk.ColumnMappings.Add("SerialNo1","SerialNo1");
                    bulk.ColumnMappings.Add("SerialNo2","SerialNo2");
                    bulk.ColumnMappings.Add("CounterFeit","CounterFeit");
                    bulk.ColumnMappings.Add("OCRRate1","OCRRate1");
                    bulk.ColumnMappings.Add("OCRRate2","OCRRate2");
                    bulk.ColumnMappings.Add("BVResult","BVResult");
                    bulk.ColumnMappings.Add("ErrorCode","ErrorCode");
                    bulk.ColumnMappings.Add("BundleDate","BundleDate");
                    bulk.ColumnMappings.Add("BundleTime","BundleTime");
                    bulk.ColumnMappings.Add("UpdateUser","UpdateUser");
                    bulk.ColumnMappings.Add("OperatorID","OperatorID");
                    bulk.ColumnMappings.Add("Updatedatetime","Updatedatetime");
                    bulk.WriteToServer(serialTbl.Copy());
                }
                log.Info(@"TBL_SERIALNUMBER input complete!!");

                log.Info(@"TBL_TRANSACTIONS input start...");
                log.Info(@"Tran count：" + tranTbl.Count);
                using (SqlBulkCopy bulk = new SqlBulkCopy(this.sqlCon))
                {
                    bulk.BatchSize = 2000;
                    bulk.DestinationTableName = "dbo.Tbl_Transactions_bak";
                    bulk.ColumnMappings.Add("TraceNumber", "TraceNumber");
                    bulk.ColumnMappings.Add("TranDate", "TranDate");
                    bulk.ColumnMappings.Add("TranTime", "TranTime");
                    bulk.ColumnMappings.Add("ClientNumber", "ClientNumber");
                    bulk.ColumnMappings.Add("ClientName", "ClientName");
                    bulk.ColumnMappings.Add("TerminalNumber", "TerminalNumber");
                    bulk.ColumnMappings.Add("MachineNumber", "MachineNumber");
                    bulk.ColumnMappings.Add("Piece", "Piece");
                    bulk.ColumnMappings.Add("Amount", "Amount");
                    bulk.ColumnMappings.Add("DeclareAmount", "DeclareAmount");
                    bulk.ColumnMappings.Add("Result", "Result");
                    bulk.ColumnMappings.Add("ErrorCode", "ErrorCode");
                    bulk.ColumnMappings.Add("ErrorDetail", "ErrorDetail");
                    bulk.ColumnMappings.Add("UpdateDateTime", "UpdateDateTime");
                    bulk.WriteToServer(transactionTbl.Copy());
                }
                log.Info(@"TBL_TRANSACTIONS input complete!!");

                sc.Close();
                sc.Dispose();
                
                return true;
            }
            catch(Exception ex)
            {
                log.Error(@"sql date input error ; ", ex);
                return false;
            }
            finally
            {
                serialTbl.Rows.Clear();
                transactionTbl.Rows.Clear();
            }
        }

        
    }
}
