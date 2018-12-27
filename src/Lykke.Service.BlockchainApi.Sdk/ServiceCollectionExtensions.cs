using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Sdk.Controllers;
using Lykke.Service.BlockchainApi.Sdk.Domain;
using Lykke.Service.BlockchainApi.Sdk.PeriodicalHandlers;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BlockchainApi.Sdk
{
    public static class ServiceCollectionExtensions
    {
        static IServiceCollection AddBlockchainControllers(this IServiceCollection services,
            Type[] controllerTypes)
        {
            services = services ??
                throw new ArgumentNullException(nameof(services));

            controllerTypes = controllerTypes ??
                throw new ArgumentNullException(nameof(controllerTypes));

            var apmInstance = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager))?.ImplementationInstance;
            var apm = apmInstance as ApplicationPartManager ??
                throw new ArgumentException("ApplicationPartManager not found");

            // we will add only specific controllers,
            // so remove SDK application part first
            var sdk = apm.ApplicationParts.FirstOrDefault(part => part.Name == Assembly.GetExecutingAssembly().GetName().Name);
            if (sdk != null)
            {
                apm.ApplicationParts.Remove(sdk);
            }

            // add feature with required controllers
            var featureProvider = new ApplicationFeatureProvider(controllerTypes.Select(type => type.GetTypeInfo()).ToArray());
            apm.FeatureProviders.Add(featureProvider);

            return services;
        }

        static IServiceCollection AddBlockchainCommonRepositories(this IServiceCollection services,
            IReloadingManager<string> connectionStringManager)
        {
            services = services ??
                throw new ArgumentNullException(nameof(services));

            connectionStringManager = connectionStringManager ??
                throw new ArgumentNullException(nameof(connectionStringManager));
            
            EntityMetamodel.Configure(new AnnotationsBasedMetamodelProvider());

            services.AddSingleton(sp => new AssetRepository(connectionStringManager, sp.GetRequiredService<ILogFactory>()));
            services.AddSingleton(sp => new DepositWalletRepository(connectionStringManager, sp.GetRequiredService<ILogFactory>()));
            services.AddSingleton(sp => new OperationRepository(connectionStringManager, sp.GetRequiredService<ILogFactory>()));

            return services;
        }

        /// <summary>
        /// Adds sign-service contract controllers, and registers integration sign-service implementation.
        /// </summary>
        /// <param name="services">Collection of services</param>
        /// <param name="signServiceFactory">Factory of <see cref="IBlockchainSignService"/></param>
        /// <returns></returns>
        public static IServiceCollection AddBlockchainSignService(this IServiceCollection services, 
            Func<IServiceProvider, IBlockchainSignService> signServiceFactory)
        {
            signServiceFactory = signServiceFactory ?? 
                throw new ArgumentNullException(nameof(signServiceFactory));

            services
                .AddSingleton(signServiceFactory)
                .AddBlockchainControllers(new[]
                {
                    typeof(SignController),
                    typeof(WalletsController)
                });

            return services;
        }

        /// <summary>
        /// Adds API contract controllers, and registers integration api-service implementation.
        /// </summary>
        /// <param name="services">Collection of services</param>
        /// <param name="connectionStringManager">Connection string to integration Azure account storage</param>
        /// <param name="apiFactory">Factory of <see cref="IBlockchainApi"/></param>
        /// <returns></returns>
        public static IServiceCollection AddBlockchainApi(this IServiceCollection services, 
            IReloadingManager<string> connectionStringManager, Func<IServiceProvider, IBlockchainApi> apiFactory)
        {
            connectionStringManager = connectionStringManager ??
                throw new ArgumentNullException(nameof(connectionStringManager));

            apiFactory = apiFactory ?? 
                throw new ArgumentNullException(nameof(apiFactory));

            services
                .AddSingleton(apiFactory)
                .AddBlockchainCommonRepositories(connectionStringManager)
                .AddBlockchainControllers(new[]
                {
                    typeof(AddressesController),
                    typeof(AssetsController),
                    typeof(BalancesController),
                    typeof(CapabilitiesController),
                    typeof(ConstantsController),
                    typeof(TransactionsController)
                });

            return services;
        }

        /// <summary>
        /// Registers periodical handler and integration job-service implementation.
        /// </summary>
        /// <param name="services">Collection of services</param>
        /// <param name="connectionStringManager">Connection string to integration Azure account storage</param>
        /// <param name="period">Interval used to call <see cref="IBlockchainJob.TraceDepositsAsync()"/></param>
        /// <param name="jobFactory">Factory of <see cref="IBlockchainJob"/></param>
        /// <returns></returns>
        public static IServiceCollection AddBlockchainJob<T>(this IServiceCollection services, 
            IReloadingManager<string> connectionStringManager, TimeSpan period, Func<IServiceProvider, IBlockchainJob<T>> jobFactory)
        {
            jobFactory = jobFactory ??
                throw new ArgumentNullException(nameof(jobFactory));

            services
                .AddSingleton(jobFactory)
                .AddBlockchainCommonRepositories(connectionStringManager) 
                .AddSingleton(sp => new StateRepository<T>(connectionStringManager, sp.GetRequiredService<ILogFactory>()))
                .AddBlockchainControllers(Type.EmptyTypes) // there are no specific controllers in job
                .AddSingleton<IStartable>(sp =>
                    new DepositHandler<T>(
                        period,
                        sp.GetRequiredService<ILogFactory>(),
                        sp.GetRequiredService<DepositWalletRepository>(),
                        sp.GetRequiredService<OperationRepository>(),
                        sp.GetRequiredService<AssetRepository>(),
                        sp.GetRequiredService<StateRepository<T>>(),
                        sp.GetRequiredService<IBlockchainJob<T>>()
                    )
                );

            return services;
        }
    }
}