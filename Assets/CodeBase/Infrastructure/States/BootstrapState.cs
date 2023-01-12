using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Services;
using CodeBase.Services.Ads;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Randomizer;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.StaticData;
using CodeBase.UI.Services.Factory;
using CodeBase.UI.Services.Windows;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
  public class BootstrapState : IState
  {
    private const string Initial = "Initial";
    private readonly GameStateMachine _stateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly AllServices _services;

    public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices services)
    {
      _stateMachine = stateMachine;
      _sceneLoader = sceneLoader;
      _services = services;

      RegisterServices();
    }

    public void Enter() =>
      _sceneLoader.Load(Initial, onLoaded: EnterLoadLevel);

    public void Exit()
    {
    }

    private void RegisterServices()
    {
      RegisterStaticDataService();
      RegisterAdsService();

      _services.RegisterSingle<IGameStateMachine>(_stateMachine);
      RegisterAssetProvider();
      _services.RegisterSingle<IInputService>(InputService());
      _services.RegisterSingle<IRandomService>(new RandomService());
      _services.RegisterSingle<IPersistentProgressService>(new PersistentProgressService());
    
      _services.RegisterSingle<IUIFactory>(new UIFactory(
        _services.Single<IAssetProvider>(),
        _services.Single<IStaticDataService>(),
        _services.Single<IPersistentProgressService>(),
        _services.Single<IAdsService>()));
      
      _services.RegisterSingle<IWindowService>(new WindowService(_services.Single<IUIFactory>()));
      
      _services.RegisterSingle<IGameFactory>(new GameFactory(
        _services.Single<IAssetProvider>(),
        _services.Single<IStaticDataService>(),
        _services.Single<IRandomService>(),
        _services.Single<IPersistentProgressService>(),
        _services.Single<IWindowService>(),
        _services.Single<IGameStateMachine>()
        ));
      
      _services.RegisterSingle<ISaveLoadService>(new SaveLoadService(
        _services.Single<IPersistentProgressService>(),
        _services.Single<IGameFactory>()));
    }

    private void RegisterAssetProvider()
    {
      AssetProvider assetProvider = new AssetProvider();
      _services.RegisterSingle<IAssetProvider>(assetProvider);
      assetProvider.Initialize();
    }

    private void RegisterAdsService()
    {
      IAdsService adsService = new AdsService();
      adsService.Initialize();
      _services.RegisterSingle<IAdsService>(adsService);
    }

    private void RegisterStaticDataService()
    {
      IStaticDataService staticData = new StaticDataService();
      staticData.Load();
      _services.RegisterSingle(staticData);
    }

    private void EnterLoadLevel() =>
      _stateMachine.Enter<LoadProgressState>();

    private static IInputService InputService() =>
      Application.isEditor
        ? (IInputService) new StandaloneInputService()
        : new MobileInputService();
  }
}