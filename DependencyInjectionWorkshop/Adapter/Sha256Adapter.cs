using System.Text;

namespace DependencyInjectionWorkshop.Adapter
{
    public interface IHash
    {
        string GetHash(string plainText);
    }

    public class Sha256Adapter : IHash
    {
        public string GetHash(string plainText)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var inputPasswordHash = hash.ToString();
            return inputPasswordHash;
        }
    }
}