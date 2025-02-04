using System.Collections.Generic;
using Ballance2.Game.GamePlay.Balls;
using Ballance2.Package;
using Ballance2.Services;
using Ballance2.Utils;
using UnityEngine;

namespace Ballance2.Game.GamePlay
{
  public static class GamePlayInitManager
  {
    private static List<GameObject> objects = new List<GameObject>();

    /// <summary>
    /// 游戏玩模块初始化
    /// </summary>
    /// <param name="callback">完成回调</param>
    public static void GamePlayInit(bool isPreview, GameManager.VoidDelegate callback) {
      Log.D("GamePlay", "GamePlayInit");

      var package = GamePackage.GetSystemPackage();

      GameUIManager.Instance.SetUIOverlayVisible(true);

      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("GamePlayManager.prefab"), "GamePlayManager"));
      
      GameManager.Instance.SetGameBaseCameraVisible(false);

      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("BallManager.prefab"), "GameBallsManager"));
      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("SectorManager.prefab"), "GameSectorManager"));
      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("MusicManager.prefab"), "GameMusicManager"));
      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("GameTranfoManager.prefab"), "GameTranfoManager"));
      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("LevelBriz.prefab"), "GameLevelBriz"));
      objects.Add(CloneUtils.CloneNewObject(package.GetPrefabAsset("PE_UFO.prefab"), "GameUFOAnimController"));

      //GamePlayUI
      var GamePlayUIGameObject = GameUIManager.Instance.InitViewToCanvas(package.GetPrefabAsset("GamePlayUI.prefab"), "GamePlayUI", false).gameObject;
      GamePlayUIGameObject.SetActive(false);
      objects.Add(GamePlayUIGameObject);
      //GamePlayUI
      var GamePlayPreviewUIGameObject = GameUIManager.Instance.InitViewToCanvas(package.GetPrefabAsset("GamePreviewUI.prefab"), "GamePlayPreviewUI", false).gameObject;
      GamePlayPreviewUIGameObject.SetActive(false);
      objects.Add(GamePlayPreviewUIGameObject);

      GameTimer.Delay(0.5f, () => {
        var GamePlayManagerInstance = GamePlayManager.Instance;
        GamePlayManagerInstance.BallManager = objects[1].GetComponent<BallManager>();
        GamePlayManagerInstance.SectorManager = objects[2].GetComponent<SectorManager>();
        GamePlayManagerInstance.MusicManager = objects[3].GetComponent<MusicManager>();
        GamePlayManagerInstance.CamManager = objects[1].transform.Find("BallCameraHost/CamTarget/CamOrient/MainCamera").GetComponent<CamManager>();
        GamePlayManagerInstance.BallSoundManager = objects[1].transform.Find("BallSoundManager").GetComponent<BallSoundManager>();

        if (isPreview) {
          var GamePreviewManagerInstance = GamePreviewManager.Instance;
          GamePreviewManagerInstance.MusicManager = GamePlayManagerInstance.MusicManager;
          GamePreviewManagerInstance.SectorManager = GamePlayManagerInstance.SectorManager;
          GamePreviewManagerInstance.CamManager = GamePlayManagerInstance.CamManager;
        }
        GameTimer.Delay(0.5f, () => {
          callback();
        });
      });
    } 
    /// <summary>
    /// 游戏玩模块卸载
    /// </summary>
    public static void GamePlayUnload() {
      Log.D("GamePlay", "GamePlayUnload");

      objects.ForEach((go) => Object.Destroy(go));
      objects.Clear();
    }
  }
}