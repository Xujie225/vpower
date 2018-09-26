using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CommandServer
{
    public class Command
    {

        #region 属性
        private Message.VpowerCmd cmdType;
        public Message.VpowerCmd CommandType
        {
            get { return cmdType; }
            set { cmdType = value; }
        }

        private IPAddress senderIP;
        public IPAddress SenderIP
        {
            get { return senderIP; }
            set { senderIP = value; }
        }

        private IPAddress target;
        public IPAddress Target
        {
            get { return target; }
            set { target = value; }
        }

        private byte[] commandBody;
        public byte[] CommandBody
        {
            get { return commandBody; }
            set { commandBody = value; }
        }

        private byte[] commandHeader;
        #endregion

        public Command(Message.VpowerCmd type, IPAddress targetMachine, byte[] commandBody)
        {
            this.cmdType = type;
            this.target = targetMachine;
            this.commandBody = commandBody;
        }

        public Command(Message.VpowerCmd type, IPAddress targetMachine, byte[] commandBody,  byte[] commandHeader)
        {
            this.cmdType = type;
            this.target = targetMachine;
            this.commandBody = commandBody;
            this.commandHeader = commandHeader;
        }

        public string getBranchNumber()
        {
            try
            {
                Message.VPowerMessage.V_UP v_head = CommonUtil.ByteToStructure<Message.VPowerMessage.V_UP>(this.commandHeader);
                return System.Text.ASCIIEncoding.Default.GetString(v_head.branchNumber);
            }
            catch
            {
                return string.Empty;
            }
        }

        public UInt16 getCmdType()
        {
            try
            {
                Message.VPowerMessage.V_UP v_head = CommonUtil.ByteToStructure<Message.VPowerMessage.V_UP>(this.commandHeader);

                return BitConverter.ToUInt16(v_head.cmd,0);
            }
            catch
            {
                return 0x30;
            }
        }

        public string getOperatorId()
        {
            try
            {
                Message.VPowerMessage.V_UP v_head = CommonUtil.ByteToStructure<Message.VPowerMessage.V_UP>(this.commandHeader);
                return System.Text.ASCIIEncoding.Default.GetString(v_head.operatorId);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string getMachineSno()
        {
            try
            {
                Message.VPowerMessage.V_UP v_head = CommonUtil.ByteToStructure<Message.VPowerMessage.V_UP>(this.commandHeader);
                return System.Text.ASCIIEncoding.Default.GetString(v_head.machineNo).Replace("\0","");
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
