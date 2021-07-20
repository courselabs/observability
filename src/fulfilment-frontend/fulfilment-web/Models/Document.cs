namespace Fulfilment.Web.Models
{
    public class Document
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public string DocType { get; set; }
        public long Size { get; set; }
        public FulfilmentStatus FulfilmentStatus { get; set; }
        public string SubmittedByUserId { get; set; }
    }
}
