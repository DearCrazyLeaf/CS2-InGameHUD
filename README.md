<div>
    <a href="#中文版介绍" style="margin-left: 0; text-decoration: none;"><img src="https://img.shields.io/badge/跳转到中文版-中文介绍-red?style=for-the-badge&logo=gitbook&logoColor=white" alt="中文介绍"></a>
    <a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;"><img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/issues" target="_blank" style="margin-left: 5px; text-decoration: none;"><img src="https://img.shields.io/github/issues/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=target" alt="Issues"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/stargazers" target="_blank" style="margin-left: 5px; text-decoration: none;"><img src="https://img.shields.io/github/stars/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=githubsponsors&logoColor=white" alt="Stars"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/pulls" target="_blank" style="margin-left: 5px; text-decoration: none;"><img src="https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=git" alt="Pull Requests"></a>
</div>

---

# CS2-InGameHUD

**A customizable in-game HUD plugin for Counter-Strike 2 servers. This plugin displays various information to players in a clean, configurable format that can be positioned anywhere on the screen.**

## Features

- **Customizable HUD Position**: Players can choose from 5 different positions (Top Left, Top Right, Bottom Left, Bottom Right, and Center)
- **Toggleable Display**: Players can turn the HUD on or off with a simple command
- **Multi-Platform Support**: Compatible with both Windows and Linux CS2 servers
- **MySQL Integration**: Store player preferences and custom data
- **Localization Support**: Easy to translate to any language
- **Player Statistics**: Display ping, KDA, health, team, and more
- **Admin Announcements**: Server admins can display announcements to all players
- **Custom Data Support**: Show credits (with Store API integration), playtime, last sign-in date, and more

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [GameHUD API](https://github.com/darkerz7/CS2-GameHUD)
- [Store API](https://github.com/schwarper/cs2-store) (optional, for credits display)
- MySQL Server (optional, for storing player preferences)

## Installation

1. Download the latest release for your server platform (Windows/Linux)
2. Extract the contents to your CS2 server's `game/csgo/addons/counterstrikesharp/plugins` directory
3. Configure the plugin settings in `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json`
4. Restart your server or load the plugin

## Configuration

The plugin's configuration file (`InGameHUD.json`) contains the following settings:

```json
{
  "version": 1,                      // Don't change this - 请勿修改此值
  "font_size": 50,                   // Your font size - 字体大小
  "font_name": "Arial Bold",         // Font family name - 字体名称
  "scale": 0.1,                      // Overall HUD scale - HUD整体缩放比例
  "background_opacity": 0.6,         // Background transparency (0-1) - 背景透明度(0-1)
  "background_scale": 0.3,           // Background size relative to content - 背景相对内容的大小
  "show_kda": true,                  // Display kills/deaths/assists - 显示击杀/死亡/助攻
  "show_health": true,               // Display player health - 显示玩家生命值
  "show_team": true,                 // Display team information - 显示队伍信息
  "show_time": true,                 // Display current time - 显示当前时间
  "show_ping": true,                 // Display player ping - 显示玩家延迟
  "show_score": true,                // Display team scores - 显示队伍比分
  "show_announcement_title": true,   // Display announcement title - 显示公告标题
  "show_announcement": true,         // Display announcement content - 显示公告内容
  "text_color": "Orange",            // HUD text color - HUD文字颜色
  "mysql_connection": {              // MySQL database configuration - MySQL数据库配置
    "host": "",                      // Database hostname or IP - 数据库主机名或IP
    "port": 3306,                    // Database port - 数据库端口
    "database": "",                  // Database name - 数据库名称
    "username": "",                  // Database user - 数据库用户名
    "password": ""                   // Database password - 数据库密码
  },
  "custom_data": {                   // Custom data display settings - 自定义数据显示设置
    "credits": {                     // Store credits display - 商店点数显示
      "enabled": true                // Enable/disable credits display - 启用/禁用点数显示
    },
    "playtime": {                    // Player playtime display - 玩家游戏时长显示
      "enabled": true,               // Enable/disable playtime display - 启用/禁用游戏时长显示
      "table_name": "time_table",    // Database table name for playtime - 游戏时长数据表名
      "column_name": "time"          // Database column name for playtime - 游戏时长字段名
    },
    "signin": {                      // Last sign-in display - 上次签到显示
      "enabled": true,               // Enable/disable sign-in display - 启用/禁用签到显示
      "table_name": "signin_table",  // Database table for sign-in records - 签到记录数据表名
      "column_name": "signin_time"   // Database column for sign-in timestamp - 签到时间字段名
    }
  }
}
```

## Database Setup

If you want to use MySQL to store player preferences:

1. Create a new database or use an existing one
2. Update the MySQL settings in the config file
3. The plugin will automatically create the required tables

## Commands

- `!hud` - Toggle HUD visibility
- `!hudpos <1-5>` - Change HUD position
  - 1: Top Left
  - 2: Top Right
  - 3: Bottom Left
  - 4: Bottom Right
  - 5: Center

## Contributing

Feel free to submit issues or pull requests if you have suggestions, bug reports, or improvements.

## License

<a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;">
    <img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License">
</a>

---

# 中文版介绍

# CS2-InGameHUD

**一个用于 Counter-Strike 2 服务器的可自定义游戏内 HUD 插件。该插件以简洁、可配置的格式向玩家展示各种信息，可以放置在屏幕的任何位置。**

## 特性

- **自定义 HUD 位置**：玩家可以从 5 种不同位置中选择（左上角、右上角、左下角、右下角和中心）
- **可切换显示**：玩家可以使用简单的命令打开或关闭 HUD
- **多平台支持**：兼容 Windows 和 Linux CS2 服务器
- **MySQL 集成**：存储玩家偏好设置和自定义数据
- **本地化支持**：易于翻译成任何语言
- **玩家统计**：显示延迟、KDA、血量、队伍等信息
- **管理员公告**：服务器管理员可以向所有玩家显示公告
- **自定义数据支持**：显示积分（通过 Store API 集成）、游戏时间、上次登录日期等

## 要求

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [GameHUD API](https://github.com/darkerz7/CS2-GameHUD)
- [Store API](https://github.com/schwarper/cs2-store) (可选，用于显示玩家的积分)
- MySQL 服务器（可选，用于存储玩家偏好）

## 安装

1. 下载适用于您服务器平台（Windows/Linux）的最新版本
2. 解压内容到您的 CS2 服务器的 `game/csgo/addons/counterstrikesharp/plugins` 目录
3. 在 `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json` 中配置插件设置
4. 重启服务器或加载插件

## 配置

插件的配置文件（`InGameHUD.json`）包含以下设置：

```json
{
  "version": 1,                      // Don't change this - 请勿修改此值
  "font_size": 50,                   // Your font size - 字体大小
  "font_name": "Arial Bold",         // Font family name - 字体名称
  "scale": 0.1,                      // Overall HUD scale - HUD整体缩放比例
  "background_opacity": 0.6,         // Background transparency (0-1) - 背景透明度(0-1)
  "background_scale": 0.3,           // Background size relative to content - 背景相对内容的大小
  "show_kda": true,                  // Display kills/deaths/assists - 显示击杀/死亡/助攻
  "show_health": true,               // Display player health - 显示玩家生命值
  "show_team": true,                 // Display team information - 显示队伍信息
  "show_time": true,                 // Display current time - 显示当前时间
  "show_ping": true,                 // Display player ping - 显示玩家延迟
  "show_score": true,                // Display team scores - 显示队伍比分
  "show_announcement_title": true,   // Display announcement title - 显示公告标题
  "show_announcement": true,         // Display announcement content - 显示公告内容
  "text_color": "Orange",            // HUD text color - HUD文字颜色
  "mysql_connection": {              // MySQL database configuration - MySQL数据库配置
    "host": "",                      // Database hostname or IP - 数据库主机名或IP
    "port": 3306,                    // Database port - 数据库端口
    "database": "",                  // Database name - 数据库名称
    "username": "",                  // Database user - 数据库用户名
    "password": ""                   // Database password - 数据库密码
  },
  "custom_data": {                   // Custom data display settings - 自定义数据显示设置
    "credits": {                     // Store credits display - 商店点数显示
      "enabled": true                // Enable/disable credits display - 启用/禁用点数显示
    },
    "playtime": {                    // Player playtime display - 玩家游戏时长显示
      "enabled": true,               // Enable/disable playtime display - 启用/禁用游戏时长显示
      "table_name": "time_table",    // Database table name for playtime - 游戏时长数据表名
      "column_name": "time"          // Database column name for playtime - 游戏时长字段名
    },
    "signin": {                      // Last sign-in display - 上次签到显示
      "enabled": true,               // Enable/disable sign-in display - 启用/禁用签到显示
      "table_name": "signin_table",  // Database table for sign-in records - 签到记录数据表名
      "column_name": "signin_time"   // Database column for sign-in timestamp - 签到时间字段名
    }
  }
}
```

## 数据库设置

如果您想使用 MySQL 存储玩家偏好：

1. 创建一个新数据库或使用现有数据库
2. 更新配置文件中的 MySQL 设置
3. 插件将自动创建所需的表

## 命令

- `!hud` - 切换 HUD 可见性
- `!hudpos <1-5>` - 更改 HUD 位置
  - 1：左上角
  - 2：右上角
  - 3：左下角
  - 4：右下角
  - 5：中心

## 贡献

如果您有建议、错误报告或改进，欢迎提交 Issue 或 Pull Request。

## 许可证

<a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;">
    <img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License">
</a>
