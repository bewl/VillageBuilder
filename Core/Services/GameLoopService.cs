using System;
using System.Threading;
using System.Threading.Tasks;
using VillageBuilder.Game.Core;

namespace VillageBuilder.Game.Core.Services
{
    /// <summary>
    /// Background service that runs the game simulation loop on a separate thread.
    /// </summary>
    public class GameLoopService
    {
        private readonly GameController _controller;
        private CancellationTokenSource? _cts;
        private Task? _gameLoopTask;

        public bool IsRunning { get; private set; }

        public GameLoopService(GameController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
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
                    _controller.TickEngine();
                    var interval = _controller.GetTickInterval();
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