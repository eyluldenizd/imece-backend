namespace Core.Common;

/// <summary>
/// İstek iptal token'ına erişim soyutlaması. HTTP dışı host'larda None dönebilir.
/// </summary>
public interface IRequestCancellation
{
    CancellationToken Token { get; }
}
