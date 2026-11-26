using System.Linq.Expressions;
using Harmony.Application.Extensions;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Extensions
{
    /// <summary>
    /// <see cref="PredicateBuilder"/> combines two query conditions into one that EF Core can translate,
    /// so the result must have a single parameter even when the inputs use different parameter names.
    /// </summary>
    public class PredicateBuilderTests
    {
        private static readonly Expression<Func<int, bool>> GreaterThanTwo = x => x > 2;
        private static readonly Expression<Func<int, bool>> LessThanFive = y => y < 5;
        private static readonly Expression<Func<int, bool>> LessThanTwo = x => x < 2;
        private static readonly Expression<Func<int, bool>> GreaterThanFive = y => y > 5;

        [Theory]
        [Trait("Requirement", "FR-013")]
        [InlineData(3, true)]
        [InlineData(1, false)]
        [InlineData(6, false)]
        public void And_TwoConditions_MatchesOnlyWhenBothTrue(int value, bool expected)
        {
            var combined = GreaterThanTwo.And(LessThanFive).Compile();

            combined(value).ShouldBe(expected);
        }

        [Fact]
        [Trait("Requirement", "FR-013")]
        public void And_TwoConditions_HasSingleParameter()
        {
            var combined = GreaterThanTwo.And(LessThanFive);

            ParameterNames(combined).ShouldBe(["x"]);
        }

        [Theory]
        [Trait("Requirement", "FR-014")]
        [InlineData(1, true)]
        [InlineData(6, true)]
        [InlineData(3, false)]
        public void Or_TwoConditions_MatchesWhenEitherTrue(int value, bool expected)
        {
            var combined = LessThanTwo.Or(GreaterThanFive).Compile();

            combined(value).ShouldBe(expected);
        }

        [Fact]
        [Trait("Requirement", "FR-014")]
        public void Or_TwoConditions_HasSingleParameter()
        {
            var combined = LessThanTwo.Or(GreaterThanFive);

            ParameterNames(combined).ShouldBe(["x"]);
        }

        /// <summary>
        /// The distinct parameters the combined lambda declares and uses in its body. A leftover parameter
        /// from the right-hand condition would make the expression fail to compile or translate.
        /// </summary>
        private static List<string> ParameterNames(Expression<Func<int, bool>> expression)
        {
            var collector = new ParameterCollector();
            collector.Visit(expression);

            return collector.Parameters.Select(parameter => parameter.Name ?? string.Empty).Order().ToList();
        }

        private sealed class ParameterCollector : ExpressionVisitor
        {
            public HashSet<ParameterExpression> Parameters { get; } = [];

            protected override Expression VisitParameter(ParameterExpression node)
            {
                Parameters.Add(node);
                return base.VisitParameter(node);
            }
        }
    }
}
