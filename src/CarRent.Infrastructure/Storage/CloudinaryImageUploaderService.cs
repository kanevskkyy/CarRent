using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CarRent.Application.Common.Settings;
using CarRent.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace CarRent.Infrastructure.ExternalServices;

public class CloudinaryImageUploaderService : IImageUploaderService
{
    private Cloudinary cloudinary;

    public CloudinaryImageUploaderService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        Account account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadAsync(
        Stream stream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        ImageUploadParams uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            PublicId = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}"
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        DeletionParams deleteParams = new DeletionParams(publicId);
        var result = await cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null)
        {
            throw new Exception($"Cloudinary deletion error: {result.Error.Message}");
        }
    }
}