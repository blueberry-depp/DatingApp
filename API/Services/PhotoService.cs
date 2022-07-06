using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        // We're going to need to have access to cloudinary so we create constructor.
        private readonly Cloudinary _cloudinary;
        // The way that we get our configuration when we've set up a class to store our configuration is we use the IOptions interface.
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            // This is a cloudinary account that we're going to create here,
            // when we add parameters inside parentheses in this way and it
            // doesn't take the configuration as an object, then we have to get the ordering correct.
            var acc = new Account
                (
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
                );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            // Check to see if we have something in file parameter.
            if (file.Length > 0)
            {
                // Add the logic to upload our file to cloudinary.
                // using: our stream is something that we're going to want to dispose of as soon as we're finished with this method.
                // Getting the file as stream of data.
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                // All of the important information is going to be contained in the upload result when it comes back from cloudinary.
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            return uploadResult;

        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result;
        }
    }
}
