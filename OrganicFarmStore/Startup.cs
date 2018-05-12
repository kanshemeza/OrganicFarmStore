using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace OrganicFarmStore
{
    public class Startup
    {
        public IConfiguration Configuration { get; } // Will help reading our secrets.json file which contains our connection string
        public Startup(IConfiguration configuration) 
        {
            Configuration = configuration;
        }
        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.GetConnectionString("AdventureWorks2016");
            Configuration.GetConnectionString("FinalProjDB");

            //Singleton:Creates one service when the app starts then it will be the same for every request
            //Transient:Constructed and destructed every single request
            //services.AddTransient<SampleService>();

            //These are not part of .NET Core... they are seperate libraries that must be installed via a program called NuGet.
            //Right click on your Project -> Manage NuGet Packages and brwose and install each of the following

            //using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
            //using Microsoft.EntityFrameworkCore;
            //using Microsoft.AspNetCore.Identity;

            services.AddDbContext<IdentityDbContext>(opt => opt.UseInMemoryDatabase("Identities"));

            services.AddIdentity<IdentityUser, IdentityRole>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<IdentityDbContext>() // Will store Identity user info
                .AddDefaultTokenProviders();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //This helps the app to use Cookies for tracking SignIn/SignOut status
            app.UseAuthentication(); 

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
