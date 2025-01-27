namespace Tetris;

class Program
{
    public static List<Option> options;

    static void Main(string[] args)
    {
        
        // Tetris tetris = new Tetris();

        options = new List<Option>
        {
            new Option("Thing", () => WriteTemporaryMessage("Hi")),
            new Option("Another Thing", () => WriteTemporaryMessage("How Are You")),
            new Option("Yet Another Thing", () => WriteTemporaryMessage("Today")),
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
    
    public static void WriteTemporaryMessage(string message)
    {
        // Console.Clear();
        Console.Write(message);
        WriteMenu(options, options.First());
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
}