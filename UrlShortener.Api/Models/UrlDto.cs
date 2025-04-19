namespace UrlShortener.Api.Models;

public record UrlRequestDto
{
    public string LongUrl { get; set; }
}


public record UrlResponseDto
{
    public string CompactUrl { get; set; }

}