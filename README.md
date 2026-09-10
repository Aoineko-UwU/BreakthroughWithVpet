**本作品为Steam《虚拟桌宠模拟器》二周年同人游戏，禁止无端转载与商用，转载需注明作者。**

**项目使用UnityEditor(2022.3.60f1c1) + C# 开发；项目中与虚拟桌宠本体形象有关美术资源皆为《虚拟桌宠模拟器》所有**

## 作品相关：

### 下载地址：
- Windows64位：<https://aoineko.lanzoup.com/iHKdZ32tjb4j> 
- Windows32位：<https://aoineko.lanzoup.com/iWXM832tjgza>

### 其他内容：
- 宣传视频地址：[视频链接](https://www.bilibili.com/video/BV1Whbrz5ETK/?spm_id_from=333.1387.list.card_archive.click&vd_source=b88999cf93553d5e453e91b686103c04)
- 开发者B站个人链接：[@葵猫猫neko](https://space.bilibili.com/200696277?spm_id_from=333.788.0.0)
- 开发者邮箱：2838116695@qq.com

## 脚本目录（Assets/Script）

按功能职责分类，目录名不代表命名空间。移动脚本时必须同步保留对应的 .meta 文件及 GUID。

| 目录 | 职责 |
| --- | --- |
| Core | 通用基础设施：Singleton |
| GameFlow | 菜单与游戏流程、难度管理 |
| Characters/Vpet | 桌宠行为与生命系统 |
| Characters/Enemies | 敌人行为、生命系统和生成 |
| Inventory | 物品数据、物品栏、拖拽和道具随机事件 |
| Items/Pickups | 可拾取道具及拾取触发器 |
| Items/Consumables | 食物、CD、扭蛋 |
| Items/Placeables | 方块、炸弹、弹簧 |
| Items/Projectiles | 传送珍珠 |
| World/Checkpoints | 重生点交互 |
| World/Hazards | 石锥等环境危险物 |
| World/Water | 水域相关组件 |
| UI/Common | 通用按钮与选中反馈 |
| UI/Settings | 音量、全屏设置控件 |
| UI/Inventory | 物品栏格子显示 |
| UI/HUD | 地图进度显示 |
| UI/Menu | 菜单专用控件 |
| Camera | 镜头缩放、震动 |
| Audio | 音频加载与播放 |
| VisualEffects | 飘字、粒子生命周期、闪光、暗区和背景表现 |

主要入口：[游戏流程](Assets/Script/GameFlow/GameManager.cs)、[菜单流程](Assets/Script/GameFlow/MenuManager.cs)、[桌宠行为](Assets/Script/Characters/Vpet/VpetAction.cs)、[物品栏](Assets/Script/Inventory/InventoryManager.cs)、[音频](Assets/Script/Audio/AudioManager.cs)。

后续桌宠运动控制器和状态机归入 Characters/Vpet，待实现时再创建 Movement、States 等子目录。RandomSelector 目前属于道具业务，通用工具目录待实际提取独立算法时再建立。
