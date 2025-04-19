namespace UrlShortener.Api.Models;

public class Url
{
    public int Id { get; set; }
    public string LongUrl { get; set; }
    public string CompactUrl { get; set; }
}
