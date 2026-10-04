using System;
using System.Threading.Tasks;
using DependencyInjection.Named.Refit;
using NUnit.Framework;
using Refit;

namespace DependencyInjection.Named.Test.Refit
{
    /// <summary>
    /// Pure unit tests for <see cref="UniqueName"/> - no DI or HTTP involved, only the
    /// synthetic-name string-building logic.
    /// </summary>
    public class UniqueNameTest
    {
        private const string Name = "someName";

        [Test]
        public void ForType_PlainInterface_BuildsExpectedSyntheticName()
        {
            var type = typeof(IPlainApi);

            var result = UniqueName.ForType(type, Name);

            var expected = BuildExpected(type, Name);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ForTypeNamed_PlainInterface_MatchesForType()
        {
            var forTypeNamed = UniqueName.ForTypeNamed<IPlainApi>(Name);
            var forType = UniqueName.ForType(typeof(IPlainApi), Name);

            Assert.That(forTypeNamed, Is.EqualTo(forType));
        }

        [Test]
        public void ForType_NestedInterface_StripsPlusSignAndKeepsOuterName()
        {
            var type = typeof(OuterContainer.INestedApi);

            var result = UniqueName.ForType(type, Name);

            var expected = BuildExpected(type, Name);
            Assert.That(result, Is.EqualTo(expected));

            var plusCount = result.Split('+').Length - 1;
            Assert.That(plusCount, Is.EqualTo(1),
                "The nested type's own '+' separator must be stripped; only the generated-prefix '+' remains.");
        }

        [Test]
        public void ForType_GenericInterface_StartsWithGeneratedPrefixAndEndsWithName()
        {
            var type = typeof(IGenericApi<string>);

            var result = UniqueName.ForType(type, Name);

            var ns = type.Namespace!.Replace(".", string.Empty);
            // IGenericApi<T> is nested inside this test class, so the generated prefix also
            // carries the enclosing type's name (the "+" nesting separator gets stripped).
            Assert.That(result, Does.StartWith($"Refit.Implementation.Generated+{ns}{nameof(UniqueNameTest)}IGenericApi`1[["));
            Assert.That(result, Does.EndWith($", {Name}"));
            Assert.That(result, Does.Contain(type.Assembly.FullName!));
        }

        [Test]
        public void ForType_DifferentNames_ProduceDifferentSyntheticNames()
        {
            var first = UniqueName.ForType(typeof(IPlainApi), "nameOne");
            var second = UniqueName.ForType(typeof(IPlainApi), "nameTwo");

            Assert.That(first, Is.Not.EqualTo(second));
        }

        /// <summary>
        /// Builds the expected synthetic name from <see cref="Type.FullName"/>/<see cref="Type.Namespace"/>
        /// directly (not by re-implementing <see cref="UniqueName.ForType"/>), valid for
        /// non-generic interface types only (plain or nested).
        /// </summary>
        private static string BuildExpected(Type type, string name)
        {
            var ns = type.Namespace!.Replace(".", string.Empty);
            var simpleName = type.FullName!.Substring(type.Namespace!.Length + 1).Replace("+", string.Empty);
            return $"Refit.Implementation.Generated+{ns}{simpleName}, {type.Assembly.FullName}, {name}";
        }

        public interface IPlainApi
        {
            [Get("/plain")]
            Task<string> Get();
        }

        public interface IGenericApi<T>
        {
            [Get("/generic")]
            Task<T> Get();
        }

        public class OuterContainer
        {
            public interface INestedApi
            {
                [Get("/nested")]
                Task<string> Get();
            }
        }
    }
}
