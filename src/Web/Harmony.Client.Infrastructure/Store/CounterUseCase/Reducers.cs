using Fluxor;

namespace Harmony.Client.Infrastructure.Store.CounterUseCase
{
	public static class Reducers
	{
		[ReducerMethod]
		public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action) =>
			new(clickCount: state.ClickCount + 1);
	}
}
