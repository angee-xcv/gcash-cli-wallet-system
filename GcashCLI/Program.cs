
using System;
using System.Linq;

namespace GcashCLI
{
    class Program
    {
        static TransactionService service = new TransactionService();
        static Account currentAccount = null;

        static void Main(string[] args)
        {
            Display.Splash();
            Console.WriteLine("  Demo accounts: 09171234567 (PIN: 1234) | 09189876543 (PIN: 5678)");

            bool running = true;
            while (running)
            {
                Display.MainMenu();
                string choice = Display.Prompt("Choose");

                switch (choice)
                {
                    case "1": HandleLogin(); break;
                    case "2": HandleRegister(); break;
                    case "0":
                        Display.Success("Goodbye!");
                        running = false;
                        break;
                    default:
                        Display.Error("Invalid option. Try again.");
                        break;
                }
            }
        }


        static void HandleLogin()
        {
            Display.Header("LOGIN");
            string phone = Display.Prompt("Mobile Number (e.g. 09171234567)");

            while (phone.Length != 11 || !phone.StartsWith("09"))
            {
                Display.Error("Invalid phone number.");
                phone = Display.Prompt("Mobile Number (e.g. 09171234567)");
            }

            if (!service.AccountExists(phone))
            {
                Display.Error("No account found with that number.");
                Display.PressAnyKey();
                return;
            }

            int attempts = 0;
            int maxAttempts = 3;

            while (attempts < maxAttempts)
            {
                string pin = Display.ReadPin("4-digit PIN");
                var account = service.Login(phone, pin);

                if (account != null)
                {
                    currentAccount = account;
                    Display.Success($"Logged in as {account.FullName}");
                    Display.PressAnyKey();
                    Dashboard();
                    return;
                }

                attempts++;
                int remaining = maxAttempts - attempts;

                if (remaining > 0)
                    Display.Error($"Incorrect PIN. {remaining} attempt(s) remaining.");
            }

            // 3 failed attempts
            Display.Error("Too many incorrect attempts.");
            Console.WriteLine();
            Console.WriteLine("  [1] Reset PIN");
            Console.WriteLine("  [0] Go Back (Account Will be Locked)");
            string choice = Display.Prompt("Choose");

            if (choice == "1")
            {
                HandleResetPIN(phone);
            }
            else
            {
                Display.Error("Account locked due to too many failed attempts.");
                Display.PressAnyKey();
                Environment.Exit(0);
            }
        }

        static void HandleRegister()
        {
            Display.Header("CREATE ACCOUNT");

            string firstName = Display.Prompt("First Name");
            while (string.IsNullOrWhiteSpace(firstName) || !firstName.Trim().All(char.IsLetter))
            {
                if (string.IsNullOrWhiteSpace(firstName))
                    Display.Error("First name cannot be empty.");
                else
                    Display.Error("First name must contain letters only. No numbers or special characters.");
                firstName = Display.Prompt("First Name");
            }
            firstName = firstName.ToUpper();

            string middleName = Display.Prompt("Middle Name(Optional)");
            while (!middleName.Trim().All(char.IsLetter))
            {
                if (middleName.Trim().All(char.IsLetter))
                    Display.Error("Middle name must contain letters only. No numbers or special characters.");
                middleName = Display.Prompt("Middle Name");
            }
            middleName = middleName.ToUpper();


            string lastName = Display.Prompt("Last Name");
            while (string.IsNullOrWhiteSpace(lastName) || !lastName.Trim().All(char.IsLetter))
            {
                if (string.IsNullOrWhiteSpace(lastName))
                    Display.Error("Last name cannot be empty.");
                else
                    Display.Error("Last name must contain letters only. No numbers or special characters.");
                lastName = Display.Prompt("Last Name");
            }
            lastName = lastName.ToUpper();

            string email = Display.Prompt("Email Address");
            while (string.IsNullOrWhiteSpace(email) || !email.Contains("@gmail.com"))
            {
                Display.Error("Invalid email. Must contain @gmail.com");
                email = Display.Prompt("Email Address");
            }

            string birthday = Display.Prompt("Birthday (MM/DD/YYYY)");
            while (true)
            {
                if (birthday.Length != 10 || !DateTime.TryParse(birthday, out DateTime parsedDate))
                {
                    Display.Error("Invalid birthday format. Use MM/DD/YYYY.");
                }
                else if (parsedDate > DateTime.Today)
                {
                    Display.Error("Birthday cannot be a future date.");
                }
                else if (parsedDate.Year > DateTime.Today.Year)
                {
                    Display.Error($"Birthday year cannot be beyond {DateTime.Today.Year}.");
                }
                else if (DateTime.Today.Year - parsedDate.Year < 18)
                {
                    Display.Error("You must be at least 18 years old to register.");
                }
                else
                {
                    break;
                }

                birthday = Display.Prompt("Birthday (MM/DD/YYYY)");
            }

            string address = Display.Prompt("Address(Brgy., City, Province)");
            while (string.IsNullOrWhiteSpace(address))
            {
                if (string.IsNullOrWhiteSpace(address))
                    Display.Error("Address name cannot be empty.");
                else
                    Display.Error("Address must contain letters only. No numbers or special characters.");
                address = Display.Prompt("Last Name");
            }

            string phone = Display.Prompt("Mobile Number (11 digits)");
            while (phone.Length != 11 || !phone.StartsWith("09") || !IsAllDigits(phone))
            {
                Display.Error("Invalid phone number.");
                phone = Display.Prompt("Mobile Number (11 digits)");
            }

            if (service.AccountExists(phone))
            {
                Display.Error("An account with that number already exists.");
                Display.PressAnyKey();
                return;
            }

            string pin = Display.ReadPin("Set 4-digit PIN");
            while (pin.Length != 4)
            {
                Display.Error("PIN must be exactly 4 digits.");
                pin = Display.ReadPin("Set 4-digit PIN");
            }

            string pinCfm = Display.ReadPin("Confirm PIN");
            while (pin != pinCfm)
            {
                Display.Error("PINs do not match. Try again.");
                pinCfm = Display.ReadPin("Confirm PIN");
            }

            bool ok = service.RegisterAccount(firstName, middleName, lastName,
                                               email, birthday, address, phone, pin);
            if (ok)
                Display.Success($"Account created for {firstName} {middleName} {lastName}! You can now log in.");
            else
                Display.Error("Registration failed. Please try again.");

            Display.PressAnyKey();
        }

        static void HandleResetPIN(string phone)
        {
            Display.Header("RESET PIN");
            Display.Info("Please verify your identity.");

            string birthday = Display.Prompt("Enter your Birthday (MM/DD/YYYY)");

            string newPin = Display.ReadPin("Set new 4-digit PIN");
            while (newPin.Length != 4)
            {
                Display.Error("PIN must be exactly 4 digits.");
                newPin = Display.ReadPin("Set new 4-digit PIN");
            }

            string confirmPin = Display.ReadPin("Confirm new PIN");
            while (newPin != confirmPin)
            {
                Display.Error("PINs do not match. Try again.");
                confirmPin = Display.ReadPin("Confirm new PIN");
            }

            bool success = service.ResetPIN(phone, birthday, newPin);
            if (success)
                Display.Success("PIN reset successfully! You can now log in with your new PIN.");
            else
                Display.Error("Verification failed. Birthday did not match our records.");

            Display.PressAnyKey();
        }


        //  DASHBOARD

        static void Dashboard()
        {
            bool loggedIn = true;
            while (loggedIn)
            {
                Console.Clear();
                Display.Header("GCASH WALLET");
                Display.DashboardMenu(currentAccount.FullName, currentAccount.Balance);
                string choice = Display.Prompt("Choose");

                switch (choice)
                {
                    case "1": HandleCashIn(); break;
                    case "2": HandleCashOut(); break;
                    case "3": HandleDeposit(); break;
                    case "4": HandleTransfer(); break;
                    case "5": HandleHistory(); break;
                    case "6": HandleMyAccount(); break;
                    case "0":
                        Display.Success("Logged out successfully.");
                        currentAccount = null;
                        loggedIn = false;
                        break;
                    default:
                        Display.Error("Invalid option.");
                        Display.PressAnyKey();
                        break;
                }
            }
        }


        //  Pin

        static bool VerifyPIN()
        {
            int attempts = 0;
            while (attempts < 3)
            {
                string pin = Display.ReadPin("Enter PIN to authorize");
                if (pin == currentAccount.PIN)
                    return true;

                attempts++;
                int remaining = 3 - attempts;
                if (remaining > 0)
                    Display.Error($"Incorrect PIN. {remaining} attempt(s) remaining.");
            }
            Display.Error("Too many incorrect PIN attempts. Transaction cancelled.");
            return false;
        }

        static bool IsAllDigits(string value)
        {
            foreach (char c in value)
                if (!char.IsDigit(c))
                    return false;
            return true;
        }

        //  Trransaction

        static void HandleCashIn()
        {
            Display.Header("CASH IN");
            Display.Info($"Current Balance: P{currentAccount.Balance:N2}");

            string input = Display.Prompt("Amount to Cash In");

            decimal amount;
            while (!decimal.TryParse(input, out amount) || amount <= 0)
            {
                Display.Error("Invalid amount. Please enter a valid number.");
                input = Display.Prompt("Amount to Cash In");
            }

            bool success = false;

            while (!success)
            {
                try
                {
                    var txn = service.CashIn(currentAccount, amount);

                    Display.PrintReceipt(txn);
                    success = true;
                }
                catch (ArgumentException ex)
                {
                    Display.Error(ex.Message);

                    input = Display.Prompt("Amount to Cash In");
                    while (!decimal.TryParse(input, out amount) || amount <= 0)
                    {
                        Display.Error("Invalid amount.");
                        input = Display.Prompt("Amount to Cash In");
                    }
                }
            }

            Display.PressAnyKey();
        }

        static void HandleCashOut()
        {
            Display.Header("CASH OUT");
            Display.Info($"Current Balance: P{currentAccount.Balance:N2}");


            string recipient = Display.Prompt("Recipient GCash Number");


            if (string.IsNullOrWhiteSpace(recipient))
                recipient = currentAccount.PhoneNumber;


            while (recipient.Length != 11 || !recipient.StartsWith("09"))
            {
                Display.Error("Invalid number. Must start with 09 and be 11 digits.");
                recipient = Display.Prompt("Recipient GCash Number");
            }


            if (!service.AccountExists(recipient))
            {
                Display.Error($"No GCash account found for {recipient}.");
                Display.PressAnyKey();
                return;
            }

            string input = Display.Prompt("Amount to Cash Out");
            decimal amount;
            while (!decimal.TryParse(input, out amount) || amount <= 0)
            {
                Display.Error("Invalid amount. Please enter a valid number.");
                input = Display.Prompt("Amount to Cash Out");
            }

            Console.WriteLine();
            Display.Warning($"Cash out P{amount:N2} from {currentAccount.PhoneNumber} to {recipient}?");
            string confirm = Display.Prompt("Type YES to confirm or NO to cancel");
            while (confirm?.ToUpper() != "YES" && confirm?.ToUpper() != "NO")
            {
                Display.Error("Invalid input. Type YES to confirm or NO to cancel.");
                confirm = Display.Prompt("Type YES to confirm or NO to cancel");
            }

            if (confirm?.ToUpper() == "NO")
            {
                Display.Info("Transaction cancelled.");
                Display.PressAnyKey();
                return;
            }

            if (!VerifyPIN()) { Display.PressAnyKey(); return; }

            bool success = false;
            while (!success)
            {
                try
                {
                    var txn = service.CashOut(currentAccount, recipient, amount);
                    Display.PrintReceipt(txn, currentAccount.PhoneNumber, recipient);
                    success = true;
                }
                catch (InvalidOperationException ex)
                {
                    Display.Error(ex.Message);
                    Display.PressAnyKey();
                    return;
                }
                catch (ArgumentException ex)
                {
                    Display.Error(ex.Message);
                    input = Display.Prompt("Amount to Cash Out");
                    while (!decimal.TryParse(input, out amount) || amount <= 0)
                    {
                        Display.Error("Invalid amount. Please enter a valid number.");
                        input = Display.Prompt("Amount to Cash Out");
                    }
                }
            }

            Display.PressAnyKey();
        }

        static void HandleDeposit()
        {
            Display.Header("DEPOSIT FROM BANK");
            Display.Info("Supported banks: BDO, BPI, Metrobank, UnionBank, RCBC");

            string[] validBanks = { "BDO", "BPI", "METROBANK", "UNIONBANK", "RCBC" };

            string bank = Display.Prompt("Bank Name");
            while (Array.IndexOf(validBanks, bank.ToUpper()) == -1)
            {
                Display.Error("Invalid bank. Choose from: BDO, BPI, Metrobank, UnionBank, RCBC");
                bank = Display.Prompt("Bank Name");
            }

            string input = Display.Prompt("Amount to Deposit");
            decimal amount;
            while (!decimal.TryParse(input, out amount) || amount <= 0)
            {
                Display.Error("Invalid amount. Please enter a valid number.");
                input = Display.Prompt("Amount to Deposit");
            }

            bool success = false;
            while (!success)
            {
                try
                {
                    var txn = service.Deposit(currentAccount, amount, bank);
                    Display.PrintReceipt(txn);
                    success = true;
                }
                catch (Exception ex)
                {
                    Display.Error(ex.Message);
                    input = Display.Prompt("Amount to Deposit");
                    while (!decimal.TryParse(input, out amount) || amount <= 0)
                    {
                        Display.Error("Invalid amount. Please enter a valid number.");
                        input = Display.Prompt("Amount to Deposit");
                    }
                }
            }

            Display.PressAnyKey();
        }

        static void HandleTransfer()
        {
            Display.Header("TRANSFER TO BANK");
            Display.Info($"Current Balance: P{currentAccount.Balance:N2}");

            string[] validBanks = { "BDO", "BPI", "METROBANK", "UNIONBANK", "RCBC" };
            string bank = Display.Prompt("Bank Name (e.g. BDO, BPI)");
            while (string.IsNullOrWhiteSpace(bank) || Array.IndexOf(validBanks, bank.ToUpper()) == -1)
            {
                Display.Error("Invalid bank. Choose from: BDO, BPI, Metrobank, UnionBank, RCBC");
                bank = Display.Prompt("Bank Name");
            }

            string bankAcct = Display.Prompt("Bank Account Number");
            while (string.IsNullOrWhiteSpace(bankAcct) || bankAcct.Length < 10 || !IsAllDigits(bankAcct))
            {
                Display.Error("Invalid bank account number. Must be at least 10 digits and numbers only.");
                bankAcct = Display.Prompt("Bank Account Number");
            }

            string input = Display.Prompt("Amount to Transfer");
            decimal amount;
            while (!decimal.TryParse(input, out amount) || amount <= 0)
            {
                Display.Error("Invalid amount. Please enter a valid number.");
                input = Display.Prompt("Amount to Transfer");
            }

            Console.WriteLine();
            Display.Warning($"Transfer P{amount:N2} to {bank} Acct#{bankAcct}?");
            string confirm = Display.Prompt("Type YES to confirm or NO to cancel");

            while (confirm?.ToUpper() != "YES" && confirm?.ToUpper() != "NO")
            {
                Display.Error("Invalid input. Type YES to confirm or NO to cancel.");
                confirm = Display.Prompt("Type YES to confirm or NO to cancel");
            }

            if (confirm?.ToUpper() == "NO")
            {
                Display.Info("Transaction cancelled.");
                Display.PressAnyKey();
                return;
            }

            if (!VerifyPIN()) { Display.PressAnyKey(); return; }

            bool success = false;
            while (!success)
            {
                try
                {
                    var txn = service.Transfer(currentAccount, bank, bankAcct, amount);
                    Display.PrintReceipt(txn);
                    success = true;
                }
                catch (InvalidOperationException ex)
                {
                    Display.Error(ex.Message);
                    Display.PressAnyKey();
                    return;
                }
                catch (ArgumentException ex)
                {
                    Display.Error(ex.Message);
                    input = Display.Prompt("Amount to Transfer");
                    while (!decimal.TryParse(input, out amount) || amount <= 0)
                    {
                        Display.Error("Invalid amount. Please enter a valid number.");
                        input = Display.Prompt("Amount to Transfer");
                    }
                }
            }

            Display.PressAnyKey();
        }

        static void HandleHistory()
        {
            Display.Header("TRANSACTION HISTORY");

            var history = service.GetHistory(currentAccount.PhoneNumber);

            if (history.Count == 0)
            {
                Display.Info("No transactions yet.");
                Display.PressAnyKey();
                return;
            }

            Console.WriteLine($"  Showing {Math.Min(history.Count, 10)} most recent transaction(s):");
            Console.WriteLine();

            int shown = 0;
            foreach (var txn in history)
            {
                if (shown >= 10) break;
                Console.WriteLine("  " + txn.ToString());
                shown++;
            }

            Display.PressAnyKey();
        }
        //  Account details and settings
        static void HandleMyAccount()
        {
            bool inAccount = true;
            while (inAccount)
            {
                Display.Header("MY ACCOUNT");
                Console.WriteLine();
                Console.WriteLine($"  Name          : {currentAccount.FullName}");
                Console.WriteLine($"  Email         : {currentAccount.EmailAddress}");
                Console.WriteLine($"  Birthday      : {currentAccount.Birthday}");
                Console.WriteLine($"  Address       : {currentAccount.Address}");
                Console.WriteLine($"  Mobile        : {currentAccount.PhoneNumber}");
                Console.WriteLine($"  Balance       : P{currentAccount.Balance:N2}");
                Console.WriteLine();
                Console.WriteLine("  [1] Edit Account Details");
                Console.WriteLine("  [2] Change PIN");
                Console.WriteLine("  [0] Go Back");
                Console.WriteLine();

                string choice = Display.Prompt("Choose");

                switch (choice)
                {
                    case "1": HandleEditAccount(); break;
                    case "2": HandleChangePIN(); break;
                    case "0": inAccount = false; break;
                    default:
                        Display.Error("Invalid option.");
                        Display.PressAnyKey();
                        break;
                }
            }
        }

        static void HandleEditAccount()
        {
            Display.Header("EDIT ACCOUNT DETAILS");
            Display.Info("Press Enter to keep your current value.");
            Console.WriteLine();

            Console.WriteLine($"  Current First Name  : {currentAccount.FirstName}");
            string newFirstName = Display.Prompt("New First Name (or Enter to keep)");
            if (string.IsNullOrWhiteSpace(newFirstName))
                newFirstName = currentAccount.FirstName;

            Console.WriteLine($"  Current Middle Name : {currentAccount.MiddleName}");
            string newMiddleName = Display.Prompt("New Middle Name (or Enter to keep)");
            if (string.IsNullOrWhiteSpace(newMiddleName))
                newMiddleName = currentAccount.MiddleName;

            Console.WriteLine($"  Current Last Name   : {currentAccount.LastName}");
            string newLastName = Display.Prompt("New Last Name (or Enter to keep)");
            if (string.IsNullOrWhiteSpace(newLastName))
                newLastName = currentAccount.LastName;

            Console.WriteLine($"  Current Email       : {currentAccount.EmailAddress}");
            string newEmail = Display.Prompt("New Email (or Enter to keep)");
            if (string.IsNullOrWhiteSpace(newEmail))
                newEmail = currentAccount.EmailAddress;
            else
            {
                while (!newEmail.Contains("@gmail.com"))
                {
                    Display.Error("Invalid email. Must contain @gmail.com");
                    newEmail = Display.Prompt("New Email");
                }
            }

            Console.WriteLine($"  Current Birthday    : {currentAccount.Birthday}");
            string newBirthday = Display.Prompt("New Birthday MM/DD/YYYY (or Enter to keep)");
            if (string.IsNullOrWhiteSpace(newBirthday))
                newBirthday = currentAccount.Birthday;
            else
            {
                while (newBirthday.Length != 10 || !DateTime.TryParse(newBirthday, out _))
                {
                    Display.Error("Invalid format. Use MM/DD/YYYY.");
                    newBirthday = Display.Prompt("New Birthday MM/DD/YYYY");
                }
            }

            Console.WriteLine($"  Current Address     : {currentAccount.Address}");
            string newAddress = Display.Prompt("New Address (or Enter to keep)");
            if (string.IsNullOrWhiteSpace(newAddress))
                newAddress = currentAccount.Address;

            Console.WriteLine();
            Display.Warning("Save these changes?");
            string confirm = Display.Prompt("Type YES to confirm or NO to cancel");

            while (confirm?.ToUpper() != "YES" && confirm?.ToUpper() != "NO")
            {
                Display.Error("Invalid input. Type YES to confirm or NO to cancel.");
                confirm = Display.Prompt("Type YES to confirm or NO to cancel");
            }

            if (confirm?.ToUpper() == "NO")
            {
                Display.Info("Edit cancelled. Nothing was changed.");
                Display.PressAnyKey();
                return;
            }

            if (!VerifyPIN())
            {
                Display.PressAnyKey();
                return;
            }

            bool success = service.UpdateAccount(currentAccount.PhoneNumber,
                                                  newFirstName, newMiddleName, newLastName,
                                                  newEmail, newBirthday, newAddress);
            if (success)
                Display.Success("Account details updated successfully!");
            else
                Display.Error("Update failed. Please try again.");

            Display.PressAnyKey();
        }

        static void HandleChangePIN()
        {
            Display.Header("CHANGE PIN");

            if (!VerifyPIN())
            {
                Display.PressAnyKey();
                return;
            }

            string newPin = Display.ReadPin("Set new 4-digit PIN");
            while (newPin.Length != 4)
            {
                Display.Error("PIN must be exactly 4 digits.");
                newPin = Display.ReadPin("Set new 4-digit PIN");
            }

            string confirmPin = Display.ReadPin("Confirm new PIN");
            while (newPin != confirmPin)
            {
                Display.Error("PINs do not match. Try again.");
                confirmPin = Display.ReadPin("Confirm new PIN");
            }

            bool success = service.ChangePIN(currentAccount.PhoneNumber, newPin);
            if (success)
                Display.Success("PIN changed successfully!");
            else
                Display.Error("Something went wrong. Please try again.");

            Display.PressAnyKey();
        }
    }
}