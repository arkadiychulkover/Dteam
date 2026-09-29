using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DteamBackend.Interfaces
{
    public interface IS3StorageService
    {
        /// <summary>
        /// Завантажити файл у сховище за вказаним ключем.
        /// </summary>
        Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Завантажити масив байтів у сховище за вказаним ключем.
        /// </summary>
        Task<string> UploadAsync(byte[] bytes, string key, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отримати потік файлу, MIME-тип та розмір за ключем.
        /// </summary>
        Task<(Stream Stream, string ContentType, long ContentLength)?> GetFileAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Видалити файл зі сховища за ключем.
        /// </summary>
        Task<bool> DeleteFileAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Перевірити наявність файлу в сховищі за ключем.
        /// </summary>
        Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
