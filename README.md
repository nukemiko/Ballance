# Ballance

这是2004年Arail 发布的Ballance游戏的开源重制版（制作中）。

## 先睹为快

[先看看演示视频](https://www.bilibili.com/video/BV1Dg411P7xp/)

## 简介

这是Ballance游戏的开源重制版，在制作中，还未完成，但已初具雏形。

作者是一个Ballance忠实粉丝，从最初为原版Ballance作图，到后面开发相关的小工具，最后又想让这个老游戏重新焕发生机。
这个项目从2018年就开始了，当时还在B吧里发布过一个测试版本，可惜太烂。中间又高考，停了好长时间，一直到大学快毕业才又想起来，一直想把它做好，可是因为天生拖延症，一拖再拖。到现在工作了，才终于有动力做，但想做又没有太多时间了。

本项目从2019年开始开发，中间断断续续（大学的时候玩心太重），现在工作了才终于有毅力想把它做好。这个项目目前由作者工作之余一个人做，大约5天更新一次。**非常欢迎想与我一起开发的小伙伴加入我一起做呀**。

使用Unity开发，希望可以将这个老游戏运行到手机平台上 (原版游戏是使用Virtools制作的，仅可在WindowsX86平台运行)。

游戏主要流程大都使用Lua作为语言（为了兼顾MOD与热更），游戏框架使用C#写。

---

![Demo](https://imengyu.top/assets/images/demo.png)

## 目标

* 与原版物理必须差不（2022/01 已完成）
* **ivp物理引擎**（2022/01 已完成）
* 支持MOD和自定义关卡加载（已完成）
* 支持用Lua来开发MOD（已完成）
* 发布至Steam并建设游戏的创意工坊
* 发布Android和IOS端手游

## 开发状态

目前游戏主体架构已经开发的差不多了。整体流程已经可以运行了。
可以加载关卡，加载机关，游戏UI基本完成。
目前仅剩细节优化，和剩余几个关卡的制作。

物理引擎使用ivp的源代码，发现这个这个物理引擎真的很意外。通过反编译virtools的physics.dll，然后不断搜索，通过比对，发现，曾经Valve的某个知名游戏发生
源代码泄露事件（hl2）中的物理引擎源代码，与virtools物理引擎里面的字符串居然一模一样，可以说virtools物理引擎就是这个源代码编译出来的。

后来仔细了解了下，才知道这个物理引擎ivp全名是Ipion Virtual Physics，也是很早有名的引擎了（年龄比我还大，我是00后，这个物理引擎是98年的），后来被havok收购，相关的信息应该都被封杀了，互联网上现在已经找不到了。

Virtools诞生比较早，应该也是购买了这个引擎。很幸运，找到了这个引擎，可以让重制版游戏与原版物理效果几乎一模一样的。

## 欢迎体验

目前仅有Level1体验版，仅有一个关卡，你可以先尝尝鲜。。

文件放在项目根目录 `Demo.zip` , 解压后运行 `Ballance.exe` 就可以看到效果啦。

目前可能BUG比较多，会有很多问题，不要抱太大的期待哦。。等我慢慢修复了。

![Demo1](https://imengyu.top/assets/images/demo1.png)

## 项目编辑器内运行步骤

提示：*(目前暂无Mac/Linux版本的物理引擎文件，请使用Win版本的Unity进行调试)*

1. 请下载 Unity 2021.2.7+ 版本打开项目。
2. 点击菜单“SLua”>“All”>“Make”以生成Lua相关文件。
3. 打开 System/Scenes/MainScene.unity 场景。
4. 选择 GameEntry 对象，设置“Debug Type”为“NoDebug”。
5. 点击运行，即可查看效果啦

## TODO: 项目待完成内容

* ✅ 已完成
* ❎ 完成能用但存在问题
* 🅿 功能有计划但目前暂停开发
* 🔙 功能回退旧版本
* 🅾 正在开发未完成
* 🈹 功能被割舍或不完全并暂停开发

---

* ✅ 基础系统
* ✅ 事件系统
* ✅ 操作与数据系统
* ✅ 基础系统
* ✅ 模组加载卸载
* ✅ 模组管理器
* ✅ Lua代码动态载入
* ✅ 模组包功能逻辑
* ✅ 调试命令管理器
* ✅ Lua调试功能
* ✅ 模组包打包功能
* ✅ 关卡包打包功能
* ✅ 逻辑场景
* ✅ Intro进入动画
* ✅ MenuLevel场景
* 🅿 MenuLevel的那个滚球动画
* ✅ 主菜单与设置菜单
* ✅ 关于菜单
* ✅ I18N
* ✅ 调试日志输出到unity
* ✅ core主模块独立打包装载
* ✅ BallLightningSphere球闪电动画
* ✅ BallManager球管理器主逻辑
* ✅ TranfoAminControl变球器动画逻辑
* ✅ 球碎片管理器主逻辑
* ✅ CamManager摄像机管理器主逻辑
* ✅ 关于菜单
* ✅ luac代码编译功能
* ✅ LUA 安全性
* ✅ LUA 按包鉴别
* ✅ 模块包安全系研究
* ✅ 【弃用】修复物理坐标问题
* ✅ 【弃用】修复物理约束碰撞问题
* ✅ 【弃用】物理弹簧
* ✅ 【弃用】物理滑动约束
* ✅ LevelEnd
* ✅ LevelBuilder
* ✅ 机关逻辑
* ✅ 简单机关
* ✅ 生命球和分数球机关
* ✅ SectorManager节逻辑
* ✅ GameManager相关逻辑
* ✅ 背景音乐相关逻辑
* ✅ 分数相关逻辑
* ✅ 自动归组
* ✅ ivp 物理引擎的C#包装与编译
* ✅ ivp 物理引擎初步调试成功
* ✅ 将基础球的物理从hovok迁移至ivp物理引擎
* ✅ 将机关的物理从hovok迁移至ivp物理引擎
* ✅ 重写球声音管理器
* ✅ 复杂机关 01
* ✅ 复杂机关 03
* ✅ 复杂机关 08
* ✅ 复杂机关 17
* ✅ 复杂机关 18
* ✅ 复杂机关 19
* ✅ 复杂机关 25
* ✅ 复杂机关 26
* ✅ 复杂机关 29
* ✅ 复杂机关 30
* ✅ 复杂机关 37
* ✅ 复杂机关 41
* ✅ 第1关
* ✅ 第2关
* ✅ 第3关
* ✅ 第4关
* ✅ 第5关
* ✅ 第6关
* ✅ 第7关
* 🅾 第8关
* 🅾 第9关
* 🅾 第10关
* 🅾 第11关
* 🅾 第12关
* 🅾 第13关
* ✅ 迷你机关调试环境
* 🅾 关卡管理菜单
* 🅾 模组管理菜单
* 🅾 菜单的键盘逻辑
* 🅾 第一关的教程
* 🅾 更新服务器与联网更新功能
* 🅾 手机端适配
* 🅾 Android and ios 物理模块调试
* 🅾 steam接入
* 🅾 最终整体调试
* 🅾 制作魔脓空间站的转译版本地图并测试整体系统功能
* 🅾 发布steam
* 🅾 发布其他平台

## 联系我

微信: brave_imengyu
**非常欢迎想与我一起开发的小伙伴加入我们一起做呀**
