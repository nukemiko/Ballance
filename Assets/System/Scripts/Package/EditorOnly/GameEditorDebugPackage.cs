﻿using Ballance2.Res;
using Ballance2.Services.Debug;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

/*
* Copyright(c) 2021  mengyu
*
* 模块名：     
* GameEditorDebugPackage.cs
* 
* 用途：
* 提供游戏模块在Unity编辑器中直接加载的模式
*
* 作者：
* mengyu
*/

#pragma warning disable 1998

namespace Ballance2.Package
{
  public class GameEditorDebugPackage : GamePackage
  {
    public override async Task<bool> LoadInfo(string filePath)
    {
      PackageFilePath = filePath;
#if !UNITY_EDITOR
      GameErrorChecker.SetLastErrorAndLog(GameError.OnlyCanUseInEditor, TAG, "This package can only use in editor.");
      await base.LoadInfo(filePath);
      return false;
#else

      string defPath = filePath + "/PackageDef.xml";
      if (!File.Exists(defPath))
      {
        GameErrorChecker.SetLastErrorAndLog(GameError.PackageDefNotFound, TAG, "PackageDef.xml not found");
        LoadError = "模块并不包含 PackageDef.xml";
        return false;
      }
      else
      {
        try
        {
          PackageDef = new XmlDocument();
          PackageDef.Load(defPath);
        }
        catch (Exception e)
        {
          GameErrorChecker.SetLastErrorAndLog(GameError.PackageIncompatible, TAG, "Format error in PackageDef.xml : " + e);
          return false;
        }
        UpdateTime = File.GetLastWriteTime(defPath);
        return ReadInfo(PackageDef);
      }
#endif
    }
    public override async Task<bool> LoadPackage()
    {
      if (!string.IsNullOrEmpty(BaseInfo.Logo))
        LoadLogo(PackageFilePath + "/" + BaseInfo.Logo);

      LoadAllFileNames();
      return await base.LoadPackage();
    }

    private Dictionary<string, string> fileList = new Dictionary<string, string>();
    private void LoadAllFileNames()
    {
      DirectoryInfo theFolder = new DirectoryInfo(PackageFilePath);
      FileInfo[] thefileInfo = theFolder.GetFiles("*.*", SearchOption.AllDirectories);
      foreach (FileInfo NextFile in thefileInfo)
      { //遍历文件
        string path = NextFile.FullName.Replace("\\", "/");

        if (path.EndsWith(".meta")) continue;
        if (path.EndsWith(".map")) continue;

        int index = path.IndexOf("Assets/");
        if (index > 0)
          path = path.Substring(index);

        fileList.Add(NextFile.Name, path);
      }
    }
    private string GetFullPathByName(string name)
    {
      if (fileList.TryGetValue(name, out string fullpath))
        return fullpath;
      return null;
    }
    private void LoadLogo(string path)
    {
      try
      {
        Texture2D texture2D = new Texture2D(128, 128);
        texture2D.LoadImage(File.ReadAllBytes(path));

        BaseInfo.LogoTexture = Sprite.Create(texture2D,
            new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)),
            new Vector2(0.5f, 0.5f));
      }
      catch (Exception e)
      {
        BaseInfo.LogoTexture = null;
        Log.E(TAG, "在加载模块的 Logo {0} 失败\n错误信息：{1}", path, e.ToString());
      }
    }
    
    public override bool CheckCodeAssetExists(string pathorname)
    {
      if (PathUtils.IsAbsolutePath(pathorname) || pathorname.StartsWith("Assets/"))
      {
        if (File.Exists(pathorname) || File.Exists(pathorname + ".js"))
        {
          return true;
        }
      }
      else
      {
        string path = PackageFilePath + "/" + pathorname;
        if (File.Exists(path) || File.Exists(path + ".js"))
        {
          return true;
        }
        else
        {
          string fullPath = GetFullPathByName(pathorname);
          if (fullPath != null && File.Exists(fullPath))
          {
            return true;
          }
        }
      }
      return false;
    }
    public override CodeAsset GetCodeAsset(string pathorname)
    {
      string realtivePath = "";
      bool enableDebugger = Entry.GameEntry.Instance.DebugEnableV8Debugger;
      if (PathUtils.IsAbsolutePath(pathorname) || pathorname.StartsWith("Assets/"))
      {
        if (File.Exists(pathorname)) {
          realtivePath = PathUtils.ReplaceAbsolutePathToRelativePath(pathorname);
          return new CodeAsset(FileUtils.ReadAllToBytes(pathorname), pathorname, realtivePath, enableDebugger ? MakeDebugJSPath(realtivePath) : pathorname);
        }
      }
      else
      {
        string path = PackageFilePath + "/" + pathorname;
        if (File.Exists(path)) {
          realtivePath = PathUtils.ReplaceAbsolutePathToRelativePath(path);
          return new CodeAsset(FileUtils.ReadAllToBytes(path), path, realtivePath, enableDebugger ? MakeDebugJSPath(realtivePath) : path);
        }
        else
        {
          string fullPath = GetFullPathByName(pathorname);
          if (fullPath != null && File.Exists(fullPath)) {
            realtivePath = PathUtils.ReplaceAbsolutePathToRelativePath(fullPath);
            return new CodeAsset(FileUtils.ReadAllToBytes(fullPath), fullPath, realtivePath, enableDebugger ? MakeDebugJSPath(realtivePath) : fullPath);
          }
        }
      }

      if(!pathorname.EndsWith(".js"))
        return GetCodeAsset(pathorname + ".js");

      GameErrorChecker.LastError = GameError.FileNotFound;
      return null;
    }
    public override Assembly LoadCodeCSharp(string pathorname)
    {
      if (PathUtils.IsAbsolutePath(pathorname) || pathorname.StartsWith("Assets/"))
      {
        if (File.Exists(pathorname))
          return Assembly.LoadFile(pathorname);
      }
      else
      {
        string path = PackageFilePath + "/" + pathorname;
        if (File.Exists(path))
          return Assembly.LoadFile(path);
      }
      return base.LoadCodeCSharp(pathorname);
    }
    public override T GetAsset<T>(string pathorname)
    {
#if UNITY_EDITOR
      if (pathorname.StartsWith("Assets/"))
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(pathorname);
      else
      {
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(GamePathManager.DEBUG_PACKAGE_FOLDER + "/" + PackageName + "/" + pathorname);
        if (asset == null && !pathorname.Contains("/") && !pathorname.Contains("\\"))
        {
          string fullPath = GetFullPathByName(pathorname);
          if (fullPath != null)
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }
        return asset;
      }
#else
      GameErrorChecker.SetLastErrorAndLog(GameError.OnlyCanUseInEditor, TAG, "Package can only use in editor.");
      return null;
#endif
    }
  }
}
