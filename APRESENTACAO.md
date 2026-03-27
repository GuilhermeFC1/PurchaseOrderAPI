# Roteiro de Apresentação e Demo


## Roteiro de Apresentação


### 1. Introdução

"O projeto é uma API REST para gerenciamento de ordens de compra com fluxo de aprovação hierárquico. A ideia é que uma ordem passe por aprovadores diferentes dependendo do seu valor — quanto maior o valor, mais níveis de aprovação são necessários antes de ser finalizada."


---


### 2. Arquitetura

"Optei por Clean Architecture, separando o projeto em quatro camadas: Domain, Application, Infrastructure e API.

O **Domain** contém as entidades e enums — é o núcleo do sistema, sem dependência de nenhum framework externo.

O **Application** tem os serviços com as regras de negócio — PurchaseOrderService, ApprovalService e AuditService. Ele depende só do Domain.

O **Infrastructure** cuida do acesso a dados: o DbContext, as migrations e os repositórios. Ele depende do Domain e implementa o que o Application precisa.

A **API** é a camada de entrada — os controllers recebem as requisições, chamam os serviços do Application e devolvem as respostas.

Essa separação garante que a lógica de negócio não fica acoplada ao banco ou ao framework HTTP. Se eu precisasse trocar o SQL Server por outro banco, só a camada de Infrastructure seria afetada."


---


### 3. Modelagem das Entidades

"O **User** é uma classe abstrata. Elaborator, Manager, Director e SuppliesTeam herdam dela. No banco, usei a estratégia Table Per Hierarchy (TPH) — todas as subclasses ficam em uma única tabela Users, com uma coluna UserType que diferencia o tipo. Escolhi TPH porque as classes são simples e têm poucos campos diferentes entre si, então não justificava criar tabelas separadas.

O **PurchaseOrder** tem os itens, as aprovações e os logs de auditoria como relacionamentos. O valor total é calculado automaticamente com base nos itens adicionados, e a alçada de aprovação é determinada por esse valor.

O **PurchaseOrderApproval** representa cada etapa do fluxo. Ele tem um campo Sequence que define a ordem das aprovações e um campo ApproverRole que determina qual cargo deve aprovar naquele passo — o sistema valida isso na hora da aprovação.

O **AuditLog** registra toda ação feita no sistema: criação, adição de item, submissão, aprovação, rejeição. Isso garante rastreabilidade completa do pedido."


---


### 4. Fluxo de Aprovação

"Quando o elaborador submete o pedido, o sistema calcula o valor total, determina a alçada e cria a cadeia de aprovações automaticamente na sequência correta.

- Tier 1, até R$ 100: só Suprimentos aprova.
- Tier 2, até R$ 1.000: Suprimentos aprova primeiro, depois o Gerente.
- Tier 3, acima de R$ 1.000: Suprimentos, depois Gerente, depois Diretor.

Na hora de aprovar, o sistema busca a próxima aprovação pendente pela sequência e valida se o cargo do aprovador bate com o cargo esperado. Se não bater, rejeita a operação."


---


### 5. Decisões Técnicas

"Usei o Entity Framework Core com migrations para versionar o banco. As migrations são aplicadas automaticamente no startup, o que facilita rodar o projeto em qualquer ambiente sem passos manuais.

Os repositórios isolam o acesso ao banco dos serviços — o Application nunca chama o DbContext diretamente.

Toda a injeção de dependência está configurada no Program.cs, sem nenhum container externo."


---
---


## Script do Demo ao Vivo

Siga essa ordem. Use valor acima de R$ 1.000 para demonstrar o fluxo completo com os três aprovadores (Tier 3).


### Passo 1 — Criar os usuários

POST /api/users — Elaborador
```json
{
  "fullName": "Carlos Elaborador",
  "email": "carlos@empresa.com",
  "userRole": "Elaborator",
  "department": "Compras"
}
```

POST /api/users — Suprimentos
```json
{
  "fullName": "Ana Suprimentos",
  "email": "ana@empresa.com",
  "userRole": "Supplies",
  "department": "Suprimentos"
}
```

POST /api/users — Gerente
```json
{
  "fullName": "Roberto Gerente",
  "email": "roberto@empresa.com",
  "userRole": "Manager",
  "department": "Gestão"
}
```

POST /api/users — Diretor
```json
{
  "fullName": "Fernanda Diretora",
  "email": "fernanda@empresa.com",
  "userRole": "Director",
  "department": "Diretoria"
}
```

> Copie os IDs retornados em cada resposta — você vai precisar deles nos próximos passos.


---


### Passo 2 — Criar a ordem de compra

POST /api/purchaseorders
```json
{
  "elaboratorId": "ID_DO_ELABORADOR",
  "orderNumber": "OC-001",
  "description": "Compra de equipamentos de TI"
}
```

> Copie o ID da ordem retornado.


---


### Passo 3 — Adicionar item

POST /api/purchaseorders/{id}/items
```json
{
  "description": "Notebook Dell",
  "quantity": 2,
  "unitPrice": 600.00
}
```

> Valor total ficará R$ 1.200 — isso aciona o Tier 3 automaticamente.


---


### Passo 4 — Submeter para aprovação

POST /api/purchaseorders/{id}/submit
```json
{
  "elaboratorId": "ID_DO_ELABORADOR"
}
```

> O sistema cria os três registros de aprovação em sequência: Suprimentos (1), Gerente (2), Diretor (3).


---


### Passo 5 — Ver a cadeia de aprovação criada

GET /api/purchaseorders/{id}/approvals

> Mostre os três registros com status Pending e os cargos na sequência correta.


---


### Passo 6 — Suprimentos aprova (sequência 1)

POST /api/purchaseorders/{id}/approve
```json
{
  "approverId": "ID_DO_SUPRIMENTOS",
  "comments": "Verificado e dentro do orçamento."
}
```


---


### Passo 7 — Gerente aprova (sequência 2)

POST /api/purchaseorders/{id}/approve
```json
{
  "approverId": "ID_DO_GERENTE",
  "comments": "Aprovado pela gerência."
}
```


---


### Passo 8 — Diretor aprova (sequência 3)

POST /api/purchaseorders/{id}/approve
```json
{
  "approverId": "ID_DO_DIRETOR",
  "comments": "Aprovado pela diretoria."
}
```

> Após essa aprovação o status da ordem muda automaticamente para Approved.


---


### Passo 9 — Mostrar o histórico de auditoria

GET /api/purchaseorders/{id}/history

> Mostre o log completo: criação, item adicionado, submissão e as três aprovações com timestamps.


---


### Dica

Se perguntarem o que acontece quando o cargo errado tenta aprovar, tente aprovar com o ID do Elaborador no Passo 6 — o sistema vai retornar erro explicando que o cargo não corresponde. Isso demonstra que a validação está funcionando.
