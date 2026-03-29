using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Storage;
using System.IO;
namespace CvManagement.Services;

public class SupabaseStorageService
{
    private readonly Supabase.Client _client;
    private readonly string _bucket;

    public SupabaseStorageService(IOptions<SupabaseSettings> settings)
    {
        var options = settings.Value;
        _bucket = options.Bucket;
        _client = new Supabase.Client(options.Url, options.ApiKey);
        // The client will be initialized on first use.
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided");

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";

        // Read the file into a byte array
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        // Upload to Supabase Storage
        // The Upload method returns a FileObject, but we don't need it.
        await _client.Storage.From(_bucket).Upload(fileBytes, fileName);

        // Get the public URL (bucket must be public)
        var publicUrl = _client.Storage.From(_bucket).GetPublicUrl(fileName);
        return publicUrl;
    }

    // Optional: delete a file by its public URL or file name
    public async Task DeleteFileAsync(string fileUrlOrName)
    {
        var fileName = Path.GetFileName(fileUrlOrName);
        await _client.Storage.From(_bucket).Remove(new List<string> { fileName });
    }
}