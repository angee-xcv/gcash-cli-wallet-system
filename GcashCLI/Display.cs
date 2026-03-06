using System;

namespace GcashCLI
{
    public static class Display
    {
        public static void Print(string text, bool newLine = true)
        {
            if (newLine) Console.WriteLine(text);
            else Console.Write(text);
        }

        public static void Success(string msg) => Print("  [OK] " + msg);
        public static void Error(string msg) => Print("  [ERROR] " + msg);
        public static void Info(string msg) => Print("  [INFO] " + msg);
        public static void Warning(string msg) => Print("  [!] " + msg);

        
        public static void PrintColor(string text, ConsoleColor color, bool newLine = true)
        {
            if (newLine) Console.WriteLine(text);
            else Console.Write(text);
        }

        public static void Line(char ch = '-', int length = 55)
        {
            Console.WriteLine(new string(ch, length));
        }

        public static void Header(string title)
        {
            Console.WriteLine();
            Line('=');
            Console.WriteLine("  " + title);
            Line('=');
        }

        public static void Splash()
        {
            Console.Clear();
            
            Console.WriteLine("                                        ");
            Console.WriteLine("            GCASH CLI WALLET            ");
            Console.WriteLine("                                        ");
           
        }

        public static void MainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("  --- MAIN MENU ---");
            Console.WriteLine("  [1] Login");
            Console.WriteLine("  [2] Register New Account");
            Console.WriteLine("  [0] Exit");
        }

        public static void DashboardMenu(string name, decimal balance)
        {
            Console.WriteLine();
            Console.WriteLine("  Welcome, " + name + "!");
            Console.WriteLine("  Balance: P" + balance.ToString("N2"));
            Console.WriteLine();
            Console.WriteLine("  --- SERVICES ---");
            Console.WriteLine("  [1] Cash In");
            Console.WriteLine("  [2] Cash Out");
            Console.WriteLine("  [3] Deposit (from Bank)");
            Console.WriteLine("  [4] Transfer to Bank");
            Console.WriteLine("  [5] Transaction History");
            Console.WriteLine("  [6] My Account Details");
            Console.WriteLine("  [0] Logout");
        }

        public static void PrintReceipt(Transaction txn, string senderName = null, string recipientName = null)
        {
            Console.WriteLine();
            Console.WriteLine("  ==========================================");
            Console.WriteLine("           TRANSACTION RECEIPT");
            Console.WriteLine("  ==========================================");
            Console.WriteLine("  Type    : " + txn.Type);
            Console.WriteLine("  Amount  : P" + txn.Amount.ToString("N2"));
            Console.WriteLine("  Date    : " + txn.Timestamp.ToString("MM/dd/yyyy hh:mm tt"));
            Console.WriteLine("  Ref. No.: " + txn.ReferenceNumber);

            if (senderName != null)
                Console.WriteLine("  From    : " + senderName);
            if (recipientName != null)
                Console.WriteLine("  To      : " + recipientName);
            Console.WriteLine("  Balance : P" + txn.BalanceAfter.ToString("N2"));
            Console.WriteLine("  Status  : SUCCESS");
            Console.WriteLine("  ==========================================");
        }

        public static string Prompt(string label)
        {
            Console.Write("  " + label + ": ");
            return Console.ReadLine()?.Trim();
        }

        public static string ReadPin(string label = "Enter PIN")
        {
            Console.Write("  " + label + ": ");
            string pin = "";
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && pin.Length > 0)
                {
                    pin = pin.Substring(0, pin.Length - 1);
                    Console.Write("\b \b");
                }
                else if (char.IsDigit(key.KeyChar))
                {
                    pin += key.KeyChar;
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return pin;
        }

        public static void PressAnyKey()
        {
            Console.WriteLine();
            Console.WriteLine("  Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}