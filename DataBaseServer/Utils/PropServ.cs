using System;
using System.IO;
using System.Collections;

namespace DataBaseServer
{
    public class PropServ
    {
        protected static string path = ".\\";

        private const int FILENAME_INDEX = 0;

        private const int KEY_INDEX = 1;

        private const int VAR_NAME = 0;

        private const int R_VALUE = 1;

        private const string EXTEND_NAME = ".prop";

        /**
	    * Get property from property file in fixed path <br>
	    * @param fileName
	    * @param key
	    * @return
	    * @throws IOException
	    */
        public static String getString(String fileName, String key)
        {
            StreamReader fis = new StreamReader(path + fileName, System.Text.Encoding.Default);
            if (fis == null) throw new IOException();

            //DataTable dt = new DataTable("propTable");
            //PropertyCollection Property = new PropertyCollection();
            //Property = dt.ExtendedProperties;
            Hashtable prov = new Hashtable();

            int cnt = 0;
            while (fis.Peek() > -1)
            {
                string stbuff = fis.ReadLine().Replace("\t", "");
                if (stbuff != "")
                {
                    String mValue = null;
                    String curline = null;
                    curline = strUtils.getEqualSepUnitByIndex(stbuff, R_VALUE);
                    if (curline != null)
                    {
                        while (strUtils.chkPosBackslash(curline.Trim()))
                        {
                            mValue += strUtils.cutBackslash(curline);
                            curline = fis.ReadLine().Replace("\t", "");
                        }
                    }
                    mValue += curline;
                    //prov.Add(strUtils.getEqualSepUnitByIndex(stbuff, VAR_NAME), strUtils.getEqualSepUnitByIndex(stbuff, R_VALUE));
                    prov.Add(strUtils.getEqualSepUnitByIndex(stbuff, VAR_NAME), mValue);
                }
            }

            return (String)prov[key];
        }       

        /**
	    * Get property from property file in fixed path <br>
	    * @param fileName
	     * @param key
	     * @return
	     * @throws Exception 
	     * @throws IOException
	     */
        public static String getString(
                String keyInfo)
        {
            return getString(
                    getFileName(keyInfo),
                    getKey(keyInfo));
            throw new Exception();
        }

        /**
        * Separate string by colon - ":" and return the second part<br>
        * example: <br>
         * 		FileName:Key => Key <br>
         * 		FileName => null <br>
         * 		Key => null <br>
         * 		
         * @param keyInfo
         * @return
         */
        private static String getKey(
            String keyInfo)
        {

            return strUtils.getColonSepUnitByIndex(keyInfo, KEY_INDEX);

        }

        /**
         * Separate string by colon - ":" and return the first part<br>
         * example: <br>
         * 		FileName:Key => FileName <br>
         * 		FileName => FileName <br>
         * 		Key => null <br>
         * 		
         * @param keyInfo
         * @return
         */
        private static String getFileName(
                String keyInfo)
        {

            return strUtils.getColonSepUnitByIndex(keyInfo, FILENAME_INDEX) + EXTEND_NAME;

        }

        /**
         * @return the path
         */
        public static String getPath()
        {
            return path;
        }

        /**
         * @param path the path to set
         */
        public static void setPath(String path)
        {
            PropServ.path = path;
        }
    }
}
