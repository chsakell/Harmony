using Fluxor;
using Harmony.Client.Store;
using Harmony.Client.Store.CounterUseCase;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Pages
{
	public partial class Index
	{
		[Inject]
		private IState<CounterState> CounterState { get; set; }

		[Inject]
		public IDispatcher Dispatcher { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();
		}

		private void IncrementCount()
		{
			var action = new IncrementCounterAction();
			Dispatcher.Dispatch(action);
		}
	}
}
