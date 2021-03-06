using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ploeh.AutoFixture.Kernel;

namespace Ploeh.AutoFixture.Idioms
{
    /// <summary>
    /// Encapsulates a unit test that verifies that a type which overrides the
    /// <see cref="object.GetHashCode()"/> method is implemented correctly with
    /// respect to the rule: calling `x.GetHashCode()` 3 times returns same value.
    /// </summary>
    public class GetHashCodeSuccessiveAssertion : IdiomaticAssertion
    {
        private readonly ISpecimenBuilder builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetHashCodeSuccessiveAssertion"/> class.
        /// </summary>
        /// <param name="builder">
        /// A composer which can create instances required to implement the idiomatic unit test,
        /// such as the owner of the property, as well as the value to be assigned and read from
        /// the member.
        /// </param>
        /// <remarks>
        /// <para>
        /// <paramref name="builder" /> will typically be a <see cref="Fixture" /> instance.
        /// </para>
        /// </remarks>
        public GetHashCodeSuccessiveAssertion(ISpecimenBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            this.builder = builder;
        }

        /// <summary>
        /// Gets the builder supplied by the constructor.
        /// </summary>
        public ISpecimenBuilder Builder
        {
            get { return this.builder; }
        }

        /// <summary>
        /// Verifies that `x.GetHashCode()` 3 times on the same instance of the type returns 
        /// the same value, if the supplied method is an override of the 
        /// <see cref="object.GetHashCode()"/>.
        /// </summary>
        /// <param name="methodInfo">The method to verify</param>
        public override void Verify(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            if (methodInfo.ReflectedType == null ||
                !methodInfo.IsObjectGetHashCodeOverrideMethod())
            {
                // The method is not an override of the Object.GetHashCode() method
                return;
            }

            var instance = this.builder.CreateAnonymous(methodInfo.ReflectedType);

            var results = Enumerable.Range(1, 3)
                .Select(i => instance.GetHashCode())
                .ToArray();

            if (results.Any(result => result != results[0]))
            {
                throw new GetHashCodeOverrideException(string.Format(CultureInfo.CurrentCulture,
                    "The type '{0}' overrides the object.GetHashCode() method incorrectly, " +
                    "calling x.GetHashCode() multiple times on the same instance should return the same value.",
                    methodInfo.ReflectedType.FullName));
            }
        }

    }
}