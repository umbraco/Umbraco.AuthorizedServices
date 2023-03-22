namespace Umbraco.AuthorizedServices.Services.Implement
{
    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
