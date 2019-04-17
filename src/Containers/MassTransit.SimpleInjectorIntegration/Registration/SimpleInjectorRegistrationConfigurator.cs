namespace MassTransit.SimpleInjectorIntegration.Registration
{
    using System;
    using MassTransit.Registration;
    using ScopeProviders;
    using Scoping;
    using SimpleInjector;


    public class SimpleInjectorRegistrationConfigurator :
        RegistrationConfigurator,
        ISimpleInjectorConfigurator
    {
        public SimpleInjectorRegistrationConfigurator(Container container)
            : base(new SimpleInjectorContainerRegistar(container))
        {
            Container = container;

            AddMassTransitComponents(Container);

            Container.RegisterInstance<IRegistrationConfigurator>(this);

            Container.RegisterSingleton(() => CreateRegistration(container.GetInstance<IConfigurationServiceProvider>()));
        }

        public Container Container { get; }

        public void AddBus(Func<IBusControl> busFactory)
        {
            Container.RegisterSingleton(busFactory);

            Container.RegisterSingleton<IBus>(() => Container.GetInstance<IBusControl>());

            Container.Register(() => (ISendEndpointProvider)Container.TryGetInstance<ScopedConsumeContextProvider>()?.GetContext() ??
                Container.GetInstance<IBus>(), Lifestyle.Scoped);

            Container.Register(() => (IPublishEndpoint)Container.TryGetInstance<ScopedConsumeContextProvider>()?.GetContext() ??
                Container.GetInstance<IBus>(), Lifestyle.Scoped);

            Container.RegisterSingleton(() => Container.GetInstance<IBus>().CreateClientFactory());
        }

        static void AddMassTransitComponents(Container container)
        {
            container.Register<ScopedConsumeContextProvider>(Lifestyle.Scoped);

            container.Register(() => container.GetInstance<ScopedConsumeContextProvider>().GetContext(), Lifestyle.Scoped);

            container.RegisterSingleton<IConsumerScopeProvider>(() => new SimpleInjectorConsumerScopeProvider(container));
            container.RegisterSingleton<ISagaRepositoryFactory>(() => new SimpleInjectorSagaRepositoryFactory(container));
            container.RegisterSingleton<IConfigurationServiceProvider>(() => new SimpleInjectorConfigurationServiceProvider(container));
        }
    }
}
