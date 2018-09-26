using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataBaseServer
{
    public class strUtils
    {
        public const char COLON = ':';
        public const char BRACE_LEFT = '{';
        public const char BRACE_RIGHT = '}';

        public const char SQURE_LEFT = '[';
        public const char SQURE_RIGHT = ']';

        public const char PERCENT_CODE = '%';
        public const char EQUAL = '=';
        public const char BACKSLASH = '\\';

        public const string QUOT = "'";
        public const string NULL_STR = "NULL";
        public const int SQURE_MODE = 0;
        public const int BRACE_MODE = 1;


        /**
         * Separate String by defined signal <br>
         * 
         * example:
         * 		a:b:c => List contains elements a and b and c when signal is ":"
         * @param keyInfo
         * @param signal
         * @return
         */
        public static List<String> getSepUnitList(
            String keyInfo,
            Char signal)
        {
            List<String> sepaUnits = new List<String>();

            String[] st = keyInfo.Split(signal);

            foreach (string s in st)
            {
                sepaUnits.Add(s);
            }

            return sepaUnits;
        }

        /**
	    * Separate String by colon and return by defined index
	    * 
	    * example:
	    * 		getColonSepUnitByIndex("a:b", 1) => "b" 
	    * @param sigSepStr
	    * @param index
	    * @return
	    */
        public static String getColonSepUnitByIndex(String sigSepStr, int index)
        {

            List<String> sepaUnits = strUtils.getSepUnitList(sigSepStr, strUtils.COLON);

            if (sepaUnits.Count < index + 1)
            {
                return null;
            }

            return (String)sepaUnits.ElementAt(index);

        }

        /**
	    * Separate String by equal and return by defined index
	    * 
	    * example:
	    * 		getEqualSepUnitByIndex("a = b", 1) => "b" 
	    * @param sigSepStr
	    * @param index
	    * @return
	    */
        public static String getEqualSepUnitByIndex(String sigSepStr, int index)
        {
            List<String> sepaUnits = strUtils.getSepUnitList(sigSepStr, strUtils.EQUAL);

            if (sepaUnits.Count < index + 1)
            {
                return null;
            }

            return (String)sepaUnits.ElementAt(index).Trim();

        }

        /**
	    * Remove optional parram sentence when no related value setted to parrams
	    * @param parrams
	    * @param text
	    * @return
	    */
        public static String getOptRepText(Hashtable parrams, String text)
        {
            int squreLeftPos = text.IndexOf(SQURE_LEFT);
            int squreRightPos = text.IndexOf(SQURE_RIGHT);



            if ((squreLeftPos == -1) || (squreRightPos == -1))
            {
                text = getRepText(parrams, text, BRACE_MODE);
                return text;
            }

            //cut
            String parrWord = text.Substring(squreLeftPos + 1, squreRightPos - squreLeftPos - 1);
            String repText = getRepText(parrams, parrWord, SQURE_MODE);

            if (repText == null)
            {
                text = text.Substring(0, squreLeftPos - 1) + text.Substring(squreRightPos + 1, text.Length - squreRightPos - 1);
            }
            else
            {
                text = text.Substring(0, squreLeftPos - 1) + repText + text.Substring(squreRightPos + 1, text.Length - squreRightPos - 1);
            }

            text = getOptRepText(parrams, text);

            return text;
        }

        /**
	    * Replace String value by values contained in Map
	    * 
	    * @param parrams
	    * @param sqlKeyInfo
	    * @return
	    */
        public static String getRepText(Hashtable parrams, String text, int mode)
        {
            int braceLeftPos = text.IndexOf(BRACE_LEFT);
            int braceRightPos = text.IndexOf(BRACE_RIGHT);

            if ((braceLeftPos == -1) || (braceRightPos == -1))
            {
                return text;
            }
            // cut
            String parrWord = text.Substring(braceLeftPos + 1, braceRightPos - braceLeftPos - 1);
            // find
            String parrVal = (String)parrams[parrWord];
            if (mode == SQURE_MODE)
            {
                if (!hasValue(parrVal) || NULL_STR.Equals(parrVal))
                {
                    return null;
                }
            }
            // replace
            text = text.Substring(0, braceLeftPos - 1) + (parrVal == null ? (strUtils.getStrParr(parrVal)) : parrVal)
                    + text.Substring(braceRightPos + 1, text.Length - braceRightPos - 1);
            String repText = getRepText(parrams, text, mode);
            return repText;
        }


        /**
         * 
         * @param strParr
         * @return
         */
        public static String getStrParr(String strParr)
        {
            if (hasValue(strParr))
            {
                return QUOT + strParr + QUOT;
            }
            else
            {
                return "NULL";
            }
        }

        /**
         * 
         * @param strParr
         * @return
         */
        public static String getStrParrIncPer(String strParr)
        {

            if (hasValue(strParr))
            {
                return QUOT + PERCENT_CODE + strParr + PERCENT_CODE + QUOT;
            }
            else
            {
                return "NULL";
            }
        }

        public static String getIntParr(String strParr)
        {
            if (hasValue(strParr))
            {
                return strParr;
            }
            else
            {
                return "NULL";
            }
        }

        /**
         * Judge if the parram is setted value
         * 
         * @param strParr
         * @return
         */
        private static bool hasValue(String strParr)
        {
            if (strParr == null || "".Equals(strParr))
            {
                return false;
            }
            return true;
        }

        /**
         *Get the string when no colon 
         * @param userIdStr
         * @return
         */
        public static String getStrNoColon(String userIdStr)
        {
            userIdStr.Replace("[", "");
            userIdStr.Replace("]", "");
            return null;
        }

        /**
         *Check the position of backslash 
         * @param strParr
         * @return
         */
        public static bool chkPosBackslash(String strParr)
        {
            return (strParr.Trim().Length - 1) == strParr.LastIndexOf(BACKSLASH);
        }

        /**
         * delete backslash 
         * @param strParr
         * @return
         */
        public static String cutBackslash(String strParr)
        {
            return strParr.Replace(BACKSLASH, ' ');
        }
    }
}
