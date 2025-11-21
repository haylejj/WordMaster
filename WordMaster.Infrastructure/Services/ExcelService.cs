using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore.Storage;
using System.Globalization;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Extensions;
using WordMaster.Domain.Results;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class ExcelService(AppDbContext context) : IExcelService
{
    public async Task<Result> ImportWordsAsync(Stream fileStream, string userId)
    {
        try
        {
            using StreamReader reader = new(fileStream);
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using CsvReader csv = new(reader, config);

            List<WordImportDto> records = new();
            try
            {
                records = csv.GetRecords<WordImportDto>().ToList();
            }
            catch (Exception ex)
            {
                return Result.Failure($"CSV ayrıştırma hatası: {ex.Message}. Lütfen başlıkların WORD ve MEANING olduğundan ve virgül ile ayrıldığından emin olun.");
            }

            if (records == null || records.Count == 0)
            {
                return Result.Failure("Dosya boş veya okunamadı.");
            }

            List<Word> wordsToAdd = new();
            DateTime now = DateTime.UtcNow;

            foreach (WordImportDto record in records)
            {
                if (string.IsNullOrWhiteSpace(record.WORD) || string.IsNullOrWhiteSpace(record.MEANING))
                {
                    continue;
                }

                wordsToAdd.Add(new Word
                {
                    EnglishWord = record.WORD.NormalizeEnglishWord(),
                    TurkishWord = record.MEANING.NormalizeTurkishWord(),
                    UserId = userId,
                    CreatedTime = now,
                    IsLastAnswerCorrect = null,
                    ConsecutiveCorrectCount = 0,
                    ConsecutiveWrongCount = 0,
                    TotalCorrectCount = 0,
                    TotalWrongCount = 0
                });
            }

            if (wordsToAdd.Count==0)
            {
                return Result.Failure("Eklenecek geçerli kelime bulunamadı.");
            }

            // Transaction
            using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Words.AddRangeAsync(wordsToAdd);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"Veritabanı hatası: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return Result.Failure($"Beklenmeyen hata: {ex.Message}");
        }
    }


}
