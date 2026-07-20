namespace Application.Common.Storage;

/// <summary>
/// Upload folder routing. Use <see cref="Gallery"/> or <see cref="Media"/> for social activity cover images.
/// </summary>
public enum StorageFeatureType
{
    Announcement,
    Event,
    Campaign,
    Gallery,
    ECard,
    Media,
    Document,
    Temporary
}
