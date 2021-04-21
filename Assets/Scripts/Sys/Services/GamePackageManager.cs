﻿using Ballance.LuaHelpers;
using Ballance2.Config;
using Ballance2.Config.Settings;
using Ballance2.Sys.Bridge;
using Ballance2.Sys.Debug;
using Ballance2.Sys.Package;
using Ballance2.Sys.Res;
using Ballance2.Sys.UI;
using Ballance2.Sys.Utils;
using Ballance2.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GamePackageManager.cs
* 
* 用途：
* 框架包管理器，用于管理每个功能包的加载与释放
*
* 作者：
* mengyu
*
* 更改历史：
* 2021-1-17 创建
* 2021-4-17 imengyu 添加模块管理器窗口
*
*/

namespace Ballance2.Sys.Services
{
    /// <summary>
    /// 框架模块管理器
    /// </summary>
    [SLua.CustomLuaClass]
    [LuaApiDescription("框架模块管理器")]
    public class GamePackageManager : GameService
    {
        private static readonly string TAG = "GamePackageManager";

        /// <summary>
        /// 系统模块的包名
        /// </summary>
        [LuaApiDescription("系统模块的包名")]
        public const string SYSTEM_PACKAGE_NAME = "core";

        [SLua.DoNotToLua]
        public GamePackageManager() : base(TAG) {}

        /// <summary>
        /// 是否是无模块模式
        /// </summary>
        /// <value></value>
        public bool NoPackageMode { get; set; }

        [SLua.DoNotToLua]
        public override void Destroy()
        {
            DestroyPackageManageWindow();
            UnLoadAllPackages();

            registeredPackages.Clear();
            packagesLoadStatus.Clear();
            loadedPackages.Clear();
            registeredPackages = null;
            packagesLoadStatus = null;
            loadedPackages = null;

            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_LOAD_FAILED);
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_LOAD_SUCCESS);
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_REGISTERED);
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_UNLOAD);
        }
        [SLua.DoNotToLua]
        public override bool Initialize()
        {
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_LOAD_FAILED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_LOAD_SUCCESS);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_REGISTERED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_UNREGISTERED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_PACKAGE_UNLOAD);
            GameManager.GameMediator.RegisterEventHandler(GamePackage.GetSystemPackage(),
                GameEventNames.EVENT_UI_MANAGER_INIT_FINISHED, "GamePackageManagerHandler", (evtName, param) =>
                {
                    //初始化调试窗口
                    InitPackageManageWindow();
                    return false;
                });

            //初始化系统包
            InitSystemPackage();
            return true;
        }

        #region 模块包自动加载管理

        private GamePackage systemPackage;
        internal class GamePackageRegisterInfo {
            public GamePackageRegisterInfo(GamePackage package) {
                this.package = package;
            }
            public GamePackageRegisterInfo(string packageName, bool enableLoad) {
                this.packageName = packageName;
                this.enableLoad = enableLoad;
            }

            public GamePackage package;

            public string packageName;
            public bool enableLoad;
        }

        internal Dictionary<string, int> packagesLoadStatus = new Dictionary<string, int>();
        internal Dictionary<string, GamePackageRegisterInfo> registeredPackages = new Dictionary<string, GamePackageRegisterInfo>();
        internal Dictionary<string, GamePackage> loadedPackages = new Dictionary<string, GamePackage>();

        private XmlDocument packageEnableStatusListXml = new XmlDocument();

        internal List<GamePackageRegisterInfo> LoadPackageRegisterInfo() {
            string pathPackageStatus = Application.persistentDataPath + "/PackageStatus.xml";
            if (File.Exists(pathPackageStatus))
            {
                StreamReader sr = new StreamReader(pathPackageStatus, Encoding.UTF8);
                try {
                    packageEnableStatusListXml.LoadXml(sr.ReadToEnd());
                } catch {
                    packageEnableStatusListXml.LoadXml(ConstStrings.DEFAULT_PACKAGE_STATUS_XML);
                }
                sr.Close();
                sr.Dispose();
            }
            else//加载默认xml文档
                packageEnableStatusListXml.LoadXml(ConstStrings.DEFAULT_PACKAGE_STATUS_XML);

            List<GamePackageRegisterInfo> lastRegisteredPackages = new List<GamePackageRegisterInfo>();
            XmlNode nodeNoPackagePackagee = packageEnableStatusListXml.SelectSingleNode("NoPackagePackagee");
            if (nodeNoPackagePackagee != null)
                NoPackageMode = bool.Parse(nodeNoPackagePackagee.InnerText);
            XmlNode nodePackageList = packageEnableStatusListXml.SelectSingleNode("PackageList");
            if (nodePackageList != null)
            {
                foreach (XmlNode n in nodePackageList) 
                    lastRegisteredPackages.Add(new GamePackageRegisterInfo(n.InnerText, n.Attributes["enabled"].Value == "True"));
            }

            return lastRegisteredPackages;
        }
        internal void SavePackageRegisterInfo() {
            StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/PackageStatus.xml", false, Encoding.UTF8);

            XmlDocument xml = new XmlDocument();
            XmlNode nodePackageConfig = xml.CreateElement("PackageConfig");
            XmlNode nodePackageList = xml.CreateElement("PackageList");
            XmlNode nodeNoPackagePackagee = xml.CreateElement("NoPackagePackagee");

            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));
            xml.AppendChild(nodePackageConfig);
            nodePackageConfig.AppendChild(nodePackageList);
            nodePackageConfig.AppendChild(nodeNoPackagePackagee);

            nodeNoPackagePackagee.InnerText = NoPackageMode.ToString();
            foreach(var s in registeredPackages.Values)
            {
                XmlNode node = xml.CreateElement("Package"); 
                XmlAttribute attr = xml.CreateAttribute("enabled");
                attr.Value = s.enableLoad.ToString();
                node.InnerText = s.package.PackageName;
                nodePackageList.AppendChild(node);
            }

            //save
            packageEnableStatusListXml.Save(sw);
            sw.Close();
            sw.Dispose();

        }
        internal void SetPackageEnableLoad(string packageName, bool val) {
            if(registeredPackages.TryGetValue(packageName, out var outPackage)) 
                outPackage.enableLoad = val;
        }

        #endregion

        /// <summary>
        /// 注册模块
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <param name="load">是否立即加载</param>
        /// <returns>返回是否加载成功。要获得错误代码，请获取 <see cref="GameErrorChecker.LastError"/></returns>
        [LuaApiDescription("注册模块", "返回是否加载成功。要获得错误代码，请获取 GameErrorChecker.LastError")]
        [LuaApiParamDescription("packageName", "包名")]
        [LuaApiParamDescription("load", "是否立即加载")]
        public async Task<bool> RegisterPackage(string packageName)
        {
            bool forceEnablePackage = false;
            if (packageName.StartsWith("Enable:")) {
                packageName = packageName.Substring(7);
                forceEnablePackage = true;
            }

            if (packagesLoadStatus.ContainsKey(packageName))
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.IsLoading, TAG, "Package {0} is loading", packageName);
                return false;
            }

            if (registeredPackages.ContainsKey(packageName))
            {
                Log.W(TAG, "Package {0} already registered!", packageName);
                return true;
            }

            string realPackagePath = null;
            GamePackage gamePackage = null;
#if UNITY_EDITOR
            //在编辑器中加载
            realPackagePath = GamePathManager.DEBUG_PACKAGE_FOLDER + "/" + packageName;
            if (DebugSettings.Instance.PackageLoadWay == LoadResWay.InUnityEditorProject
                && Directory.Exists(realPackagePath))
            {
                gamePackage = new GameEditorDebugPackage();
                Log.D(TAG, "Load package in editor : {0}", realPackagePath);
            }
            else 
#else
            if(true) 
#endif
            {
                //路径转换
                realPackagePath = GamePathManager.GetResRealPath("package", packageName + ".ballance");
                string realPackagePathInCore = GamePathManager.GetResRealPath("core", packageName + ".ballance");
                if (GamePathManager.Exists(realPackagePathInCore)) realPackagePath = realPackagePathInCore;
                else if (!GamePathManager.Exists(realPackagePath))
                {
                    Log.E(TAG, "Package {0} register failed because file {1} not found", packageName, realPackagePath);
                    return false;
                }
            }

            //设置正在加载
            packagesLoadStatus.Add(packageName, 1);

            Log.D(TAG, "Registing package {0}", packageName);

            if (gamePackage == null)
            {
                //判断文件类型
                if (FileUtils.TestFileIsZip(realPackagePath))
                    gamePackage = new GameZipPackage();
                else if (FileUtils.TestFileIsAssetBundle(realPackagePath))
                    gamePackage = new GameAssetBundlePackage();
                else
                {
                    packagesLoadStatus.Remove(packageName);
                    //文件格式不支持
                    GameErrorChecker.LastError = GameError.NotSupportFileType;
                    Log.E(TAG, "Package file type not support {0}", realPackagePath);
                    return false;
                }
            }

            gamePackage._Status = GamePackageStatus.Registing;

            //加载信息
            if (await gamePackage.LoadInfo(realPackagePath))
            {

                packagesLoadStatus.Remove(packageName);
                if(!registeredPackages.ContainsKey(packageName)) {
                    var info = new GamePackageRegisterInfo(gamePackage);
                    info.enableLoad = forceEnablePackage;
                    registeredPackages.Add(packageName, info);
                }

                gamePackage._Status = GamePackageStatus.Registered;

                Log.D(TAG, "Package {0} registered", packageName);
                //通知事件
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_PACKAGE_REGISTERED, "*", packageName);

                return true;
            }

            packagesLoadStatus.Remove(packageName);
            return false;
        }
        /// <summary>
        /// 查找已注册的模块
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns>返回模块实例，如果未找到，则返回null</returns>
        [LuaApiDescription("查找已注册的模块", "返回模块实例，如果未找到，则返回null")]
        [LuaApiParamDescription("packageName", "包名")]
        public GamePackage FindRegisteredPackage(string packageName)
        {
            registeredPackages.TryGetValue(packageName, out var outPackage);
            return outPackage != null ? outPackage.package : null;
        }
        /// <summary>
        /// 检测模块是否用户选择了启用
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns></returns>
        [LuaApiDescription("检测模块是否用户选择了启用", "")]
        [LuaApiParamDescription("packageName", "包名")]
        public bool IsPackageEnableLoad(string packageName)
        {
            registeredPackages.TryGetValue(packageName, out var outPackage);
            return outPackage != null && outPackage.enableLoad;
        }
        /// <summary>
        /// 取消注册模块
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <param name="unLoadImmediately">是否立即卸载</param>
        /// <returns>返回是否成功</returns>
        [LuaApiDescription("取消注册模块", "返回是否成功")]
        [LuaApiParamDescription("packageName", "包名")]
        [LuaApiParamDescription("unLoadImmediately", "是否立即卸载")]
        public bool UnRegisterPackage(string packageName, bool unLoadImmediately)
        {
            bool success = false;
            if (packageName == systemPackage.PackageName)
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.AccessDenined, TAG,
                    "Package {0} can not UnRegister", packageName);
                return success;
            }
            if (registeredPackages.ContainsKey(packageName))
            {
                registeredPackages.Remove(packageName);
                success = true;
            }
            if (packagesLoadStatus.ContainsKey(packageName))
                packagesLoadStatus.Remove(packageName);

            if (IsPackageLoaded(packageName))
                UnLoadPackage(packageName, unLoadImmediately);
            
            Log.D(TAG, "Package {0} unregistered", packageName);
            //通知事件
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_PACKAGE_UNREGISTERED, "*", packageName);
            return success;
        }

        /// <summary>
        /// 获取模块是否正在加载
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns></returns>
        [LuaApiDescription("获取模块是否正在加载", "")]
        [LuaApiParamDescription("packageName", "包名")]
        public bool IsPackageLoading(string packageName)
        {
            return packagesLoadStatus.ContainsKey(packageName) && packagesLoadStatus[packageName] == 2;
        }        
        /// <summary>
        /// 获取模块是否正在注册
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns></returns>
        [LuaApiDescription("获取模块是否正在注册", "")]
        [LuaApiParamDescription("packageName", "包名")]
        public bool IsPackageRegistering(string packageName)
        {
            return packagesLoadStatus.ContainsKey(packageName) && packagesLoadStatus[packageName] == 1;
        }
        /// <summary>
        /// 获取模块是否已加载
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns></returns>
        [LuaApiDescription("获取模块是否已加载", "")]
        [LuaApiParamDescription("packageName", "包名")]
        public bool IsPackageLoaded(string packageName)
        {
            return loadedPackages.ContainsKey(packageName); 
        }

        /// <summary>
        /// 通知模块运行
        /// </summary>
        /// <param name="packageNameFilter">包名筛选，为“*”时表示所有包，为正则表达式时使用正则匹配包。</param>
        [LuaApiDescription("通知模块运行")]
        [LuaApiParamDescription("packageNameFilter", "包名筛选，为“*”时表示所有包，为正则表达式时使用正则匹配包。")]
        public void NotifyAllPackageRun(string packageNameFilter)
        {
            foreach (GamePackage package in loadedPackages.Values)
            {
                if (package.Status == GamePackageStatus.LoadSuccess && !package.IsEntryCodeExecuted() &&
                    (packageNameFilter == "*" || Regex.IsMatch(package.PackageName, packageNameFilter)))
                    package.RunPackageExecutionCode();
            }
        }

        /// <summary>
        /// 加载模块
        /// </summary>
        /// <param name="packageName">模块包名</param>
        /// <returns>返回加载是否成功</returns>
        [LuaApiDescription("加载模块", "返回加载是否成功")]
        [LuaApiParamDescription("packageName", "模块包名")]
        public async Task<bool> LoadPackage(string packageName)
        {
            if(!StringUtils.IsPackageName(packageName))
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.InvalidPackageName, TAG, 
                    "Invalid packageName {0}", packageName);
                return false;
            }
            if (IsPackageLoaded(packageName))
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.AlreadyRegistered, TAG, 
                    "Package {0} already loaded!", packageName);
                return true;
            }
            if (IsPackageLoading(packageName))
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.IsLoading, TAG,
                    "Package {0} is loading!", packageName);
                return false;
            }

            //注册包
            GamePackage package = FindRegisteredPackage(packageName);
            if (package == null)
            {
                if(!await RegisterPackage(packageName))
                {
                    packagesLoadStatus.Remove(packageName);
                    string err = string.Format("Package {0} could not load, because RegisterPackage failed",
                        packageName);

                    Log.E(TAG, err);

                    //通知事件
                    GameManager.GameMediator.DispatchGlobalEvent(
                        GameEventNames.EVENT_PACKAGE_LOAD_FAILED, "*", packageName, err);
                    return false;
                }

                package = FindRegisteredPackage(packageName);
            }

            packagesLoadStatus.Add(packageName, 2);
            package._Status = GamePackageStatus.Loading;

            Log.D(TAG, "Loading package {0}", packageName);

            //加载依赖
            List<GamePackageDependencies> dependencies = package.BaseInfo.Dependencies;
            if (dependencies.Count > 0)
            {
                GamePackageDependencies dependency;
                GamePackage dependencyPackage;

                //加载依赖
                for (int i = 0; i < dependencies.Count; i++)
                {
                    dependency = dependencies[i];
                    if (!IsPackageLoaded(dependency.Name))
                    {
                        bool loadSuccess = await LoadPackage(dependency.Name);
                        if(!loadSuccess)
                        {
                            packagesLoadStatus.Remove(packageName);
                            string err = string.Format("Package {0} load failed because a dependency {1}/{2} " +
                               "load failed",
                               packageName, dependency.Name, dependency.MinVersion);

                            Log.E(TAG, err);
                            //通知事件
                            GameManager.GameMediator.DispatchGlobalEvent(
                                GameEventNames.EVENT_PACKAGE_LOAD_FAILED, "*", packageName, err);

                            package._Status = GamePackageStatus.LoadFailed;
                            return false;
                        }
                    }
                }
                //检查依赖版本
                for (int i = 0; i < dependencies.Count; i++)
                { 
                    dependency = dependencies[i];
                    
                    if (IsPackageLoaded(dependency.Name)) {
                        dependencyPackage = loadedPackages[dependency.Name];
                        if (dependencyPackage.PackageVersion < dependency.MinVersion)
                        {
                            string err = string.Format("Package {0} load failed because dependency {1} {2} " +
                                "less than required version {3}",
                                packageName, dependency.Name, dependencyPackage.PackageVersion,
                                dependency.MinVersion);

                            packagesLoadStatus.Remove(packageName);
                            package._Status = GamePackageStatus.LoadFailed;

                            Log.E(TAG, err);
                            //通知事件
                            GameManager.GameMediator.DispatchGlobalEvent(
                                GameEventNames.EVENT_PACKAGE_LOAD_FAILED, "*", packageName, err);

                            return false;
                        }
                        //添加依赖计数
                        dependencyPackage.DependencyRefCount++;
                    }
                }
            }

            //加载
            if(!await package.LoadPackage())
            {
                package._Status = GamePackageStatus.LoadFailed;
                packagesLoadStatus.Remove(packageName);

                string err = string.Format("Package {0} load failed {1}",
                        packageName, GameErrorChecker.GetLastErrorMessage());

                Log.E(TAG, err);
                //通知事件
                GameManager.GameMediator.DispatchGlobalEvent(
                    GameEventNames.EVENT_PACKAGE_LOAD_FAILED, "*", packageName, err);
                return false;
            }

            package._Status = GamePackageStatus.LoadSuccess;
            loadedPackages.Add(packageName, package);
            packagesLoadStatus.Remove(packageName);

            Log.D(TAG, "Package {0} loaded", packageName);
            //通知事件
            GameManager.GameMediator.DispatchGlobalEvent(
                GameEventNames.EVENT_PACKAGE_LOAD_SUCCESS, "*", packageName, package);

            return true;
        }
        /// <summary>
        /// 卸载模块
        /// </summary>
        /// <param name="packageName">模块包名</param>
        /// <param name="unLoadImmediately">
        /// 是否立即卸载，如果为false，此模块
        /// 将等待至依赖它的模块全部卸载之后才会卸载
        /// </param>
        /// <returns>返回是否成功</returns>
        [LuaApiDescription("卸载模块", "bbb")]
        [LuaApiParamDescription("packageName", "模块包名")]
        [LuaApiParamDescription("unLoadImmediately", "是否立即卸载，如果为false，此模块将等待至依赖它的模块全部卸载之后才会卸载")]
        public bool UnLoadPackage(string packageName, bool unLoadImmediately)
        {
            if (packageName == systemPackage.PackageName)
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.AccessDenined, TAG,
                    "Package {0} can not unload", packageName);
                return false;
            }

            GamePackage package = FindPackage(packageName);
            if (package == null)
            {
                GameErrorChecker.SetLastErrorAndLog(GameError.NotLoad, TAG,
                    "Can not unload package " + packageName + " because it isn't load! ");
                return false;
            }
            if (package.UnLoadWhenDependencyRefNone)
                return true;

            Log.D(TAG, "Unload package {0}", packageName);

            //如果不是立即卸载并且依赖引用计数大于0
            if (!unLoadImmediately && package.DependencyRefCount > 0)
            {
                package.UnLoadWhenDependencyRefNone = true;
                package._Status = GamePackageStatus.UnloadWaiting;
                Log.D(TAG, "Set package {0} to unload waiting", packageName);
                return true;
            }

            //依赖计数处理，在这里当前模块卸载了，所以要将他们的依赖计数减一
            List<GamePackageDependencies> dependencies = package.BaseInfo.Dependencies;
            GamePackage dependencyPackage;
            for (int i = 0; i < dependencies.Count; i++)
            {
                if (IsPackageLoaded(dependencies[i].Name))
                {
                    dependencyPackage = loadedPackages[dependencies[i].Name];
                    dependencyPackage.DependencyRefCount--;
                    if (dependencyPackage.DependencyRefCount <= 0
                        && dependencyPackage.UnLoadWhenDependencyRefNone)
                        UnLoadPackage(dependencyPackage.PackageName, true);
                }
            }

            //卸载

            //通知事件
            GameManager.GameMediator.DispatchGlobalEvent(
                GameEventNames.EVENT_PACKAGE_UNLOAD, "*", packageName, package);

            package._Status = GamePackageStatus.NotLoad;
            package.Destroy();
            packagesLoadStatus.Remove(packageName);
            loadedPackages.Remove(packageName);

            Log.D(TAG, "Package {0} unloaded", packageName);
            return true;
        }
        /// <summary>
        /// 查找已加载的模块
        /// </summary>
        /// <param name="packageName">模块包名</param>
        /// <returns>返回模块实例，如果未找到，则返回null</returns>
        [LuaApiDescription("查找已加载的模块", "返回模块实例，如果未找到，则返回null")]
        [LuaApiParamDescription("packageName", "模块包名")]
        public GamePackage FindPackage(string packageName)
        {
            loadedPackages.TryGetValue(packageName, out var outPackage);
            return outPackage;
        }

        private void UnLoadAllPackages()
        {
            List<string> packageNames = new List<string>(loadedPackages.Keys);
            foreach (string key in packageNames)
                if (key != SYSTEM_PACKAGE_NAME)
                    UnLoadPackage(key, true);
            packageNames.Clear();
        }
        private void InitSystemPackage()
        {
            systemPackage = GamePackage.GetSystemPackage();
            systemPackage._Status = GamePackageStatus.LoadSuccess;
            registeredPackages.Add(systemPackage.PackageName, new GamePackageRegisterInfo(systemPackage));
            loadedPackages.Add(systemPackage.PackageName, systemPackage);
        }
        
        #region 模块管理窗口

        private Window PackageManageWindow;
        private GameUIManager GameUIManager;
        
        private void InitPackageManageWindow() {
            GameUIManager = GameSystem.GetSystemService("GameUIManager") as GameUIManager;
            //Store data
            var storeDataShowPackageManageWindow = GameManager.Instance.GameStore.AddParameter("DbgShowPackageManageWindow", StoreDataAccess.GetAndSet, StoreDataType.Boolean);
            storeDataShowPackageManageWindow.SetDataProvider(0, (get, val) => {
                if(get) return PackageManageWindow.GetVisible();
                else { PackageManageWindow.SetVisible((bool)val); return null; }
            }); 
            //Create window
            PackageManageWindow = GameUIManager.CreateWindow("Package manager", 
                CloneUtils.CloneNewObject(GameStaticResourcesPool.FindStaticPrefabs("PackageManageWindow"), "PackageManageWindow").GetComponent<RectTransform>(),
                false, 0, 0, 900, 600);
            PackageManageWindow.CloseAsHide = true;
        }
        private void DestroyPackageManageWindow() {   
            GameManager.Instance.GameStore.RemoveParameter("DbgShowPackageManageWindow");
            if(PackageManageWindow != null) {
                PackageManageWindow.Destroy();
                PackageManageWindow = null;
            }
        }

        #endregion
    }
}
