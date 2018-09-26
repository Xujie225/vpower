using log4net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace CommandServer
{
    public class TcpSender
    {
        /// <summary>
        /// 日志
        /// </summary>
        private static ILog log = LogManager.GetLogger(typeof(ClientManager));

        /// <summary>
        /// 送り先のソケット
        /// </summary>
        private Socket socket;
        
        NetworkStream networkStream;

        private static TcpSender  me = null;

        private int tryTimes = 1;
        public int TryTimes
        {
            set { this.tryTimes = value;}
        }

        private int fsncount = 0;
        public int FsnCount
        {
            get { return this.fsncount; }
            set { this.fsncount = value; }
        }

        private byte[] fsnheader;
        public byte[] Fsnheader
        {
            get { return this.fsnheader; }
            set { this.fsnheader = value; }
        }

        private byte[] sendMessage;
        public byte[] SendMessage
        {
            get { return this.sendMessage; }
            set { this.sendMessage = value; }
        }

        public enum ReturnValue
        {
            Success,
            Failure,
            Exception,
        }

        #region Constructor
        public TcpSender()
        {
            this.sendMessage = null;

            this.fsnheader = null;

        }

        /// <summary>
        /// インスタンス_ゲット
        /// </summary>
        /// <returns></returns>
        public static TcpSender getInstance()
        {
            if (me == null)
            {
                me = new TcpSender();
            }

            return me;
        }
        #endregion

        #region private method

        private ReturnValue Connect(IPEndPoint ip)
        {
            //log.Info("Connect to CSPBC Server...");
            int SleepTime = 2;

            try
            {
                // Socket 初期化
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                this.socket.SendTimeout = 5 * 1000;

                this.socket.ReceiveTimeout = 5 * 1000;

                for (int i = 0; i < this.tryTimes;i++ )
                {
                    this.socket.Connect(ip.Address, ip.Port);

                    if (this.socket.Connected)
                    {
                        log.Info("CSPBC Server...Connected!");
                        break;
                    }

                    Thread.Sleep((SleepTime + i) * 1000);
                }

                if (!this.socket.Connected)
                {
                    log.Error("<Err> Connect to CSPBC Server...Fail");
                    return ReturnValue.Failure;
                }

                this.networkStream = new NetworkStream(this.socket);
                return ReturnValue.Success;
            }
            catch (SocketException e)
            {
                log.Error("<Err> Connect to CSPBC Server... " + e.Message);
                return ReturnValue.Exception;
            }
            catch (Exception ex)
            {
                log.Error("<Err> Connect to CSPBC Server... " + ex.Message);
                return ReturnValue.Exception;
            }

        }

        System.Threading.Semaphore semaphor = new System.Threading.Semaphore(1, 1);
        private ReturnValue SendData(byte[] data)
        {
            try
            {
                semaphor.WaitOne();

                this.networkStream.Write(data, 0, data.Length);

                this.networkStream.Flush();

                return ReturnValue.Success;
            }
            catch (Exception e)
            {
                log.Error("<Err> CSPBC Send ：" + e.Message);

                return ReturnValue.Exception;
            }
            finally
            {
                semaphor.Release();
            }
        }

        private ReturnValue ReceiveData(object cmd)
        {
            try
            {
                if (!this.socket.Connected)
                {
                    return ReturnValue.Failure;
                }

                byte[] buf = new byte[4];                

                this.networkStream.Read(buf, 0, buf.Length);

                if (this.CheckStartWordisVaild(buf) == ReturnValue.Failure)
                {
                    return ReturnValue.Failure;
                }
                else
                {
                    Array.Clear(buf, 0, buf.Length);
                }

                // メッセージ長さを取得
                this.networkStream.Read(buf, 0, buf.Length);
                int size = BitConverter.ToInt32(buf, 0) - 8;

                // メッセージを取得
                buf = new byte[size];
                this.networkStream.Read(buf, 0, buf.Length);

                switch ((Message.CSPBCCmd)cmd)
                {
                    case Message.CSPBCCmd.CSPBC_SIMPLE:
                    case Message.CSPBCCmd.CSPBC_UP:
                        #region 送信要求コマンドのレスポンス
                        Message.CSPBCMessage.CSPBC_RTN cspbc_rtn = CommonUtil.ByteToStructure<Message.CSPBCMessage.CSPBC_RTN>(buf);

                        if (BitConverter.ToUInt16(cspbc_rtn.retCode, 0) != 0x00F0)
                        {
                            // 送信失敗
                            return ReturnValue.Failure;
                        }
                        #endregion
                        break;
                    case Message.CSPBCCmd.CSPBC_CLOSE:
                        break;
                }

                return ReturnValue.Success;
            }
            catch (SocketException e)
            {
                log.Error("<Err> Exception with socket occured. " + e.Message);
                return ReturnValue.Exception;
            }
            catch (Exception ex)
            {
                log.Error("<Err> Exception occured. " + ex.Message);
                return ReturnValue.Exception;
            }
        }

        private ReturnValue CheckStartWordisVaild(byte[] currentStartWord)
        {
            byte[] startWord = BitConverter.GetBytes(Message.CSPBCMessage.StartWord);
            for (int i = 0; i < startWord.Length; i++)
            {
                if (!startWord[i].Equals(currentStartWord[i]))
                {
                    return ReturnValue.Failure;
                }
            }
            return ReturnValue.Success;
        }

        #endregion

        #region public method

        /// <summary>
        /// 湖南人民銀行アップロード処理
        /// </summary>
        public ReturnValue MainUpload(IPEndPoint ip, string machineSNo, string bussinessType, 
                                      string branchCode, string operId,byte[] noteInfo, int noteCount)
        {
            try
            {
                if(this.Connect(ip) != ReturnValue.Success)
                {
                    log.Info("<Info> connect fail!");
                    return ReturnValue.Failure;
                }

                // 送信メッセージ
                byte[] sendMessage = null;
                // TCPコマンド
                object cmd = null;

                #region Step1:送信要求
                //送信要求の初期化
                Message.CSPBCMessage.CSPBC_SIMPLE cspbc_simple = new Message.CSPBCMessage.CSPBC_SIMPLE(machineSNo.Substring(machineSNo.Length - 6), branchCode);

                sendMessage = CommonUtil.StructureToBytes(cspbc_simple);

                if (this.SendData(sendMessage) != ReturnValue.Success)
                {
                    return ReturnValue.Failure;
                }
                cmd = Message.CSPBCCmd.CSPBC_SIMPLE;
                if (this.ReceiveData(cmd) != ReturnValue.Success)
                {
                    return ReturnValue.Failure;
                }
                #endregion

                #region Step2:データ送信
                // 操作員ID
                byte[] operatorId;
                if (!string.IsNullOrEmpty(operId))
                {
                    operatorId = ASCIIEncoding.ASCII.GetBytes(operId);
                }
                else
                {
                    operatorId = ASCIIEncoding.ASCII.GetBytes("0");
                }

                Message.CSPBCMessage.CSPBC_UP cspbc_up = new Message.CSPBCMessage.CSPBC_UP(machineSNo.Substring(machineSNo.Length - 6), bussinessType, noteCount,
                                                                                           operatorId, noteInfo.Length + 84 + 2);
                byte[] sendMessageTmp = CommonUtil.StructureToBytes(cspbc_up);
                sendMessage = new byte[sendMessageTmp.Length + noteInfo.Length + 2];

                // メッセージヘッダー
                Array.Copy(sendMessageTmp, 0, sendMessage, 0, sendMessageTmp.Length);
                // ファイルボディ
                Array.Copy(noteInfo, 0, sendMessage, sendMessageTmp.Length, noteInfo.Length);
                // checksum
                Array.Copy(new byte[] { 0x00, 0x00 }, 0, sendMessage, sendMessageTmp.Length + noteInfo.Length, 2);

                if(this.SendData(sendMessage)!= ReturnValue.Success)
                {
                    return ReturnValue.Failure;
                }
                cmd = Message.CSPBCCmd.CSPBC_UP;
                if (this.ReceiveData(cmd) != ReturnValue.Success)
                {
                    return ReturnValue.Failure;
                }
                #endregion

                #region Step3:データ送信完了
                //送信クローズ
                Message.CSPBCMessage.CSPBC_CLOSE cspbc_close = new Message.CSPBCMessage.CSPBC_CLOSE(machineSNo.Substring(machineSNo.Length - 6));
                sendMessage = CommonUtil.StructureToBytes(cspbc_close);

                if (this.SendData(sendMessage) != ReturnValue.Success)
                {
                    return ReturnValue.Failure;
                }
                else
                {
                    log.Info("CSPBC UPLOAD SUCCESS!!");
                }
                #endregion

                return ReturnValue.Success;
            }
            catch(Exception ex)
            {
                log.Error("<Err> CSPBC->MainUpload()$" + ex.Message);
                return ReturnValue.Exception;
            }
            finally
            {
                //接続切断
                this.socket.Close();
            }
        }
        #endregion
    }
}