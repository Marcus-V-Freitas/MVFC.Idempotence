namespace MVFC.Idempotence.Models;

internal sealed record FilterState(
    ISet<string> AllowedMethods, 
    string HeaderName);