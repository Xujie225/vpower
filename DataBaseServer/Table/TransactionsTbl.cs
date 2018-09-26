
namespace DataBaseServer
{
    public class TransactionsTbl
    {
        public string TraceNumber { set; get; }
        public int TranDate { set; get; }
        public int TranTime { set; get; }
        public int ClientNumber { set; get; }
        public string ClientName { set; get; }
        public int TerminalNumber { set; get; }
        public string MachineNumber { set; get; }
        public int Piece { set; get; }
        public int Amount { set; get; }
        public int DeclareAmount { set; get; }
        public int Result { set; get; }
        public string ErrorCode { set; get; }
        public string ErrorDetail { set; get; }
        public string UpdateDateTime { set; get; }
    }
}
