namespace WordMaster.WebUI.Models;

using WordMaster.Domain.Entities;
using System.Collections.Generic;

public class FolderDetailViewModel
{
	public int FolderId { get; set; }
	public string FolderName { get; set; } = string.Empty;

	public List<Word> Words { get; set; } = new();
	public List<Word> AllWords { get; set; } = new();
}


