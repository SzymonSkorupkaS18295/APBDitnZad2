using System;
using System.Text.RegularExpressions;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository = new();
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {

            if (!IsValidName(firstName, lastName))
                return false;
            if (!IsValidEmail(email))
                return false;
            if (!IsAdult(dateOfBirth))
                return false;
            
            Client client = CheckIfExists(clientId);
            if (client == null)
            {
                _clientRepository.AddClient(clientId, lastName, email);
                client = _clientRepository.GetById(clientId);
            }

            Console.WriteLine(client.ToString());
            var user = UpdateExistingClient(client, firstName, lastName, email, dateOfBirth);
            Console.WriteLine(user);
            if (user == null) return false;
            UserDataAccess.AddUser(user);
            return true;
        }
        private static bool IsValidName(string firstName, string lastName)
        {
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }
        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$");
        }
        private static bool IsAdult(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
            return age >= 21;
        }
        private static Client CheckIfExists(int clientId)
        {
            try
            {
                var clientRepository = new ClientRepository();
                var client = clientRepository.GetById(clientId);
                return client;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private static User UpdateExistingClient(Client client, string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            using (var userCreditService = new UserCreditService())
            {
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (client.Type == "VeryImportantClient")
                {
                    user.HasCreditLimit = false;
                }
                else if (client.Type == "ImportantClient")
                {
                    creditLimit *= 2;
                }
                user.CreditLimit = creditLimit;
            }

            return user.CreditLimit < 500 ? null : user;
        }

    }
}
