namespace Fulfilment.Web.Model
{
    public class AuthorizationResult
    {
        public string UserId { get; set; }

        public DocumentAction Action { get; set; }

        public bool IsAllowed { get; set; }
    }
}
