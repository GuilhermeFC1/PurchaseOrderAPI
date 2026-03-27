# PurchaseOrderAPI

API REST desenvolvida em .NET 9 para gerenciamento de ordens de compra com fluxo de aprovação por níveis hierárquicos.

## Sobre o projeto

O sistema permite criar e gerenciar ordens de compra, submetê-las para aprovação e acompanhar todo o histórico de ações. O fluxo de aprovação é definido automaticamente com base no valor total do pedido, passando pelos níveis necessários até a aprovação final.

**Roles disponíveis:**
- **Elaborador** — cria e submete ordens de compra
- **Gerente** — aprova ordens de menor valor
- **Diretor** — aprova ordens de maior valor
- **Suprimentos** — realiza a aprovação final após os demais níveis

**Níveis de aprovação por valor:**
| Tier | Valor | Aprovadores |
|------|-------|-------------|
| Tier 1 | Até R$ 100 | Suprimentos |
| Tier 2 | R$ 101 a R$ 1.000 | Gerente → Suprimentos |
| Tier 3 | Acima de R$ 1.000 | Gerente → Diretor → Suprimentos |

## Tecnologias

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- SQL Server
- Swagger / OpenAPI
- xUnit

## Arquitetura

O projeto segue os princípios de Clean Architecture, organizado em camadas:

```
src/
├── API/            → Controllers, configuração e entry point
├── Application/    → Serviços e regras de negócio
├── Domain/         → Entidades e enums
└── Infrastructure/ → Repositórios, DbContext e migrations
```

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local ou remoto)

## Como rodar

**1. Clone o repositório**
```bash
git clone https://github.com/GuilhermeFC1/PurchaseOrderAPI.git
cd PurchaseOrderAPI
```

**2. Configure a connection string**

Edite o arquivo `src/API/appsettings.json` com os dados do seu SQL Server:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SEU_SERVIDOR;Database=PurchaseOrderApiDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

**3. Execute a aplicação**
```bash
dotnet run --project src/API/PurchaseOrderAPI.csproj
```

As migrations são aplicadas automaticamente na primeira execução. O banco de dados `PurchaseOrderApiDb` será criado caso não exista.

**4. Acesse o Swagger**
```
http://localhost:5235/swagger
```

## Endpoints principais

### Usuários — `/api/users`
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/users` | Criar usuário |
| GET | `/api/users` | Listar todos |
| GET | `/api/users/role/{role}` | Filtrar por role |
| PUT | `/api/users/{id}` | Atualizar usuário |
| PUT | `/api/users/{id}/toggle-active` | Ativar/desativar |

### Ordens de Compra — `/api/purchaseorders`
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/purchaseorders` | Criar ordem |
| GET | `/api/purchaseorders` | Listar todas |
| GET | `/api/purchaseorders/status/{status}` | Filtrar por status |
| POST | `/api/purchaseorders/{id}/items` | Adicionar item |
| POST | `/api/purchaseorders/{id}/submit` | Submeter para aprovação |
| POST | `/api/purchaseorders/{id}/approve` | Aprovar |
| POST | `/api/purchaseorders/{id}/reject` | Rejeitar |
| POST | `/api/purchaseorders/{id}/request-revision` | Solicitar revisão |
| GET | `/api/purchaseorders/{id}/approvals` | Ver cadeia de aprovação |
| GET | `/api/purchaseorders/{id}/history` | Ver histórico de auditoria |

## Testando com Postman

O repositório inclui o arquivo `Postman_Collection.json` com todas as requisições configuradas e prontas para uso.

## Testes

```bash
dotnet test
```
