using System.Security.Claims;
using System;

namespace Tetris;
class Program
{
    public static List<Option> options;

    static void Main(string[] args)
    {
        
        // Tetris tetris = new Tetris();
        
        options = new List<Option>
        {
            new Option("Own Ip Adress", () =>
            {
                Console.Clear();    
                Console.WriteLine(IpManager.GetLocalNetworkIpAddress());
                
                // Get back to the menu
                ReturnToMenu();
            }),
            new Option("Add New IP Address", () =>
            {
                Console.Clear();
                Console.WriteLine("Please enter a new IP address.");
                
                string input = Console.ReadLine();

                if (IpManager.IsValidIpAddress(input))
                {
                    IpManager.IpAdresses.Add(input);
                    Console.WriteLine("New IP Address added succesfully");
                }else
                {
                    Console.WriteLine("Invalid IP address. Press any key to try again...");
                }
                
                // Get back to the menu
                ReturnToMenu();
            }),
            new Option("See all ip Adresses", () =>
            {
                Console.Clear();
                Console.WriteLine("All Currently saved Ip Addresses:");
                
                foreach (string ipAddress in IpManager.IpAdresses)
                {
                    Console.WriteLine(ipAddress);
                }
                
                // Get back to the menu
                ReturnToMenu();
            }),
            new Option("Test Connection", () =>
            {
                Console.Clear();
                Console.WriteLine("Current Conectioln Status:");
                
                foreach (string ipAddress in IpManager.IpAdresses)
                {
                    Console.WriteLine($"{ipAddress} - {(IpManager.CanConnectTo(ipAddress) ? "ok" : "not ok!")}");
                }
                
                // Get back to the menu
                ReturnToMenu();
            }),
            new Option("Exit", () => Environment.Exit(0)),
        };

        // Index of the selected item (default = the first one)
        int index = 0;
        
        // Write the menu out
        WriteMenu(options, options[index]);

        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (index + 1 < options.Count)
                {
                    index++;
                    WriteMenu(options, options[index]);
                }
            }

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (index - 1 < options.Count)
                {
                    index--;
                    WriteMenu(options, options[index]);
                }
            }
            
            // Handle different action for the option
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                options[index].Selected.Invoke();
                index = 0;
            }
        }while(keyInfo.Key != ConsoleKey.Escape);
        
        Console.ReadKey();

        // Starting of Tetris logic in Tetris class
        //tetris.Main(); 
    }

    public static void WriteMenu(List<Option> options, Option selectedOption)
    {
        Console.Clear();

        foreach (Option option in options)
        {
            if (option == selectedOption)
            {
                Console.Write(">");
            }
            else
            {
                Console.Write(" ");
            }
            Console.WriteLine(option.Name);
        }
    }
    
    private static void ReturnToMenu()
    {
        Console.WriteLine("\nPress any key to return to the menu...");
        Console.ReadKey();
        Console.Clear();
        WriteMenu(options, options.First());
    }
}