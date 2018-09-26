using log4net;
using System;
using System.Text;

namespace FsnCreateServer
{
    public class FileNameManage
    {
        private static ILog log = LogManager.GetLogger(typeof(FileNameManage));

        /// <summary>
        /// 紙幣種類
        /// </summary>
        protected const string ChinaCurrency = "CNY";

        /// <summary>
        /// 会社略称
        /// </summary>
        public const string CompanyName = "GLY";
        public const string CompanyName4Byte = "GLRY";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FileNameManage()
        {
        }

        /// <summary>
        /// 日時を除く仮ファイル名を作る
        /// </summary>
        /// <param name="transactionInfo">取引情報</param>
        /// <param name="machineInfo">装置情報</param>
        /// <returns>紙幣情報ファイルの仮ファイル名</returns>
        protected string CreateProvisionalFileName()
        {
            StringBuilder fileName = new StringBuilder();

            // 幣種
            fileName.Append(CompanyName4Byte).Append("_");

            // 業務タイプ
            fileName.Append(Const.DEFAULT_SERVICE_TYPE_GRG).Append("_");

            fileName.Append(Const.FAIL_MAKING_DATE);

            // 拡張子
            fileName.Append(Const.EXTENSION_FSN);

            return fileName.ToString();

        }

        public int GetProvisionalFileName(out string provisionalFileName)
        {
            provisionalFileName = string.Empty;
            try
            {
                //仮ファイル名
                provisionalFileName = this.CreateProvisionalFileName();
                if (string.IsNullOrEmpty(provisionalFileName))
                {
                    log.Error(string.Format("$CreateFile()$:  Provisional File Name Create Error"));
                    return Const.FAILURE;
                }
                return Const.SUCCESS;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("$CreateFile()$:  Exception - {0}.", ex.ToString()));
                return Const.FAILURE;
            }
        }
    }
}
