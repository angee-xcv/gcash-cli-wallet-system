
using System;
using System.Collections.Generic;


namespace GcashCLI
{
    public class TransactionService
    {
        private Dictionary<string, Account> _accounts;
        private Dictionary<string, List<Transaction>> _transactionHistory;
        private int limit = 10000; 
        public TransactionService()
        {
            _accounts = new Dictionary<string, Account>();
            _transactionHistory = new Dictionary<string, List<Transaction>>();

            // For demo
            SeedDemoAccounts();
        }

        //  Aaccount management

        public bool RegisterAccount(string firstName, string middleName, string lastName, string emailAddress, string birthday, string address,  string phoneNumber, string pin)
        {
            
            if (_accounts.ContainsKey(phoneNumber))
                return false; 

            var account = new Account(firstName, middleName, lastName, emailAddress, birthday, address, phoneNumber,  pin, 0);
            _accounts[phoneNumber] = account;
            _transactionHistory[phoneNumber] = new List<Transaction>();
            return true;
        }

        public Account Login(string phoneNumber, string pin)
        {   
            
            if (_accounts.TryGetValue(phoneNumber, out Account account))
            {
                if (account.PIN == pin)
                    return account;
            }
            return null;

        }

        //Resetting of Pin
        public bool ResetPIN(string phoneNumber, string birthday, string newPin)
        {
            if (_accounts.TryGetValue(phoneNumber, out Account account))
            {
                if (account.Birthday == birthday)
                {
                    account.PIN = newPin;
                    return true;
                }
            }
            return false;
        }


        public bool AccountExists(string phoneNumber)
        {
            return _accounts.ContainsKey(phoneNumber);
        }


        //  TRANSACTIONS

        public Account GetAccount(string phoneNumber)
        {
            if (_accounts.TryGetValue(phoneNumber, out Account account))
                return account;
            return null;
        }

        /// Cash In 
        public Transaction CashIn(Account account, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");
            if (amount > limit)
                throw new ArgumentException($"CashIn amount cannot exceed P{limit:N2}.");

            account.Balance += amount;
            var txn = new Transaction(TransactionType.CashIn, amount,
                $"Cash In to {account.PhoneNumber}", account.Balance);

            AddHistory(account.PhoneNumber, txn);
            return txn;
        }

        public Transaction CashOut(Account sender, string recipientNumber, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            if (amount > limit)
                throw new ArgumentException($"Cash Out amount cannot exceed P{limit:N2}.");

            if (sender.Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            if (!_accounts.ContainsKey(recipientNumber))
                throw new ArgumentException("Recipient account not found.");

            Account recipient = _accounts[recipientNumber];
           
            sender.Balance -= amount;

            recipient.Balance += amount;

            var senderTxn = new Transaction(
                TransactionType.CashOut,
                amount,
                $"Sent to {recipient.PhoneNumber}",
                sender.Balance
            );

            var recipientTxn = new Transaction(
                TransactionType.CashIn,
                amount,
                $"Received from {sender.PhoneNumber}",
                recipient.Balance
            );

            AddHistory(sender.PhoneNumber, senderTxn);
            AddHistory(recipient.PhoneNumber, recipientTxn);

            return senderTxn;
        }



        //Deposit 
        public Transaction Deposit(Account account, decimal amount, string bankName)
        {
            int depositLimit = 500000;

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            if (amount > depositLimit)
                throw new ArgumentException($"Deposit amount cannot exceed P{depositLimit:N2}.");

            account.Balance += amount;

            var txn = new Transaction(
                TransactionType.Deposit,
                amount,
                $"Deposit from {bankName}",
                account.Balance
            );

            AddHistory(account.PhoneNumber, txn);
            return txn;
        }

        //Transfer to bank
        public Transaction Transfer(Account account, string bankName, string bankAccountNumber, decimal amount)
        {
            int transferLimit = 500000;
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            if (amount > transferLimit)
                throw new ArgumentException($"Transfer amount cannot exceed P{transferLimit:N2}.");

            if (account.Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            account.Balance -= amount;
            var txn = new Transaction(TransactionType.Transfer, amount,
                $"Transfer to {bankName} Acct#{bankAccountNumber}", account.Balance);

            AddHistory(account.PhoneNumber, txn);
            return txn;
        }

        //Returns the full transaction history
        public List<Transaction> GetHistory(string phoneNumber)
        {
            if (_transactionHistory.TryGetValue(phoneNumber, out var history))
                return history;
            return new List<Transaction>();
        }

        private void AddHistory(string phoneNumber, Transaction txn)
        {
            if (!_transactionHistory.ContainsKey(phoneNumber))
                _transactionHistory[phoneNumber] = new List<Transaction>();

            _transactionHistory[phoneNumber].Insert(0, txn);
        }

        public bool ChangePIN(string phoneNumber, string newPin)
        {
            if (_accounts.TryGetValue(phoneNumber, out Account account))
            {
                account.PIN = newPin;
                return true;
            }
            return false;
        }

       
        public bool UpdateAccount(string phoneNumber, string firstName, string middleName,
                           string lastName, string email, string birthday, string address)
        {
            if (_accounts.TryGetValue(phoneNumber, out Account account))
            {
                account.FirstName = firstName;
                account.MiddleName = middleName;
                account.LastName = lastName;
                account.EmailAddress = email;
                account.Birthday = birthday;
                account.Address = address;
                return true;
            }
            return false;
        }


        private void SeedDemoAccounts()
        {
            // demo accounts
            var demo1 = new Account("ERIKA", "FLORES", "RANJO" , "erikaranjo@gmail.com", "01/21/2000", "Oas", "09171234567", "1234", 500);
            var demo2 = new Account("ANGELINE", "BRACIA","SANDAGON", "angelinesandagon@gmail.com", "01/06/2005", "Polangui", "09189876543", "5678", 1500);

            _accounts[demo1.PhoneNumber] = demo1;
            _accounts[demo2.PhoneNumber] = demo2;
            _transactionHistory[demo1.PhoneNumber] = new List<Transaction>();
            _transactionHistory[demo2.PhoneNumber] = new List<Transaction>();
        }

        

        
    }
}
