#if !NO_MOD_SUPPORT
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization; // Требуется ссылка на System.Web.Extensions

namespace InfiniTD_2.Framework
{
    public class ModManifest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string AssemblyName { get; set; }
    }

    public static class ModManager
    {
        private static readonly Dictionary<string, string> ModDirectories = new Dictionary<string, string>();
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static List<ModManifest> loadedMods = new List<ModManifest>();

        public static void InitAndLoadMods(string modsRootPath)
        {
            if (!Directory.Exists(modsRootPath)) Directory.CreateDirectory(modsRootPath);

            // ВАЖНО: Перехватываем поиск зависимостей (чтобы подгружать Harmony и др. из папки мода)
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            // Сканируем папки модов
            string[] modFolders = Directory.GetDirectories(modsRootPath);

            foreach (string modFolder in modFolders)
            {
                string manifestPath = Path.Combine(modFolder, "mod.manifest");
                if (!File.Exists(manifestPath)) continue;

                try
                {
                    // Читаем манифест
                    string json = File.ReadAllText(manifestPath);
                    ModManifest manifest = Serializer.Deserialize<ModManifest>(json);

                    string mainDllPath = Path.Combine(modFolder, manifest.AssemblyName);
                    if (!File.Exists(mainDllPath))
                    {
                        Console.WriteLine($"[Error] Исполняемый файл {manifest.AssemblyName} не найден для мода {manifest.Name}");
                        continue;
                    }

                    // Запоминаем имя сборки и путь к её папке, чтобы резолвить зависимости
                    string assemblyNameWithoutExt = Path.GetFileNameWithoutExtension(manifest.AssemblyName);
                    if (!ModDirectories.ContainsKey(assemblyNameWithoutExt))
                    {
                        ModDirectories.Add(assemblyNameWithoutExt, modFolder);
                    }

                    // Загружаем основную DLL мода
                    Assembly asm = Assembly.LoadFrom(mainDllPath);

                    // Ищем точку входа Init()
                    bool initFound = false;
                    foreach (Type type in asm.GetTypes())
                    {
                        MethodInfo initMethod = type.GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
                        if (initMethod != null)
                        {
                            initMethod.Invoke(null, null);
                            initFound = true;
                            Console.WriteLine($"[Mods] Мод '{manifest.Name}' v{manifest.Version} успешно загружен.");
                            
                            loadedMods.Add(manifest);
                            Console.WriteLine(loadedMods.Count);
                            break;
                        }
                    }

                    if (!initFound)
                    {
                        Console.WriteLine($"[Warning] Мод '{manifest.Name}' загружен, но static void Init() не найден.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Ошибка при загрузке мода из {modFolder}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Этот метод вызывается CLR, когда подгружаемый код мода требует стороннюю DLL (например, Harmony),
        /// и система не может найти её в корне игры.
        /// </summary>
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Получаем короткое имя запрашиваемой библиотеки (например, "0Harmony")
            AssemblyName requestingName = new AssemblyName(args.Name);
            string shortName = requestingName.Name;

            // Определяем, какой мод запросил эту сборку
            Assembly requestingAssembly = args.RequestingAssembly;
            string modFolder = null;

            if (requestingAssembly != null)
            {
                ModDirectories.TryGetValue(requestingAssembly.GetName().Name, out modFolder);
            }

            // Если мы знаем, из какой папки этот мод, ищем зависимость там
            if (!string.IsNullOrEmpty(modFolder))
            {
                string dependencyPath = Path.Combine(modFolder, shortName + ".dll");
                if (File.Exists(dependencyPath))
                {
                    return Assembly.LoadFrom(dependencyPath);
                }
            }

            // Альтернативный поиск по всем папкам модов, если RequestingAssembly равен null
            foreach (string folder in ModDirectories.Values)
            {
                string dependencyPath = Path.Combine(folder, shortName + ".dll");
                if (File.Exists(dependencyPath))
                {
                    return Assembly.LoadFrom(dependencyPath);
                }
            }

            return null; // Позволяем стандартному механизму выдать ошибку, если библиотека нигде не найдена
        }
    }
}
#endif