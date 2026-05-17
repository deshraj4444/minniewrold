using System;
namespace MayaAstro.Models
{
    public class BankDetailVM
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string GSTNumber { get; set; }
        public List<BankDetailVM> BankList { get; set; } = new List<BankDetailVM>();

    }
}

