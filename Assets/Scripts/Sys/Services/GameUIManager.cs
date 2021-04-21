﻿using Ballance.LuaHelpers;
using Ballance2.Sys.Bridge;
using Ballance2.Sys.Debug;
using Ballance2.Sys.Package;
using Ballance2.Sys.Res;
using Ballance2.Sys.UI;
using Ballance2.Sys.UI.Parts;
using Ballance2.Sys.UI.Utils;
using Ballance2.Sys.Utils;
using Ballance2.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GameUIManager.cs
* 
* 用途：
* UI 管理器，用于管理UI通用功能
*
* 作者：
* mengyu
*
* 更改历史：
* 2021-1-14 创建
* 2021-4-16 扩展窗口功能
*
*/

namespace Ballance2.Sys.Services
{
    /// <summary>
    /// UI 管理器
    /// </summary>
    [SLua.CustomLuaClass]
    [LuaApiDescription("UI 管理器")]
    public class GameUIManager : GameService
    {
        #region 基础

        private static readonly string TAG = "GameUIManager";

        [SLua.DoNotToLua]
        public GameUIManager() : base(TAG) {}

        private GameObject GameUICommonHost;

        [SLua.DoNotToLua]
        public override void Destroy()
        {
            DestroyWindowManagement();
            Object.Destroy(uiManagerGameObject);

            Log.D(TAG, "Destroy {0} ui objects", UIRoot.transform.childCount);
            for (int i = 0, c = UIRoot.transform.childCount; i < c; i++)
                Object.Destroy(UIRoot.transform.GetChild(i).gameObject);
        }
        [SLua.DoNotToLua]
        public override bool Initialize()
        {
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_UI_MANAGER_INIT_FINISHED);
            //等待基础加载完成
            GameManager.GameMediator.RegisterEventHandler(GamePackage.GetSystemPackage(),
                GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (evtName, param) =>
                {
                    UIRoot = GameManager.Instance.GameCanvas;
                    UIFadeManager = UIRoot.gameObject.AddComponent<UIFadeManager>();
                    GlobalFadeMaskWhite = UIRoot.transform.Find("GlobalFadeMaskWhite").gameObject.GetComponent<Image>();
                    GlobalFadeMaskBlack = UIRoot.transform.Find("GlobalFadeMaskBlack").gameObject.GetComponent<Image>();

                    //隐藏遮住初始化的遮罩
                    var GlobalBlackMask = UIRoot.transform.Find("GlobalBlackMask");
                    if(GlobalBlackMask != null) GlobalBlackMask.gameObject.SetActive(false);

                    //黑色遮罩
                    GlobalFadeMaskBlack.gameObject.SetActive(true);
                    MaskBlackSet(true);

                    //Add object
                    GameObject uiManagerGameObject = CloneUtils.CreateEmptyObjectWithParent(UIRoot.transform, "GameUIManager");
                    GameUIManagerObject gameUIManagerObject = uiManagerGameObject.AddComponent<GameUIManagerObject>();
                    gameUIManagerObject.GameUIManagerUpdateDelegate = Update;

                    //Init all
                    InitAllObects();
                    InitWindowManagement();

                    
                    //更新主管理器中的Canvas变量
                    GameManager.Instance.GameCanvas = ViewsRectTransform;

                    //发送就绪事件
                    GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_UI_MANAGER_INIT_FINISHED, "*");
                    return false;
                });
            //退出时的黑
            GameManager.GameMediator.RegisterEventHandler(GamePackage.GetSystemPackage(),
                GameEventNames.EVENT_BEFORE_GAME_QUIT, TAG, (evtName, param) =>
            {
                MaskBlackFadeIn(0.25f);
                return false;
            });
            return true;
        }

        /// <summary>
        /// UI 根
        /// </summary>
        [LuaApiDescription("UI 根")]
        public RectTransform UIRoot;
        /// <summary>
        /// 渐变管理器
        /// </summary>
        [LuaApiDescription("渐变管理器")]
        public UIFadeManager UIFadeManager;

        private GameObject uiManagerGameObject = null;

        private void Update()
        {
            UpdateToastShow();
        }

        //根管理
        private RectTransform TemporarilyRectTransform;
        private RectTransform GlobalWindowRectTransform;
        private RectTransform PagesRectTransform;
        private RectTransform WindowsRectTransform;
        private RectTransform ViewsRectTransform;

        /// <summary>
        /// UI 根 RectTransform
        /// </summary>
        public RectTransform UIRootRectTransform { get; private set; }

        private void InitAllObects()
        {
            UIRootRectTransform = UIRoot.GetComponent<RectTransform>();
            TemporarilyRectTransform = CloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUITemporarily").GetComponent<RectTransform>();
            TemporarilyRectTransform.gameObject.SetActive(false);
            GlobalWindowRectTransform = CloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIGlobalWindow").GetComponent<RectTransform>();
            PagesRectTransform = CloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIPages").GetComponent<RectTransform>();
            ViewsRectTransform = CloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameViewsRectTransform").GetComponent<RectTransform>();
            WindowsRectTransform = CloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIWindow").GetComponent<RectTransform>();

            InitAllPrefabs();

            UIAnchorPosUtils.SetUIAnchor(ViewsRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(ViewsRectTransform, 0, 0, 0, 0);
            UIAnchorPosUtils.SetUIAnchor(PagesRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(PagesRectTransform, 0, 0, 0, 0);
            UIAnchorPosUtils.SetUIAnchor(GlobalWindowRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(GlobalWindowRectTransform, 0, 0, 0, 0);
            UIAnchorPosUtils.SetUIAnchor(WindowsRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(WindowsRectTransform, 0, 0, 0, 0);

            UIToast = CloneUtils.CloneNewObjectWithParent(GameStaticResourcesPool.FindStaticPrefabs("PrefabToast"), UIRoot.transform, "GlobalUIToast").GetComponent<RectTransform>();
            UIToastImage = UIToast.GetComponent<Image>();
            UIToastText = UIToast.Find("Text").GetComponent<Text>();
            UIToast.gameObject.SetActive(false);
            UIToast.SetAsLastSibling();
            EventTriggerListener.Get(UIToast.gameObject).onClick = (g) => { toastTimeTick = 1; };
        }

        #endregion

        #region UI Prefab

        private struct UIPrefab {
            public GameObject prefab;
            public GameUIPrefabType type;
        }
        private Dictionary<string, UIPrefab> uIPrefabs = new Dictionary<string, UIPrefab>();

        /// <summary>
        /// 获取 UI 控件预制体
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        [LuaApiDescription("获取 UI 控件预制体")]
        [LuaApiParamDescription("name", "名称")]
        public GameObject GetUIPrefab(string name, GameUIPrefabType type) {
            if(uIPrefabs.ContainsKey(name)) {
                uIPrefabs.TryGetValue(name, out UIPrefab prefab);
                return prefab.type == type ? prefab.prefab : null;
            }
            return null;
        }
        /// <summary>
        /// 注册 UI 控件预制体
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="perfab">预制体</param>
        /// <returns>返回注册是否成功</returns>
        [LuaApiDescription("注册 UI 控件预制体", "返回注册是否成功")]
        [LuaApiParamDescription("name", "名称")]
        [LuaApiParamDescription("perfab", "预制体")]
        public bool RegisterUIPrefab(string name, GameUIPrefabType type, GameObject perfab) {
            if(uIPrefabs.ContainsKey(name)) {
                GameErrorChecker.SetLastErrorAndLog(GameError.AlreadyRegistered, TAG, "UI Prefab {0} 已经注册", name);
                return false;
            }
            UIPrefab uiprefab = new UIPrefab();
            uiprefab.type = type;
            uiprefab.prefab = perfab;
            uIPrefabs[name] = uiprefab;
            return true;
        }
        /// <summary>
        /// 清除已注册的 UI 控件预制体
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>返回是否成功</returns>
        [LuaApiDescription("清除已注册的 UI 控件预制体", "返回是否成功")]
        [LuaApiParamDescription("name", "名称")]
        public bool RemoveUIPrefab(string name) {
            if(uIPrefabs.ContainsKey(name)) {
                uIPrefabs.Remove(name);
                return true;
            }
            else {
                GameErrorChecker.SetLastErrorAndLog(GameError.NotRegister, TAG, "UI控件Prefab {0} 未注册", name);
                return false;
            }
        }

        private void InitAllPrefabs() {
            RegisterUIPrefab("Toggle", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabToggle"));
            RegisterUIPrefab("Slider", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabSlider"));
            RegisterUIPrefab("Progress", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabProgress"));
            RegisterUIPrefab("ScrollView", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabScrollView"));
            RegisterUIPrefab("InputField", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabInput"));
            RegisterUIPrefab("Dropdown", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabDropdown"));
            RegisterUIPrefab("CheckBox", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabCheck"));
            RegisterUIPrefab("Button", GameUIPrefabType.Control, GameStaticResourcesPool.FindStaticPrefabs("UIPrefabButton"));
            
            RegisterUIPrefab("PageCommon", GameUIPrefabType.Page, GameStaticResourcesPool.FindStaticPrefabs("GameUIPageBallanceCommon"));
            RegisterUIPrefab("PageTransparent", GameUIPrefabType.Page, GameStaticResourcesPool.FindStaticPrefabs("GameUIPageBallanceTransparent"));
        }

        #endregion

        #region 页管理

        private Dictionary<string, GameUIPage> pages = new Dictionary<string, GameUIPage>();
        private List<GameUIPage> pageStack = new List<GameUIPage>();
        private GameUIPage currentPage = null;

        /// <summary>
        /// 注册页
        /// </summary>
        /// <param name="name">页名称</param>
        /// <param name="prefabName">页模板名称</param>
        /// <returns>返回新创建的页实例，如果失败则返回null，请查看LastError</returns>
        [LuaApiDescription("注册页", "返回新创建的页实例，如果失败则返回null，请查看LastError")]
        [LuaApiParamDescription("name", "页名称")]
        [LuaApiParamDescription("prefabName", "页模板名称")]
        public GameUIPage RegisterPage(string name, string prefabName) {
            if(pages.TryGetValue(name, out GameUIPage pageOld)) {
                GameErrorChecker.SetLastErrorAndLog(GameError.AlreadyRegistered, TAG, "页 {0} 已经注册", name);
                return pageOld;
            }
            GameObject prefab = GetUIPrefab(prefabName, GameUIPrefabType.Page);
            if (prefab == null) {
                GameErrorChecker.SetLastErrorAndLog(GameError.PrefabNotFound, TAG, "未找到页模板 {0}", prefabName);
                return null;
            }
            GameObject go = CloneUtils.CloneNewObjectWithParent(prefab, PagesRectTransform, name);
            GameUIPage page = go.GetComponent<GameUIPage>();
            if(page == null) {
                GameErrorChecker.SetLastErrorAndLog(GameError.ClassNotFound, TAG, "页模板上未找到 GameUIPage 类");
                return null;
            }
            go.SetActive(false);
            pages.Add(name, page);
            return page;
        }
        /// <summary>
        /// 跳转到页
        /// </summary>
        /// <param name="name">页名称</param>
        /// <returns></returns>
        [LuaApiDescription("跳转到页")]
        [LuaApiParamDescription("name", "页名称")]
        public bool GoPage(string name) {

            if(currentPage != null && currentPage.name == name)
                return true;
            if(!pages.TryGetValue(name, out GameUIPage page)) {
                GameErrorChecker.SetLastErrorAndLog(GameError.NotRegister, TAG, "页 {0} 未注册", name);
                return false;
            }

            //Hide old
            if(pageStack.Count > 0) pageStack[pageStack.Count - 1].Hide();
            if(!pageStack.Contains(page)) pageStack.Add(page);
            
            page.Show();
            currentPage = page;
            return true;
        }
        /// <summary>
        /// 获取当前显示页
        /// </summary>
        /// <returns></returns>
        [LuaApiDescription("获取当前显示页")]
        public GameUIPage GetCurrentPage() { return currentPage;  }
        /// <summary>
        /// 关闭所有显示的页
        /// </summary>
        [LuaApiDescription("关闭所有显示的页")]
        public void CloseAllPage() {
            foreach(var p in pageStack) 
                p.Hide();
            currentPage = null;
        }
        /// <summary>
        /// 返回上一页
        /// </summary>
        /// <returns>如果可以返回，则返回true，否则返回false</returns>
        [LuaApiDescription("返回上一页", "如果可以返回，则返回true，否则返回false")]
        public bool BackPreviusPage() {
            if(pageStack.Count > 0) {
                //隐藏当前
                var page = pageStack[pageStack.Count - 1];
                pageStack.RemoveAt(pageStack.Count - 1);
                page.Hide();
                //显示前一个
                page = pageStack[pageStack.Count - 1];
                page.Show();
                currentPage = page;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 取消注册页
        /// </summary>
        /// <param name="name">页名称</param>
        /// <returns>返回是否成功</returns>
        [LuaApiDescription("取消注册页", "返回是否成功")]
        [LuaApiParamDescription("name", "页名称")]
        public bool UnRegisterPage(string name) {
            if(pages.TryGetValue(name, out GameUIPage page)) {
                if(page == currentPage) BackPreviusPage();
                if(pageStack.Contains(page)) pageStack.Remove(page);
                Object.Destroy(page.gameObject);
                pages.Remove(name);
                return true;
            }
            GameErrorChecker.LastError = GameError.NotRegister;
            return false;
        }

        #endregion

        #region 全局对话框

        private RectTransform UIToast;
        private Image UIToastImage;
        private Text UIToastText;

        private struct ToastData
        {
            public string text;
            public float showTime;

            public ToastData(string t, float i)
            {
                text = t;
                showTime = i;
            }
        }

        private List<ToastData> toastDatas = new List<ToastData>();

        private float toastTimeTick = 0;
        private float toastNextDelayTimeTick = 0;

        /// <summary>
        /// 显示全局土司提示
        /// </summary>
        /// <param name="text">提示文字</param>
        [LuaApiDescription("显示全局土司提示")]
        [LuaApiParamDescription("text", "提示文字")]
        public void GlobalToast(string text)
        {
            GlobalToast(text, text.Length / 30.0f);
        }
        /// <summary>
        /// 显示全局土司提示
        /// </summary>
        /// <param name="text">提示文字</param>
        /// <param name="showSec">显示时长（秒）</param>
        [LuaApiDescription("显示全局土司提示")]
        [LuaApiParamDescription("text", "提示文字")]
        [LuaApiParamDescription("showSec", "显示时长（秒）")]
        public void GlobalToast(string text, float showSec)
        {
            if (toastTimeTick <= 0) ShowToast(text, showSec);
            else toastDatas.Add(new ToastData(text, showSec));
        }

        private void ShowPendingToast()
        {
            if (toastDatas.Count > 0)
            {
                ShowToast(toastDatas[0].text, toastDatas[0].showTime);
                toastDatas.RemoveAt(0);
            }
        }
        private void ShowToast(string text, float time)
        {
            UIToastText.text = text;
            float h = UIToastText.preferredHeight;
            UIToast.sizeDelta = new Vector2(UIToast.sizeDelta.x, h > 50 ? h : 50);
            UIToast.gameObject.SetActive(true);
            UIToast.SetAsLastSibling();

            UIFadeManager.AddFadeIn(UIToastImage, 0.26f);
            UIFadeManager.AddFadeIn(UIToastText, 0.25f);
            toastTimeTick = time + 0.25f;
        }
        private void UpdateToastShow()
        {
            if (toastTimeTick >= 0)
            {
                toastTimeTick -= Time.deltaTime;
                if (toastTimeTick <= 0)
                {
                    UIFadeManager.AddFadeOut(UIToastImage, 0.4f, true);
                    UIFadeManager.AddFadeOut(UIToastText, 0.4f, false);
                    toastNextDelayTimeTick = 1.0f;
                }
            }
            if (toastNextDelayTimeTick >= 0)
            {
                toastNextDelayTimeTick -= Time.deltaTime;
                if (toastNextDelayTimeTick <= 0) ShowPendingToast();
            }
        }

        /// <summary>
        /// 显示全局 Alert 对话框（窗口模式）
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <returns>返回对话框ID</returns>
        [LuaApiDescription("显示全局 Alert 对话框（窗口模式）", "返回对话框ID")]
        [LuaApiParamDescription("text", "内容")]
        [LuaApiParamDescription("title", "标题")]
        [LuaApiParamDescription("okText", "OK 按钮文字")]
        public int GlobalAlertWindow(string text, string title, string okText = "确定")
        {
            GameObject windowGo = CloneUtils.CloneNewObjectWithParent(PrefabUIAlertWindow, WindowsRectTransform.transform, "");
            RectTransform rectTransform = windowGo.GetComponent<RectTransform>();
            Button btnOk = rectTransform.Find("Button").GetComponent<Button>();
            rectTransform.Find("DialogText").GetComponent<Text>().text = text;
            rectTransform.Find("Button/Text").GetComponent<Text>().text = okText;
            Window window = CreateWindow(title, rectTransform);
            window.WindowType = WindowType.GlobalAlert;
            window.CanClose = true;
            window.CanDrag = true;
            window.CanResize = true;
            window.CanMax = false;
            window.CanMin = false;
            window.MinSize = new Vector2(300, 250);
            window.Show();
            btnOk.onClick.AddListener(() => {
                window.Close();
            });
            window.onClose += (id) =>
            {
                PagesRectTransform.gameObject.SetActive(true);
                WindowsRectTransform.gameObject.SetActive(true);
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    id, false);
            };
            return window.GetWindowId();
        }
        /// <summary>
        /// 显示全局 Confirm 对话框（窗口模式）
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <param name="cancelText">Cancel 按钮文字</param>
        /// <returns></returns>
        [LuaApiDescription("显示全局 Confirm 对话框（窗口模式）", "返回对话框ID")]
        [LuaApiParamDescription("text", "内容")]
        [LuaApiParamDescription("title", "标题")]
        [LuaApiParamDescription("okText", "OK 按钮文字")]
        [LuaApiParamDescription("cancelText", "Cancel 按钮文字")]
        public int GlobalConfirmWindow(string text, string title, string okText = "确定", string cancelText = "取消")
        {
            GameObject windowGo = CloneUtils.CloneNewObjectWithParent(PrefabUIConfirmWindow, WindowsRectTransform.transform, "");
            RectTransform rectTransform = windowGo.GetComponent<RectTransform>();
            Button btnYes = rectTransform.Find("ButtonConfirm").GetComponent<Button>();
            Button btnNo = rectTransform.Find("ButtonCancel").GetComponent<Button>();
            rectTransform.Find("ButtonConfirm/Text").GetComponent<Text>().text = okText;
            rectTransform.Find("ButtonCancel/Text").GetComponent<Text>().text = cancelText;
            rectTransform.Find("DialogText").GetComponent<Text>().text = text;
            Window window = CreateWindow(title, rectTransform);
            window.WindowType = WindowType.GlobalAlert;
            window.CanClose = false;
            window.CanDrag = true;
            window.CanResize = false;
            window.CanMax = false;
            window.CanMin = false;
            window.Show();
            window.onClose += (id) =>
            {
                PagesRectTransform.gameObject.SetActive(true);
                WindowsRectTransform.gameObject.SetActive(true);
            };
            btnYes.onClick.AddListener(() =>
            {
                window.Close();
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    window.GetWindowId(), true);
            });
            btnNo.onClick.AddListener(() => {
                window.Close();
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    window.GetWindowId(), false);
            });
            return window.GetWindowId();
        }

        #endregion

        #region 窗口管理

        private List<int> returnWindowIds = new List<int>();
        private int startWindowId = 16;

        private void ReturnWindowId(int id)
        {
            if (!returnWindowIds.Contains(id))
                returnWindowIds.Add(id);
        }
        internal int GenWindowId()
        {
            if(returnWindowIds.Count > 0)
            {
                int result = returnWindowIds[0];
                returnWindowIds.RemoveAt(0);
                return result;
            }
            if (startWindowId < 0xffff)
                startWindowId++;
            else
                startWindowId = 0;
            return startWindowId;
        }

        private GameObject PrefabUIAlertWindow;
        private GameObject PrefabUIConfirmWindow;
        private GameObject PrefabUIWindow;

        private void InitWindowManagement()
        {
            managedWindows = new Dictionary<int, Window>();

            PrefabUIAlertWindow = GameStaticResourcesPool.FindStaticPrefabs("PrefabAlertWindow");
            PrefabUIConfirmWindow = GameStaticResourcesPool.FindStaticPrefabs("PrefabConfirmWindow");
            PrefabUIWindow = GameStaticResourcesPool.FindStaticPrefabs("PrefabWindow");

            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE);
        }
        private void DestroyWindowManagement()
        {
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE);

            if (managedWindows != null)
            {
                foreach (var w in managedWindows)
                    w.Value.Destroy();
                managedWindows.Clear();
                managedWindows = null;
            }
        }

        //窗口

        private Dictionary<int, Window> managedWindows = null;

        /// <summary>
        /// 创建自定义窗口（默认不显示）
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="customView">窗口自定义View</param>
        /// <returns>返回窗口实例</returns>
        [LuaApiDescription("创建自定义窗口（默认不显示）", "返回窗口实例")]
        [LuaApiParamDescription("title", "标题")]
        [LuaApiParamDescription("customView", "窗口自定义View")]
        public Window CreateWindow(string title, RectTransform customView)
        {
            return CreateWindow(title, customView, false);
        }
        /// <summary>
        /// 创建自定义窗口
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="show">创建后是否立即显示</param>
        /// <param name="customView">窗口自定义View</param>
        /// <returns>返回窗口实例</returns>
        [LuaApiDescription("创建自定义窗口", "返回窗口实例")]
        [LuaApiParamDescription("title", "标题")]
        [LuaApiParamDescription("show", "创建后是否立即显示")]
        [LuaApiParamDescription("title", "标题")]
        public Window CreateWindow(string title, RectTransform customView, bool show)
        {
            return CreateWindow(title, customView, show, 0, 0, 0, 0);
        }
        /// <summary>
        /// 创建自定义窗口
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="show">创建后是否立即显示</param>
        /// <param name="customView">窗口自定义View</param>
        /// <param name="x">X 坐标</param>
        /// <param name="y">Y 坐标</param>
        /// <param name="w">宽度，0 使用默认</param>
        /// <param name="h">高度，0 使用默认</param>
        /// <returns>返回窗口实例</returns>
        [LuaApiDescription("创建自定义窗口", "返回窗口实例")]
        [LuaApiParamDescription("title", "标题")]
        [LuaApiParamDescription("show", "创建后是否立即显示")]
        [LuaApiParamDescription("title", "标题")]
        [LuaApiParamDescription("x", "X 坐标")]
        [LuaApiParamDescription("y", "Y 坐标")]
        [LuaApiParamDescription("w", "宽度，0 使用默认")]
        [LuaApiParamDescription("h", "高度，0 使用默认")]
        public Window CreateWindow(string title, RectTransform customView, bool show, float x, float y, float w, float h)
        {
            GameObject windowGo = CloneUtils.CloneNewObjectWithParent(PrefabUIWindow, WindowsRectTransform.transform);
            Window window = windowGo.GetComponent<Window>();
            window.Title = title;
            window.SetView(customView);
            window.windowId = GenWindowId();
            RegisterWindow(window);

            window.onClose += (id) =>
            {
                ReturnWindowId(id);
                window.Destroy();
                managedWindows.Remove(window.GetWindowId());
            };
            
            if (w != 0 && h != 0) window.Size = new Vector2(w, h);
            if (x == 0 && y == 0) window.MoveToCenter();
            else window.Position = new Vector2(x, y);

            if (show) window.Show();
            else window.SetVisible(false);
            
            return window;
        }
        /// <summary>
        /// 注册窗口到管理器中
        /// </summary>
        /// <param name="window">窗口实例</param>
        /// <returns></returns>
        [LuaApiDescription("注册窗口到管理器中")]
        [LuaApiParamDescription("window", "窗口实例")]
        public Window RegisterWindow(Window window)
        {
            int id = window.GetWindowId();
            window.name = "GameUIWindow_" + id;
            if (!managedWindows.ContainsKey(id))
                managedWindows.Add(id, window);
            else
                managedWindows[id] = window;
            return window;
        }
        /// <summary>
        /// 通过 ID 查找窗口
        /// </summary>
        /// <param name="windowId">窗口ID</param>
        /// <returns>返回找到的窗口实例，如果找不到则返回null</returns>
        [LuaApiDescription("通过 ID 查找窗口", "返回找到的窗口实例，如果找不到则返回null")]
        [LuaApiParamDescription("windowId", "窗口ID")]
        public Window FindWindowById(int windowId)
        {
            managedWindows.TryGetValue(windowId, out Window w);
            return w;
        }

        private Window currentVisibleWindowAlert = null;
        private Window currentActiveWindow = null;

        /// <summary>
        /// 获取当前激活的窗口
        /// </summary>
        /// <returns></returns>
        [LuaApiDescription("获取当前激活的窗口")]
        public Window GetCurrentActiveWindow() { return currentActiveWindow; }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="window">窗口实例</param>
        [LuaApiDescription("显示窗口")]
        [LuaApiParamDescription("window", "窗口实例")]
        public void ShowWindow(Window window)
        {
            switch (window.WindowType)
            {
                case WindowType.GlobalAlert:
                    window.GetRectTransform().transform.SetParent(GlobalWindowRectTransform.transform);
                    PagesRectTransform.gameObject.SetActive(false);
                    WindowsRectTransform.gameObject.SetActive(false);
                    WindowsRectTransform.SetAsLastSibling();
                    currentVisibleWindowAlert = window;
                    break;
                case WindowType.Normal:
                    window.GetRectTransform().transform.SetParent(WindowsRectTransform.transform);
                    WindowsRectTransform.SetAsLastSibling();
                    break;
            }
            window.SetVisible(true);
        }
        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="window">窗口实例</param>
        [LuaApiDescription("隐藏窗口")]
        [LuaApiParamDescription("window", "窗口实例")]
        public void HideWindow(Window window) { window.Hide(); }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="window">窗口实例</param>
        [LuaApiDescription("关闭窗口")]
        [LuaApiParamDescription("window", "窗口实例")]
        public void CloseWindow(Window window) { 
            window.Close(); 
        }
        /// <summary>
        /// 激活窗口至最顶层
        /// </summary>
        /// <param name="window">窗口实例</param>
        [LuaApiDescription("激活窗口至最顶层")]
        [LuaApiParamDescription("window", "窗口实例")]
        public void ActiveWindow(Window window) {
            if(currentActiveWindow != null) 
                currentActiveWindow.WindowTitleImage.color = currentActiveWindow.TitleDefaultColor;
            currentActiveWindow = window;
            currentActiveWindow.WindowTitleImage.color = currentActiveWindow.TitleActiveColor;
            currentActiveWindow.WindowRectTransform.transform.SetAsLastSibling();
        }

        #endregion

        #region 全局渐变遮罩

        private Image GlobalFadeMaskWhite;
        private Image GlobalFadeMaskBlack;

        /// <summary>
        /// 全局黑色遮罩控制（无渐变动画）
        /// </summary>
        /// <param name="show">为true则显示遮罩，否则隐藏</param>
        [LuaApiDescription("全局黑色遮罩隐藏（无渐变动画）")]
        [LuaApiParamDescription("全局黑色遮罩控制", "为true则显示遮罩，否则隐藏")]
        public void MaskBlackSet(bool show)
        {
            GlobalFadeMaskBlack.color = new Color(GlobalFadeMaskBlack.color.r,
                   GlobalFadeMaskBlack.color.g, GlobalFadeMaskBlack.color.b, show ? 1.0f : 0f);
            GlobalFadeMaskBlack.gameObject.SetActive(show);
            GlobalFadeMaskBlack.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局白色遮罩控制（无渐变动画）
        /// </summary>
        /// <param name="show">为true则显示遮罩，否则隐藏</param>
        [LuaApiDescription("全局白色遮罩控制（无渐变动画）")]
        [LuaApiParamDescription("show", "为true则显示遮罩，否则隐藏")]
        public void MaskWhiteSet(bool show)
        {
            GlobalFadeMaskWhite.color = new Color(GlobalFadeMaskWhite.color.r,
                GlobalFadeMaskWhite.color.g, GlobalFadeMaskWhite.color.b, show ? 1.0f : 0f);
            GlobalFadeMaskWhite.gameObject.SetActive(show);
            GlobalFadeMaskWhite.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        [LuaApiDescription("全局黑色遮罩渐变淡入")]
        [LuaApiParamDescription("second", "耗时（秒）")]
        public void MaskBlackFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskBlack, second);
            GlobalFadeMaskBlack.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局白色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        [LuaApiDescription("全局白色遮罩渐变淡入")]
        [LuaApiParamDescription("second", "耗时（秒）")]
        public void MaskWhiteFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskWhite, second);
            GlobalFadeMaskWhite.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        [LuaApiDescription("全局黑色遮罩渐变淡出")]
        [LuaApiParamDescription("second", "耗时（秒）")]
        public void MaskBlackFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskBlack, second, true);
            GlobalFadeMaskBlack.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局白色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        [LuaApiDescription("全局白色遮罩渐变淡出")]
        [LuaApiParamDescription("second", "耗时（秒）")]
        public void MaskWhiteFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskWhite, second, true);
            GlobalFadeMaskWhite.transform.SetAsLastSibling();
        }

        #endregion

        #region 通用管理

        /// <summary>
        /// 设置一个UI至临时区域
        /// </summary>
        /// <param name="view">指定UI</param>
        [LuaApiDescription("设置一个UI至临时区域")]
        [LuaApiParamDescription("view", "指定UI")]
        public void SetViewToTemporarily(RectTransform view)
        {
            view.SetParent(TemporarilyRectTransform.gameObject.transform);
        }
        /// <summary>
        /// 将一个UI附加到主Canvas
        /// </summary>
        /// <param name="view">指定UI</param>
        [LuaApiDescription("将一个UI附加到主Canvas")]
        [LuaApiParamDescription("view", "指定UI")]
        public void AttatchViewToCanvas(RectTransform view)
        {
            view.SetParent(UIRoot.gameObject.transform);
        }
        /// <summary>
        /// 使用Prefab初始化一个对象并附加到主Canvas
        /// </summary>
        /// <param name="prefab">Prefab</param>
        /// <param name="name">新对象名称</param>
        /// <returns>返回新对象的RectTransform</returns>
        [LuaApiDescription("使用Prefab初始化一个对象并附加到主Canvas", "返回新对象的RectTransform")]
        [LuaApiParamDescription("prefab", "Prefab")]
        [LuaApiParamDescription("name", "新对象名称")]
        public RectTransform InitViewToCanvas(GameObject prefab, string name)
        {
            GameObject go = CloneUtils.CloneNewObjectWithParent(prefab,
                ViewsRectTransform.transform, name);
            RectTransform view = go.GetComponent<RectTransform>();
            view.SetParent(ViewsRectTransform.gameObject.transform);
            return view;
        }

        #endregion
    }

    /// <summary>
    /// UIPrefab的类型
    /// </summary>
    [SLua.CustomLuaClass]
    public enum GameUIPrefabType {
        /// <summary>
        /// 控件
        /// </summary>
        Control,
        /// <summary>
        /// 页
        /// </summary>
        Page
    }
}
