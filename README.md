API REST desenvolvida em .NET 9 para gerenciamento de ordens de compra com fluxo de aprovação por cargos hierárquicos.

---

Sobre o projeto:

O sistema permite criar e gerenciar ordens de compra, submetê-las para aprovação e acompanhar todo o histórico de ações.
O fluxo de aprovação é definido automaticamente com base no valor total do pedido, passando pelos cargos necessários até a aprovação final.

Níveis de aprovação por valor:
- Até R$ 100 → Suprimentos
- R$ 101 a R$ 1.000 → Gerente → Suprimentos
- Acima de R$ 1.000 → Gerente → Diretor → Suprimentos


---

Tecnologias

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- SQL Server
- Swagger


---

Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local ou remoto)


---

Como rodar

1. Clone o repositório

```bash
git clone https://github.com/GuilhermeFC1/PurchaseOrderAPI.git
cd PurchaseOrderAPI
```

2. Configure a connection string

Edite o arquivo `src/API/appsettings.json` com os dados do seu SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SEU_SERVIDOR;Database=PurchaseOrderApiDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

3. Execute a aplicação

```bash
dotnet run --project src/API/PurchaseOrderAPI.csproj
```

As migrations são aplicadas automaticamente na primeira execução. O banco de dados `PurchaseOrderApiDb` será criado caso não exista.

4. Acesse o Swagger

```
http://localhost:5235/swagger
```


---

Testando com Insomnia

Importe o arquivo `Postman_Collection.json` pelo menu *File → Import* — o Insomnia aceita o formato do Postman nativamente.

Caso prefira criar as requisições manualmente, use os exemplos de body abaixo. Todas as requisições devem ter o header `Content-Type: application/json`.


### Usuários

**POST /api/users** — Criar usuário
```json
{
  "fullName": "João Silva",
  "email": "joao.silva@email.com",
  "userRole": "Elaborator",
  "department": "Compras"
}
```
Valores aceitos em `userRole`: `Elaborator`, `Manager`, `Director`, `Supplies`

---

**PUT /api/users/{id}** — Atualizar usuário
```json
{
  "fullName": "João Silva Atualizado",
  "department": "Financeiro"
}
```

---

**PUT /api/users/{id}/toggle-active** — Ativar/desativar usuário

Sem body.

---

### Ordens de Compra

**POST /api/purchaseorders** — Criar ordem
```json
{
  "elaboratorId": "00000000-0000-0000-0000-000000000000",
  "orderNumber": "OC-001",
  "description": "Compra de materiais de escritório"
}
```

---

**PUT /api/purchaseorders/{id}** — Editar ordem (apenas status Draft)
```json
{
  "description": "Descrição atualizada"
}
```

---

**POST /api/purchaseorders/{id}/items** — Adicionar item
```json
{
  "description": "Resma de papel A4",
  "quantity": 10,
  "unitPrice": 25.90
}
```

---

**POST /api/purchaseorders/{id}/submit** — Submeter para aprovação
```json
{
  "elaboratorId": "00000000-0000-0000-0000-000000000000"
}
```

---

**POST /api/purchaseorders/{id}/approve** — Aprovar
```json
{
  "approverId": "00000000-0000-0000-0000-000000000000",
  "comments": "Aprovado conforme orçamento."
}
```

---

**POST /api/purchaseorders/{id}/reject** — Rejeitar
```json
{
  "approverId": "00000000-0000-0000-0000-000000000000",
  "reason": "Valor acima do orçamento disponível."
}
```

---

**POST /api/purchaseorders/{id}/request-revision** — Solicitar revisão
```json
{
  "approverId": "00000000-0000-0000-0000-000000000000",
  "reason": "Necessário ajustar a descrição dos itens."
}
```

---

**DELETE /api/purchaseorders/{id}** — Cancelar ordem
```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "reason": "Compra não é mais necessária."
}
```

---

Endpoints principais

Usuários — `/api/users`

- POST - `/api/users` -> Criar usuário
- GET - `/api/users` -> Listar todos
- GET - `/api/users/role/{role}` -> Filtrar por função
- PUT - `/api/users/{id}` -> Atualizar usuário
- PUT - `/api/users/{id}/toggle-active` -> Ativar ou desativar

Ordens de Compra — `/api/purchaseorders`

- POST - `/api/purchaseorders` -> Criar ordem
- GET - `/api/purchaseorders` -> Listar todas
- GET - `/api/purchaseorders/status/{status}` -> Filtrar por status
- POST - `/api/purchaseorders/{id}/items` -> Adicionar item
- POST - `/api/purchaseorders/{id}/submit` -> Submeter para aprovação
- POST - `/api/purchaseorders/{id}/approve` -> Aprovar
- POST - `/api/purchaseorders/{id}/reject` -> Rejeitar
- POST - `/api/purchaseorders/{id}/request-revision` -> Solicitar revisão
- GET - `/api/purchaseorders/{id}/approvals` -> Ver cadeia de aprovação
- GET - `/api/purchaseorders/{id}/history` -> Ver histórico de auditoria
