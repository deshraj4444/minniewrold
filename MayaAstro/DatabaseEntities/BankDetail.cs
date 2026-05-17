using System;
namespace MayaAstro.DatabaseEntities
{
    public class BankDetail
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string GstNumber { get; set; }

        public bool IsActive { get; set; }
    }
}

