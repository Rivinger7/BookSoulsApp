﻿using BookSoulsApp.Application.DatabaseContext;
using BookSoulsApp.Application.Mappers;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Cloudinary;
using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Payment;
using BookSoulsApp.Domain.Enums.SchemaFilter;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using BookSoulsApp.Infrastructure.Services;
using BookSoulsApp.Infrastructure.ThirdPartyServices.Cloudinaries;
using BookSoulsApp.Infrastructure.ThirdPartyServices.Payment;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace BookSoulsApp.Infrastructure.DependencyInjections;
public static class DependencyInjection
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.ConfigRoute();

        services.AddAuthentication();
        services.AddAuthorization();

        services.AddDatabase();
        services.AddServices();

        services.AddSignalR();

        services.AddCloudinary();

        services.AddEnumMemberSerializer();

        services.AddSwaggerGen();
        services.AddJWT();

        services.AddHttpContextAccessor();
    }

    public static void AddDatabase(this IServiceCollection services)
    {
        // Load MongoDB settings from environment variables
        string connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? throw new NotFoundCustomException("Connection String Database is not set in the environment variables");
        string databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") ?? throw new NotFoundCustomException("Database Name is not set in the environment variables");

        // Register the MongoDB settings as a singleton
        MongoDbSetting mongoDbSettings = new()
        {
            ConnectionString = connectionString,
            DatabaseName = databaseName
        };

        // Register the MongoDBSetting with DI
        services.AddSingleton(mongoDbSettings);

        // Register MongoClient as singleton, sharing the connection across all usages
        services.AddSingleton<IMongoClient>(sp =>
        {
            return new MongoClient(mongoDbSettings.ConnectionString);
        });
        //services.AddSingleton<IMongoClient>(_lazyClient.Value);

        // Register IMongoDatabase as a scoped service
        services.AddScoped(sp =>
        {
            IMongoClient client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDbSettings.DatabaseName);
        });

        // Register the MongoDB context (or client)
        services.AddSingleton<BookSooulsDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static void ConfigRoute(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        services.Configure<DataProtectionTokenProviderOptions>(otp =>
        {
            otp.TokenLifespan = TimeSpan.FromMinutes(3);
        });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPublisherService, PublisherService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IJsonWebToken, JsonWebToken>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPayosService, PayosService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IDashboardService, DashboardService>();
    }

    public static void AddCloudinary(this IServiceCollection services)
    {
        // Get the Cloudinary URL from the environment variables loaded by .env
        string? cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
        if (string.IsNullOrEmpty(cloudinaryUrl))
        {
            throw new NotFoundCustomException("Cloudinary URL is not set in the environment variables");
        }

        // Initialize Cloudinary instance
        Cloudinary cloudinary = new(cloudinaryUrl)
        {
            Api = { Secure = true }
        };

        // Register the Cloudinary with DI
        services.AddSingleton(provider => cloudinary);

        // Register Cloudinary in DI container as a scoped service
        services.AddScoped<CloudinaryService>();

        services.AddScoped<ICloudinaryService, CloudinaryService>();
    }

    public static void AddAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationBuilder().AddPolicy("GoogleOrJwt", policy =>
        {
            policy.AddAuthenticationSchemes(GoogleDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
        });
    }

    public static void AddAuthentication(this IServiceCollection services)
    {
        // Config the Google Identity
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        }).AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = Environment.GetEnvironmentVariable("Authentication_Google_ClientId") ?? throw new Exception("Google's ClientId property is not set in environment or not found");
            googleOptions.ClientSecret = Environment.GetEnvironmentVariable("Authentication_Google_ClientSecret") ?? throw new Exception("Google's Client Secret property is not set in environment or not found");

        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                //tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,

                // Các issuer và audience hợp lệ
                //ValidIssuers = [Environment.GetEnvironmentVariable("JWT_ISSUER_PRODUCTION"), "https://localhost:7018"],
                //ValidAudiences = [Environment.GetEnvironmentVariable("JWT_AUDIENCE_PRODUCTION"), Environment.GetEnvironmentVariable("JWT_AUDIENCE_PRODUCTION_BE"), "https://localhost:7018"],

                //ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSettings_SecretKey") ?? throw new Exception("JWT's Secret Mode property is not set in environment or not found"))),

                ClockSkew = TimeSpan.Zero,

                // Đặt RoleClaimType
                RoleClaimType = ClaimTypes.Role
            };

            // Cấu hình SignalR để đọc token
            opt.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Lấy origin từ request
                    string? origin = context.Request.Headers.Origin;

                    //// Các origin được phép truy cập
                    //IEnumerable<string?> securedOrigins = new[]
                    //{
                    //        Environment.GetEnvironmentVariable("SPOTIFY_HUB_CORS_ORIGIN_FE_PRODUCTION"),
                    //        Environment.GetEnvironmentVariable("SPOTIFY_HUB_CORS_ORIGIN_FE_01_DEVELOPMENT"),
                    //        Environment.GetEnvironmentVariable("PAY_OS_CORE_ORIGIN")
                    //}.Where(origin => !string.IsNullOrWhiteSpace(origin));

                    // Kiểm tra xem origin có trong danh sách được phép không
                    if (string.IsNullOrWhiteSpace(origin))
                    {
                        return Task.CompletedTask;
                    }

                    // Query chứa token, sử dụng nó
                    string? accessToken = context.Request.Query["access_token"];
                    PathString path = context.HttpContext.Request.Path;

                    // Các segment được bảo mật
                    //IEnumerable<string?> securedSegments = new[]
                    //{
                    //        Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_COUNT_STREAM_URL"),
                    //        Environment.GetEnvironmentVariable("SPOTIFYPOOL_HUB_PLAYLIST_URL"),

                    //}.Where(segment => !string.IsNullOrWhiteSpace(segment)); // Lọc ra các segment không rỗng

                    // Kiểm tra xem path có chứa segment cần xác thực không
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        //context.Token = accessToken["Bearer ".Length..].Trim(); // SubString()
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };

            // Remove "Bearer " prefix
            // Chỉ remove Bearer prefix khi đang trong môi trường phát triển hoặc debug
            //opt.Events = new JwtBearerEvents
            //{
            //    OnMessageReceived = context =>
            //    {
            //        // Check if the token is present without "Bearer" prefix
            //        if (context.Request.Headers.ContainsKey("Authorization"))
            //        {
            //            var token = context.Request.Headers.Authorization.ToString();
            //            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            //            {
            //                context.Token = token; // Set token without "Bearer" prefix
            //            }
            //        }
            //        return Task.CompletedTask;
            //    }
            //};
        });
    }

    public static void AddJWT(this IServiceCollection services)
    {
        // Config JWT
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BookSouls",
                Version = "v1",
                Description = "",
                TermsOfService = new Uri("https://myfrontend.com/terms"),
                Contact = new OpenApiContact
                {
                    Name = "Support Team",
                    Email = "support@example.com",
                    Url = new Uri("https://myfrontend.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
            });

            if (OperationSystemHandle.IsWindows())
            {
                // Include the XML comments (path to the XML file)
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Path to XML documentation file for the controller project
                var controllerXmlFile = Path.Combine(AppContext.BaseDirectory, "SpotifyPool.xml");
                if (File.Exists(controllerXmlFile))
                {
                    c.IncludeXmlComments(controllerXmlFile);
                }
            }

            // Schema Filter
            c.SchemaFilter<EnumSchemaFilter>();

            #region Add JWT Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            #endregion

            #region Add OAuth2 Authentication
            //c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            //{
            //    Type = SecuritySchemeType.OAuth2,
            //    Description = "OAuth2 Authorization Code Flow",
            //    Flows = new OpenApiOAuthFlows
            //    {
            //        AuthorizationCode = new OpenApiOAuthFlow
            //        {
            //            AuthorizationUrl = new Uri("https://accounts.spotify.com/authorize"), // URL ủy quyền của Spotify
            //            TokenUrl = new Uri("https://accounts.spotify.com/api/token"),       // URL token của Spotify
            //            Scopes = new Dictionary<string, string>
            //            {
            //                { "user-top-read", "Read user's top artists and tracks" },
            //                { "playlist-read-private", "Read private playlists" },
            //                { "playlist-modify-public", "Modify public playlists" },
            //                { "user-library-read", "Read user's library" }
            //            }
            //        }
            //    }
            //});

            //c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = "OAuth2"
            //            }
            //        },
            //        new List<string> { "user-top-read", "playlist-read-private" } // Các scope mặc định
            //    }
            //});
            #endregion
        });
    }

    public static void AddEnumMemberSerializer(this IServiceCollection services)
    {
        //// User
        //BsonSerializer.RegisterSerializer(typeof(UserProduct), new EnumMemberSerializer<UserProduct>());
        //BsonSerializer.RegisterSerializer(typeof(UserRole), new EnumMemberSerializer<UserRole>());
        //BsonSerializer.RegisterSerializer(typeof(UserStatus), new EnumMemberSerializer<UserStatus>());
        //BsonSerializer.RegisterSerializer(typeof(UserGender), new EnumMemberSerializer<UserGender>());

        //// Tracks
        //BsonSerializer.RegisterSerializer(typeof(PlaylistName), new EnumMemberSerializer<PlaylistName>());
        //BsonSerializer.RegisterSerializer(typeof(RestrictionReason), new EnumMemberSerializer<RestrictionReason>());
        //BsonSerializer.RegisterSerializer(typeof(Mood), new EnumMemberSerializer<Mood>());

        //// Cloudinary
        //BsonSerializer.RegisterSerializer(typeof(AudioTagChild), new EnumMemberSerializer<AudioTagChild>());
        //BsonSerializer.RegisterSerializer(typeof(AudioTagParent), new EnumMemberSerializer<AudioTagParent>());
        //BsonSerializer.RegisterSerializer(typeof(ImageTag), new EnumMemberSerializer<ImageTag>());

        //// Album
        //BsonSerializer.RegisterSerializer(typeof(ReleaseStatus), new EnumMemberSerializer<ReleaseStatus>());

        //// Reccomendation
        //BsonSerializer.RegisterSerializer(typeof(Algorithm), new EnumMemberSerializer<Algorithm>());
    }
}
