
namespace GcashCLI
{
    public class Account
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string Birthday { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }
        public string PIN { get; set; }
        public decimal Balance { get; set; }

        public string FullName => $"{FirstName} {MiddleName} {LastName}";

        public Account(string firstName, string middleName, string lastName,
                      string email, string birthday, string address,
                      string phoneNumber, string pin, decimal initialBalance = 0)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            EmailAddress = email;
            Birthday = birthday;
            Address = address;
            PhoneNumber = phoneNumber;
            PIN = pin;
            Balance = initialBalance;
        }
    }
}