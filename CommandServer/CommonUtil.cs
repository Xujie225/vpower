using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CommandServer
{
    /// <summary>
    /// 共通クラス
    /// </summary>
    public static class CommonUtil
    {
        /// <summary>
        /// 文字列を固定長さのushort配列に変換する共通メソッド
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="ushortLength">配列の長さ</param>
        public static ushort[] StringToUshort(string str, int ushortLength)
        {
            // byte をushortに変換する
            byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(str);
            ushort[] arrayTmp = new ushort[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                arrayTmp[i] = Convert.ToUInt16(byteArray[i]);
            }

            ushort[] array = new ushort[ushortLength];
            Array.Copy(arrayTmp, 0, array, 0, arrayTmp.Length);
            return array;
        }

        /// <summary>
        /// 文字列を固定長さのushort配列に変換する共通メソッド
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="ushortLength">配列の長さ</param>
        public static string UshortToString(ushort[] context)
        {
            // byte をushortに変換する
            byte[] byteArray = new byte[context.Length];
            for (int i = 0; i < context.Length; i++)
            {
                if (context[i] != 0x00)
                {
                    byteArray[i] = (byte)context[i];
                }
            }

            return ASCIIEncoding.ASCII.GetString(byteArray, 0, context.Length).Replace("\0", "");
        }

        /// <summary>
        /// メッセージを構造体に変換する共通メソッド
        /// </summary>
        /// <param name="message">メッセージ</param>
        public static T ByteToStructure<T>(byte[] message)
        {
            // メッセージの長さ
            int size = message.Length;

            // ポインタのメモリを申請する
            IntPtr msgData = Marshal.AllocHGlobal(size);

            // メッセージをポインタにコピーする
            Marshal.Copy(message, 0, msgData, size);

            // ポインタを構造体に変換する
            T structure = (T)Marshal.PtrToStructure(msgData, typeof(T));

            // ポインタのメモリを解放する
            Marshal.FreeHGlobal(msgData);

            // 構造体をリターンする
            return structure;
        }

        /// <summary>
        /// 構造体をメッセージに変換する共通メソッド
        /// </summary>
        /// <param name="structure">構造体を</param>
        public static byte[] StructureToBytes(object structure)
        {
            // 構造体のサイズ
            int size = Marshal.SizeOf(structure);

            // メッセージ
            byte[] message = new byte[size];

            // ポインタのメモリを申請する
            IntPtr msgData = Marshal.AllocHGlobal(size);

            // 構造体をポインタに変換する
            Marshal.StructureToPtr(structure, msgData, true);

            // ポインタ内容をバイト配列にコピーする
            Marshal.Copy(msgData, message, 0, size);

            // ポインタのメモリを解放する
            Marshal.FreeHGlobal(msgData);

            // バイト配列をリターンする
            return message;
        }

        /// <summary>
        /// 構造体をメッセージに変換する共通メソッド
        /// </summary>
        /// <param name="structure">構造体を</param>
        public static byte[] StructureToBytes(object structure, int addSize)
        {
            // 構造体のサイズ
            int size = Marshal.SizeOf(structure);

            // メッセージ
            byte[] message = new byte[size + addSize];

            // ポインタのメモリを申請する
            IntPtr msgData = Marshal.AllocHGlobal(size);

            // 構造体をポインタに変換する
            Marshal.StructureToPtr(structure, msgData, true);

            // ポインタ内容をバイト配列にコピーする
            Marshal.Copy(msgData, message, 0, size);

            // ポインタのメモリを解放する
            Marshal.FreeHGlobal(msgData);

            // バイト配列をリターンする
            return message;
        }
    }
}
