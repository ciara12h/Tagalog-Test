using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TagalogTest
{
    class Program
    {
        public static Dictionary<string, List<TagalogListItem>> OurList { get; set; } = new Dictionary<string, List<TagalogListItem>>();
        public static string CurrentLoadedList { get; set; }
        public static string FileName { get; set; }
        static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            FileName = args.FirstOrDefault() ?? "TagalogTest.json";
            if (File.Exists(Path.Combine(path,FileName)))
            {
                OurList =
                    JsonSerializer.Deserialize<Dictionary<string, List<TagalogListItem>>>(File.ReadAllText(Path.Combine(path,FileName)));
            }

            while (true)
            {
                var menuOption = PrintMenu<MenuOptions>();
                Console.Clear();
                Console.WriteLine($"You chose {menuOption}\n");
                switch (menuOption)
                {
                    case MenuOptions.AddToList:
                        AddToList();
                        break;
                    case MenuOptions.SaveList:
                        SaveList();
                        break;
                    case MenuOptions.LoadList:
                        LoadList();
                        break;
                    case MenuOptions.DisplayList:
                        DisplayList();
                        break;
                    case MenuOptions.TakeTest:
                        TakeTest();
                        break;
                    case MenuOptions.Exit:
                        return;
                }
            }
        }

        private static void TakeTest()
        {
            if(string.IsNullOrEmpty(CurrentLoadedList))
                LoadList();

            var menuOption = PrintMenu<DifficultyLevel>();
            var questions = OurList[CurrentLoadedList].OrderBy(x => Guid.NewGuid()).ToList();

            while (questions.Any())
            {
                Console.WriteLine(menuOption == DifficultyLevel.Easy 
                    ? $"What is \"{questions.First().Tagalog}\" in English?"
                    : $"What is {questions.First().English} in Tagalog?");
                var answer = Console.ReadLine();
                if (answer?.Equals((menuOption == DifficultyLevel.Easy
                        ? questions.First().English
                        : questions.First().Tagalog), StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    Console.Clear();
                    Console.WriteLine("Great job, you are rocking this.");
                    questions.RemoveAt(0);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"Oops, {questions.First().Tagalog}: {questions.First().English}");
                    Console.WriteLine("That's going to cost ya big time.\n");
                    questions.Add(questions.First());
                    questions = questions.OrderBy(x => Guid.NewGuid()).ToList();
                }
            }
            Console.WriteLine("You made it through the whole list, good job.");
        }

        private static void DisplayList()
        {
            if(string.IsNullOrEmpty(CurrentLoadedList))
                LoadList();

            Console.Clear();
            Console.WriteLine($"{CurrentLoadedList}:");
            foreach (var item in OurList[CurrentLoadedList])
            {
                Console.WriteLine($"{item.Tagalog}: {item.English}" );
            }
            Console.WriteLine();
        }

        private static void LoadList()
        {
            var options = OurList.Keys.ToList();
            for (var index = 0; index < options.Count; index++)
            {
                var listName = options[index];
                Console.WriteLine($"{index} - {listName}");
            }

            while (true)
            {
                Console.WriteLine("Enter the number or name of the list to load, or a new name to add");
                CurrentLoadedList = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(CurrentLoadedList)) break;
            }

            if (int.TryParse(CurrentLoadedList, out int selected) && selected < options.Count && selected >= 0)
            {
                CurrentLoadedList = options[selected];
            }
            else if (!options.Select(x=> x.ToLower()).Contains(CurrentLoadedList.ToLower() ))
            {
                OurList.Add(CurrentLoadedList, new List<TagalogListItem>());
            }
            else
            {
                CurrentLoadedList =
                    options.Find(x => x.Equals(CurrentLoadedList, StringComparison.CurrentCultureIgnoreCase));
            }
            Console.Clear();
        }

        private static void SaveList()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var options = new JsonSerializerOptions()
                {Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true};
            File.WriteAllText(Path.Combine(path,FileName),
                JsonSerializer.Serialize(OurList, options));
            Console.Clear();
            Console.WriteLine("List saved\n");
            PrintMenu<MenuOptions>();
        }

        private static void AddToList()
        {
            if(string.IsNullOrEmpty(CurrentLoadedList))
                LoadList();

            int itemsAdded = 0;

            while (true)
            {
                Console.WriteLine("Enter Tagalog or enter to stop adding:");
                var tagalog = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(tagalog))
                    break;

                Console.WriteLine("Enter English:");
                var english = Console.ReadLine();

                itemsAdded++;
                OurList[CurrentLoadedList].Add(new TagalogListItem(tagalog, english));
            }

            Console.WriteLine($"You added {itemsAdded} to the list.");
        }

        public static T PrintMenu<T>() where T : struct, IConvertible, IComparable, IFormattable
        {
            foreach (var menuOption in Enum.GetValues(typeof(T)).OfType<T>())
            {
                Console.WriteLine($"{(int)Enum.Parse(typeof(T), menuOption.ToString())} - {menuOption}");
            }
            Console.WriteLine("Choose an option from above.");

            while (true)
            {
                if(Enum.TryParse<T>(Console.ReadLine(), true, out var menuChoice) 
                   && Enum.IsDefined(typeof(T), menuChoice))
                    return (T)menuChoice;

                Console.WriteLine("That is not a valid option");
            }
        }
    }

    enum MenuOptions
    {
        AddToList,
        SaveList,
        LoadList,
        DisplayList,
        TakeTest,
        Exit
    }

    enum DifficultyLevel
    {
        Easy,
        Hard
    }

    public class TagalogListItem
    {
        public string Tagalog { get; set; }
        public string English { get; set; }

        public TagalogListItem(){}
        public TagalogListItem(string tagalog, string english)
        {
            Tagalog = tagalog;
            English = english;
        }
    }
}