# Introduction

I created this prototype application to practise using Autofac, Caliburn Micro and MVVM architecture. There are still some improvements to the application that I want to make in the future, which are:
1. Implement clean architecture using SOLID principles, in particular make WiFi and Profile class immutable. Separate UI logic from domain logic in separate modules, create separate module to Mock behaviour of WiFi library.
2. Implement IEquatable interface in WiFi and Profile class so that datagrids maintain the selected item more easily when datagrid is refreshed.
3. Validate XML profiles using Microsoft WiFi schema instead of parsing XML file.
4. Use adapter pattern logic of Autofac instead of implementing it self.
5. Use MahApps.Metro UI and HamburgerMenu.

# Easy Password Recovery WiFi

This password recovery tool is a Windows 10 desktop application which provides a way of matching WiFi passwords. The application is built using c# (WPF) and the architecture is based on Caliburn Micro (MVVM architecture) and Autofac (dependency injection/inversion of control). Application users can supply passwords in three different ways, manually by right clicking access point entry, using password dictionaries and regular expressions.

<img alt="screenshot" src="https://github.com/Mohamed1976/EasyPasswordRecoveryWiFi/blob/master/PreviewImages/BruteforceView.png" />

The WiFi Profile Manager view allows you to view your Wireless Network Profiles in Windows 10. Over time, if you connect to a lot of different wireless networks, this list can get pretty large and at times you may wish to remove unnecessary wireless profiles, especially if these networks were public WiFi at the local fast-food spot and you don’t want your laptop connecting automatically to it again.

WiFi Profile Manager lets you:
1. View all WiFi Profiles
2. Change list order
3. Export to XML
4. Import from XML
5. Remove WiFi Profiles
6. View plain text password of profiles

<img alt="screenshot" src="https://github.com/Mohamed1976/EasyPasswordRecoveryWiFi/blob/master/PreviewImages/ProfileManager.png" />

An interesting feature of this application is the Smart editor. This editor allows you to specify a regular expression which can then be used to generate passwords. The Smart editor is shown below.

<img alt="screenshot" src="https://github.com/Mohamed1976/EasyPasswordRecoveryWiFi/blob/master/PreviewImages/RegExView.png" />

The settings view allows you to configure the WiFi connection timeout, and the password casing of the words found in password dictionaries.

<img alt="screenshot" src="https://github.com/Mohamed1976/EasyPasswordRecoveryWiFi/blob/master/PreviewImages/SettingsView.png" />

The clickonce application can be launched from <a href="https://github.com/Mohamed1976/EasyPasswordRecoveryWiFi/blob/master/EasyPasswordRecoveryWiFi/publish/setup.exe" target="_blank">here</a>. After you have downloaded the setup.exe to your local Windows 10 computer, run the setup.exe and it will guide you through the installation process. Alternatively you can clone the project and build it yourself.  
