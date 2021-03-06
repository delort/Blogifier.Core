﻿using Blogifier.Core.Common;
using Blogifier.Core.Data;
using Blogifier.Core.Data.Interfaces;
using Blogifier.Core.Data.Repositories;
using Blogifier.Core.Middleware;
using Blogifier.Core.Services.Data;
using Blogifier.Core.Services.FileSystem;
using Blogifier.Core.Services.Routing;
using Blogifier.Core.Services.Search;
using Blogifier.Core.Services.Syndication.Rss;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Blogifier.Core
{
    public class Configuration
    {
		public static void InitServices(IServiceCollection services, System.Action<DbContextOptionsBuilder> databaseOptions = null, IConfiguration config = null)
		{   
            if(config != null)
            {
                LoadFromConfigFile(config);
            }
            services.AddTransient<IRssService, RssService>();
			services.AddTransient<IBlogStorage, BlogStorage>();
            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<IDataService, DataService>();           

            // add blog route from ApplicationSettings
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(opt =>
                opt.UseBlogRoutePrefix(new Microsoft.AspNetCore.Mvc.RouteAttribute(ApplicationSettings.BlogRoute)));

            // add route constraint
            services.Configure<RouteOptions>(options =>
                options.ConstraintMap.Add("author", typeof(AuthorRouteConstraint)));

            if (databaseOptions != null)
            {
                ApplicationSettings.DatabaseOptions = databaseOptions;
            }

            AddDatabase(services);

			AddFileProviders(services);
		}

		public static void InitApplication(IApplicationBuilder app, IHostingEnvironment env)
		{
            app.UseMiddleware<AppSettingsLoader>();
			app.UseMiddleware<EmbeddedResources>();     

            ApplicationSettings.WebRootPath = env.WebRootPath;
			ApplicationSettings.ContentRootPath = env.ContentRootPath;
        }

		static void AddDatabase(IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<BlogifierDbContext>(ApplicationSettings.DatabaseOptions);
        }

		static void AddFileProviders(IServiceCollection services)
		{
			var assemblyName = "Blogifier.Core";

			try
			{
				var assembly = Assembly.Load(new AssemblyName(assemblyName));

				services.Configure<RazorViewEngineOptions>(options =>
				{
                    // in some environments provider order matters
                    if(ApplicationSettings.PrependFileProvider)
					    options.FileProviders.Insert(0, new EmbeddedFileProvider(assembly, assemblyName));
                    else
					    options.FileProviders.Add(new EmbeddedFileProvider(assembly, assemblyName));
				});
			}
			catch { }
		}

        static void LoadFromConfigFile(IConfiguration config)
        {
            try
            {
                if (config != null)
                {
                    if (!string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")))
                        ApplicationSettings.ConnectionString = config.GetConnectionString("DefaultConnection");

                    var section = config.GetSection("Blogifier");
                    if (section != null)
                    {
                        // system settings

                        if (section["BlogRoute"] != null)
                            ApplicationSettings.BlogRoute = section.GetValue<string>("BlogRoute");

                        if (section["SingleBlog"] != null)
                            ApplicationSettings.SingleBlog = section.GetValue<bool>("SingleBlog");

                        if (section["UseInMemoryDatabase"] != null)
                            ApplicationSettings.UseInMemoryDatabase = section.GetValue<bool>("UseInMemoryDatabase");

                        if (section["ConnectionString"] != null)
                            ApplicationSettings.ConnectionString = section.GetValue<string>("ConnectionString");

                        if (section["EnableLogging"] != null)
                            ApplicationSettings.EnableLogging = section.GetValue<bool>("EnableLogging");

                        if (section["BlogStorageFolder"] != null)
                            ApplicationSettings.BlogStorageFolder = section.GetValue<string>("BlogStorageFolder");

                        if (section["SupportedStorageFiles"] != null)
                            ApplicationSettings.SupportedStorageFiles = section.GetValue<string>("SupportedStorageFiles");

                        if (section["AdminTheme"] != null)
                            ApplicationSettings.AdminTheme = section.GetValue<string>("AdminTheme");

                        if (section["BlogTheme"] != null)
                            ApplicationSettings.BlogTheme = section.GetValue<string>("BlogTheme");

                        // applicatin settings

                        if (section["Title"] != null)
                            ApplicationSettings.Title = section.GetValue<string>("Title");

                        if (section["Description"] != null)
                            ApplicationSettings.Description = section.GetValue<string>("Description");

                        if (section["ItemsPerPage"] != null)
                            ApplicationSettings.ItemsPerPage = section.GetValue<int>("ItemsPerPage");


                        if (section["ProfileAvatar"] != null)
                            ApplicationSettings.ProfileAvatar = section.GetValue<string>("ProfileAvatar");

                        if (section["ProfileLogo"] != null)
                            ApplicationSettings.ProfileLogo = section.GetValue<string>("ProfileLogo");

                        if (section["ProfileImage"] != null)
                            ApplicationSettings.ProfileImage = section.GetValue<string>("ProfileImage");

                        if (section["PostImage"] != null)
                            ApplicationSettings.PostImage = section.GetValue<string>("PostImage");

                        // troubleshooting

                        if (section["AddContentTypeHeaders"] != null)
                            ApplicationSettings.AddContentTypeHeaders = section.GetValue<bool>("AddContentTypeHeaders");

                        if (section["AddContentLengthHeaders"] != null)
                            ApplicationSettings.AddContentLengthHeaders = section.GetValue<bool>("AddContentLengthHeaders");

                        if (section["PrependFileProvider"] != null)
                            ApplicationSettings.PrependFileProvider = section.GetValue<bool>("PrependFileProvider");
                    }
                }
            }
            catch { }
        }
    }
}