using DataBaseServer;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
namespace CommandServer
{    
    public class ClientManager
    {

        private Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// 日志
        /// </summary>
        private static ILog log = LogManager.GetLogger(typeof(ClientManager));

        /// Gets the IP address of connected remote client.This is 'IPAddress.None' if the client is not connected.
        public IPAddress IP
        {
            get
            {
                if (this.socket != null)
                    return ((IPEndPoint)this.socket.RemoteEndPoint).Address;
                else
                    return IPAddress.None;
            }
        }

        /// Gets the port number of connected remote client.This is -1 if the client is not connected.
        public int Port
        {
            get
            {
                if (this.socket != null)
                    return ((IPEndPoint)this.socket.RemoteEndPoint).Port;
                else
                    return -1;
            }
        }

        public bool Connected
        {
            get
            {
                if (this.socket != null)
                    return this.socket.Connected;
                else
                    return false;
            }
        }

        private IPEndPoint serverIp;
        private int tryTimes;
        private Socket socket;

        NetworkStream networkStream;
        private BackgroundWorker bwReceiver;

        #region Constructor

        public ClientManager(Socket clientSocket)
        {
            this.serverIp = null;
            this.tryTimes = 0;
            this.socket = clientSocket;
            this.networkStream = new NetworkStream(this.socket);
            this.networkStream.ReadTimeout = 5000;
            this.networkStream.WriteTimeout = 5000;
            this.bwReceiver = new BackgroundWorker();
            this.bwReceiver.DoWork += StartReceive;
            this.bwReceiver.RunWorkerAsync();
        }

        public ClientManager(Socket clientSocket,IPEndPoint cspbcIp,int tryTimes)
        {
            this.serverIp = cspbcIp;
            this.tryTimes = tryTimes;
            this.socket = clientSocket;
            this.networkStream = new NetworkStream(this.socket);
            this.networkStream.ReadTimeout = 5000;
            this.networkStream.WriteTimeout = 5000;
            this.bwReceiver= new BackgroundWorker();
            this.bwReceiver.DoWork += StartReceive;
            this.bwReceiver.RunWorkerAsync();
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// 記番号リシーバ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartReceive(object sender, DoWorkEventArgs e)
        {
            byte[] revBuffHeader; //通信ヘッダ部
            byte[] revBuffBody;   //通信ボディ部
            try
            {
                while (this.socket.Connected)
                {
                    if (!this.stopwatch.IsRunning)
                    {
                        this.stopwatch.Start();
                    }
                    log.Info("Start Receive...");
                    //Read the Header Content.(Start_Word)
                    byte[] buffer = new byte[4];
                    int readBytes = this.networkStream.Read(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        break;
                    }
                    else if (!this.CheckStartWordisVaild(buffer))
                    {
                        log.Error("<Err> at $StartReceive()$ Net Info Header is indivual.");
                        break;
                    }
                    //コマンドヘッダ部
                    revBuffHeader = new byte[88];
                    Array.Copy(buffer, 0, revBuffHeader, 0, buffer.Length);

                    buffer = new byte[6];
                    readBytes = this.networkStream.Read(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        break;
                    }
                    Array.Copy(buffer, 0, revBuffHeader, 4, buffer.Length);

                    //コマンド
                    buffer = new byte[2];
                    readBytes = this.networkStream.Read(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        break;
                    }
                    else if (BitConverter.ToUInt16(buffer, 0) == Message.VPowerMessage.Up_ST)
                    {
                        revBuffHeader = new byte[26];
                        Array.Copy(buffer, 0, revBuffHeader, 0, buffer.Length);

                        readBytes = this.networkStream.Read(revBuffHeader, buffer.Length, revBuffHeader.Length - buffer.Length);

                        Message.VPowerMessage.V_START v_s = CommonUtil.ByteToStructure<Message.VPowerMessage.V_START>(revBuffHeader);

                        log.Info(string.Format("UWF4 {0} Transaction Start !", System.Text.ASCIIEncoding.Default.GetString(v_s.machineNo, 0, 12).Replace("\0", "")));

                        TcpSender.getInstance().SendMessage = null;
                        TcpSender.getInstance().FsnCount = 0;

                        TcpSender.getInstance().Fsnheader = null;
                        //break;
                    }
                    else
                    {
                        Array.Copy(buffer, 0, revBuffHeader, 10, buffer.Length);

                        readBytes = this.networkStream.Read(revBuffHeader, buffer.Length + 10, revBuffHeader.Length - buffer.Length - 10);
                        if (readBytes == 0)
                        {
                            log.Error("<Err> at $StartReceive()$ Net Info Front half is indivual.");
                            break;
                        }
                        //ヘッダ部の構造化
                        Message.VPowerMessage.V_UP v_up = CommonUtil.ByteToStructure<Message.VPowerMessage.V_UP>(revBuffHeader);
                        if (BitConverter.ToUInt16(v_up.cmd, 0) != Message.VPowerMessage.Up_HM
                            && BitConverter.ToUInt16(v_up.cmd, 0) != Message.VPowerMessage.Up_KA
                            && BitConverter.ToUInt16(v_up.cmd, 0) != Message.VPowerMessage.Up_KH
                            && BitConverter.ToUInt16(v_up.cmd, 0) != Message.VPowerMessage.Up_KA_S
                            && BitConverter.ToUInt16(v_up.cmd, 0) != Message.VPowerMessage.Up_KH_S)
                        {
                            log.Error(string.Format("<Err> at $StartReceive()$ Net Info Cmd is {0}.",BitConverter.ToUInt16(v_up.cmd, 0).ToString()));
                            log.Error("<Err> at $StartReceive()$ Net Info Cmd Receive incorrect.");
                            break;
                        }
                        else if (BitConverter.ToUInt16(v_up.cmd, 0) == Message.VPowerMessage.Up_HM
                            || BitConverter.ToUInt16(v_up.cmd, 0) == Message.VPowerMessage.Up_KA_S
                            || BitConverter.ToUInt16(v_up.cmd, 0) == Message.VPowerMessage.Up_KH_S)
                        {
                            TcpSender.getInstance().SendMessage = null;
                            TcpSender.getInstance().FsnCount = 0;
                            TcpSender.getInstance().Fsnheader = null;
                        }

                        //コマンドﾎﾞﾃﾞｲ部(FSNヘッダ部＋ﾎﾞﾃﾞｲ部)
                        revBuffBody = new byte[BitConverter.ToInt32(v_up.valid_num, 0)];

                        int offset = 0;
                        while (offset < revBuffBody.Length)
                        {
                            if (offset == 0)
                            {
                                readBytes = this.networkStream.Read(revBuffBody, 0, revBuffBody.Length);
                            }
                            else
                            {
                                readBytes = this.networkStream.Read(revBuffBody, offset, revBuffBody.Length - offset);
                            }

                            offset += readBytes;
                        }
                        if (offset != revBuffBody.Length)
                        {
                            log.Error("<Err> at $StartReceive()$ Net Info Behind half is indivual..");
                            break;
                        }

                        //SUM検証
                        buffer = new byte[2];
                        readBytes = this.networkStream.Read(buffer, 0, buffer.Length);
                        UInt16 csum = (UInt16)(0 - this.checksum(revBuffBody, revBuffBody.Length));
                        if (readBytes != buffer.Length || csum != BitConverter.ToUInt16(buffer, 0))
                        {
                            log.Error("<Err> at $StartReceive()$ check sum is incorrect.");
                            break;
                        }
                        //最後エンドワード
                        buffer = new byte[4];
                        readBytes = this.networkStream.Read(buffer, 0, buffer.Length);
                        if (readBytes != buffer.Length || 0x5555AAAA != BitConverter.ToUInt32(buffer, 0))
                        {
                            log.Error("<Err> at $StartReceive()$ end work is incorrect.");
                            break;
                        }

                        Command cmd = new Command(Message.VpowerCmd.V_UP, this.IP, revBuffBody, revBuffHeader);
                        this.OnCommandReceived(new CommandEventArgs(cmd)); // ローカル処理
                    }

                    Command cmdRev = new Command(Message.VpowerCmd.V_UP,this.IP,null);
                    this.SendVpowerCommandToClient(cmdRev);
                }                
                this.OnDisconnected(new ClientEventArgs(this.socket));
                this.Disconnect();
            }
            catch (SocketException es)
            {
                if (es.ErrorCode == 10060)
                {
                    // ログを書き込み
                    log.Error("<Err> at $StartReceive()$ Socket Connect TimeOut.");
                }
                else
                {
                    // Not Host Cut Connection
                    if (es.ErrorCode != 10053 && es.ErrorCode != 10054)
                    {
                        // ログを書き込み
                        log.Error(string.Format("<Err> at $StartReceive()$ SocketException - {0}, ErrorCode = {1}.", es.ToString(), es.ErrorCode));
                    }
                }

                // ソケット切断
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
                this.socket = null;
                
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at $StartReceive()$ Exception - {0}.", ex.ToString()));
                
                // ソケット切断
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
                this.socket = null;
            }
            finally
            {
            }
        }

        //ヘッダ文字
        protected byte[] startWord = BitConverter.GetBytes(Message.VPowerMessage.StartWord);

        protected bool CheckStartWordisVaild(byte[] currentStartWord)
        {
            for (int i = 0; i < currentStartWord.Length; i++)
            {
                // 開始フラグをチェック
                if (!this.startWord[i].Equals(currentStartWord[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// データ整理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwSender_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                log.Info("Sno Data Check in...");

                Command cmd = (Command)e.Argument;
                //e.Result = this.SendVpowerCommandToClient(cmd);
                e.Result = true;

                byte[] revBuffBody = cmd.CommandBody;
                string tranNumber = string.Empty;

                int fsnCnt = (revBuffBody.Length - Message.VPowerMessage.NoteHeadSize) / Message.VPowerMessage.NoteSize;
                //int sequence = SNDBClient.GetInstance().Sequence;

                List<SerialNumberTbl> snoTmpTbl = new List<SerialNumberTbl>();
                List<TransactionsTbl> tranTmpTbl = new List<TransactionsTbl>();

                for (UInt16 i = 0; i < fsnCnt; i++)
                {
                    byte[] revBuffBodyTmp = new byte[Message.VPowerMessage.NoteSize];

                    Array.Copy(revBuffBody, Message.VPowerMessage.NoteHeadSize + i * Message.VPowerMessage.NoteSize, revBuffBodyTmp, 0, Message.VPowerMessage.NoteSize);

                    //FSNファイル構造化
                    Message.Pboc_NoteInfo pboc_fsn_Tmp = CommonUtil.ByteToStructure<Message.Pboc_NoteInfo>(revBuffBodyTmp);
                    string date = this.GetDate(BitConverter.ToUInt16(pboc_fsn_Tmp.Date, 0));
                    string time = this.GetTime(BitConverter.ToUInt16(pboc_fsn_Tmp.Time, 0));
                    string[] dt = date.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] ti = time.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                    //TranNumber確定
                    //tranNumber = System.Text.ASCIIEncoding.Default.GetString(pboc_fsn_Tmp.MachineSNo, 0, pboc_fsn_Tmp.MachineSNo.Length).Replace("\0", string.Empty);
                    //string tranNumberCur = time.Replace(":", "") + tranNumber.Substring(13, 6) + GenerateRandomCode(4) + sequence.ToString().PadLeft(5, '0');
                    tranNumber = System.Text.ASCIIEncoding.Default.GetString(pboc_fsn_Tmp.MachineSNo, 0, pboc_fsn_Tmp.MachineSNo.Length).Replace("\0", string.Empty);
                    string tranNumberCur = "0001" + tranNumber.Substring(tranNumber.Length - 9, 9) + "NOSEQUENCE";

                    #region 取引データ
                    if(i == 0)
                    {
                        TransactionsTbl tranT = new TransactionsTbl();
                        tranT.TraceNumber = tranNumberCur;
                        tranT.ClientNumber = 1;
                        tranT.MachineNumber = tranNumber.Substring(tranNumber.Length - 9, 9);
                        tranT.TranDate = Int32.Parse(dt[0] + dt[1] + dt[2]);
                        tranT.TranTime = Int32.Parse(ti[0] + ti[1] + ti[2]);
                        tranT.ClientNumber = 1;
                        tranT.ClientName = "1";
                        tranT.TerminalNumber = 1;
                        tranT.Piece = fsnCnt;
                        tranT.Amount = 0;
                        tranT.DeclareAmount = 0;
                        tranT.Result = 0;
                        tranT.ErrorCode = "";
                        tranT.ErrorDetail = "";
                        tranT.UpdateDateTime = DateTime.Now.ToString();

                        tranTmpTbl.Add(tranT);
                    }
                     #endregion

                    #region 記番号データのまとめ
                    SerialNumberTbl snoS = new SerialNumberTbl();
                    snoS.TraceNumber = tranNumberCur;
                    snoS.TranDate = Int32.Parse(dt[0] + dt[1] + dt[2]);
                    snoS.TranTime = Int32.Parse(ti[0] + ti[1] + ti[2]);
                    snoS.Sequence = i + 1;
                    snoS.Country = System.Text.Encoding.Default.GetString(pboc_fsn_Tmp.MoneyFlag).Replace(new String(new Char[] { '\0' }), string.Empty);
                    snoS.Media = BitConverter.ToUInt16(pboc_fsn_Tmp.Valuta, 0).ToString();
                    snoS.UnitValue = BitConverter.ToUInt16(pboc_fsn_Tmp.Valuta, 0);
                    if (BitConverter.ToUInt16(pboc_fsn_Tmp.Ver,0) == 0)
                    {
                        snoS.Version = 1990;
                    }
                    else if (BitConverter.ToUInt16(pboc_fsn_Tmp.Ver, 0) == 1)
                    {
                        snoS.Version = 1999;
                    }
                    else if (BitConverter.ToUInt16(pboc_fsn_Tmp.Ver, 0) == 2)
                    {
                        snoS.Version = 2005;
                    }
                    else if (BitConverter.ToUInt16(pboc_fsn_Tmp.Ver, 0) == 3)
                    {
                        snoS.Version = 2015;
                    }
                    else
                    {
                        snoS.Version = 9999;
                    }
                    snoS.SerialNo1 = System.Text.Encoding.Default.GetString(pboc_fsn_Tmp.SNo).Replace(new String(new Char[] { '\0' }), "").Substring(0,10);
                    snoS.SerialNo2 = null;
                    snoS.CounterFeit = 0;
                    snoS.OCRRate1 = 10000;
                    snoS.OCRRate2 = 10000;
                    snoS.BVResult = BitConverter.ToInt16(pboc_fsn_Tmp.tfFlag,0);
                    snoS.ErrorCode = BitConverter.ToUInt16(pboc_fsn_Tmp.ErrorCode, 0)
                                   + BitConverter.ToUInt16(pboc_fsn_Tmp.ErrorCode, 2)
                                   + BitConverter.ToUInt16(pboc_fsn_Tmp.ErrorCode, 4);
                    snoS.BundleDate = 0;
                    snoS.BundleTime = 0;
                    snoS.UpdateUser = 0;
                    snoS.OperatorID = 1;
                    snoS.Updatedatetime = DateTime.Now.ToString();

                    snoTmpTbl.Add(snoS);

                    #endregion
                }

                log.Info(string.Format("Sno {0} recieved...",fsnCnt));

                //データベースリスト追加
                DBCreater.DbCreaterWk dbWk = new DBCreater.DbCreaterWk(tranTmpTbl, snoTmpTbl);

                DBCreater.GetInstance().makingTblList.Add(dbWk);

                //データベース更新
                //SNDBClient.GetInstance().BulkInsert(tranTmpTbl, snoTmpTbl);

                if(this.stopwatch.IsRunning)
                {
                    this.stopwatch.Stop();
                    log.Info(string.Format("★★★★ Run In {0} ms with {1} ★★★★\n", this.stopwatch.ElapsedMilliseconds, fsnCnt));
                    this.stopwatch.Reset();
                }

                //人民銀行へのデータアップロード
                if (cmd.getCmdType() != 0x30)
                {
                    byte[] temp = null;

                    string bussinessType = (cmd.getCmdType() == (UInt16)0x31 || cmd.getCmdType() == (UInt16)0x41 ? "KA" : "KH");

                    TcpSender.getInstance().TryTimes = this.tryTimes;

                    TcpSender.getInstance().FsnCount += fsnCnt;

                    log.Info(string.Format("CSPBC: BussinessType is {0}", bussinessType));

                    log.Info(string.Format("CSPBC: TryTimes is {0}", this.tryTimes));

                    log.Info(string.Format("CSPBC: {0} Sno has been received !", fsnCnt));

                    log.Info(string.Format("CSPBC: Current Sno is {0}!", TcpSender.getInstance().FsnCount));

                    if (TcpSender.getInstance().SendMessage == null)
                    {
                        TcpSender.getInstance().SendMessage = new byte[cmd.CommandBody.Length - Message.VPowerMessage.NoteHeadSize];

                        Array.Copy(cmd.CommandBody, Message.VPowerMessage.NoteHeadSize, TcpSender.getInstance().SendMessage, 0, cmd.CommandBody.Length - Message.VPowerMessage.NoteHeadSize);
                    }
                    else
                    {
                        temp = new byte[cmd.CommandBody.Length - Message.VPowerMessage.NoteHeadSize + TcpSender.getInstance().SendMessage.Length];
                        Array.Copy(TcpSender.getInstance().SendMessage, 0, temp, 0, TcpSender.getInstance().SendMessage.Length);
                        Array.Copy(cmd.CommandBody, Message.VPowerMessage.NoteHeadSize, temp, TcpSender.getInstance().SendMessage.Length, cmd.CommandBody.Length - Message.VPowerMessage.NoteHeadSize);

                        TcpSender.getInstance().SendMessage = temp;
                    }
                    
                    if (TcpSender.getInstance().FsnCount >= 1000)
                    {
                        log.Info(string.Format("CSPBC: Start Upload ....."));

                        temp = new byte[Message.VPowerMessage.NoteHeadSize];
                        Array.Copy(cmd.CommandBody, 0, temp, 0, Message.VPowerMessage.NoteHeadSize);
                        Message.FsnFileHeaderStruct pboc_head_Tmp = CommonUtil.ByteToStructure<Message.FsnFileHeaderStruct>(temp);

                        byte[] headerData;
                        if(!this.UpdateFileHeader(TcpSender.getInstance().FsnCount, pboc_head_Tmp,out headerData))
                        {
                            log.Error(string.Format("<Err> at $bwSender_DoWork()$ update fsn header Failed!"));
                            TcpSender.getInstance().FsnCount = 0;
                            TcpSender.getInstance().SendMessage = null;
                            return;
                        }

                        temp = new byte[Message.VPowerMessage.NoteHeadSize + TcpSender.getInstance().SendMessage.Length];
                        Array.Copy(headerData, 0, temp, 0, headerData.Length);
                        Array.Copy(TcpSender.getInstance().SendMessage, 0, temp, Message.VPowerMessage.NoteHeadSize, TcpSender.getInstance().SendMessage.Length);

                        TcpSender.getInstance().SendMessage = null;

                        TcpSender.getInstance().MainUpload(this.serverIp, cmd.getMachineSno(), bussinessType, cmd.getBranchNumber(),
                                                           cmd.getOperatorId(), temp, TcpSender.getInstance().FsnCount);

                        TcpSender.getInstance().FsnCount = 0;
                    }
                }

                //SNDBClient.GetInstance().Sequence = sequence + 1;
               
            }
            catch(Exception ex)
            {
                TcpSender.getInstance().SendMessage = null;

                TcpSender.getInstance().FsnCount = 0;

                log.Error(string.Format("<Err> at $bwSender_DoWork()$ {0}.", ex.Message));
            }
        }

        private void bwSender_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null && ((bool)e.Result))
                this.OnCommandSent(new EventArgs());
            else
                this.OnCommandFailed(new EventArgs());

            ((BackgroundWorker)sender).Dispose();
            GC.Collect();
        }

        System.Threading.Semaphore semaphor = new System.Threading.Semaphore(1, 1);
        private bool SendVpowerCommandToClient(Command cmd)
        {
            try
            {
                semaphor.WaitOne();

                if(cmd.CommandType == Message.VpowerCmd.V_UP)
                {
                    this.networkStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);

                    this.networkStream.Flush();
                }

                semaphor.Release();
                return true;
            }
            catch
            {
                semaphor.Release();
                return false;
            }
        }

        /// <summary>
        /// ファイルヘッダ更新
        /// </summary>
        /// <param name="noteCount">紙幣枚数</param>
        /// <param name="headerData">ファイルヘッダ部のバイナリ配列</param>
        /// <returns>結果</returns>
        private bool UpdateFileHeader(int noteCount, Message.FsnFileHeaderStruct fsnFileHeader, out byte[] headerData)
        {
            try
            {
                // カウンタ更新
                fsnFileHeader.counter += (uint)noteCount;
                // ファイルヘッダ部のバイナリ配列作成
                headerData = new byte[Message.VPowerMessage.NoteHeadSize];
                IntPtr ptr = Marshal.AllocHGlobal(headerData.Length);
                Marshal.StructureToPtr(fsnFileHeader, ptr, true);
                Marshal.Copy(ptr, headerData, 0, headerData.Length);
                Marshal.FreeHGlobal(ptr);
                return true;
            }
            catch (Exception ex)
            {
                // ログを書き込み
                log.Error(string.Format("<Err> at UpdateFileHeader()$ {0}.", ex.Message));
                headerData = null;
                return false;
            }
        }
        #endregion

        #region CheckSum検証法
        private UInt16 checksum(byte[] data, int len)
        {
            UInt16 sum = 0x0000;
            for (int i = 32; i < len-32; i = i + 2)
            {
                byte[] tmp = new byte[2];

                Array.Copy(data, i, tmp, 0, 2);

                UInt16 tmpWk = BitConverter.ToUInt16(data, i);

                sum += tmpWk;
            }
            return sum;
        }
        #endregion

        #region Public Methods

        //public static string GenerateRandomCode(int length)
        //{
        //    StringBuilder result = new StringBuilder();
        //    for (int i = 0; i < length; i++)
        //     {
        //        var r = new Random(Guid.NewGuid().GetHashCode());
        //        result.Append(r.Next(0, 10));
        //     }
        //   return result.ToString();
        //}

        public void SendVpowerRevCommand(Command cmd)
        {
            if (this.socket != null && this.socket.Connected)
            {
                BackgroundWorker bwSender = new BackgroundWorker();
                //データレシーブ
                bwSender.DoWork += new DoWorkEventHandler(bwSender_DoWork);
                bwSender.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwSender_RunWorkerCompleted);
                bwSender.RunWorkerAsync(cmd);
            }
            else
            {
                this.OnCommandFailed(new EventArgs());
            }
        }

        // Disconnect the current client manager from the remote client and returns true if the client had been disconnected from the server.
        public bool Disconnect()
        {
            if (this.socket != null && this.socket.Connected)
            {
                try
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public String GetDate(int dateTemp)
        {

            int year = dateTemp >> 9;

            String month = "";
            int monthTemp = (dateTemp - (year << 9)) >> 5;

            if ((monthTemp) / 10 < 1)
            {
                month = "0" + monthTemp;
            }
            else
            {
                month = "" + monthTemp;
            }

            String day = "";
            int dayTemp = dateTemp - (year << 9) - ((monthTemp) << 5);

            if (dayTemp / 10 < 1)
            {
                day = "0" + dayTemp;
            }
            else
            {
                day = "" + dayTemp;
            }

            String date = "" + (year + 1980) + "/" + month + "/" + day;
            return date;
        }

        public String GetTime(int timeTemp)
        {

            int hourTemp = timeTemp >> 11;
            String hour = hourTemp.ToString().PadLeft(2, '0');

            int minuteTemp = (timeTemp - (hourTemp << 11)) >> 5;
            String minute = "";

            if (minuteTemp / 10 < 1)
            {
                minute = "0" + minuteTemp;
            }
            else
            {
                minute = "" + minuteTemp;
            }

            int secondTemp = (timeTemp - (hourTemp << 11) - (minuteTemp << 5)) << 1;
            String second = "";

            if (secondTemp / 10 < 1)
            {
                second = "0" + secondTemp;
            }
            else
            {
                second = "" + secondTemp;
            }

            String time = "" + hour + ":" + minute + ":" + second;
            return time;
        }
        
        #endregion

        #region Events
        public event CommandReceivedEventHandler CommandReceived;
        protected virtual void OnCommandReceived(CommandEventArgs e)
        {
            if (CommandReceived != null)
                CommandReceived(this, e);
        }
        public event CommandSendingFailedEventHandler CommandFailed;
        protected virtual void OnCommandFailed(EventArgs e)
        {
            if (CommandFailed != null)
                CommandFailed(this, e);
        }
        public event DisconnectedEventHandler Disconnected;
        protected virtual void OnDisconnected(ClientEventArgs e)
        {
            if (Disconnected != null)
                Disconnected(this, e);
        }
        public event CommandSentEventHandler CommandSent;
        protected virtual void OnCommandSent(EventArgs e)
        {
            if (CommandSent != null)
                CommandSent(this, e);
        }
        #endregion
    }
}
