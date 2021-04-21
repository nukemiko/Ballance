﻿using Ballance.LuaHelpers;
using Ballance2.UI.Parts;
using Ballance2.Utils;
using System.Text;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GameErrorChecker.cs
* 
* 用途：
* 错误检查器。
* 使用错误检查器获取游戏API的调用错误。
* 错误检查器还可负责弹出全局错误窗口以检查BUG.
*
* 作者：
* mengyu
*
* 更改历史：
* 2021-1-14 创建
*
*/

namespace Ballance2.Sys.Debug
{
    /// <summary>
    /// 错误检查器。使用错误检查器获取游戏API的调用错误。
    /// </summary>
    [SLua.CustomLuaClass]
    [LuaApiDescription("错误检查器。使用错误检查器获取游戏API的调用错误")]
    public class GameErrorChecker
    {
        private static GameGlobalErrorUI gameGlobalErrorUI;

        internal static void SetGameErrorUI(GameGlobalErrorUI errorUI)
        {
            gameGlobalErrorUI = errorUI;
        }

        /// <summary>
        /// 抛出游戏异常，此操作会直接终止游戏
        /// </summary>
        /// <param name="code">错误代码</param>
        /// <param name="message">关于错误的异常信息</param>
        [LuaApiDescription("抛出游戏异常，此操作会直接终止游戏")]
        [LuaApiParamDescription("code", "错误代码")]
        [LuaApiParamDescription("message", "关于错误的异常信息")]
        public static void ThrowGameError(GameError code, string message) 
        {
            StringBuilder stringBuilder = new StringBuilder("错误代码：");
            stringBuilder.Append(code.ToString());
            stringBuilder.Append("\n");
            stringBuilder.Append(string.IsNullOrEmpty(message) ? GameErrorInfo.GetErrorMessage(code) : message);
            stringBuilder.Append("\n");
            stringBuilder.Append(DebugUtils.GetStackTrace(1));

            GameSystem.ForceInterruptGame();
            gameGlobalErrorUI.ShowErrorUI(stringBuilder.ToString());
        }

        /// <summary>
        /// 获取或设置上一个操作的错误
        /// </summary>
        [LuaApiDescription("获取或设置上一个操作的错误")]
        public static GameError LastError { get; set; }
        /// <summary>
        /// 获取上一个操作的错误说明文字
        /// </summary>
        /// <returns></returns>
        [LuaApiDescription("获取上一个操作的错误说明文字")]
        public static string GetLastErrorMessage()
        {
            return GameErrorInfo.GetErrorMessage(LastError);
        }

        /// <summary>
        /// 设置错误码并打印日志
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志信息格式化字符串</param>
        /// <param name="param">日志信息格式化参数</param>
        [LuaApiDescription("设置错误码并打印日志")]
        [LuaApiParamDescription("code", "错误代码")]
        [LuaApiParamDescription("tag", "日志标签")]
        [LuaApiParamDescription("message", "日志信息格式化字符串")]
        [LuaApiParamDescription("param", "日志信息格式化参数")]
        public static void SetLastErrorAndLog(GameError code, string tag, string message, params object[] param)
        {
            LastError = code;
            Log.E(tag, message, param);
        }
        /// <summary>
        /// 设置错误码并打印日志
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="tag">TAG</param>
        /// <param name="message">错误信息</param>
        [LuaApiDescription("设置错误码并打印日志")]
        [LuaApiParamDescription("code", "错误代码")]
        [LuaApiParamDescription("tag", "日志标签")]
        [LuaApiParamDescription("message", "日志信息")]
        public static void SetLastErrorAndLog(GameError code, string tag, string message)
        {
            LastError = code;
            Log.E(tag, message);
        }
    }
}
