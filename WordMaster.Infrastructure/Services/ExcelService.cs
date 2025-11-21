using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore.Storage;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Extensions;
using WordMaster.Domain.Results;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class ExcelService(AppDbContext context) : IExcelService
{
    public async Task<ServiceResult> ImportWordsAsync(Stream fileStream, Guid userId)
    {
        try
        {
            using StreamReader reader = new(fileStream);
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using CsvReader csv = new(reader, config);

            // Başlığı oku
            if (!await csv.ReadAsync() || !csv.ReadHeader())
            {
                return ServiceResult.Failure("Dosya boş veya başlık satırı okunamadı.", HttpStatusCode.BadRequest);
            }

            string[]? headers = csv.HeaderRecord;
            if (headers == null)
            {
                return ServiceResult.Failure("Başlıklar okunamadı.", HttpStatusCode.BadRequest);
            }

            // WORD ve MEANING sütunlarını dinamik olarak bul
            // Önce tam eşleşme ara, yoksa içerik kontrolü yap
            string? wordHeader = headers.FirstOrDefault(h => h.Trim().Equals("WORD", StringComparison.OrdinalIgnoreCase));
            string? meaningHeader = headers.FirstOrDefault(h => h.Trim().Equals("MEANING", StringComparison.OrdinalIgnoreCase));

            wordHeader ??= headers.FirstOrDefault(h => h.Trim().Contains("WORD", StringComparison.OrdinalIgnoreCase));

            meaningHeader ??= headers.FirstOrDefault(h => h.Trim().Contains("MEANING", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(wordHeader) || string.IsNullOrEmpty(meaningHeader))
            {
                return ServiceResult.Failure("CSV dosyasında 'WORD' ve 'MEANING' sütunları bulunamadı (veya bunları içeren sütunlar).", HttpStatusCode.BadRequest);
            }

            List<Word> wordsToAdd = new();
            DateTime now = DateTime.UtcNow;

            while (await csv.ReadAsync())
            {
                string? wordVal = csv.GetField(wordHeader);
                string? meaningVal = csv.GetField(meaningHeader);

                if (string.IsNullOrWhiteSpace(wordVal) || string.IsNullOrWhiteSpace(meaningVal))
                {
                    continue;
                }

                // Kelime ve anlamı temizle
                string cleanWord = wordVal.Trim().Trim('"').NormalizeEnglishWord();

                // Virgülleri boşluk yap, sonra birden fazla boşluğu tek boşluğa indir
                string tempMeaning = meaningVal.Trim().Trim('"').Replace(",", " ");
                string cleanMeaning = Regex.Replace(tempMeaning, @"\s+", " ").NormalizeTurkishWord();

                if (string.IsNullOrWhiteSpace(cleanWord) || string.IsNullOrWhiteSpace(cleanMeaning))
                {
                    continue;
                }

                wordsToAdd.Add(new Word
                {
                    EnglishWord = cleanWord,
                    TurkishWord = cleanMeaning,
                    UserId = userId,
                    CreatedTime = now,
                    IsLastAnswerCorrect = null,
                    ConsecutiveCorrectCount = 0,
                    ConsecutiveWrongCount = 0,
                    TotalCorrectCount = 0,
                    TotalWrongCount = 0
                });
            }

            if (wordsToAdd.Count == 0)
            {
                return ServiceResult.Failure("Eklenecek geçerli kelime bulunamadı.", HttpStatusCode.BadRequest);
            }

            // Transaction
            using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Words.AddRangeAsync(wordsToAdd);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.SuccessAsCreated();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Failure($"Veritabanı hatası: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"Beklenmeyen hata: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
