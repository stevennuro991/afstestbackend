using Afstest.API.Data;
using Afstest.API.Infrastructure.Filters;
using Afstest.API.Models;
using Afstest.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Afstest.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<HttpGlobalExceptionFilter>();//THis helps catching all exceptions in the controllers
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins, ///This one is for cors errors
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });
//Identity is used in the auth 
            builder.Services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 2;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
           .AddRoles<IdentityRole>()
           .AddSignInManager<SignInManager<User>>()
           .AddEntityFrameworkStores<AppDbContext>()
           .AddDefaultTokenProviders();

            //custom services
            builder.Services.AddScoped<IdentityService>();
            builder.Services.AddScoped<SearchService>();
            builder.Services.AddScoped<TokenService>();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                var jwtOptions = builder.Configuration.GetSection(JwtOptions.JwtConfiguration).Get<JwtOptions>();
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ClockSkew = TimeSpan.FromSeconds(0),
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidIssuer = jwtOptions!.Issuer,
                                    ValidAudience = jwtOptions.Audience,
                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey))
                                };
                            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Afstest API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            var app = builder.Build();

            #region db migration
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

                logger!.LogInformation("connectionString: {0}", builder.Configuration.GetConnectionString("DefaultConnection"));
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Database.Migrate();
            }
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
