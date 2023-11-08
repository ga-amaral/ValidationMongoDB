namespace AdvContratoDomain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; }
        public string Key { get; set; }
        public bool Activated { get; set; }
        public DateTime KeyExpirationDate { get; set; } // Campo de expiração da chave
    }
}