using Zenject;

namespace Game
{
    public class GameInstaller : MonoInstaller
    {
        public LiquidatedObjectPool poolPrefab;

        public override void InstallBindings()
        {
            var pool = new LiquidatedObjectPool(Container);
            Container.Bind<LiquidatedObjectPool>().FromInstance(pool).AsSingle();
        }
    }
}

