namespace WordMaster.Application.Constants;

public static class CacheKeys
{
    public static string Word(long wordId, Guid userId)
    {
        return $"word:{wordId}:user:{userId}";
    }

    public static string Words(Guid userId)
    {
        return $"words:user:{userId}";
    }

    public static string UserWordsDropdown(Guid userId)
    {
        return $"dropdown_words:user:{userId}";
    }

    public static string Favorites(Guid userId)
    {
        return $"favorites:user:{userId}";
    }

    public static string Favorite(long favoriteId, Guid userId)
    {
        return $"favorite:{favoriteId}:user:{userId}";
    }

    public static string Unknows(Guid userId)
    {
        return $"unknows:user:{userId}";
    }

    public static string Unknow(long unknowId, Guid userId)
    {
        return $"unknows:{unknowId}:user:{userId}";
    }

    public static string Folders(Guid userId)
    {
        return $"folders:user:{userId}";
    }

    public static string FolderWords(long folderId, Guid userId)
    {
        return $"folder:{folderId}:user:{userId}:words";
    }
}
