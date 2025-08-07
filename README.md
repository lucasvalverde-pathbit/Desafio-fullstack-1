# Gerenciador de Produtos e Pedidos

## 📄 Descrição
Este projeto é uma aplicação web fullstack voltada para o gerenciamento de produtos e pedidos, oferecendo áreas distintas para administradores e clientes. A aplicação permite que usuários realizem operações como cadastro, pedidos e acompanhamento, através de uma interface moderna, responsiva e intuitiva.

> O frontend é desenvolvido com **Razor Pages (ASP.NET Core)**, com HTML, CSS e JavaScript modularizado. O backend também é construído em **ASP.NET Core** com autenticação JWT e integração com banco de dados via **Entity Framework Core**.

---

## ✨ Funcionalidades

### 👨‍💼 Área do Administrador
- Cadastro de novos produtos (nome, preço, estoque).
- Acompanhamento e atualização do status de pedidos dos clientes.

### 👤 Área do Cliente
- Cadastro e login com autenticação JWT.
- Criação de novos pedidos com endereço via CEP e seleção de produtos.
- Visualização de histórico de pedidos.

### 💻 Interface
- Layout responsivo com design moderno.
- Modais de edição e exclusão de conta.
- Componentização com JavaScript em módulos.

---

## ⚙️ Instalação

1. Clone o repositório:
```bash
git clone https://github.com/lucasvalverde-pathbit/Desafio-fullstack-1.git
cd Desafio-fullstack-1
```

2. Construa e inicie os containers com Docker:
```bash
docker-compose up -d --build
```

---

## 🚀 Como Usar
Após iniciar a aplicação, acesse no navegador:
```
http://localhost:9090
```

---

## 📡 API - Principais Endpoints

### 🔐 Usuários
- Cadastro e login com retorno de token JWT.
- Atualização de perfil e informações pessoais.

### 📦 Produtos
- Cadastro de produtos (admin).
- Listagem para pedidos (cliente).

### 🛒 Pedidos
- Criação de pedidos com busca de endereço via CEP.
- Visualização de pedidos do cliente autenticado.
- Atualização de status (admin).

---

## 🛠 Tecnologias Utilizadas

- **Frontend**: Razor Pages (ASP.NET Core), HTML5, CSS3, JavaScript (ES Modules), Bootstrap.
- **Backend**: ASP.NET Core, Entity Framework Core, autenticação com JWT.
- **DevOps**: Docker, Docker Compose.
- **Outros**: Webpack, bibliotecas auxiliares para construção e validação.
