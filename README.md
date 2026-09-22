# Сиквел моего старого проекта InfiniTD.


### Для работы игры установите .NET Framework 4.8.1
- Ссылка на скачивание: [https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/thank-you/net48-web-installer](https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/thank-you/net481-web-installer)

### Вы можете создавать моды для этой игры.
- Создайте в папке mods новую папку для своего мода
- Добавьте mod.manifest чтобы игра определила мод
- Пример .manifest
  ```
  {
  "Id": "com.modder.infinitd2.musicmod",
  "Name": "Music Mod",
  "Version": "1.0.0",
  "Author": "Kernelit",
  "AssemblyName": "KrutolMod.dll",
  }

- Далее создайте в Visual Studio 2026 библиотеку классов на .NET Framework 4.8
- Создайте ссылку на InfiniTD2.exe с поддержкой модов
### Теперь вы можете создать мод меняющий значения
- Просто через ссылку на InfiniTD2 меняйте значения в функции Init() класса вашего мода
- Если вы хотите менять функции игры то добавьте ссылку на Harmony и используйте его для переписывания нужной функции
