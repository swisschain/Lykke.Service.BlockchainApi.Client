using System;
using System.Linq;
using System.Reflection;
using Autofac;
using FluentValidation;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Contract.Testing;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.BlockchainApi.Sdk.Controllers;
using Lykke.Service.BlockchainApi.Sdk.Domain.Assets;
using Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets;
using Lykke.Service.BlockchainApi.Sdk.Domain.Operations;
using Lykke.Service.BlockchainApi.Sdk.Domain.State;
using Lykke.Service.BlockchainApi.Sdk.Models;
using Lykke.Service.BlockchainApi.Sdk.PeriodicalHandlers;
using Lykke.Service.BlockchainApi.Sdk.Validation;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Mvc;
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
            var apm = (ApplicationPartManager)apmInstance;

            // we will add only specific controllers,
            // so remove SDK application part first
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var sdk = apm.ApplicationParts.FirstOrDefault(part => part.Name == assemblyName.Name);
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

        static IServiceCollection AddChaos(this IServiceCollection services,
            ChaosSettings chaosSettings)
        {
            if (chaosSettings != null)
            {
                services.AddSingleton<IChaosKitty>(sp => new ChaosKitty(chaosSettings.StateOfChaos));
            }

            return services;
        }

        static IServiceCollection ConfigureInvalidModelStateResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                    new BadRequestObjectResult(ErrorResponseFactory.Create(actionContext.ModelState));
            });

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
                })
                .AddTransient<IValidator<SignTransactionRequest>, SignTransactionRequestValidator>()
                .ConfigureInvalidModelStateResponse();

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
            IReloadingManager<string> connectionStringManager, 
            Func<IServiceProvider, IBlockchainApi> apiFactory,
            ChaosSettings chaosSettings = null)
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
                    typeof(TestingController),
                    typeof(TransactionsController)
                })
                .AddChaos(chaosSettings)
                .AddTransient<IValidator<BuildSingleTransactionRequest>, BuildSingleTransactionRequestValidator>()
                .AddTransient<IValidator<BuildTransactionWithManyInputsRequest>, BuildTransactionWithManyInputsRequestValidator>()
                .AddTransient<IValidator<BuildTransactionWithManyOutputsRequest>, BuildTransactionWithManyOutputsRequestValidator>()
                .AddTransient<IValidator<BroadcastTransactionRequest>, BroadcastTransactionRequestValidator>()
                .AddTransient<IValidator<CreateAssetRequest>, CreateAssetRequestValidator>()
                .AddTransient<IValidator<TestingTransferRequest>, TestingTransferRequestValidator>()
                .ConfigureInvalidModelStateResponse();

            return services;
        }

        /// <summary>
        /// Registers periodical handler and integration job-service implementation.
        /// </summary>
        /// <typeparam name="TState">Type of object to keep state between calls of <see cref="IBlockchainJob.TraceDepositsAsync()"/>.</typeparam>  
        /// <param name="services">Collection of services</param>
        /// <param name="connectionStringManager">Connection string to integration Azure account storage</param>
        /// <param name="period">Interval used to call <see cref="IBlockchainJob.TraceDepositsAsync()"/></param>
        /// <param name="jobFactory">Factory of <see cref="IBlockchainJob"/></param>
        /// <returns></returns>
        public static IServiceCollection AddBlockchainJob<TState>(this IServiceCollection services, 
            IReloadingManager<string> connectionStringManager, 
            TimeSpan period, 
            Func<IServiceProvider, IBlockchainJob<TState>> jobFactory,
            ChaosSettings chaosSettings = null)
        {
            jobFactory = jobFactory ??
                throw new ArgumentNullException(nameof(jobFactory));

            services
                .AddSingleton(jobFactory)
                .AddBlockchainCommonRepositories(connectionStringManager) 
                .AddSingleton(sp => new StateRepository<TState>(connectionStringManager, sp.GetRequiredService<ILogFactory>()))
                .AddBlockchainControllers(Type.EmptyTypes) // there are no specific controllers in job
                .AddSingleton<IStartable>(sp =>
                    new DepositHandler<TState>(
                        period,
                        sp.GetRequiredService<ILogFactory>(),
                        sp.GetRequiredService<DepositWalletRepository>(),
                        sp.GetRequiredService<OperationRepository>(),
                        sp.GetRequiredService<AssetRepository>(),
                        sp.GetRequiredService<StateRepository<TState>>(),
                        sp.GetRequiredService<IBlockchainJob<TState>>(),
                        sp.GetService<IChaosKitty>()
                    )
                )
                .AddChaos(chaosSettings);

            return services;
        }
    }
}