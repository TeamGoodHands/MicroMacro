namespace Module.Application.SceneSwitch
{
    public interface IFadeHandler
    {
        void StartFadeOut();        
        void StartFadeIn();
        bool IsFadeOutComplete();   
        bool IsFading();
    }
}