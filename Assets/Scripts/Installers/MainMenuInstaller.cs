using Api;
using Zenject;

namespace Installers
{
    public class MainMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ApiHandler>().AsSingle().NonLazy();
        }
    }
}