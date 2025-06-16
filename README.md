<div>
    <a href="#中文版介绍" style="margin-left: 0; text-decoration: none;"><img src="https://img.shields.io/badge/跳转到中文版-中文介绍-red?style=for-the-badge&logo=gitbook&logoColor=white" alt="中文介绍"></a>
    <a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;"><img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/issues" target="_blank" style="margin-left: 10px; text-decoration: none;"><img src="https://img.shields.io/github/issues/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=github" alt="Issues"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/stargazers" target="_blank" style="margin-left: 10px; text-decoration: none;"><img src="https://img.shields.io/github/stars/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=github" alt="Stars"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/pulls" target="_blank" style="margin-left: 10px; text-decoration: none;"><img src="https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=git" alt="Pull Requests"></a>
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

- CounterStrikeSharp (API Version 1.0.318 or higher)
- GameHUD API
- MySQL Server (optional, for storing player preferences)
- Store API (optional, for credits display)

## Installation

1. Download the latest release for your server platform (Windows/Linux)
2. Extract the contents to your CS2 server's `game/csgo/addons/counterstrikesharp/plugins` directory
3. Configure the plugin settings in `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json`
4. Restart your server or load the plugin

## Configuration

The plugin's configuration file (`InGameHUD.json`) contains the following settings:

```json
{
  "version": 1,
  "font_size": 50,
  "font_name": "Arial Bold",
  "scale": 0.1,
  "background_opacity": 0.6,
  "background_scale": 0.3,
  "show_kda": true,
  "show_health": true,
  "show_team": true,
  "show_time": true,
  "show_ping": true,
  "show_score": true,
  "show_announcement_title": true,
  "show_announcement": true,
  "text_color": "Orange",
  "mysql_connection": {
    "host": "",
    "port": ,
    "database": "",
    "username": "",
    "password": ""
  },
  "custom_data": {
    "credits": {
      "enabled": true
    },
    "playtime": {
      "enabled": true,
      "table_name": "time_table",
      "column_name": "time"
    },
    "signin": {
      "enabled": true,
      "table_name": "signin_table",
      "column_name": "signin_time"
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

- CounterStrikeSharp (API 版本 1.0.318 或更高)
- GameHUD API
- MySQL 服务器（可选，用于存储玩家偏好）
- Store API（可选，用于显示积分）

## 安装

1. 下载适用于您服务器平台（Windows/Linux）的最新版本
2. 解压内容到您的 CS2 服务器的 `game/csgo/addons/counterstrikesharp/plugins` 目录
3. 在 `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json` 中配置插件设置
4. 重启服务器或加载插件

## 配置

插件的配置文件（`InGameHUD.json`）包含以下设置：

```json
{
  "version": 1,
  "font_size": 50,
  "font_name": "Arial Bold",
  "scale": 0.1,
  "background_opacity": 0.6,
  "background_scale": 0.3,
  "show_kda": true,
  "show_health": true,
  "show_team": true,
  "show_time": true,
  "show_ping": true,
  "show_score": true,
  "show_announcement_title": true,
  "show_announcement": true,
  "text_color": "Orange",
  "mysql_connection": {
    "host": "",
    "port": ,
    "database": "",
    "username": "",
    "password": ""
  },
  "custom_data": {
    "credits": {
      "enabled": true
    },
    "playtime": {
      "enabled": true,
      "table_name": "time_table",
      "column_name": "time"
    },
    "signin": {
      "enabled": true,
      "table_name": "signin_table",
      "column_name": "signin_time"
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
