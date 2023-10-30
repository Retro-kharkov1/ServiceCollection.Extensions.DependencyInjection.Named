using Analytics.Linq.Core.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using ServiceCollection.Extensions.DependencyInjection.Named;
using DependencyInjection.Named.Test.Models;

namespace Analytics.Linq.Core.Test
{
    public class DIInjectTest
    {

        IServiceProvider ServiceProvider { get; set; }
        [SetUp]
        public void Setup()
        {
            ServiceProvider = InitClient(new Microsoft.Extensions.DependencyInjection.ServiceCollection())
                .AddNamedInjectedServices()
           .BuildServiceProvider();
        }

        protected IServiceCollection InitClient(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddNamedScoped<IRepository, RepositoryOne>(Consts.Name1);
            serviceDescriptors.AddNamedScoped<IRepository, RepositoryTwo>(Consts.Name2);
            serviceDescriptors.AddScoped<IBusinesOne, BusinesRepOne>();
            serviceDescriptors.AddScoped<IBusinesTwo, BusinesRepTwo>();
            serviceDescriptors.AddNamedScoped<IBusines, BusinesRepTwo>(Consts.Name2);
            serviceDescriptors.AddNamedScoped<IRepository>((sp) => new Repository(Consts.CustomName1), Consts.CustomName1);
            serviceDescriptors.AddScoped<IBusines, BusinesCustom>();

            serviceDescriptors.AddScoped<IBusinesProp, BusinesPropRepOne>();
            serviceDescriptors.AddNamedScoped<IBusinesPropTwo, BusinesPropRepTwo>(Consts.Name2);
            serviceDescriptors.AddScoped<IBusinesPropCustom, BusinesPropCustom>();
            return serviceDescriptors;
        }

        [Test]
        public void Constructor_NameInject_Success()
        {
           var data = this.ServiceProvider.GetRequiredService<IBusinesOne>();
           
            Assert.That(data.RepositoryName == Consts.Name1);
        }

        [Test]
        public void Constructor_NameInject2_Success()
        {
            var data = this.ServiceProvider.GetNamedService<IBusines>(Consts.Name2);

            Assert.That(data.RepositoryName == Consts.Name2);
        }

        [Test]
        public void Constructor_CustomInject_Success()
        {
            var data = this.ServiceProvider.GetRequiredService<IBusines>();

            Assert.That(data.RepositoryName == Consts.CustomName1);
        }

        
        [Test]
        public void Property_NameInject_Success()
        {
            var data = this.ServiceProvider.GetRequiredService<IBusinesProp>();

            Assert.That(data.RepositoryName == Consts.Name1);
        }

        [Test]
        public void Property_NameInject2_Success()
        {
            var data = this.ServiceProvider.GetNamedService<IBusinesPropTwo>(Consts.Name2);

            Assert.That(data.RepositoryName == Consts.Name2);
        }

        [Test]
        public void Property_CustomInject_Success()
        {
            var data = this.ServiceProvider.GetRequiredService<IBusinesPropCustom>();

            Assert.That(data.RepositoryName == Consts.CustomName1);
        }
    }
}