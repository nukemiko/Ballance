using Ballance2.Utils;
using Ballance2.Res;
using Ballance2.Services;
using Ballance2.Services.Debug;
using Ballance2.Services.I18N;
using Ballance2.Entry;
using System.Collections.Generic;
using UnityEngine;
using Ballance2.Package;
using Ballance2.Base;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GameSystem.cs
* 
* 用途：
* 游戏的基础系统与入口管理。
* 此管理器用来管理基础系统初始化和一些基础服务。
* 与GameManager不同，GameManager管理的是上层的服务，而此服务管理的是基础服务。
*
* 作者：
* mengyu
*
*/

namespace Ballance2
{
  /// <summary>
  /// 基础系统
  /// </summary>
  public static class GameSystem
  {
    private const string TAG = "GameSystem";

    #region 系统入口

    /// <summary>
    /// 系统接管器回调
    /// </summary>
    /// <param name="act">当前操作</param>
    public delegate void SysHandler(int act);
    private static SysHandler sysHandler = null;

    public const int ACTION_INIT = 1;
    public const int ACTION_DESTROY = 2;
    public const int ACTION_FORCE_INT = 3;

    /// <summary>
    /// 注册系统接管器
    /// </summary>
    /// <param name="handler">系统接管器</param>
    public static void RegSysHandler(SysHandler handler)
    {
      if (sysHandler != null)
      {
        Log.E("GameSystemInit", "SysHandler already set ");
        GameErrorChecker.LastError = GameError.AccessDenined;
        return;
      }
      sysHandler = handler;
    }
    /// <summary>
    /// 退出程序
    /// </summary>
    public static void QuitPlayer()
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }

    #endregion

    #region 系统服务

    private static Dictionary<string, GameService> systemService = new Dictionary<string, GameService>();

    /// <summary>
    /// 注册系统服务
    /// </summary>
    /// <param name="name">服务名称</param>
    /// <param name="classObject">服务对象</param>
    /// <returns></returns>
    public static bool RegSystemService<T>() where T : GameService
    {
      GameObject newManager = CloneUtils.CreateEmptyObject("NewManagerObject");
      T manager = newManager.AddComponent<T>();
      newManager.name = manager.Name;

      if (systemService.ContainsKey(manager.Name))
      {
        GameErrorChecker.LastError = GameError.AlreadyRegistered;
        return false;
      }

      //init
      if (!manager.Initialize())
      {
        Log.E(TAG, "Service {0} init failed ! {1}({2})", manager.Name, GameErrorChecker.LastError, GameErrorChecker.GetLastErrorMessage());
        return false;
      }

      systemService.Add(manager.Name, manager);

      return true;
    }
    /// <summary>
    /// 取消注册系统服务
    /// </summary>
    /// <param name="name">服务</param>
    /// <returns></returns>
    public static bool UnRegSystemService(string name)
    {
      if (!systemService.ContainsKey(name))
      {
        GameErrorChecker.LastError = GameError.NotRegister;
        return false;
      }

      //释放
      GameService gameService = systemService[name];
      gameService.Destroy();

      return systemService.Remove(name);
    }
    /// <summary>
    /// 获取系统服务
    /// </summary>
    /// <param name="name">服务名称</param>
    /// <returns></returns>
    public static GameService GetSystemService(string name)
    {
      if (!systemService.TryGetValue(name, out GameService o))
        GameErrorChecker.LastError = GameError.ClassNotFound;
      return o;
    }

    #endregion

    #region 调试提供

    /// <summary>
    /// 系统调试提供者
    /// </summary>
    public interface SysDebugProvider
    {
      bool StartDebug();
    }
    public delegate SysDebugProvider SysDebugProviderCheck();

    private static SysDebugProvider sysDebugProvider = null;
    private static SysDebugProviderCheck sysDebugProviderCheck = null;

    /// <summary>
    /// 注册调试提供者
    /// </summary>
    public static void RegSysDebugProvider(SysDebugProviderCheck providerCheck)
    {
      sysDebugProviderCheck = providerCheck;
    }
    private static void StartRunDebugProvider()
    {
      if (sysDebugProviderCheck != null)
      {
        sysDebugProvider = sysDebugProviderCheck.Invoke();
        if (sysDebugProvider != null)
          sysDebugProvider.StartDebug();
      }
    }

    #endregion

    #region 初始化

    private static bool sysInit = false;

    internal static void FillResEntry(GameStaticResEntry gameEntry) { gameStaticResEntryInstance = gameEntry; }

    private static GameStaticResEntry gameStaticResEntryInstance = null;

    public static bool IsRestart = false;

    public static void PreInit()
    {
      //初始化设置
      GameSettingsManager.Init();
      //初始化I18N 和系统字符串资源
      I18NProvider.SetCurrentLanguage((SystemLanguage)GameSettingsManager.GetSettings("core").GetInt("language", (int)Application.systemLanguage));
      I18NProvider.LoadLanguageResources(Resources.Load<TextAsset>("StaticLangResource").text);
    }

    /// <summary>
    /// 初始化主入口
    /// </summary> 
    public static void Init()
    {
      if (!sysInit)
      {
        sysInit = true;

        //Init system
        Ballance2.Utils.UnityLogCatcher.Init();
        GameErrorChecker.Init();

        //初始化静态资源入口
        GameStaticResourcesPool.InitStaticPrefab(gameStaticResEntryInstance.GamePrefab, gameStaticResEntryInstance.GameAssets);

        //Call game init
        if (sysHandler == null)
        {
          Log.D(TAG, "Not found SysHandler, did you call RegSysHandler first?");
          GameErrorChecker.ThrowGameError(GameError.ConfigueNotRight, null);
          return;
        }        
        //Call init
        sysHandler(ACTION_INIT);

        GamePackageManager.PreRegInternalPackage();

        //Init system services
        RegSystemService<GameMediator>();

        GameManager.GameMediator = (GameMediator)GetSystemService("GameMediator");
        GameManager.GameMediator.RegisterEventHandler(GamePackage.GetSystemPackage(), GameEventNames.EVENT_BASE_INIT_FINISHED, "DebuggerHandler", (evtName, param) => {
          StartRunDebugProvider();
          return false;
        });

        //Init base services
        RegSystemService<GameManager>();
        RegSystemService<GamePackageManager>();
        RegSystemService<GameUIManager>();
        RegSystemService<GameSoundManager>();

        Log.D(TAG, "System init ok");
      }
      else
      {
        Log.D(TAG, "System already init ok");
      }
    }
    /// <summary>
    /// 消毁
    /// </summary>
    public static void Destroy()
    {
      if (sysInit)
      {
        sysInit = false;

        Log.D(TAG, "System destroy");

        sysHandler?.Invoke(ACTION_DESTROY);

        //Destroy system service
        List<string> serviceNames = new List<string>(systemService.Keys);
        for (int i = serviceNames.Count - 1; i >= 0; i--)
        {
          try
          {
            systemService[serviceNames[i]].Destroy();
          }
          catch (System.Exception e)
          {
            UnityEngine.Debug.LogError("Exception when destroy service " + serviceNames[i] + "," + e.ToString());
          }
        }
        serviceNames.Clear();
        systemService.Clear();

        GameManager.GameMediator = null;

        //释放其他组件
        I18NProvider.ClearAllLanguageResources();
        GameSettingsManager.Destroy();
        Ballance2.Utils.UnityLogCatcher.Destroy();
        GameErrorChecker.Destroy();

        if (IsRestart)
        {
          System.GC.Collect();
          Init();
        }
        else QuitPlayer();
      }
    }
    /// <summary>
    /// 强制停止游戏
    /// </summary>
    public static void ForceInterruptGame()
    {
      sysHandler?.Invoke(ACTION_FORCE_INT);
    }

    #endregion
  }
}
