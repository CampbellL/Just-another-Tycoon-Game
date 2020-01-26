using Economy;
using Game;
using Zenject;

namespace Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EconomyController>().AsSingle().NonLazy();
            Container.Bind<PlayerState>().AsSingle().NonLazy();
            Container.Bind<PlayerStateService>().AsSingle();
            Container.Bind<Worker.WorkerController>().AsSingle();
        }
    }
}