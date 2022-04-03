# Ballance

## 简介

这是 Ballance 游戏的开源 Unity 重制版.

![image](/Assets/System/Textures/splash_app.bmp)

---

## 文档

[完整文档可以参考这里](http://ballance-docs.imengyu.top/#/readme)

[API文档参考这里](http://ballance-docs.imengyu.top/#/LuaApi/readme)

[完整文档可以参考这里 - Github Pages](https://imengyu.github.io/Ballance/#/readme)

[API文档参考这里 - Github Pages](https://imengyu.github.io/Ballance/#/LuaApi/readme)

## 系统需求

||最低配置|推荐配置|
|---|---|---|
|操作系统|Windows 7-11|Windows 7-11|
|处理器|Dual core 3Ghz+|Quad core 3Ghz+|
|内存|1 GB RAM|2 GB RAM|
|显卡|DirectX 10.1 capable GPU with 512 MB VRAM - GeForce GTX 260, Radeon HD 4850 or Intel HD Graphics 5500|DirectX 11 capable GPU with 2 GB VRAM - GeForce GTX 750 Ti, Radeon R7 360|
|DirectX 版本|11|11|
|存储空间|100 MB 可用空间|200 MB 可用空间|

## 游戏相册

原版关卡

![Demo](docs/DemoImages/11.jpg)
![Demo](docs/DemoImages/12.jpg)
![Demo](docs/DemoImages/13.jpg)
![Demo](docs/DemoImages/14.jpg)
![Demo](docs/DemoImages/18.jpg)
![Demo](docs/DemoImages/9.jpg)
![Demo](docs/DemoImages/6.jpg)
![Demo](docs/DemoImages/7.jpg)
![Demo](docs/DemoImages/15.jpg)
![Demo](docs/DemoImages/16.jpg)
![Demo](docs/DemoImages/17.jpg)

13关的大螺旋

![Demo](docs/DemoImages/9.gif)
![Demo](docs/DemoImages/10.png)

（转译版）自制地图（魔脓空间站）

![Demo](docs/DemoImages/3.jpg)
![Demo](docs/DemoImages/4.jpg)
![Demo](docs/DemoImages/5.jpg)

关卡预览器查看13关

![Demo](docs/DemoImages/8.jpg)

用关卡预览器查看自制地图

![Demo](docs/DemoImages/1.jpg)
![Demo](docs/DemoImages/2.jpg)

## 开发状态

目前游戏主体架构已经开发的差不多了。整体流程已经可以运行了。
可以加载关卡，加载机关，游戏UI，游戏流程基本完成。
目前仅剩细节优化。

<details>
<summary>关于作者</summary>

贴吧ID: q717021

作者是一个Ballance忠实粉丝，从最初为原版Ballance作图，到后面开发相关的小工具，最后又想让这个老游戏重新焕发生机。
这个项目从2018年就开始了，当时还在B吧里发布过一个测试版本，可惜太烂。中间又高考，停了好长时间，一直到大学快毕业才又想起来，一直想把它做好，可是因为天生拖延症，一拖再拖。到现在工作了，才终于有动力做。

现在只有作者一个人为爱编写这个游戏，非常欢迎加入我一起开发呀。

</details>

<details>
<summary>关于物理引擎</summary>

物理引擎使用ivp的源代码，发现这个这个物理引擎真的很意外。通过反编译virtools的physics.dll，然后不断搜索，通过比对，发现，曾经Valve的某个知名游戏发生
源代码泄露事件（hl2）中的物理引擎源代码，与virtools物理引擎里面的字符串居然一模一样，可以说virtools物理引擎就是这个源代码编译出来的。

[物理引擎的C++源代码可以到这里查看](https://github.com/nillerusr/source-physics) (这个不是作者本人的仓库，我在这里复制了一份用来编译).

后来仔细了解了下，才知道这个物理引擎ivp全名是Ipion Virtual Physics，也是很早有名的引擎了（年龄比我还大，我是00后，这个物理引擎是98年的），后来被havok收购，相关的信息应该都被封杀了，互联网上现在已经找不到了。

Virtools诞生比较早，应该也是购买了这个引擎。很幸运，找到了这个引擎，可以让重制版游戏与原版物理效果几乎一模一样的。
</details>

---

## TODO: 项目待完成内容

* ✅ 已完成
* ❎ 完成能用但存在问题
* 🅿 功能有计划但目前暂停开发

---

<details>
<summary>很早就完成的内容</summary>

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
* ✅ MenuLevel的那个滚球动画
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
* ✅ 第8关
* ✅ 第9关
* ✅ 第10关
* ✅ 第11关
* ✅ 第12关
* ✅ 第13关
* ✅ 迷你机关调试环境
* ✅ 模组管理菜单
* ✅ 关卡管理菜单
* ✅ 关于菜单
* ✅ 手机端适配
* ✅ 过关后才能进入下一关
* ✅ 第一关的教程
* ✅ 菜单的键盘逻辑
* ✅ 手机方向键盘

</details>

---

* ✅ 更换Shader并尽量接近原版材质效果
* ✅ 自定义关卡制作教程文档
* ✅ 添加球的阴影
* ✅ 关卡预览器
* ✅ 制作魔脓空间站的转译版本地图并测试整体系统功能
* 🅾 最终整体调试
* 🅿 自定义模组开发教程文档

---

* 下面的内容不一定会完成，看大家喜不喜欢，如果大家还喜欢这个游戏，我就继续完善下去
* 🅿 Android and ios 物理模块调试
* 🅿 steam接入
* 🅿 发布steam
* 🅿 发布其他平台
* 🅿 更新服务器与联网更新功能
* 🅿 联机玩（关卡，模组，分数共享平台，多人游戏）

## 联系我

wechart: brave_imengyu