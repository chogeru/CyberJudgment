using Zenject;
using UnityEngine;
using AbubuResouse.Singleton;
using AbubuResouse.MVP.Presenter;
using AbubuResouse.MVP.Model;
using AbubuResouse.MVP.View;
using AbubuResouse.MVP.Repository;

public class UIInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<UIRepository>().AsSingle().WithArguments($"{Application.persistentDataPath}/ui_database.db");
        Container.Bind<UIView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<UIModel>().AsSingle();
        Container.Bind<UIPresenter>().FromComponentInHierarchy().AsSingle();
    }
}
