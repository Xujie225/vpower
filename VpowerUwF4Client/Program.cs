using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using CommandServer;
using FsnCreateServer;
using DataBaseServer;
using log4net;
using System.Globalization;

namespace VpowerUwF4Client
{
    class Program
    {
        private List<ClientManager> clients;
        private BackgroundWorker bwListener;
        private Socket listenerSocket;
        private IPAddress serverIP;
        private int serverPort;

        //湖南人民銀行関連設定
        private bool cspbcFlg;
        private IPAddress cspbcIP;
        private int cspbcPort;
        private int tryTimes;

        private FsnCreater fsnCreater;
        
        /// <summary>
        /// 日志
        /// </summary>
        private static ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Start the console server.
        /// </summary>
        static void Main(string [] args)
        {
            Process[] processes = System.Diagnostics.Process.GetProcessesByName(Application.CompanyName);
            if (processes.Length > 1)
            {
                MessageBox.Show("程序已经在运行中!!");
                Thread.Sleep(1000);
                System.Environment.Exit(1);
            }

            log.Info("Program Start...");
            log.Info("Log File Check...");

            //期限切れのログファイルを削除する
            int dayDiff = 0 - int.Parse(PropServ.getString("Setting:keepDays"));
            if (Directory.Exists(".\\Log\\AppLog\\"))
            {
                foreach (string file in Directory.GetFiles(".\\Log\\AppLog\\"))
                {
                    if (File.GetCreationTime(file) < DateTime.Now.Date.AddDays(dayDiff))
                    {
                        File.Delete(file);
                        log.Info(string.Format("LogFile {0} has been deleted.", new FileInfo(file).Name));
                    }
                }
            }
            //期限切れのFSNファイルを削除する
            try
            {
                if (Directory.Exists(".\\Fsn\\"))
                {
                    foreach (string dirTmp in Directory.GetDirectories(".\\Fsn\\"))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(dirTmp);

                        if (DateTime.ParseExact(dirInfo.Name, "yyyy-MM-dd", CultureInfo.CurrentCulture) < DateTime.Now.Date.AddDays(dayDiff))
                        {
                            Directory.Delete(dirTmp, true);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(".\\Fsn\\");
                }
            }
            catch (Exception ex) { log.Info(string.Format("FSN DELETE : {0} ", ex.Message)); }

            //データベースの初期化
            //SQLの接続文
            SNDBClient.GetInstance().SqlCon = String.Format("Data Source={0};User ID={1};Password={2};Initial Catalog={3}", PropServ.getString("Setting:server")
                                                                                                                        , PropServ.getString("Setting:userid")
                                                                                                                     , PropServ.getString("Setting:password")
                                                                                                                        , PropServ.getString("Setting:database"));
            try
            {
                Hashtable parrams = new Hashtable();

                //DBクリア
                log.Info("Sno Data Check...");
                parrams.Add("keepDays", strUtils.getStrParr(PropServ.getString("Setting:keepDays")));
                DbMng.update(parrams, "dbMng:SNDBClient.Transactions.delete");
                DbMng.update(parrams, "dbMng:SNDBClient.SerialNumber.delete");

                //シーケンス番号を確定
                log.Info("Transactions sequence updating...");
                parrams = new Hashtable();
                parrams.Add("machineNumber", strUtils.getStrParr(PropServ.getString("Setting:machineNo")));
                TableContainer tc = DbMng.exeQuarySQL(parrams, "dbMng:SNDBClient.Transactions.select");
                
                if (!TableContainer.hasValue(tc))
                {
                    Console.WriteLine("*** Sequence will begin with 1 ***\n");
                    SNDBClient.GetInstance().Sequence = 0;
                }
                else
                {                    
                    List<string> s = tc.getTable()[tc.getTable().Count - 1];
                    int tmpCurSeq = int.Parse(s[0].Substring(s[0].Length - 5, 5));
                    SNDBClient.GetInstance().Sequence = tmpCurSeq;
                }

                log.Info(string.Format("Transactions sequence is {0}", SNDBClient.GetInstance().Sequence + 1));

                Program progDomain = new Program();
                progDomain.clients = new List<ClientManager>();

                //デフォルト値或は設定ポート
                progDomain.serverPort = 8000;
                progDomain.serverIP = IPAddress.Any;
                if (args.Length == 1)
                {
                    progDomain.serverIP = IPAddress.Parse(args[0]);
                }
                else if (args.Length == 2)
                {
                    progDomain.serverIP = IPAddress.Parse(args[0]);
                    progDomain.serverPort = int.Parse(args[1]);
                }
                log.Info(string.Format("ServerIp is {0}", progDomain.serverIP));
                log.Info(string.Format("ServerPort is {0}\n", progDomain.serverPort));

                //湖南人民銀行関連設定
                progDomain.cspbcFlg = Convert.ToBoolean(PropServ.getString("Setting:cspbcEnable"));
                log.Info(string.Format("CSPBC ServerEnable is {0}", progDomain.cspbcFlg));

                if (progDomain.cspbcFlg)
                {
                    progDomain.cspbcIP = IPAddress.Parse(PropServ.getString("Setting:cspbcIP"));
                    progDomain.cspbcPort = int.Parse(PropServ.getString("Setting:cspbcPort"));
                    progDomain.tryTimes = int.Parse(PropServ.getString("Setting:tryTimes"));
                    log.Info(string.Format("CSPBC ServerIp is {0}", progDomain.cspbcIP));
                    log.Info(string.Format("CSPBC ServerPort is {0}", progDomain.cspbcPort));
                    log.Info(string.Format("CSPBC TryTimes is {0}", progDomain.tryTimes));
                }

                //fsn作成スレッド
                progDomain.fsnCreater = new FsnCreater();
                
                //監視ポートが立ち上がる
                progDomain.bwListener = new BackgroundWorker();
                progDomain.bwListener.WorkerSupportsCancellation = true;
                progDomain.bwListener.DoWork += new DoWorkEventHandler(progDomain.StartToListen);
                progDomain.bwListener.RunWorkerAsync();

                Console.WriteLine("*** Listening on port {0}{1}{2} started.Press ENTER to shutdown server. ***\n", progDomain.serverIP.ToString(), ":", progDomain.serverPort.ToString());

                Console.ReadLine();

                progDomain.fsnCreater.fileMakingProcessThread.Abort();

                progDomain.fsnCreater.fileMakingProcessThread = null;

                progDomain.fsnCreater = null;

                DBCreater.GetInstance().dbMakingProcessThread.Abort();

                DBCreater.GetInstance().dbMakingProcessThread = null;
                
                progDomain.DisconnectServer();
                
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        public void DisconnectServer()
        {
            if (this.clients != null)
            {
                foreach (ClientManager mngr in this.clients)
                    mngr.Disconnect();

                this.bwListener.CancelAsync();
                this.bwListener.Dispose();
                this.listenerSocket.Close();
                GC.Collect();
            }
        }

        /// <summary>
        /// 監視ポート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartToListen(object sender, DoWorkEventArgs e)
        {
            this.listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listenerSocket.Bind(new IPEndPoint(this.serverIP, this.serverPort));
            this.listenerSocket.Listen(10);
            while (true)
            {
                int seqCur = 0;
                lock (SNDBClient.GetInstance())
                {
                    seqCur = SNDBClient.GetInstance().Sequence + 1;

                    DBCreater.GetInstance().sequenceList.Add(seqCur);

                    SNDBClient.GetInstance().Sequence++;
                }

                this.CreateNewClientManager(this.listenerSocket.Accept());
            }
            
        }

        private void CreateNewClientManager(Socket socket)
        {
            ClientManager newClientManager = null;

            if (this.cspbcFlg)
            {
                newClientManager = new ClientManager(socket, new IPEndPoint(this.cspbcIP, this.cspbcPort), this.tryTimes);
            }
            else
            {
                newClientManager = new ClientManager(socket);
            }

            newClientManager.CommandReceived += new CommandReceivedEventHandler(CommandReceived);
            newClientManager.Disconnected += new DisconnectedEventHandler(ClientDisconnected);
            this.clients.Add(newClientManager);
            this.UpdateConsole("Connected.", newClientManager.IP, newClientManager.Port);
        }

        private void CommandReceived(object sender, CommandEventArgs e)
        {
            if(e.Command.CommandType.Equals(CommandServer.Message.VpowerCmd.V_UP))
            {
                //結果の返事
                this.SendCommandToTarget(e.Command);

                //FSN作成
                this.fsnCreater.makingFileList.Add(e.Command.CommandBody);
            }
        }

        private void SendCommandToTarget(Command cmd)
        {

            foreach (ClientManager mngr in this.clients)
                if (mngr.IP.Equals(cmd.Target))
                {
                    mngr.SendVpowerRevCommand(cmd);
                    return;
                }
        }

        private void UpdateConsole(string status, IPAddress IP, int port)
        {
            log.Info(string.Format("Client {0}{1}{2} has been {3} ( {4}|{5} )", IP.ToString(), ":", port.ToString(), status, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
        }

        void ClientDisconnected(object sender, ClientEventArgs e)
        {
            if (this.RemoveClientManager(e.IP,e.Port))
                this.UpdateConsole("Disconnected.", e.IP, e.Port);
        }

        private bool RemoveClientManager(IPAddress ip,int port)
        {
            try
            {
                lock (this)
                {
                    for (int index = 0 ; index < this.clients.Count;index++)
                    {
                        if (this.clients[index].Port == port && this.clients[index].IP.Equals(ip))
                        {
                            this.clients.RemoveAt(index);
                            break;
                        }
                    }
                    return true;                    
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
