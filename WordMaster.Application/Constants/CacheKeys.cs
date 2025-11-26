namespace WordMaster.Application.Constants;

public static class CacheKeys
{
    public static string Word(long wordId, Guid userId) => $"word:{wordId}:user:{userId}";
    public static string Words(Guid userId) => $"words:user:{userId}";
    public static string UserWordsDropdown(Guid userId) => $"dropdown_words:user:{userId}";

    public static string Favorites(Guid userId) => $"favorites:user:{userId}";
    public static string Favorite(long favoriteId, Guid userId) => $"favorite:{favoriteId}:user:{userId}";

    public static string Unknows(Guid userId) => $"unknows:user:{userId}";
    public static string Unknow(long unknowId, Guid userId) => $"unknows:{unknowId}:user:{userId}";

    public static string Folders(Guid userId) => $"folders:user:{userId}";
    public static string Folder(long folderId, Guid userId) => $"folder:{folderId}:user:{userId}";
    public static string FolderWords(long folderId) => $"folder:{folderId}:words";
}
