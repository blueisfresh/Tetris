using System.Security.Claims;
using System;

namespace Tetris;
class Program
{
    // Main Game Logic
    // Tetris tetris = new Tetris();
    
    // Access the singleton instance
    private static readonly IpManager ipManager = IpManager.Instance;

    public static List<Option> options = new List<Option>
    {
        new Option("Own Ip Adress", () =>
        {
            Console.Clear();
            Console.WriteLine(ipManager.GetLocalNetworkIpAddress());
            ReturnToMenu();
        }),
        new Option("Add New IP Address", () =>
        {
            Console.Clear();
            Console.WriteLine("Please enter a new IP address.");

            string input = Console.ReadLine();

            if (ipManager.IsValidIpAddress(input))
            {
                ipManager.IpAdresses.Add(input);
                Console.WriteLine("New IP Address added succesfully");
            }
            else
            {
                Console.WriteLine("Invalid IP address. Press any key to try again...");
            }

            ReturnToMenu();
        }),
        new Option("See All IP Addresses", () =>
        {
            if (ipManager.IpAdresses.Count == 0)
            {
                Console.WriteLine("No IP addresses saved.");
                ReturnToMenu();
                return;
            }

            NavigateList(
                ipManager.IpAdresses,
                ip =>
                {
                    Console.Clear();
                    Console.WriteLine($"Selected IP Address: {ip}");
                    ReturnToMenu();
                },
                new Dictionary<ConsoleKey, Action<int>> // Custom key behavior
                {
                    {
                        ConsoleKey.X, index =>
                        {
                            Console.Clear();
                            Console.WriteLine($"Are you sure you want to delete {ipManager.IpAdresses[index]}? (y/n)");
                            var confirmation = Console.ReadKey(true).Key;
                            if (confirmation == ConsoleKey.Y)
                            {
                                Console.WriteLine($"{ipManager.IpAdresses[index]} deleted.");
                                ipManager.IpAdresses.RemoveAt(index);

                                if (ipManager.IpAdresses.Count == 0)
                                {
                                    Console.WriteLine("No more IP addresses to display.");
                                    ReturnToMenu();
                                    return;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Deletion cancelled.");
                            }

                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                        }
                    }
                });
        }),
        new Option("Test Connection", () =>
        {
            Console.Clear();
            Console.WriteLine("Current Conectioln Status:");

            foreach (string ipAddress in ipManager.IpAdresses)
            {
                ipManager.CanConnectTo(ipAddress);
            }

            ReturnToMenu();
        }),
        new Option("Exit", () => Environment.Exit(0)),
    };

    static void Main(string[] args)
    {
        while (true)
        {
            NavigateList(options, option => { option.Selected.Invoke(); });
        }

        // Starting of Tetris logic in Tetris class
        //tetris.Main(); 
    }

    public static void DisplaySelection<T>(List<T> options, T selectedOption)
    {
        Console.Clear();

        foreach (var option in options)
        {
            if (EqualityComparer<T>.Default.Equals(option, selectedOption))
            {
                Console.Write(">");
            }
            else
            {
                Console.Write(" ");
            }

            Console.WriteLine(option.ToString());
        }
    }

    private static void ReturnToMenu()
    {
        Console.WriteLine("\nPress any key to return to the menu...");
        Console.ReadKey();
        Console.Clear();
    }

    public static void NavigateList<T>(List<T> options, Action<T> onSelect,
        Dictionary<ConsoleKey, Action<int>> customKeyActions = null)
    {
        // Return empty items List
        if (options == null || options.Count == 0)
        {
            Console.WriteLine("Nothing to display");
            ReturnToMenu();
            return;
        }

        // Variables
        int index = 0;
        ConsoleKey key;

        do
        {
            DisplaySelection(options, options[index]);

            key = Console.ReadKey(true).Key;
            
            // Custom Key action 
            
            if (customKeyActions != null && customKeyActions.ContainsKey(key))
            {
                // Execute custom action (like delete) and pass the current index
                customKeyActions[key].Invoke(index);
                continue; // Skip other actions after a custom action is executed
            }

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    index = (index > 0) ? index - 1 : 0;
                    break;
                case ConsoleKey.DownArrow:
                    index = (index < options.Count - 1) ? index + 1 : index;
                    break;
                case ConsoleKey.Enter:
                    onSelect(options[index]); // Execute action on the selected item
                    return; // Exit navigation after selection
                case ConsoleKey.Escape:
                    break;
                default:
                    Console.WriteLine(
                        "Invalid key. Please use Up, Down, Enter, or Escape."); // Handle unrecognized keys
                    break;
            }
        } while (true);
    }
}