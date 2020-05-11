﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using PhotosApp.Areas.Identity.Data;
using PhotosApp.Services;
using PhotosApp.Services.Authorization;
using PhotosApp.Services.TicketStores;

[assembly: HostingStartup(typeof(PhotosApp.Areas.Identity.IdentityHostingStartup))]
namespace PhotosApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                //services.AddDbContext<UsersDbContext>(options =>
                //    options.UseSqlite(
                //        context.Configuration.GetConnectionString("UsersDbContextConnection")));
                //services.AddDbContext<TicketsDbContext>(options =>
                //    options.UseSqlite(
                //        context.Configuration.GetConnectionString("TicketsDbContextConnection")));


                //services.AddDefaultIdentity<PhotosAppUser>(options => options.SignIn.RequireConfirmedAccount = false)
                //    .AddRoles<IdentityRole>()
                //    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>()
                //    .AddEntityFrameworkStores<UsersDbContext>()
                //    .AddPasswordValidator<UsernameAsPasswordValidator<PhotosAppUser>>()
                //    .AddErrorDescriber<RussianIdentityErrorDescriber>();

                //services.AddScoped<IPasswordHasher<PhotosAppUser>, SimplePasswordHasher<PhotosAppUser>>();
                //services.Configure<IdentityOptions>(options =>
                //{
                //    options.Password.RequireDigit = false;
                //    options.Password.RequireLowercase = true;
                //    options.Password.RequireNonAlphanumeric = false;
                //    options.Password.RequireUppercase = false;
                //    options.Password.RequiredLength = 6;
                //    options.Password.RequiredUniqueChars = 1;
                //});

                //services.AddTransient<EntityTicketStore>();
                //services.ConfigureApplicationCookie(options =>
                //{
                //    var serviceProvider = services.BuildServiceProvider();
                //    //options.SessionStore = serviceProvider.GetRequiredService<EntityTicketStore>();
                //    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                //    options.Cookie.Name = "PhotosApp.Auth";
                //    options.Cookie.HttpOnly = true;
                //    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                //    options.LoginPath = "/Identity/Account/Login";
                //    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                //    options.SlidingExpiration = true;
                //});

                //services.ConfigureExternalCookie(options =>
                //{
                //    options.Cookie.Name = "PhotosApp.Auth.External";
                //    options.Cookie.HttpOnly = true;
                //    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                //    options.SlidingExpiration = true;
                //});


                //services.AddAuthentication()
                //    //.AddGoogle("Google", options =>
                //    //{
                //    //    options.ClientId = context.Configuration["Authentication:Google:ClientId"];
                //    //    options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
                //    //})
                //    .AddOpenIdConnect(
                //        authenticationScheme: "Google",
                //        displayName: "Google",
                //        options =>
                //        {
                //            options.Authority = "https://accounts.google.com/";
                //            options.ClientId = context.Configuration["Authentication:Google:ClientId"];
                //            options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];

                //            options.CallbackPath = "/signin-google";
                //            options.SignedOutCallbackPath = "/signout-callback-google";
                //            options.RemoteSignOutPath = "/signout-google";

                //            options.Scope.Add("email");

                //            //options.SaveTokens = true;
                //        });


                //services.AddAuthentication()
                //    .AddJwtBearer(options =>
                //    {
                //        options.RequireHttpsMetadata = false;
                //        options.TokenValidationParameters = new TokenValidationParameters
                //        {
                //            ValidateIssuer = false,
                //            ValidateAudience = false,
                //            ValidateLifetime = true,
                //            ClockSkew = TimeSpan.Zero,
                //            ValidateIssuerSigningKey = true,
                //            IssuerSigningKey = TemporaryTokens.SigningKey
                //        };
                //        options.Events = new JwtBearerEvents
                //        {
                //            OnMessageReceived = c =>
                //            {
                //                c.Token = c.Request.Cookies[TemporaryTokens.CookieName];
                //                return Task.CompletedTask;
                //            }
                //        };
                //    });


                const string oidcAuthority = "https://localhost:7001";
                var oidcConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{oidcAuthority}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());

                services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(oidcConfigurationManager);

                services.AddAuthentication(options =>
                    {
                        // NOTE: Схема, которую внешние провайдеры будут использовать для сохранения данных о пользователе
                        // NOTE: Так как значение совпадает с DefaultScheme, то эту настройку можно не задавать
                        options.DefaultSignInScheme = "Cookie";
                        // NOTE: Схема, которая будет вызываться, если у пользователя нет доступа
                        options.DefaultChallengeScheme = "Passport";
                        // NOTE: Схема на все остальные случаи жизни
                        options.DefaultScheme = "Cookie";
                    })
                    .AddCookie("Cookie", options =>
                    {
                        // NOTE: Пусть у куки будет имя, которое расшифровывается на странице «Decode»
                        options.Cookie.Name = "PhotosApp.Auth";
                    });

                services.AddAuthentication()
                    .AddOpenIdConnect("Passport", "Паспорт", options =>
                    {
                        options.Authority = oidcAuthority;

                        options.ClientId = "Photos App by OIDC";
                        options.ClientSecret = "secret";
                        options.ResponseType = "code";

                        // NOTE: oidc и profile уже добавлены по-умолчанию
                        options.Scope.Add("email");
                        options.Scope.Add("photos_app");
                        options.Scope.Add("photos_service");
                        options.Scope.Add("offline_access");

                        options.CallbackPath = "/signin-passport";
                        options.SignedOutCallbackPath = "/signout-callback-passport";

                        options.ConfigurationManager = oidcConfigurationManager;

                        // NOTE: все эти проверки токена выполняются по умолчанию, указаны для ознакомления
                        options.TokenValidationParameters.ValidateIssuer = true; // проверка издателя
                        options.TokenValidationParameters.ValidateAudience = true; // проверка получателя
                        options.TokenValidationParameters.ValidateLifetime = true; // проверка не протух ли
                        options.TokenValidationParameters.RequireSignedTokens = true; // есть ли валидная подпись издателя

                        options.Events = new OpenIdConnectEvents()
                        {
                            OnTokenResponseReceived = context =>
                            {
                                var tokenResponse = context.TokenEndpointResponse;

                                var tokenHandler = new JwtSecurityTokenHandler();
                                if (tokenResponse.AccessToken != null)
                                {
                                    var accessToken = tokenHandler.ReadToken(tokenResponse.AccessToken);
                                }
                                if (tokenResponse.IdToken != null)
                                {
                                    var idToken = tokenHandler.ReadToken(tokenResponse.IdToken);
                                }
                                if (tokenResponse.RefreshToken != null)
                                {
                                    // NOTE: Это не JWT-токен
                                    var refreshToken = tokenResponse.RefreshToken;
                                }

                                return Task.CompletedTask;
                            }
                        };

                        options.SaveTokens = true;
                    });


                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.AddPolicy(
                        "Beta",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.RequireClaim("testing", "beta");
                        });
                    options.AddPolicy(
                        "CanAddPhoto",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.RequireClaim("subscription", "paid");
                        });
                    //options.AddPolicy(
                    //    "MustOwnPhoto",
                    //    policyBuilder =>
                    //    {
                    //        policyBuilder.RequireAuthenticatedUser();
                    //        policyBuilder.AddRequirements(new MustOwnPhotoRequirement());
                    //    });
                    options.AddPolicy(
                        "Dev",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            //policyBuilder.RequireRole("Dev");
                            //policyBuilder.AddAuthenticationSchemes(
                            //    JwtBearerDefaults.AuthenticationScheme,
                            //    IdentityConstants.ApplicationScheme);
                        });
                });

                //services.AddScoped<IAuthorizationHandler, MustOwnPhotoHandler>();


                //services.AddTransient<IEmailSender, SimpleEmailSender>(serviceProvider =>
                //    new SimpleEmailSender(
                //        serviceProvider.GetRequiredService<ILogger<SimpleEmailSender>>(),
                //        serviceProvider.GetRequiredService<IWebHostEnvironment>(),
                //        context.Configuration["SimpleEmailSender:Host"],
                //        context.Configuration.GetValue<int>("SimpleEmailSender:Port"),
                //        context.Configuration.GetValue<bool>("SimpleEmailSender:EnableSSL"),
                //        context.Configuration["SimpleEmailSender:UserName"],
                //        context.Configuration["SimpleEmailSender:Password"]
                //    ));
            });
        }
    }
}