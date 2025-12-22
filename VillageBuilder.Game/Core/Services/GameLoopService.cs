using System;
using System.Threading;
using System.Threading.Tasks;
using VillageBuilder.Game.UI.ViewModels;

namespace VillageBuilder.Game.Core.Services
{
    public class GameLoopService
    {
        private readonly GameViewModel _viewModel;
        private CancellationTokenSource? _cts;
        private Task? _gameLoopTask;

        public bool IsRunning { get; private set; }

        public GameLoopService(GameViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _gameLoopTask = Task.Run(() => GameLoopAsync(_cts.Token));
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _cts?.Cancel();
            _gameLoopTask?.Wait(TimeSpan.FromSeconds(2));
            _cts?.Dispose();
            _cts = null;
            _gameLoopTask = null;
            IsRunning = false;
        }

        private async Task GameLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _viewModel.TickEngine();
                    var interval = _viewModel.GetTickInterval();
                    await Task.Delay(interval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}