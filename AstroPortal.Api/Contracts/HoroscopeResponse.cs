namespace AstroPortal.Api.Contracts;

public record HoroscopeResponse(
    string Sign,
    DateOnly Date,
    string Content,
    string Source);
