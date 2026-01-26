# BankMore APIs

Projeto desenvolvido como desafio técnico.

# BankMore – API de Conta Corrente e Transferência

Este projeto implementa uma solução de **microserviços em .NET 8** para gerenciamento de **contas correntes** e **transferências**, com foco em:

- Arquitetura limpa (API / Application / Domain / Infrastructure)
- Idempotência
- Testes de integração reais (in-memory)
- Comunicação entre serviços via HTTP
- Persistência com SQLite

---

## Arquitetura

A solução é composta por **dois serviços independentes**:

## Conta Corrente
Responsável por:
- Criar contas
- Autenticar usuários
- Movimentar saldo (crédito / débito)
- Consultar saldo

## Transferência
Responsável por:
- Transferir valores entre contas
- Garantir idempotência por `requestId`
- Orquestrar chamadas para a API de Conta Corrente

Cada serviço possui sua própria base de dados SQLite.

---

## Estrutura da Solução

BankMore
│
├── BankMore.ContaCorrente.API
├── BankMore.ContaCorrente.Application
├── BankMore.ContaCorrente.Domain
├── BankMore.ContaCorrente.Infrastructure
├── BankMore.ContaCorrente.Tests
│
├── BankMore.Transferencia.API
├── BankMore.Transferencia.Application
├── BankMore.Transferencia.Domain
├── BankMore.Transferencia.Infrastructure
├── BankMore.Transferencia.Tests
│
├── Database
│ ├── scripts.sql
│ └── scripts-transferencia.sql
│
└── BankMore.sln


## Autenticação

Autenticação via JWT Bearer Token

Token obtido no endpoint de login

Token é exigido para movimentações, saldo e transferências

## Restaurar dependências
dotnet restore

## Build
dotnet build

## Executar testes
dotnet test


## Endpoints Principais
POST /api/contas

{
  "nomeTitular": "João Silva",
  "cpf": "12345678909",
  "senha": "Senha@123"
}

## Login
POST /api/contas/login

{
  "cpf": "12345678909",
  "senha": "Senha@123"
}


## Movimentar conta
POST /api/contas/movimentos
Authorization: Bearer {token}

{
  "requestId": "guid",
  "tipo": "C",
  "valor": 100
}


## Consultar saldo
GET /api/contas/saldo
Authorization: Bearer {token}


## Transferencia
POST /api/transferencias
Authorization: Bearer {token}

{
  "requestId": "guid",
  "numeroContaDestino": "123456",
  "valor": 50
}


