using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Extensions;
using WordMaster.Domain.Results;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class ExcelService(AppDbContext context, ILogger<ExcelService> logger) : IExcelService
{
    public async Task<ServiceResult<WordImportSummaryResponse>> ImportWordsAsync(Stream fileStream, Guid userId)
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
                return ServiceResult<WordImportSummaryResponse>.Failure("Dosya boş veya başlık satırı okunamadı.", HttpStatusCode.BadRequest);
            }

            string[]? headers = csv.HeaderRecord;
            if (headers == null)
            {
                return ServiceResult<WordImportSummaryResponse>.Failure("Başlıklar okunamadı.", HttpStatusCode.BadRequest);
            }

            // WORD ve MEANING sütunlarını dinamik olarak bul
            // Önce tam eşleşme ara, yoksa içerik kontrolü yap
            string? wordHeader = headers.FirstOrDefault(h => h.Trim().Equals("WORD", StringComparison.OrdinalIgnoreCase));
            string? meaningHeader = headers.FirstOrDefault(h => h.Trim().Equals("MEANING", StringComparison.OrdinalIgnoreCase));

            wordHeader ??= headers.FirstOrDefault(h => h.Trim().Contains("WORD", StringComparison.OrdinalIgnoreCase));

            meaningHeader ??= headers.FirstOrDefault(h => h.Trim().Contains("MEANING", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(wordHeader) || string.IsNullOrEmpty(meaningHeader))
            {
                return ServiceResult<WordImportSummaryResponse>.Failure("CSV dosyasında 'WORD' ve 'MEANING' sütunları bulunamadı (veya bunları içeren sütunlar).", HttpStatusCode.BadRequest);
            }

            // Kullanıcının mevcut kelimelerini çek (Duplicate kontrolü için)
            List<string> existingWords = await context.Words
                .Where(x => x.UserId == userId && x.EnglishWord != null)
                .Select(x => x.EnglishWord!)
                .ToListAsync();

            HashSet<string> existingWordSet = new(existingWords, StringComparer.OrdinalIgnoreCase);

            List<Word> wordsToAdd = new();
            DateTime now = DateTime.UtcNow;

            WordImportSummaryResponse summary = new();

            while (await csv.ReadAsync())
            {
                summary.TotalProcessed++;

                string? wordVal = csv.GetField(wordHeader);
                string? meaningVal = csv.GetField(meaningHeader);

                if (string.IsNullOrWhiteSpace(wordVal) || string.IsNullOrWhiteSpace(meaningVal))
                {
                    summary.FailedCount++;
                    summary.FailedRows.Add($"Satır {summary.TotalProcessed}: Kelime veya anlam boş.");
                    continue;
                }

                // Kelime ve anlamı temizle
                string cleanWord = wordVal.Trim().Trim('"').NormalizeEnglishWord();

                // Virgülleri boşluk yap, sonra birden fazla boşluğu tek boşluğa indir
                string tempMeaning = meaningVal.Trim().Trim('"').Replace(",", " ");
                string cleanMeaning = Regex.Replace(tempMeaning, @"\s+", " ").NormalizeTurkishWord();

                if (string.IsNullOrWhiteSpace(cleanWord) || string.IsNullOrWhiteSpace(cleanMeaning))
                {
                    summary.FailedCount++;
                    summary.FailedRows.Add($"Satır {summary.TotalProcessed}: Normalize sonrası kelime veya anlam boş kaldı.");
                    continue;
                }

                // Duplicate kontrolü
                if (existingWordSet.Contains(cleanWord))
                {
                    summary.DuplicateCount++;
                    continue;
                }

                // Yeni kelimeyi set'e de ekle ki CSV içinde tekrar ediyorsa eklenmesin
                if (wordsToAdd.Any(w => w.EnglishWord == cleanWord))
                {
                    summary.DuplicateCount++;
                    continue;
                }

                // Set'e ekleyelim ki sonraki satırlarda tekrar kontrol edebilelim (yukarıdaki Any kontrolü yerine set'e eklemek daha performanslı olabilir ama wordsToAdd listesi henüz save edilmediği için set'e eklemek mantıklı)
                // Ancak existingWordSet zaten veritabanındaki kelimeleri tutuyor. CSV içindeki mükerrerleri yakalamak için wordsToAdd listesine bakmak yerine,
                // eklediğimiz kelimeyi existingWordSet'e de ekleyebiliriz.
                existingWordSet.Add(cleanWord);

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

            summary.AddedCount = wordsToAdd.Count;

            if (wordsToAdd.Count == 0)
            {
                logger.LogWarning("Import işlemi tamamlandı ancak eklenecek kelime bulunamadı. User: {UserId}, Total: {Total}, Duplicate: {Duplicate}, Failed: {Failed}",
                    userId, summary.TotalProcessed, summary.DuplicateCount, summary.FailedCount);
                return ServiceResult<WordImportSummaryResponse>.Success(summary, HttpStatusCode.OK);
            }

            using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Words.AddRangeAsync(wordsToAdd);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("Import işlemi başarılı. User: {UserId}, Added: {Added}, Duplicate: {Duplicate}, Failed: {Failed}",
                    userId, summary.AddedCount, summary.DuplicateCount, summary.FailedCount);

                return ServiceResult<WordImportSummaryResponse>.SuccessAsCreated(summary);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Import işlemi sırasında veritabanı hatası. User: {UserId}", userId);
                return ServiceResult<WordImportSummaryResponse>.Failure($"Veritabanı hatası: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Import işlemi sırasında beklenmeyen hata. User: {UserId}", userId);
            return ServiceResult<WordImportSummaryResponse>.Failure($"Beklenmeyen hata: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
