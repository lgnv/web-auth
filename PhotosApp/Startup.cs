using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotosApp.Areas.Identity.Data;
using PhotosApp.Data;
using PhotosApp.Models;
using PhotosApp.Services;

namespace PhotosApp
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;
        private IConfiguration configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvc = services.AddControllersWithViews();
            services.AddRazorPages();
            if (env.IsDevelopment())
                mvc.AddRazorRuntimeCompilation();

            // NOTE: Подключение IHttpContextAccessor, чтобы можно было получать HttpContext там,
            // где это не получается сделать более явно.
            services.AddHttpContextAccessor();

            services.AddDbContext<PhotosDbContext>(o =>
                o.UseSqlite(configuration.GetConnectionString("PhotosDbContextConnection")));
            // NOTE: Вместо Sqlite можно использовать LocalDB от Microsoft или другой SQL Server
            //services.AddDbContext<PhotosDbContext>(o =>
            //    o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=PhotosApp;Trusted_Connection=True;"));

            services.AddScoped<IPhotoRepository, PhotoRepository>();

            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<PhotoEntity, Photo>().ReverseMap();

                cfg.CreateMap<AddPhotoModel, PhotoEntity>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                cfg.CreateMap<EditPhotoModel, PhotoEntity>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            }, new System.Reflection.Assembly[0]);
            
            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                options.SignIn.RequireConfirmedAccount = false;
            });
            
            services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                options.IterationCount = 12000;
            });
            
            services.AddScoped<IPasswordHasher<PhotoAppUser>, SimplePasswordHasher<PhotoAppUser>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Exception");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Photo}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
