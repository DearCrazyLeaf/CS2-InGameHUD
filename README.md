# CS2-InGameHUD

[![中文版介绍](https://img.shields.io/badge/跳转到中文版-中文介绍-red)](#中文版介绍)
[![Release](https://img.shields.io/github/v/release/DearCrazyLeaf/CS2-InGameHUD?include_prereleases&color=blueviolet)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/releases/latest)
[![License](https://img.shields.io/badge/License-GPL%203.0-orange)](https://www.gnu.org/licenses/gpl-3.0.txt)
[![Issues](https://img.shields.io/github/issues/DearCrazyLeaf/CS2-InGameHUD?color=darkgreen)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/issues)
[![Pull Requests](https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-InGameHUD?color=blue)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/pulls)
[![Downloads](https://img.shields.io/github/downloads/DearCrazyLeaf/CS2-InGameHUD/total?color=brightgreen)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/releases)
[![GitHub Stars](https://img.shields.io/github/stars/DearCrazyLeaf/CS2-InGameHUD?color=yellow)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/stargazers)

**A customizable in-game HUD plugin for Counter-Strike 2 servers that displays various information to players in a clean, configurable format that can be positioned anywhere on the screen**

![47c6624b-44d7-4832-961d-b32c54f81e3f](https://github.com/user-attachments/assets/f8c53ed9-6a19-46c4-a233-8a81f1159ce4)

## Features

- **Customizable HUD Position**: Players can choose from 5 different positions (Top Left, Top Right, Bottom Left, Bottom Right, and Center)

- **Toggleable Display**: Players can turn the HUD on or off with a simple command

- **MySQL Integration**: Store player preferences and custom data

- **Localization Support**: Easy to translate to any language

- **Player Statistics**: Display maptime, maprounds, ping, KDA, health, team, and more

- **Admin Announcements**: Server admins can display announcements to all players

- **Custom Data Support**: Show credits (with Store API integration), playtime, last sign-in date, and more

> [!WARNING]
> Since this is targeted development, credits display only works with schwarper/cs2-store plugin system!
> If you're not using this plugin, credit display is disabled by default in the HUD configuration file!
> Additionally, the last sign-in time display is customized for our own sign-in system!
> You can modify these custom content items to display other information as needed, please see the custom content description for details!

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [GameHUD API](https://github.com/darkerz7/CS2-GameHUD)
- [Store API](https://github.com/schwarper/cs2-store) (optional, for credits display)
- MySQL Server (optional, for storing player preferences)

## Installation

1. Download the latest release
2. Extract the contents to your CS2 server's `game/csgo/addons/counterstrikesharp/plugins` directory
3. Configure the plugin settings in `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json`
4. Restart your server or load the plugin

## Configuration

The plugin's configuration file (`InGameHUD.json`) contains the following settings:

```json
{
  "version": 1,                      // Don't change this
  "font_size": 20,                   // Your font size
  "font_name": "Arial Bold",         // Font family name
  "scale": 0.08,                     // Overall HUD scale
  "background_opacity": 0.2,         // Background transparency (0-1)
  "background_scale": 0.1,           // Background size relative to content
  "show_kda": true,                  // Display kills/deaths/assists
  "show_health": true,               // Display player health
  "show_team": true,                 // Display team information
  "show_time": true,                 // Display current time
  "show_map_time": true,             // Display current map time or rounds
  "map_time_mode": 0,                // Change map time or rounds mode, 0 for TimeLimit, 1 for RoundLimit
  "show_ping": true,                 // Display player ping
  "show_score": true,                // Display player scores
  "show_announcement_title": true,   // Display announcement title
  "show_announcement": true,         // Display announcement content
  "text_color": "Orange",            // HUD text color
  "mysql_connection": {              // MySQL database configuration
    "use_mysql": true,               // Enable/disable MySQL connection, set to false it will use SQLite instead
    "host": "",                      // Database hostname or IP
    "port": 3306,                    // Database port
    "database": "",                  // Database name
    "username": "",                  // Database user
    "password": ""                   // Database password
  },
  "custom_data": {                   // Custom data display settings
    "credits": {                     // Store credits display (requires schwarper/cs2-store system, disabled by default)
      "enabled": true                // Enable/disable credits display
    },
    "playtime": {                    // Player playtime display (customized, see custom content section to modify if you don't have this system)
      "enabled": true,               // Enable/disable playtime display
      "table_name": "time_table",    // Database table name for playtime
      "column_name": "time"          // Database column name for playtime
    },
    "signin": {                      // Last sign-in display (customized, see custom content section to modify if you don't have this system)
      "enabled": true,               // Enable/disable sign-in display
      "table_name": "signin_table",  // Database table for sign-in records
      "column_name": "signin_time"   // Database column for sign-in timestamp
    }
  },
  "positions": {                     // HUD preset position parameter settings
    "TopLeft": {                     // Top left position
      "x_offset": -21,               // X-axis offset, recommended range (-50 to 50), exceeding this range may cause abnormal HUD display
      "y_offset": 3,                 // Y-axis offset, recommended range (-10 to 10), exceeding this range may cause abnormal HUD display
      "z_distance": 42               // Z-axis distance, recommended range (40-70), exceeding this range may cause abnormal HUD display and possible occlusion by walls
    },
    "TopRight": {                    // Top right position
      "x_offset": 21,                // X-axis offset, recommended range (-50 to 50), exceeding this range may cause abnormal HUD display
      "y_offset": 3,                 // Y-axis offset, recommended range (-10 to 10), exceeding this range may cause abnormal HUD display
      "z_distance": 42               // Z-axis distance, recommended range (40-70), exceeding this range may cause abnormal HUD display and possible occlusion by walls
    },
    "BottomLeft": {                  // Bottom left position
      "x_offset": -21,               // X-axis offset, recommended range (-50 to 50), exceeding this range may cause abnormal HUD display
      "y_offset": -3,                // Y-axis offset, recommended range (-10 to 10), exceeding this range may cause abnormal HUD display
      "z_distance": 42               // Z-axis distance, recommended range (40-70), exceeding this range may cause abnormal HUD display and possible occlusion by walls
    },
    "BottomRight": {                 // Bottom right position
      "x_offset": 21,                // X-axis offset, recommended range (-50 to 50), exceeding this range may cause abnormal HUD display
      "y_offset": -3,                // Y-axis offset, recommended range (-10 to 10), exceeding this range may cause abnormal HUD display
      "z_distance": 42               // Z-axis distance, recommended range (40-70), exceeding this range may cause abnormal HUD display and possible occlusion by walls
    },
    "Center": {                      // Center position
      "x_offset": 0,                 // Center position does not need X-axis offset
      "y_offset": 0,                 // Center position does not need Y-axis offset
      "z_distance": 42               // Z-axis distance, recommended range (40-70), exceeding this range may cause abnormal HUD display and possible occlusion by walls
    }
  }
}
```

> [!NOTE]
> ### Please note!
> Please configure `map_time_mode` based on your server settings! `0` for `TimeLimit` mode, `1` for `RoundLimit` mode!
> If your server is based on time mode, set it to `0`, otherwise set it to `1`!
> If your server is based on round mode, set it to `1`, otherwise set it to `0`!

## Custom Content Module

### This is a special module system for retrieving specific content from database tables to display corresponding information. Here's a detailed description:
#### Current module development is very limited; you can add your own desired functionality and submit Pull requests, or clone it for personal use

| Parameter              | Description                                                                 |
|------------------------|-----------------------------------------------------------------------------|
| `credits`              | Credits display module, developed only for schwarper/cs2-store system, displays player's store credits |
| `playtime`             | Playtime module, developed only for k4system, displays player's game time recorded in k4system |
| `signin`               | Recent sign-in time module, developed only for our sign-in system, displays most recent sign-in time |
| `enabled`              | Whether to enable display, `true` enables, `false` disables. Note: using these features requires properly set tables and database connection! |
| `table_name`           | Target table name to retrieve data from |
| `column_name`          | Target column name (field) to retrieve data from |

### How to use this module
- The module's design logic is to retrieve the current player's `steamid`, match it with your specified table name and column name, get the corresponding player data from that table's column, and display it with a custom title through the language file. Currently, only two parameter types are provided: time and date.

- Currently, only `playtime` and `signin` modules can be modified, with limitations due to targeted development.

- In `playtime`, the table's `steamid` field name must be `steam_id`, and the data must be in seconds. The calculation method automatically converts it to `n hours n minutes` and prints it on the HUD.

- In `signin`, the table's `steamid` field name must be `steamid64`, and the data must be in standard date format. The calculation method computes the difference between the retrieved data and the query time, retaining only the day parameter difference, and finally displays `n days ago` or `today`.

- After modifying these parameters and correctly matching column names, please modify the language file at `...\addons\counterstrikesharp\plugins\CS2-InGameHUD\lang`. For example, for `en`:

```json
{
  "hud.greeting": "Hello! [{0}]",
  "hud.separator": "===================",
  "hud.map_time_remaining": "Map Time: {0}m{1}s",
  "hud.map_round_info": "Round: {0}/{1}",
  "hud.warmuptime": "Map Status: Warm-Up",
  "hud.current_time": "Current Time: {0}",
  "hud.ping": "Ping: {0} ms",
  "hud.kda": "KDA: {0}/{1}/{2}",
  "hud.health": "Health: {0}",
  "hud.team": "Team: {0}",
  "hud.score": "Score: {0}",
  "hud.credits": "Credits: {0}",
  "hud.last_signin": "Last Sign-in: {0}",     // Change "Last Sign-in" to your desired title to adapt to data retrieved from your table. Don't modify {0}!
  "hud.never_signed": "Never signed in or data anomaly",
  "hud.today": "Today",
  "hud.days_ago": "{0} days ago",
  "hud.playtime": "Playtime: {0}h {1}m",      // Change "Playtime" to your desired title to adapt to data retrieved from your table. Don't modify {0}{1}!
  "hud.separator_bottom": "===================",
  "hud.hint_toggle": "Custom message, you can write, !hud to toggle panel",
  "hud.hint_help": "Custom message, you can write, !help for help",
  "hud.hint_store": "Custom message, you can write, !store to open shop",
  "hud.hint_website": "Custom message, preset for website.",
  "hud.separator_final": "===================",
  "hud.announcement_title": "[ANNOUNCEMENT TITLE]",
  "hud.announcement_content": "Announcement content 1\nAnnouncement content 2\nAnnouncement content 3\nAnd so on",  // \n is a newline character
  "hud.team_t": "T",
  "hud.team_ct": "CT",
  "hud.team_spec": "Spectator",
  "hud.enabled": "{White}HUD {Lime}Enabled{White}!",
  "hud.disabled": "{White}HUD {Lime}Disabled{White}!",
  "hud.invalid_state": "{Red}Cannot enable HUD in current state (dead or spectating)!",
  "hud.position_usage": "{White}Usage: {Lime}!hudpos {White}<{Lime}1-5{White}>",
  "hud.position_help": "{Lime}1{White}:TopLeft  {Lime}2{White}:TopRight  {Lime}3{White}:BottomLeft  {Lime}4{White}:BottomRight  {Lime}5{White}:Center",
  "hud.position_changed": "{White}HUD position {Lime}changed{White}!",
  "hud.position_invalid": "{White}Invalid position! Please use {Lime}1-5{White}!"
}
```
- Change "Last Sign-in" to your desired title to adapt to data retrieved from your table. Don't modify `{0}`!

- Change "Playtime" to your desired title to adapt to data retrieved from your table. Don't modify `{0}` `{1}`!

- You can investigate how to modify other custom content (such as ping, KDA, team, etc., but do not modify the numbers after them, as these are parameter content!). Supports CounterStrikeSharp's native color display:


![image](https://github.com/user-attachments/assets/7471300a-d5a1-4690-81c4-25fe88ac34cd)

#### Sample image from [oqyh/cs2-Kill-Sound-GoldKingZ](https://github.com/oqyh/cs2-Kill-Sound-GoldKingZ?tab=readme-ov-file) 's repository (because I'm a bit lazy)

> [!WARNING]
> Do not add color information to text in the announcement content or above, as those are displayed on the HUD and color codes won't work! 
> HUD text only supports changing all text color at once, which must be set in the configuration file!

## HUD Preset Position Parameter Settings
### This is a HUD preset position parameter setting block for adjusting the HUD's display position on the screen. See the detailed description below.
#### Introduction: The HUDAPI's printing logic is based on the player's current view, calculating the HUD's position through X, Y, Z axis offsets, hence this preset position parameter setting block.

#### The preset position parameters are designed to allow you to easily adjust the HUD's position on the screen without needing to manually calculate offsets. The recommended ranges for X, Y, and Z offsets are provided to ensure proper display without obstruction by walls or other objects.

| Parameter Name       | Description                                                                |
|----------------------|----------------------------------------------------------------------------|
| `TopLeft`            | Top left position settings                                                 |
| `TopRight`           | Top right position settings                                                |
| `BottomLeft`         | Bottom left position settings                                              |
| `BottomRight`        | Bottom right position settings                                             |
| `Center`             | Center position settings                                                   |
| `x_offset`           | X-axis offset for the HUD position, recommended range (-50 to 50)          |
| `y_offset`           | Y-axis offset for the HUD position, recommended range (-10 to 10)          |
| `z_distance`         | Z-axis distance for the HUD position, recommended range (40 to 70)         |

### Usage Instructions
- This module's design logic uses 5 different preset positions with specific offsets and distances to adjust the HUD display position on screen

- You can modify these parameters according to your needs for fine-tuning and different display effects

- The presets have been tested and work well for most screen resolutions and aspect ratios, but if you have special requirements, you can adjust them

- The preset parameter values provide optimal results and have recommended ranges; exceeding these ranges may cause abnormal HUD display or occlusion by walls, so modify with caution

- Players can use the command !hudpos <1-5> in-game to switch between HUD positions, where parameters 1-5 correspond to the preset positions above. If a database is configured, player settings will be saved and persist upon rejoining the game

> [!WARNING]
> Please note: If you change the font parameter settings in the configuration (font_size and scale),
> the HUD will recalculate positions based on the new font size and scale, which may affect the display on screen!
> It's recommended to use the preset values unless you have special requirements!

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

Feel free to submit issues or pull requests if you have any questions, suggestions, or would like to contribute to the project.

## License

<a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;">
    <img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License">
</a>

---

# 中文版介绍 
### >[中文论坛帖子](https://bbs.csgocn.net/thread-1029.htm)<

# CS2-InGameHUD

[![English Version](https://img.shields.io/badge/Back_to_English-English-blue)](#CS2-InGameHUD)
[![Release](https://img.shields.io/github/v/release/DearCrazyLeaf/CS2-InGameHUD?include_prereleases&color=blueviolet&label=最新版本)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/releases/latest)
[![License](https://img.shields.io/badge/许可证-GPL%203.0-orange)](https://www.gnu.org/licenses/gpl-3.0.txt)
[![Issues](https://img.shields.io/github/issues/DearCrazyLeaf/CS2-InGameHUD?color=darkgreen&label=反馈)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/issues)
[![Pull Requests](https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-InGameHUD?color=blue&label=请求)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/pulls)
[![Downloads](https://img.shields.io/github/downloads/DearCrazyLeaf/CS2-InGameHUD/total?color=brightgreen&label=下载)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/releases)
[![GitHub Stars](https://img.shields.io/github/stars/DearCrazyLeaf/CS2-InGameHUD?color=yellow&label=标星)](https://github.com/DearCrazyLeaf/CS2-InGameHUD/stargazers)

**一个用于 Counter-Strike 2 服务器的可自定义游戏内 HUD 插件，该插件以简洁、可配置的格式向玩家展示各种信息，可以放置在屏幕的任何位置**

![a1cb7966-3c40-419d-89bd-69b4311a025a](https://github.com/user-attachments/assets/c2ef4fc1-64f0-4421-876f-7ff6f40934eb)

## 特性

- **自定义 HUD 位置**：玩家可以从 5 种不同位置中选择（左上角、右上角、左下角、右下角和中心）

- **可切换显示**：玩家可以使用简单的命令打开或关闭 HUD

- **MySQL 集成**：存储玩家偏好设置和自定义数据

- **本地化支持**：易于翻译成任何语言

- **玩家统计**：显示地图时间，回合，延迟、KDA、血量、队伍等信息

- **管理员公告**：服务器管理员可以向所有玩家显示公告

- **自定义数据支持**：显示积分（通过 Store API 集成）、游戏时间、上次登录日期等

> [!WARNING]
> 因为是定向开发，所以只能使用 schwarper/cs2-store 的插件系统来显示对应的玩家积分！
> 如果你没有在使用该插件，本HUD配置文件中默认关闭积分显示！
> 此外，对于上次签到时间的显示，这是对于适配我们自己开发的签到系统而定制的！
> 你可以修改这些自定义内容为其他想要显示的东西，具体请查看自定义内容介绍！

## 要求

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [GameHUD API](https://github.com/darkerz7/CS2-GameHUD)
- [Store API](https://github.com/schwarper/cs2-store) (可选，用于显示玩家的积分)
- MySQL 服务器（可选，用于存储玩家偏好）

## 安装

1. 下载最新版本
2. 解压内容到您的 CS2 服务器的 `game/csgo/addons/counterstrikesharp/plugins` 目录
3. 在 `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json` 中配置插件设置
4. 重启服务器或加载插件

## 配置

插件的配置文件（`InGameHUD.json`）包含以下设置：

```json
{
  "version": 1,                      // 请勿修改此值
  "font_size": 20,                   // 字体大小
  "font_name": "Arial Bold",         // 字体名称
  "scale": 0.08,                     // HUD整体缩放比例
  "background_opacity": 0.2,         // 背景透明度(0-1)
  "background_scale": 0.1,           // 背景相对内容的大小
  "show_kda": true,                  // 显示击杀/死亡/助攻
  "show_health": true,               // 显示玩家生命值
  "show_team": true,                 // 显示队伍信息
  "show_time": true,                 // 显示当前时间
  "show_map_time": true,             // 显示当前地图剩余时间或回合
  "map_time_mode": 0,                // 切换地图剩余时间或回合模式，0为剩余时间，1为剩余回合
  "show_ping": true,                 // 显示玩家延迟
  "show_score": true,                // 显示玩家得分
  "show_announcement_title": true,   // 显示公告标题
  "show_announcement": true,         // 显示公告内容
  "text_color": "Orange",            // HUD文字颜色
  "mysql_connection": {              // MySQL数据库配置
    "use_mysql": true,               // 启用/禁用 MySQL 连接，设置为 false 则使用 SQLite
    "host": "",                      // 数据库主机名或IP
    "port": 3306,                    // 数据库端口
    "database": "",                  // 数据库名称
    "username": "",                  // 数据库用户名
    "password": ""                   // 数据库密码
  },
  "custom_data": {                   // 自定义数据显示设置
    "credits": {                     // 商店点数显示（必须基于schwarper/cs2-store的商店系统，默认关闭）
      "enabled": true                // 启用/禁用点数显示
    },
    "playtime": {                    // 玩家游戏时长显示（定制，如果您没有该系统请看自定义内容介绍来更换）
      "enabled": true,               // 启用/禁用游戏时长显示
      "table_name": "time_table",    // 游戏时长数据表名
      "column_name": "time"          // 游戏时长字段名
    },
    "signin": {                      // 上次签到显示（定制，如果您没有该系统请看自定义内容介绍来更换）
      "enabled": true,               // 启用/禁用签到显示
      "table_name": "signin_table",  // 签到记录数据表名
      "column_name": "signin_time"   // 签到时间字段名
    }
  },
  "positions": {                     // HUD预设位置参数设置
    "TopLeft": {                     // 左上角位置
      "x_offset": -21,               // X轴偏移，推荐范围（-50-50），超出此范围可能会导致HUD显示异常
      "y_offset": 3,                 // Y轴偏移，推荐范围（-10-10），超出此范围可能会导致HUD显示异常
      "z_distance": 42               // Z轴距离，推荐范围（40-70），超出此范围可能会导致HUD显示异常，并且可能会被墙体遮挡
    },
    "TopRight": {                    // 右上角位置
      "x_offset": 21,                // X轴偏移，推荐范围（-50-50），超出此范围可能会导致HUD显示异常
      "y_offset": 3,                 // Y轴偏移，推荐范围（-10-10），超出此范围可能会导致HUD显示异常
      "z_distance": 42               // Z轴距离，推荐范围（40-70），超出此范围可能会导致HUD显示异常，并且可能会被墙体遮挡
    },
    "BottomLeft": {                  // 左下角位置
      "x_offset": -21,               // X轴偏移，推荐范围（-50-50），超出此范围可能会导致HUD显示异常
      "y_offset": -3,                // Y轴偏移，推荐范围（-10-10），超出此范围可能会导致HUD显示异常
      "z_distance": 42               // Z轴距离，推荐范围（40-70），超出此范围可能会导致HUD显示异常，并且可能会被墙体遮挡
    },
    "BottomRight": {                 // 右下角位置
      "x_offset": 21,                // X轴偏移，推荐范围（-50-50），超出此范围可能会导致HUD显示异常
      "y_offset": -3,                // Y轴偏移，推荐范围（-10-10），超出此范围可能会导致HUD显示异常
      "z_distance": 42               // Z轴距离，推荐范围（40-70），超出此范围可能会导致HUD显示异常，并且可能会被墙体遮挡
    },
    "Center": {                      // 居中位置
      "x_offset": 0,                 // 居中位置不需要X轴偏移
      "y_offset": 0,                 // 居中位置不需要Y轴偏移
      "z_distance": 42               // Z轴距离，推荐范围（40-70），超出此范围可能会导致HUD显示异常，并且可能会被墙体遮挡
    }
  }
}
```

> [!NOTE]
> ### 请注意！
> 请基于您的服务器设置来配置`map_time_mode`！`0`为`TimeLimit`模式，`1`为`RoundLimit`模式！
> 如果您的服务器基于时间模式，请将其设置为`0`，否则请设置为`1`！
> 如果您的服务器基于回合制模式，请将其设置为`1`，否则请设置为`0`！

## 自定义内容模块

### 这是一个特殊的模块系统，用于获取指定数据库表中特定的内容来显示对应的信息，以下是详细介绍
#### 目前模块开发很有限，你可以自行添加想要的功能，然后提交Pull requests，或者是克隆当作自己使用

| 参数名称              | 描述                                                                          |
|-----------------------|-------------------------------------------------------------------------------|
| `credits`             | 积分显示模块，仅开发用于schwarper/cs2-store的商店系统，显示玩家商店中玩家拥有的积分 |
| `playtime`            | 游戏时长模块，仅开发用于k4system，用于显示k4系统中记录的玩家游戏时长               |
| `signin`              | 最近签到时间模块，仅开发用于我们自己的签到系统，用于显示最近一次的签到时间           |
| `enabled`             | 是否开启显示，`true`开启显示，`false`关闭，请注意，使用这些功能一定要设置好表并且连接好数据库，否则无效！|
| `table_name`          | 要获取的目标表名称                                                              |
| `column_name`         | 要获取的目标列名称（字段）                                                       |

### 对于这个模块的使用方法
- 这个模块设计逻辑是，通过获取当前玩家的`steamid`，然后匹配你设置的表名称，列名称，获取对应玩家在这张表指定的列中的数据，然后通过lang文件自定义标题来显示这个数据，目前只提供了两种参数类型，时间和日期

- 目前仅提供修改`playtime`，`signin`两个模块，且存在限制，因为是定向开发

- 在`playtime`中，匹配的表中记录的`steamid`字段名称为`steam_id`，且数据必须是以秒为单位，计算方法会自动计算成`n小时n分钟`然后打印在HUD上

- 在`signin`中，匹配的表中记录的`steamid`字段名称为`steamid64`，且数据必须是标准日期格式，计算方法会根据日期计算获取的数据和查询的时刻日期差距，然后仅保留day参数的差距，最后打印显示`n天前`，`今天`的信息

- 修改完上述参数并且确切匹配了列名称之后，请修改位于`...\addons\counterstrikesharp\plugins\CS2-InGameHUD\lang`下对应的语言文件，以`zh-Hans`为例：

```json
{
  "hud.greeting": "你好！【{0}】",
  "hud.separator": "===================",
  "hud.map_time_remaining": "地图剩余: {0}分{1}秒",
  "hud.map_round_info": "回合剩余: {0}/{1}",
  "hud.warmuptime": "地图状态: 热身中",
  "hud.current_time": "当前时间: {0}",
  "hud.ping": "延迟: {0} ms",
  "hud.kda": "战绩: {0}/{1}/{2}",
  "hud.health": "生命值: {0}",
  "hud.team": "阵营: {0}",
  "hud.score": "得分: {0}",
  "hud.credits": "积分: {0}",
  "hud.last_signin": "上次签到: {0}",         // 修改"上次签到"为你想要显示的标题来适配从你数据表中获取的数据，请勿修改{0}!
  "hud.never_signed": "从未签到或数据异常",
  "hud.today": "今天",
  "hud.days_ago": "{0}天前",
  "hud.playtime": "游玩时长: {0}小时{1}分钟",  // 修改"游玩时长"为你想要显示的标题来适配从你数据表中获取的数据，请勿修改{0}{1}!
  "hud.separator_bottom": "===================",
  "hud.hint_toggle": "自定义信息，你可以写，!hud开关面板",
  "hud.hint_help": "自定义信息，你可以写，!help查看帮助",
  "hud.hint_store": "自定义信息，你可以写，!store打开商店",
  "hud.hint_website": "自定义信息，预设是填写网站，你可以写QQ群等",
  "hud.separator_final": "===================",
  "hud.announcement_title": "【公告标题】",
  "hud.announcement_content": "公告内容1\n公告内容2\n公告内容3\n以此类推",  // \n是换行符
  "hud.team_t": "T",
  "hud.team_ct": "CT",
  "hud.team_spec": "观察",
  "hud.enabled": "{White}HUD{Lime}已启用{White}！",
  "hud.disabled": "{White}HUD{Lime}已禁用{White}！",
  "hud.invalid_state": "{Red}当前状态无法启用HUD（死亡或观察状态）！",
  "hud.position_usage": "{White}用法: {Lime}!hudpos {White}<{Lime}1-5{White}>",
  "hud.position_help": "{Lime}1{White}:左上  {Lime}2{White}:右上  {Lime}3{White}:左下  {Lime}4{White}:右下  {Lime}5{White}:居中",
  "hud.position_changed": "{White}HUD位置{Lime}已更改{White}！",
  "hud.position_invalid": "{White}无效的位置! 请使用{Lime}1-5{White}！"
}
```
- 修改"上次签到"为你想要显示的标题来适配你数据表中获取的数据，请勿修改`{0}`!

- 修改"游玩时长"为你想要显示的标题来适配你数据表中获取的数据，请勿修改`{0}` `{1}`!

- 其余的自定义内容你可以自行考究如何修改（比如延迟，战绩，阵营等内容，但是切勿修改后面的数字，因为这是显示的参数内容！），支持CounterStrikeSharp原生的颜色显示：


![image](https://github.com/user-attachments/assets/7471300a-d5a1-4690-81c4-25fe88ac34cd)

#### 示例图片来自 [oqyh/cs2-Kill-Sound-GoldKingZ](https://github.com/oqyh/cs2-Kill-Sound-GoldKingZ?tab=readme-ov-file) 的仓库（因为比较懒）

> [!WARNING]
> 不要在包括公告内容及以上的所有文字前添加颜色标记，因为那些是显示在hud上的内容，不会生效！
> HUD的文字仅支持全体更换，在配置文件中单独设置！

## HUD预设位置参数设置
### 这是一个HUD预设位置参数的设置区块，用于调整HUD在屏幕上显示的预设位置调整，以下是详细介绍

#### 简介：由于HUDAPI的打印逻辑是基于玩家当前视角，通过X,Y,Z轴的偏移量来计算HUD的位置，所以有了这个预设位置参数的设置区块

| 参数名称              | 描述                                                                          |
|-----------------------|-------------------------------------------------------------------------------|
| `TopLeft`             | 左上角位置设置                                                                |
| `TopRight`            | 右上角位置设置                                                                |
| `BottomLeft`          | 左下角位置设置                                                                |
| `BottomRight`         | 右下角位置设置                                                                |
| `Center`              | 居中位置设置                                                                  |
| `x_offset`            | X轴偏移量，推荐范围（-50-50），超出此范围可能会导致HUD显示异常                     |
| `y_offset`            | Y轴偏移量，推荐范围（-10-10），超出此范围可能会导致HUD显示异常                     |
| `z_distance`          | Z轴距离，推荐范围（40-70），超出此范围可能会导致HUD显示异常，并且可能会被墙体遮挡 |

#### 使用方法
- 这个模块的设计逻辑是通过插件预设好的5个不同位置的偏移量和距离来调整HUD在屏幕上的显示位置

- 你可以根据自己的需求修改这些参数来微调，形成不同的显示效果

- 预设已经过测试，适用于大多数屏幕分辨率和比例，但如果你有特殊需求，可以自行调整

- 预设的参数值已是最佳效果，且已经给定了推荐范围，超出此范围可能会导致HUD显示异常或被墙体遮挡，请谨慎修改

- 玩家在服务器内可以使用命令`!hudpos <1-5>`来切换HUD位置，参数1-5对应上述预设位置，如果配置了数据库，玩家的设置会被保存到数据库中，重新进入游戏后仍然有效

> [!WARNING]
> 请注意：如果你更改了配置中的字体参数设置（`font_size`和`scale`）
> 那么HUD会根据新的字体大小和缩放比例重新计算位置，这可能会影响HUD在屏幕上的显示效果！
> 推荐使用预设值，除非你有特殊需求！

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
