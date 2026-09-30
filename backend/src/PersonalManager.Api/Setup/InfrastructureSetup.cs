using Amazon.Runtime;
using Amazon.S3;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;

namespace PersonalManager.Api.Setup;

/// <summary>外部資源的服務註冊：寄信與檔案儲存。未設定時使用本機替代實作。</summary>
public static class InfrastructureSetup
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        var section = configuration.GetSection("Email");
        services.Configure<EmailSettings>(section);

        if ((section.Get<EmailSettings>() ?? new EmailSettings()).IsConfigured)
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
            logger.LogInformation("Email：SMTP 模式");
        }
        else
        {
            services.AddScoped<IEmailService, NoOpEmailService>();
            logger.LogInformation("Email：未設定 SMTP，信件內容只寫入 log");
        }
        return services;
    }

    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

        var s3 = configuration.GetSection("FileStorage:S3").Get<S3StorageSettings>();
        if (s3?.IsConfigured == true)
        {
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                new BasicAWSCredentials(s3.AccessKey, s3.SecretKey),
                new AmazonS3Config { ServiceURL = s3.ServiceUrl, ForcePathStyle = s3.ForcePathStyle }));
            services.AddScoped<IFileStorageProvider, S3FileStorageProvider>();
            logger.LogInformation("檔案儲存：S3 相容 Object Storage");
        }
        else
        {
            services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
            logger.LogInformation("檔案儲存：本機磁碟");
        }
        return services;
    }
}
