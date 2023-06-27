# PasswordChangeAssistant
[![Version](https://img.shields.io/github/release/rookiestyle/passwordchangeassistant)](https://github.com/rookiestyle/passwordchangeassistant/releases/latest)
[![Releasedate](https://img.shields.io/github/release-date/rookiestyle/passwordchangeassistant)](https://github.com/rookiestyle/passwordchangeassistant/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/rookiestyle/passwordchangeassistant/total?color=%2300cc00)](https://github.com/rookiestyle/passwordchangeassistant/releases/latest/download/PasswordChangeAssistant.plgx)\
[![License: GPL v3](https://img.shields.io/github/license/rookiestyle/passwordchangeassistant)](https://www.gnu.org/licenses/gpl-3.0)

PasswordChangeAssistant is a KeePass plugin that offers multiple assistance functions all related to changing passwords:
- Simplify changing passwords 
- Highlight the most recently used password profile per entry
- Synchronize password profiles as part your database

# Table of Contents
- [Configuration](#configuration)
- [Usage](#usage)
- [Translations](#translations)
- [Download & updates](#download--updates)
- [Requirements](#requirements)

# Configuration
PasswordChangeAssistant is designed to *simply work* and does not require any kind of configuration for changing passwords.
  
Only for synchronizing password profiles, PasswordChangeAssistant integrates into KeePass' options form.  

# Usage
A more detailed description of the different functions can be found in the [Wiki](https://github.com/rookiestyle/passwordchangeassistant/wiki)
## Changing passwords
PasswordChangeAssistant integrates into the entry form and offers a standalone form in addition.

Within the enry form, PasswordChangeAssistant displays a different icon in front of the most recently used password profile.  
![Last used profile](images/PasswordChangeAssistant%20-%20last%20profile.png)

In addition, a new button to auto-type/copy both old and new passwords is displayed
![Entry form integration](images/PasswordChangeAssistant%20-%20entry%20form.png)

The stand alone form can be accessed using the context menu und supports [PEDCalc](https://github.com/rookiestyle/pedcalc) as well.
It offers additional functionality like 
- 2nd URL fields that can hold the direct link to the *Change password* site of the selected entry
- Select - and maintain - Auto-type sequences for changing the password
![Standalone form](images/PasswordChangeAssistant%20-%20standalone%20form.png)


## Synchronizing password profiles
Here you can move existing profiles in various directions
- Between two databases
- From an open database to the global KeePass configuration
- From the global KeePass configuration to an open database

Storing a password profile does **not** increase security. The password profile/pattern is not a secret, only the generated password is secret.  
This option is added for convenience reasons. A password profile stored within the database is part of synchronization.  
If you synchronize a database between two devices and have PasswordChangeAssistant installed on both of them, changing or adding a profile on device A will be synchronized to device B.

![Profile sync](images/PasswordChangeAssistant%20-%20Options.png)

## Password profile: At least 1 per set  
There are quite some websites that force you to follow password rules like this:  
* Minimum 8 characters
* Maximum 30 characters
* At least 1 upper case letter
* At least 1 lower case letter
* At least 1 special characters
* At least 1 digit  

While the length requirement is easy to achieve with KeePass' built-in password generator (just go for 30 characters), the *at least 1* is not achievable in an easy way.  
PasswordChangeAssistant now provides a custom password generator that allows creation of passwords with at least 1 character of every selected set.  
**IF** required by the website, feel free to use it.

![Password Generator](images/PasswordChangeAssistant%20-%20Password%20Generator.png)

# Translations
PasswordChangeAssistant is provided with English language built-in and allow usage of translation files.
These translation files need to be placed in a folder called *Translations* inside in your plugin folder.
If a text is missing in the translation file, it is backfilled with English text.
You're welcome to add additional translation files by creating a pull request as deacribed in the [wiki](https://github.com/Rookiestyle/PasswordChangeAssistant/wiki/Create-or-update-translations).

Naming convention for translation files: `<plugin name>.<language identifier>.language.xml`\
Example: `PasswordChangeAssistant.de.language.xml`
  
The language identifier in the filename must match the language identifier inside the KeePass language that you can select using *View -> Change language...*\
This identifier is shown there as well, if you have [EarlyUpdateCheck](https://github.com/rookiestyle/earlyupdatecheck) installed.

# Download & updates
Please follow these links to download the plugin file itself.
- [Download newest release](https://github.com/rookiestyle/passwordchangeassistant/releases/latest/download/PasswordChangeAssistant.plgx)
- [Download history](https://github.com/rookiestyle/passwordchangeassistant/releases)

If you're interested in any of the available translations in addition, please download them from the [Translations](Translations) folder.

In addition to the manual way of downloading the plugin, you can use [EarlyUpdateCheck](https://github.com/rookiestyle/earlyupdatecheck/) to update both the plugin and its translations automatically.  
See the [one click plugin update wiki](https://github.com/Rookiestyle/EarlyUpdateCheck/wiki/One-click-plugin-update) for more details.
# Requirements
* KeePass: 2.40
* .NET framework: 3.5
