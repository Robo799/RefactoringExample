using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidUserData(firstName, lastName, email, dateOfBirth))
                return false;

            var client = new ClientRepository().GetById(clientId);
            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidUserData(string firstName, string lastName, string email, DateTime dob)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return false;

            if (!email.Contains("@") && !email.Contains("."))
                return false;

            var now = DateTime.Now;
            int age = now.Year - dob.Year;
            if (now.Month < dob.Month || (now.Month == dob.Month && now.Day < dob.Day)) age--;

            return age >= 21;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dob, Client client)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                DateOfBirth = dob,
                Client = client
            };

            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                user.HasCreditLimit = true;
                using (var creditService = new UserCreditService())
                {
                    int creditLimit = creditService.GetCreditLimit(lastName, dob);
                    if (client.Type == "ImportantClient")
                    {
                        creditLimit *= 2;
                    }
                    user.CreditLimit = creditLimit;
                }
            }

            return user;
        }
    }
}
