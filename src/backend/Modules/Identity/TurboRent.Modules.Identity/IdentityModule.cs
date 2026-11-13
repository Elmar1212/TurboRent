using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Features.Login;
using TurboRent.Modules.Identity.Features.Password;
using TurboRent.Modules.Identity.Features.Register;
using TurboRent.Modules.Identity.Features.VerifyEmail;
using TurboRent.Modules.Identity.Infrastructure.Email;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Infrastructure.Security;

namespace TurboRent.Modules.Identity;

public static class IdentityModule
{
    /// <summary>
    /// Accounts need a way to deliver verification email. Development and Testing use the local
    /// mailbox; any other environment needs an SMTP relay (<c>Email:Smtp:Host</c>) configured.
    /// Without one, account endpoints stay unmapped rather than accepting sign-ups nobody can verify.
    /// </summary>
    public static bool AccountsEnabled(IHostEnvironment environment, IConfiguration configuration) =>
        IsLocal(environment) || !string.IsNullOrWhiteSpace(configuration[EmailSettings.SmtpHostKey]);

    private static bool IsLocal(IHostEnvironment environment) =>
        environment.IsDevelopment() || environment.IsEnvironment("Testing");

    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("ConnectionStrings:Database must be configured.");
            if (environment?.IsEnvironment("Testing") == true &&
                new Npgsql.NpgsqlConnectionStringBuilder(connection).Database?.StartsWith(
                    "turborent_identity_test_", StringComparison.Ordinal) != true)
                throw new InvalidOperationException("Testing requires an isolated Identity test database.");
            options.UseNpgsql(connection,
                postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));
        });
        return services;
    }

    public static IServiceCollection AddIdentityRegistration(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIdentityPersistence(configuration, environment);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IListingOwnerDirectory, ListingOwnerDirectory>();

        // Services are always registered; the host maps account routes only when AccountsEnabled.
        // Configuration is read lazily because test hosts add settings when the app is built.
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = string.Empty;
            options.Password.RequiredLength = 15;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
            options.SignIn.RequireConfirmedEmail = true;
            options.Tokens.PasswordResetTokenProvider = PasswordResetTokenProviderOptions.ProviderName;
        }).AddRoles<ApplicationRole>().AddEntityFrameworkStores<IdentityDbContext>().AddDefaultTokenProviders()
          .AddTokenProvider<PasswordResetTokenProvider>(PasswordResetTokenProviderOptions.ProviderName);
        services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(24));

        // Keys must survive restarts and be shared by every API instance, or sessions and links break.
        services.AddDataProtection().SetApplicationName("TurboRent.Identity.Local");
        services.AddOptions<KeyManagementOptions>().Configure<IConfiguration, ILoggerFactory>((options, settings, loggers) =>
        {
            var keyPath = settings["Identity:DataProtectionKeysPath"] ?? Path.Combine(LocalIdentityFiles.Root, "keys");
            LocalIdentityFiles.ProtectDirectory(keyPath);
            options.XmlRepository = new FileSystemXmlRepository(new DirectoryInfo(keyPath), loggers);
            if (OperatingSystem.IsWindows()) options.XmlEncryptor = new DpapiXmlEncryptor(false, loggers);
        });
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-TurboRent.Csrf";
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        });

        services.AddScoped<IEmailTransport>(provider =>
            string.IsNullOrWhiteSpace(provider.GetRequiredService<IConfiguration>()[EmailSettings.SmtpHostKey])
                ? ActivatorUtilities.CreateInstance<LocalMailbox>(provider)
                : ActivatorUtilities.CreateInstance<SmtpEmailTransport>(provider));
        services.AddScoped<IdentityEmail>();
        services.AddScoped<IIdentityEmail>(provider => provider.GetRequiredService<IdentityEmail>());
        services.AddScoped<IUserNotifier>(provider => provider.GetRequiredService<IdentityEmail>());

        services.AddSingleton<IdentityLimits>();
        services.AddScoped<AdminTotpService>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<LoginHandler>();
        services.AddSingleton<ApplicationSessionCookie>();
        services.AddScoped<SessionAuthenticator>();
        services.AddScoped<IAuthenticatedUser, AuthenticatedUserResolver>();
        services.AddScoped<IVerifiedListingOwner, VerifiedListingOwner>();
        services.AddScoped<IVerifiedAdmin, VerifiedAdmin>();
        return services;
    }
}
