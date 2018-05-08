using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Uno.UI.Demo.AspnetShell
{
	public class Startup
	{
		private const string PlaygroundHost = "playground.platform.uno";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
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

			// Static content
			app.UseStaticFiles(new StaticFileOptions
			{
				ContentTypeProvider = CreateContentTypeProvider(),
				OnPrepareResponse = ConfigureCacheControl,
			});

			// MVC (dynamic) content
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		private static void ConfigureCacheControl(StaticFileResponseContext ctx)
		{
			if (ctx.File.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			{
				// Cache managed files for a year, based on this update:
				// https://github.com/nventive/Uno.Wasm.Bootstrap/commit/f4859452c715c54ac40b968a303a242b0399d59a
				ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
			}
		}

		private FileExtensionContentTypeProvider CreateContentTypeProvider()
		{
			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".wasm"] = "application/wasm";
			provider.Mappings[".dll"] = "application/octet-stream";
			return provider;
		}
	}
}
