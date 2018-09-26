using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataBaseServer
{
    public class DbMng
    {
        protected static SqlConnection cn;
        protected static SqlDataAdapter da;

        /// <summary>
        /// 日志
        /// </summary>
        private static ILog log = LogManager.GetLogger(typeof(DbMng));

        public static void update(Hashtable parrams, String fileKey)
        {
            List<String> list = new List<String>();
            list.Add(getSql(fileKey, parrams));
            update(list);
        }

        public static void update(List<String> sqlList)
        {
            //log.debug("Excecute a update SQL !");
            SqlTransaction sTrans;
            SqlCommand scom;
            cn = null;
            DbMng.getAccessConnection();
            cn.Open();

            scom = cn.CreateCommand();
            sTrans = cn.BeginTransaction("updateTransaction");

            //SqlCommand インスタンス化
            scom.Connection = cn;
            scom.Transaction = sTrans;

            String sql;
            int effected = 0;
            try
            {

                for (int i = 0; i < sqlList.Count; i++)
                {
                    sql = (String)sqlList[i];
                    scom.CommandText = sql;
                    effected += scom.ExecuteNonQuery();
                }
                sTrans.Commit();	

            }
            catch (Exception e)
            {
                try
                {
                    sTrans.Rollback();
                }
                catch (SqlException ex)
                {
                    log.Error("An exception of type " + ex.GetType() +
                            " was encountered while attempting to roll back the transaction.");
                }
                log.Error("An exception of type " + e.GetType() +
                        " was encountered while inserting the data.");
                log.Error("Neither record was written to database.");

            }
            finally
            {
                cn.Close();
            }

        }

        public static String getSql(String fileKey, Hashtable parrams)
        {
            String sql;
            sql = PropServ.getString(fileKey);
            sql = strUtils.getOptRepText(parrams, sql);
            return sql;
        }

        public static TableContainer exeQuarySQL(
            Hashtable parrams,
            String sqlKeyInfo)
        {

            TableContainer tc;
            String sql = PropServ.getString(sqlKeyInfo);

            sql = strUtils.getOptRepText(parrams, sql);

            tc = exeQuarySQL(sql);

            return tc;

        }

        public static TableContainer exeQuarySQL(String sql)
        {

            //log.debug("Excecute a queary SQL !");

            cn = null;
            da = null;
            DataSet rs = new DataSet();
            TableContainer tc = new TableContainer();

            try
            {

                DbMng.getAccessConnection();
                da = new SqlDataAdapter(sql, cn);
                cn.Open();
                int recordsAffected = da.Fill(rs);

                tc = new TableContainer(rs);
            }
            catch(Exception ex)
            {
                throw ex;

            }
            finally
            {
                if (cn != null)
                {
                    cn.Close();
                }
            }

            return tc;
        }

        public static void getAccessConnection()
        {
            cn = new SqlConnection(SNDBClient.GetInstance().SqlCon);
        }
    }
}
