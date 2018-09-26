using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CommandServer
{
    public class Message
    {
        #region Vpower整理
        /// <summary>
        /// V コマンド
        /// </summary>
        public enum VpowerCmd
        {
            /// <summary>
            /// 記番号送信コマンド
            /// </summary>
            V_UP,

            /// <summary>
            /// 記番号送信コマンド返事
            /// </summary>
            V_RTN,
        }

        /// <summary>
        /// VPOWER Message
        /// </summary>
        public struct VPowerMessage
        {
            /// <summary>
            /// 開始フラグ
            /// </summary>
            public const UInt32 StartWord = 0xAA55AA55;

            /// <summary>
            /// 番号記録
            /// </summary>
            public const UInt16 Up_HM = 0x30;

            /// <summary>
            /// 大束業務(ATM券)
            /// </summary>
            public const UInt16 Up_KA = 0x31;
            public const UInt16 Up_KA_S = 0x41;

            /// <summary>
            /// 大束業務(流通券)
            /// </summary>
            public const UInt16 Up_KH = 0x32;
            public const UInt16 Up_KH_S = 0x42;

            public const UInt16 Up_ST = 0xF0;

            /// <summary>
            /// 記番号ヘッダサイズ
            /// </summary>
            public const int NoteHeadSize = 32;

            /// <summary>
            /// 記番号サイズ
            /// </summary>
            public const int NoteSize = 1644;


            /// <summary>
            /// File Upload Message
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct V_UP
            {
                /// <summary>
                /// 開始フラグ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] start_word;

                /// <summary>
                /// 記番号の枚数
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] FSN_count;

                /// <summary>
                /// メッセージサイズ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] valid_num;

                /// <summary>
                /// コマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] cmd;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
                public byte[] machineNo;

                /// <summary>
                /// 操作員ID
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] operatorId;

                /// <summary>
                /// 機構番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] branchNumber;

            }

            /// <summary>
            /// Reply Message
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct V_RTN
            {
                //0x00→成功
                UInt16 errorCode;
            }

            /// <summary>
            /// Transaction Start
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct V_START
            {
                /// <summary>
                /// コマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] cmd;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] machineNo;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] end_word;
            }
        }

        /// <summary>
        /// 標準記番号構造体
        /// </summary>
        public struct Pboc_NoteInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Date;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Time;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] tfFlag;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] ErrorCode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] MoneyFlag;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Ver;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Valuta;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] CharNUM;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public byte[] SNo;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] MachineSNo;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Reserver1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] imageCharactorNum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] imageHeight;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] imageWidth;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1536)]
            public byte[] imageData;
        }

        /// <summary>
        /// FSNファイルフォーマットヘッダの構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct FsnFileHeaderStruct
        {
            /// <summary>
            /// ヘッダー開始部分
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] headStart;

            /// <summary>
            /// ヘッダーストリーム
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public ushort[] headString;

            /// <summary>
            /// 記番号の数
            /// </summary>
            [MarshalAs(UnmanagedType.I4, SizeConst = 4)]
            public uint counter;

            /// <summary>
            /// ヘッダー終了部分
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] headEnd;            
        }

        #endregion

        #region 湖南人民銀行
        /// <summary>
        /// 湖南人民銀行 コマンド
        /// </summary>
        public enum CSPBCCmd
        {
            /// <summary>
            /// 記番号送信の許可要求コマンド
            /// </summary>
            CSPBC_SIMPLE = 0x00A1,

            /// <summary>
            /// 記番号送信コマンド
            /// </summary>
            CSPBC_UP = 0x000B,

            /// <summary>
            /// 記番号送信完了コマンド
            /// </summary>
            CSPBC_CLOSE = 0x00A2,
        }

        /// <summary>
        /// CSPBC Message
        /// </summary>
        public struct CSPBCMessage
        {
            /// <summary>
            /// 開始フラグ
            /// </summary>
            public const UInt32 StartWord = 0x4C4A4040;

            /// <summary>
            /// バージョン
            /// </summary>
            public const UInt16 Version = 15;

            /// <summary>
            /// 紙幣情報送信の許可要求のメッセージ
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct CSPBC_SIMPLE
            {
                /// <summary>
                /// 開始フラグ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] start_word;

                /// <summary>
                /// メッセージの長さ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] msg_length;

                /// <summary>
                /// インターフェースのバージョン
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] version;

                /// <summary>
                /// コマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] cmd;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public UInt16[] machineNo;

                /// <summary>
                /// 支店番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] bankNumber;

                /// <summary>
                /// マシンタイプ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] MachineType;

                /// <summary>
                /// パッケージインデックス
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] packageIndex;

                /// <summary>
                /// 保留字
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] reserve;

                public CSPBC_SIMPLE(string machineCode, string branchCode)
                {
                    this.start_word = BitConverter.GetBytes(StartWord);

                    this.msg_length = BitConverter.GetBytes(54);

                    this.version = BitConverter.GetBytes(Version);

                    this.cmd = new byte[] { 0xA1, 0x00 };

                    this.machineNo = CommonUtil.StringToUshort(machineCode, 14);

                    //サイズ８以下
                    if (branchCode.Length > 8)
                    {
                        branchCode = branchCode.Substring(branchCode.Length - 8, 8);
                    }

                    this.bankNumber = ASCIIEncoding.UTF8.GetBytes(branchCode.PadLeft(8, '0'));

                    this.MachineType = ASCIIEncoding.UTF8.GetBytes("ZX");

                    this.packageIndex = new byte[] { 0x00, 0x00 };

                    this.reserve = new byte[] { 0x00, 0x00 };

                }
            }

            /// <summary>
            /// 紙幣情報要求のコマンドに対するサーバからのレスポンスメッセージ
            /// </summary>
            public struct CSPBC_RTN
            {
                /// <summary>
                /// インターフェースのバージョン
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] version;

                /// <summary>
                /// 対応のリクエストコマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] requestCmd;

                /// <summary>
                /// リターンコード
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] retCode;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public UInt16[] machineNo;

                /// <summary>
                /// パッケージインデックス
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] packageIndex;

                /// <summary>
                /// 保留字
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] reserve;
            }

            /// <summary>
            /// 紙幣情報送信メッセージ
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct CSPBC_UP
            {
                /// <summary>
                /// 開始フラグ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] start_word;

                /// <summary>
                /// メッセージの長さ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] msg_length;

                /// <summary>
                /// インターフェースのバージョン
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] version;

                /// <summary>
                /// コマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] cmd;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public UInt16[] machineNo;

                /// <summary>
                /// コード
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] code;

                /// <summary>
                /// ビジネスタイプ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] busi_type;

                /// <summary>
                /// 紙幣枚数
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] recordCount;

                /// <summary>
                /// 操作員
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] oper_no;

                public CSPBC_UP(string machineCode, string bussinessType, int noteCount, byte[] operId, int msgLen)
                {
                    this.start_word = BitConverter.GetBytes(StartWord);

                    this.msg_length = BitConverter.GetBytes(msgLen);

                    this.version = BitConverter.GetBytes(Version);

                    this.cmd = new byte[] { 0x0B, 0x00 };

                    this.machineNo = CommonUtil.StringToUshort(machineCode, 14);

                    this.code = new byte[20];

                    this.busi_type = System.Text.ASCIIEncoding.ASCII.GetBytes(bussinessType);

                    this.recordCount = BitConverter.GetBytes((UInt16)noteCount);

                    this.oper_no = new byte[20];
                    Array.Copy(operId, 0, oper_no, 0, operId.Length);
                }
            }

            /// <summary>
            /// 紙幣情報送信のコマンドに対するサーバからのレスポンスメッセージ
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct CSPBC_UP_RTN
            {
                /// <summary>
                /// 開始フラグ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] start_word;

                /// <summary>
                /// メッセージの長さ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] msg_length;

                /// <summary>
                /// インターフェースのバージョン
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] version;

                /// <summary>
                /// コマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] requestCmd;

                /// <summary>
                /// リータン
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] retCode;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public UInt16[] machineNo;

                /// <summary>
                /// パッケージインデックス
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] packageIndex;

                /// <summary>
                /// 保留字
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] reserve;
            }

            /// <summary>
            /// 紙幣情報送信完了メッセージ
            /// </summary>
            public struct CSPBC_CLOSE
            {
                /// <summary>
                /// 開始フラグ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] start_word;

                /// <summary>
                /// メッセージの長さ
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] msg_length;

                /// <summary>
                /// インターフェースのバージョン
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] version;

                /// <summary>
                /// コマンド
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] cmd;

                /// <summary>
                /// 機種番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
                public UInt16[] machineNo;

                /// <summary>
                /// パッケージインデックス
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] packageIndex;

                /// <summary>
                /// 保留字
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public byte[] reserve;

                public CSPBC_CLOSE(string machineCode)
                {
                    this.start_word = BitConverter.GetBytes(StartWord);

                    this.msg_length = BitConverter.GetBytes(44);

                    this.version = BitConverter.GetBytes(Version);

                    this.cmd = new byte[] { 0xA2, 0x00 };

                    this.machineNo = CommonUtil.StringToUshort(machineCode, 14);

                    this.packageIndex = new byte[] { 0x00, 0x00 };

                    this.reserve = new byte[] { 0x00, 0x00 };
                }
            }

        }
        #endregion
    }
}
