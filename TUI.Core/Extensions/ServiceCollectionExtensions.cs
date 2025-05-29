// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ktsu.TUI.Core.Extensions;

/// <summary>
/// Extension methods for configuring TUI services in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds TUI services to the service collection
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddTUI(this IServiceCollection services)
	{
		services.TryAddSingleton<IConsoleProvider, SpectreConsoleProvider>();
		services.TryAddSingleton<IUIApplication, UIApplication>();

		return services;
	}

	/// <summary>
	/// Adds TUI services with a custom console provider
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="consoleProviderFactory">Factory for creating the console provider</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddTUI<TConsoleProvider>(
		this IServiceCollection services,
		Func<IServiceProvider, TConsoleProvider> consoleProviderFactory)
		where TConsoleProvider : class, IConsoleProvider
	{
		services.TryAddSingleton<IConsoleProvider>(consoleProviderFactory);
		services.TryAddSingleton<IUIApplication, UIApplication>();

		return services;
	}

	/// <summary>
	/// Adds TUI services with a specific console provider type
	/// </summary>
	/// <typeparam name="TConsoleProvider">The console provider type</typeparam>
	/// <param name="services">The service collection</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddTUI<TConsoleProvider>(this IServiceCollection services)
		where TConsoleProvider : class, IConsoleProvider
	{
		services.TryAddSingleton<IConsoleProvider, TConsoleProvider>();
		services.TryAddSingleton<IUIApplication, UIApplication>();

		return services;
	}

	/// <summary>
	/// Adds the Spectre.Console provider specifically
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddSpectreConsole(this IServiceCollection services)
	{
		services.TryAddSingleton<IConsoleProvider, SpectreConsoleProvider>();
		return services;
	}
}
