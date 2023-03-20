namespace Umbraco.AuthorizedServices.TestSite.Models;

public class HubspotContactResponse
{
    public IEnumerable<Result> Results { get; set; } = Enumerable.Empty<Result>();

    public class Result
    {
        public string Id { get; set; } = string.Empty;

        public ResultProperties Properties { get; set; } = new ResultProperties();
    }

    public class ResultProperties
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
