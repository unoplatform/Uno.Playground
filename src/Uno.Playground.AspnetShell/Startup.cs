using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Uno.UI.Demo.AspnetShell
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddResponseCompression(c =>
			{
				c.EnableForHttps = true;
			});
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
				app.UseExceptionHandler("/error.html");
			}

			app.UseResponseCompression();

			// Static content
			app.UseDefaultFiles();
			app.UseStaticFiles(new StaticFileOptions
			{
				ContentTypeProvider = CreateContentTypeProvider(),
				OnPrepareResponse = ConfigureCacheControl
			});
		}

		private static void ConfigureCacheControl(StaticFileResponseContext ctx)
		{
			if (ctx.File.Name.EndsWith(".clr", StringComparison.OrdinalIgnoreCase))
			{
				// Cache managed files for a year, based on this update:
				// https://github.com/nventive/Uno.Wasm.Bootstrap/commit/f4859452c715c54ac40b968a303a242b0399d59a
				ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
			}
		}

		private FileExtensionContentTypeProvider CreateContentTypeProvider()
		{
			var provider = new FileExtensionContentTypeProvider
			{
				Mappings =
				{
					[".wasm"] = "application/wasm",
					[".clr"] = "application/octet-stream"
				}
			};
			return provider;
		}
	}
}
