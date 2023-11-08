using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AdvContratoDomain.Entities
{
    public class UserManager
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserManager(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _usersCollection = database.GetCollection<User>("users");
        }

        public async Task<bool> RegisterUserAsync(string email, string key)
        {
            // Checar se a chave já existe
            var existingUser = await _usersCollection.Find(u => u.Key == key).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                // Chave já existe
                return false;
            }

            // Registrar novo usuário
            var newUser = new User
            {
                Email = email,
                Key = key,
                Activated = false
            };

            await _usersCollection.InsertOneAsync(newUser);
            return true;
        }

        public async Task<(bool Success, string Message)> AuthenticateUserAsync(string email, string key)
        {
            // Tentar encontrar o usuário
            var user = await _usersCollection.Find(u => u.Email == email && u.Key == key).FirstOrDefaultAsync();
            if (user == null)
            {
                return (false, "Usuário ou chave inválida.");
            }

            if (user.Activated)
            {
                return (false, "Chave já foi ativada.");
            }

            // Ativar usuário se não estiver ativado
            if (!user.Activated)
            {
                user.Activated = true;
                var update = Builders<User>.Update.Set(u => u.Activated, true);
                await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
                return (true, "Usuário autenticado e chave ativada com sucesso.");
            }

            return (false, "Ocorreu um erro não esperado.");
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }
        // Você pode querer criar métodos adicionais para gerenciar usuários, como redefinir chaves, etc.
        public async Task<bool> ResetUserKeyAsync(Guid userId, string newKey)
        {
            var update = Builders<User>.Update.Set(u => u.Key, newKey).Set(u => u.Activated, false);
            var result = await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount == 1;
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            var existingUser = await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
            return existingUser != null;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var update = Builders<User>.Update.Set(u => u.Activated, false);
            var result = await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount == 1;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var result = await _usersCollection.DeleteOneAsync(u => u.Id == userId);
            return result.DeletedCount == 1;
        }
        public string GenerateEncryptedKey()
        {
            // Aqui usaremos um método simples para geração de uma chave criptografada,
            // mas você pode substituir por qualquer algoritmo de criptografia de sua escolha.
            var randomBytes = new byte[16]; // 128 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            // Converter para Base64 para obter uma string
            return Convert.ToBase64String(randomBytes);
        }

        // Método para registrar um usuário novo
        public async Task<bool> RegisterNewUserAsync(string email, int keyValidityDuration)
        {
            if (await IsEmailRegisteredAsync(email))
            {
                // O e-mail já está cadastrado
                return false;
            }

            // Gerar a chave encriptada
            string encryptedKey = GenerateEncryptedKey();

            // Definir a data de expiração da chave
            DateTime expirationDate = DateTime.UtcNow.AddDays(keyValidityDuration);

            var newUser = new User
            {
                Email = email,
                Key = encryptedKey, // Salve a chave encriptada
                Activated = false,
                KeyExpirationDate = expirationDate
            };

            await _usersCollection.InsertOneAsync(newUser);
            return true;
        }
    }

}
