# InventoryDispatchApi

API REST para gestão de produtos e pedidos com regra transacional de logística.

O foco do projeto não é CRUD simples, mas a implementação de uma regra de negócio real:

> Ao despachar um pedido, o sistema valida estoque e realiza baixa transacional dos produtos.

---

## 🧠 Contexto

Este projeto simula um sistema interno de logística, sem camadas de cliente/endereço (não é CRM).

O objetivo é demonstrar:

- Modelagem de domínio
- Validação de regras de negócio
- Transações com EF Core
- Tratamento adequado de erros (ProblemDetails)
- Separação entre Entidades e DTOs
- Estrutura organizada para evolução futura

---

## 🚀 Tecnologias

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQLite
- Migrations
- Swagger
- xUnit (testes)
- ProblemDetails (RFC 7807)

---

## 🏗 Arquitetura

Controllers/
Domain/
Dtos/
Data/
Migrations/


- **Domain** → Entidades do negócio
- **Dtos** → Objetos de entrada/saída (evita expor entidades)
- **Data** → DbContext e configurações
- **Controllers** → Endpoints REST

---

## 📦 Entidades

### Product
- Id (Guid)
- Sku (único)
- Name
- QuantityOnHand
- MinimumQuantity
- CreatedAt

### Order
- Id
- Status (Open | Dispatched | Cancelled)
- CreatedAt
- Items

### OrderItem
- Id
- OrderId
- ProductId
- Quantity

---

## 🔥 Regra Principal: Dispatch

Endpoint:

POST /orders/{id}/dispatch


### Regras aplicadas:

- Pedido deve estar com Status = Open
- Todos os itens devem ter estoque suficiente
- A operação ocorre dentro de uma transação de banco
- Se qualquer item não tiver estoque:
  - retorna 409 Conflict
  - lista os SKUs com quantidade faltante
- Se válido:
  - baixa QuantityOnHand
  - altera Status para Dispatched

---

## 📊 Dashboard

Endpoint:


GET /dashboard

Retorna:

- total de pedidos Open
- total de pedidos Dispatched
- produtos abaixo do estoque mínimo

---

## ▶ Como rodar

```bash
dotnet ef database update
dotnet run

Swagger disponível em: