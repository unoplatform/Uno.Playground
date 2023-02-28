
#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.Immutable;
using System.Threading;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO.Compression;
using NuGet.Frameworks;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Reflection.Emit;
using System.Data;

namespace Uno.UI.Demo.Samples
{
	public class Compiler
	{
		private static AssemblyLoadContext _ctx;

		public record CompilationResult
		{
			public Assembly? Assembly;
			public ImmutableArray<Diagnostic> Diagnostics;
			public string EntryPointType;
			public string EntryPointMethod;
			public AssemblyLoadContext LoadContext;
		}

		static int _loadCount;

		public static async Task<CompilationResult> Compile(string source)
		{
			//var temporaryNamespace = $"_{Guid.NewGuid().ToString().Replace("-", "")}";
			//var updatedSource = $"namespace {temporaryNamespace} {{ {source} }}";

			var packages = await ResolveNugetPackages(source);

			var cus = SyntaxFactory.ParseCompilationUnit(source);

			var sourceLanguage = CSharpLanguage.Instance;

			await sourceLanguage.Initialize();

			Compilation compilation = sourceLanguage
			  .CreateLibraryCompilation(assemblyName: "InMemoryAssembly", enableOptimisations: false)
			  .AddReferences(packages.References)
			  .AddSyntaxTrees(new[] { cus.SyntaxTree });

			var driver = CSharpGeneratorDriver.Create(
				packages.Generators,
				parseOptions: null
			);

			driver.RunGeneratorsAndUpdateCompilation(compilation, out compilation, out var driverDiagnostics);

			//var driver = CSharpGeneratorDriver
			//.Create(generator)
			//.WithUpdatedParseOptions(new CSharpParseOptions(
			//	preprocessorSymbols: preprocessorSymbols))
			//.AddAdditionalTexts(ImmutableArray.Create(additionalTexts))
			//.WithUpdatedAnalyzerConfigOptions(options)
			//.RunGeneratorsAndUpdateCompilation(
			//	compilation,
			//	out compilation,
			//	out _, cancellationToken);

			Console.WriteLine($"Got compilation");

			// GetDiagnostics seems to keep running in a CPU Bound loop
			Console.WriteLine($"Got compilation Diagnostics: {compilation.GetDiagnostics().Length}");
			Console.WriteLine($"Got compilation Driver Diagnostics: {driverDiagnostics.Length}");
			Console.WriteLine($"Got compilation DeclarationDiagnostics: {compilation.GetDeclarationDiagnostics().Length}");

			Console.WriteLine($"Emitting assembly...");
			var stream = new MemoryStream();
			var emitResult = compilation.Emit(stream);

			if (emitResult.Success)
			{
				Console.WriteLine($"Got binary assembly: {emitResult.Success}");

				if (compilation.GetEntryPoint(CancellationToken.None) is { } entryPoint)
				{
					_ctx = new AssemblyLoadContext($"test-{_loadCount++}", isCollectible: true);
					_ctx.Unloading += (a) =>
					{
						Console.WriteLine($"Unloading AssemblyLoadContext={a.Name}");
					};

					stream.Position = 0;

					foreach (var packageAssembly in packages.References)
					{
						if (packageAssembly.FilePath != null)
						{
							_ctx.LoadFromAssemblyPath(packageAssembly.FilePath);
						}
					}

					return new CompilationResult()
					{
						Assembly = _ctx.LoadFromStream(stream),
						Diagnostics = emitResult.Diagnostics,
						EntryPointType = entryPoint.ContainingType.ToDisplayString(),
						EntryPointMethod = entryPoint.Name,
						LoadContext = _ctx,
					};

					//return new CompilationResult()
					//{
					//	Assembly = Assembly.Load(stream.ToArray()),
					//	Diagnostics = emitResult.Diagnostics,
					//	EntryPointType = entryPoint.ContainingType.ToDisplayString(),
					//	EntryPointMethod = entryPoint.Name
					//};
				}
				else
				{
					throw new InvalidOperationException($"The source does not contain at top level statement");
				}
			}
			else
			{
				return new CompilationResult()
				{
					Assembly = null,
					Diagnostics = emitResult.Diagnostics
				};
			}
		}

		private static async Task<(PortableExecutableReference[] References, ISourceGenerator[] Generators)> ResolveNugetPackages(string source)
		{
			var references = Regex.Match(source, "//ref:(?<packageId>.*?)@(?<version>.*?)\n");

			List<(string packageId, string version)> packages = new();

			if (references.Success)
			{
				do
				{
					packages.Add(
						(
						references.Groups["packageId"].Value.ToLowerInvariant(),
						references.Groups["version"].Value.ToLowerInvariant()
						)
					);
				}
				while ((references = references.NextMatch()) is { Success: true });
			}

			List<string> assemblies = new();
			List<string> analyzers = new();
			foreach (var (packageId, version) in packages)
			{
				var basePath = Path.Combine(
					Windows.Storage.ApplicationData.Current.LocalFolder.Path,
					".nuget",
					"packages",
					packageId,
					version);

				await DownloadNugetPackage(packageId, version, basePath);

				var packageAssemblies = GetPackageAssemblies(basePath);
				assemblies.AddRange(packageAssemblies.Assemblies);
				analyzers.AddRange(packageAssemblies.Analyzers);
			}

			foreach (var asm in assemblies)
			{
				Console.WriteLine($"Got package assembly: {asm}");
			}

			var sourceGenerators = GetGenerators(analyzers);

			await Console.Out.WriteLineAsync($"Generators: {sourceGenerators.Length}");

			return
			(
				assemblies
				.Select(a => MetadataReference.CreateFromFile(a))
				.ToArray(),
				sourceGenerators
			);
		}

		private static ISourceGenerator[] GetGenerators(List<string> analyzers)
		{
			return analyzers.SelectMany(
				a => 
				{
					Console.WriteLine($"Searching for analyzers in {a}");
					var assembly = Assembly.LoadFile(a);

					return assembly
						.GetTypes()
						.Where(t => t.GetCustomAttribute<GeneratorAttribute>() is object)
						.Select(t => (ISourceGenerator)Activator.CreateInstance(t)!);
				}
			)
			.ToArray();
		}

		private static async Task DownloadNugetPackage(string? packageId, string? version, string basePath)
		{
			var fullPackagePath = Path.Combine(basePath, $"{packageId}.{version}.nupkg");

			if (!File.Exists(fullPackagePath))
			{
				version = version?.Replace("\r", "");

				var url = $"https://api.nuget.org/v3-flatcontainer/{packageId}/{version}/{packageId}.{version}.nupkg";

				Console.WriteLine($"Downloading {url}");
				var client = new HttpClient();
				var inputStream = await client.GetStreamAsync(url);

				Directory.CreateDirectory(basePath);

				using (var outputStream = File.OpenWrite(fullPackagePath))
				{
					await inputStream.CopyToAsync(outputStream);
				}

				ExtractNugetPackage(fullPackagePath);
			}
			else
			{
				Console.WriteLine($"Package {packageId}@{version} already exists locally");
			}
		}

		private record class PackageAssemblies(List<string> Assemblies, List<string> Analyzers);

		private static PackageAssemblies GetPackageAssemblies(string basePath)
		{
			var project = NuGetFramework.Parse("net6.0");

			var libPath = Path.Combine(basePath, "lib");

			var frameworksFolders = Directory
				.GetDirectories(libPath, "*.*", SearchOption.TopDirectoryOnly)
				.Select(s => Path.GetFileName(s));

			Console.WriteLine($"Source folders in [{string.Join(',', frameworksFolders)}]");

			var frameworks = frameworksFolders.Select(s => NuGetFramework.Parse(s))
				.ToList();

			FrameworkReducer reducer = new();

			var nearest = reducer.GetNearest(project, frameworks);
			List<string> assemblies = new();

			if(nearest != null)
			{
				var targetFrameworkPath = Path.Combine(libPath, nearest.GetShortFolderName());

				Console.WriteLine($"Found nearest target framework [{targetFrameworkPath}]");

				assemblies.AddRange(Directory.GetFiles(targetFrameworkPath, "*.dll", SearchOption.TopDirectoryOnly));
			}
			else
			{
				Console.WriteLine($"Unable to compatible target framework in [{string.Join(',', frameworks)}]");
			}

			List<string> analyzers = new();
			if(basePath.Contains("/refit/", StringComparison.OrdinalIgnoreCase))
			{
				string path = Path.Combine(basePath, "analyzers");

				var generator = Directory
					.GetFiles(path, "InterfaceStubGeneratorV1.dll", SearchOption.AllDirectories)
					.FirstOrDefault();

				Console.WriteLine($"Refit generator [{generator}]");

				if (generator != null)
				{
					analyzers.Add(generator);
				}
			}

			return new (assemblies, analyzers);
		}

		private static void ExtractNugetPackage(string fullPackagePath)
		{
			Console.WriteLine($"Extracting {fullPackagePath}");

			if (Path.GetDirectoryName(fullPackagePath) is { Length: > 0 } extractPath)
			{
				using (var file = File.OpenRead(fullPackagePath))
				{
					using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
					{
						archive.ExtractToDirectory(extractPath);
					}
				}
			}
			else
			{
				throw new InvalidOperationException($"Unable to get the directory for {fullPackagePath}");
			}
		}
	}

	public class CSharpLanguage : ILanguageService
	{
		private MetadataReference[] _references;

		public static CSharpLanguage Instance { get; } = new CSharpLanguage();

		private CSharpLanguage()
		{
		}

		public async Task Initialize()
		{
			if (_references != null)
			{
				return;
			}

			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///dotnet-sdk/fileslist.txt"));

			var sdkFiles = await FileIO.ReadLinesAsync(file);

			var tasks = sdkFiles
				.Select(f =>
				{
					async Task<MetadataReference> LoadReference()
					{
						var targetFile = f
						.Replace(".dll", ".clr")
						.Replace("dotnet-sdk-source", "dotnet-sdk")
						.Replace("uno-sdk-source", "dotnet-sdk")
						;

						var sdkFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{targetFile}"));

						using var stream = await sdkFile.OpenReadAsync();
						var streamCopy = new MemoryStream();
						stream.AsStream().CopyTo(streamCopy);

						streamCopy.Position = 0;
						return MetadataReference.CreateFromStream(streamCopy);
					}

					return LoadReference();
				});

			_references = await Task.WhenAll(tasks);
		}

		public Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
		{
			var options = new CSharpCompilationOptions(
				OutputKind.ConsoleApplication,
				optimizationLevel: OptimizationLevel.Release,
				allowUnsafe: true)
				// Disabling concurrent builds allows for the emit to finish.
				.WithConcurrentBuild(false)
				;

			return CSharpCompilation.Create(assemblyName, options: options, references: _references);
		}
	}

}
