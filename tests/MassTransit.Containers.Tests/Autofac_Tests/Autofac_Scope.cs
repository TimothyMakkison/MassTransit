namespace MassTransit.Containers.Tests.Autofac_Tests
{
    using System.Threading.Tasks;
    using Autofac;
    using Common_Tests;
    using NUnit.Framework;


    [TestFixture]
    public class Autofac_Scope :
        Common_Scope
    {
        readonly IContainer _container;

        public Autofac_Scope()
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransit(x =>
            {
                x.AddBus(provider => BusControl);
            });

            _container = builder.Build();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _container.DisposeAsync();
        }

        protected override ISendEndpointProvider GetSendEndpointProvider()
        {
            return _container.Resolve<ISendEndpointProvider>();
        }

        protected override IPublishEndpoint GetPublishEndpoint()
        {
            return _container.Resolve<IPublishEndpoint>();
        }
    }
}
