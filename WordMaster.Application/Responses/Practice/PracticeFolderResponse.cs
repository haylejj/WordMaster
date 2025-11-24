using System.Collections.Generic;
using WordMaster.Application.Responses;

namespace WordMaster.Application.Responses.Practice;

public class PracticeFolderResponse
{
    public long FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;
    public List<PracticeFolderWordResponse> Words { get; set; } = [];
}
