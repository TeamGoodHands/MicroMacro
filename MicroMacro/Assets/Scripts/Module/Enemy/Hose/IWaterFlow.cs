using System;

namespace Module.Enemy.Hose
{
    public enum WaterState
    {
        None,
        Pushing,
        Pushed,
        Ending,
        End
    }
    
    public interface IWaterFlow
    {
        float CurrentIntensity { get; }
        WaterFlow WaterFlow { get; }
        event Action<WaterState> OnWaterStateChanged;
    }
}