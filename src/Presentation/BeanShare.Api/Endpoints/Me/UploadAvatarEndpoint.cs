using BeanShare.Application.Abstractions;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Me;

public sealed class UploadAvatarEndpoint : EndpointWithoutRequest<UploadAvatarResponse>
{
    private readonly IUserContext _userContext;
    private readonly IWebHostEnvironment _environment;

    public UploadAvatarEndpoint(IUserContext userContext, IWebHostEnvironment environment)
    {
        _userContext = userContext;
        _environment = environment;
    }

    public override void Configure()
    {
        Post("/api/me/avatar");
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Upload user avatar";
            s.Description = "Upload a profile picture for the current user. Accepts jpg, png, gif, webp formats. Max 5MB.";
            s.Response<UploadAvatarResponse>(200, "Avatar uploaded successfully");
            s.Response(400, "Invalid file format or size");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var file = Files.FirstOrDefault();
        if (file is null)
        {
            await SendAsync(new UploadAvatarResponse(false, null, "No file provided"), 400, ct);
            return;
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            await SendAsync(new UploadAvatarResponse(false, null, "File size exceeds 5MB limit"), 400, ct);
            return;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            await SendAsync(new UploadAvatarResponse(false, null, "Invalid file format. Allowed: jpg, png, gif, webp"), 400, ct);
            return;
        }

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            await SendAsync(new UploadAvatarResponse(false, null, "Invalid content type"), 400, ct);
            return;
        }

        try
        {
            var userId = _userContext.CurrentUserId.Value;
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "avatars");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var fileName = $"{userId}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            foreach (var ext in allowedExtensions)
            {
                var existingPath = Path.Combine(uploadsPath, $"{userId}{ext}");
                if (File.Exists(existingPath))
                {
                    File.Delete(existingPath);
                }
            }

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            var avatarUrl = $"/uploads/avatars/{fileName}";
            await SendOkAsync(new UploadAvatarResponse(true, avatarUrl, null), ct);
        }
        catch (Exception)
        {
            await SendAsync(new UploadAvatarResponse(false, null, "Failed to upload avatar. Please try again."), 500, ct);
        }
    }
}

public sealed record UploadAvatarResponse(bool Success, string? AvatarUrl, string? Error);
