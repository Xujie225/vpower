using System;
using System.Collections.Generic;
using System.Data;

namespace DataBaseServer
{
    public class TableContainer
    {
        protected List<Object> columns = new List<Object>();
        protected List<List<String>> table = new List<List<String>>();
        protected List<String> record = new List<String>();

        public TableContainer() { }

        //Init table
        public void initTable()
        {
            this.table = new List<List<String>>();
        }

        //Init record
        public void iniRecord()
        {
            record = new List<String>();
            for (int i = 0; i < columns.Count; i++)
                record.Add("");
        }

        //Read Result Set to List when new object of TableContainer
        public TableContainer(DataSet rs)
        {
            setTableAndColumns(rs);
        }

        /**
         * Set ResultSet data to List constructure
         * @param rs
         * @throws Exception
         **/
        public void setTableAndColumns(DataSet rs)
        {
            this.columns = new List<object>();
            this.table = new List<List<String>>();

            List<String> dataList = null;

            //get columns
            for (int i = 0; i < rs.Tables[0].Columns.Count; i++)
            {
                this.columns.Add(rs.Tables[0].Columns[i].ToString().ToUpper());
                //System.Console.Write("find table columns :" + rs.Tables[0].Columns[i].ToString());
            }

            //set table
            foreach (DataRow dr in rs.Tables[0].Rows)
            {
                dataList = new List<String>();

                for (int ci = 0; ci < this.columns.Count; ci++)
                    dataList.Add(dr[this.columns[ci].ToString()].ToString());

                this.table.Add(dataList);
            }
        }

        /**
         * Get the columns
         * @return
         **/
        public List<Object> getColumns()
        {
            return this.columns;
        }

        /**
	     * Set columns contained in DataSet
	     * @param rs
	     * @throws Exception
	     */
        public void setColumns(DataSet rs)
        {
            //get columns
            for (int i = 0; i < rs.Tables[0].Columns.Count; i++)
            {
                this.columns.Add(rs.Tables[0].Columns[i].ToString().ToUpper());
                //System.Console.Write("find table columns :" + rs.Tables[0].Columns[i].ToString());
            }
        }

        /**
	    * Set columns
	    * @param columns
	    */
        public void setColumns(List<Object> columns)
        {
            this.columns = columns;
        }

        /**
	     * Get the Table
	     * @return
	     */
        public List<List<String>> getTable()
        {
            return table;
        }

        /**
	     * Set ResultSet data to List by defined columns
	     * @param rs
	     * @param columns
	     * @throws Exception
	     */
        public void setTable(DataSet rs, List<Object> columns)
        {

            List<String> dataList = null;

            //set table
            foreach (DataRow dr in rs.Tables[0].Rows)
            {
                dataList = new List<String>();

                for (int ci = 0; ci < this.columns.Count; ci++)
                    dataList.Add(dr[this.columns[ci].ToString()].ToString());

                this.table.Add(dataList);
            }
        }

        /**
	     * Set ResultSet data to List by defined columns
	     * @param String[]
	     * @param columns
	     * @throws Exception
	     */
        public void setTable(String[][] str, List<Object> columns)
        {

            List<String> dataList = null;

            //set table
            foreach (String[] s in str)
            {
                dataList = new List<String>();

                dataList.Add(s[0]);
                dataList.Add(s[1]);

                this.table.Add(dataList);
            }
        }

        /**
	    * Search defined data in table by column and index <br>
	    * If no data returns null
	    * @param index
	    * @param column
	    * @return
	    */
        public String getString(
            int rowIndex,
            String col)
        {
            List<String> rec = table[rowIndex];
            int colIndex = columns.IndexOf(col.ToUpper());
            if (colIndex == -1) return null;

            return rec[colIndex].ToString();
        }

        /**
	     * Search defined data in table by column and index <br>
	     * If no data returns null
	     * @param index
	     * @param column
	     * @return
	     */
        public String getString(
            int rowIndex,
            int colIndex)
        {
            List<String> rec = table[rowIndex];

            if (colIndex == -1)
            {
                return null;
            }

            return record[colIndex].ToString();
        }

        /**
	    * Get size of the table
	    * @return
	    */
        public int getSize()
        {

            return table.Count;

        }

        /**
	     * Set value to record by column name
	     * @param record
	     * @param spendCateCD
	     * @throws CcolumnNameNotFoundExeption 
	     */
        public void setValueByColumn(String columnName, String value)
        {
            int columnIndex = columns.IndexOf(columnName);
            if (columnIndex == -1)
            {
                //throw  new CcolumnNameNotFoundExeption(columnName );  
                throw new Exception("column name is not founded!");
            }
            while (columnIndex >= record.Count)
            {
                record.Add("");
            }
            record.Insert(columnIndex, value);

        }

        /**
	    * Set record to table
	    *
	    */
        public void setRecord()
        {

            table.Add(record);

        }

        public static bool hasValue(TableContainer tc)
        {
            List<List<String>> table = tc.getTable();
            if (table == null || table.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
