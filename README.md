# Gerenciador de Produtos e Pedidos

## Descrição
Este projeto é uma aplicação que fornece uma interface para clientes gerenciarem seus pedidos. A aplicação permite que os usuários criem pedidos, visualizem seus pedidos existentes e interajam com a interface de forma intuitiva.

## Funcionalidades
- Área do Administrador: O administrador pode cadastrar produtos e atualizar o status dos pedidos dos clientes.
- Formulário de Criar Produto: O administrador insere informações como nome, preço e quantidade em estoque para os produtos.
- Área do Cliente: Os clientes podem criar novos pedidos e visualizar seus pedidos existentes.
- Formulário de Criar Pedido: Os clientes podem inserir informações como CEP, endereço, número da casa, selecionar produtos e especificar quantidades.
- Interface Responsiva: A aplicação é projetada para ser responsiva e funcionar em diferentes dispositivos.

## Instalação
Para instalar e executar o projeto, siga os passos abaixo:

1. Clone o repositório:
   ```bash
   git clone https://github.com/caroline-nunes-pathbit/Projeto01.git
   cd Projeto01
   ```


2. Construa e inicie os containers usando Docker:
   ```bash
   docker-compose up -d --build
   ```

## Uso
Após iniciar a aplicação, acesse `http://localhost:9090` em seu navegador.

## Funcionalidades do Backend
- **Endpoint de Clientes**: 
  - Criação de clientes automaticamente ao cadastrar um usuário do tipo 'Cliente'.
  - Atualização do perfil do usuário.

- **Endpoint de Pedidos**:
  - Obtenção de endereço formatado a partir do CEP fornecido.
  - Obtenção de todos os pedidos de um cliente autenticado.
  - Criação de novos pedidos com validação de dados.
  - Obtenção de todos os pedidos de um administrador.
  - Atualização do status de um pedido por um administrador.

- **Endpoint de Produtos**:
  - Criação de novos produtos por administradores.

- **Endpoint de Usuários**:
  - Login de usuários com retorno de token JWT.
  - Cadastro de novos usuários.
  - Atualização das informações de usuários existentes.

## Tecnologias
- **Frontend**: Utiliza JavaScript, HTML e CSS.
- **Backend**: Utiliza ASP.NET Core, Entity Framework Core, e autenticação em JWT.
- **Dependências**: Serve, Webpack, e outras bibliotecas para construção e estilização.



