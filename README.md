# DevManager

Sistema Full Stack para gerenciamento de desenvolvedores, cidades, estados e linguagens de programação.

Projeto desenvolvido como solução para o teste técnico de Desenvolvedor Full Stack da Cartsys Software.

---

# Objetivo

O DevManager foi desenvolvido utilizando uma arquitetura moderna baseada em Clean Architecture, priorizando:

* Separação de responsabilidades
* Baixo acoplamento
* Alta coesão
* Facilidade de manutenção
* Escalabilidade
* Boas práticas de desenvolvimento

A aplicação permite o gerenciamento completo de desenvolvedores e suas tecnologias, incluindo autenticação JWT, geração de relatórios em PDF, Soft Delete, validações e integração completa entre Frontend e Backend.

---

# Tecnologias Utilizadas

## Backend

* .NET 8
* C# 12
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* AutoMapper
* FluentValidation
* JWT Authentication
* Swagger/OpenAPI
* QuestPDF
* Docker

## Frontend

* React 18
* Next.js 14
* TypeScript
* TailwindCSS
* shadcn/ui
* TanStack Query
* React Hook Form
* Zod
* Axios
* Sonner (Toast)

---

# Arquitetura

O backend foi desenvolvido utilizando Clean Architecture.

```
DevManager.Api
│
├── Controllers
├── Middleware
├── Extensions
└── Program.cs

DevManager.Application
│
├── DTOs
├── Services
├── Validators
├── Interfaces
├── Mappings
└── Common

DevManager.Domain
│
├── Entities
├── Interfaces
└── Enums

DevManager.Infrastructure
│
├── Persistence
├── Repositories
├── Identity
└── Reports
```

Cada camada possui responsabilidades bem definidas.

## Domain

Contém apenas regras de negócio.

Não possui dependência das demais camadas.

---

## Application

Responsável pelos casos de uso.

Contém:

* Services
* DTOs
* Interfaces
* Validators
* Result Pattern

---

## Infrastructure

Implementação de:

* Entity Framework
* Repositórios
* JWT
* Password Hash
* Banco de dados
* Relatórios PDF

---

## API

Responsável pela exposição dos endpoints REST.

Possui:

* Controllers
* Middleware
* Swagger
* Configuração JWT
* Dependency Injection

---

# Funcionalidades

## Login

* Autenticação JWT
* Senha criptografada
* Controle de acesso

---

## Usuários

* Cadastro
* Login
* E-mail único
* Senha com Hash

---

## Estados

CRUD completo

* Cadastro
* Edição
* Exclusão lógica
* Pesquisa

---

## Cidades

CRUD completo

Relacionadas a Estados.

---

## Linguagens

CRUD completo

Tipos disponíveis:

* Backend
* Frontend
* Mobile
* Database
* Cloud
* DevOps
* Game

---

## Desenvolvedores

Cadastro contendo:

* Nome
* Email
* Senioridade
* Cidade
* Linguagens
* Observações

Relacionamento N:N com Linguagens.

---

## Relatórios

Relatório PDF contendo:

* Nome
* Cidade
* Estado
* Senioridade
* Linguagens

---

# Funcionalidades Técnicas

* JWT Authentication
* Soft Delete
* Result Pattern
* FluentValidation
* Middleware Global de Exceções
* AutoMapper
* Dependency Injection
* Generic Repository
* Unit of Work
* Docker
* Swagger
* Seed Inicial

---

# Banco de Dados

PostgreSQL

Relacionamentos:

```
Estado
   │
   ├── Cidade
            │
            ├── Desenvolvedor
                        │
                        └── Linguagens (N:N)
```

---

# Segurança

* JWT Authentication
* Password Hash
* Middleware Global
* Validações
* E-mail único
* Soft Delete

---

# Docker

O projeto pode ser executado utilizando Docker Compose.

Containers:

* PostgreSQL
* Backend (.NET)
* Frontend (Next.js)

---

# Como executar

## Pré-requisitos

* Docker Desktop

ou

* .NET 8 SDK
* Node.js 20+
* PostgreSQL

---

## Utilizando Docker

Clone o projeto

```bash
git clone https://github.com/ViniFerreira07/devmanager.git
```

Entre na pasta

```bash
cd devmanager
```

Execute

```bash
docker compose up --build
```

---

Frontend

```
http://localhost:3000
```

Backend

```
http://localhost:5000
```

Swagger

```
http://localhost:5000/swagger
```

---

# Execução sem Docker

## Backend

```bash
cd DevManager.Api

dotnet restore

dotnet run
```

---

## Frontend

```bash
cd frontend

npm install

npm run dev
```

---

# Estrutura Frontend

```
frontend

app

components

services

hooks

schemas

types

contexts

lib

styles
```

---

# Validações

Todas as entradas são validadas utilizando FluentValidation no Backend e Zod no Frontend.

Entre elas:

* Email obrigatório
* Email único
* Linguagem obrigatória
* Estado obrigatório
* Cidade obrigatória
* Desenvolvedor deve possuir pelo menos uma linguagem

---

# Decisões Técnicas

## Clean Architecture

Escolhida para manter baixo acoplamento entre regras de negócio e infraestrutura.

---

## Repository Pattern

Centraliza acesso ao banco e facilita manutenção.

---

## Result Pattern

Evita utilização excessiva de exceções como fluxo de controle e padroniza respostas da API.

---

## FluentValidation

Separação das regras de validação da lógica de negócio.

---

## AutoMapper

Redução de código repetitivo entre Entidades e DTOs.

---

## JWT

Autenticação stateless, adequada para APIs REST.

---

## Soft Delete

Os registros não são removidos fisicamente do banco, preservando histórico e integridade referencial.

---

# Diferenciais Implementados

* Clean Architecture
* JWT
* Docker
* Swagger
* Seed inicial
* Middleware Global
* Soft Delete
* Result Pattern
* Generic Repository
* Unit of Work
* AutoMapper
* FluentValidation
* React Hook Form
* Zod
* TanStack Query
* QuestPDF
* Dark Mode
* Componentização com shadcn/ui

---
