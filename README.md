<div>
    <a href="#���İ����" style="margin-left: 0; text-decoration: none;"><img src="https://img.shields.io/badge/��ת�����İ�-���Ľ���-red?style=for-the-badge&logo=gitbook&logoColor=white" alt="���Ľ���"></a>
    <a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;"><img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/issues" target="_blank" style="margin-left: 5px; text-decoration: none;"><img src="https://img.shields.io/github/issues/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=target" alt="Issues"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/stargazers" target="_blank" style="margin-left: 5px; text-decoration: none;"><img src="https://img.shields.io/github/stars/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=githubsponsors&logoColor=white" alt="Stars"></a>
    <a href="https://github.com/DearCrazyLeaf/CS2-InGameHUD/pulls" target="_blank" style="margin-left: 5px; text-decoration: none;"><img src="https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-InGameHUD?style=for-the-badge&logo=git" alt="Pull Requests"></a>
</div>

---

# CS2-InGameHUD

**A customizable in-game HUD plugin for Counter-Strike 2 servers that displays various information to players in a clean, configurable format that can be positioned anywhere on the screen**

![a1cb7966-3c40-419d-89bd-69b4311a025a](https://github.com/user-attachments/assets/c2ef4fc1-64f0-4421-876f-7ff6f40934eb)

## Features

- **Customizable HUD Position**: Players can choose from 5 different positions (Top Left, Top Right, Bottom Left, Bottom Right, and Center)

- **Toggleable Display**: Players can turn the HUD on or off with a simple command

- **MySQL Integration**: Store player preferences and custom data

- **Localization Support**: Easy to translate to any language

- **Player Statistics**: Display ping, KDA, health, team, and more

- **Admin Announcements**: Server admins can display announcements to all players

- **Custom Data Support**: Show credits (with Store API integration), playtime, last sign-in date, and more

> [!WARNING]
> Since this is targeted development, credits display only works with schwarper/cs2-store plugin system!
> If you're not using this plugin, credit display is disabled by default in the HUD configuration file!
> Additionally, the last sign-in time display is customized for our own sign-in system!
> You can modify these custom content items to display other information as needed, please see the custom content description for details!

> [!NOTE]
> # Help wanted!
> Need HUD resolution adaptation.
> Due to the tricky logic of HUD display, I have no clue how to make it adapt to different resolutions.
> I can only use hard-coded presets to adjust suitable positions.
> If anyone can add resolution support to the plugin so that these presets work properly under different aspect ratios, please submit a Pull request to the repository! Thank you very much!

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
  "font_size": 50,                   // Your font size
  "font_name": "Arial Bold",         // Font family name
  "scale": 0.1,                      // Overall HUD scale
  "background_opacity": 0.6,         // Background transparency (0-1)
  "background_scale": 0.3,           // Background size relative to content
  "show_kda": true,                  // Display kills/deaths/assists
  "show_health": true,               // Display player health
  "show_team": true,                 // Display team information
  "show_time": true,                 // Display current time
  "show_ping": true,                 // Display player ping
  "show_score": true,                // Display player scores
  "show_announcement_title": true,   // Display announcement title
  "show_announcement": true,         // Display announcement content
  "text_color": "Orange",            // HUD text color
  "mysql_connection": {              // MySQL database configuration
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
  }
}
```

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

# ���İ���� 
### >[������̳����](https://bbs.csgocn.net/thread-1029.htm)<

# CS2-InGameHUD

**һ������ Counter-Strike 2 �������Ŀ��Զ�����Ϸ�� HUD ������ò���Լ�ࡢ�����õĸ�ʽ�����չʾ������Ϣ�����Է�������Ļ���κ�λ��**

![a1cb7966-3c40-419d-89bd-69b4311a025a](https://github.com/user-attachments/assets/c2ef4fc1-64f0-4421-876f-7ff6f40934eb)

## ����

- **�Զ��� HUD λ��**����ҿ��Դ� 5 �ֲ�ͬλ����ѡ�����Ͻǡ����Ͻǡ����½ǡ����½Ǻ����ģ�

- **���л���ʾ**����ҿ���ʹ�ü򵥵�����򿪻�ر� HUD

- **MySQL ����**���洢���ƫ�����ú��Զ�������

- **���ػ�֧��**�����ڷ�����κ�����

- **���ͳ��**����ʾ�ӳ١�KDA��Ѫ�����������Ϣ

- **����Ա����**������������Ա���������������ʾ����

- **�Զ�������֧��**����ʾ���֣�ͨ�� Store API ���ɣ�����Ϸʱ�䡢�ϴε�¼���ڵ�

> [!WARNING]
> ��Ϊ�Ƕ��򿪷�������ֻ��ʹ�� schwarper/cs2-store �Ĳ��ϵͳ����ʾ��Ӧ����һ��֣�
> �����û����ʹ�øò������HUD�����ļ���Ĭ�Ϲرջ�����ʾ��
> ���⣬�����ϴ�ǩ��ʱ�����ʾ�����Ƕ������������Լ�������ǩ��ϵͳ�����Ƶģ�
> ������޸���Щ�Զ�������Ϊ������Ҫ��ʾ�Ķ�����������鿴�Զ������ݽ��ܣ�

> [!NOTE]
> # ��Ҫ������
> ��ҪHud�ķֱ������䣬
> ��ΪHud��ʾ�߼��Ƚϵ��꣬��û��һ��ͷ���������ֱ������䣬ֻ�����ڲ�д����Ԥ�����������ʵ�λ�ã�
> ������κ��˿��Ը�������һ���ֱ���֧�֣�ʹ�ò�ͬ�����µķֱ��ʿ�������ʹ����ЩԤ�裬����ֿ��ύPull request���ǳ���л�� 

## Ҫ��

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [GameHUD API](https://github.com/darkerz7/CS2-GameHUD)
- [Store API](https://github.com/schwarper/cs2-store) (��ѡ��������ʾ��ҵĻ���)
- MySQL ����������ѡ�����ڴ洢���ƫ�ã�

## ��װ

1. �������°汾
2. ��ѹ���ݵ����� CS2 �������� `game/csgo/addons/counterstrikesharp/plugins` Ŀ¼
3. �� `addons/counterstrikesharp/configs/plugins/InGameHUD/InGameHUD.json` �����ò������
4. ��������������ز��

## ����

����������ļ���`InGameHUD.json`�������������ã�

```json
{
  "version": 1,                      // �����޸Ĵ�ֵ
  "font_size": 50,                   // �����С
  "font_name": "Arial Bold",         // ��������
  "scale": 0.1,                      // HUD�������ű���
  "background_opacity": 0.6,         // ����͸����(0-1)
  "background_scale": 0.3,           // ����������ݵĴ�С
  "show_kda": true,                  // ��ʾ��ɱ/����/����
  "show_health": true,               // ��ʾ�������ֵ
  "show_team": true,                 // ��ʾ������Ϣ
  "show_time": true,                 // ��ʾ��ǰʱ��
  "show_ping": true,                 // ��ʾ����ӳ�
  "show_score": true,                // ��ʾ��ҵ÷�
  "show_announcement_title": true,   // ��ʾ�������
  "show_announcement": true,         // ��ʾ��������
  "text_color": "Orange",            // HUD������ɫ
  "mysql_connection": {              // MySQL���ݿ�����
    "host": "",                      // ���ݿ���������IP
    "port": 3306,                    // ���ݿ�˿�
    "database": "",                  // ���ݿ�����
    "username": "",                  // ���ݿ��û���
    "password": ""                   // ���ݿ�����
  },
  "custom_data": {                   // �Զ���������ʾ����
    "credits": {                     // �̵������ʾ���������schwarper/cs2-store���̵�ϵͳ��Ĭ�Ϲرգ�
      "enabled": true                // ����/���õ�����ʾ
    },
    "playtime": {                    // �����Ϸʱ����ʾ�����ƣ������û�и�ϵͳ�뿴�Զ������ݽ�����������
      "enabled": true,               // ����/������Ϸʱ����ʾ
      "table_name": "time_table",    // ��Ϸʱ�����ݱ���
      "column_name": "time"          // ��Ϸʱ���ֶ���
    },
    "signin": {                      // �ϴ�ǩ����ʾ�����ƣ������û�и�ϵͳ�뿴�Զ������ݽ�����������
      "enabled": true,               // ����/����ǩ����ʾ
      "table_name": "signin_table",  // ǩ����¼���ݱ���
      "column_name": "signin_time"   // ǩ��ʱ���ֶ���
    }
  }
}
```

## �Զ�������ģ��

### ����һ�������ģ��ϵͳ�����ڻ�ȡָ�����ݿ�����ض�����������ʾ��Ӧ����Ϣ����������ϸ����
#### Ŀǰģ�鿪�������ޣ���������������Ҫ�Ĺ��ܣ�Ȼ���ύPull requests�������ǿ�¡�����Լ�ʹ��

| ��������              | ����                                                                          |
|-----------------------|-------------------------------------------------------------------------------|
| `credits`             | ������ʾģ�飬����������schwarper/cs2-store���̵�ϵͳ����ʾ����̵������ӵ�еĻ��� |
| `playtime`            | ��Ϸʱ��ģ�飬����������k4system��������ʾk4ϵͳ�м�¼�������Ϸʱ��               |
| `signin`              | ���ǩ��ʱ��ģ�飬���������������Լ���ǩ��ϵͳ��������ʾ���һ�ε�ǩ��ʱ��           |
| `enabled`             | �Ƿ�����ʾ��`true`������ʾ��`false`�رգ���ע�⣬ʹ����Щ����һ��Ҫ���úñ������Ӻ����ݿ⣬������Ч��|
| `table_name`          | Ҫ��ȡ��Ŀ�������                                                              |
| `column_name`         | Ҫ��ȡ��Ŀ�������ƣ��ֶΣ�                                                       |

### �������ģ���ʹ�÷���
- ���ģ������߼��ǣ�ͨ����ȡ��ǰ��ҵ�`steamid`��Ȼ��ƥ�������õı����ƣ������ƣ���ȡ��Ӧ��������ű�ָ�������е����ݣ�Ȼ��ͨ��lang�ļ��Զ����������ʾ������ݣ�Ŀǰֻ�ṩ�����ֲ������ͣ�ʱ�������

- Ŀǰ���ṩ�޸�`playtime`��`signin`����ģ�飬�Ҵ������ƣ���Ϊ�Ƕ��򿪷�

- ��`playtime`�У�ƥ��ı��м�¼��`steamid`�ֶ�����Ϊ`steam_id`�������ݱ���������Ϊ��λ�����㷽�����Զ������`nСʱn����`Ȼ���ӡ��HUD��

- ��`signin`�У�ƥ��ı��м�¼��`steamid`�ֶ�����Ϊ`steamid64`�������ݱ����Ǳ�׼���ڸ�ʽ�����㷽����������ڼ����ȡ�����ݺͲ�ѯ��ʱ�����ڲ�࣬Ȼ�������day�����Ĳ�࣬����ӡ��ʾ`n��ǰ`��`����`����Ϣ

- �޸���������������ȷ��ƥ����������֮�����޸�λ��`...\addons\counterstrikesharp\plugins\CS2-InGameHUD\lang`�¶�Ӧ�������ļ�����`zh-Hans`Ϊ����

```json
{
  "hud.greeting": "��ã���{0}��",
  "hud.separator": "===================",
  "hud.current_time": "��ǰʱ��: {0}",
  "hud.ping": "�ӳ�: {0} ms",
  "hud.kda": "ս��: {0}/{1}/{2}",
  "hud.health": "����ֵ: {0}",
  "hud.team": "��Ӫ: {0}",
  "hud.score": "�÷�: {0}",
  "hud.credits": "����: {0}",
  "hud.last_signin": "�ϴ�ǩ��: {0}",         // �޸�"�ϴ�ǩ��"Ϊ����Ҫ��ʾ�ı���������������ݱ��л�ȡ�����ݣ������޸�{0}!
  "hud.never_signed": "��δǩ���������쳣",
  "hud.today": "����",
  "hud.days_ago": "{0}��ǰ",
  "hud.playtime": "����ʱ��: {0}Сʱ{1}����",  // �޸�"����ʱ��"Ϊ����Ҫ��ʾ�ı���������������ݱ��л�ȡ�����ݣ������޸�{0}{1}!
  "hud.separator_bottom": "===================",
  "hud.hint_toggle": "�Զ�����Ϣ�������д��!hud�������",
  "hud.hint_help": "�Զ�����Ϣ�������д��!help�鿴����",
  "hud.hint_store": "�Զ�����Ϣ�������д��!store���̵�",
  "hud.hint_website": "�Զ�����Ϣ��Ԥ������д��վ�������дQQȺ��",
  "hud.separator_final": "===================",
  "hud.announcement_title": "��������⡿",
  "hud.announcement_content": "��������1\n��������2\n��������3\n�Դ�����",  // \n�ǻ��з�
  "hud.team_t": "T",
  "hud.team_ct": "CT",
  "hud.team_spec": "�۲�",
  "hud.enabled": "{White}HUD{Lime}������{White}��",
  "hud.disabled": "{White}HUD{Lime}�ѽ���{White}��",
  "hud.invalid_state": "{Red}��ǰ״̬�޷�����HUD��������۲�״̬����",
  "hud.position_usage": "{White}�÷�: {Lime}!hudpos {White}<{Lime}1-5{White}>",
  "hud.position_help": "{Lime}1{White}:����  {Lime}2{White}:����  {Lime}3{White}:����  {Lime}4{White}:����  {Lime}5{White}:����",
  "hud.position_changed": "{White}HUDλ��{Lime}�Ѹ���{White}��",
  "hud.position_invalid": "{White}��Ч��λ��! ��ʹ��{Lime}1-5{White}��"
}
```
- �޸�"�ϴ�ǩ��"Ϊ����Ҫ��ʾ�ı��������������ݱ��л�ȡ�����ݣ������޸�`{0}`!

- �޸�""Ϊ����Ҫ��ʾ�ı��������������ݱ��л�ȡ�����ݣ������޸�`{0}` `{1}`!

- ������Զ���������������п�������޸ģ������ӳ٣�ս������Ӫ�����ݣ����������޸ĺ�������֣���Ϊ������ʾ�Ĳ������ݣ�����֧��CounterStrikeSharpԭ������ɫ��ʾ��


![image](https://github.com/user-attachments/assets/7471300a-d5a1-4690-81c4-25fe88ac34cd)

#### ʾ��ͼƬ���� [oqyh/cs2-Kill-Sound-GoldKingZ](https://github.com/oqyh/cs2-Kill-Sound-GoldKingZ?tab=readme-ov-file) �Ĳֿ⣨��Ϊ�Ƚ�����

> [!WARNING]
> ��Ҫ�ڰ����������ݼ����ϵ���������ǰ�����ɫ��ǣ���Ϊ��Щ����ʾ��hud�ϵ����ݣ�������Ч��
> HUD�����ֽ�֧��ȫ��������������ļ��е������ã�

## ���ݿ�����

�������ʹ�� MySQL �洢���ƫ�ã�

1. ����һ�������ݿ��ʹ���������ݿ�
2. ���������ļ��е� MySQL ����
3. ������Զ���������ı�

## ����

- `!hud` - �л� HUD �ɼ���
- `!hudpos <1-5>` - ���� HUD λ��
  - 1�����Ͻ�
  - 2�����Ͻ�
  - 3�����½�
  - 4�����½�
  - 5������

## ����

������н��顢���󱨸��Ľ�����ӭ�ύ Issue �� Pull Request��

## ���֤

<a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;">
    <img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License">
</a>
