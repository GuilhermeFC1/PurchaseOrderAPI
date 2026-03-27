using System.Text.Json.Serialization;

namespace PurchaseOrderAPI.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    // Define os diferentes papéis que um usuário pode ter
    public enum UserRole
    {
        Elaborator = 0,     // Cria pedidos
        Supplies = 1,       // Aprova em Suprimentos
        Manager = 2,        // Aprova como Gerente
        Director = 3        // Aprova como Diretor
    }
}