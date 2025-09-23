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

## 部分脚本功能介绍(Assets/Scrpits路径下)：

### ../(杂项)
 - [GameDifficultySystem.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/GameDifficultySystem.cs)  <br>  `游戏难度系统单例脚本`
 - [FullScreenToggle.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/FullScreenToggle.cs)   <br>  `全屏功能键脚本(含PlayerPrefs)`

### ../Menu/(主菜单相关)
 - [MenuManager.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Menu/MenuManager.cs)   <br>  `游戏主菜单逻辑脚本`

### ../Gaming/(游戏全局功能相关)
 - [AudioManager.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/AudioManager.cs)   <br>  `全局音效管理器(含字典缓存与Addressable)`

### ../Gaming/Vpet(角色相关)
 - [VpetAction.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/Vpet/VpetAction.cs)   <br>  `角色行为主脚本(含角色动画和全逻辑控制)`
 - [VpetHealthSystem.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/Vpet/VpetHealthSystem.cs)   <br>  `角色生命系统脚本`
   
### ../Gaming/UI&Extra Function/(游戏功能相关)
 - [CameraScaleBar.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/UI%26Extra%20Function/CameraScaleBar.cs)   <br>  `摄像机缩放(含摄像机动画)`
 - [CameraShake.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/UI%26Extra%20Function/CameraShake.cs)   <br>  `摄像机晃动效果`
 - [TMP_Figure.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/UI%26Extra%20Function/TMP_Figure.cs)   <br>  `伤害/恢复数字UI(TextMesh Pro)`
 - [VpetRespawnPointObserver.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/UI%26Extra%20Function/VpetRespawnPointObserver.cs)   <br>  `重生点观察者(记录当前重生点)`
 - [MapProgressBar.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/UI%26Extra%20Function/MapProgressBar.cs)   <br>  `迷你地图进度条脚本`

### ../Gaming/InventorySystem/(物品栏与道具相关)
 - [InventoryManager.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/InventoryManager.cs)   <br>  `物品栏脚本(含道具生成)`
 - [DragController.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/DragController.cs)   <br>  `物品拖拽控制器脚本`
 - [RandomSelector.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/RandomSelector.cs)   <br>  `道具事件的随机选择器`
 - [SlotUI.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/SlotUI.cs)   <br>  `点击物品格的处理脚本`
 - [ItemData.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/ItemData.cs)   <br>  `可复用的可视化道具数据类脚本`
 - [Item_Block.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/Item/Item_Block.cs)   <br>  `方块类道具`
 - [Item_Gacha.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/Item/Item_Gacha.cs)   <br>  `扭蛋道具`
 - [Item_Food.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/InventorySystem/Item/Item_Food.cs)   <br>  `食物类道具`

### ../Gaming/Enemy/(敌怪相关)
 - [Enemy01_Frog.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/Enemy/Enemy01_Frog.cs)   <br>  `怪物青蛙的AI逻辑`
 - [Enemy03_Bear.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/Enemy/Enemy03_Bear.cs)   <br>  `怪物熊的AI逻辑`
 - [EnemyHealthSystem.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/Enemy/EnemyHealthSystem.cs)   <br>  `所有怪物的生命系统脚本`
 - [SpawnPoint.cs](https://github.com/Aoineko-UwU/BreakthroughWithVpet/blob/main/Assets/Script/Gaming/Enemy/SpawnPoint.cs)   <br>  `怪物刷新点逻辑`
