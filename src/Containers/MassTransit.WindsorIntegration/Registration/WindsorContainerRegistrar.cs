// Copyright 2007-2019 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
namespace MassTransit.WindsorIntegration.Registration
{
    using System;
    using Castle.MicroKernel.Lifestyle.Scoped;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Courier;
    using Definition;
    using MassTransit.Registration;
    using Saga;
    using ScopeProviders;
    using Scoping;


    public class WindsorContainerRegistrar :
        IContainerRegistrar
    {
        readonly IWindsorContainer _container;

        public WindsorContainerRegistrar(IWindsorContainer container)
        {
            _container = container;
        }

        public void RegisterConsumer<T>()
            where T : class, IConsumer
        {
            _container.Register(
                Component.For<T>()
                    .LifestyleScoped());
        }

        public void RegisterConsumerDefinition<TDefinition, TConsumer>()
            where TDefinition : class, IConsumerDefinition<TConsumer>
            where TConsumer : class, IConsumer
        {
            _container.Register(
                Component.For<IConsumerDefinition<TConsumer>>()
                    .ImplementedBy<TDefinition>());
        }

        public void RegisterSaga<T>()
            where T : class, ISaga
        {
        }

        public void RegisterSagaDefinition<TDefinition, TSaga>()
            where TDefinition : class, ISagaDefinition<TSaga>
            where TSaga : class, ISaga
        {
            _container.Register(
                Component.For<ISagaDefinition<TSaga>>()
                    .ImplementedBy<TDefinition>());
        }

        public void RegisterExecuteActivity<TActivity, TArguments>()
            where TActivity : class, ExecuteActivity<TArguments>
            where TArguments : class
        {
            RegisterActivityIfNotPresent<TActivity>();

            _container.Register(
                Component.For<IExecuteActivityScopeProvider<TActivity, TArguments>>()
                    .ImplementedBy<WindsorExecuteActivityScopeProvider<TActivity, TArguments>>());
        }

        public void RegisterActivityDefinition<TDefinition, TActivity, TArguments, TLog>()
            where TDefinition : class, IActivityDefinition<TActivity, TArguments, TLog>
            where TActivity : class, Activity<TArguments, TLog>
            where TArguments : class
            where TLog : class
        {
            _container.Register(
                Component.For<IActivityDefinition<TActivity, TArguments, TLog>>()
                    .ImplementedBy<TDefinition>());
        }

        public void RegisterExecuteActivityDefinition<TDefinition, TActivity, TArguments>()
            where TDefinition : class, IExecuteActivityDefinition<TActivity, TArguments>
            where TActivity : class, ExecuteActivity<TArguments>
            where TArguments : class
        {
            _container.Register(
                Component.For<IExecuteActivityDefinition<TActivity, TArguments>>()
                    .ImplementedBy<TDefinition>());
        }

        public void RegisterEndpointDefinition<TDefinition, T>(IEndpointSettings<IEndpointDefinition<T>> settings)
            where TDefinition : class, IEndpointDefinition<T>
            where T : class
        {
            _container.Register(
                Component.For<IEndpointDefinition<T>>()
                    .ImplementedBy<TDefinition>(),
                Component.For<IEndpointSettings<IEndpointDefinition<T>>>().Instance(settings));
        }

        public void RegisterRequestClient<T>(RequestTimeout timeout = default)
            where T : class
        {
            _container.Register(Component.For<IRequestClient<T>>().UsingFactoryMethod(kernel =>
            {
                var clientFactory = kernel.Resolve<IClientFactory>();

                var currentScope = CallContextLifetimeScope.ObtainCurrentScope();
                return (currentScope != null)
                    ? clientFactory.CreateRequestClient<T>(kernel.Resolve<ConsumeContext>(), timeout)
                    : clientFactory.CreateRequestClient<T>(timeout);
            }));
        }

        public void RegisterRequestClient<T>(Uri destinationAddress, RequestTimeout timeout = default)
            where T : class
        {
            _container.Register(Component.For<IRequestClient<T>>().UsingFactoryMethod(kernel =>
            {
                var clientFactory = kernel.Resolve<IClientFactory>();

                var currentScope = CallContextLifetimeScope.ObtainCurrentScope();
                return (currentScope != null)
                    ? clientFactory.CreateRequestClient<T>(kernel.Resolve<ConsumeContext>(), destinationAddress, timeout)
                    : clientFactory.CreateRequestClient<T>(destinationAddress, timeout);
            }));
        }

        public void RegisterCompensateActivity<TActivity, TLog>()
            where TActivity : class, CompensateActivity<TLog>
            where TLog : class
        {
            RegisterActivityIfNotPresent<TActivity>();

            _container.Register(
                Component.For<ICompensateActivityScopeProvider<TActivity, TLog>>()
                    .ImplementedBy<WindsorCompensateActivityScopeProvider<TActivity, TLog>>());
        }

        void RegisterActivityIfNotPresent<TActivity>()
            where TActivity : class
        {
            if (!_container.Kernel.HasComponent(typeof(TActivity)))
                _container.Register(Component.For<TActivity>().LifestyleScoped());
        }
    }
}
