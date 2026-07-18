namespace ImeceWebAPI.Authentication;

/// <summary>
/// Authentication scheme adları. Uygulama içi kod bu sabitleri kullanır;
/// controller/servisler doğrudan scheme'e bağlanmaz.
/// </summary>
public static class ImeceAuthenticationSchemes
{
    public const string Development = "ImeceDevelopment";
    public const string Test = "ImeceTest";
}
