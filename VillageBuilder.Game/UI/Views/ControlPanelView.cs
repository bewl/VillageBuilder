using Terminal.Gui;
using System;
using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Game.UI.Views
{
    public class ControlPanelView : FrameView
    {
        public event EventHandler? PauseResumeClicked;
        public event EventHandler<float>? TimeScaleChanged;
        public event EventHandler<BuildingType>? BuildingRequested;

        public ControlPanelView() : base("Controls")
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            var y = 1;

            var pauseButton = new Button("Pause/Resume (Space)", true)
            {
                X = 1,
                Y = y++
            };
            pauseButton.Clicked += () => PauseResumeClicked?.Invoke(this, EventArgs.Empty);

            var speedUpButton = new Button("Speed Up (+)", true)
            {
                X = 1,
                Y = y++
            };
            speedUpButton.Clicked += () => TimeScaleChanged?.Invoke(this, 2.0f);

            var slowDownButton = new Button("Slow Down (-)", true)
            {
                X = 1,
                Y = y++
            };
            slowDownButton.Clicked += () => TimeScaleChanged?.Invoke(this, 0.5f);

            var normalSpeedButton = new Button("Normal Speed (=)", true)
            {
                X = 1,
                Y = y++
            };
            normalSpeedButton.Clicked += () => TimeScaleChanged?.Invoke(this, 1.0f);

            y++;

            var buildFarmButton = new Button("Build Farm (F)", true)
            {
                X = 1,
                Y = y++
            };
            buildFarmButton.Clicked += () => BuildingRequested?.Invoke(this, BuildingType.Farm);

            var buildHouseButton = new Button("Build House (H)", true)
            {
                X = 1,
                Y = y++
            };
            buildHouseButton.Clicked += () => BuildingRequested?.Invoke(this, BuildingType.House);

            var buildWarehouseButton = new Button("Build Warehouse (W)", true)
            {
                X = 1,
                Y = y++
            };
            buildWarehouseButton.Clicked += () => BuildingRequested?.Invoke(this, BuildingType.Warehouse);

            Add(pauseButton, speedUpButton, slowDownButton, normalSpeedButton,
                buildFarmButton, buildHouseButton, buildWarehouseButton);
        }
    }
}