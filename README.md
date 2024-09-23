# Integração com Google Calendar API

Este projeto demonstra uma integração com a API do Google Calendar, permitindo autenticação OAuth, criação e busca de eventos no calendário.

## Estrutura do Projeto

O projeto está dividido em duas partes principais: frontend e backend.

### Frontend (oauth-frontend)

O frontend é responsável pela interface do usuário e interação inicial com o Google OAuth.

Rotas principais:
- `/`: Página de login (componente `LoginComponent`)
- `/logout`: Página de logout e liberação de permissões (componente `LogoutComponent`)
- `/home`: Página para criar eventos no calendário (componente `HomeComponent`)

### Backend

O backend gerencia a comunicação com a API do Google Calendar e manipula tokens de autenticação.

Endpoints principais:
- `ExchangeCodeForToken`: Troca o código de autorização por tokens de acesso e atualização
- `RefreshAccessToken`: Atualiza o token de acesso expirado
- `CreateEvent`: Cria um novo evento no Google Calendar
- `GetEvent`: Busca informações de um evento específico

## Configuração

1. Clone o repositório
2. Configure as credenciais do Google OAuth no projeto
3. Instale as dependências do frontend e backend
4. Inicie o servidor backend
5. Inicie o aplicativo frontend

## Uso

1. Acesse a página inicial para fazer login com sua conta Google
2. Após o login bem-sucedido, você será redirecionado para a página inicial
3. Use a página inicial para criar novos eventos no seu Google Calendar
4. Para fazer logout e revogar as permissões, acesse a rota de logout

## Desenvolvimento

- Para adicionar novas funcionalidades relacionadas ao Google Calendar, expanda os componentes existentes ou crie novos no frontend
- Implemente novos endpoints no backend para interagir com diferentes aspectos da API do Google Calendar

## Notas

- Certifique-se de manter os tokens de acesso e atualização seguros
- Respeite as políticas de uso e limites da API do Google Calendar

Para mais informações sobre a API do Google Calendar, consulte a [documentação oficial](https://developers.google.com/calendar).
